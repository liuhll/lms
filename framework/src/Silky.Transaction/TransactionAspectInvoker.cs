﻿using System.Threading.Tasks;
using Silky.Core;
using Silky.Core.DynamicProxy;
using Silky.Rpc.Runtime.Server;
using Silky.Transaction.Handler;

namespace Silky.Transaction
{
    public class TransactionAspectInvoker
    {
        private static TransactionAspectInvoker invoker = new();


        private TransactionAspectInvoker()
        {
        }

        public static TransactionAspectInvoker GetInstance()
        {
            return invoker;
        }

        public async Task Invoke(TransactionContext transactionContext, ISilkyMethodInvocation invocation)
        {
            var transactionHandlerFactory = EngineContext.Current.Resolve<ITransactionHandlerFactory>();
            if (transactionHandlerFactory != null)
            {
                var serviceEntry = invocation.ArgumentsDictionary["serviceEntry"] as ServiceEntry;
                var serviceKey = invocation.ArgumentsDictionary["serviceKey"] as string;

                var transactionHandler =
                    transactionHandlerFactory.FactoryOf(transactionContext, serviceEntry, serviceKey);
                if (transactionHandler != null)
                {
                    await transactionHandler.Handler(transactionContext, invocation);
                   
                }
                else
                {
                    await invocation.ProceedAsync();
                }
            }
            else
            {
                await invocation.ProceedAsync();
            }
        }
    }
}