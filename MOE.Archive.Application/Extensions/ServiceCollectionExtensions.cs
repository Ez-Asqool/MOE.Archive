using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MOE.Archive.Application.Categories.Services;
using MOE.Archive.Application.Documents.Services;
using MOE.Archive.Application.Mappings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationLayer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);


            // ⚠️ IMPORTANT: do NOT register AuthService here,
            // because its implementation lives in Infrastructure.
            // Just register validators & pure application services.

            // AutoMapper
            services.AddAutoMapper(typeof(MappingProfile));

            // Services
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IDocumentService, DocumentService>();    

            return services;
        }
    }
}
