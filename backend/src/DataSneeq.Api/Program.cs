using DataSneeq.Api.Middleware;
using DataSneeq.Infrastructure;
using DataSneeq.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var sqliteConn = builder.Configuration.GetConnectionString("AppDb") ?? "Data Source=datasneeq.db";
builder.Services.AddInfrastructure(sqliteConn);

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:3000" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.Migrate();
    }
    catch (SqliteException ex) when (ex.SqliteErrorCode == 1 && ex.Message.Contains("already exists"))
    {
        // Table was created by EnsureCreated before migrations; add missing column and mark migration applied
        try
        {
            db.Database.ExecuteSqlRaw(
                """ALTER TABLE "MappingTemplates" ADD COLUMN "PrimaryKeyGenerationStrategy" INTEGER NOT NULL DEFAULT 1""");
        }
        catch (SqliteException addColEx) when (addColEx.Message.Contains("duplicate column name"))
        {
            // Column already present (e.g. EnsureCreated ran with updated model)
        }

        db.Database.ExecuteSqlRaw(
            """INSERT OR IGNORE INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES ('20260313075924_AddPrimaryKeyGenerationStrategy', '8.0.11')""");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("AllowFrontend");
app.MapControllers();

app.Run();
