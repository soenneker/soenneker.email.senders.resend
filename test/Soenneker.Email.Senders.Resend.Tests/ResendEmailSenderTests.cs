using Soenneker.Email.Senders.Resend.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.Email.Senders.Resend.Tests;

[Collection("Collection")]
public sealed class ResendEmailSenderTests : FixturedUnitTest
{
    private readonly IResendEmailSender _util;

    public ResendEmailSenderTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IResendEmailSender>(true);
    }

    [Fact]
    public void Default()
    {

    }
}
