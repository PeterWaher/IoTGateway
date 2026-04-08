using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.P2P;
using Waher.Networking.XMPP.P2P.E2E;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;
using Waher.Runtime.Settings;

namespace Waher.Networking.XMPP.Test
{
	[TestClass]
	public class ContractsClientStateTests
	{
		private FilesProvider provider;
		private XmppClient client;
		private ContractsClient contractsClient;
		private string dataFolder;

		[ClassInitialize]
		public static void ClassInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(FilesProvider).Assembly,
				typeof(ObjectSerializer).Assembly,
				typeof(ContractsClientStateTests).Assembly,
				typeof(ContractsClient).Assembly,
				typeof(EndpointSecurity).Assembly,
				typeof(RuntimeSettings).Assembly,
				typeof(CaseInsensitiveString).Assembly);
		}

		[TestInitialize]
		public async Task TestInitialize()
		{
			this.dataFolder = Path.Combine(Path.GetTempPath(), "ContractsClientStateTests", Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(this.dataFolder);

			this.provider = await FilesProvider.CreateAsync(this.dataFolder, "Default", 8192, 1000, 8192, Encoding.UTF8, 10000, true);
			Database.Register(this.provider, false);

			this.client = new XmppClient(new XmppCredentials()
			{
				Host = "example.org",
				Port = 5222,
				Account = "contracts.state.test",
				Password = "test"
			}, "en", typeof(ContractsClientStateTests).Assembly);

			this.contractsClient = new ContractsClient(this.client, "legal.example.org");
			Assert.IsTrue(await this.contractsClient.LoadKeys(true));
		}

		[TestCleanup]
		public async Task TestCleanup()
		{
			if (this.client is not null)
			{
				await this.client.DisposeAsync();
				this.client = null;
			}

			if (this.provider is not null)
			{
				Database.Register(new NullDatabaseProvider(), false);
				await this.provider.DisposeAsync();
				this.provider = null;
			}

			if (!string.IsNullOrEmpty(this.dataFolder) && Directory.Exists(this.dataFolder))
				Directory.Delete(this.dataFolder, true);
		}

		[TestMethod]
		public async Task ContractsClient_State_Test_01_ExportKeysSkipsApprovedStatesWithoutMatchingLocalPrivateKey()
		{
			byte[] MatchingPublicKey = GetFirstLocalPublicKey(this.contractsClient);
			byte[] StalePublicKey = CreateStalePublicKey(MatchingPublicKey);

			await InsertStateAsync(this.client.BareJID, "matching-id", IdentityState.Approved, MatchingPublicKey, DateTime.UtcNow.AddMinutes(-1));
			await InsertStateAsync(this.client.BareJID, "stale-id", IdentityState.Approved, StalePublicKey, DateTime.UtcNow);

			string Xml = await this.contractsClient.ExportKeys();

			StringAssert.Contains(Xml, "matching-id");
			Assert.IsFalse(Xml.Contains("stale-id", StringComparison.Ordinal));
		}

		[TestMethod]
		public async Task ContractsClient_State_Test_02_GetLatestApprovedLegalIdIgnoresNullAndStalePublicKeys()
		{
			byte[] MatchingPublicKey = GetFirstLocalPublicKey(this.contractsClient);
			byte[] StalePublicKey = CreateStalePublicKey(MatchingPublicKey);

			await InsertStateAsync(this.client.BareJID, "matching-id", IdentityState.Approved, MatchingPublicKey, DateTime.UtcNow.AddMinutes(-2));
			await InsertStateAsync(this.client.BareJID, "null-key-id", IdentityState.Approved, null, DateTime.UtcNow.AddMinutes(-1));
			await InsertStateAsync(this.client.BareJID, "stale-id", IdentityState.Approved, StalePublicKey, DateTime.UtcNow);

			string LatestApprovedLegalId = await this.contractsClient.GetLatestApprovedLegalId();
			string LatestApprovedMatchingLegalId = await this.contractsClient.GetLatestApprovedLegalId(MatchingPublicKey);

			Assert.AreEqual("matching-id", LatestApprovedLegalId);
			Assert.AreEqual("matching-id", LatestApprovedMatchingLegalId);
		}

		[TestMethod]
		public async Task ContractsClient_State_Test_03_SignWithLatestApprovedIdUsesMatchingApprovedState()
		{
			byte[] MatchingPublicKey = GetFirstLocalPublicKey(this.contractsClient);
			byte[] StalePublicKey = CreateStalePublicKey(MatchingPublicKey);

			await InsertStateAsync(this.client.BareJID, "matching-id", IdentityState.Approved, MatchingPublicKey, DateTime.UtcNow.AddMinutes(-2));
			await InsertStateAsync(this.client.BareJID, "stale-id", IdentityState.Approved, StalePublicKey, DateTime.UtcNow);

			using MemoryStream Data = new MemoryStream(Encoding.UTF8.GetBytes("contracts-client-state-test"));
			byte[] Signature = await this.contractsClient.SignAsync(Data, SignWith.LatestApprovedId);

			Assert.IsNotNull(Signature);
			Assert.IsTrue(Signature.Length > 0);
		}

		[TestMethod]
		public async Task ContractsClient_State_Test_04_CanGenerateNewKeysReturnsTrueWhenActiveStatesHaveMatchingLocalKeys()
		{
			byte[] MatchingPublicKey = GetFirstLocalPublicKey(this.contractsClient);

			await InsertStateAsync(this.client.BareJID, "matching-id", IdentityState.Approved, MatchingPublicKey, DateTime.UtcNow);

			Assert.IsTrue(await this.contractsClient.CanGenerateNewKeys());
		}

		[TestMethod]
		public async Task ContractsClient_State_Test_05_CanGenerateNewKeysReturnsFalseWhenActiveStatesExistWithoutPersistedPrivateKeys()
		{
			byte[] MatchingPublicKey = GetFirstLocalPublicKey(this.contractsClient);

			await InsertStateAsync(this.client.BareJID, "matching-id", IdentityState.Approved, MatchingPublicKey, DateTime.UtcNow);
			await RuntimeSettings.DeleteWhereKeyLikeAsync(this.contractsClient.KeySettingsPrefix + "*", "*");

			Assert.IsFalse(await this.contractsClient.CanGenerateNewKeys());
		}

		private static byte[] GetFirstLocalPublicKey(ContractsClient ContractsClient)
		{
			FieldInfo LocalKeysField = typeof(ContractsClient).GetField("localKeys", BindingFlags.Instance | BindingFlags.NonPublic);
			EndpointSecurity LocalKeys = (EndpointSecurity)LocalKeysField.GetValue(ContractsClient);

			PropertyInfo KeysProperty = typeof(EndpointSecurity).GetProperty("Keys", BindingFlags.Instance | BindingFlags.NonPublic);
			IE2eEndpoint[] Keys = (IE2eEndpoint[])KeysProperty.GetValue(LocalKeys);

			Assert.IsNotNull(Keys);
			Assert.IsTrue(Keys.Length > 0);

			return (byte[])Keys[0].PublicKey.Clone();
		}

		private static byte[] CreateStalePublicKey(byte[] MatchingPublicKey)
		{
			byte[] Result = (byte[])MatchingPublicKey.Clone();
			Result[0] ^= 1;
			return Result;
		}

		private static async Task InsertStateAsync(string BareJid, string LegalId, IdentityState State, byte[] PublicKey, DateTime Timestamp)
		{
			await Database.Insert(new LegalIdentityState()
			{
				BareJid = BareJid,
				LegalId = LegalId,
				PublicKey = PublicKey,
				State = State,
				Timestamp = Timestamp
			});
		}
	}
}