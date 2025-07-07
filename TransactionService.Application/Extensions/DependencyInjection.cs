using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TransactionService.Application.Behaviors;
using TransactionService.Application.Validators;

namespace TransactionService.Application.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(typeof(DependencyInjection).Assembly);
            });

            services.AddValidatorsFromAssemblyContaining<CreditCommandValidator>();

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }
    }
}
