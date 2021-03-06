﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Rpc.Diagnostics;
using Silky.Rpc.Messages;
using Silky.Rpc.Security;
using Silky.Rpc.Transport;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultServiceMessageReceivedHandler : IServiceMessageReceivedHandler
    {
        private readonly IServiceEntryLocator _serviceEntryLocator;

        protected static readonly DiagnosticListener s_diagnosticListener =
            new(RpcDiagnosticListenerNames.DiagnosticServerListenerName);

        public DefaultServiceMessageReceivedHandler(IServiceEntryLocator serviceEntryLocator)
        {
            _serviceEntryLocator = serviceEntryLocator;
        }

        public async Task Handle(string messageId, IMessageSender sender, RemoteInvokeMessage message)
        {
            RpcContext.GetContext()
                .SetAttachments(message.Attachments);
            RpcContext.GetContext()
                .SetAttachment(AttachmentKeys.IsGatewayHost, false);
            var tracingTimestamp = TracingBefore(message, messageId);
            RemoteResultMessage remoteResultMessage;
            try
            {
                var serviceEntry =
                    _serviceEntryLocator.GetLocalServiceEntryById(message.ServiceId);
                if (serviceEntry == null)
                {
                    throw new SilkyException($"通过服务id{message.ServiceId}获取本地服务条目失败", StatusCode.NotFindLocalServiceEntry);
                }

                var tokenValidator = EngineContext.Current.Resolve<ITokenValidator>();
                if (!tokenValidator.Validate())
                {
                    throw new RpcAuthenticationException("rpc token不合法");
                }

                var currentServiceKey = EngineContext.Current.Resolve<ICurrentServiceKey>();
                var result = await serviceEntry.Executor(currentServiceKey.ServiceKey,
                    message.Parameters);

                remoteResultMessage = new RemoteResultMessage()
                {
                    Result = result,
                    StatusCode = StatusCode.Success
                };
                TracingAfter(tracingTimestamp, messageId, message.ServiceId, remoteResultMessage);
            }
            catch (Exception e)
            {
                remoteResultMessage = new RemoteResultMessage()
                {
                    ExceptionMessage = e.GetExceptionMessage(),
                    StatusCode = e.GetExceptionStatusCode(),
                    ValidateErrors = e.GetValidateErrors().ToArray()
                };
                TracingError(tracingTimestamp, messageId, message.ServiceId, e.GetExceptionStatusCode(), e);
            }

            var resultTransportMessage = new TransportMessage(remoteResultMessage, messageId);
            await sender.SendAndFlushAsync(resultTransportMessage);
        }

        private long? TracingBefore(RemoteInvokeMessage message, string messageId)
        {
            if (s_diagnosticListener.IsEnabled(RpcDiagnosticListenerNames.BeginRpcServerHandler))
            {
                var remoteAddress = RpcContext.GetContext().GetAttachment(AttachmentKeys.RemoteAddress).ToString();
                var eventData = new RpcInvokeEventData()
                {
                    MessageId = messageId,
                    OperationTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    ServiceId = message.ServiceId,
                    Message = message,
                    RemoteAddress = remoteAddress,
                    IsGateWay = RpcContext.GetContext().IsGateway(),
                    ServiceMethodName = RpcContext.GetContext().GetAttachment(AttachmentKeys.ServiceMethodName).ToString()
                };

                s_diagnosticListener.Write(RpcDiagnosticListenerNames.BeginRpcServerHandler, eventData);

                return eventData.OperationTimestamp;
            }

            return null;
        }

        private void TracingAfter(long? tracingTimestamp, string messageId, string serviceId, RemoteResultMessage remoteResultMessage)
        {
            if (tracingTimestamp != null &&
                s_diagnosticListener.IsEnabled(RpcDiagnosticListenerNames.EndRpcServerHandler))
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var eventData = new RpcResultEventData()
                {
                    MessageId = messageId,
                    ServiceId = serviceId,
                    Result = remoteResultMessage.Result,
                    StatusCode = remoteResultMessage.StatusCode,
                    ElapsedTimeMs = now - tracingTimestamp.Value,
                    RemoteAddress = RpcContext.GetContext().GetAttachment(AttachmentKeys.RemoteAddress).ToString()
                };

                s_diagnosticListener.Write(RpcDiagnosticListenerNames.EndRpcServerHandler, eventData);
            }
        }

        private void TracingError(long? tracingTimestamp, string messageId, string serviceId, StatusCode statusCode,
            Exception ex)
        {
            if (tracingTimestamp != null &&
                s_diagnosticListener.IsEnabled(RpcDiagnosticListenerNames.ErrorRpcServerHandler))
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var eventData = new RpcExcetionEventData()
                {
                    MessageId = messageId,
                    ServiceId = serviceId,
                    StatusCode = statusCode,
                    ElapsedTimeMs = now - tracingTimestamp.Value,
                    RemoteAddress = RpcContext.GetContext().GetAttachment(AttachmentKeys.RemoteAddress).ToString(),
                    Exception = ex
                };
                s_diagnosticListener.Write(RpcDiagnosticListenerNames.ErrorRpcServerHandler, eventData);
            }
        }
    }
}