﻿using Silky.Core;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Runtime.Server;
using Silky.Transaction.Handler;

namespace Silky.Transaction.Tcc.Handlers
{
    public class TccTransactionHandlerFactory : AbstractTransactionHandlerFactory, ITransientDependency
    {
        public override ITransactionHandler FactoryOf(TransactionContext context, ServiceEntry serviceEntry,
            string serviceKey)
        {
            if (context == null)
            {
                var tccTransactionProvider = serviceEntry.GetTccTransactionProvider(serviceKey);
                if (tccTransactionProvider != null)
                {
                    return EngineContext.Current.ResolveNamed<ITransactionHandler>(TransactionRole.Start.ToString());
                }

                return null;
            }

            ITransactionHandler handler = null;
            switch (context.TransactionRole)
            {
                case TransactionRole.Participant:
                    handler = EngineContext.Current.ResolveNamed<ITransactionHandler>(TransactionRole.Participant
                        .ToString());
                    break;
                case TransactionRole.Start:
                case TransactionRole.Consumer:
                    handler = EngineContext.Current.ResolveNamed<ITransactionHandler>(
                        TransactionRole.Consumer.ToString());
                    break;
            }

            return handler;
        }
    }
}