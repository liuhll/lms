﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Runtime.Server.Parameter;

namespace Lms.Rpc.Runtime.Client
{
    public class DefaultRemoteServiceExecutor : IRemoteServiceExecutor
    {
        public async Task<object> Execute(ServiceEntry serviceEntry, IDictionary<ParameterFrom, object> parameters)
        {
            return "远程服务执行者";
        }
    }
}