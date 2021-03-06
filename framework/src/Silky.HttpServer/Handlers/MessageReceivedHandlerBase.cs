using System.Threading.Tasks;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Silky.Rpc.Configuration;
using Silky.Rpc.Runtime;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Runtime.Server.Parameter;
using Silky.Rpc.Transport;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Silky.HttpServer.Configuration;
using Silky.Rpc;

namespace Silky.HttpServer.Handlers
{
    internal abstract class MessageReceivedHandlerBase : IMessageReceivedHandler
    {
        protected readonly IParameterParser _parameterParser;
        protected readonly ISerializer _serializer;
        protected readonly RpcOptions _rpcOptions;
        protected readonly IServiceExecutor _serviceExecutor;
        protected readonly GatewayOptions _gatewayOptions;
        protected readonly IMiniProfiler _miniProfiler;

        protected MessageReceivedHandlerBase(
            IParameterParser parameterParser,
            ISerializer serializer,
            IOptions<RpcOptions> rpcOptions,
            IOptions<GatewayOptions> gatewayOptions,
            IServiceExecutor serviceExecutor,
            IMiniProfiler miniProfiler)
        {
            _parameterParser = parameterParser;
            _serializer = serializer;
            _serviceExecutor = serviceExecutor;
            _miniProfiler = miniProfiler;
            _rpcOptions = rpcOptions.Value;
            _gatewayOptions = gatewayOptions.Value;
        }

        public virtual async Task Handle(HttpContext context, ServiceEntry serviceEntry)
        {
            Check.NotNull(context, nameof(context));
            Check.NotNull(serviceEntry, nameof(serviceEntry));
            var requestParameters = await _parameterParser.Parser(context.Request, serviceEntry);
            RpcContext.GetContext()
                .SetAttachment(AttachmentKeys.RequestHeader, requestParameters[ParameterFrom.Header]);
            RpcContext.GetContext()
                .SetAttachment(AttachmentKeys.IsGatewayHost, true);
            var rpcParameters = serviceEntry.ResolveParameters(requestParameters);
            string serviceKey = null;

            if (context.Request.Headers.ContainsKey("serviceKey"))
            {
                serviceKey = context.Request.Headers["serviceKey"].ToString();
                RpcContext.GetContext().SetAttachment(AttachmentKeys.ServiceKey, serviceKey);
                _miniProfiler.Print(MiniProfileConstant.Route.Name,
                    MiniProfileConstant.Route.State.FindServiceKey,
                    $"serviceKey:{serviceKey}");
            }
            else
            {
                _miniProfiler.Print(MiniProfileConstant.Route.Name,
                    MiniProfileConstant.Route.State.FindServiceKey,
                    "没有设置serviceKey");
            }

            RpcContext.GetContext().SetAttachment(AttachmentKeys.RpcToken, _rpcOptions.Token);
            var excuteResult = await _serviceExecutor.Execute(serviceEntry, rpcParameters, serviceKey);
            context.Response.ContentType = "application/json;charset=utf-8";
            context.Response.StatusCode = ResponseStatusCode.Success;
            if (_gatewayOptions.WrapResult)
            {
                var responseResult = new ResponseResultDto()
                {
                    Data = excuteResult,
                    Status = StatusCode.Success,
                };
                var responseData = _serializer.Serialize(responseResult);
                context.Response.ContentLength = responseData.GetBytes().Length;
                await context.Response.WriteAsync(responseData);
            }
            else
            {
                if (excuteResult != null)
                {
                    var responseData = _serializer.Serialize(excuteResult);
                    context.Response.ContentLength = responseData.GetBytes().Length;
                    await context.Response.WriteAsync(responseData);
                }
                else
                {
                    context.Response.ContentLength = 0;
                    await context.Response.WriteAsync(string.Empty);
                }
            }
        }
    }
}