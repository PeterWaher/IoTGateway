using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.XMPP.P2P.SymmetricCiphers;

namespace Waher.Networking.XMPP.Test.E2eTests
{
	public abstract class XmppE2eTests : E2eTests
	{
		[TestMethod]
		public async Task Test_01_Message_AES()
		{
			await this.Test_Message(
				this.GenerateEndpoints(new Aes256()),
				this.GenerateEndpoints(new Aes256()));
		}

		[TestMethod]
		public async Task Test_02_Message_ChaCha20()
		{
			await this.Test_Message(
				this.GenerateEndpoints(new ChaCha20()),
				this.GenerateEndpoints(new ChaCha20()));
		}

		[TestMethod]
		public async Task Test_03_Message_AEAD_ChaCha20_Poly1305()
		{
			await this.Test_Message(
				this.GenerateEndpoints(new AeadChaCha20Poly1305()),
				this.GenerateEndpoints(new AeadChaCha20Poly1305()));
		}

		private async Task Test_Message(IE2eEndpoint[] Endpoints1, IE2eEndpoint[] Endpoints2)
		{
			this.endpoints1 = Endpoints1;
			this.endpoints2 = Endpoints2;

			await this.ConnectClients();
			try
			{
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
				this.endpointSecurity1?.Dispose();
				this.endpointSecurity2?.Dispose();

				await this.DisposeClients();
			}
		}

		[TestMethod]
		public Task Test_04_IQ_Get_AES()
		{
			return this.Test_IQ_Get(
				this.GenerateEndpoints(new Aes256()),
				this.GenerateEndpoints(new Aes256()));
		}

		[TestMethod]
		public Task Test_05_IQ_Get_ChaCha20()
		{
			return this.Test_IQ_Get(
				this.GenerateEndpoints(new ChaCha20()),
				this.GenerateEndpoints(new ChaCha20()));
		}

		[TestMethod]
		public Task Test_06_IQ_Get_AEAD_ChaCha20_Poly1305()
		{
			return this.Test_IQ_Get(
				this.GenerateEndpoints(new AeadChaCha20Poly1305()),
				this.GenerateEndpoints(new AeadChaCha20Poly1305()));
		}

		private Task Test_IQ_Get(IE2eEndpoint[] Endpoints1, IE2eEndpoint[] Endpoints2)
		{
			return this.Test_IQ_Get(Endpoints1, Endpoints2, "Hello", "Hello", true);
		}

		private async Task Test_IQ_Get(IE2eEndpoint[] Endpoints1, IE2eEndpoint[] Endpoints2,
			string Send, string Check, bool ExpectOk)
		{
			this.endpoints1 = Endpoints1;
			this.endpoints2 = Endpoints2;

			await this.ConnectClients();
			try
			{
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
							(!ExpectOk || (e.FirstElement is not null &&
							e.FirstElement.LocalName == "test" &&
							e.FirstElement.NamespaceURI == "testns" &&
							e.FirstElement.InnerText == "World")))
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
				this.endpointSecurity1?.Dispose();
				this.endpointSecurity2?.Dispose();

				await this.DisposeClients();
			}
		}

		[TestMethod]
		public Task Test_07_IQ_Set_AES()
		{
			return this.Test_IQ_Set(
				this.GenerateEndpoints(new Aes256()),
				this.GenerateEndpoints(new Aes256()));
		}

		[TestMethod]
		public Task Test_08_IQ_Set_ChaCha20()
		{
			return this.Test_IQ_Set(
				this.GenerateEndpoints(new ChaCha20()),
				this.GenerateEndpoints(new ChaCha20()));
		}

		[TestMethod]
		public Task Test_09_IQ_Set_AEAD_ChaCha20_Poly1305()
		{
			return this.Test_IQ_Set(
				this.GenerateEndpoints(new AeadChaCha20Poly1305()),
				this.GenerateEndpoints(new AeadChaCha20Poly1305()));
		}

		private async Task Test_IQ_Set(IE2eEndpoint[] Endpoints1, IE2eEndpoint[] Endpoints2)
		{
			this.endpoints1 = Endpoints1;
			this.endpoints2 = Endpoints2;

			await this.ConnectClients();
			try
			{
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
				this.endpointSecurity1?.Dispose();
				this.endpointSecurity2?.Dispose();

				await this.DisposeClients();
			}
		}

		[TestMethod]
		public Task Test_10_IQ_Error_AES()
		{
			return this.Test_IQ_Error(
				this.GenerateEndpoints(new Aes256()),
				this.GenerateEndpoints(new Aes256()));
		}

		[TestMethod]
		public Task Test_11_IQ_Error_ChaCha20()
		{
			return this.Test_IQ_Error(
				this.GenerateEndpoints(new ChaCha20()),
				this.GenerateEndpoints(new ChaCha20()));
		}

		[TestMethod]
		public Task Test_12_IQ_Error_AEAD_ChaCha20_Poly1305()
		{
			return this.Test_IQ_Error(
				this.GenerateEndpoints(new AeadChaCha20Poly1305()),
				this.GenerateEndpoints(new AeadChaCha20Poly1305()));
		}

		private Task Test_IQ_Error(IE2eEndpoint[] Endpoints1, IE2eEndpoint[] Endpoints2)
		{
			return this.Test_IQ_Get(Endpoints1, Endpoints2, "Hello", "Bye", false);
		}

		[TestMethod]
		public void Test_13_Binary_AES()
		{
			Test_Binary(
				this.GenerateEndpoints(new Aes256()),
				this.GenerateEndpoints(new Aes256()));
		}

		[TestMethod]
		public void Test_14_Binary_ChaCha20()
		{
			Test_Binary(
				this.GenerateEndpoints(new ChaCha20()),
				this.GenerateEndpoints(new ChaCha20()));
		}

		[TestMethod]
		public void Test_15_Binary_AEAD_ChaCha20_Poly1305()
		{
			Test_Binary(
				this.GenerateEndpoints(new AeadChaCha20Poly1305()),
				this.GenerateEndpoints(new AeadChaCha20Poly1305()));
		}

		private static void Test_Binary(IE2eEndpoint[] Endpoints1, IE2eEndpoint[] Endpoints2)
		{
			if (Endpoints1 is null || Endpoints2 is null)
				return;

			IE2eEndpoint Endpoint1 = Endpoints1[0];
			IE2eEndpoint Endpoint2 = Endpoints2[0];

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
			Assert.AreEqual(c, Decrypted.Length, "Length mismatch.");

			for (i = 0; i < c; i++)
				Assert.AreEqual(Data[i], Decrypted[i], "Encryption/Decryption failed.");
		}

		[TestMethod]
		public Task Test_16_Stream_AES()
		{
			return Test_Stream(
				this.GenerateEndpoints(new Aes256()),
				this.GenerateEndpoints(new Aes256()));
		}

		[TestMethod]
		public Task Test_17_Stream_ChaCha20()
		{
			return Test_Stream(
				this.GenerateEndpoints(new ChaCha20()),
				this.GenerateEndpoints(new ChaCha20()));
		}

		[TestMethod]
		public Task Test_18_Stream_AEAD_ChaCha20_Poly1305()
		{
			return Test_Stream(
				this.GenerateEndpoints(new AeadChaCha20Poly1305()),
				this.GenerateEndpoints(new AeadChaCha20Poly1305()));
		}

		private static async Task Test_Stream(IE2eEndpoint[] Endpoints1, IE2eEndpoint[] Endpoints2)
		{
			if (Endpoints1 is null || Endpoints2 is null)
				return;

			IE2eEndpoint Endpoint1 = Endpoints1[0];
			IE2eEndpoint Endpoint2 = Endpoints2[0];

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
				i = await Data.ReadAsync(Temp, 0, Temp.Length);
				Assert.AreEqual(i, await Decrypted.ReadAsync(Temp2, 0, Temp2.Length));

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
