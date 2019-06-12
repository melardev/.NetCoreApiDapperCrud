using ApiCoreDapperCrud.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ApiCoreDapperCrud.Infrastructure.Extensions
{
    public static class IoCExtensions
    {
        public static void AddAppServices(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Both implementations work fine
            services.AddTransient<ITodoService, TodoService>();
            services.AddTransient<ITodoService, TodoStoredProceduresService>();
        }
    }
}