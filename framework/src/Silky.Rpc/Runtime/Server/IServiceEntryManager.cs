using System;
using System.Collections.Generic;
using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServiceEntryManager : ISingletonDependency
    {
        IReadOnlyList<ServiceEntry> GetLocalEntries();
        
        IReadOnlyList<ServiceEntry> GetAllEntries();

        void Update(ServiceEntry serviceEntry);

        event EventHandler<ServiceEntry> OnUpdate;

    }
}