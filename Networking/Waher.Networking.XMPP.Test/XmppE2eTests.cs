using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Files;

[assembly: TestDataSourceDiscovery(TestDataSourceDiscoveryOption.DuringDiscovery)]

namespace Waher.Networking.XMPP.Test
{
	public enum AsymmetricCipher
	{
		BrainpoolP160,
		BrainpoolP192,
		BrainpoolP224,
		BrainpoolP256,
		BrainpoolP320,
		BrainpoolP384,
		BrainpoolP512,
		NistP192,
		NistP224,
		NistP256,
		NistP384,
		NistP521,
		Edwards25519,
		Edwards448,
		Curve25519,
		Curve448,
		Rsa,
		ModuleLattice128,
		ModuleLattice192,
		ModuleLattice256,
		Ephemeral
	}

	public enum SymmetricCipher
	{
		Aes256,
		ChaCha20,
		AeadChaCha20Poly1305
	}

	[TestClass]
	public class XmppE2eTests : E2eTests.E2eTests
	{
		private static FilesProvider provider;
		private static string dataFolder;

		[ClassInitialize]
		public static async Task ClassInitialize(TestContext _)
		{
			dataFolder = Path.Combine("Data", "XmppE2eTests");

			provider = await FilesProvider.CreateAsync(dataFolder, "Default", 8192, 1000, 8192, Encoding.UTF8, 10000, false);
			Database.Register(provider, false);

			SetupSnifferAndLog();
		}

		[ClassCleanup]
		public static async Task ClassCleanup()
		{
			await DisposeSnifferAndLog();

			if (provider is not null)
			{
				Database.Register(new NullDatabaseProvider(), false);
				await provider.DisposeAsync();
				provider = null;
			}

			if (!string.IsNullOrEmpty(dataFolder) && Directory.Exists(dataFolder))
				Directory.Delete(dataFolder, true);
		}

		public override async Task DisposeClients()
		{
			await base.DisposeClients();
			
			this.endpointSecurity1?.Dispose();
			this.endpointSecurity2?.Dispose();
		}

		[TestMethod]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.AeadChaCha20Poly1305, true)]
		public async Task Test_01_Message_AES(AsymmetricCipher AsymmetricCipherType, 
			int SecurityStrength, SymmetricCipher SymmetricCipherType, bool P2p)
		{
			this.PrepareEndpoints(AsymmetricCipherType, SecurityStrength, SymmetricCipherType);

			try
			{
				await this.ConnectClients(SecurityStrength, P2p);
				
				ManualResetEvent Done = new(false);
				ManualResetEvent Error = new(false);

				this.client2.OnNormalMessage += (Sender, e) =>
				{
					if (e.UsesE2eEncryption && e.Body == "Test message" && e.Subject == "Subject" && e.Id == "1")
						Done.Set();
					else
						Error.Set();

					return Task.CompletedTask;
				};

				await this.endpointSecurity1.SendMessage(this.client1, E2ETransmission.AssertE2E,
					QoSLevel.Unacknowledged, MessageType.Normal, "1", this.client2.FullJID,
					"<test/>", "Test message", "Subject", "en", string.Empty, string.Empty,
					null, null);

				Assert.AreEqual(0, WaitHandle.WaitAny([Done, Error], 5000));
			}
			finally
			{
				await this.DisposeClients();
			}
		}

		[TestMethod]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.AeadChaCha20Poly1305, true)]
		public Task Test_02_IQ_Get(AsymmetricCipher AsymmetricCipherType, 
			int SecurityStrength, SymmetricCipher SymmetricCipherType, bool P2p)
		{
			this.PrepareEndpoints(AsymmetricCipherType, SecurityStrength, SymmetricCipherType);

			return this.Test_IQ_Get("Hello", "Hello", true, SecurityStrength, P2p);
		}

		private async Task Test_IQ_Get(string Send, string Check, bool ExpectOk, int SecurityStrength, bool P2p)
		{
			try
			{
				await this.ConnectClients(SecurityStrength, P2p);
				
				ManualResetEvent Done = new(false);
				ManualResetEvent Error = new(false);

				this.client2.RegisterIqGetHandler("test", "testns", async (Sender, e) =>
				{
					if (e.UsesE2eEncryption &&
						e.E2eEncryption is not null &&
						!string.IsNullOrEmpty(e.E2eReference) &&
						e.E2eSymmetricCipher is not null &&
						e.Query.InnerText == Check)
					{
						await e.IqResult("<test xmlns='testns'>World</test>");
					}
					else
						await e.IqError(new StanzaErrors.BadRequestException("Bad request", e.IQ));
				}, true);

				await this.endpointSecurity1.SendIqGet(this.client1, E2ETransmission.AssertE2E,
					this.client2.FullJID, "<test xmlns='testns'>" + Send + "</test>", (Sender, e) =>
					{
						if (e.UsesE2eEncryption &&
							e.E2eEncryption is not null &&
							!string.IsNullOrEmpty(e.E2eReference) &&
							e.E2eSymmetricCipher is not null &&
							e.Ok == ExpectOk &&
							(!ExpectOk || e.FirstElement is not null &&
							e.FirstElement.LocalName == "test" &&
							e.FirstElement.NamespaceURI == "testns" &&
							e.FirstElement.InnerText == "World"))
						{
							Done.Set();
						}
						else
							Error.Set();

						return Task.CompletedTask;
					}, null);

				Assert.AreEqual(0, WaitHandle.WaitAny([Done, Error], 5000));
			}
			finally
			{
				await this.DisposeClients();
			}
		}

		[TestMethod]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.AeadChaCha20Poly1305, true)]
		public async Task Test_03_IQ_Set(AsymmetricCipher AsymmetricCipherType, 
			int SecurityStrength, SymmetricCipher SymmetricCipherType, bool P2p)
		{
			this.PrepareEndpoints(AsymmetricCipherType, SecurityStrength, SymmetricCipherType);

			try
			{
				await this.ConnectClients(SecurityStrength, P2p);
				
				ManualResetEvent Done = new(false);
				ManualResetEvent Error = new(false);

				this.client2.RegisterIqSetHandler("test", "testns", async (Sender, e) =>
				{
					if (e.UsesE2eEncryption &&
						e.E2eEncryption is not null &&
						!string.IsNullOrEmpty(e.E2eReference) &&
						e.E2eSymmetricCipher is not null &&
						e.Query.InnerText == "Hello")
					{
						await e.IqResult("<test xmlns='testns'>World</test>");
					}
					else
						await e.IqError(new StanzaErrors.BadRequestException("Bad request", e.IQ));
				}, true);

				await this.endpointSecurity1.SendIqSet(this.client1, E2ETransmission.AssertE2E,
					this.client2.FullJID, "<test xmlns='testns'>Hello</test>", (Sender, e) =>
					{
						if (e.E2eEncryption is not null &&
							!string.IsNullOrEmpty(e.E2eReference) &&
							e.E2eSymmetricCipher is not null &&
							e.Ok &&
							e.FirstElement is not null &&
							e.FirstElement.LocalName == "test" &&
							e.FirstElement.NamespaceURI == "testns" &&
							e.FirstElement.InnerText == "World")
						{
							Done.Set();
						}
						else
							Error.Set();

						return Task.CompletedTask;
					}, null);

				Assert.AreEqual(0, WaitHandle.WaitAny([Done, Error], 5000));
			}
			finally
			{
				await this.DisposeClients();
			}
		}

		[TestMethod]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.Aes256, false)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.ChaCha20, false)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.AeadChaCha20Poly1305, false)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.AeadChaCha20Poly1305, true)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.Aes256, true)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.ChaCha20, true)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.AeadChaCha20Poly1305, true)]
		public Task Test_04_IQ_Error(AsymmetricCipher AsymmetricCipherType, 
			int SecurityStrength, SymmetricCipher SymmetricCipherType, bool P2p)
		{
			this.PrepareEndpoints(AsymmetricCipherType, SecurityStrength, SymmetricCipherType);

			return this.Test_IQ_Get("Hello", "Bye", false, SecurityStrength, P2p);
		}

		[TestMethod]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		public void Test_05_Binary(AsymmetricCipher AsymmetricCipherType, 
			int SecurityStrength, SymmetricCipher SymmetricCipherType)
		{
			this.PrepareEndpoints(AsymmetricCipherType, SecurityStrength, SymmetricCipherType);
		
			IE2eEndpoint Endpoint1 = this.endpoints1?[0];
			IE2eEndpoint Endpoint2 = this.endpoints2?[0];

			byte[] Data = new byte[1024];
			using (RandomNumberGenerator Rnd = RandomNumberGenerator.Create())
			{
				Rnd.GetBytes(Data);
			}

			byte[] Encrypted = Endpoint1.DefaultSymmetricCipher.Encrypt(
				"ID", "Type", "From", "To", 1, Data, Endpoint1, Endpoint2);
			byte[] Decrypted = Endpoint2.DefaultSymmetricCipher.Decrypt(
				"ID", "Type", "From", "To", Encrypted, Endpoint1, Endpoint2);

			Assert.IsNotNull(Decrypted, "Decryption failed.");

			int i, c = Data.Length;
			Assert.HasCount(c, Decrypted, "Length mismatch.");

			for (i = 0; i < c; i++)
				Assert.AreEqual(Data[i], Decrypted[i], "Encryption/Decryption failed.");
		}

		[TestMethod]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP160, 80, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP192, 96, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP224, 112, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP256, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP320, 160, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP384, 192, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.BrainpoolP512, 256, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP192, 96, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP224, 112, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP256, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP384, 192, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.NistP521, 256, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Edwards25519, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Edwards448, 224, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Curve25519, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Curve448, 224, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Rsa, 96, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Rsa, 112, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Rsa, 140, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.ModuleLattice128, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.ModuleLattice192, 192, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.ModuleLattice256, 256, SymmetricCipher.AeadChaCha20Poly1305)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.Aes256)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.ChaCha20)]
		[DataRow(AsymmetricCipher.Ephemeral, 128, SymmetricCipher.AeadChaCha20Poly1305)]
		public async Task Test_06_Stream_AES(AsymmetricCipher AsymmetricCipherType, 
			int SecurityStrength, SymmetricCipher SymmetricCipherType)
		{
			this.PrepareEndpoints(AsymmetricCipherType, SecurityStrength, SymmetricCipherType);

			IE2eEndpoint Endpoint1 = this.endpoints1?[0];
			IE2eEndpoint Endpoint2 = this.endpoints2?[0];

			MemoryStream Data = new();
			byte[] Temp = new byte[1024];
			byte[] Temp2 = new byte[1024];
			int i;

			using (RandomNumberGenerator Rnd = RandomNumberGenerator.Create())
			{
				for (i = 0; i < 1024; i++)
				{
					Rnd.GetBytes(Temp);
					Data.Write(Temp, 0, Temp.Length);
				}
			}

			MemoryStream Encrypted = new();

			Data.Position = 0;
			await Endpoint1.DefaultSymmetricCipher.Encrypt(
				"ID", "Type", "From", "To", 1, Data, Encrypted, Endpoint1, Endpoint2);

			Encrypted.Position = 0;
			Stream Decrypted = await Endpoint2.DefaultSymmetricCipher.Decrypt(
				"ID", "Type", "From", "To", Encrypted, Endpoint1, Endpoint2);

			Assert.IsNotNull(Decrypted, "Decryption failed.");

			long c = Data.Length;
			Assert.AreEqual(c, Decrypted.Length, "Length mismatch.");

			Decrypted.Position = 0;
			Data.Position = 0;

			while (true)
			{
				i = await Data.ReadAsync(Temp, 0, Temp.Length, CancellationToken.None);
				Assert.AreEqual(i, await Decrypted.ReadAsync(Temp2, 0, Temp2.Length,
					CancellationToken.None));

				if (i <= 0)
					break;

				while (i > 0)
				{
					i--;
					Assert.AreEqual(Temp[i], Temp2[i], "Encryption/Decryption failed.");
				}
			}
		}

	}
}
