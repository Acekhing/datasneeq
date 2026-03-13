using DataSneeq.Application.Interfaces;
using DataSneeq.Application.Services;
using DataSneeq.Application.Transformations;
using DataSneeq.Infrastructure.Database.Providers;
using DataSneeq.Infrastructure.Excel;
using DataSneeq.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DataSneeq.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string sqliteConnectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(sqliteConnectionString));

        services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddMemoryCache();

        services.AddScoped<IExcelParsingService, ExcelParsingService>();
        services.AddScoped<IDatabaseProvider, PostgreSqlDatabaseProvider>();
        services.AddScoped<IColumnMatchingService, ColumnMatchingService>();
        services.AddScoped<IDataValidationService, DataValidationService>();
        services.AddScoped<IForeignKeyResolutionService, ForeignKeyResolutionService>();
        services.AddScoped<IForeignKeyBuildService, ForeignKeyBuildService>();
        services.AddScoped<IMappingTemplateService, MappingTemplateService>();
        services.AddScoped<ITransformationEngine, TransformationEngine>();
        services.AddSingleton<IUploadSessionService, UploadSessionService>();
        services.AddScoped<IUploadOrchestrationService, UploadOrchestrationService>();

        return services;
    }
}
