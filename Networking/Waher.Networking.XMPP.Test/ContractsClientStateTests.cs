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
using Waher.Networking.XMPP.P2P.SymmetricCiphers;
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

		[TestMethod]
		public async Task ContractsClient_State_Test_15_SnapshotUsesPrivateKeyThatMatchesEndpointPublicKeyAfterRotation()
		{
			IE2eEndpoint OriginalEndpoint = GetFirstLocalEndpoint(this.contractsClient);
			byte[] OriginalPublicKey = (byte[])OriginalEndpoint.PublicKey.Clone();

			await AwaitOrTimeout(this.contractsClient.GenerateNewKeys(), "GenerateNewKeys");

			byte[] RotatedRuntimePrivateKey = await GetRuntimePrivateKeyAsync(this.contractsClient, OriginalEndpoint.LocalName);
			IE2eEndpoint RotatedRuntimeEndpoint = CreatePrivateEndpoint(OriginalEndpoint.LocalName, OriginalEndpoint.Namespace, RotatedRuntimePrivateKey);

			try
			{
				Assert.IsFalse(AreEqual(OriginalPublicKey, RotatedRuntimeEndpoint.PublicKey));

				LegalIdentityState State = new LegalIdentityState()
				{
					BareJid = this.client.BareJID,
					LegalId = "snapshot-regression-id",
					PublicKey = (byte[])OriginalPublicKey.Clone(),
					State = IdentityState.Approved,
					Timestamp = DateTime.UtcNow
				};

				Assert.IsTrue(await InvokeSetLegalIdentityKeySnapshotAsync(this.contractsClient, State, OriginalEndpoint));
				Assert.IsTrue(State.HasPrivateKey);

				IE2eEndpoint SnapshotEndpoint = CreatePrivateEndpoint(State.KeyName, State.KeyNamespace, State.PrivateKey);

				try
				{
					CollectionAssert.AreEqual(OriginalPublicKey, State.PublicKey);
					CollectionAssert.AreEqual(OriginalPublicKey, SnapshotEndpoint.PublicKey);
				}
				finally
				{
					SnapshotEndpoint.Dispose();
				}
			}
			finally
			{
				RotatedRuntimeEndpoint.Dispose();
			}
		}

		[TestMethod]
		public async Task ContractsClient_State_Test_16_GenerateNewKeysRefreshesActiveKeysWhenSharedWithE2e()
		{
			IE2eEndpoint OriginalEndpoint = GetLocalEndpoint(this.contractsClient, "ed448");
			byte[] OriginalPublicKey = (byte[])OriginalEndpoint.PublicKey.Clone();

			await this.contractsClient.EnableE2eEncryption(true, false);
			await AwaitOrTimeout(this.contractsClient.GenerateNewKeys(), "GenerateNewKeys");

			IE2eEndpoint RotatedEndpoint = GetLocalEndpoint(this.contractsClient, "ed448");
			byte[] RotatedPublicKey = (byte[])RotatedEndpoint.PublicKey.Clone();
			byte[] RotatedRuntimePrivateKey = await GetRuntimePrivateKeyAsync(this.contractsClient, "ed448");
			IE2eEndpoint RotatedRuntimeEndpoint = CreatePrivateEndpoint("ed448", EndpointSecurity.IoTHarmonizationE2ECurrent, RotatedRuntimePrivateKey);

			try
			{
				Assert.IsFalse(AreEqual(OriginalPublicKey, RotatedPublicKey));
				CollectionAssert.AreEqual(RotatedRuntimeEndpoint.PublicKey, RotatedPublicKey);
			}
			finally
			{
				RotatedRuntimeEndpoint.Dispose();
			}
		}

		[TestMethod]
		public async Task ContractsClient_State_Test_17_SaveContractSharedSecretPersistsContractState()
		{
			byte[] SharedSecret = new byte[] { 1, 2, 3, 4, 5, 6 };

			Assert.IsTrue(await InvokeSaveContractSharedSecretAsync(this.contractsClient, "contract-state-id", this.client.BareJID,
				SharedSecret, SymmetricCipherAlgorithms.Aes256, false));

			ContractSharedSecretState State = await GetContractStateAsync(this.client.BareJID, "contract-state-id");
			Assert.IsNotNull(State);
			Assert.AreEqual(this.client.BareJID, State.CreatorJid);
			Assert.AreEqual(SymmetricCipherAlgorithms.Aes256, State.KeyAlgorithm);
			CollectionAssert.AreEqual(SharedSecret, State.SharedSecret);
			Assert.AreEqual(string.Empty, await RuntimeSettings.GetAsync(
				this.contractsClient.ContractKeySettingsPrefix + "contract-state-id", string.Empty));

			Tuple<SymmetricCipherAlgorithms, string, byte[]> Secret = await InvokeTryLoadContractSharedSecretAsync(this.contractsClient, "contract-state-id");
			Assert.IsNotNull(Secret);
			Assert.AreEqual(SymmetricCipherAlgorithms.Aes256, Secret.Item1);
			Assert.AreEqual(this.client.BareJID, Secret.Item2);
			CollectionAssert.AreEqual(SharedSecret, Secret.Item3);
		}

		[TestMethod]
		public async Task ContractsClient_State_Test_18_LegacyContractSharedSecretBackfillsContractStateOnLoad()
		{
			byte[] SharedSecret = new byte[] { 9, 8, 7, 6, 5, 4 };
			await RuntimeSettings.SetAsync(this.contractsClient.ContractKeySettingsPrefix + "legacy-contract-id",
				EncodeLegacyContractSharedSecret(SymmetricCipherAlgorithms.AeadChaCha20Poly1305, "creator@example.org", SharedSecret));

			Tuple<SymmetricCipherAlgorithms, string, byte[]> Secret = await InvokeTryLoadContractSharedSecretAsync(this.contractsClient, "legacy-contract-id");
			Assert.IsNotNull(Secret);
			Assert.AreEqual(SymmetricCipherAlgorithms.AeadChaCha20Poly1305, Secret.Item1);
			Assert.AreEqual("creator@example.org", Secret.Item2);
			CollectionAssert.AreEqual(SharedSecret, Secret.Item3);

			ContractSharedSecretState State = await GetContractStateAsync(this.client.BareJID, "legacy-contract-id");
			Assert.IsNotNull(State);
			Assert.AreEqual("creator@example.org", State.CreatorJid);
			Assert.AreEqual(SymmetricCipherAlgorithms.AeadChaCha20Poly1305, State.KeyAlgorithm);
			CollectionAssert.AreEqual(SharedSecret, State.SharedSecret);
		}

		[TestMethod]
		public async Task ContractsClient_State_Test_19_SaveContractSharedSecretOnlyIfNewRespectsLegacyRuntimeValue()
		{
			byte[] ExistingSharedSecret = new byte[] { 3, 3, 3, 3 };
			byte[] NewSharedSecret = new byte[] { 4, 4, 4, 4 };

			await RuntimeSettings.SetAsync(this.contractsClient.ContractKeySettingsPrefix + "existing-contract-id",
				EncodeLegacyContractSharedSecret(SymmetricCipherAlgorithms.Aes256, "creator@example.org", ExistingSharedSecret));

			Assert.IsFalse(await InvokeSaveContractSharedSecretAsync(this.contractsClient, "existing-contract-id", this.client.BareJID,
				NewSharedSecret, SymmetricCipherAlgorithms.AeadChaCha20Poly1305, true));

			ContractSharedSecretState State = await GetContractStateAsync(this.client.BareJID, "existing-contract-id");
			Assert.IsNotNull(State);
			Assert.AreEqual("creator@example.org", State.CreatorJid);
			Assert.AreEqual(SymmetricCipherAlgorithms.Aes256, State.KeyAlgorithm);
			CollectionAssert.AreEqual(ExistingSharedSecret, State.SharedSecret);
		}

		[TestMethod]
		public async Task ContractsClient_State_Test_20_ExportKeysMigratesLegacyContractSecretAndUsesContractStateFormat()
		{
			byte[] PersistedSecret = new byte[] { 5, 4, 3, 2 };
			byte[] LegacySecret = new byte[] { 2, 3, 4, 5 };

			Assert.IsTrue(await InvokeSaveContractSharedSecretAsync(this.contractsClient, "persisted-contract-id", this.client.BareJID,
				PersistedSecret, SymmetricCipherAlgorithms.Aes256, false));
			await RuntimeSettings.SetAsync(this.contractsClient.ContractKeySettingsPrefix + "legacy-contract-id",
				EncodeLegacyContractSharedSecret(SymmetricCipherAlgorithms.AeadChaCha20Poly1305, "creator@example.org", LegacySecret));

			string Xml = await this.contractsClient.ExportKeys();

			Assert.IsTrue(Xml.Contains("<C ", StringComparison.Ordinal));
			Assert.IsTrue(Xml.Contains("persisted-contract-id", StringComparison.Ordinal));
			Assert.IsTrue(Xml.Contains("legacy-contract-id", StringComparison.Ordinal));
			Assert.IsFalse(Xml.Contains("ContractState", StringComparison.Ordinal));

			ContractSharedSecretState State = await GetContractStateAsync(this.client.BareJID, "legacy-contract-id");
			Assert.IsNotNull(State);
			Assert.AreEqual("creator@example.org", State.CreatorJid);
			CollectionAssert.AreEqual(LegacySecret, State.SharedSecret);
		}

		[TestMethod]
		public async Task ContractsClient_State_Test_21_ImportKeysRoundTripWithContractStateRestoresSharedSecret()
		{
			byte[] SharedSecret = new byte[] { 7, 7, 7, 7, 7 };

			Assert.IsTrue(await InvokeSaveContractSharedSecretAsync(this.contractsClient, "roundtrip-contract-id", this.client.BareJID,
				SharedSecret, SymmetricCipherAlgorithms.Aes256, false));

			string Xml = await this.contractsClient.ExportKeys();

			await DeleteAllContractStatesAsync(this.client.BareJID);
			await RuntimeSettings.DeleteWhereKeyLikeAsync(this.contractsClient.ContractKeySettingsPrefix + "*", "*");

			Assert.IsTrue(await this.contractsClient.ImportKeys(Xml));

			Tuple<SymmetricCipherAlgorithms, string, byte[]> Secret = await InvokeTryLoadContractSharedSecretAsync(this.contractsClient, "roundtrip-contract-id");
			Assert.IsNotNull(Secret);
			Assert.AreEqual(SymmetricCipherAlgorithms.Aes256, Secret.Item1);
			Assert.AreEqual(this.client.BareJID, Secret.Item2);
			CollectionAssert.AreEqual(SharedSecret, Secret.Item3);
		}

		[TestMethod]
		public async Task ContractsClient_State_Test_22_ImportKeysBackwardCompatibilityRestoresLegacyContractSharedSecret()
		{
			byte[] SharedSecret = new byte[] { 6, 6, 6, 6 };
			string Xml = "<LegalId xmlns=\"" + ContractsClient.NamespaceOnboarding + "\"><C n=\"legacy-import-contract-id\" v=\"" +
				EncodeLegacyContractSharedSecret(SymmetricCipherAlgorithms.AeadChaCha20Poly1305, "creator@example.org", SharedSecret) +
				"\" /></LegalId>";

			Assert.IsTrue(await this.contractsClient.ImportKeys(Xml));

			ContractSharedSecretState State = await GetContractStateAsync(this.client.BareJID, "legacy-import-contract-id");
			Assert.IsNotNull(State);
			Assert.AreEqual("creator@example.org", State.CreatorJid);
			Assert.AreEqual(SymmetricCipherAlgorithms.AeadChaCha20Poly1305, State.KeyAlgorithm);
			CollectionAssert.AreEqual(SharedSecret, State.SharedSecret);
		}

		[TestMethod]
		public async Task ContractsClient_State_Test_23_MalformedLegacyContractSharedSecretIsIgnored()
		{
			await RuntimeSettings.SetAsync(this.contractsClient.ContractKeySettingsPrefix + "malformed-contract-id", "not|valid");

			Assert.IsNull(await InvokeTryLoadContractSharedSecretAsync(this.contractsClient, "malformed-contract-id"));
			Assert.IsNull(await GetContractStateAsync(this.client.BareJID, "malformed-contract-id"));
		}

		[TestMethod]
		public async Task ContractsClient_State_Test_24_ContractStateSharedSecretIsProtectedByCallStack()
		{
			FieldInfo ApprovedSources = typeof(ContractSharedSecretState).GetField("approvedSources", BindingFlags.Static | BindingFlags.NonPublic);
			ApprovedSources.SetValue(null, null);

			try
			{
				ContractSharedSecretState State = new ContractSharedSecretState()
				{
					ContractId = "protected-contract-id"
				};

				State.SharedSecret = new byte[] { 1, 2, 3 };
				ContractSharedSecretState.SetAllowedSources(new CallStack.ICallStackCheck[]
				{
					new CallStack.ApproveType(typeof(ContractsClient))
				});

				Assert.ThrowsException<CallStack.UnauthorizedCallstackException>(() =>
				{
					byte[] Bin = State.SharedSecret;
				});
			}
			finally
			{
				ApprovedSources.SetValue(null, null);
			}
		}

		private static byte[] GetFirstLocalPublicKey(ContractsClient ContractsClient)
		{
			IE2eEndpoint[] Keys = GetLocalKeys(ContractsClient);

			Assert.IsNotNull(Keys);
			Assert.IsTrue(Keys.Length > 0);

			return (byte[])Keys[0].PublicKey.Clone();
		}

		private static IE2eEndpoint GetFirstLocalEndpoint(ContractsClient ContractsClient)
		{
			IE2eEndpoint[] Keys = GetLocalKeys(ContractsClient);

			Assert.IsNotNull(Keys);
			Assert.IsTrue(Keys.Length > 0);

			return Keys[0];
		}

		private static IE2eEndpoint GetLocalEndpoint(ContractsClient ContractsClient, string LocalName)
		{
			IE2eEndpoint[] Keys = GetLocalKeys(ContractsClient);

			foreach (IE2eEndpoint Key in Keys)
			{
				if (Key.LocalName == LocalName)
					return Key;
			}

			Assert.Fail("Local endpoint not found: " + LocalName);
			return null;
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

		private static async Task<ContractSharedSecretState> GetContractStateAsync(string BareJid, string ContractId)
		{
			return await Database.FindFirstIgnoreRest<ContractSharedSecretState>(new FilterAnd(
				new FilterFieldEqualTo("BareJid", BareJid),
				new FilterFieldEqualTo("ContractId", ContractId)));
		}

		private static async Task<List<ContractSharedSecretState>> GetContractStatesAsync(string BareJid)
		{
			return (await Database.Find<ContractSharedSecretState>(new FilterFieldEqualTo("BareJid", BareJid))).ToList();
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

		private static async Task DeleteAllContractStatesAsync(string BareJid)
		{
			foreach (ContractSharedSecretState State in await GetContractStatesAsync(BareJid))
				await Database.Delete(State);
		}

		private static string EncodeLegacyContractSharedSecret(SymmetricCipherAlgorithms Algorithm, string CreatorJid, byte[] SharedSecret)
		{
			return Algorithm.ToString() + "|" + CreatorJid + "|" + Convert.ToBase64String(SharedSecret);
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

		private static async Task<bool> InvokeSetLegalIdentityKeySnapshotAsync(ContractsClient ContractsClient, LegalIdentityState State, IE2eEndpoint Endpoint)
		{
			MethodInfo Method = typeof(ContractsClient).GetMethod("SetLegalIdentityKeySnapshotAsync", BindingFlags.Instance | BindingFlags.NonPublic);
			Task<bool> Task = (Task<bool>)Method.Invoke(ContractsClient, new object[] { State, Endpoint });
			return await Task;
		}

		private static async Task<bool> InvokeSaveContractSharedSecretAsync(ContractsClient ContractsClient, string ContractId,
			string CreatorJid, byte[] SharedSecret, SymmetricCipherAlgorithms Algorithm, bool OnlyIfNew)
		{
			MethodInfo Method = typeof(ContractsClient).GetMethod("SaveContractSharedSecret", BindingFlags.Instance | BindingFlags.NonPublic, null,
				new Type[] { typeof(string), typeof(string), typeof(byte[]), typeof(SymmetricCipherAlgorithms), typeof(bool) }, null);
			Task<bool> Task = (Task<bool>)Method.Invoke(ContractsClient, new object[] { ContractId, CreatorJid, SharedSecret, Algorithm, OnlyIfNew });
			return await Task;
		}

		private static async Task<Tuple<SymmetricCipherAlgorithms, string, byte[]>> InvokeTryLoadContractSharedSecretAsync(
			ContractsClient ContractsClient, string ContractId)
		{
			MethodInfo Method = typeof(ContractsClient).GetMethod("TryLoadContractSharedSecret", BindingFlags.Instance | BindingFlags.NonPublic, null,
				new Type[] { typeof(string) }, null);
			Task<Tuple<SymmetricCipherAlgorithms, string, byte[]>> Task =
				(Task<Tuple<SymmetricCipherAlgorithms, string, byte[]>>)Method.Invoke(ContractsClient, new object[] { ContractId });
			return await Task;
		}

		private static IE2eEndpoint CreatePrivateEndpoint(string KeyName, string KeyNamespace, byte[] PrivateKey)
		{
			Assert.IsTrue(EndpointSecurity.TryCreateEndpoint(KeyName, KeyNamespace, out IE2eEndpoint Template));

			try
			{
				return Template.CreatePrivate(PrivateKey);
			}
			finally
			{
				Template.Dispose();
			}
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
