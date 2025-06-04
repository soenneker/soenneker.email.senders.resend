using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.Email.Senders.Resend.Tests;

[Collection("Collection")]
public sealed class ResendEmailSenderTests : FixturedUnitTest
{
    public ResendEmailSenderTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
    }

    [Fact]
    public void Default()
    {

    }
}
