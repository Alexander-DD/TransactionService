using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TransactionService.Application.Extensions;
using TransactionService.Domain.Exceptions;
using TransactionService.Infrastructure.Extensions;
using TransactionService.Infrastructure.Persistence;
using TransactionService.Infrastructure.Persistence.Entities;
namespace TransactionService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Serilog (Console only)
            builder.Host.UseSerilog((ctx, lc) => lc
                .WriteTo.Console()
                .ReadFrom.Configuration(ctx.Configuration));

            // Add Application-layer
            builder.Services.AddApplication();

            // Add Infrastructure-layer
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                                   ?? "Host=localhost;Port=5432;Database=transactionsdb;Username=postgres;Password=postgres";
            builder.Services.AddInfrastructure(connectionString);

            // Controllers + FluentValidation auto-validation
            builder.Services
                .AddControllers();

            // Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Health checks
            builder.Services.AddHealthChecks();

            // Set ProblemDetails for RFC 9457
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var problemDetails = new ValidationProblemDetails(context.ModelState);
                    return new BadRequestObjectResult(problemDetails);
                };
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
                if (env.IsDevelopment())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    // Ensure database is created and migrations are applied
                    // ONLY for DEVELOPMENT mode,
                    // to avoid issues in production with many instances.
                    db.Database.Migrate();

                    if (!db.Transactions.Any())
                    {
                        db.Transactions.Add(new TransactionEntity
                        {
                            Id = Guid.Parse("8f0452b2-867b-4ef8-9a9d-3c9c03d9afdf"),
                            ClientId = Guid.Parse("cfaa0d3f-7fea-4423-9f69-ebff826e2f89"),
                            Amount = (decimal)23.05,
                            DateTime = DateTime.Parse("2019-04-02T13:10:20.0263632+03:00").ToUniversalTime()
                        });

                        db.Transactions.Add(new TransactionEntity
                        {
                            Id = Guid.Parse("05eb235c-4955-4c16-bcdd-34e8178228de"),
                            ClientId = Guid.Parse("cfaa0d3f-7fea-4423-9f69-ebff826e2f89"),
                            Amount = (decimal)23.05,
                            DateTime = DateTime.Parse("2019-04-02T13:10:25.0263632+03:00").ToUniversalTime()
                        });

                        db.SaveChanges();
                    }
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRouting();

            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    var exception = exceptionHandlerPathFeature?.Error;

                    ProblemDetails problemDetails = exception switch
                    {
                        FluentValidation.ValidationException validationException => new ProblemDetails
                        {
                            Type = "https://tools.ietf.org/html/rfc9457#section-3",
                            Title = "Validation error",
                            Status = StatusCodes.Status400BadRequest,
                            Detail = validationException.Message,
                            Instance = context.Request.Path
                        },
                        DomainException domainException => new ProblemDetails
                        {
                            Type = "https://tools.ietf.org/html/rfc9457#section-3",
                            Title = "Domain error",
                            Status = StatusCodes.Status400BadRequest,
                            Detail = domainException.Message,
                            Instance = context.Request.Path
                        },
                        _ => new ProblemDetails
                        {
                            Type = "https://tools.ietf.org/html/rfc9457#section-3",
                            Title = "An unexpected error occurred",
                            Status = StatusCodes.Status500InternalServerError,
                            Detail = exception?.Message,
                            Instance = context.Request.Path
                        }
                    };

                    context.Response.ContentType = "application/problem+json";
                    context.Response.StatusCode = problemDetails.Status ?? 500;
                    await context.Response.WriteAsJsonAsync(problemDetails);
                });
            });

            app.UseAuthorization();

            app.MapControllers();
            app.MapHealthChecks("/health");

            app.Run();
        }
    }
}
