using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.XMPP.P2P.E2E;

namespace Waher.Networking.XMPP.Test.E2eTests
{
    [TestClass]
    public class Rsa1024Tests : XmppE2eTests
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

		public override int SecurityStrength => 80;

		public override IE2eEndpoint[] GenerateEndpoints(IE2eSymmetricCipher Cipher)
        {
            RSA RSA = RSA.Create();
            RSA.KeySize = 1024;
            return [new RsaEndpoint(RSA, Cipher)];
        }

    }
}
