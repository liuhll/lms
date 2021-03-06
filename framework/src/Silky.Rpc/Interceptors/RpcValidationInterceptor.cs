using System;
using System.Diagnostics;
using Silky.Core;
using Silky.Core.DynamicProxy;
using Silky.Validation;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Interceptors
{
    public class RpcValidationInterceptor : ValidationInterceptor
    {
        private readonly IMiniProfiler _miniProfiler;

        public RpcValidationInterceptor(IMethodInvocationValidator methodInvocationValidator,
            IMiniProfiler miniProfiler)
            : base(methodInvocationValidator)
        {
            _miniProfiler = miniProfiler;
        }

        protected override void Validate(ISilkyMethodInvocation invocation)
        {
            _miniProfiler.Print(MiniProfileConstant.ValidationInterceptor.Name,
                MiniProfileConstant.ValidationInterceptor.State.Begin,
                $"开始校验输入参数");
            try
            {
                var serviceEntry = invocation.ArgumentsDictionary["serviceEntry"] as ServiceEntry;
                Debug.Assert(serviceEntry != null);
                var parameters = invocation.ArgumentsDictionary["parameters"] as object[];
                parameters = serviceEntry.ConvertParameters(parameters);
                _methodInvocationValidator.Validate(
                    new MethodInvocationValidationContext(
                        serviceEntry.MethodInfo,
                        parameters
                    )
                );
                _miniProfiler.Print(MiniProfileConstant.ValidationInterceptor.Name,
                    MiniProfileConstant.ValidationInterceptor.State.Success,
                    $"输入参数校验成功");
            }
            catch (Exception e)
            {
                _miniProfiler.Print(MiniProfileConstant.ValidationInterceptor.Name,
                    MiniProfileConstant.ValidationInterceptor.State.Fail,
                    $"输入参数校验失败", true);
                throw;
            }
        }
    }
}