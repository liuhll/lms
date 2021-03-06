﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Silky.Core.Exceptions;
using Polly;
using Silky.Core;
using Silky.Rpc.Address.Selector;
using Silky.Rpc.Messages;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public class DefaultRemoteServiceExecutor : IRemoteServiceExecutor
    {
        private readonly IRemoteServiceInvoker _remoteServiceInvoker;
        private readonly IMiniProfiler _miniProfiler;
        public DefaultRemoteServiceExecutor(IRemoteServiceInvoker remoteServiceInvoker,
            IMiniProfiler miniProfiler)
        {
            _remoteServiceInvoker = remoteServiceInvoker;
            _miniProfiler = miniProfiler;
        }
        
        public async Task<object> Execute(ServiceEntry serviceEntry, object[] parameters, string serviceKey = null)
        {
            var remoteInvokeMessage = new RemoteInvokeMessage()
            {
                ServiceId = serviceEntry.ServiceDescriptor.Id,
                Parameters = parameters,
            };
            string hashKey = null;
            if (serviceEntry.GovernanceOptions.ShuntStrategy == AddressSelectorMode.HashAlgorithm)
            {
                hashKey = serviceEntry.GetHashKeyValue(parameters.ToArray());
                _miniProfiler.Print(MiniProfileConstant.Rpc.Name, MiniProfileConstant.Rpc.State.HashKey,
                    $"hashKey为:{hashKey}");
            }

            IAsyncPolicy<object> executePolicy = Policy<object>
                    .Handle<TimeoutException>()
                    .Or<CommunicatonException>()
                    .Or<OverflowMaxRequestException>()
                    .RetryAsync(serviceEntry.GovernanceOptions.FailoverCount)
                ;
            if (serviceEntry.FallBackExecutor != null)
            {
                var dictParams = serviceEntry.CreateDictParameters(parameters.ToArray());
                var fallbackPolicy = Policy<object>.Handle<SilkyException>(
                        ex => !ex.IsBusinessException()
                              && !(ex is CommunicatonException)
                              && !(ex is OverflowMaxRequestException)
                    )
                    .FallbackAsync<object>(serviceEntry.FallBackExecutor(new object[] {dictParams}).GetAwaiter()
                        .GetResult());
                executePolicy = Policy.WrapAsync(executePolicy, fallbackPolicy);
            }

            return await executePolicy
                .ExecuteAsync(async () =>
                {
                    var invokeResult =
                        await _remoteServiceInvoker.Invoke(remoteInvokeMessage, serviceEntry.GovernanceOptions,
                            hashKey);
                    return invokeResult.Result;
                });
        }
    }
}