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
using Waher.Persistence.Filters;
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
		public async Task ContractsClient_State_Test_01_LegacyApprovedStateBackfillsPrivateKeySnapshot()
		{
			byte[] MatchingPublicKey = GetFirstLocalPublicKey(this.contractsClient);

			await InsertStateAsync(this.client.BareJID, "matching-id", IdentityState.Approved, MatchingPublicKey, DateTime.UtcNow);

			Assert.IsTrue(await this.contractsClient.HasPrivateKey("matching-id"));

			LegalIdentityState State = await GetStateAsync(this.client.BareJID, "matching-id");
			Assert.IsNotNull(State);
			Assert.IsTrue(State.HasPrivateKey);
			Assert.IsFalse(string.IsNullOrEmpty(State.KeyName));
			Assert.IsNotNull(State.PrivateKey);
			CollectionAssert.AreEqual(MatchingPublicKey, State.PublicKey);
		}

		[TestMethod]
		public async Task ContractsClient_State_Test_02_GenerateNewKeysMigratesActiveStateAndSigningStillWorks()
		{
			byte[] OldPublicKey = GetFirstLocalPublicKey(this.contractsClient);

			await InsertStateAsync(this.client.BareJID, "approved-id", IdentityState.Approved, OldPublicKey, DateTime.UtcNow);
			await AwaitOrTimeout(this.contractsClient.GenerateNewKeys(), "GenerateNewKeys");

			byte[] NewPublicKey = GetFirstLocalPublicKey(this.contractsClient);
			Assert.IsFalse(AreEqual(OldPublicKey, NewPublicKey));

			LegalIdentityState State = await GetStateAsync(this.client.BareJID, "approved-id");
			Assert.IsNotNull(State);
			Assert.IsTrue(State.HasPrivateKey);
			CollectionAssert.AreEqual(OldPublicKey, State.PublicKey);
			Assert.AreEqual("approved-id", await AwaitOrTimeout(this.contractsClient.GetLatestApprovedLegalId(), "GetLatestApprovedLegalId"));
			Assert.IsTrue(await AwaitOrTimeout(this.contractsClient.HasPrivateKey("approved-id"), "HasPrivateKey"));

			byte[] Signature = await AwaitOrTimeout(this.contractsClient.SignAsync(Encoding.UTF8.GetBytes("contracts-client-state-test"), SignWith.LatestApprovedId), "SignAsync");
			Assert.IsNotNull(Signature);
			Assert.IsTrue(Signature.Length > 0);
		}

		[TestMethod]
		public async Task ContractsClient_State_Test_03_GenerateNewKeysStillSucceedsWhenLegacySnapshotCannotBeBackfilled()
		{
			byte[] MatchingPublicKey = GetFirstLocalPublicKey(this.contractsClient);

			await InsertStateAsync(this.client.BareJID, "approved-id", IdentityState.Approved, MatchingPublicKey, DateTime.UtcNow);
			await RuntimeSettings.DeleteWhereKeyLikeAsync(this.contractsClient.KeySettingsPrefix + "*", "*");
			await AwaitOrTimeout(this.contractsClient.GenerateNewKeys(), "GenerateNewKeys");

			LegalIdentityState State = await GetStateAsync(this.client.BareJID, "approved-id");
			Assert.IsNotNull(State);
			Assert.IsFalse(State.HasPrivateKey);
			Assert.IsFalse(await this.contractsClient.HasPrivateKey("approved-id"));
		}

		[TestMethod]
		public async Task ContractsClient_State_Test_04_ExportKeysIncludesPersistedApprovedStateAfterRotation()
		{
			byte[] OldPublicKey = GetFirstLocalPublicKey(this.contractsClient);

			await InsertStateAsync(this.client.BareJID, "approved-id", IdentityState.Approved, OldPublicKey, DateTime.UtcNow);
			await AwaitOrTimeout(this.contractsClient.GenerateNewKeys(), "GenerateNewKeys");

			string Xml = await this.contractsClient.ExportKeys();

			StringAssert.Contains(Xml, "approved-id");
			StringAssert.Contains(Xml, "privateKey=");
			StringAssert.Contains(Xml, "keyName=");
			StringAssert.Contains(Xml, Convert.ToBase64String(OldPublicKey));
		}

		[TestMethod]
		public async Task ContractsClient_State_Test_05_LatestApprovedSelectionIgnoresStaleStatesWithoutSnapshots()
		{
			byte[] MatchingPublicKey = GetFirstLocalPublicKey(this.contractsClient);
			byte[] StalePublicKey = CreateStalePublicKey(MatchingPublicKey);

			await InsertStateAsync(this.client.BareJID, "matching-id", IdentityState.Approved, MatchingPublicKey, DateTime.UtcNow.AddMinutes(-1));
			await InsertStateAsync(this.client.BareJID, "stale-id", IdentityState.Approved, StalePublicKey, DateTime.UtcNow);

			string LatestApprovedLegalId = await AwaitOrTimeout(this.contractsClient.GetLatestApprovedLegalId(), "GetLatestApprovedLegalId");
			string LatestApprovedMatchingLegalId = await AwaitOrTimeout(this.contractsClient.GetLatestApprovedLegalId(MatchingPublicKey), "GetLatestApprovedLegalId(PublicKey)");

			Assert.AreEqual("matching-id", LatestApprovedLegalId);
			Assert.AreEqual("matching-id", LatestApprovedMatchingLegalId);

			await AwaitOrTimeout(this.contractsClient.GenerateNewKeys(), "GenerateNewKeys");

			byte[] Signature = await AwaitOrTimeout(this.contractsClient.SignAsync(Encoding.UTF8.GetBytes("contracts-client-state-test"), SignWith.LatestApprovedId), "SignAsync");
			Assert.IsNotNull(Signature);
			Assert.IsTrue(Signature.Length > 0);
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

		private static bool AreEqual(byte[] A, byte[] B)
		{
			if (ReferenceEquals(A, B))
				return true;

			if (A is null || B is null || A.Length != B.Length)
				return false;

			for (int i = 0; i < A.Length; i++)
			{
				if (A[i] != B[i])
					return false;
			}

			return true;
		}

		private static async Task<LegalIdentityState> GetStateAsync(string BareJid, string LegalId)
		{
			return await Database.FindFirstIgnoreRest<LegalIdentityState>(new FilterAnd(
				new FilterFieldEqualTo("BareJid", BareJid),
				new FilterFieldEqualTo("LegalId", LegalId)));
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

		private static async Task AwaitOrTimeout(Task OperationTask, string Operation)
		{
			if (await Task.WhenAny(OperationTask, Task.Delay(TimeSpan.FromSeconds(5))) != OperationTask)
				Assert.Fail(Operation + " timed out.");

			await OperationTask;
		}

		private static async Task<T> AwaitOrTimeout<T>(Task<T> OperationTask, string Operation)
		{
			if (await Task.WhenAny(OperationTask, Task.Delay(TimeSpan.FromSeconds(5))) != OperationTask)
				Assert.Fail(Operation + " timed out.");

			return await OperationTask;
		}
	}
}
