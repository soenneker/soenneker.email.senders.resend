[![](https://img.shields.io/nuget/v/soenneker.email.senders.resend.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.email.senders.resend/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.email.senders.resend/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.email.senders.resend/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.email.senders.resend.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.email.senders.resend/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.email.senders.resend/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.email.senders.resend/actions/workflows/codeql.yml)

# Soenneker.Email.Senders.Resend

A high-level utility responsible for orchestrating the creation and delivery of templated email messages using Resend.

## Install

```bash
dotnet add package Soenneker.Email.Senders.Resend
```

## Quick start

```csharp
using Soenneker.Email.Senders.Resend.Registrars;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
var result = services.AddResendEmailSenderAsSingleton();
```

Adds `IEmailSender` as a singleton service.

## What you get

- `ResendEmailSenderRegistrar` — A high-level utility responsible for orchestrating the creation and delivery of templated email messages using Resend.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `ResendEmailSenderRegistrar.AddResendEmailSenderAsSingleton(services)` | Adds `IEmailSender` as a singleton service. | The same service collection, so additional registrations can be chained. |
| `ResendEmailSenderRegistrar.AddResendEmailSenderAsScoped(services)` | Adds `IEmailSender` as a scoped service. | The same service collection, so additional registrations can be chained. |
