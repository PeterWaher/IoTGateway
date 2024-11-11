using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.XMPP.P2P.E2E;

namespace Waher.Networking.XMPP.Test.E2eTests.HttpxTests
{
    [TestClass]
    public class NistP521Tests : XmppHttpxTests
	{
		[ClassInitialize]
		public static void ClassInitialize(TestContext _)
		{
			SetupSnifferAndLog();
		}

		[ClassCleanup]
		public static void ClassCleanup()
		{
			DisposeSnifferAndLog();
		}

		public override IE2eEndpoint GenerateEndpoint(IE2eSymmetricCipher Cipher)
        {
            return new NistP521Endpoint(Cipher);
        }

    }
}
