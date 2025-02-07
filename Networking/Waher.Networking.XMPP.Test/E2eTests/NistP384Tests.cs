using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Waher.Networking.XMPP.P2P.E2E;

namespace Waher.Networking.XMPP.Test.E2eTests
{
    [TestClass]
    public class NistP384Tests : XmppE2eTests
	{
		[ClassInitialize]
		public static void ClassInitialize(TestContext _)
		{
			SetupSnifferAndLog();
		}

		[ClassCleanup]
		public static async Task ClassCleanup()
		{
			await DisposeSnifferAndLog();
		}

		public override int SecurityStrength => 192;

		public override IE2eEndpoint[] GenerateEndpoints(IE2eSymmetricCipher Cipher)
        {
            return [new NistP384Endpoint(Cipher)];
        }

    }
}
