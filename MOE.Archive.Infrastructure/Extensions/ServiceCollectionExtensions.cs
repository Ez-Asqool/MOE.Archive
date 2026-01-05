using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MOE.Archive.Application.Auth.Services;
using MOE.Archive.Application.Common.Storage;
using MOE.Archive.Domain.Interfaces;
using MOE.Archive.Infrastructure.Auth;
using MOE.Archive.Infrastructure.Data;
using MOE.Archive.Infrastructure.Identity;
using MOE.Archive.Infrastructure.Repositories;
using MOE.Archive.Infrastructure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 1) DbContext – SQL Server
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"));
            });

            // 2) Identity using ApplicationUser + ApplicationRole
            services
                    .AddIdentityCore<ApplicationUser>(options =>
                    {
                        options.Password.RequireNonAlphanumeric = false;
                        options.Password.RequiredLength = 6;
                    })
                    .AddRoles<ApplicationRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

            // 3) Your repositories & services will be added here later

            // Repositories
            services.AddScoped<IDocumentRepository, DocumentRepository>();
            services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();

            // Unit Of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // wtToken Authentication Service
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IAuthService, AuthService>();

            // File Storage Service (Local)
            services.AddScoped<IFileStorage, LocalFileStorage>();

            services.AddScoped<IdentitySeeder>();
            return services;
        }
    }
}
