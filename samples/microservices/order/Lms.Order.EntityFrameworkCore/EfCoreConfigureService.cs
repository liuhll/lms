using Lms.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TanvirArjel.EFCore.GenericRepository;

namespace Lms.Order.EntityFrameworkCore
{
    public class EfCoreConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<OrderDbContext>(opt =>
                    opt.UseMySql(configuration.GetConnectionString("Default"),
                        ServerVersion.AutoDetect(configuration.GetConnectionString("Default"))))
                .AddGenericRepository<OrderDbContext>(ServiceLifetime.Transient)
                ;
        }

        public int Order { get; } = 1;
    }
}