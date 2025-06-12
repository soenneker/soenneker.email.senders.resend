using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Soenneker.Email.Senders.Abstract;
using Soenneker.Extensions.Configuration;
using Soenneker.Extensions.ValueTask;
using Soenneker.Messages.Email;
using Soenneker.Resend.Emails.Abstract;
using Soenneker.Utils.Json;
using Soenneker.Utils.Template.Abstract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Soenneker.Extensions.String;
using Soenneker.Extensions.Dictionaries.StringString;

namespace Soenneker.Email.Senders.Resend;

/// <inheritdoc cref="IEmailSender"/>
public sealed class ResendEmailSender : IEmailSender
{
    private readonly IResendEmailsUtil _resendEmailsUtil;
    private readonly ILogger<ResendEmailSender> _logger;
    private readonly ITemplateUtil _templateUtil;
    private readonly IHostEnvironment _hostEnv;
    private readonly bool _enabled;

    private const string _defaultTemplate = "default.html";
    private readonly string _defaultAddress;
    private readonly string _defaultName;

    public ResendEmailSender(IResendEmailsUtil resendEmailsUtil, IConfiguration configuration, ILogger<ResendEmailSender> logger, ITemplateUtil templateUtil, IHostEnvironment hostEnv)
    {
        _resendEmailsUtil = resendEmailsUtil;
        _logger = logger;
        _templateUtil = templateUtil;
        _hostEnv = hostEnv;

        _enabled = configuration.GetValueStrict<bool>("Email:Enabled");
        _defaultAddress = configuration.GetValueStrict<string>("Email:DefaultAddress");
        _defaultName = configuration.GetValueStrict<string>("Email:DefaultName");
    }

    public Task<bool> Send(string messageContent, Type? type, CancellationToken cancellationToken = default)
    {
        if (!_enabled)
        {
            _logger.LogDebug("{name} has been disabled from config", nameof(ResendEmailSender));
            return Task.FromResult(false);
        }

        if (type == null)
            throw new Exception("Service bus message did not have a type");

        object? msgModel = JsonUtil.Deserialize(messageContent, type);

        if (msgModel is not EmailMessage message)
            throw new Exception($"Service bus message was not a {nameof(EmailMessage)}");

        return Send(message, cancellationToken);
    }

    public async Task<bool> Send(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (!_enabled)
        {
            _logger.LogDebug("{name} has been disabled from config", nameof(ResendEmailSender));
            return false;
        }

        string html = await BuildHtml(message, cancellationToken).NoSync();

        string from;

        if (message.Name != null)
            from = $"{message.Name} <{message.Address}>";
        else
            from = message.Address;

        List<string>? replyTo = null;
        if (message.ReplyTo.HasContent())
        {
            replyTo = [message.ReplyTo];
        }

        await _resendEmailsUtil.Send(from, message.To, message.Subject, html, null, message.Cc, message.Bcc, replyTo, null, null, null, cancellationToken)
                               .NoSync();

        return true;
    }

    private async ValueTask<string> BuildHtml(EmailMessage message, CancellationToken cancellationToken)
    {
        message.TemplateFileName ??= _defaultTemplate;

        message.Name ??= _defaultName;
        message.Address ??= _defaultAddress;

        string templateFilePath = Path.Combine(_hostEnv.ContentRootPath, "LocalResources", "Email", "Templates", message.TemplateFileName);

        string? contentFilePath = null;

        if (message.ContentFileName != null)
        {
            contentFilePath = Path.Combine(_hostEnv.ContentRootPath, "LocalResources", "Email", "Contents", message.ContentFileName);
        }

        Dictionary<string, object> tokens = message.Tokens != null ? message.Tokens.ToObjectDictionary() : new Dictionary<string, object>();

        tokens.Add("subject", message.Subject);

        if (contentFilePath != null)
        {
            return await _templateUtil.RenderWithContent(templateFilePath, tokens, contentFilePath, "bodyText", message.Partials, cancellationToken).NoSync();
        }

        return await _templateUtil.Render(templateFilePath, tokens, message.Partials, cancellationToken).NoSync();
    }
}