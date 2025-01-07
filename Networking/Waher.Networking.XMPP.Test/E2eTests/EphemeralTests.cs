using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Test.E2eTests
{
    [TestClass]
    public class EphemeralTests : XmppE2eTests
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

		public override int SecurityStrength => 128;

		public override IE2eEndpoint[] GenerateEndpoints(IE2eSymmetricCipher Cipher)
        {
            return null;
        }
    }
}
