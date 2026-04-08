using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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
using CallStack = Waher.Security.CallStack;

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

		[TestMethod]
		public async Task ContractsClient_State_Test_06_ImportKeysRoundTripWithStateSnapshotsRestoresApprovedIdentity()
		{
			byte[] OriginalPublicKey = GetFirstLocalPublicKey(this.contractsClient);

			await InsertStateAsync(this.client.BareJID, "approved-id", IdentityState.Approved, OriginalPublicKey, DateTime.UtcNow);
			Assert.IsTrue(await this.contractsClient.HasPrivateKey("approved-id"));

			string Xml = ExtractExportedStateXml(await this.contractsClient.ExportKeys(), RemovePrivateStateAttributes: false, KeepRuntimeSettings: false);

			await AwaitOrTimeout(this.contractsClient.GenerateNewKeys(), "GenerateNewKeys");
			await DeleteAllStatesAsync(this.client.BareJID);
			await RuntimeSettings.DeleteWhereKeyLikeAsync(this.contractsClient.KeySettingsPrefix + "*", "*");

			Assert.IsTrue(await this.contractsClient.ImportKeys(Xml));
			Assert.AreEqual("approved-id", await AwaitOrTimeout(this.contractsClient.GetLatestApprovedLegalId(), "GetLatestApprovedLegalId"));
			Assert.IsTrue(await AwaitOrTimeout(this.contractsClient.HasPrivateKey("approved-id"), "HasPrivateKey"));

			byte[] Signature = await AwaitOrTimeout(this.contractsClient.SignAsync(Encoding.UTF8.GetBytes("roundtrip"), SignWith.LatestApprovedId), "SignAsync");
			Assert.IsTrue(Signature.Length > 0);
		}

		[TestMethod]
		public async Task ContractsClient_State_Test_07_ImportKeysBackwardCompatibilityBackfillsSnapshotFromRuntimeSettings()
		{
			byte[] OriginalPublicKey = GetFirstLocalPublicKey(this.contractsClient);

			await InsertStateAsync(this.client.BareJID, "approved-id", IdentityState.Approved, OriginalPublicKey, DateTime.UtcNow);
			Assert.IsTrue(await this.contractsClient.HasPrivateKey("approved-id"));

			string Xml = ExtractExportedStateXml(await this.contractsClient.ExportKeys(), RemovePrivateStateAttributes: true, KeepRuntimeSettings: true);

			await DeleteAllStatesAsync(this.client.BareJID);
			await RuntimeSettings.DeleteWhereKeyLikeAsync(this.contractsClient.KeySettingsPrefix + "*", "*");

			Assert.IsTrue(await this.contractsClient.ImportKeys(Xml));

			LegalIdentityState State = await GetStateAsync(this.client.BareJID, "approved-id");
			Assert.IsNotNull(State);
			Assert.IsFalse(State.HasPrivateKey);

			Assert.IsTrue(await AwaitOrTimeout(this.contractsClient.HasPrivateKey("approved-id"), "HasPrivateKey"));

			State = await GetStateAsync(this.client.BareJID, "approved-id");
			Assert.IsTrue(State.HasPrivateKey);
			Assert.IsFalse(string.IsNullOrEmpty(State.KeyName));
		}

		[TestMethod]
		public async Task ContractsClient_State_Test_08_SnapshotOnlyApprovedStateWorksAfterRuntimeKeysAreDeleted()
		{
			byte[] OriginalPublicKey = GetFirstLocalPublicKey(this.contractsClient);

			await InsertStateAsync(this.client.BareJID, "approved-id", IdentityState.Approved, OriginalPublicKey, DateTime.UtcNow);
			Assert.IsTrue(await this.contractsClient.HasPrivateKey("approved-id"));

			await RuntimeSettings.DeleteWhereKeyLikeAsync(this.contractsClient.KeySettingsPrefix + "*", "*");
			Assert.IsTrue(await this.contractsClient.LoadKeys(true));

			byte[] NewPublicKey = GetFirstLocalPublicKey(this.contractsClient);
			Assert.IsFalse(AreEqual(OriginalPublicKey, NewPublicKey));

			Assert.AreEqual("approved-id", await AwaitOrTimeout(this.contractsClient.GetLatestApprovedLegalId(), "GetLatestApprovedLegalId"));
			Assert.IsTrue(await AwaitOrTimeout(this.contractsClient.HasPrivateKey("approved-id"), "HasPrivateKey"));

			byte[] Signature = await AwaitOrTimeout(this.contractsClient.SignAsync(Encoding.UTF8.GetBytes("snapshot-only"), SignWith.LatestApprovedId), "SignAsync");
			Assert.IsTrue(Signature.Length > 0);
		}

		[TestMethod]
		public async Task ContractsClient_State_Test_09_UpdateSettingsReusesPreviewStateInsteadOfCreatingDuplicate()
		{
			byte[] PublicKey = GetFirstLocalPublicKey(this.contractsClient);

			await InsertStateAsync(this.client.BareJID, "preview-id", IdentityState.Created, PublicKey, DateTime.UtcNow.AddMinutes(-1));

			LegalIdentity Identity = new LegalIdentity()
			{
				Id = "real-id",
				State = IdentityState.Approved,
				Created = DateTime.UtcNow.AddMinutes(-1),
				Updated = DateTime.UtcNow
			};

			await InvokeUpdateSettingsAsync(this.contractsClient, Identity, PublicKey);

			List<LegalIdentityState> States = await GetStatesAsync(this.client.BareJID);
			Assert.AreEqual(1, States.Count);
			Assert.AreEqual("real-id", (string)States[0].LegalId);
			Assert.AreEqual(IdentityState.Approved, States[0].State);
		}

		[TestMethod]
		public async Task ContractsClient_State_Test_10_MismatchedSnapshotIsIgnored()
		{
			IE2eEndpoint[] Keys = GetLocalKeys(this.contractsClient);
			Assert.IsTrue(Keys.Length >= 2);

			await InsertStateAsync(this.client.BareJID, "approved-id", IdentityState.Approved, Keys[0].PublicKey, DateTime.UtcNow);
			LegalIdentityState State = await GetStateAsync(this.client.BareJID, "approved-id");
			State.KeyName = Keys[1].LocalName;
			State.KeyNamespace = Keys[1].Namespace;
			State.PrivateKey = await GetRuntimePrivateKeyAsync(this.contractsClient, Keys[1].LocalName);
			await Database.Update(State);

			Assert.IsNull(await AwaitOrTimeout(this.contractsClient.GetLatestApprovedLegalId(), "GetLatestApprovedLegalId"));
			Assert.IsFalse(await AwaitOrTimeout(this.contractsClient.HasPrivateKey("approved-id"), "HasPrivateKey"));
		}

		[TestMethod]
		public async Task ContractsClient_State_Test_11_NewerUnusableApprovedStateIsSkippedInFavorOfOlderUsableState()
		{
			IE2eEndpoint[] Keys = GetLocalKeys(this.contractsClient);
			Assert.IsTrue(Keys.Length >= 2);

			await InsertStateAsync(this.client.BareJID, "usable-id", IdentityState.Approved, Keys[0].PublicKey, DateTime.UtcNow.AddMinutes(-1));
			Assert.IsTrue(await this.contractsClient.HasPrivateKey("usable-id"));

			await InsertStateAsync(this.client.BareJID, "newer-unusable-id", IdentityState.Approved, Keys[0].PublicKey, DateTime.UtcNow);
			LegalIdentityState Unusable = await GetStateAsync(this.client.BareJID, "newer-unusable-id");
			Unusable.KeyName = Keys[1].LocalName;
			Unusable.KeyNamespace = Keys[1].Namespace;
			Unusable.PrivateKey = await GetRuntimePrivateKeyAsync(this.contractsClient, Keys[1].LocalName);
			await Database.Update(Unusable);

			Assert.AreEqual("usable-id", await AwaitOrTimeout(this.contractsClient.GetLatestApprovedLegalId(), "GetLatestApprovedLegalId"));
		}

		[TestMethod]
		public async Task ContractsClient_State_Test_12_GenerateNewKeysPreservesExistingSnapshot()
		{
			byte[] OriginalPublicKey = GetFirstLocalPublicKey(this.contractsClient);

			await InsertStateAsync(this.client.BareJID, "approved-id", IdentityState.Approved, OriginalPublicKey, DateTime.UtcNow);
			Assert.IsTrue(await this.contractsClient.HasPrivateKey("approved-id"));

			LegalIdentityState Before = await GetStateAsync(this.client.BareJID, "approved-id");
			string PrivateKeyBefore = Convert.ToBase64String(Before.PrivateKey);
			string KeyNameBefore = Before.KeyName;
			string KeyNamespaceBefore = Before.KeyNamespace;

			await AwaitOrTimeout(this.contractsClient.GenerateNewKeys(), "GenerateNewKeys");

			LegalIdentityState After = await GetStateAsync(this.client.BareJID, "approved-id");
			Assert.AreEqual(KeyNameBefore, After.KeyName);
			Assert.AreEqual(KeyNamespaceBefore, After.KeyNamespace);
			Assert.AreEqual(PrivateKeyBefore, Convert.ToBase64String(After.PrivateKey));
		}

		[TestMethod]
		public async Task ContractsClient_State_Test_13_LegalIdentityStatePrivateKeyIsProtectedByCallStack()
		{
			FieldInfo ApprovedSources = typeof(LegalIdentityState).GetField("approvedSources", BindingFlags.Static | BindingFlags.NonPublic);
			ApprovedSources.SetValue(null, null);

			try
			{
				LegalIdentityState State = new LegalIdentityState()
				{
					LegalId = "protected-id"
				};

				State.PrivateKey = new byte[] { 1, 2, 3 };
				LegalIdentityState.SetAllowedSources(new CallStack.ICallStackCheck[]
				{
					new CallStack.ApproveType(typeof(ContractsClient))
				});

				Assert.ThrowsException<CallStack.UnauthorizedCallstackException>(() =>
				{
					byte[] Bin = State.PrivateKey;
				});
			}
			finally
			{
				ApprovedSources.SetValue(null, null);
			}
		}

		[TestMethod]
		public async Task ContractsClient_State_Test_14_ExportKeysSkipsMalformedSnapshotStates()
		{
			IE2eEndpoint[] Keys = GetLocalKeys(this.contractsClient);

			await InsertStateAsync(this.client.BareJID, "malformed-id", IdentityState.Approved, Keys[0].PublicKey, DateTime.UtcNow);
			await AwaitOrTimeout(this.contractsClient.GenerateNewKeys(), "GenerateNewKeys");

			LegalIdentityState State = await GetStateAsync(this.client.BareJID, "malformed-id");
			State.KeyName = Keys[0].LocalName;
			State.KeyNamespace = Keys[0].Namespace;
			State.PrivateKey = new byte[] { 1, 2, 3, 4 };
			await Database.Update(State);

			string Xml = await this.contractsClient.ExportKeys();
			Assert.IsFalse(Xml.Contains("malformed-id", StringComparison.Ordinal));
		}

		private static byte[] GetFirstLocalPublicKey(ContractsClient ContractsClient)
		{
			IE2eEndpoint[] Keys = GetLocalKeys(ContractsClient);

			Assert.IsNotNull(Keys);
			Assert.IsTrue(Keys.Length > 0);

			return (byte[])Keys[0].PublicKey.Clone();
		}

		private static IE2eEndpoint[] GetLocalKeys(ContractsClient ContractsClient)
		{
			FieldInfo LocalKeysField = typeof(ContractsClient).GetField("localKeys", BindingFlags.Instance | BindingFlags.NonPublic);
			EndpointSecurity LocalKeys = (EndpointSecurity)LocalKeysField.GetValue(ContractsClient);

			PropertyInfo KeysProperty = typeof(EndpointSecurity).GetProperty("Keys", BindingFlags.Instance | BindingFlags.NonPublic);
			return (IE2eEndpoint[])KeysProperty.GetValue(LocalKeys);
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

		private static async Task<List<LegalIdentityState>> GetStatesAsync(string BareJid)
		{
			return (await Database.Find<LegalIdentityState>(new FilterFieldEqualTo("BareJid", BareJid))).ToList();
		}

		private static async Task DeleteAllStatesAsync(string BareJid)
		{
			foreach (LegalIdentityState State in await GetStatesAsync(BareJid))
			{
				await Database.Delete(State);

				if (!string.IsNullOrEmpty(State.LegalId))
					Types.UnregisterSingleton(State, State.LegalId);
			}
		}

		private static string ExtractExportedStateXml(string Xml, bool RemovePrivateStateAttributes, bool KeepRuntimeSettings)
		{
			XmlDocument Doc = new XmlDocument();
			Doc.LoadXml(Xml);

			List<XmlNode> ToRemove = new List<XmlNode>();

			foreach (XmlNode N in Doc.DocumentElement.ChildNodes)
			{
				if (N is not XmlElement E)
					continue;

				if (E.LocalName == "State")
				{
					if (RemovePrivateStateAttributes)
					{
						E.RemoveAttribute("privateKey");
						E.RemoveAttribute("keyName");
						E.RemoveAttribute("keyNamespace");
						E.RemoveAttribute("hasPrivateKey");
					}

					continue;
				}

				if (KeepRuntimeSettings && (E.LocalName == "S" || E.LocalName == "DT"))
					continue;

				ToRemove.Add(E);
			}

			foreach (XmlNode N in ToRemove)
				Doc.DocumentElement.RemoveChild(N);

			return Doc.OuterXml;
		}

		private static async Task<byte[]> GetRuntimePrivateKeyAsync(ContractsClient ContractsClient, string KeyName)
		{
			string Value = await RuntimeSettings.GetAsync(ContractsClient.KeySettingsPrefix + KeyName, string.Empty);
			Assert.IsFalse(string.IsNullOrEmpty(Value));

			return Convert.FromBase64String(Value);
		}

		private static async Task InvokeUpdateSettingsAsync(ContractsClient ContractsClient, LegalIdentity Identity, byte[] PublicKey)
		{
			MethodInfo Method = typeof(ContractsClient).GetMethod("UpdateSettings", BindingFlags.Instance | BindingFlags.NonPublic, null,
				new Type[] { typeof(LegalIdentity), typeof(byte[]) }, null);

			Task Task = (Task)Method.Invoke(ContractsClient, new object[] { Identity, PublicKey });
			await Task;
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
