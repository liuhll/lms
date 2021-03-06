using System.Collections.Generic;
using System.Linq;
using Silky.Rpc.Address;
using Silky.Rpc.Address.Descriptor;

namespace Silky.Rpc.Routing.Descriptor
{
    public static class ServiceRouteDescriptorExtensions
    {
        public static ServiceRoute ConvertToServiceRoute(this ServiceRouteDescriptor serviceRouteDescriptor)
        {
            var serviceRoute = new ServiceRoute()
            {
                ServiceDescriptor = serviceRouteDescriptor.ServiceDescriptor,
                Addresses = serviceRouteDescriptor.AddressDescriptors.Select(p => p.ConvertToAddressModel()).ToArray()
            };
            return serviceRoute;
        }
    }
}