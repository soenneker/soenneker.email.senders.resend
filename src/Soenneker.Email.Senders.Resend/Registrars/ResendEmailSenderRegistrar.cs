using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Email.Senders.Abstract;
using Soenneker.Resend.Emails.Registrars;
using Soenneker.Utils.Template.Registrars;

namespace Soenneker.Email.Senders.Resend.Registrars;

/// <summary>
/// A high-level utility responsible for orchestrating the creation and delivery of templated email messages using Resend
/// </summary>
public static class ResendEmailSenderRegistrar
{
    /// <summary>
    /// Adds <see cref="IEmailSender"/> as a singleton service. <para/>
    /// </summary>
    public static IServiceCollection AddResendEmailSenderAsSingleton(this IServiceCollection services)
    {
        services.AddResendEmailsUtilAsSingleton().TryAddSingleton<IEmailSender, ResendEmailSender>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IEmailSender"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddResendEmailSenderAsScoped(this IServiceCollection services)
    {
        services.AddResendEmailsUtilAsScoped().AddTemplateUtilAsScoped().TryAddScoped<IEmailSender, ResendEmailSender>();

        return services;
    }
}