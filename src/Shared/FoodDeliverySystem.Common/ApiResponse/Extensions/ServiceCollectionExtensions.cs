using FluentValidation;
using FoodDeliverySystem.Common.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FoodDeliverySystem.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommonMediatRAndValidation(this IServiceCollection services, params Assembly[] assemblies)
    {
        // Đăng ký MediatR
        services.AddMediatR(cfg =>
        {
            // Đăng ký handlers từ các assembly được truyền vào
            cfg.RegisterServicesFromAssemblies(assemblies);

            // Thêm ValidationBehavior
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // Đăng ký tất cả validator từ các assembly
        services.AddValidatorsFromAssemblies(assemblies);

        return services;
    }
}