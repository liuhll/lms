﻿using Silky.Rpc.Runtime.Server;

namespace Silky.Transaction.Handler
{
    public abstract class AbstractTransactionHandlerFactory : ITransactionHandlerFactory
    {
        public abstract ITransactionHandler FactoryOf(TransactionContext context, ServiceEntry serviceEntry, string serviceKey);

    }
}