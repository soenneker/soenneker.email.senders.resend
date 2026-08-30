[![](https://img.shields.io/nuget/v/soenneker.email.senders.resend.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.email.senders.resend/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.email.senders.resend/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.email.senders.resend/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.email.senders.resend.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.email.senders.resend/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.email.senders.resend/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.email.senders.resend/actions/workflows/codeql.yml)

# Soenneker.Email.Senders.Resend

Renders `EmailMessage` payloads with Scriban templates and delivers them through the Resend API.

## Install

```bash
dotnet add package Soenneker.Email.Senders.Resend
```

## Resource layout

```text
LocalResources/
  Email/
    Templates/
      default.html
    Contents/
      welcome.html
```

Paths are resolved beneath the application's output directory. `TemplateFileName` defaults to `default.html`; `ContentFileName` is optional and, when supplied, is rendered into the outer template as `bodyText`. Absolute paths and traversal outside the template or content root are rejected.

Message tokens and partials become Scriban globals. The sender always sets the `subject` token from `EmailMessage.Subject`, replacing a caller-supplied token with that name.

## Configuration

```json
{
  "Email": {
    "Enabled": true,
    "DefaultAddress": "mailer@example.com",
    "DefaultName": "Example App"
  },
  "Resend": {
    "ApiKey": "use-a-secret-provider"
  }
}
```

All four values are required when resolving the sender. Keep the API key in a secret provider.

## Registration

```csharp
using Soenneker.Email.Senders.Resend.Registrars;

services.AddResendEmailSenderAsSingleton();
```

This registers `IEmailSender`, the Resend email utility/client, and the template utility as singletons.

For request-scoped rendering, use:

```csharp
services.AddResendEmailSenderAsScoped();
```

That registration makes the sender, email utility, and template utility scoped while deliberately retaining the Resend client utility as a singleton. Disposing a scope tears down the wrapper utilities without recreating or disposing the shared HTTP client on every request.

## Send an email

```csharp
using Soenneker.Email.Senders.Abstract;
using Soenneker.Enums.Email.Format;
using Soenneker.Enums.Email.Priority;
using Soenneker.Messages.Email;

var message = new EmailMessage
{
    Type = "email.welcome.v1",
    Id = Guid.NewGuid().ToString("N"),
    Queue = "email",
    Sender = "accounts-api",
    CreatedAt = DateTimeOffset.UtcNow,
    To = ["recipient@example.net"],
    Subject = "Welcome, Alex",
    Format = EmailFormat.Html,
    Priority = EmailPriority.Normal,
    ContentFileName = "welcome.html",
    Tokens = new Dictionary<string, string> { ["first_name"] = "Alex" }
};

IEmailSender sender = serviceProvider.GetRequiredService<IEmailSender>();
bool accepted = await sender.Send(message, cancellationToken);
```

HTML messages are sent through Resend's `html` field; plaintext messages use its `text` field. `Name` and `Address` fall back to the configured defaults, and `ReplyTo`, `To`, `Cc`, and `Bcc` are forwarded. `Priority` is not mapped because this adapter does not send a priority field to Resend.

The result is `true` only when Resend returns a provider email ID. It is `false` when email is disabled or the provider response has no ID. API, rendering, configuration, and deserialization failures are raised as exceptions.

The string overload always deserializes `messageContent` as `EmailMessage`; the accompanying `type` is transport metadata and does not select an arbitrary CLR type.
