using Soenneker.Tests.HostedUnit;

namespace Soenneker.Email.Senders.Resend.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class ResendEmailSenderTests : HostedUnitTest
{
    public ResendEmailSenderTests(Host host) : base(host)
    {
    }

    [Test]
    public void Default()
    {

    }
}
