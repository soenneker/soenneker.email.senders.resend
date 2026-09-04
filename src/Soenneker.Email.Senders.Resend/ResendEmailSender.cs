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
using Soenneker.Extensions.String;
using Soenneker.Extensions.Dictionaries.StringString;
using Soenneker.Enums.Email.Format;

namespace Soenneker.Email.Senders.Resend;

/// <inheritdoc cref="IEmailSender" />
public sealed class ResendEmailSender : IEmailSender
{
    private readonly IResendEmailsUtil _resendEmailsUtil;
    private readonly ILogger<ResendEmailSender> _logger;
    private readonly ITemplateUtil _templateUtil;
    private readonly bool _enabled;

    private const string _defaultTemplate = "default.html";
    private readonly string _defaultAddress;
    private readonly string _defaultName;
    private readonly string _templatesRoot;
    private readonly string _contentsRoot;

    public ResendEmailSender(IResendEmailsUtil resendEmailsUtil, IConfiguration configuration, ILogger<ResendEmailSender> logger, ITemplateUtil templateUtil)
    {
        _resendEmailsUtil = resendEmailsUtil;
        _logger = logger;
        _templateUtil = templateUtil;

        _enabled = configuration.GetValueStrict<bool>("Email:Enabled");
        _defaultAddress = configuration.GetValueStrict<string>("Email:DefaultAddress");
        _defaultName = configuration.GetValueStrict<string>("Email:DefaultName");

        string resourcesRoot = Path.Combine(AppContext.BaseDirectory, "LocalResources", "Email");
        _templatesRoot = Path.Combine(resourcesRoot, "Templates");
        _contentsRoot = Path.Combine(resourcesRoot, "Contents");
    }

    public Task<bool> Send(string messageContent, string type, CancellationToken cancellationToken = default)
    {
        if (!_enabled)
        {
            _logger.LogDebug("{name} has been disabled from config", nameof(ResendEmailSender));
            return Task.FromResult(false);
        }

        var msgModel = JsonUtil.Deserialize<EmailMessage>(messageContent);

        if (msgModel is null)
            throw new InvalidOperationException($"Service bus message was not a {nameof(EmailMessage)}");

        return Send(msgModel, cancellationToken);
    }

    public async Task<bool> Send(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (!_enabled)
        {
            _logger.LogDebug("{name} has been disabled from config", nameof(ResendEmailSender));
            return false;
        }

        string html = await BuildHtml(message, cancellationToken)
            .NoSync();

        string address = message.Address ?? _defaultAddress;
        string? name = message.Name ?? _defaultName;
        string from = name.HasContent() ? $"{name} <{address}>" : address;

        List<string>? replyTo = null;
        if (message.ReplyTo.HasContent())
        {
            replyTo = [message.ReplyTo];
        }

        string? providerId = message.Format == EmailFormat.Plaintext
            ? await _resendEmailsUtil.Send(from, message.To, message.Subject, null, html, message.Cc, message.Bcc, replyTo, null, null, null, cancellationToken)
                                     .NoSync()
            : await _resendEmailsUtil.Send(from, message.To, message.Subject, html, null, message.Cc, message.Bcc, replyTo, null, null, null, cancellationToken)
                                     .NoSync();

        return providerId is not null;
    }

    private async ValueTask<string> BuildHtml(EmailMessage message, CancellationToken cancellationToken)
    {
        message.TemplateFileName ??= _defaultTemplate;

        string templateFilePath = ResolveResourceFile(_templatesRoot, message.TemplateFileName, "template");

        string? contentFilePath = null;

        if (message.ContentFileName != null)
            contentFilePath = ResolveResourceFile(_contentsRoot, message.ContentFileName, "content");

        Dictionary<string, object> tokens = message.Tokens != null ? message.Tokens.ToObjectDictionary() : new Dictionary<string, object>();

        tokens["subject"] = message.Subject;

        if (contentFilePath != null)
            return await _templateUtil.RenderWithContent(templateFilePath, tokens, contentFilePath, "bodyText", message.Partials, cancellationToken)
                                      .NoSync();

        return await _templateUtil.Render(templateFilePath, tokens, message.Partials, cancellationToken)
                                  .NoSync();
    }

    private static string ResolveResourceFile(string root, string relativePath, string description)
    {
        string fullRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root)) + Path.DirectorySeparatorChar;
        string fullPath = Path.GetFullPath(Path.Combine(fullRoot, relativePath));

        if (!fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"The email {description} file must be located under {fullRoot}");

        return fullPath;
    }
}
