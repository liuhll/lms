﻿using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Silky.Castle;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Modularity;
using Silky.Lock;
using Microsoft.Extensions.DependencyInjection;
using Silky.Caching;
using Silky.Rpc.Address;
using Silky.Rpc.Address.Selector;
using Silky.Rpc.Configuration;
using Silky.Rpc.Interceptors;
using Silky.Rpc.Messages;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Transport;
using Silky.Rpc.Transport.Codec;
using Microsoft.Extensions.Configuration;

namespace Silky.Rpc
{
    [DependsOn(typeof(LockModule), typeof(CachingModule))]
    public class RpcModule : SilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<RpcOptions>()
                .Bind(configuration.GetSection(RpcOptions.Rpc));
            services.AddOptions<RegistryCenterOptions>()
                .Bind(configuration.GetSection(RegistryCenterOptions.RegistryCenter));
            services.AddOptions<GovernanceOptions>()
                .Bind(configuration.GetSection(GovernanceOptions.Governance));
            services.AddOptions<WebSocketOptions>()
                .Bind(configuration.GetSection(WebSocketOptions.WebSocket));
        }
        
        protected override void RegisterServices(ContainerBuilder builder)
        {
            var localEntryTypes = ServiceEntryHelper.FindServiceLocalEntryTypes(EngineContext.Current.TypeFinder)
                .ToArray();
            builder.RegisterTypes(localEntryTypes)
                .PropertiesAutowired()
                .AsSelf()
                .InstancePerDependency()
                .AsImplementedInterfaces();

            var serviceKeyTypes =
                localEntryTypes.Where(p => p.GetCustomAttributes().OfType<ServiceKeyAttribute>().Any());
            foreach (var serviceKeyType in serviceKeyTypes)
            {
                var serviceKeyAttribute = serviceKeyType.GetCustomAttributes().OfType<ServiceKeyAttribute>().First();
                builder.RegisterType(serviceKeyType).Named(serviceKeyAttribute.Name,
                        serviceKeyType.GetInterfaces().First(p =>
                            p.GetCustomAttributes().OfType<IRouteTemplateProvider>().Any()))
                    ;
            }

            RegisterServicesForAddressSelector(builder);
            RegisterServicesExecutor(builder);
        }

        public async override Task Initialize(ApplicationContext applicationContext)
        {
            var serviceRouteManager = applicationContext.ServiceProvider.GetService<IServiceRouteManager>();
            if (serviceRouteManager == null)
            {
                throw new SilkyException("您必须指定依赖的服务注册中心模块");
            }

            await serviceRouteManager.CreateSubscribeDataChanges();
            await serviceRouteManager.EnterRoutes();
            var messageListeners = applicationContext.ServiceProvider.GetServices<IServerMessageListener>();
            if (messageListeners.Any())
            {
                foreach (var messageListener in messageListeners)
                {
                    messageListener.Received += async (sender, message) =>
                    {
                        Debug.Assert(message.IsInvokeMessage());
                        var remoteInvokeMessage = message.GetContent<RemoteInvokeMessage>();
                        var messageReceivedHandler = EngineContext.Current.Resolve<IServiceMessageReceivedHandler>();
                        await messageReceivedHandler.Handle(message.Id, sender, remoteInvokeMessage);
                    };
                }
            }
        }


        private void RegisterServicesForAddressSelector(ContainerBuilder builder)
        {
            builder.RegisterType<DefaultTransportMessageEncoder>().AsSelf().AsImplementedInterfaces()
                .InstancePerDependency();
            builder.RegisterType<DefaultTransportMessageDecoder>().AsSelf().AsImplementedInterfaces()
                .InstancePerDependency();

            builder.RegisterType<PollingAddressSelector>()
                .SingleInstance()
                .AsSelf()
                .Named<IAddressSelector>(AddressSelectorMode.Polling.ToString());
            builder.RegisterType<PollingAddressSelector>()
                .SingleInstance()
                .AsSelf()
                .Named<IAddressSelector>(AddressSelectorMode.Random.ToString());
            builder.RegisterType<HashAlgorithmAddressSelector>()
                .SingleInstance()
                .AsSelf()
                .Named<IAddressSelector>(AddressSelectorMode.HashAlgorithm.ToString());
        }

        private void RegisterServicesExecutor(ContainerBuilder builder)
        {
            builder.RegisterType<DefaultLocalExecutor>()
                .As<ILocalExecutor>()
                .InstancePerLifetimeScope()
                ;

            builder.RegisterType<DefaultRemoteServiceExecutor>()
                .As<IRemoteServiceExecutor>()
                .InstancePerLifetimeScope()
                ;

            builder.RegisterType<DefaultServiceExecutor>()
                .As<IServiceExecutor>()
                .InstancePerLifetimeScope()
                .AddInterceptors(
                    typeof(RpcValidationInterceptor),
                    typeof(CachingInterceptor)
                )
                ;
        }
    }
}