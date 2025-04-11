using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Content.Xsl;
using Waher.Events;
using Waher.Events.XMPP;
using Waher.Networking.XMPP.Contracts.EventArguments;
using Waher.Networking.XMPP.Contracts.HumanReadable;
using Waher.Networking.XMPP.Contracts.Search;
using Waher.Networking.XMPP.Events;
using Waher.Networking.XMPP.HttpFileUpload;
using Waher.Networking.XMPP.P2P;
using Waher.Networking.XMPP.P2P.E2E;
using Waher.Networking.XMPP.P2P.SymmetricCiphers;
using Waher.Networking.XMPP.StanzaErrors;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Runtime.Cache;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;
using Waher.Runtime.Profiling;
using Waher.Runtime.Settings;
using Waher.Runtime.Temporary;
using Waher.Script;
using Waher.Security;
using Waher.Security.CallStack;
using Waher.Security.EllipticCurves;
using static System.Net.Mime.MediaTypeNames;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Adds support for legal identities, smart contracts and signatures to an XMPP client.
	/// 
	/// The interface is defined in the Neuro-Foundation XMPP IoT extensions:
	/// https://neuro-foundation.io
	/// </summary>
	public class ContractsClient : XmppExtension
	{
		/// <summary>
		/// urn:ieee:iot:leg:id:1.0
		/// </summary>
		public const string NamespaceLegalIdentitiesIeeeV1 = "urn:ieee:iot:leg:id:1.0";

		/// <summary>
		/// urn:nf:iot:leg:id:1.0
		/// </summary>
		public const string NamespaceLegalIdentitiesNeuroFoundationV1 = "urn:nf:iot:leg:id:1.0";

		/// <summary>
		/// Current namespace for legal identities.
		/// </summary>
		public const string NamespaceLegalIdentitiesCurrent = NamespaceLegalIdentitiesNeuroFoundationV1;

		/// <summary>
		/// Default cipher name for encrypted parameters, if an algorithm is not explicitly defined.
		/// </summary>
		public const SymmetricCipherAlgorithms DefaultCipherAlgorithm = SymmetricCipherAlgorithms.Aes256;

		/// <summary>
		/// Namespaces supported for legal identities.
		/// </summary>
		public static readonly string[] NamespacesLegalIdentities = new string[]
		{
			NamespaceLegalIdentitiesIeeeV1,
			NamespaceLegalIdentitiesNeuroFoundationV1
		};

		/// <summary>
		/// urn:ieee:iot:leg:sc:1.0
		/// </summary>
		public const string NamespaceSmartContractsIeeeV1 = "urn:ieee:iot:leg:sc:1.0";

		/// <summary>
		/// urn:nf:iot:leg:sc:1.0
		/// </summary>
		public const string NamespaceSmartContractsNeuroFoundationV1 = "urn:nf:iot:leg:sc:1.0";

		/// <summary>
		/// Current namespce for smart contracts.
		/// </summary>
		public const string NamespaceSmartContractsCurrent = NamespaceSmartContractsNeuroFoundationV1;

		/// <summary>
		/// Namespaces supported for smart contracts.
		/// </summary>
		public static readonly string[] NamespacesSmartContracts = new string[]
		{
			NamespaceSmartContractsIeeeV1,
			NamespaceSmartContractsNeuroFoundationV1
		};

		/// <summary>
		/// http://waher.se/schema/Onboarding/v1.xsd
		/// </summary>
		public const string NamespaceOnboarding = "http://waher.se/schema/Onboarding/v1.xsd";

		private static readonly string KeySettings = typeof(ContractsClient).FullName + ".";
		private static readonly string ContractKeySettings = typeof(ContractsClient).Namespace + ".Contracts.";

		private readonly Dictionary<string, KeyEventArgs> publicKeys = new Dictionary<string, KeyEventArgs>();
		private readonly Dictionary<string, KeyEventArgs> matchingKeys = new Dictionary<string, KeyEventArgs>();
		private readonly Cache<string, KeyValuePair<byte[], bool>> contentPerPid = new Cache<string, KeyValuePair<byte[], bool>>(int.MaxValue, TimeSpan.FromDays(1), TimeSpan.FromDays(1));
		private EndpointSecurity localKeys;
		private EndpointSecurity localE2eEndpoint;
		private DateTime keysTimestamp = DateTime.MinValue;
		private SymmetricCipherAlgorithms preferredEncryptionAlgorithm = DefaultCipherAlgorithm;
		private object[] approvedSources = null;
		private readonly string componentAddress;
		private string keySettingsPrefix;
		private string contractKeySettingsPrefix;
		private bool keySettingsPrefixLocked = false;
		private bool localKeysForE2e = false;
		private bool preferredEncryptionAlgorithmLocked = false;
		private RandomNumberGenerator rnd = RandomNumberGenerator.Create();
		private Aes aes;

		#region Construction

		/// <summary>
		/// Adds support for legal identities, smart contracts and signatures to an XMPP client.
		/// 
		/// The interface is defined in the Neuro-Foundation XMPP IoT extensions:
		/// https://neuro-foundation.io
		/// 
		/// Before the Contracts Client can be used, you either need to load previously stored
		/// keys using <see cref="LoadKeys(bool)"/>, or generate new keys, calling
		/// <see cref="GenerateNewKeys"/>.
		/// </summary>
		/// <param name="Client">XMPP Client to use.</param>
		/// <param name="ComponentAddress">Address to the contracts component.</param>
		public ContractsClient(XmppClient Client, string ComponentAddress)
			: this(Client, ComponentAddress, null)
		{
		}

		/// <summary>
		/// Adds support for legal identities, smart contracts and signatures to an XMPP client.
		/// 
		/// The interface is defined in the Neuro-Foundation XMPP IoT extensions:
		/// https://neuro-foundation.io
		/// 
		/// Before the Contracts Client can be used, you either need to load previously stored
		/// keys using <see cref="LoadKeys(bool)"/>, or generate new keys, calling
		/// <see cref="GenerateNewKeys"/>.
		/// </summary>
		/// <param name="Client">XMPP Client to use.</param>
		/// <param name="ComponentAddress">Address to the contracts component.</param>
		/// <param name="ApprovedSources">If access to sensitive methods is only accessible from a set of approved sources.</param>
		public ContractsClient(XmppClient Client, string ComponentAddress, object[] ApprovedSources)
			: base(Client)
		{
			this.componentAddress = ComponentAddress;
			this.approvedSources = ApprovedSources;
			this.localKeys = null;

			#region NeuroFoundation V1

			this.client.RegisterMessageHandler("identity", NamespaceLegalIdentitiesNeuroFoundationV1, this.IdentityMessageHandler, true);
			this.client.RegisterMessageHandler("petitionIdentityMsg", NamespaceLegalIdentitiesNeuroFoundationV1, this.PetitionIdentityMessageHandler, false);
			this.client.RegisterMessageHandler("petitionIdentityResponseMsg", NamespaceLegalIdentitiesNeuroFoundationV1, this.PetitionIdentityResponseMessageHandler, false);
			this.client.RegisterMessageHandler("petitionSignatureMsg", NamespaceLegalIdentitiesNeuroFoundationV1, this.PetitionSignatureMessageHandler, false);
			this.client.RegisterMessageHandler("petitionSignatureResponseMsg", NamespaceLegalIdentitiesNeuroFoundationV1, this.PetitionSignatureResponseMessageHandler, false);
			this.client.RegisterMessageHandler("petitionClientUrl", NamespaceLegalIdentitiesNeuroFoundationV1, this.PetitionClientUrlEventHandler, false);
			this.client.RegisterMessageHandler("identityReview", NamespaceLegalIdentitiesNeuroFoundationV1, this.IdentityReviewEventHandler, false);
			this.client.RegisterMessageHandler("clientMessage", NamespaceLegalIdentitiesNeuroFoundationV1, this.ClientMessageEventHandler, false);

			this.client.RegisterMessageHandler("contractSigned", NamespaceSmartContractsNeuroFoundationV1, this.ContractSignedMessageHandler, true);
			this.client.RegisterMessageHandler("contractCreated", NamespaceSmartContractsNeuroFoundationV1, this.ContractCreatedMessageHandler, false);
			this.client.RegisterMessageHandler("contractUpdated", NamespaceSmartContractsNeuroFoundationV1, this.ContractUpdatedMessageHandler, false);
			this.client.RegisterMessageHandler("contractDeleted", NamespaceSmartContractsNeuroFoundationV1, this.ContractDeletedMessageHandler, false);
			this.client.RegisterMessageHandler("petitionContractMsg", NamespaceSmartContractsNeuroFoundationV1, this.PetitionContractMessageHandler, false);
			this.client.RegisterMessageHandler("petitionContractResponseMsg", NamespaceSmartContractsNeuroFoundationV1, this.PetitionContractResponseMessageHandler, false);
			this.client.RegisterMessageHandler("contractProposal", NamespaceSmartContractsNeuroFoundationV1, this.ContractProposalMessageHandler, false);

			#endregion

			#region IEEE v1

			this.client.RegisterMessageHandler("identity", NamespaceLegalIdentitiesIeeeV1, this.IdentityMessageHandler, true);
			this.client.RegisterMessageHandler("petitionIdentityMsg", NamespaceLegalIdentitiesIeeeV1, this.PetitionIdentityMessageHandler, false);
			this.client.RegisterMessageHandler("petitionIdentityResponseMsg", NamespaceLegalIdentitiesIeeeV1, this.PetitionIdentityResponseMessageHandler, false);
			this.client.RegisterMessageHandler("petitionSignatureMsg", NamespaceLegalIdentitiesIeeeV1, this.PetitionSignatureMessageHandler, false);
			this.client.RegisterMessageHandler("petitionSignatureResponseMsg", NamespaceLegalIdentitiesIeeeV1, this.PetitionSignatureResponseMessageHandler, false);
			this.client.RegisterMessageHandler("petitionClientUrl", NamespaceLegalIdentitiesIeeeV1, this.PetitionClientUrlEventHandler, false);
			this.client.RegisterMessageHandler("identityReview", NamespaceLegalIdentitiesIeeeV1, this.IdentityReviewEventHandler, false);
			this.client.RegisterMessageHandler("clientMessage", NamespaceLegalIdentitiesIeeeV1, this.ClientMessageEventHandler, false);

			this.client.RegisterMessageHandler("contractSigned", NamespaceSmartContractsIeeeV1, this.ContractSignedMessageHandler, true);
			this.client.RegisterMessageHandler("contractCreated", NamespaceSmartContractsIeeeV1, this.ContractCreatedMessageHandler, false);
			this.client.RegisterMessageHandler("contractUpdated", NamespaceSmartContractsIeeeV1, this.ContractUpdatedMessageHandler, false);
			this.client.RegisterMessageHandler("contractDeleted", NamespaceSmartContractsIeeeV1, this.ContractDeletedMessageHandler, false);
			this.client.RegisterMessageHandler("petitionContractMsg", NamespaceSmartContractsIeeeV1, this.PetitionContractMessageHandler, false);
			this.client.RegisterMessageHandler("petitionContractResponseMsg", NamespaceSmartContractsIeeeV1, this.PetitionContractResponseMessageHandler, false);
			this.client.RegisterMessageHandler("contractProposal", NamespaceSmartContractsIeeeV1, this.ContractProposalMessageHandler, false);

			#endregion

			this.aes = Aes.Create();
			this.aes.BlockSize = 128;
			this.aes.KeySize = 256;
			this.aes.Mode = CipherMode.CBC;
			this.aes.Padding = PaddingMode.None;

			this.keySettingsPrefix = KeySettings;
			this.contractKeySettingsPrefix = ContractKeySettings;
		}

		/// <summary>
		/// Disposes of the extension.
		/// </summary>
		public override void Dispose()
		{
			#region NeuroFoundation V1

			this.client.UnregisterMessageHandler("identity", NamespaceLegalIdentitiesNeuroFoundationV1, this.IdentityMessageHandler, true);
			this.client.UnregisterMessageHandler("petitionIdentityMsg", NamespaceLegalIdentitiesNeuroFoundationV1, this.PetitionIdentityMessageHandler, false);
			this.client.UnregisterMessageHandler("petitionIdentityResponseMsg", NamespaceLegalIdentitiesNeuroFoundationV1, this.PetitionIdentityResponseMessageHandler, false);
			this.client.UnregisterMessageHandler("petitionSignatureMsg", NamespaceLegalIdentitiesNeuroFoundationV1, this.PetitionSignatureMessageHandler, false);
			this.client.UnregisterMessageHandler("petitionSignatureResponseMsg", NamespaceLegalIdentitiesNeuroFoundationV1, this.PetitionSignatureResponseMessageHandler, false);
			this.client.UnregisterMessageHandler("petitionClientUrl", NamespaceLegalIdentitiesNeuroFoundationV1, this.PetitionClientUrlEventHandler, false);
			this.client.UnregisterMessageHandler("identityReview", NamespaceLegalIdentitiesNeuroFoundationV1, this.IdentityReviewEventHandler, false);
			this.client.UnregisterMessageHandler("clientMessage", NamespaceLegalIdentitiesNeuroFoundationV1, this.ClientMessageEventHandler, false);

			this.client.UnregisterMessageHandler("contractSigned", NamespaceSmartContractsNeuroFoundationV1, this.ContractSignedMessageHandler, true);
			this.client.UnregisterMessageHandler("contractCreated", NamespaceSmartContractsNeuroFoundationV1, this.ContractCreatedMessageHandler, false);
			this.client.UnregisterMessageHandler("contractUpdated", NamespaceSmartContractsNeuroFoundationV1, this.ContractUpdatedMessageHandler, false);
			this.client.UnregisterMessageHandler("contractDeleted", NamespaceSmartContractsNeuroFoundationV1, this.ContractDeletedMessageHandler, false);
			this.client.UnregisterMessageHandler("petitionContractMsg", NamespaceSmartContractsNeuroFoundationV1, this.PetitionContractMessageHandler, false);
			this.client.UnregisterMessageHandler("petitionContractResponseMsg", NamespaceSmartContractsNeuroFoundationV1, this.PetitionContractResponseMessageHandler, false);
			this.client.UnregisterMessageHandler("contractProposal", NamespaceSmartContractsNeuroFoundationV1, this.ContractProposalMessageHandler, false);

			#endregion

			#region IEEE v1

			this.client.UnregisterMessageHandler("identity", NamespaceLegalIdentitiesIeeeV1, this.IdentityMessageHandler, true);
			this.client.UnregisterMessageHandler("petitionIdentityMsg", NamespaceLegalIdentitiesIeeeV1, this.PetitionIdentityMessageHandler, false);
			this.client.UnregisterMessageHandler("petitionIdentityResponseMsg", NamespaceLegalIdentitiesIeeeV1, this.PetitionIdentityResponseMessageHandler, false);
			this.client.UnregisterMessageHandler("petitionSignatureMsg", NamespaceLegalIdentitiesIeeeV1, this.PetitionSignatureMessageHandler, false);
			this.client.UnregisterMessageHandler("petitionSignatureResponseMsg", NamespaceLegalIdentitiesIeeeV1, this.PetitionSignatureResponseMessageHandler, false);
			this.client.UnregisterMessageHandler("petitionClientUrl", NamespaceLegalIdentitiesIeeeV1, this.PetitionClientUrlEventHandler, false);
			this.client.UnregisterMessageHandler("identityReview", NamespaceLegalIdentitiesIeeeV1, this.IdentityReviewEventHandler, false);
			this.client.UnregisterMessageHandler("clientMessage", NamespaceLegalIdentitiesIeeeV1, this.ClientMessageEventHandler, false);

			this.client.UnregisterMessageHandler("contractSigned", NamespaceSmartContractsIeeeV1, this.ContractSignedMessageHandler, true);
			this.client.UnregisterMessageHandler("contractCreated", NamespaceSmartContractsIeeeV1, this.ContractCreatedMessageHandler, false);
			this.client.UnregisterMessageHandler("contractUpdated", NamespaceSmartContractsIeeeV1, this.ContractUpdatedMessageHandler, false);
			this.client.UnregisterMessageHandler("contractDeleted", NamespaceSmartContractsIeeeV1, this.ContractDeletedMessageHandler, false);
			this.client.UnregisterMessageHandler("petitionContractMsg", NamespaceSmartContractsIeeeV1, this.PetitionContractMessageHandler, false);
			this.client.UnregisterMessageHandler("petitionContractResponseMsg", NamespaceSmartContractsIeeeV1, this.PetitionContractResponseMessageHandler, false);
			this.client.UnregisterMessageHandler("contractProposal", NamespaceSmartContractsIeeeV1, this.ContractProposalMessageHandler, false);

			#endregion

			this.localKeys?.Dispose();
			this.localKeys = null;
			this.keysTimestamp = DateTime.MinValue;

			this.rnd?.Dispose();
			this.rnd = null;

			this.aes?.Dispose();
			this.aes = null;

			base.Dispose();
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[] { };

		/// <summary>
		/// Component address.
		/// </summary>
		public string ComponentAddress => this.componentAddress;

		#endregion

		#region Keys

		/// <summary>
		/// Timestamps of current keys used for signatures.
		/// </summary>
		public DateTime KeysTimestamp => this.keysTimestamp;

		/// <summary>
		/// Prefix for client key runtime settings.
		/// </summary>
		public string KeySettingsPrefix => this.keySettingsPrefix;

		/// <summary>
		/// Prefix for contract key runtime settings.
		/// </summary>
		public string ContractKeySettingsPrefix => this.contractKeySettingsPrefix;

		/// <summary>
		/// Preferred Encryption Algorithm
		/// </summary>
		public SymmetricCipherAlgorithms PreferredEncryptionAlgorithm => this.preferredEncryptionAlgorithm;

		/// <summary>
		/// Sets the preferred encryption algorithm.
		/// </summary>
		/// <param name="Algorithm">Preferred algorithm.</param>
		/// <param name="Lock">If the setting should be locked for the rest of the runtime of the application.</param>
		/// <exception cref="InvalidOperationException">If attempting to change a locked setting.</exception>
		public void SetPreferredEncryptionAlgorithm(SymmetricCipherAlgorithms Algorithm, bool Lock)
		{
			if (this.preferredEncryptionAlgorithm == Algorithm)
				return;

			if (this.preferredEncryptionAlgorithmLocked)
				throw new InvalidOperationException("Preferred Encryptio Algorithm has been locked.");

			this.preferredEncryptionAlgorithm = Algorithm;
			this.preferredEncryptionAlgorithmLocked = Lock;
		}

		/// <summary>
		/// Loads keys from the underlying persistence layer.
		/// </summary>
		/// <param name="CreateIfNone">Allows new keys to be created, if no keys were found in the persistence layer.</param>
		/// <returns>If keys were loaded or generated (i.e. can be used) or not.</returns>
		public Task<bool> LoadKeys(bool CreateIfNone)
		{
			return this.LoadKeys(CreateIfNone, null);
		}

		/// <summary>
		/// Loads keys from the underlying persistence layer.
		/// </summary>
		/// <param name="CreateIfNone">Allows new keys to be created, if no keys were found in the persistence layer.</param>
		/// <param name="Thread">Optional thread to use during profiling.</param>
		/// <returns>If keys were loaded or generated (i.e. can be used) or not.</returns>
		public async Task<bool> LoadKeys(bool CreateIfNone, ProfilerThread Thread)
		{
			Thread = Thread?.CreateSubThread("Load Keys", ProfilerThreadType.Sequential);
			Thread?.Start();
			try
			{
				Thread?.NewState("Search");

				List<IE2eEndpoint> Keys = new List<IE2eEndpoint>();
				Dictionary<string, object> Settings = await RuntimeSettings.GetWhereKeyLikeAsync(this.keySettingsPrefix + "*", "*");

				Thread?.NewState("Endpoints");

				IE2eEndpoint[] AvailableEndpoints = EndpointSecurity.CreateEndpoints(256, 192, int.MaxValue, typeof(EllipticCurveEndpoint), Thread);
				DateTime? Timestamp = null;
				bool DisposeEndpoints = true;
				byte[] Key;

				Thread?.NewState("Select");

				foreach (KeyValuePair<string, object> Setting in Settings)
				{
					string LocalName = Setting.Key[this.keySettingsPrefix.Length..];

					if (Setting.Value is string d)
					{
						if (string.IsNullOrEmpty(d))
							continue;

						try
						{
							Key = Convert.FromBase64String(d);
						}
						catch (Exception)
						{
							continue;
						}

						foreach (IE2eEndpoint Curve in AvailableEndpoints)
						{
							if (Curve.LocalName == LocalName)
							{
								Keys.Add(Curve.CreatePrivate(Key));
								break;
							}
						}
					}
					else if (Setting.Value is DateTime TP && LocalName == "Timestamp")
						Timestamp = TP;
				}

				if (Keys.Count == 0)
				{
					if (!CreateIfNone)
						return false;

					Thread?.NewState("Create");

					DisposeEndpoints = false;

					foreach (IE2eEndpoint Endpoint in AvailableEndpoints)
					{
						if (Endpoint is EllipticCurveEndpoint Curve)
						{
							Key = this.GetKey(Curve.Curve);
							await RuntimeSettings.SetAsync(this.keySettingsPrefix + Curve.LocalName, Convert.ToBase64String(Key));
							Keys.Add(Curve);
						}
					}

					Timestamp = DateTime.Now;
					await RuntimeSettings.SetAsync(this.keySettingsPrefix + "Timestamp", Timestamp.Value);

					Log.Notice("Private keys for contracts client created.", this.client.BareJID, string.Empty, "NewKeys");
				}
				else if (!Timestamp.HasValue)
				{
					Thread?.NewState("Time");

					Timestamp = DateTime.Now;
					await RuntimeSettings.SetAsync(this.keySettingsPrefix + "Timestamp", Timestamp.Value);
				}

				Thread?.NewState("Sec");

				this.localKeys?.Dispose();
				this.localKeys = null;

				this.localKeys = new EndpointSecurity(this.localKeysForE2e ? this.client : null, 128, Keys.ToArray());
				this.keysTimestamp = Timestamp.Value;

				if (this.localKeysForE2e)
					this.localE2eEndpoint = this.localKeys;

				if (DisposeEndpoints)
				{
					Thread?.NewState("Dispose");

					foreach (IE2eEndpoint Curve in AvailableEndpoints)
						Curve.Dispose();
				}
			}
			finally
			{
				Thread?.Stop();
			}

			return true;
		}

		private byte[] GetKey(EllipticCurve Curve)
		{
			string s = Curve.Export();
			XmlDocument Doc = new XmlDocument()
			{
				PreserveWhitespace = true
			};
			Doc.LoadXml(s);
			s = Doc.DocumentElement.GetAttribute("d");
			return Convert.FromBase64String(s);
		}

		/// <summary>
		/// Generates new keys for the contracts clients.
		/// </summary>
		public async Task GenerateNewKeys()
		{
			await RuntimeSettings.DeleteWhereKeyLikeAsync(this.keySettingsPrefix + "*", "*");
			await this.LoadKeys(true);

			lock (this.matchingKeys)
			{
				this.matchingKeys.Clear();
			}

			foreach (LegalIdentityState State in await Database.Find<LegalIdentityState>(
				new FilterFieldEqualTo("BareJid", this.client.BareJID)))
			{
				LegalIdentity Identity;

				if (State.State == IdentityState.Created || State.State == IdentityState.Approved)
				{
					try
					{
						Identity = await this.GetLegalIdentityAsync(State.LegalId);   // Make sure we have the latest.
						if (Identity.State != State.State)
						{
							State.State = Identity.State;
							State.Timestamp = Identity.Updated;

							switch (Identity.State)
							{
								case IdentityState.Rejected:
								case IdentityState.Obsoleted:
								case IdentityState.Compromised:
									State.PublicKey = null;
									break;
							}

							await Database.Update(State);
						}
					}
					catch (ItemNotFoundException)
					{
						await Database.Delete(State);
						continue;
					}
					catch (Exception ex)
					{
						Log.Exception(ex, State.LegalId);
					}
				}

				switch (State.State)
				{
					case IdentityState.Created:
					case IdentityState.Approved:
						try
						{
							Identity = await this.ObsoleteLegalIdentityAsync(State.LegalId);
							await this.UpdateSettings(Identity);
						}
						catch (ItemNotFoundException)
						{
							await Database.Delete(State);
						}
						catch (Exception ex)
						{
							Log.Exception(ex, State.LegalId);
						}
						break;
				}
			}
		}

		/// <summary>
		/// Exports Keys to XML.
		/// </summary>
		/// <returns>XML string.</returns>
		public async Task<string> ExportKeys()
		{
			StringBuilder Xml = new StringBuilder();
			XmlWriterSettings Settings = XML.WriterSettings(false, true);

			using (XmlWriter Output = XmlWriter.Create(Xml, Settings))
			{
				await this.ExportKeys(Output);
			}

			return Xml.ToString();
		}

		/// <summary>
		/// Exports Keys to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public async Task ExportKeys(XmlWriter Output)
		{
			this.AssertAllowed();

			Output.WriteStartElement("LegalId", NamespaceOnboarding);

			Dictionary<string, object> Settings = await RuntimeSettings.GetWhereKeyLikeAsync(this.keySettingsPrefix + "*", "*");

			foreach (KeyValuePair<string, object> Setting in Settings)
			{
				string Name = Setting.Key[this.keySettingsPrefix.Length..];

				if (Setting.Value is string s)
				{
					Output.WriteStartElement("S");
					Output.WriteAttributeString("n", Name);
					Output.WriteAttributeString("v", s);
					Output.WriteEndElement();
				}
				else if (Setting.Value is DateTime TP)
				{
					Output.WriteStartElement("DT");
					Output.WriteAttributeString("n", Name);
					Output.WriteAttributeString("v", XML.Encode(TP));
					Output.WriteEndElement();
				}
			}

			foreach (LegalIdentityState State in await Database.Find<LegalIdentityState>(new FilterAnd(
				new FilterFieldEqualTo("BareJid", this.client.BareJID),
				new FilterFieldEqualTo("State", IdentityState.Approved))))
			{
				Output.WriteStartElement("State");
				Output.WriteAttributeString("legalId", State.LegalId);
				Output.WriteAttributeString("publicKey", Convert.ToBase64String(State.PublicKey));
				Output.WriteAttributeString("timestamp", XML.Encode(State.Timestamp));
				Output.WriteEndElement();
			}

			Settings = await RuntimeSettings.GetWhereKeyLikeAsync(this.contractKeySettingsPrefix + "*", "*");

			foreach (KeyValuePair<string, object> Setting in Settings)
			{
				string Name = Setting.Key[this.contractKeySettingsPrefix.Length..];

				if (Setting.Value is string s)
				{
					Output.WriteStartElement("C");
					Output.WriteAttributeString("n", Name);
					Output.WriteAttributeString("v", s);
					Output.WriteEndElement();
				}
			}

			Output.WriteEndElement();
		}

		/// <summary>
		/// Imports keys
		/// </summary>
		/// <param name="Xml">XML Definition of keys.</param>
		/// <returns>If keys could be loaded into the client.</returns>
		public Task<bool> ImportKeys(string Xml)
		{
			XmlDocument Doc = new XmlDocument();
			Doc.LoadXml(Xml);

			return this.ImportKeys(Doc);
		}

		/// <summary>
		/// Imports keys
		/// </summary>
		/// <param name="Xml">XML Definition of keys.</param>
		/// <returns>If keys could be loaded into the client.</returns>
		public Task<bool> ImportKeys(XmlDocument Xml)
		{
			return this.ImportKeys(Xml.DocumentElement);
		}

		/// <summary>
		/// Imports keys
		/// </summary>
		/// <param name="Xml">XML Definition of keys.</param>
		/// <returns>If keys could be loaded into the client.</returns>
		public async Task<bool> ImportKeys(XmlElement Xml)
		{
			this.AssertAllowed();

			if (Xml is null || Xml.LocalName != "LegalId" || Xml.NamespaceURI != NamespaceOnboarding)
				return false;

			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (!(N is XmlElement E))
					continue;

				if (E.NamespaceURI != NamespaceOnboarding)
					return false;

				switch (E.LocalName)
				{
					case "S":
						string Name = XML.Attribute(E, "n");
						string StringValue = XML.Attribute(E, "v");

						await RuntimeSettings.SetAsync(this.keySettingsPrefix + Name, StringValue);
						break;

					case "DT":
						Name = XML.Attribute(E, "n");
						DateTime DateTimeValue = XML.Attribute(E, "v", DateTime.MinValue);

						await RuntimeSettings.SetAsync(this.keySettingsPrefix + Name, DateTimeValue);
						break;

					case "C":
						Name = XML.Attribute(E, "n");
						StringValue = XML.Attribute(E, "v");

						await RuntimeSettings.SetAsync(this.contractKeySettingsPrefix + Name, StringValue);
						break;

					case "State":
						string LegalId = XML.Attribute(E, "legalId");
						string PublicKeyStr = XML.Attribute(E, "publicKey");
						byte[] PublicKey;
						DateTimeValue = XML.Attribute(E, "timestamp", DateTime.MinValue);

						try
						{
							PublicKey = Convert.FromBase64String(PublicKeyStr);
						}
						catch (Exception)
						{
							return false;
						}

						LegalIdentityState IdState = await Database.FindFirstDeleteRest<LegalIdentityState>(new FilterAnd(
							new FilterFieldEqualTo("BareJid", this.client.BareJID),
							new FilterFieldEqualTo("LegalId", LegalId)));

						if (IdState is null)
						{
							IdState = new LegalIdentityState()
							{
								BareJid = this.client.BareJID,
								LegalId = LegalId,
								State = IdentityState.Approved,
								Timestamp = DateTimeValue,
								PublicKey = PublicKey
							};

							await Database.Insert(IdState);
						}
						else
						{
							IdState.State = IdentityState.Approved;
							IdState.Timestamp = DateTimeValue;
							IdState.PublicKey = PublicKey;

							await Database.Update(IdState);
						}
						break;

					default:
						return false;
				}
			}

			return await this.LoadKeys(false);
		}

		/// <summary>
		/// Sets the key settings instance name.
		/// </summary>
		/// <param name="InstanceName">Instance name.</param>
		/// <param name="Locked">If the key settings instance name should be locked.</param>
		public void SetKeySettingsInstance(string InstanceName, bool Locked)
		{
			if (this.keySettingsPrefixLocked)
				throw new InvalidOperationException("Key settings instance is locked.");

			if (string.IsNullOrEmpty(InstanceName))
			{
				this.keySettingsPrefix = KeySettings;
				this.contractKeySettingsPrefix = ContractKeySettings;
			}
			else
			{
				this.keySettingsPrefix = InstanceName + "." + KeySettings;
				this.contractKeySettingsPrefix = InstanceName + "." + ContractKeySettings;
			}

			this.keySettingsPrefixLocked = Locked;
		}

		/// <summary>
		/// Defines if End-to-End encryption should use the keys used by the contracts client to perform signatures.
		/// </summary>
		/// <param name="UseLocalKeys">If local keys should be used in End-to-End encryption.</param>
		public Task EnableE2eEncryption(bool UseLocalKeys)
		{
			return this.EnableE2eEncryption(UseLocalKeys, true);
		}

		/// <summary>
		/// Defines if End-to-End encryption should use the keys used by the contracts client to perform signatures.
		/// </summary>
		/// <param name="UseLocalKeys">If local keys should be used in End-to-End encryption.</param>
		/// <param name="CreateKeysIfNone">If local keys should be created, if none available.</param>
		public async Task EnableE2eEncryption(bool UseLocalKeys, bool CreateKeysIfNone)
		{
			bool Reload = !(this.localKeys is null);

			this.localKeysForE2e = UseLocalKeys;

			if (Reload)
				await this.LoadKeys(CreateKeysIfNone);
		}

		/// <summary>
		/// Enables End-to-End encryption with a separate set of keys.
		/// </summary>
		/// <param name="E2eEndpoint">Endpoint managing the keys on the network.</param>
		public Task EnableE2eEncryption(EndpointSecurity E2eEndpoint)
		{
			return this.EnableE2eEncryption(E2eEndpoint, true);
		}

		/// <summary>
		/// Enables End-to-End encryption with a separate set of keys.
		/// </summary>
		/// <param name="E2eEndpoint">Endpoint managing the keys on the network.</param>
		/// <param name="CreateKeysIfNone">If local keys should be created, if none available.</param>
		public async Task EnableE2eEncryption(EndpointSecurity E2eEndpoint, bool CreateKeysIfNone)
		{
			bool Reload = !(this.localKeys is null);

			this.localKeysForE2e = false;
			this.localE2eEndpoint = E2eEndpoint;

			if (Reload)
				await this.LoadKeys(CreateKeysIfNone);
		}

		/// <summary>
		/// Creates an array of random bytes.
		/// </summary>
		/// <param name="Nr">Number of bytes.</param>
		/// <returns>Array of random bytes.</returns>
		/// <exception cref="ArgumentException">If <paramref name="Nr"/> is negative.</exception>
		public byte[] RandomBytes(int Nr)
		{
			if (Nr < 0)
				throw new ArgumentException(nameof(Nr));

			byte[] Bytes = new byte[Nr];

			this.rnd.GetBytes(Bytes);

			return Bytes;
		}

		/// <summary>
		/// Creates a random long unsigned integer.
		/// </summary>
		/// <returns>Random long integer.</returns>
		public ulong RandomInteger()
		{
			byte[] Bin = this.RandomBytes(8);
			return BitConverter.ToUInt64(Bin, 0);
		}

		/// <summary>
		/// Creates a random long unsigned integer lower than <paramref name="MaxExclusive"/>.
		/// </summary>
		/// <param name="MaxExclusive">Result will be below this value,</param>
		/// <returns>Random long integer.</returns>
		public ulong RandomInteger(ulong MaxExclusive)
		{
			if (MaxExclusive == 0)
				throw new ArgumentException(nameof(MaxExclusive));

			return this.RandomInteger() % MaxExclusive;
		}

		/// <summary>
		/// Creates a random number in a range.
		/// </summary>
		/// <param name="MinInclusive">Smallest allowed value (value included).</param>
		/// <param name="MaxInclusive">Largest allowed value (value included).</param>
		/// <returns>Randomin integer.</returns>
		/// <exception cref="ArgumentException">If <paramref name="MaxInclusive"/> is
		/// smaller than <paramref name="MinInclusive"/>.</exception>
		public int RandomInteger(int MinInclusive, int MaxInclusive)
		{
			if (MaxInclusive < MinInclusive)
				throw new ArgumentException(nameof(MaxInclusive));

			ulong Diff = (uint)(MaxInclusive - MinInclusive);
			if (Diff == 0)
				return MinInclusive;

			int Result = (int)this.RandomInteger(Diff + 1UL);
			Result += MinInclusive;

			return Result;
		}

		#endregion

		#region Security

		/// <summary>
		/// If access to sensitive methods is only accessible from a set of approved sources.
		/// </summary>
		/// <param name="ApprovedSources">Approved sources.</param>
		/// <exception cref="NotSupportedException">If trying to change previously set sources.</exception>
		public void SetAllowedSources(object[] ApprovedSources)
		{
			if (!(this.approvedSources is null))
				throw new NotSupportedException("Changing approved sources not permitted.");

			this.approvedSources = ApprovedSources;
		}

		private void AssertAllowed()
		{
			if (!(this.approvedSources is null))
				Assert.CallFromSource(this.approvedSources);
		}

		#endregion

		#region URIs

		/// <summary>
		/// Legal identity URI, as a string.
		/// </summary>
		/// <param name="LegalId">Legal ID</param>
		/// <returns>URI String</returns>
		public static string LegalIdUriString(string LegalId)
		{
			return "iotid:" + LegalId;
		}

		/// <summary>
		/// Legal identity URI.
		/// </summary>
		/// <param name="LegalId">Legal ID</param>
		/// <returns>URI</returns>
		public static Uri LegalIdUri(string LegalId)
		{
			return new Uri(LegalIdUriString(LegalId));
		}

		/// <summary>
		/// Contract identity URI, as a string.
		/// </summary>
		/// <param name="ContractId">Contract ID</param>
		/// <returns>URI String</returns>
		public static string ContractIdUriString(string ContractId)
		{
			return "iotsc:" + ContractId;
		}

		/// <summary>
		/// Contract identity URI.
		/// </summary>
		/// <param name="ContractId">Contract ID</param>
		/// <returns>URI</returns>
		public static Uri ContractIdUri(string ContractId)
		{
			return new Uri(ContractIdUriString(ContractId));
		}

		#endregion

		#region Server Public Keys

		/// <summary>
		/// Gets the server public key.
		/// </summary>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public Task GetServerPublicKey(EventHandlerAsync<KeyEventArgs> Callback, object State)
		{
			return this.GetServerPublicKey(this.componentAddress, Callback, State);
		}

		/// <summary>
		/// Gets the server public key.
		/// </summary>
		/// <param name="Address">Address of entity whose public key is requested.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public async Task GetServerPublicKey(string Address, EventHandlerAsync<KeyEventArgs> Callback, object State)
		{
			KeyEventArgs e0;

			lock (this.publicKeys)
			{
				if (!this.publicKeys.TryGetValue(Address, out e0))
					e0 = null;
			}

			if (!(e0 is null))
			{
				e0 = new KeyEventArgs(e0, e0.Key)
				{
					State = State
				};

				await Callback.Raise(this, e0);
			}
			else
			{
				await this.client.SendIqGet(Address, "<getPublicKey xmlns=\"" + NamespaceLegalIdentitiesCurrent + "\"/>", async (Sender, e) =>
				{
					IE2eEndpoint ServerKey = null;
					XmlElement E;

					if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "publicKey")
					{
						foreach (XmlNode N in E.ChildNodes)
						{
							if (N is XmlElement E2)
							{
								ServerKey = EndpointSecurity.ParseE2eKey(E2);
								if (!(ServerKey is null))
									break;
							}
						}

						e.Ok = !(ServerKey is null);
					}
					else
						e.Ok = false;

					e0 = new KeyEventArgs(e, ServerKey);

					if (e0.Ok)
					{
						lock (this.publicKeys)
						{
							this.publicKeys[Address] = e0;
						}
					}

					await Callback.Raise(this, e0);
				}, State);
			}
		}

		/// <summary>
		/// Gets the server public key.
		/// </summary>
		/// <returns>Server public key.</returns>
		public Task<IE2eEndpoint> GetServerPublicKeyAsync()
		{
			return this.GetServerPublicKeyAsync(this.componentAddress);
		}

		/// <summary>
		/// Gets the server public key.
		/// </summary>
		/// <param name="Address">Address of entity whose public key is requested.</param>
		/// <returns>Server public key.</returns>
		public async Task<IE2eEndpoint> GetServerPublicKeyAsync(string Address)
		{
			TaskCompletionSource<IE2eEndpoint> Result = new TaskCompletionSource<IE2eEndpoint>();

			await this.GetServerPublicKey(Address, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Key);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to get public key."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		#endregion

		#region Matching Local Keys

		/// <summary>
		/// Get the local key that matches the server key.
		/// </summary>
		/// <param name="Callback">Method called when response is available.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetMatchingLocalKey(EventHandlerAsync<KeyEventArgs> Callback, object State)
		{
			return this.GetMatchingLocalKey(this.componentAddress, Callback, State);
		}

		/// <summary>
		/// Endpoint used for identity and contract signatures.
		/// </summary>
		private EndpointSecurity LocalEndpoint
		{
			get
			{
				if (this.localKeys is null)
					throw new InvalidOperationException("Local keys not loaded or generated.");

				return this.localKeys;
			}
		}

		/// <summary>
		/// Endpoint used for End-to-End encryption.
		/// </summary>
		public EndpointSecurity LocalE2eEndpoint
		{
			get
			{
				if (this.localE2eEndpoint is null)
					throw new InvalidOperationException("End-to-End Encryption not enabled or Local keys not loaded or generated.");

				return this.localE2eEndpoint;
			}
		}

		/// <summary>
		/// Get the local key that matches a given server key.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Callback">Method called when response is available.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public async Task GetMatchingLocalKey(string Address, EventHandlerAsync<KeyEventArgs> Callback, object State)
		{
			KeyEventArgs e0;

			lock (this.matchingKeys)
			{
				if (!this.matchingKeys.TryGetValue(Address, out e0))
					e0 = null;
			}

			if (!(e0 is null))
			{
				e0 = new KeyEventArgs(e0, e0.Key)
				{
					State = State
				};

				await Callback.Raise(this, e0);
			}
			else
			{
				await this.GetServerPublicKey(Address, async (Sender, e) =>
				{
					IE2eEndpoint LocalKey = null;

					if (e.Ok)
					{
						LocalKey = this.LocalEndpoint.FindLocalEndpoint(e.Key);
						if (LocalKey is null)
							e.Ok = false;
					}

					e0 = new KeyEventArgs(e, LocalKey);

					if (e0.Ok)
					{
						lock (this.matchingKeys)
						{
							this.matchingKeys[Address] = e0;
						}
					}

					await Callback.Raise(this, e0);

				}, State);
			}
		}

		/// <summary>
		/// Get the local key that matches the server key.
		/// </summary>
		/// <returns>Local key.</returns>
		public Task<IE2eEndpoint> GetMatchingLocalKeyAsync()
		{
			return this.GetMatchingLocalKeyAsync(this.componentAddress);
		}

		/// <summary>
		/// Get the local key that matches a given server key.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <returns>Local key.</returns>
		public async Task<IE2eEndpoint> GetMatchingLocalKeyAsync(string Address)
		{
			TaskCompletionSource<IE2eEndpoint> Result = new TaskCompletionSource<IE2eEndpoint>();

			await this.GetMatchingLocalKey(Address, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Key);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to get matching local key."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		#endregion

		#region ID Application Attributes

		/// <summary>
		/// Gets attributes relevant for application for legal identities on the broker.
		/// </summary>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetIdApplicationAttributes(EventHandlerAsync<IdApplicationAttributesEventArgs> Callback, object State)
		{
			return this.client.SendIqGet(this.componentAddress, "<applicationAttributes xmlns='" + NamespaceLegalIdentitiesCurrent + "'/>", (Sender, e) =>
			{
				return Callback.Raise(this, new IdApplicationAttributesEventArgs(e));

			}, State);
		}

		/// <summary>
		/// Gets attributes relevant for application for legal identities on the broker.
		/// </summary>
		/// <returns>ID Application attributes</returns>
		public async Task<IdApplicationAttributesEventArgs> GetIdApplicationAttributesAsync()
		{
			TaskCompletionSource<IdApplicationAttributesEventArgs> Result = new TaskCompletionSource<IdApplicationAttributesEventArgs>();

			await this.GetIdApplicationAttributes((Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to get ID Application attributes."));

				return Task.CompletedTask;
			}, null);

			return await Result.Task;
		}

		#endregion

		#region Apply for a Legal Identity

		/// <summary>
		/// Applies for a legal identity to be registered.
		/// </summary>
		/// <param name="Properties">Properties of the legal identity.</param>
		/// <param name="Callback">Method to call when registration response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task Apply(Property[] Properties, EventHandlerAsync<LegalIdentityEventArgs> Callback, object State)
		{
			return this.Apply(this.componentAddress, Properties, Callback, State);
		}

		/// <summary>
		/// Applies for a legal identity to be registered.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Properties">Properties of the legal identity.</param>
		/// <param name="Callback">Method to call when registration response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public async Task Apply(string Address, Property[] Properties, EventHandlerAsync<LegalIdentityEventArgs> Callback, object State)
		{
			this.AssertAllowed();

			await this.GetMatchingLocalKey(Address, async (Sender, e) =>
			{
				if (e.Ok)
				{
					StringBuilder Xml = new StringBuilder();

					Xml.Append("<apply xmlns=\"");
					Xml.Append(NamespaceLegalIdentitiesCurrent);
					Xml.Append("\">");

					StringBuilder Identity = new StringBuilder();

					Identity.Append("<identity><clientPublicKey>");
					e.Key.ToXml(Identity, NamespaceLegalIdentitiesCurrent);
					Identity.Append("</clientPublicKey>");

					foreach (Property Property in Properties)
					{
						Identity.Append("<property name=\"");
						Identity.Append(XML.Encode(Property.Name));
						Identity.Append("\" value=\"");
						Identity.Append(XML.Encode(Property.Value));
						Identity.Append("\"/>");
					}

					string s = Identity.ToString();
					Xml.Append(s);

					s += "</identity>";

					byte[] Bin = Encoding.UTF8.GetBytes(s);
					byte[] Signature = e.Key.Sign(Bin);

					Xml.Append("<clientSignature>");
					Xml.Append(Convert.ToBase64String(Signature));
					Xml.Append("</clientSignature>");

					Xml.Append("</identity></apply>");

					await this.client.SendIqSet(Address, Xml.ToString(), async (sender2, e2) =>
					{
						LegalIdentity Identity2 = null;
						XmlElement E;

						if (e2.Ok && !((E = e2.FirstElement) is null) &&
							E.LocalName == "identity")
						{
							Identity2 = LegalIdentity.Parse(E);
							await this.UpdateSettings(Identity2, e.Key.PublicKey);
						}
						else
							e2.Ok = false;

						await Callback.Raise(this, new LegalIdentityEventArgs(e2, Identity2));
					}, e.State);
				}
				else
					await Callback.Raise(this, new LegalIdentityEventArgs(e, null));
			}, State);
		}

		/// <summary>
		/// Applies for a legal identity to be registered.
		/// </summary>
		/// <param name="Properties">Properties of the legal identity.</param>
		/// <returns>Identity object representing the application.</returns>
		public Task<LegalIdentity> ApplyAsync(Property[] Properties)
		{
			return this.ApplyAsync(this.componentAddress, Properties);
		}

		/// <summary>
		/// Applies for a legal identity to be registered.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Properties">Properties of the legal identity.</param>
		/// <returns>Identity object representing the application.</returns>
		public async Task<LegalIdentity> ApplyAsync(string Address, Property[] Properties)
		{
			TaskCompletionSource<LegalIdentity> Result = new TaskCompletionSource<LegalIdentity>();

			await this.Apply(Address, Properties, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Identity);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to apply for a legal identity to be registered."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		#endregion

		#region Mark Identity as Ready for Approval

		/// <summary>
		/// Marks an Identity as Ready for Approval. Call this after necessary attachments have been
		/// added. If automatic KYC modules exist on the server, they may at this point process
		/// available information, and choose to automatically approve or reject the application.
		/// </summary>
		/// <param name="LegalIdentityId">ID of Legal Identity that is ready for approval.</param>
		/// <param name="Callback">Method to call when registration response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task ReadyForApproval(string LegalIdentityId, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.ReadyForApproval(this.componentAddress, LegalIdentityId, Callback, State);
		}

		/// <summary>
		/// Marks an Identity as Ready for Approval. Call this after necessary attachments have been
		/// added. If automatic KYC modules exist on the server, they may at this point process
		/// available information, and choose to automatically approve or reject the application.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="LegalIdentityId">ID of Legal Identity that is ready for approval.</param>
		/// <param name="Callback">Method to call when registration response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task ReadyForApproval(string Address, string LegalIdentityId, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			this.AssertAllowed();

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<readyForApproval xmlns=\"");
			Xml.Append(NamespaceLegalIdentitiesCurrent);
			Xml.Append("\" id=\"");
			Xml.Append(XML.Encode(LegalIdentityId));
			Xml.Append("\"/>");

			return this.client.SendIqSet(Address, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// Marks an Identity as Ready for Approval. Call this after necessary attachments have been
		/// added. If automatic KYC modules exist on the server, they may at this point process
		/// available information, and choose to automatically approve or reject the application.
		/// </summary>
		/// <param name="LegalIdentityId">ID of Legal Identity that is ready for approval.</param>
		public Task ReadyForApprovalAsync(string LegalIdentityId)
		{
			return this.ReadyForApprovalAsync(this.componentAddress, LegalIdentityId);
		}

		/// <summary>
		/// Marks an Identity as Ready for Approval. Call this after necessary attachments have been
		/// added. If automatic KYC modules exist on the server, they may at this point process
		/// available information, and choose to automatically approve or reject the application.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="LegalIdentityId">ID of Legal Identity that is ready for approval.</param>
		public async Task ReadyForApprovalAsync(string Address, string LegalIdentityId)
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();

			await this.ReadyForApproval(Address, LegalIdentityId, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(true);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to flag identity as ready for approval."));

				return Task.CompletedTask;

			}, null);

			await Result.Task;
		}

		#endregion

		#region Identity Review message

		private async Task IdentityReviewEventHandler(object Sender, MessageEventArgs e)
		{
			string LegalId = XML.Attribute(e.Content, "id");
			IdentityReviewEventArgs e2 = new IdentityReviewEventArgs(e, LegalId);
			this.ParseValidationDetails(e.Content, e2);

			if ((!e2.IsValid.HasValue && (e2.HasValidatedClaims || e2.HasValidatedPhotos)) ||
				e2.IsValid.Value)
			{
				await this.AddIdentityReviewAttachment(e2);
			}

			await this.IdentityReview.Raise(this, e2);
		}

		/// <summary>
		/// Event raised when an Identity Application has been automatically reviewed.
		/// </summary>
		public event EventHandlerAsync<IdentityReviewEventArgs> IdentityReview;

		private void ParseValidationDetails(XmlElement Content, IdentityReviewEventArgs e)
		{
			ChunkedList<InvalidClaim> InvalidClaims = null;
			ChunkedList<InvalidPhoto> InvalidPhotos = null;
			ChunkedList<ValidationError> ValidationErrors = null;
			ChunkedList<ValidClaim> ValidClaims = null;
			ChunkedList<ValidPhoto> ValidPhotos = null;
			ChunkedList<string> UnvalidatedClaims = null;
			ChunkedList<string> UnvalidatedPhotos = null;

			foreach (XmlNode N in Content.ChildNodes)
			{
				if (!(N is XmlElement E))
					continue;

				switch (E.LocalName)
				{
					case "invalidClaim":
						string Claim = XML.Attribute(E, "claim");
						string Message = XML.Attribute(E, "message");
						string Code = XML.Attribute(E, "code");
						string Service = XML.Attribute(E, "service");
						string Language = XML.Attribute(E, "xml:lang");

						InvalidClaims ??= new ChunkedList<InvalidClaim>();
						InvalidClaims.Add(new InvalidClaim(Claim, Message, Language, Code, Service));
						break;

					case "invalidPhoto":
						string FileName = XML.Attribute(E, "fileName");
						Message = XML.Attribute(E, "message");
						Code = XML.Attribute(E, "code");
						Service = XML.Attribute(E, "service");
						Language = XML.Attribute(E, "xml:lang");

						InvalidPhotos ??= new ChunkedList<InvalidPhoto>();
						InvalidPhotos.Add(new InvalidPhoto(FileName, Message, Language, Code, Service));
						break;

					case "error":
						ValidationErrorType Type = XML.Attribute(E, "type", ValidationErrorType.Client);
						Message = XML.Attribute(E, "message");
						Code = XML.Attribute(E, "code");
						Service = XML.Attribute(E, "service");
						Language = XML.Attribute(E, "xml:lang");

						ChunkedList<KeyValuePair<string, object>> Tags = null;

						foreach (XmlNode N2 in E.ChildNodes)
						{
							if (!(N2 is XmlElement E2))
								continue;

							if (E2.LocalName == "tag")
							{
								string TagName = XML.Attribute(E2, "name");
								string TagValue = XML.Attribute(E2, "value");
								string TagType = XML.Attribute(E2, "type");

								if (!XmppEventReceptor.TryParse(TagValue, TagType, out object TagValueParsed))
									TagValueParsed = TagValue;

								Tags ??= new ChunkedList<KeyValuePair<string, object>>();
								Tags.Add(new KeyValuePair<string, object>(TagName, TagValueParsed));
							}
						}

						ValidationErrors ??= new ChunkedList<ValidationError>();
						ValidationErrors.Add(new ValidationError(Type, Message, Language, Code, Service,
							Tags?.ToArray() ?? Array.Empty<KeyValuePair<string, object>>()));
						break;

					case "validatedClaim":
						Claim = XML.Attribute(E, "claim");
						Service = XML.Attribute(E, "service");

						ValidClaims ??= new ChunkedList<ValidClaim>();
						ValidClaims.Add(new ValidClaim(Claim, Service));
						break;

					case "validatedPhoto":
						FileName = XML.Attribute(E, "fileName");
						Service = XML.Attribute(E, "service");

						ValidPhotos ??= new ChunkedList<ValidPhoto>();
						ValidPhotos.Add(new ValidPhoto(FileName, Service));
						break;

					case "unvalidatedClaim":
						Claim = XML.Attribute(E, "claim");

						UnvalidatedClaims ??= new ChunkedList<string>();
						UnvalidatedClaims.Add(Claim);
						break;

					case "unvalidatedPhoto":
						FileName = XML.Attribute(E, "fileName");

						UnvalidatedPhotos ??= new ChunkedList<string>();
						UnvalidatedPhotos.Add(FileName);
						break;
				}
			}

			e.InvalidClaims = InvalidClaims?.ToArray();
			e.InvalidPhotos = InvalidPhotos?.ToArray();
			e.ValidationErrors = ValidationErrors?.ToArray();
			e.ValidClaims = ValidClaims?.ToArray();
			e.ValidPhotos = ValidPhotos?.ToArray();
			e.UnvalidatedClaims = UnvalidatedClaims?.ToArray();
			e.UnvalidatedPhotos = UnvalidatedPhotos?.ToArray();
		}

		/// <summary>
		/// Adds an attachment to a legal identity with information about an identity review.
		/// </summary>
		/// <param name="e">Identity Review message event arguments.</param>
		/// <returns>Updated identity.</returns>
		private async Task<LegalIdentity> AddIdentityReviewAttachment(IdentityReviewEventArgs e)
		{
			if (!this.client.TryGetExtension(out HttpFileUploadClient HttpFileUploadClient))
				throw new InvalidOperationException("No HTTP File Upload extension added to the XMPP Client.");

			string Xml = e.Content.OuterXml;
			byte[] Data = Encoding.UTF8.GetBytes(Xml.ToString());
			byte[] Signature = await this.SignAsync(Data, SignWith.CurrentKeys);
			string FileName = "ApplicationReview.xml";
			string ContentType = "text/xml; charset=utf-8";

			HttpFileUploadEventArgs e2 = await HttpFileUploadClient.RequestUploadSlotAsync(FileName, ContentType, Data.Length);
			if (!e2.Ok)
				throw new IOException("Unable to upload Application Review attachment to broker.");

			await e2.PUT(Data, ContentType, 10000);

			return await this.AddLegalIdAttachmentAsync(e.LegalId, e2.GetUrl, Signature);
		}

		#endregion

		#region Client Message

		private async Task ClientMessageEventHandler(object Sender, MessageEventArgs e)
		{
			string LegalId = XML.Attribute(e.Content, "id");
			string Code = XML.Attribute(e.Content, "code");
			ValidationErrorType Type = XML.Attribute(e.Content, "type", ValidationErrorType.Client);
			ClientMessageEventArgs e2 = new ClientMessageEventArgs(e, LegalId, Code, Type);
			this.ParseValidationDetails(e.Content, e2);

			await this.ClientMessage.Raise(this, e2);
		}

		/// <summary>
		/// Event raised when a Client Message has been received.
		/// </summary>
		public event EventHandlerAsync<ClientMessageEventArgs> ClientMessage;

		#endregion

		#region Validate Legal Identity

		/// <summary>
		/// Validates a legal identity.
		/// </summary>
		/// <param name="Identity">Legal identity to validate</param>
		/// <param name="Callback">Method to call when validation is completed</param>
		/// <param name="State">State object to pass to callback method.</param>
		public Task Validate(LegalIdentity Identity, EventHandlerAsync<IdentityValidationEventArgs> Callback, object State)
		{
			return this.Validate(Identity, true, Callback, State);
		}

		/// <summary>
		/// Validates a legal identity.
		/// </summary>
		/// <param name="Identity">Legal identity to validate</param>
		/// <param name="ValidateState">If the state attribute should be validated. (Default=true)</param>
		/// <param name="Callback">Method to call when validation is completed</param>
		/// <param name="State">State object to pass to callback method.</param>
		public async Task Validate(LegalIdentity Identity, bool ValidateState, EventHandlerAsync<IdentityValidationEventArgs> Callback, object State)
		{
			if (Identity is null)
			{
				await this.ReturnStatus(IdentityStatus.IdentityUndefined, Callback, State);
				return;
			}

			if (ValidateState && Identity.State != IdentityState.Approved)
			{
				await this.ReturnStatus(IdentityStatus.NotApproved, Callback, State);
				return;
			}

			DateTime Now = DateTime.Now;

			if (Now.Date.AddDays(1) < Identity.From)    // To avoid Time-zone problems
			{
				await this.ReturnStatus(IdentityStatus.NotValidYet, Callback, State);
				return;
			}

			if (Now.Date.AddDays(-1) > Identity.To)      // To avoid Time-zone problems
			{
				await this.ReturnStatus(IdentityStatus.NotValidAnymore, Callback, State);
				return;
			}

			if (string.IsNullOrEmpty(Identity.Provider))
			{
				await this.ReturnStatus(IdentityStatus.NoTrustProvider, Callback, State);
				return;
			}

			if (string.IsNullOrEmpty(Identity.ClientKeyName) ||
				Identity.ClientPubKey is null || Identity.ClientPubKey.Length == 0)
			{
				await this.ReturnStatus(IdentityStatus.NoClientPublicKey, Callback, State);
				return;
			}

			if (Identity.ClientSignature is null || Identity.ClientSignature.Length == 0)
			{
				await this.ReturnStatus(IdentityStatus.NoClientSignature, Callback, State);
				return;
			}

			StringBuilder Xml = new StringBuilder();
			Identity.Serialize(Xml, false, false, false, false, false, false, false);
			byte[] Data = Encoding.UTF8.GetBytes(Xml.ToString());

			bool? b = this.ValidateSignature(Identity, Data, Identity.ClientSignature);
			if (b.HasValue)
			{
				if (!b.Value)
				{
					await this.ReturnStatus(IdentityStatus.ClientSignatureInvalid, Callback, State);
					return;
				}
			}
			else
			{
				await this.ReturnStatus(IdentityStatus.ClientKeyNotRecognized, Callback, State);
				return;
			}

			if (Identity.State == IdentityState.Approved && ValidateState && !(Identity.Attachments is null))
			{
				foreach (Attachment Attachment in Identity.Attachments)
				{
					if (string.IsNullOrEmpty(Attachment.Url))
					{
						await this.ReturnStatus(IdentityStatus.AttachmentLacksUrl, Callback, State);
						return;
					}

					try
					{
						KeyValuePair<string, TemporaryFile> P = await this.GetAttachmentAsync(Attachment.Url, SignWith.LatestApprovedIdOrCurrentKeys, 30000);
						using TemporaryFile File = P.Value;

						if (P.Key != Attachment.ContentType)
						{
							await this.ReturnStatus(IdentityStatus.AttachmentInconsistency, Callback, State);
							return;
						}

						File.Position = 0;

						b = this.ValidateSignature(Identity, File, Attachment.Signature);
						if (b.HasValue)
						{
							if (!b.Value)
							{
								await this.ReturnStatus(IdentityStatus.AttachmentSignatureInvalid, Callback, State);
								return;
							}
						}
						else
						{
							await this.ReturnStatus(IdentityStatus.ClientKeyNotRecognized, Callback, State);
							return;
						}
					}
					catch (Exception ex)
					{
						this.client.Error("Attachment " + Attachment.Url + "unavailable: " + ex.Message);
						await this.ReturnStatus(IdentityStatus.AttachmentUnavailable, Callback, State);
						return;
					}
				}
			}

			if (Identity.ServerSignature is null || Identity.ServerSignature.Length == 0)
			{
				await this.ReturnStatus(IdentityStatus.NoProviderSignature, Callback, State);
				return;
			}

			Xml.Clear();
			Identity.Serialize(Xml, false, true, true, true, true, false, false);
			Data = Encoding.UTF8.GetBytes(Xml.ToString());

			bool HasOldPublicKey;

			lock (this.publicKeys)
			{
				HasOldPublicKey = this.publicKeys.ContainsKey(Identity.Provider);
			}

			await this.GetServerPublicKey(Identity.Provider, async (Sender, e) =>
			{
				if (e.Ok && !(e.Key is null))
				{
					bool Valid = e.Key.Verify(Data, Identity.ServerSignature);

					if (Valid)
					{
						await this.ReturnStatus(IdentityStatus.Valid, Callback, State);
						return;
					}

					if (!HasOldPublicKey)
					{
						await this.ReturnStatus(IdentityStatus.ProviderSignatureInvalid, Callback, State);
						return;
					}

					lock (this.publicKeys)
					{
						this.publicKeys.Remove(Identity.Provider);
					}

					await this.GetServerPublicKey(Identity.Provider, (sender2, e2) =>
					{
						if (e2.Ok && !(e2.Key is null))
						{
							if (e.Key.Equals(e2.Key))
								return this.ReturnStatus(IdentityStatus.ProviderSignatureInvalid, Callback, State);

							Valid = e2.Key.Verify(Data, Identity.ServerSignature);

							if (Valid)
								return this.ReturnStatus(IdentityStatus.Valid, Callback, State);
							else
								return this.ReturnStatus(IdentityStatus.ProviderSignatureInvalid, Callback, State);
						}
						else
							return this.ReturnStatus(IdentityStatus.NoProviderPublicKey, Callback, State);

					}, State);
				}
				else
					await this.ReturnStatus(IdentityStatus.NoProviderPublicKey, Callback, State);

			}, State);
		}

		/// <summary>
		/// Validates a signature of binary data.
		/// </summary>
		/// <param name="Identity">Legal identity used to create the signature.</param>
		/// <param name="Data">Binary data to sign.</param>
		/// <param name="Signature">Digital signature of data</param>
		/// <returns>
		/// true = Signature is valid.
		/// false = Signature is invalid.
		/// null = Client key algorithm is unknown, and veracity of signature could not be established.
		/// </returns>
		public bool? ValidateSignature(LegalIdentity Identity, byte[] Data, byte[] Signature)
		{
			if (Identity.ClientKeyName.StartsWith("RSA") &&
				int.TryParse(Identity.ClientKeyName[3..], out int KeySize))
			{
				return RsaEndpoint.Verify(Data, Signature, KeySize, Identity.ClientPubKey);
			}
			else if (EndpointSecurity.TryGetEndpoint(Identity.ClientKeyName,
				Identity.Namespace.Replace(":iot:leg:id:", ":iot:e2e:").Replace("urn:ieee:", "urn:nf:"),
				out IE2eEndpoint LocalKey) &&
				LocalKey is EllipticCurveEndpoint LocalEc)
			{
				return LocalEc.Verify(Data, Identity.ClientPubKey, Signature);
			}
			else
				return null;
		}

		/// <summary>
		/// Validates a signature of binary data.
		/// </summary>
		/// <param name="Identity">Legal identity used to create the signature.</param>
		/// <param name="Data">Binary data to sign.</param>
		/// <param name="Signature">Digital signature of data</param>
		/// <returns>
		/// true = Signature is valid.
		/// false = Signature is invalid.
		/// null = Client key algorithm is unknown, and veracity of signature could not be established.
		/// </returns>
		public bool? ValidateSignature(LegalIdentity Identity, Stream Data, byte[] Signature)
		{
			if (Identity.ClientKeyName.StartsWith("RSA") &&
				int.TryParse(Identity.ClientKeyName[3..], out int KeySize))
			{
				return RsaEndpoint.Verify(Data, Signature, KeySize, Identity.ClientPubKey);
			}
			else if (EndpointSecurity.TryGetEndpoint(Identity.ClientKeyName,
				Identity.Namespace.Replace(":iot:leg:id:", ":iot:e2e:").Replace("urn:ieee:", "urn:nf:"),
				out IE2eEndpoint LocalKey) &&
				LocalKey is EllipticCurveEndpoint LocalEc)
			{
				return LocalEc.Verify(Data, Identity.ClientPubKey, Signature);
			}
			else
				return null;
		}

		private async Task ReturnStatus(IdentityStatus Status, EventHandlerAsync<IdentityValidationEventArgs> Callback, object State)
		{
			await Callback.Raise(this, new IdentityValidationEventArgs(Status, State));
		}

		/// <summary>
		/// Validates a legal identity.
		/// </summary>
		/// <param name="Identity">Legal identity to validate</param>
		/// <returns>Status of validation.</returns>
		public Task<IdentityStatus> ValidateAsync(LegalIdentity Identity)
		{
			return this.ValidateAsync(Identity, true);
		}

		/// <summary>
		/// Validates a legal identity.
		/// </summary>
		/// <param name="Identity">Legal identity to validate</param>
		/// <param name="ValidateState">If the state attribute should be validated. (Default=true)</param>
		/// <returns>Status of validation.</returns>
		public async Task<IdentityStatus> ValidateAsync(LegalIdentity Identity, bool ValidateState)
		{
			TaskCompletionSource<IdentityStatus> Result = new TaskCompletionSource<IdentityStatus>();

			await this.Validate(Identity, ValidateState, (Sender, e) =>
			{
				Result.TrySetResult(e.Status);
				return Task.CompletedTask;
			}, null);

			return await Result.Task;
		}

		#endregion

		#region Legal Identity update event

		private bool IsFromTrustProvider(string Id, string From)
		{
			int i = Id.IndexOf('@');
			if (i < 0)
				return false;

			Id = Id[(i + 1)..];

			i = From.IndexOf('@');
			if (i >= 0)
				return false;

			return (string.Compare(Id, From, true) == 0 ||
				From.EndsWith("." + Id, StringComparison.CurrentCultureIgnoreCase));
		}

		private async Task IdentityMessageHandler(object Sender, MessageEventArgs e)
		{
			LegalIdentity Identity = LegalIdentity.Parse(e.Content);

			if (!this.IsFromTrustProvider(Identity.Id, e.From))
			{
				this.client.Warning("Incoming identity message discarded: " + Identity.Id + " not from " + e.From + ".");
				return;
			}

			if (string.Compare(e.FromBareJID, Identity.Provider, true) != 0)
			{
				this.client.Warning("Incoming identity message discarded: Sender " + e.FromBareJID + " not equal to Trust Provider " + Identity.Provider + ".");
				return;
			}

			await this.Validate(Identity, false, async (sender2, e2) =>
			{
				if (e2.Status != IdentityStatus.Valid)
				{
					this.client.Warning("Invalid legal identity received and discarded. Validation status: " + e2.Status.ToString());

					Log.Warning("Invalid legal identity received and discarded.", this.client.BareJID, e.From,
						new KeyValuePair<string, object>("Status", e2.Status));

					return;
				}

				await this.UpdateSettings(Identity);
				await this.IdentityUpdated.Raise(this, new LegalIdentityEventArgs(new IqResultEventArgs(e.Message, e.Id, e.To, e.From, e.Ok, null), Identity));

			}, null);
		}

		private Task UpdateSettings(LegalIdentity Identity)
		{
			return this.UpdateSettings(Identity, null);
		}

		/// <summary>
		/// Checks if the private key of a legal identity is available. Private keys are required to be able to
		/// sign data, petitions and contracts.
		/// </summary>
		/// <param name="Identity">Legal Identity.</param>
		/// <returns>If the private key of the legal identity is available (and belongs to the Bare JID of the client).</returns>
		public Task<bool> HasPrivateKey(LegalIdentity Identity)
		{
			return this.HasPrivateKey(Identity.Id);
		}

		/// <summary>
		/// Checks if the private key of a legal identity is available. Private keys are required to be able to
		/// sign data, petitions and contracts.
		/// </summary>
		/// <param name="IdentityId">Identity string of Legal Identity.</param>
		/// <returns>If the private key of the legal identity is available (and belongs to the Bare JID of the client).</returns>
		public async Task<bool> HasPrivateKey(string IdentityId)
		{
			LegalIdentityState State = await Database.FindFirstIgnoreRest<LegalIdentityState>(new FilterAnd(
				new FilterFieldEqualTo("BareJid", this.client.BareJID),
				new FilterFieldEqualTo("LegalId", IdentityId)));

			if (State?.PublicKey is null)
				return false;

			IE2eEndpoint Endpoint = this.LocalEndpoint.FindLocalEndpoint(State.PublicKey);

			return !(Endpoint is null);
		}

		/// <summary>
		/// Gets the latest approved Legal ID.
		/// </summary>
		/// <returns>Legal ID, if found, null otherwise.</returns>
		public Task<string> GetLatestApprovedLegalId()
		{
			return this.GetLatestApprovedLegalId(null);
		}

		/// <summary>
		/// Gets the (latest) approved Legal ID whose public key matches <paramref name="PublicKey"/>.
		/// </summary>
		/// <param name="PublicKey">Public key</param>
		/// <returns>Legal ID, if found, null otherwise.</returns>
		public async Task<string> GetLatestApprovedLegalId(byte[] PublicKey)
		{
			string PublicKeyBase64 = PublicKey is null ? string.Empty : Convert.ToBase64String(PublicKey);

			foreach (LegalIdentityState State in await Database.Find<LegalIdentityState>(new FilterAnd(
				new FilterFieldEqualTo("BareJid", this.client.BareJID),
				new FilterFieldEqualTo("State", IdentityState.Approved)), "-Timestamp"))
			{
				if (!(PublicKey is null) && Convert.ToBase64String(State.PublicKey) != PublicKeyBase64)
					continue;

				IE2eEndpoint Endpoint = this.LocalEndpoint.FindLocalEndpoint(State.PublicKey);
				if (Endpoint is null)
					continue;

				return State.LegalId;
			}

			return null;
		}

		private async Task<IE2eEndpoint> GetLatestApprovedKey(bool ExceptionIfNone)
		{
			bool HaveStates = false;

			foreach (LegalIdentityState State in await Database.Find<LegalIdentityState>(new FilterAnd(
				new FilterFieldEqualTo("BareJid", this.client.BareJID),
				new FilterFieldEqualTo("State", IdentityState.Approved)), "-Timestamp"))
			{
				HaveStates = true;

				IE2eEndpoint Endpoint = this.LocalEndpoint.FindLocalEndpoint(State.PublicKey);
				if (Endpoint is null)
					continue;

				return Endpoint;
			}

			if (ExceptionIfNone)
			{
				if (HaveStates)
				{
					throw new Exception("Private keys are not available on this device (" + this.client.BareJID +
						"). Were they created on another device?");
				}
				else
					throw new Exception("No approved legal identity available on this device (" + this.client.BareJID + ").");
			}

			return null;
		}

		private async Task UpdateSettings(LegalIdentity Identity, byte[] PublicKey)
		{
			if (!string.IsNullOrEmpty(Identity.Id))
			{
				LegalIdentityState StateObj = Types.Instantiate<LegalIdentityState>(false, Identity.Id);

				if (string.IsNullOrEmpty(StateObj.ObjectId))
				{
					LegalIdentityState StateObj2 = await Database.FindFirstDeleteRest<LegalIdentityState>(new FilterAnd(
						new FilterFieldEqualTo("BareJid", this.client.BareJID),
						new FilterFieldEqualTo("LegalId", Identity.Id)));

					if (StateObj2 is null)
						StateObj.BareJid = this.client.BareJID;
					else
					{
						Types.ReplaceSingleton(StateObj2, Identity.Id);
						StateObj = StateObj2;
					}
				}

				DateTime Timestamp = Identity.Updated > Identity.Created ? Identity.Updated : Identity.Created;

				if (Timestamp > StateObj.Timestamp ||
					(StateObj.PublicKey is null && !(PublicKey is null)) ||
					Identity.State > StateObj.State)
				{
					StateObj.State = Identity.State;
					StateObj.Timestamp = Timestamp;

					if (PublicKey is null)
					{
						switch (Identity.State)
						{
							case IdentityState.Compromised:
							case IdentityState.Obsoleted:
							case IdentityState.Rejected:
								StateObj.PublicKey = null;
								break;
						}
					}
					else
						StateObj.PublicKey = PublicKey;

					if (string.IsNullOrEmpty(StateObj.ObjectId))
						await Database.Insert(StateObj);
					else
						await Database.Update(StateObj);
				}
			}
		}

		/// <summary>
		/// Event raised whenever the legal identity has been updated by the server.
		/// The identity is validated before the event is raised. Invalid identities are discarded.
		/// </summary>
		public event EventHandlerAsync<LegalIdentityEventArgs> IdentityUpdated = null;

		#endregion

		#region Get Legal Identities

		/// <summary>
		/// Gets legal identities registered with the account.
		/// </summary>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public Task GetLegalIdentities(EventHandlerAsync<LegalIdentitiesEventArgs> Callback, object State)
		{
			return this.GetLegalIdentities(this.componentAddress, Callback, State);
		}

		/// <summary>
		/// Gets legal identities registered with the account.
		/// </summary>
		/// <param name="Address">Address of entity on which the legal identities are registered.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public Task GetLegalIdentities(string Address, EventHandlerAsync<LegalIdentitiesEventArgs> Callback, object State)
		{
			return this.client.SendIqGet(Address, "<getLegalIdentities xmlns=\"" + NamespaceLegalIdentitiesCurrent + "\"/>",
				this.IdentitiesResponse, new object[] { Callback, State });
		}

		private async Task IdentitiesResponse(object Sender, IqResultEventArgs e)
		{
			object[] P = (object[])e.State;
			EventHandlerAsync<LegalIdentitiesEventArgs> Callback = (EventHandlerAsync<LegalIdentitiesEventArgs>)P[0];
			LegalIdentity[] Identities = null;
			XmlElement E;

			if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "identities")
			{
				List<LegalIdentity> IdentitiesList = new List<LegalIdentity>();

				foreach (XmlNode N in E.ChildNodes)
				{
					if (N is XmlElement E2 && E2.LocalName == "identity")
						IdentitiesList.Add(LegalIdentity.Parse(E2));
				}

				Identities = IdentitiesList.ToArray();
			}
			else
				e.Ok = false;

			e.State = P[1];
			await Callback.Raise(this, new LegalIdentitiesEventArgs(e, Identities));
		}

		/// <summary>
		/// Gets legal identities registered with the account.
		/// </summary>
		/// <returns>Set of legal identities registered on the account.</returns>
		public Task<LegalIdentity[]> GetLegalIdentitiesAsync()
		{
			return this.GetLegalIdentitiesAsync(this.componentAddress);
		}

		/// <summary>
		/// Gets legal identities registered with the account.
		/// </summary>
		/// <param name="Address">Address of entity on which the legal identities are registered.</param>
		/// <returns>Set of legal identities registered on the account.</returns>
		public async Task<LegalIdentity[]> GetLegalIdentitiesAsync(string Address)
		{
			TaskCompletionSource<LegalIdentity[]> Result = new TaskCompletionSource<LegalIdentity[]>();

			await this.GetLegalIdentities(Address, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Identities);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to get legal identities."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		#endregion

		#region Get Legal Identity

		/// <summary>
		/// Gets information about a legal identity given its ID.
		/// </summary>
		/// <param name="LegalIdentityId">ID of the legal identity to get.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public Task GetLegalIdentity(string LegalIdentityId, EventHandlerAsync<LegalIdentityEventArgs> Callback, object State)
		{
			return this.GetLegalIdentity(this.GetTrustProvider(LegalIdentityId), LegalIdentityId, Callback, State);
		}

		/// <summary>
		/// Gets the trust provider hosting an entity with a given ID, in the form of LocalId@Provider.
		/// </summary>
		/// <param name="EntityId">ID, in the form of LocalId@Provider.</param>
		/// <returns>Provider</returns>
		public string GetTrustProvider(string EntityId)
		{
			int i = EntityId.IndexOf('@');
			if (i < 0)
				return this.componentAddress;
			else
				return EntityId[(i + 1)..];
		}

		/// <summary>
		/// Gets information about a legal identity given its ID.
		/// </summary>
		/// <param name="Address">Address of entity on which the legal identity are registered.</param>
		/// <param name="LegalIdentityId">ID of the legal identity to get.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public Task GetLegalIdentity(string Address, string LegalIdentityId, EventHandlerAsync<LegalIdentityEventArgs> Callback, object State)
		{
			return this.client.SendIqGet(Address, "<getLegalIdentity id=\"" + XML.Encode(LegalIdentityId) + "\" xmlns=\"" +
				NamespaceLegalIdentitiesCurrent + "\"/>", async (Sender, e) =>
			{
				LegalIdentity Identity = null;
				XmlElement E;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "identity")
					Identity = LegalIdentity.Parse(E);
				else
					e.Ok = false;

				await Callback.Raise(this, new LegalIdentityEventArgs(e, Identity));
			}, State);
		}

		/// <summary>
		/// Gets legal identity registered with the account.
		/// </summary>
		/// <param name="LegalIdentityId">ID of the legal identity to get.</param>
		/// <returns>Legal identity object corresponding to <paramref name="LegalIdentityId"/>.</returns>
		public Task<LegalIdentity> GetLegalIdentityAsync(string LegalIdentityId)
		{
			return this.GetLegalIdentityAsync(this.GetTrustProvider(LegalIdentityId), LegalIdentityId);
		}

		/// <summary>
		/// Gets legal identity registered with the account.
		/// </summary>
		/// <param name="Address">Address of entity on which the legal identity are registered.</param>
		/// <param name="LegalIdentityId">ID of the legal identity to get.</param>
		/// <returns>Legal identity object corresponding to <paramref name="LegalIdentityId"/>.</returns>
		public async Task<LegalIdentity> GetLegalIdentityAsync(string Address, string LegalIdentityId)
		{
			TaskCompletionSource<LegalIdentity> Result = new TaskCompletionSource<LegalIdentity>();

			await this.GetLegalIdentity(Address, LegalIdentityId, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Identity);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to get legal identity."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		#endregion

		#region Obsolete Legal Identity

		/// <summary>
		/// Obsoletes one of the legal identities of the account, given its ID.
		/// </summary>
		/// <param name="LegalIdentityId">ID of the legal identity to obsolete.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public Task ObsoleteLegalIdentity(string LegalIdentityId, EventHandlerAsync<LegalIdentityEventArgs> Callback, object State)
		{
			return this.ObsoleteLegalIdentity(this.GetTrustProvider(LegalIdentityId), LegalIdentityId, Callback, State);
		}

		/// <summary>
		/// Obsoletes one of the legal identities of the account, given its ID.
		/// </summary>
		/// <param name="Address">Address of entity on which the legal identity are registered.</param>
		/// <param name="LegalIdentityId">ID of the legal identity to obsolete.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public Task ObsoleteLegalIdentity(string Address, string LegalIdentityId, EventHandlerAsync<LegalIdentityEventArgs> Callback, object State)
		{
			this.AssertAllowed();

			return this.client.SendIqSet(Address, "<obsoleteLegalIdentity id=\"" + XML.Encode(LegalIdentityId) + "\" xmlns=\"" +
				NamespaceLegalIdentitiesCurrent + "\"/>", async (Sender, e) =>
				{
					LegalIdentity Identity = null;
					XmlElement E;

					if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "identity")
					{
						Identity = LegalIdentity.Parse(E);
						await this.UpdateSettings(Identity);
					}
					else
						e.Ok = false;

					await Callback.Raise(this, new LegalIdentityEventArgs(e, Identity));
				}, State);
		}

		/// <summary>
		/// Obsoletes one of the legal identities of the account, given its ID.
		/// </summary>
		/// <param name="LegalIdentityId">ID of the legal identity to obsolete.</param>
		/// <returns>Legal identity object corresponding to <paramref name="LegalIdentityId"/>.</returns>
		public Task<LegalIdentity> ObsoleteLegalIdentityAsync(string LegalIdentityId)
		{
			return this.ObsoleteLegalIdentityAsync(this.GetTrustProvider(LegalIdentityId), LegalIdentityId);
		}

		/// <summary>
		/// Obsoletes one of the legal identities of the account, given its ID.
		/// </summary>
		/// <param name="Address">Address of entity on which the legal identity are registered.</param>
		/// <param name="LegalIdentityId">ID of the legal identity to obsolete.</param>
		/// <returns>Legal identity object corresponding to <paramref name="LegalIdentityId"/>.</returns>
		public async Task<LegalIdentity> ObsoleteLegalIdentityAsync(string Address, string LegalIdentityId)
		{
			TaskCompletionSource<LegalIdentity> Result = new TaskCompletionSource<LegalIdentity>();

			await this.ObsoleteLegalIdentity(Address, LegalIdentityId, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Identity);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to obsolete legal identity."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		#endregion

		#region Compromised Legal Identity

		/// <summary>
		/// Reports as Compromised one of the legal identities of the account, given its ID.
		/// </summary>
		/// <param name="LegalIdentityId">ID of the legal identity to compromise.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public Task CompromisedLegalIdentity(string LegalIdentityId, EventHandlerAsync<LegalIdentityEventArgs> Callback, object State)
		{
			return this.CompromisedLegalIdentity(this.GetTrustProvider(LegalIdentityId), LegalIdentityId, Callback, State);
		}

		/// <summary>
		/// Reports as Compromised one of the legal identities of the account, given its ID.
		/// </summary>
		/// <param name="Address">Address of entity on which the legal identity are registered.</param>
		/// <param name="LegalIdentityId">ID of the legal identity to compromise.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public Task CompromisedLegalIdentity(string Address, string LegalIdentityId, EventHandlerAsync<LegalIdentityEventArgs> Callback, object State)
		{
			this.AssertAllowed();

			return this.client.SendIqSet(Address, "<compromisedLegalIdentity id=\"" + XML.Encode(LegalIdentityId) + "\" xmlns=\"" +
				NamespaceLegalIdentitiesCurrent + "\"/>", async (Sender, e) =>
				{
					LegalIdentity Identity = null;
					XmlElement E;

					if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "identity")
					{
						Identity = LegalIdentity.Parse(E);
						await this.UpdateSettings(Identity);
					}
					else
						e.Ok = false;

					await Callback.Raise(this, new LegalIdentityEventArgs(e, Identity));
				}, State);
		}

		/// <summary>
		/// Reports as Compromised one of the legal identities of the account, given its ID.
		/// </summary>
		/// <param name="LegalIdentityId">ID of the legal identity to compromise.</param>
		/// <returns>Legal identity object corresponding to <paramref name="LegalIdentityId"/>.</returns>
		public Task<LegalIdentity> CompromisedLegalIdentityAsync(string LegalIdentityId)
		{
			return this.CompromisedLegalIdentityAsync(this.GetTrustProvider(LegalIdentityId), LegalIdentityId);
		}

		/// <summary>
		/// Reports as Compromised one of the legal identities of the account, given its ID.
		/// </summary>
		/// <param name="Address">Address of entity on which the legal identity are registered.</param>
		/// <param name="LegalIdentityId">ID of the legal identity to compromise.</param>
		/// <returns>Legal identity object corresponding to <paramref name="LegalIdentityId"/>.</returns>
		public async Task<LegalIdentity> CompromisedLegalIdentityAsync(string Address, string LegalIdentityId)
		{
			TaskCompletionSource<LegalIdentity> Result = new TaskCompletionSource<LegalIdentity>();

			await this.CompromisedLegalIdentity(Address, LegalIdentityId, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Identity);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to compromise legal identity."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		#endregion

		#region Signatures

		/// <summary>
		/// Signs binary data with the corresponding private key.
		/// </summary>
		/// <param name="Data">Binary data to sign.</param>
		/// <param name="SignWith">What keys that can be used to sign the data.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public Task Sign(byte[] Data, SignWith SignWith, EventHandlerAsync<SignatureEventArgs> Callback, object State)
		{
			return this.Sign(this.componentAddress, Data, SignWith, Callback, State);
		}

		/// <summary>
		/// Signs binary data with the corresponding private key.
		/// </summary>
		/// <param name="Address">Address of entity on which the legal identity are registered.</param>
		/// <param name="Data">Binary data to sign.</param>
		/// <param name="SignWith">What keys that can be used to sign the data.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public async Task Sign(string Address, byte[] Data, SignWith SignWith, EventHandlerAsync<SignatureEventArgs> Callback, object State)
		{
			this.AssertAllowed();

			byte[] Signature = null;
			IE2eEndpoint Key = SignWith switch
			{
				SignWith.CurrentKeys => null,
				SignWith.LatestApprovedId => await this.GetLatestApprovedKey(true),
				_ => await this.GetLatestApprovedKey(false),
			};

			if (Key is null)
			{
				await this.GetMatchingLocalKey(Address, async (Sender, e) =>
				{
					if (e.Ok)
						Signature = e.Key.Sign(Data);

					await Callback.Raise(this, new SignatureEventArgs(e, Signature));

				}, State);
			}
			else
			{
				Signature = Key.Sign(Data);

				await Callback.Raise(this, new SignatureEventArgs(Key, Signature, State));
			}
		}

		/// <summary>
		/// Signs binary data with the corresponding private key.
		/// </summary>
		/// <param name="Data">Binary data to sign.</param>
		/// <param name="SignWith">What keys that can be used to sign the data.</param>
		/// <returns>Digital signature.</returns>
		public Task<byte[]> SignAsync(byte[] Data, SignWith SignWith)
		{
			return this.SignAsync(this.componentAddress, Data, SignWith);
		}

		/// <summary>
		/// Signs binary data with the corresponding private key.
		/// </summary>
		/// <param name="Address">Address of entity on which the legal identity are registered.</param>
		/// <param name="Data">Binary data to sign.</param>
		/// <param name="SignWith">What keys that can be used to sign the data.</param>
		/// <returns>Digital signature.</returns>
		public async Task<byte[]> SignAsync(string Address, byte[] Data, SignWith SignWith)
		{
			TaskCompletionSource<byte[]> Result = new TaskCompletionSource<byte[]>();

			await this.Sign(Address, Data, SignWith, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Signature);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to sign data."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		/// <summary>
		/// Signs binary data with the corresponding private key.
		/// </summary>
		/// <param name="Data">Binary data to sign.</param>
		/// <param name="SignWith">What keys that can be used to sign the data.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public Task Sign(Stream Data, SignWith SignWith, EventHandlerAsync<SignatureEventArgs> Callback, object State)
		{
			return this.Sign(this.componentAddress, Data, SignWith, Callback, State);
		}

		/// <summary>
		/// Signs binary data with the corresponding private key.
		/// </summary>
		/// <param name="Address">Address of entity on which the legal identity are registered.</param>
		/// <param name="Data">Binary data to sign.</param>
		/// <param name="SignWith">What keys that can be used to sign the data.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public async Task Sign(string Address, Stream Data, SignWith SignWith, EventHandlerAsync<SignatureEventArgs> Callback, object State)
		{
			this.AssertAllowed();

			IE2eEndpoint Key = SignWith == SignWith.CurrentKeys ? null : await this.GetLatestApprovedKey(true);
			byte[] Signature = null;

			if (Key is null)
			{
				await this.GetMatchingLocalKey(Address, async (Sender, e) =>
				{
					if (e.Ok)
						Signature = e.Key.Sign(Data);

					await Callback.Raise(this, new SignatureEventArgs(e, Signature));

				}, State);
			}
			else
			{
				Signature = Key.Sign(Data);

				await Callback.Raise(this, new SignatureEventArgs(Key, Signature, State));
			}
		}

		/// <summary>
		/// Signs binary data with the corresponding private key.
		/// </summary>
		/// <param name="Data">Binary data to sign.</param>
		/// <param name="SignWith">What keys that can be used to sign the data.</param>
		/// <returns>Digital signature.</returns>
		public Task<byte[]> SignAsync(Stream Data, SignWith SignWith)
		{
			return this.SignAsync(this.componentAddress, Data, SignWith);
		}

		/// <summary>
		/// Signs binary data with the corresponding private key.
		/// </summary>
		/// <param name="Address">Address of entity on which the legal identity are registered.</param>
		/// <param name="Data">Binary data to sign.</param>
		/// <param name="SignWith">What keys that can be used to sign the data.</param>
		/// <returns>Digital signature.</returns>
		public async Task<byte[]> SignAsync(string Address, Stream Data, SignWith SignWith)
		{
			TaskCompletionSource<byte[]> Result = new TaskCompletionSource<byte[]>();

			await this.Sign(Address, Data, SignWith, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Signature);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to sign data."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		#endregion

		#region Validating Signatures

		/// <summary>
		/// Validates a signature of binary data.
		/// </summary>
		/// <param name="LegalId">Legal identity used to create the signature. If empty, current approved legal identities will be used to validate the signature.</param>
		/// <param name="Data">Binary data to sign.</param>
		/// <param name="Signature">Digital signature of data</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public Task ValidateSignature(string LegalId, byte[] Data, byte[] Signature, EventHandlerAsync<LegalIdentityEventArgs> Callback, object State)
		{
			return this.ValidateSignature(this.GetTrustProvider(LegalId), LegalId, Data, Signature, Callback, State);
		}

		/// <summary>
		/// Validates a signature of binary data.
		/// </summary>
		/// <param name="Address">Address of entity on which the legal identity are registered.</param>
		/// <param name="LegalId">Legal identity used to create the signature. If empty, current approved legal identities will be used to validate the signature.</param>
		/// <param name="Data">Binary data to sign.</param>
		/// <param name="Signature">Digital signature of data</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public Task ValidateSignature(string Address, string LegalId, byte[] Data, byte[] Signature, EventHandlerAsync<LegalIdentityEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<validateSignature data=\"");
			Xml.Append(Convert.ToBase64String(Data));

			if (!string.IsNullOrEmpty(LegalId))
			{
				Xml.Append("\" id=\"");
				Xml.Append(XML.Encode(LegalId));
			}

			Xml.Append("\" s=\"");
			Xml.Append(Convert.ToBase64String(Signature));

			Xml.Append("\" xmlns=\"");
			Xml.Append(NamespaceLegalIdentitiesCurrent);
			Xml.Append("\"/>");

			return this.client.SendIqGet(Address, Xml.ToString(), async (Sender, e) =>
			{
				LegalIdentity Identity = null;
				XmlElement E;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "identity")
					Identity = LegalIdentity.Parse(E);
				else
					e.Ok = false;

				await Callback.Raise(this, new LegalIdentityEventArgs(e, Identity));
			}, State);
		}

		/// <summary>
		/// Validates a signature of binary data.
		/// </summary>
		/// <param name="LegalId">Legal identity used to create the signature. If empty, current approved legal identities will be used to validate the signature.</param>
		/// <param name="Data">Binary data to sign.</param>
		/// <param name="Signature">Digital signature of data</param>
		/// <returns>Legal identity object.</returns>
		public Task<LegalIdentity> ValidateSignatureAsync(string LegalId, byte[] Data, byte[] Signature)
		{
			return this.ValidateSignatureAsync(this.GetTrustProvider(LegalId), LegalId, Data, Signature);
		}

		/// <summary>
		/// Validates a signature of binary data.
		/// </summary>
		/// <param name="Address">Address of entity on which the legal identity are registered.</param>
		/// <param name="LegalId">Legal identity used to create the signature. If empty, current approved legal identities will be used to validate the signature.</param>
		/// <param name="Data">Binary data to sign.</param>
		/// <param name="Signature">Digital signature of data</param>
		/// <returns>Legal identity object.</returns>
		public async Task<LegalIdentity> ValidateSignatureAsync(string Address, string LegalId, byte[] Data, byte[] Signature)
		{
			TaskCompletionSource<LegalIdentity> Result = new TaskCompletionSource<LegalIdentity>();

			await this.ValidateSignature(Address, LegalId, Data, Signature, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Identity);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to sign data."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		#endregion

		#region Create Contract

		/// <summary>
		/// Creates a new contract.
		/// </summary>
		/// <param name="ForMachines">Machine-readable content.</param>
		/// <param name="ForHumans">Human-readable localized content. Provide one object for each language supported by the contract.</param>
		/// <param name="Roles">Roles defined in contract.</param>
		/// <param name="Parts">Parts defined in contract. Can be empty or null, if creating an open contract or a template.</param>
		/// <param name="Parameters">Any contractual parameters defined for the contract.</param>
		/// <param name="Visibility">Visibility of the contract.</param>
		/// <param name="PartsMode">How parts are defined in the contract. If equal to <see cref="ContractParts.ExplicitlyDefined"/>,
		/// then the explicitly defined parts must be provided in <paramref name="Parts"/>.</param>
		/// <param name="Duration">Duration of the contract, once signed.</param>
		/// <param name="ArchiveRequired">Required archivation duration, after signed contract has become obsolete.</param>
		/// <param name="ArchiveOptional">Optional archivation duration, after required archivation duration has elapsed.</param>
		/// <param name="SignAfter">Signatures will only be accepted after this point in time, if provided.</param>
		/// <param name="SignBefore">Signatures will only be accepted until this point in time, if provided.</param>
		/// <param name="CanActAsTemplate">If the contract can act as a template.</param>
		/// <param name="Callback">Method to call when registration response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task CreateContract(XmlElement ForMachines, HumanReadableText[] ForHumans, Role[] Roles,
			Part[] Parts, Parameter[] Parameters, ContractVisibility Visibility, ContractParts PartsMode, Duration? Duration,
			Duration? ArchiveRequired, Duration? ArchiveOptional, DateTime? SignAfter, DateTime? SignBefore, bool CanActAsTemplate,
			EventHandlerAsync<SmartContractEventArgs> Callback, object State)
		{
			return this.CreateContract(this.componentAddress, ForMachines, ForHumans, Roles, Parts, Parameters, Visibility, PartsMode,
				Duration, ArchiveRequired, ArchiveOptional, SignAfter, SignBefore, CanActAsTemplate, Callback, State);
		}

		/// <summary>
		/// Creates a new contract.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ForMachines">Machine-readable content.</param>
		/// <param name="ForHumans">Human-readable localized content. Provide one object for each language supported by the contract.</param>
		/// <param name="Roles">Roles defined in contract.</param>
		/// <param name="Parts">Parts defined in contract. Can be empty or null, if creating an open contract or a template.</param>
		/// <param name="Parameters">Any contractual parameters defined for the contract.</param>
		/// <param name="Visibility">Visibility of the contract.</param>
		/// <param name="PartsMode">How parts are defined in the contract. If equal to <see cref="ContractParts.ExplicitlyDefined"/>,
		/// then the explicitly defined parts must be provided in <paramref name="Parts"/>.</param>
		/// <param name="Duration">Duration of the contract, once signed.</param>
		/// <param name="ArchiveRequired">Required archivation duration, after signed contract has become obsolete.</param>
		/// <param name="ArchiveOptional">Optional archivation duration, after required archivation duration has elapsed.</param>
		/// <param name="SignAfter">Signatures will only be accepted after this point in time, if provided.</param>
		/// <param name="SignBefore">Signatures will only be accepted until this point in time, if provided.</param>
		/// <param name="CanActAsTemplate">If the contract can act as a template.</param>
		/// <param name="Callback">Method to call when registration response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task CreateContract(string Address, XmlElement ForMachines, HumanReadableText[] ForHumans, Role[] Roles,
			Part[] Parts, Parameter[] Parameters, ContractVisibility Visibility, ContractParts PartsMode, Duration? Duration,
			Duration? ArchiveRequired, Duration? ArchiveOptional, DateTime? SignAfter, DateTime? SignBefore, bool CanActAsTemplate,
			EventHandlerAsync<SmartContractEventArgs> Callback, object State)
		{
			return this.CreateContract(Address, ForMachines, ForHumans, Roles, Parts, Parameters,
				Visibility, PartsMode, Duration, ArchiveRequired, ArchiveOptional, SignAfter,
				SignBefore, CanActAsTemplate, null, Callback, State);
		}

		/// <summary>
		/// Creates a new contract.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ForMachines">Machine-readable content.</param>
		/// <param name="ForHumans">Human-readable localized content. Provide one object for each language supported by the contract.</param>
		/// <param name="Roles">Roles defined in contract.</param>
		/// <param name="Parts">Parts defined in contract. Can be empty or null, if creating an open contract or a template.</param>
		/// <param name="Parameters">Any contractual parameters defined for the contract.</param>
		/// <param name="Visibility">Visibility of the contract.</param>
		/// <param name="PartsMode">How parts are defined in the contract. If equal to <see cref="ContractParts.ExplicitlyDefined"/>,
		/// then the explicitly defined parts must be provided in <paramref name="Parts"/>.</param>
		/// <param name="Duration">Duration of the contract, once signed.</param>
		/// <param name="ArchiveRequired">Required archivation duration, after signed contract has become obsolete.</param>
		/// <param name="ArchiveOptional">Optional archivation duration, after required archivation duration has elapsed.</param>
		/// <param name="SignAfter">Signatures will only be accepted after this point in time, if provided.</param>
		/// <param name="SignBefore">Signatures will only be accepted until this point in time, if provided.</param>
		/// <param name="CanActAsTemplate">If the contract can act as a template.</param>
		/// <param name="Algorithm">Algorithm to use for encrypting values.</param>
		/// <param name="Callback">Method to call when registration response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public async Task CreateContract(string Address, XmlElement ForMachines, HumanReadableText[] ForHumans, Role[] Roles,
			Part[] Parts, Parameter[] Parameters, ContractVisibility Visibility, ContractParts PartsMode, Duration? Duration,
			Duration? ArchiveRequired, Duration? ArchiveOptional, DateTime? SignAfter, DateTime? SignBefore, bool CanActAsTemplate,
			IParameterEncryptionAlgorithm Algorithm, EventHandlerAsync<SmartContractEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<createContract xmlns=\"");
			Xml.Append(NamespaceSmartContractsCurrent);
			Xml.Append("\">");

			Contract Contract = new Contract()
			{
				Namespace = NamespaceSmartContractsCurrent,
				ForMachines = ForMachines,
				ForHumans = ForHumans,
				Roles = Roles,
				Parts = Parts,
				Parameters = Parameters,
				Visibility = Visibility,
				PartsMode = PartsMode,
				Duration = Duration,
				ArchiveRequired = ArchiveRequired,
				ArchiveOptional = ArchiveOptional,
				SignAfter = SignAfter,
				SignBefore = SignBefore,
				CanActAsTemplate = CanActAsTemplate
			};

			byte[] Nonce = Guid.NewGuid().ToByteArray();
			string NonceStr = Convert.ToBase64String(Nonce);
			SymmetricCipherAlgorithms EncryptionAlgorithm = Algorithm?.Algorithm ?? this.preferredEncryptionAlgorithm;

			if (Contract.HasEncryptedParameters)
			{
				Algorithm ??= await ParameterEncryptionAlgorithm.Create(EncryptionAlgorithm, this);

				Contract.EncryptEncryptedParameters(this.client.BareJID, Algorithm);
			}

			Contract.Serialize(Xml, false, false, false, false, false, false, false);

			if (Contract.HasTransientParameters)
			{
				Xml.Append("<transient>");

				foreach (Parameter Parameter in Contract.Parameters)
				{
					if (Parameter.Protection == ProtectionLevel.Transient)
					{
						Parameter.Protection = ProtectionLevel.Normal;
						Parameter.Serialize(Xml, true);
						Parameter.Protection = ProtectionLevel.Transient;
					}
				}

				Xml.Append("</transient>");
			}

			Xml.Append("</createContract>");

			await this.client.SendIqSet(Address, Xml.ToString(), this.ContractResponse, new object[] { Callback, State, Contract.HasEncryptedParameters, Algorithm?.Algorithm, Algorithm?.Key });
		}

		private async Task ContractResponse(object Sender, IqResultEventArgs e)
		{
			object[] P = (object[])e.State;
			EventHandlerAsync<SmartContractEventArgs> Callback = (EventHandlerAsync<SmartContractEventArgs>)P[0];
			Contract Contract = null;
			XmlElement E;

			if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "contract")
			{
				ParsedContract Parsed = await Contract.Parse(E, this, false);
				Contract = Parsed?.Contract;
				if (Contract is null)
					e.Ok = false;
				else if (Contract.HasEncryptedParameters)
				{
					string CreatorJid = this.client.BareJID;

					if (P.Length >= 5 &&
						P[2] is bool HasEncryptedParameters &&
						HasEncryptedParameters &&
						P[3] is SymmetricCipherAlgorithms Algorithm &&
						P[4] is byte[] Key)
					{
						await this.SaveContractSharedSecret(Contract.ContractId,
							CreatorJid, Key, Algorithm, false);
					}
					else
					{
						Tuple<SymmetricCipherAlgorithms, string, byte[]> T = await this.TryLoadContractSharedSecret(Contract.ContractId);

						if (HasEncryptedParameters = !(T is null))
						{
							Algorithm = T.Item1;
							CreatorJid = T.Item2;
							Key = T.Item3;
						}
						else
						{
							Algorithm = this.preferredEncryptionAlgorithm;
							Key = null;
						}
					}

					if (HasEncryptedParameters)
					{
						IParameterEncryptionAlgorithm AlgorithmInstance = await ParameterEncryptionAlgorithm.Create(
							Contract.ContractId, Algorithm, this, CreatorJid, Key);

						Contract.DecryptEncryptedParameters(CreatorJid, AlgorithmInstance);
					}
				}
			}
			else
				e.Ok = false;

			e.State = P[1];
			await Callback.Raise(this, new SmartContractEventArgs(e, Contract));
		}

		/// <summary>
		/// Creates a new contract.
		/// </summary>
		/// <param name="ForMachines">Machine-readable content.</param>
		/// <param name="ForHumans">Human-readable localized content. Provide one object for each language supported by the contract.</param>
		/// <param name="Roles">Roles defined in contract.</param>
		/// <param name="Parts">Parts defined in contract. Can be empty or null, if creating an open contract or a template.</param>
		/// <param name="Parameters">Any contractual parameters defined for the contract.</param>
		/// <param name="Visibility">Visibility of the contract.</param>
		/// <param name="PartsMode">How parts are defined in the contract. If equal to <see cref="ContractParts.ExplicitlyDefined"/>,
		/// then the explicitly defined parts must be provided in <paramref name="Parts"/>.</param>
		/// <param name="Duration">Duration of the contract, once signed.</param>
		/// <param name="ArchiveRequired">Required archivation duration, after signed contract has become obsolete.</param>
		/// <param name="ArchiveOptional">Optional archivation duration, after required archivation duration has elapsed.</param>
		/// <param name="SignAfter">Signatures will only be accepted after this point in time, if provided.</param>
		/// <param name="SignBefore">Signatures will only be accepted until this point in time, if provided.</param>
		/// <param name="CanActAsTemplate">If the contract can act as a template.</param>
		/// <returns>Contract.</returns>
		public Task<Contract> CreateContractAsync(XmlElement ForMachines, HumanReadableText[] ForHumans, Role[] Roles,
			Part[] Parts, Parameter[] Parameters, ContractVisibility Visibility, ContractParts PartsMode, Duration? Duration,
			Duration? ArchiveRequired, Duration? ArchiveOptional, DateTime? SignAfter, DateTime? SignBefore, bool CanActAsTemplate)
		{
			return this.CreateContractAsync(this.componentAddress, ForMachines, ForHumans, Roles, Parts, Parameters, Visibility,
				PartsMode, Duration, ArchiveRequired, ArchiveOptional, SignAfter, SignBefore, CanActAsTemplate);
		}

		/// <summary>
		/// Creates a new contract.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ForMachines">Machine-readable content.</param>
		/// <param name="ForHumans">Human-readable localized content. Provide one object for each language supported by the contract.</param>
		/// <param name="Roles">Roles defined in contract.</param>
		/// <param name="Parts">Parts defined in contract. Can be empty or null, if creating an open contract or a template.</param>
		/// <param name="Parameters">Any contractual parameters defined for the contract.</param>
		/// <param name="Visibility">Visibility of the contract.</param>
		/// <param name="PartsMode">How parts are defined in the contract. If equal to <see cref="ContractParts.ExplicitlyDefined"/>,
		/// then the explicitly defined parts must be provided in <paramref name="Parts"/>.</param>
		/// <param name="Duration">Duration of the contract, once signed.</param>
		/// <param name="ArchiveRequired">Required archivation duration, after signed contract has become obsolete.</param>
		/// <param name="ArchiveOptional">Optional archivation duration, after required archivation duration has elapsed.</param>
		/// <param name="SignAfter">Signatures will only be accepted after this point in time, if provided.</param>
		/// <param name="SignBefore">Signatures will only be accepted until this point in time, if provided.</param>
		/// <param name="CanActAsTemplate">If the contract can act as a template.</param>
		/// <returns>Contract.</returns>
		public async Task<Contract> CreateContractAsync(string Address, XmlElement ForMachines, HumanReadableText[] ForHumans, Role[] Roles,
			Part[] Parts, Parameter[] Parameters, ContractVisibility Visibility, ContractParts PartsMode, Duration? Duration,
			Duration? ArchiveRequired, Duration? ArchiveOptional, DateTime? SignAfter, DateTime? SignBefore, bool CanActAsTemplate)
		{
			TaskCompletionSource<Contract> Result = new TaskCompletionSource<Contract>();

			await this.CreateContract(Address, ForMachines, ForHumans, Roles, Parts, Parameters, Visibility, PartsMode, Duration,
				ArchiveRequired, ArchiveOptional, SignAfter, SignBefore, CanActAsTemplate, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Contract);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to create the contract."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		#endregion

		#region Create Contract From Template

		/// <summary>
		/// Creates a new contract from a template.
		/// </summary>
		/// <param name="TemplateId">ID of contract to be used as a template.</param>
		/// <param name="Parts">Parts defined in contract. Can be empty or null, if creating an open contract or a template.</param>
		/// <param name="Parameters">Any contractual parameters defined for the contract.</param>
		/// <param name="Visibility">Visibility of the contract.</param>
		/// <param name="PartsMode">How parts are defined in the contract. If equal to <see cref="ContractParts.ExplicitlyDefined"/>,
		/// then the explicitly defined parts must be provided in <paramref name="Parts"/>.</param>
		/// <param name="Duration">Duration of the contract, once signed.</param>
		/// <param name="ArchiveRequired">Required archivation duration, after signed contract has become obsolete.</param>
		/// <param name="ArchiveOptional">Optional archivation duration, after required archivation duration has elapsed.</param>
		/// <param name="SignAfter">Signatures will only be accepted after this point in time, if provided.</param>
		/// <param name="SignBefore">Signatures will only be accepted until this point in time, if provided.</param>
		/// <param name="CanActAsTemplate">If the contract can act as a template.</param>
		/// <param name="Callback">Method to call when registration response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task CreateContract(string TemplateId, Part[] Parts, Parameter[] Parameters, ContractVisibility Visibility,
			ContractParts PartsMode, Duration? Duration, Duration? ArchiveRequired, Duration? ArchiveOptional, DateTime? SignAfter,
			DateTime? SignBefore, bool CanActAsTemplate, EventHandlerAsync<SmartContractEventArgs> Callback, object State)
		{
			return this.CreateContract(this.componentAddress, TemplateId, Parts, Parameters, Visibility, PartsMode, Duration,
				ArchiveRequired, ArchiveOptional, SignAfter, SignBefore, CanActAsTemplate, null, Callback, State);
		}

		/// <summary>
		/// Creates a new contract from a template.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="TemplateId">ID of contract to be used as a template.</param>
		/// <param name="Parts">Parts defined in contract. Can be empty or null, if creating an open contract or a template.</param>
		/// <param name="Parameters">Any contractual parameters defined for the contract.</param>
		/// <param name="Visibility">Visibility of the contract.</param>
		/// <param name="PartsMode">How parts are defined in the contract. If equal to <see cref="ContractParts.ExplicitlyDefined"/>,
		/// then the explicitly defined parts must be provided in <paramref name="Parts"/>.</param>
		/// <param name="Duration">Duration of the contract, once signed.</param>
		/// <param name="ArchiveRequired">Required archivation duration, after signed contract has become obsolete.</param>
		/// <param name="ArchiveOptional">Optional archivation duration, after required archivation duration has elapsed.</param>
		/// <param name="SignAfter">Signatures will only be accepted after this point in time, if provided.</param>
		/// <param name="SignBefore">Signatures will only be accepted until this point in time, if provided.</param>
		/// <param name="CanActAsTemplate">If the contract can act as a template.</param>
		/// <param name="Callback">Method to call when registration response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task CreateContract(string Address, string TemplateId, Part[] Parts, Parameter[] Parameters, ContractVisibility Visibility,
			ContractParts PartsMode, Duration? Duration, Duration? ArchiveRequired, Duration? ArchiveOptional, DateTime? SignAfter,
			DateTime? SignBefore, bool CanActAsTemplate, EventHandlerAsync<SmartContractEventArgs> Callback, object State)
		{
			return this.CreateContract(Address, TemplateId, Parts, Parameters, Visibility,
				PartsMode, Duration, ArchiveRequired, ArchiveOptional, SignAfter,
				SignBefore, CanActAsTemplate, null, Callback, State);
		}


		/// <summary>
		/// Creates a new contract from a template.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="TemplateId">ID of contract to be used as a template.</param>
		/// <param name="Parts">Parts defined in contract. Can be empty or null, if creating an open contract or a template.</param>
		/// <param name="Parameters">Any contractual parameters defined for the contract.</param>
		/// <param name="Visibility">Visibility of the contract.</param>
		/// <param name="PartsMode">How parts are defined in the contract. If equal to <see cref="ContractParts.ExplicitlyDefined"/>,
		/// then the explicitly defined parts must be provided in <paramref name="Parts"/>.</param>
		/// <param name="Duration">Duration of the contract, once signed.</param>
		/// <param name="ArchiveRequired">Required archivation duration, after signed contract has become obsolete.</param>
		/// <param name="ArchiveOptional">Optional archivation duration, after required archivation duration has elapsed.</param>
		/// <param name="SignAfter">Signatures will only be accepted after this point in time, if provided.</param>
		/// <param name="SignBefore">Signatures will only be accepted until this point in time, if provided.</param>
		/// <param name="CanActAsTemplate">If the contract can act as a template.</param>
		/// <param name="Algorithm">Algorithm to use for encrypting values.</param>
		/// <param name="Callback">Method to call when registration response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public async Task CreateContract(string Address, string TemplateId, Part[] Parts, Parameter[] Parameters, ContractVisibility Visibility,
			ContractParts PartsMode, Duration? Duration, Duration? ArchiveRequired, Duration? ArchiveOptional, DateTime? SignAfter,
			DateTime? SignBefore, bool CanActAsTemplate, IParameterEncryptionAlgorithm Algorithm,
			EventHandlerAsync<SmartContractEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();
			uint i, c = (uint)(Parameters?.Length ?? 0);
			bool HasEncryptedParameters = false;

			for (i = 0; i < c; i++)
			{
				Parameter P = Parameters[i];

				if (P.Protection == ProtectionLevel.Encrypted)
				{
					HasEncryptedParameters = true;
					break;
				}
			}

			byte[] Nonce = Guid.NewGuid().ToByteArray();
			string NonceStr = Convert.ToBase64String(Nonce);
			SymmetricCipherAlgorithms EncryptionAlgorithm = Algorithm?.Algorithm ?? this.preferredEncryptionAlgorithm;

			if (HasEncryptedParameters)
			{
				Algorithm ??= await ParameterEncryptionAlgorithm.Create(EncryptionAlgorithm, this);

				for (i = 0; i < c; i++)
				{
					Parameter P = Parameters[i];

					if (P.Protection == ProtectionLevel.Encrypted && P.ProtectedValue is null)
						P.ProtectedValue = Algorithm.Encrypt(P.Name, P.ParameterType, i, this.client.BareJID, Nonce, P.ObjectValue is null ? null : P.StringValue);
				}
			}

			Xml.Append("<createContract xmlns=\"");
			Xml.Append(NamespaceSmartContractsCurrent);
			Xml.Append("\"><template archiveOpt=\"");
			Xml.Append(ArchiveOptional.ToString());
			Xml.Append("\" archiveReq=\"");
			Xml.Append(ArchiveRequired.ToString());
			Xml.Append("\" canActAsTemplate=\"");
			Xml.Append(CommonTypes.Encode(CanActAsTemplate));
			Xml.Append("\" duration=\"");
			Xml.Append(Duration.ToString());
			Xml.Append("\" id=\"");
			Xml.Append(XML.Encode(TemplateId));
			Xml.Append("\" nonce=\"");
			Xml.Append(NonceStr);
			Xml.Append('"');

			if (SignAfter.HasValue && SignAfter > DateTime.MinValue)
			{
				Xml.Append(" signAfter=\"");
				Xml.Append(XML.Encode(SignAfter.Value));
				Xml.Append('"');
			}

			if (SignBefore.HasValue && SignBefore < DateTime.MaxValue)
			{
				Xml.Append(" signBefore=\"");
				Xml.Append(XML.Encode(SignBefore.Value));
				Xml.Append('"');
			}

			Xml.Append(" visibility=\"");
			Xml.Append(Visibility.ToString());
			Xml.Append("\"><parts>");

			switch (PartsMode)
			{
				case ContractParts.Open:
					Xml.Append("<open/>");
					break;

				case ContractParts.TemplateOnly:
					Xml.Append("<templateOnly/>");
					break;

				case ContractParts.ExplicitlyDefined:
					if (!(Parts is null))
					{
						foreach (Part Part in Parts)
						{
							Xml.Append("<part legalId=\"");
							Xml.Append(XML.Encode(Part.LegalId));
							Xml.Append("\" role=\"");
							Xml.Append(XML.Encode(Part.Role));
							Xml.Append("\"/>");
						}
					}
					break;
			}

			Xml.Append("</parts>");

			LinkedList<Parameter> TransientParameters = null;

			if (!(Parameters is null) && Parameters.Length > 0)
			{
				Xml.Append("<parameters>");

				foreach (Parameter Parameter in Parameters)
				{
					if (Parameter.Protection == ProtectionLevel.Transient)
					{
						Parameter.ProtectedValue ??= Guid.NewGuid().ToByteArray();

						TransientParameters ??= new LinkedList<Parameter>();
						TransientParameters.AddLast(Parameter);
					}

					Parameter.Serialize(Xml, true);
				}

				Xml.Append("</parameters>");
			}

			Xml.Append("</template>");

			if (!(TransientParameters is null))
			{
				Xml.Append("<transient>");

				foreach (Parameter Parameter in TransientParameters)
				{
					Parameter.Protection = ProtectionLevel.Normal;
					Parameter.Serialize(Xml, true);
					Parameter.Protection = ProtectionLevel.Transient;
				}

				Xml.Append("</transient>");
			}

			Xml.Append("</createContract>");

			await this.client.SendIqSet(Address, Xml.ToString(), this.ContractResponse, new object[] { Callback, State, HasEncryptedParameters, Algorithm?.Algorithm, Algorithm?.Key });
		}

		/// <summary>
		/// Creates a new contract from a template.
		/// </summary>
		/// <param name="TemplateId">ID of contract to be used as a template.</param>
		/// <param name="Parts">Parts defined in contract. Can be empty or null, if creating an open contract or a template.</param>
		/// <param name="Parameters">Any contractual parameters defined for the contract.</param>
		/// <param name="Visibility">Visibility of the contract.</param>
		/// <param name="PartsMode">How parts are defined in the contract. If equal to <see cref="ContractParts.ExplicitlyDefined"/>,
		/// then the explicitly defined parts must be provided in <paramref name="Parts"/>.</param>
		/// <param name="Duration">Duration of the contract, once signed.</param>
		/// <param name="ArchiveRequired">Required archivation duration, after signed contract has become obsolete.</param>
		/// <param name="ArchiveOptional">Optional archivation duration, after required archivation duration has elapsed.</param>
		/// <param name="SignAfter">Signatures will only be accepted after this point in time, if provided.</param>
		/// <param name="SignBefore">Signatures will only be accepted until this point in time, if provided.</param>
		/// <param name="CanActAsTemplate">If the contract can act as a template.</param>
		/// <returns>Contract.</returns>
		public Task<Contract> CreateContractAsync(string TemplateId, Part[] Parts, Parameter[] Parameters, ContractVisibility Visibility,
			ContractParts PartsMode, Duration? Duration, Duration? ArchiveRequired, Duration? ArchiveOptional, DateTime? SignAfter,
			DateTime? SignBefore, bool CanActAsTemplate)
		{
			return this.CreateContractAsync(this.componentAddress, TemplateId, Parts, Parameters, Visibility,
				PartsMode, Duration, ArchiveRequired, ArchiveOptional, SignAfter, SignBefore, CanActAsTemplate);
		}

		/// <summary>
		/// Creates a new contract from a template.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="TemplateId">ID of contract to be used as a template.</param>
		/// <param name="Parts">Parts defined in contract. Can be empty or null, if creating an open contract or a template.</param>
		/// <param name="Parameters">Any contractual parameters defined for the contract.</param>
		/// <param name="Visibility">Visibility of the contract.</param>
		/// <param name="PartsMode">How parts are defined in the contract. If equal to <see cref="ContractParts.ExplicitlyDefined"/>,
		/// then the explicitly defined parts must be provided in <paramref name="Parts"/>.</param>
		/// <param name="Duration">Duration of the contract, once signed.</param>
		/// <param name="ArchiveRequired">Required archivation duration, after signed contract has become obsolete.</param>
		/// <param name="ArchiveOptional">Optional archivation duration, after required archivation duration has elapsed.</param>
		/// <param name="SignAfter">Signatures will only be accepted after this point in time, if provided.</param>
		/// <param name="SignBefore">Signatures will only be accepted until this point in time, if provided.</param>
		/// <param name="CanActAsTemplate">If the contract can act as a template.</param>
		/// <returns>Contract.</returns>
		public async Task<Contract> CreateContractAsync(string Address, string TemplateId, Part[] Parts, Parameter[] Parameters,
			ContractVisibility Visibility, ContractParts PartsMode, Duration? Duration, Duration? ArchiveRequired, Duration? ArchiveOptional,
			DateTime? SignAfter, DateTime? SignBefore, bool CanActAsTemplate)
		{
			TaskCompletionSource<Contract> Result = new TaskCompletionSource<Contract>();

			await this.CreateContract(Address, TemplateId, Parts, Parameters, Visibility, PartsMode, Duration,
				ArchiveRequired, ArchiveOptional, SignAfter, SignBefore, CanActAsTemplate, (Sender, e) =>
				{
					if (e.Ok)
						Result.TrySetResult(e.Contract);
					else
						Result.TrySetException(e.StanzaError ?? new Exception("Unable to create the contract."));

					return Task.CompletedTask;

				}, null);

			return await Result.Task;
		}

		#endregion

		#region Get Created Contract References

		/// <summary>
		/// Get references to contracts the account has created.
		/// </summary>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetCreatedContractReferences(EventHandlerAsync<IdReferencesEventArgs> Callback, object State)
		{
			return this.GetCreatedContractReferences(this.componentAddress, 0, int.MaxValue, Callback, State);
		}

		/// <summary>
		/// Get references to contracts the account has created.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetCreatedContractReferences(string Address, EventHandlerAsync<IdReferencesEventArgs> Callback, object State)
		{
			return this.GetCreatedContractReferences(Address, 0, int.MaxValue, Callback, State);
		}

		/// <summary>
		/// Get references to contracts the account has created.
		/// </summary>
		/// <param name="Offset">Result will start with the response at this offset into result set.</param>
		/// <param name="MaxCount">Result will be limited to this number of items.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetCreatedContractReferences(int Offset, int MaxCount, EventHandlerAsync<IdReferencesEventArgs> Callback, object State)
		{
			return this.GetCreatedContractReferences(this.componentAddress, Offset, MaxCount, Callback, State);
		}

		/// <summary>
		/// Get references to contracts the account has created.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Offset">Result will start with the response at this offset into result set.</param>
		/// <param name="MaxCount">Result will be limited to this number of items.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetCreatedContractReferences(string Address, int Offset, int MaxCount, EventHandlerAsync<IdReferencesEventArgs> Callback, object State)
		{
			if (Offset < 0)
				throw new ArgumentException("Offsets cannot be negative.", nameof(Offset));

			if (MaxCount <= 0)
				throw new ArgumentException("Must be postitive.", nameof(MaxCount));

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getCreatedContracts references='true' xmlns='");
			Xml.Append(NamespaceSmartContractsCurrent);

			if (Offset > 0)
			{
				Xml.Append("' offset='");
				Xml.Append(Offset.ToString());
			}

			if (MaxCount < int.MaxValue)
			{
				Xml.Append("' maxCount='");
				Xml.Append(MaxCount.ToString());
			}

			Xml.Append("'/>");

			return this.client.SendIqGet(Address, Xml.ToString(), this.IdReferencesResponse, new object[] { Callback, State });
		}

		private async Task IdReferencesResponse(object Sender, IqResultEventArgs e)
		{
			object[] P = (object[])e.State;
			EventHandlerAsync<IdReferencesEventArgs> Callback = (EventHandlerAsync<IdReferencesEventArgs>)P[0];
			XmlElement E = e.FirstElement;
			List<string> IDs = new List<string>();

			if (e.Ok && !(E is null))
			{
				foreach (XmlNode N in E.ChildNodes)
				{
					if (N is XmlElement E2 && E2.LocalName == "ref")
					{
						string Id = XML.Attribute(E2, "id");
						IDs.Add(Id);
					}
				}
			}
			else
				e.Ok = false;

			e.State = P[1];
			await Callback.Raise(this, new IdReferencesEventArgs(e, IDs.ToArray()));
		}

		/// <summary>
		/// Get references to contracts the account has created.
		/// </summary>
		/// <returns>Contract IDs</returns>
		public Task<string[]> GetCreatedContractReferencesAsync()
		{
			return this.GetCreatedContractReferencesAsync(this.componentAddress, 0, int.MaxValue);
		}

		/// <summary>
		/// Get references to contracts the account has created.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <returns>Contract IDs</returns>
		public Task<string[]> GetCreatedContractReferencesAsync(string Address)
		{
			return this.GetCreatedContractReferencesAsync(Address, 0, int.MaxValue);
		}

		/// <summary>
		/// Get references to contracts the account has created.
		/// </summary>
		/// <param name="Offset">Result will start with the response at this offset into result set.</param>
		/// <param name="MaxCount">Result will be limited to this number of items.</param>
		/// <returns>Contract IDs</returns>
		public Task<string[]> GetCreatedContractReferencesAsync(int Offset, int MaxCount)
		{
			return this.GetCreatedContractReferencesAsync(this.componentAddress, Offset, MaxCount);
		}

		/// <summary>
		/// Get references to contracts the account has created.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Offset">Result will start with the response at this offset into result set.</param>
		/// <param name="MaxCount">Result will be limited to this number of items.</param>
		/// <returns>Contract IDs</returns>
		public async Task<string[]> GetCreatedContractReferencesAsync(string Address, int Offset, int MaxCount)
		{
			TaskCompletionSource<string[]> Result = new TaskCompletionSource<string[]>();

			await this.GetCreatedContractReferences(Address, Offset, MaxCount, (Sender, e) =>
				{
					if (e.Ok)
						Result.TrySetResult(e.References);
					else
						Result.TrySetException(e.StanzaError ?? new Exception("Unable to get created contract references."));

					return Task.CompletedTask;

				}, null);

			return await Result.Task;
		}

		#endregion

		#region Get Created Contracts

		/// <summary>
		/// Get contracts the account has created.
		/// </summary>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetCreatedContracts(EventHandlerAsync<ContractsEventArgs> Callback, object State)
		{
			return this.GetCreatedContracts(this.componentAddress, 0, int.MaxValue, Callback, State);
		}

		/// <summary>
		/// Get contracts the account has created.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetCreatedContracts(string Address, EventHandlerAsync<ContractsEventArgs> Callback, object State)
		{
			return this.GetCreatedContracts(Address, 0, int.MaxValue, Callback, State);
		}

		/// <summary>
		/// Get contracts the account has created.
		/// </summary>
		/// <param name="Offset">Result will start with the response at this offset into result set.</param>
		/// <param name="MaxCount">Result will be limited to this number of items.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetCreatedContracts(int Offset, int MaxCount, EventHandlerAsync<ContractsEventArgs> Callback, object State)
		{
			return this.GetCreatedContracts(this.componentAddress, Offset, MaxCount, Callback, State);
		}

		/// <summary>
		/// Get contracts the account has created.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Offset">Result will start with the response at this offset into result set.</param>
		/// <param name="MaxCount">Result will be limited to this number of items.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetCreatedContracts(string Address, int Offset, int MaxCount, EventHandlerAsync<ContractsEventArgs> Callback, object State)
		{
			if (Offset < 0)
				throw new ArgumentException("Offsets cannot be negative.", nameof(Offset));

			if (MaxCount <= 0)
				throw new ArgumentException("Must be postitive.", nameof(MaxCount));

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getCreatedContracts references='false' xmlns='");
			Xml.Append(NamespaceSmartContractsCurrent);

			if (Offset > 0)
			{
				Xml.Append("' offset='");
				Xml.Append(Offset.ToString());
			}

			if (MaxCount < int.MaxValue)
			{
				Xml.Append("' maxCount='");
				Xml.Append(MaxCount.ToString());
			}

			Xml.Append("'/>");

			return this.client.SendIqGet(Address, Xml.ToString(), this.ContractsResponse, new object[] { Callback, State });
		}

		private async Task ContractsResponse(object Sender, IqResultEventArgs e)
		{
			object[] P = (object[])e.State;
			EventHandlerAsync<ContractsEventArgs> Callback = (EventHandlerAsync<ContractsEventArgs>)P[0];
			XmlElement E = e.FirstElement;
			List<Contract> Contracts = new List<Contract>();
			List<string> References = new List<string>();

			if (e.Ok && !(E is null))
			{
				foreach (XmlNode N in E.ChildNodes)
				{
					if (N is XmlElement E2)
					{
						switch (E2.LocalName)
						{
							case "contract":
								ParsedContract ParsedContract = await Contract.Parse(E2, this, false);

								if (!(ParsedContract is null))
									Contracts.Add(ParsedContract.Contract);
								break;

							case "ref":
								string ContractId = XML.Attribute(E2, "id");
								References.Add(ContractId);
								break;
						}
					}
				}
			}
			else
				e.Ok = false;

			e.State = P[1];
			await Callback.Raise(this, new ContractsEventArgs(e, Contracts.ToArray(), References.ToArray()));
		}

		/// <summary>
		/// Get contracts the account has created.
		/// </summary>
		/// <returns>Response with results.</returns>
		public Task<ContractsEventArgs> GetCreatedContractsAsync()
		{
			return this.GetCreatedContractsAsync(this.componentAddress, 0, int.MaxValue);
		}

		/// <summary>
		/// Get contracts the account has created.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <returns>Response with results.</returns>
		public Task<ContractsEventArgs> GetCreatedContractsAsync(string Address)
		{
			return this.GetCreatedContractsAsync(Address, 0, int.MaxValue);
		}

		/// <summary>
		/// Get contracts the account has created.
		/// </summary>
		/// <param name="Offset">Result will start with the response at this offset into result set.</param>
		/// <param name="MaxCount">Result will be limited to this number of items.</param>
		/// <returns>Response with results.</returns>
		public Task<ContractsEventArgs> GetCreatedContractsAsync(int Offset, int MaxCount)
		{
			return this.GetCreatedContractsAsync(this.componentAddress, Offset, MaxCount);
		}

		/// <summary>
		/// Get contracts the account has created.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Offset">Result will start with the response at this offset into result set.</param>
		/// <param name="MaxCount">Result will be limited to this number of items.</param>
		/// <returns>Response with results.</returns>
		public async Task<ContractsEventArgs> GetCreatedContractsAsync(string Address, int Offset, int MaxCount)
		{
			TaskCompletionSource<ContractsEventArgs> Result = new TaskCompletionSource<ContractsEventArgs>();

			await this.GetCreatedContracts(Address, Offset, MaxCount, (Sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		#endregion

		#region Sign Contract

		/// <summary>
		/// Signs a contract
		/// </summary>
		/// <param name="Contract">Smart Contract to sign.</param>
		/// <param name="Role">Role of the legal idenity, in the contract.</param>
		/// <param name="Transferable">If the signature should be transferable or not.
		/// Transferable signatures are copied to contracts based on the current contract as a template,
		/// and only if no parameters and attributes are changed. (Otherwise the signature would break.)</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task SignContract(Contract Contract, string Role, bool Transferable, EventHandlerAsync<SmartContractEventArgs> Callback, object State)
		{
			return this.SignContract(this.GetTrustProvider(Contract.ContractId), Contract, Role, Transferable, Callback, State);
		}

		/// <summary>
		/// Signs a contract
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Contract">Smart Contract to sign.</param>
		/// <param name="Role">Role of the legal idenity, in the contract.</param>
		/// <param name="Transferable">If the signature should be transferable or not.
		/// Transferable signatures are copied to contracts based on the current contract as a template,
		/// and only if no parameters and attributes are changed. (Otherwise the signature would break.)</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public async Task SignContract(string Address, Contract Contract, string Role, bool Transferable,
			EventHandlerAsync<SmartContractEventArgs> Callback, object State)
		{
			if (Contract.HasEncryptedParameters)
			{
				Tuple<SymmetricCipherAlgorithms, string, byte[]> T = await this.TryLoadContractSharedSecret(Contract.ContractId);
				if (!(T is null))
				{
					IParameterEncryptionAlgorithm Algorithm = await ParameterEncryptionAlgorithm.Create(
						Contract.ContractId, T.Item1, this, T.Item2, T.Item3);

					Contract.EncryptEncryptedParameters(T.Item2, Algorithm);
				}
			}

			StringBuilder Xml = new StringBuilder();
			Contract.Serialize(Xml, false, false, false, false, false, false, false);
			byte[] Data = Encoding.UTF8.GetBytes(Xml.ToString());

			await this.Sign(Address, Data, SignWith.LatestApprovedId, async (Sender, e) =>
			{
				if (e.Ok)
				{
					Xml.Clear();
					Xml.Append("<signContract xmlns='");
					Xml.Append(NamespaceSmartContractsCurrent);
					Xml.Append("' id='");
					Xml.Append(XML.Encode(Contract.ContractId));
					Xml.Append("' role='");
					Xml.Append(XML.Encode(Role));

					if (Transferable)
						Xml.Append("' transferable='true");

					Xml.Append("' s='");
					Xml.Append(Convert.ToBase64String(e.Signature));
					Xml.Append("'/>");

					await this.client.SendIqSet(Address, Xml.ToString(), this.ContractResponse, new object[] { Callback, State });
				}
				else
					await Callback.Raise(this, new SmartContractEventArgs(e, null));
			}, State);
		}

		/// <summary>
		/// Signs a contract
		/// </summary>
		/// <param name="Contract">Smart Contract to sign.</param>
		/// <param name="Role">Role of the legal idenity, in the contract.</param>
		/// <param name="Transferable">If the signature should be transferable or not.
		/// Transferable signatures are copied to contracts based on the current contract as a template,
		/// and only if no parameters and attributes are changed. (Otherwise the signature would break.)</param>
		/// <returns>Contract</returns>
		public Task<Contract> SignContractAsync(Contract Contract, string Role, bool Transferable)
		{
			return this.SignContractAsync(this.GetTrustProvider(Contract.ContractId), Contract, Role, Transferable);
		}

		/// <summary>
		/// Signs a contract
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Contract">Smart Contract to sign.</param>
		/// <param name="Role">Role of the legal idenity, in the contract.</param>
		/// <param name="Transferable">If the signature should be transferable or not.
		/// Transferable signatures are copied to contracts based on the current contract as a template,
		/// and only if no parameters and attributes are changed. (Otherwise the signature would break.)</param>
		/// <returns>Contract</returns>
		public async Task<Contract> SignContractAsync(string Address, Contract Contract, string Role, bool Transferable)
		{
			TaskCompletionSource<Contract> Result = new TaskCompletionSource<Contract>();

			await this.SignContract(Address, Contract, Role, Transferable, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Contract);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to sign the contract."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		#endregion

		#region Get Signed Contract References

		/// <summary>
		/// Get references to contracts the account has signed.
		/// </summary>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetSignedContractReferences(EventHandlerAsync<IdReferencesEventArgs> Callback, object State)
		{
			return this.GetSignedContractReferences(this.componentAddress, 0, int.MaxValue, Callback, State);
		}

		/// <summary>
		/// Get references to contracts the account has signed.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetSignedContractReferences(string Address, EventHandlerAsync<IdReferencesEventArgs> Callback, object State)
		{
			return this.GetSignedContractReferences(Address, 0, int.MaxValue, Callback, State);
		}

		/// <summary>
		/// Get references to contracts the account has signed.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Offset">Result will start with the response at this offset into result set.</param>
		/// <param name="MaxCount">Result will be limited to this number of items.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetSignedContractReferences(string Address, int Offset, int MaxCount, EventHandlerAsync<IdReferencesEventArgs> Callback, object State)
		{
			if (Offset < 0)
				throw new ArgumentException("Offsets cannot be negative.", nameof(Offset));

			if (MaxCount <= 0)
				throw new ArgumentException("Must be postitive.", nameof(MaxCount));

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getSignedContracts references='true' xmlns='");
			Xml.Append(NamespaceSmartContractsCurrent);

			if (Offset > 0)
			{
				Xml.Append("' offset='");
				Xml.Append(Offset.ToString());
			}

			if (MaxCount < int.MaxValue)
			{
				Xml.Append("' maxCount='");
				Xml.Append(MaxCount.ToString());
			}

			Xml.Append("'/>");

			return this.client.SendIqGet(Address, Xml.ToString(), this.IdReferencesResponse, new object[] { Callback, State });
		}

		/// <summary>
		/// Get references to contracts the account has signed.
		/// </summary>
		/// <returns>Contract IDs</returns>
		public Task<string[]> GetSignedContractReferencesAsync()
		{
			return this.GetSignedContractReferencesAsync(this.componentAddress, 0, int.MaxValue);
		}

		/// <summary>
		/// Get references to contracts the account has signed.
		/// </summary>
		/// <param name="Offset">Result will start with the response at this offset into result set.</param>
		/// <param name="MaxCount">Result will be limited to this number of items.</param>
		/// <returns>Contract IDs</returns>
		public Task<string[]> GetSignedContractReferencesAsync(int Offset, int MaxCount)
		{
			return this.GetSignedContractReferencesAsync(this.componentAddress, Offset, MaxCount);
		}

		/// <summary>
		/// Get references to contracts the account has signed.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Offset">Result will start with the response at this offset into result set.</param>
		/// <param name="MaxCount">Result will be limited to this number of items.</param>
		/// <returns>Contract IDs</returns>
		public async Task<string[]> GetSignedContractReferencesAsync(string Address, int Offset, int MaxCount)
		{
			TaskCompletionSource<string[]> Result = new TaskCompletionSource<string[]>();

			await this.GetSignedContractReferences(Address, Offset, MaxCount, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.References);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to get signed contract references."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		#endregion

		#region Get Signed Contracts

		/// <summary>
		/// Get contracts the account has signed.
		/// </summary>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetSignedContracts(EventHandlerAsync<ContractsEventArgs> Callback, object State)
		{
			return this.GetSignedContracts(this.componentAddress, 0, int.MaxValue, Callback, State);
		}

		/// <summary>
		/// Get contracts the account has signed.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetSignedContracts(string Address, EventHandlerAsync<ContractsEventArgs> Callback, object State)
		{
			return this.GetSignedContracts(Address, 0, int.MaxValue, Callback, State);
		}

		/// <summary>
		/// Get contracts the account has signed.
		/// </summary>
		/// <param name="Offset">Result will start with the response at this offset into result set.</param>
		/// <param name="MaxCount">Result will be limited to this number of items.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetSignedContracts(int Offset, int MaxCount, EventHandlerAsync<ContractsEventArgs> Callback, object State)
		{
			return this.GetSignedContracts(this.componentAddress, Offset, MaxCount, Callback, State);
		}

		/// <summary>
		/// Get contracts the account has signed.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Offset">Result will start with the response at this offset into result set.</param>
		/// <param name="MaxCount">Result will be limited to this number of items.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetSignedContracts(string Address, int Offset, int MaxCount, EventHandlerAsync<ContractsEventArgs> Callback, object State)
		{
			if (Offset < 0)
				throw new ArgumentException("Offsets cannot be negative.", nameof(Offset));

			if (MaxCount <= 0)
				throw new ArgumentException("Must be postitive.", nameof(MaxCount));

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getSignedContracts references='false' xmlns='");
			Xml.Append(NamespaceSmartContractsCurrent);

			if (Offset > 0)
			{
				Xml.Append("' offset='");
				Xml.Append(Offset.ToString());
			}

			if (MaxCount < int.MaxValue)
			{
				Xml.Append("' maxCount='");
				Xml.Append(MaxCount.ToString());
			}

			Xml.Append("'/>");

			return this.client.SendIqGet(Address, Xml.ToString(), this.ContractsResponse, new object[] { Callback, State });
		}

		/// <summary>
		/// Get contracts the account has signed.
		/// </summary>
		/// <returns>Response with results.</returns>
		public Task<ContractsEventArgs> GetSignedContractsAsync()
		{
			return this.GetSignedContractsAsync(this.componentAddress, 0, int.MaxValue);
		}

		/// <summary>
		/// Get contracts the account has signed.
		/// </summary>
		/// <param name="Offset">Result will start with the response at this offset into result set.</param>
		/// <param name="MaxCount">Result will be limited to this number of items.</param>
		/// <returns>Response with results.</returns>
		public Task<ContractsEventArgs> GetSignedContractsAsync(int Offset, int MaxCount)
		{
			return this.GetSignedContractsAsync(this.componentAddress, Offset, MaxCount);
		}

		/// <summary>
		/// Get contracts the account has signed.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Offset">Result will start with the response at this offset into result set.</param>
		/// <param name="MaxCount">Result will be limited to this number of items.</param>
		/// <returns>Response with results.</returns>
		public async Task<ContractsEventArgs> GetSignedContractsAsync(string Address, int Offset, int MaxCount)
		{
			TaskCompletionSource<ContractsEventArgs> Result = new TaskCompletionSource<ContractsEventArgs>();

			await this.GetSignedContracts(Address, Offset, MaxCount, (Sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		#endregion

		#region Contract Signature event

		private Task ContractSignedMessageHandler(object Sender, MessageEventArgs e)
		{
			string ContractId = XML.Attribute(e.Content, "contractId");
			string LegalId = XML.Attribute(e.Content, "legalId");
			string Role = XML.Attribute(e.Content, "role");

			return this.ContractSigned.Raise(this, new ContractSignedEventArgs(ContractId, LegalId, Role));
		}

		/// <summary>
		/// Event raised whenever a contract has been signed.
		/// </summary>
		public event EventHandlerAsync<ContractSignedEventArgs> ContractSigned = null;

		#endregion

		#region Get Contract

		/// <summary>
		/// Gets a contract
		/// </summary>
		/// <param name="ContractId">ID of contract to get.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetContract(string ContractId, EventHandlerAsync<SmartContractEventArgs> Callback, object State)
		{
			return this.GetContract(this.GetTrustProvider(ContractId), ContractId, Callback, State);
		}

		/// <summary>
		/// Gets a contract
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ContractId">ID of contract to get.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetContract(string Address, string ContractId, EventHandlerAsync<SmartContractEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getContract xmlns='");
			Xml.Append(NamespaceSmartContractsCurrent);
			Xml.Append("' id='");
			Xml.Append(XML.Encode(ContractId));
			Xml.Append("'/>");

			return this.client.SendIqGet(Address, Xml.ToString(), this.ContractResponse, new object[] { Callback, State });
		}

		/// <summary>
		/// Gets a contract
		/// </summary>
		/// <param name="ContractId">ID of contract to get.</param>
		/// <returns>Contract</returns>
		public Task<Contract> GetContractAsync(string ContractId)
		{
			return this.GetContractAsync(this.GetTrustProvider(ContractId), ContractId);
		}

		/// <summary>
		/// Gets a contract
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ContractId">ID of contract to get.</param>
		/// <returns>Contract</returns>
		public async Task<Contract> GetContractAsync(string Address, string ContractId)
		{
			TaskCompletionSource<Contract> Result = new TaskCompletionSource<Contract>();

			await this.GetContract(Address, ContractId, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Contract);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to get the contract."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		#endregion

		#region Get Contracts

		/// <summary>
		/// Gets a collection of contracts
		/// </summary>
		/// <param name="ContractIds">IDs of contracts to get.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public async Task GetContracts(string[] ContractIds, EventHandlerAsync<ContractsEventArgs> Callback, object State)
		{
			Dictionary<string, List<string>> ByTrustProvider = new Dictionary<string, List<string>>();
			string LastTrustProvider = string.Empty;
			List<string> LastList = null;

			foreach (string ContractId in ContractIds)
			{
				string TrustProvider = this.GetTrustProvider(ContractId);

				if (TrustProvider != LastTrustProvider || LastList is null)
				{
					if (!ByTrustProvider.TryGetValue(TrustProvider, out LastList))
					{
						LastList = new List<string>();
						ByTrustProvider[TrustProvider] = LastList;
					}

					LastTrustProvider = TrustProvider;
				}

				LastList.Add(ContractId);
			}

			List<Contract> Contracts = new List<Contract>();
			List<string> References = new List<string>();
			bool Ok = true;
			int NrLeft = ByTrustProvider.Count;

			foreach (KeyValuePair<string, List<string>> P in ByTrustProvider)
			{
				await this.GetContracts(P.Key, P.Value.ToArray(), async (Sender, e) =>
				{
					lock (Contracts)
					{
						if (e.Ok)
						{
							Contracts.AddRange(e.Contracts);
							References.AddRange(e.References);
						}
						else
							Ok = false;

						NrLeft--;
						if (NrLeft > 0)
							return;
					}

					ContractsEventArgs e2 = new ContractsEventArgs(e, Contracts.ToArray(), References.ToArray())
					{
						Ok = Ok
					};

					await Callback.Raise(this, e2);

				}, State);
			}
		}

		/// <summary>
		/// Gets a collection of contracts
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ContractIds">IDs of contracts to get.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetContracts(string Address, string[] ContractIds, EventHandlerAsync<ContractsEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getContracts xmlns='");
			Xml.Append(NamespaceSmartContractsCurrent);
			Xml.Append("'>");

			foreach (string ContractId in ContractIds)
			{
				Xml.Append("<ref id='");
				Xml.Append(XML.Encode(ContractId));
				Xml.Append("'/>");
			}

			Xml.Append("</getContracts>");

			return this.client.SendIqGet(Address, Xml.ToString(), this.ContractsResponse, new object[] { Callback, State });
		}

		/// <summary>
		/// Gets a collection of contracts
		/// </summary>
		/// <param name="ContractIds">IDs of contracts to get.</param>
		/// <returns>Contract</returns>
		public async Task<ContractsEventArgs> GetContractsAsync(string[] ContractIds)
		{
			TaskCompletionSource<ContractsEventArgs> Result = new TaskCompletionSource<ContractsEventArgs>();

			await this.GetContracts(ContractIds, (Sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		/// <summary>
		/// Gets a collection of contracts
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ContractIds">IDs of contracts to get.</param>
		/// <returns>Contracts that could be retrieved, and references for the IDs that could not be retrieved.</returns>
		public async Task<ContractsEventArgs> GetContractsAsync(string Address, string[] ContractIds)
		{
			TaskCompletionSource<ContractsEventArgs> Result = new TaskCompletionSource<ContractsEventArgs>();

			await this.GetContracts(Address, ContractIds, (Sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		#endregion

		#region Obsolete Contract

		/// <summary>
		/// Obsoletes a contract
		/// </summary>
		/// <param name="ContractId">ID of contract to obsolete.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task ObsoleteContract(string ContractId, EventHandlerAsync<SmartContractEventArgs> Callback, object State)
		{
			return this.ObsoleteContract(this.GetTrustProvider(ContractId), ContractId, Callback, State);
		}

		/// <summary>
		/// Obsoletes a contract
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ContractId">ID of contract to obsolete.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task ObsoleteContract(string Address, string ContractId,
			EventHandlerAsync<SmartContractEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<obsoleteContract xmlns='");
			Xml.Append(NamespaceSmartContractsCurrent);
			Xml.Append("' id='");
			Xml.Append(XML.Encode(ContractId));
			Xml.Append("'/>");

			return this.client.SendIqSet(Address, Xml.ToString(), this.ContractResponse, new object[] { Callback, State });
		}

		/// <summary>
		/// Obsoletes a contract
		/// </summary>
		/// <param name="ContractId">ID of contract to obsolete.</param>
		/// <returns>Contract</returns>
		public Task<Contract> ObsoleteContractAsync(string ContractId)
		{
			return this.ObsoleteContractAsync(this.GetTrustProvider(ContractId), ContractId);
		}

		/// <summary>
		/// Obsoletes a contract
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ContractId">ID of contract to obsolete.</param>
		/// <returns>Contract</returns>
		public async Task<Contract> ObsoleteContractAsync(string Address, string ContractId)
		{
			TaskCompletionSource<Contract> Result = new TaskCompletionSource<Contract>();

			await this.ObsoleteContract(Address, ContractId, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Contract);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to obsolete the contract."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		#endregion

		#region Delete Contract

		/// <summary>
		/// Deletes a contract
		/// </summary>
		/// <param name="ContractId">ID of contract to delete.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task DeleteContract(string ContractId, EventHandlerAsync<SmartContractEventArgs> Callback, object State)
		{
			return this.DeleteContract(this.GetTrustProvider(ContractId), ContractId, Callback, State);
		}

		/// <summary>
		/// Deletes a contract
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ContractId">ID of contract to delete.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task DeleteContract(string Address, string ContractId,
			EventHandlerAsync<SmartContractEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<deleteContract xmlns='");
			Xml.Append(NamespaceSmartContractsCurrent);
			Xml.Append("' id='");
			Xml.Append(XML.Encode(ContractId));
			Xml.Append("'/>");

			return this.client.SendIqSet(Address, Xml.ToString(), this.ContractResponse, new object[] { Callback, State });
		}

		/// <summary>
		/// Deletes a contract
		/// </summary>
		/// <param name="ContractId">ID of contract to delete.</param>
		/// <returns>Contract</returns>
		public Task<Contract> DeleteContractAsync(string ContractId)
		{
			return this.DeleteContractAsync(this.GetTrustProvider(ContractId), ContractId);
		}

		/// <summary>
		/// Deletes a contract
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ContractId">ID of contract to delete.</param>
		/// <returns>Contract</returns>
		public async Task<Contract> DeleteContractAsync(string Address, string ContractId)
		{
			TaskCompletionSource<Contract> Result = new TaskCompletionSource<Contract>();

			await this.DeleteContract(Address, ContractId, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Contract);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to delete the contract."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		#endregion

		#region Contract Created event

		private Task ContractCreatedMessageHandler(object Sender, MessageEventArgs e)
		{
			string ContractId = XML.Attribute(e.Content, "contractId");

			if (!this.IsFromTrustProvider(ContractId, e.From))
				return Task.CompletedTask;

			return this.ContractCreated.Raise(this, new ContractReferenceEventArgs(ContractId));
		}

		/// <summary>
		/// Event raised whenever a contract has been created.
		/// </summary>
		public event EventHandlerAsync<ContractReferenceEventArgs> ContractCreated = null;

		#endregion

		#region Contract Updated event

		private Task ContractUpdatedMessageHandler(object Sender, MessageEventArgs e)
		{
			string ContractId = XML.Attribute(e.Content, "contractId");

			if (!this.IsFromTrustProvider(ContractId, e.From))
				return Task.CompletedTask;

			return this.ContractUpdated.Raise(this, new ContractReferenceEventArgs(ContractId));
		}

		/// <summary>
		/// Event raised whenever a contract has been updated.
		/// </summary>
		public event EventHandlerAsync<ContractReferenceEventArgs> ContractUpdated = null;

		#endregion

		#region Contract Deleted event

		private Task ContractDeletedMessageHandler(object Sender, MessageEventArgs e)
		{
			string ContractId = XML.Attribute(e.Content, "contractId");

			if (!this.IsFromTrustProvider(ContractId, e.From))
				return Task.CompletedTask;

			return this.ContractDeleted.Raise(this, new ContractReferenceEventArgs(ContractId));
		}

		/// <summary>
		/// Event raised whenever a contract has been deleted.
		/// </summary>
		public event EventHandlerAsync<ContractReferenceEventArgs> ContractDeleted = null;

		#endregion

		#region Update Contract

		/// <summary>
		/// Updates a contract
		/// </summary>
		/// <param name="Contract">Contract to update.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task UpdateContract(Contract Contract, EventHandlerAsync<SmartContractEventArgs> Callback, object State)
		{
			return this.UpdateContract(this.GetTrustProvider(Contract.ContractId), Contract, Callback, State);
		}

		/// <summary>
		/// Updates a contract
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Contract">Contract to update.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public async Task UpdateContract(string Address, Contract Contract,
			EventHandlerAsync<SmartContractEventArgs> Callback, object State)
		{
			if (Contract.HasEncryptedParameters)
			{
				Tuple<SymmetricCipherAlgorithms, string, byte[]> KeyInfo =
					await this.TryLoadContractSharedSecret(Contract.ContractId);

				if (!(KeyInfo is null))
				{
					IParameterEncryptionAlgorithm Algorithm = await ParameterEncryptionAlgorithm.Create(
						Contract.ContractId, KeyInfo.Item1, this, KeyInfo.Item2, KeyInfo.Item3);

					Contract.EncryptEncryptedParameters(this.client.BareJID, Algorithm);
				}
			}

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<updateContract xmlns='");
			Xml.Append(NamespaceSmartContractsCurrent);
			Xml.Append("'>");

			Contract.Serialize(Xml, false, true, true, true, false, false, false);

			Xml.Append("</updateContract>");

			await this.client.SendIqSet(Address, Xml.ToString(), this.ContractResponse, new object[] { Callback, State });
		}

		/// <summary>
		/// Updates a contract
		/// </summary>
		/// <param name="Contract">Contract to update.</param>
		/// <returns>Contract</returns>
		public Task<Contract> UpdateContractAsync(Contract Contract)
		{
			return this.UpdateContractAsync(this.GetTrustProvider(Contract.ContractId), Contract);
		}

		/// <summary>
		/// Updates a contract
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Contract">Contract to update.</param>
		/// <returns>Contract</returns>
		public async Task<Contract> UpdateContractAsync(string Address, Contract Contract)
		{
			TaskCompletionSource<Contract> Result = new TaskCompletionSource<Contract>();

			await this.UpdateContract(Address, Contract, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Contract);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to update the contract."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		#endregion

		#region Validate Contract

		/// <summary>
		/// Validates a smart contract.
		/// </summary>
		/// <param name="Contract">Contract to validate</param>
		/// <param name="Callback">Method to call when validation is completed</param>
		/// <param name="State">State object to pass to callback method.</param>
		public Task Validate(Contract Contract, EventHandlerAsync<ContractValidationEventArgs> Callback, object State)
		{
			return this.Validate(Contract, true, Callback, State);
		}

		/// <summary>
		/// Validates a smart contract.
		/// </summary>
		/// <param name="Contract">Contract to validate</param>
		/// <param name="ValidateState">If the state attribute should be validated. (Default=true)</param>
		/// <param name="Callback">Method to call when validation is completed</param>
		/// <param name="State">State object to pass to callback method.</param>
		public async Task Validate(Contract Contract, bool ValidateState, EventHandlerAsync<ContractValidationEventArgs> Callback, object State)
		{
			if (Contract is null)
			{
				await this.ReturnStatus(ContractStatus.ContractUndefined, Callback, State);
				return;
			}

			if (ValidateState &&
				Contract.State != ContractState.Approved &&
				Contract.State != ContractState.BeingSigned &&
				Contract.State != ContractState.Signed)
			{
				await this.ReturnStatus(ContractStatus.NotApproved, Callback, State);
				return;
			}

			DateTime Now = DateTime.Now;

			if (Now.Date.AddDays(1) < Contract.From)    // To avoid Time-zone problems
			{
				await this.ReturnStatus(ContractStatus.NotValidYet, Callback, State);
				return;
			}

			if (Now.Date.AddDays(-1) > Contract.To)      // To avoid Time-zone problems
			{
				await this.ReturnStatus(ContractStatus.NotValidAnymore, Callback, State);
				return;
			}

			if (string.IsNullOrEmpty(Contract.Provider))
			{
				await this.ReturnStatus(ContractStatus.NoTrustProvider, Callback, State);
				return;
			}

			if (Contract.PartsMode == ContractParts.TemplateOnly)
			{
				await this.ReturnStatus(ContractStatus.TemplateOnly, Callback, State);
				return;
			}

			if (Contract.ClientSignatures is null || Contract.ClientSignatures.Length == 0)
			{
				await this.ReturnStatus(ContractStatus.NoClientSignatures, Callback, State);
				return;
			}

			if (!await Contract.IsLegallyBinding(false, this))
			{
				await this.ReturnStatus(ContractStatus.NotLegallyBinding, Callback, State);
				return;
			}

			if (!await IsHumanReadableWellDefined(Contract))
			{
				await this.ReturnStatus(ContractStatus.HumanReadableNotWellDefined, Callback, State);
				return;
			}

			if ((Contract.Parameters?.Length ?? 0) > 0)
			{
				try
				{
					Variables Variables = new Variables()
					{
						{ "Duration", Contract.Duration }
					};

					foreach (Parameter Parameter in Contract.Parameters)
						Parameter.Populate(Variables);

					foreach (Parameter Parameter in Contract.Parameters)
					{
						if (!await Parameter.IsParameterValid(Variables, this))
						{
							await this.ReturnStatus(ContractStatus.ParameterValuesNotValid, Callback, State);
							return;
						}
					}
				}
				catch (Exception)
				{
					await this.ReturnStatus(ContractStatus.ParameterValuesNotValid, Callback, State);
					return;
				}
			}

			if (string.IsNullOrEmpty(Contract.ForMachinesLocalName) ||
				string.IsNullOrEmpty(Contract.ForMachinesNamespace) ||
				Contract.ForMachines is null ||
				Contract.ForMachinesLocalName != Contract.ForMachines.LocalName ||
				Contract.ForMachinesNamespace != Contract.ForMachines.NamespaceURI)
			{
				await this.ReturnStatus(ContractStatus.MachineReadableNotWellDefined, Callback, State);
				return;
			}

			XmlDocument Doc;

			try
			{
				Doc = new XmlDocument()
				{
					PreserveWhitespace = true
				};

				Doc.LoadXml(Contract.ForMachines.OuterXml);
			}
			catch (Exception)
			{
				await this.ReturnStatus(ContractStatus.MachineReadableNotWellDefined, Callback, State);
				return;
			}

			Dictionary<string, XmlSchema> Schemas = new Dictionary<string, XmlSchema>();
			LinkedList<XmlNode> ToCheck = new LinkedList<XmlNode>();

			ToCheck.AddLast(Doc.DocumentElement);

			while (!(ToCheck.First is null))
			{
				XmlNode N = ToCheck.First.Value;
				ToCheck.RemoveFirst();

				while (!(N is null))
				{
					if (N is XmlElement E)
					{
						if (!string.IsNullOrEmpty(E.NamespaceURI))
							Schemas[E.NamespaceURI] = null;

						foreach (XmlNode N2 in E.ChildNodes)
							ToCheck.AddLast(N2);
					}

					N = N.NextSibling;
				}
			}

			int NrSchemas = Schemas.Count;
			if (NrSchemas == 0 || !Schemas.ContainsKey(Contract.ForMachinesNamespace))
			{
				await this.ReturnStatus(ContractStatus.MachineReadableNotWellDefined, Callback, State);
				return;
			}

			string SchemaKey = Contract.ForMachinesNamespace + "#" + Convert.ToBase64String(Contract.ContentSchemaDigest);
			byte[] SchemaBin;
			XmlSchema Schema;

			lock (this.schemas)
			{
				if (this.schemas.TryGetValue(SchemaKey, out KeyValuePair<byte[], XmlSchema> P))
				{
					SchemaBin = P.Key;
					Schema = P.Value;
				}
				else
				{
					SchemaBin = null;
					Schema = null;
				}
			}

			if (!(Schema is null))
				Schemas[Contract.ForMachinesNamespace] = Schema;

			string[] Namespaces = new string[Schemas.Count];
			Schemas.Keys.CopyTo(Namespaces, 0);

			string ContractComponent;
			int i = Contract.ContractId.IndexOf('@');
			if (i < 0)
				ContractComponent = this.componentAddress;
			else
				ContractComponent = Contract.ContractId[(i + 1)..];

			foreach (string Namespace in Namespaces)
			{
				if (Schemas.TryGetValue(Namespace, out Schema) && !(Schema is null))
					continue;

				TaskCompletionSource<ContractStatus> T = new TaskCompletionSource<ContractStatus>();
				SchemaDigest SchemaDigest;

				if (Namespace == Contract.ForMachinesNamespace)
					SchemaDigest = new SchemaDigest(Contract.ContentSchemaHashFunction, Contract.ContentSchemaDigest);
				else
					SchemaDigest = null;

				await this.GetSchema(ContractComponent, Namespace, SchemaDigest, (_, e) =>
				{
					if (e.Ok)
					{
						try
						{
							SchemaBin = e.Schema;
							using (MemoryStream ms = new MemoryStream(SchemaBin))
							{
								Schema = XSL.LoadSchema(ms, string.Empty);
							}

							Schemas[Namespace] = Schema;

							if (Namespace == Contract.ForMachinesNamespace)
							{
								byte[] Digest = Hashes.ComputeHash(Contract.ContentSchemaHashFunction, SchemaBin);

								if (Convert.ToBase64String(Digest) != Convert.ToBase64String(Contract.ContentSchemaDigest))
								{
									T.TrySetResult(ContractStatus.FraudulentSchema);
									return Task.CompletedTask;
								}

								lock (this.schemas)
								{
									this.schemas[SchemaKey] = new KeyValuePair<byte[], XmlSchema>(SchemaBin, Schema);
								}
							}
							else
							{
								lock (this.schemas)
								{
									this.schemas[Namespace] = new KeyValuePair<byte[], XmlSchema>(SchemaBin, Schema);
								}
							}

							T.TrySetResult(ContractStatus.Valid);
						}
						catch (Exception)
						{
							T.TrySetResult(ContractStatus.CorruptSchema);
						}
					}
					else
						T.TrySetResult(ContractStatus.NoSchemaAccess);

					return Task.CompletedTask;
				}, null);

				ContractStatus Temp = await T.Task;
				if (Temp != ContractStatus.Valid)
				{
					await this.ReturnStatus(Temp, Callback, State);
					return;
				}
			}

			try
			{
				XmlSchema[] Schemas2 = new XmlSchema[Schemas.Count];
				Schemas.Values.CopyTo(Schemas2, 0);

				XSL.Validate(string.Empty, Doc, Contract.ForMachinesLocalName, Contract.ForMachinesNamespace, Schemas2);
			}
			catch (Exception)
			{
				await this.ReturnStatus(ContractStatus.FraudulentMachineReadable, Callback, State);
				return;
			}

			StringBuilder Xml = new StringBuilder();
			Contract.Serialize(Xml, false, false, false, false, false, false, false);
			byte[] Data = Encoding.UTF8.GetBytes(Xml.ToString());
			Dictionary<string, LegalIdentity> Identities = new Dictionary<string, LegalIdentity>();

			foreach (ClientSignature Signature in Contract.ClientSignatures)
			{
				LegalIdentity Identity = await this.ValidateSignatureAsync(Signature.LegalId, Data, Signature.DigitalSignature);
				if (Identity is null)
				{
					await this.ReturnStatus(ContractStatus.ClientSignatureInvalid, Callback, State);
					return;
				}

				IdentityStatus Status = await this.ValidateAsync(Identity);
				if (Status != IdentityStatus.Valid)
				{
					await this.ReturnStatus(ContractStatus.ClientIdentityInvalid, Callback, State);
					return;
				}

				Identities[Signature.LegalId] = Identity;
			}

			if (!(Contract.Attachments is null))
			{
				foreach (Attachment Attachment in Contract.Attachments)
				{
					if (string.IsNullOrEmpty(Attachment.Url))
					{
						await this.ReturnStatus(ContractStatus.AttachmentLacksUrl, Callback, State);
						return;
					}

					try
					{
						KeyValuePair<string, TemporaryFile> P = await this.GetAttachmentAsync(Attachment.Url, SignWith.LatestApprovedId, 30000);
						bool? IsValid;
						using TemporaryFile File = P.Value;

						if (P.Key != Attachment.ContentType)
						{
							await this.ReturnStatus(ContractStatus.AttachmentInconsistency, Callback, State);
							return;
						}

						File.Position = 0;

						if (Identities.TryGetValue(Attachment.LegalId, out LegalIdentity Identity))
							IsValid = this.ValidateSignature(Identity, File, Attachment.Signature);
						else
						{
							MemoryStream ms = new MemoryStream();
							await File.CopyToAsync(ms);
							Data = ms.ToArray();

							try
							{
								Identity = await this.ValidateSignatureAsync(Attachment.LegalId, Data, Attachment.Signature);
								Identities[Attachment.LegalId] = Identity;
								IsValid = true;
							}
							catch (Exception)
							{
								IsValid = false;
							}
						}

						if (IsValid.HasValue)
						{
							if (!IsValid.Value)
							{
								await this.ReturnStatus(ContractStatus.AttachmentSignatureInvalid, Callback, State);
								return;
							}
						}
						else
						{
							await this.ReturnStatus(ContractStatus.AttachmentSignatureInvalid, Callback, State);
							return;
						}
					}
					catch (Exception)
					{
						await this.ReturnStatus(ContractStatus.AttachmentUnavailable, Callback, State);
						return;
					}
				}
			}

			if (Contract.ServerSignature is null)
			{
				await this.ReturnStatus(ContractStatus.NoProviderSignature, Callback, State);
				return;
			}

			Xml.Clear();
			Contract.Serialize(Xml, false, true, true, true, true, false, false);
			Data = Encoding.UTF8.GetBytes(Xml.ToString());

			bool HasOldPublicKey;

			lock (this.publicKeys)
			{
				HasOldPublicKey = this.publicKeys.ContainsKey(Contract.Provider);
			}

			await this.GetServerPublicKey(Contract.Provider, async (Sender, e) =>
			{
				if (e.Ok && !(e.Key is null))
				{
					bool Valid = e.Key.Verify(Data, Contract.ServerSignature.DigitalSignature);

					if (Valid)
					{

						await this.ReturnStatus(ContractStatus.Valid, Callback, State);
						return;
					}

					if (!HasOldPublicKey)
					{
						await this.ReturnStatus(ContractStatus.ProviderSignatureInvalid, Callback, State);
						return;
					}

					lock (this.publicKeys)
					{
						this.publicKeys.Remove(Contract.Provider);
					}

					await this.GetServerPublicKey(Contract.Provider, (sender2, e2) =>
					{
						if (e2.Ok && !(e2.Key is null))
						{
							if (e.Key.Equals(e2.Key))
								return this.ReturnStatus(ContractStatus.ProviderSignatureInvalid, Callback, State);

							Valid = e2.Key.Verify(Data, Contract.ServerSignature.DigitalSignature);

							if (Valid)
								return this.ReturnStatus(ContractStatus.Valid, Callback, State);
							else
								return this.ReturnStatus(ContractStatus.ProviderSignatureInvalid, Callback, State);
						}
						else
							return this.ReturnStatus(ContractStatus.NoProviderPublicKey, Callback, State);

					}, State);
				}
				else
					await this.ReturnStatus(ContractStatus.NoProviderPublicKey, Callback, State);

			}, State);
		}

		private readonly Dictionary<string, KeyValuePair<byte[], XmlSchema>> schemas = new Dictionary<string, KeyValuePair<byte[], XmlSchema>>();

		private Task ReturnStatus(ContractStatus Status, EventHandlerAsync<ContractValidationEventArgs> Callback, object State)
		{
			return Callback.Raise(this, new ContractValidationEventArgs(Status, State));
		}

		private static async Task<bool> IsHumanReadableWellDefined(HumanReadableText[] Texts)
		{
			if (Texts is null)
				return false;

			foreach (HumanReadableText Text in Texts)
			{
				if (!(await Text.IsWellDefined() is null))
					return false;
			}

			return true;
		}

		private static async Task<bool> IsHumanReadableWellDefined(Contract Contract)
		{
			if (!await IsHumanReadableWellDefined(Contract.ForHumans))
				return false;

			if (!(Contract.Roles is null))
			{
				foreach (Role Role in Contract.Roles)
				{
					if (!await IsHumanReadableWellDefined(Role.Descriptions))
						return false;
				}
			}

			if (!(Contract.Parameters is null))
			{
				foreach (Parameter Parameter in Contract.Parameters)
				{
					if (!await IsHumanReadableWellDefined(Parameter.Descriptions))
						return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Validates a smart contract.
		/// </summary>
		/// <param name="Contract">Contract to validate</param>
		/// <returns>Status of validation.</returns>
		public Task<ContractStatus> ValidateAsync(Contract Contract)
		{
			return this.ValidateAsync(Contract, true);
		}

		/// <summary>
		/// Validates a smart contract.
		/// </summary>
		/// <param name="Contract">Contract to validate</param>
		/// <param name="ValidateState">If the state attribute should be validated. (Default=true)</param>
		/// <returns>Status of validation.</returns>
		public async Task<ContractStatus> ValidateAsync(Contract Contract, bool ValidateState)
		{
			TaskCompletionSource<ContractStatus> Result = new TaskCompletionSource<ContractStatus>();

			await this.Validate(Contract, ValidateState, (Sender, e) =>
			{
				Result.TrySetResult(e.Status);
				return Task.CompletedTask;
			}, null);

			return await Result.Task;
		}

		#endregion

		#region Can Sign As

		/// <summary>
		/// Checks if an identity can sign for another reference identity (i.e. the old might have been obsoleted and/or
		/// compromized, and the signatory ID is a new ID for the same account and person).
		/// </summary>
		/// <param name="ReferenceId">Reference ID</param>
		/// <param name="SignatoryId">ID used for signature</param>
		/// <returns>If the Signatory ID can be used to sign for the reference ID.</returns>
		public async Task<bool> CanSignAs(CaseInsensitiveString ReferenceId, CaseInsensitiveString SignatoryId)
		{
			string ReferenceDomain = XmppClient.GetDomain(ReferenceId);
			string SignatoryDomain = XmppClient.GetDomain(SignatoryId);

			if (ReferenceDomain != SignatoryDomain)
				return false;

			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<canSignAs xmlns='");
			Xml.Append(NamespaceLegalIdentitiesCurrent);
			Xml.Append("' referenceId='");
			Xml.Append(XML.Encode(ReferenceId));
			Xml.Append("' signatoryId='");
			Xml.Append(XML.Encode(SignatoryId));
			Xml.Append("'/>");

			await this.client.SendIqGet(ReferenceDomain, Xml.ToString(), (_, e) =>
			{
				Result.TrySetResult(e.Ok);
				return Task.CompletedTask;
			}, null);

			return await Result.Task;
		}

		#endregion

		#region SendContractProposal

		/// <summary>
		/// Sends a contract proposal to a recipient. If the contract contains encrypted parameters, and End-to-End encryption is
		/// enabled, the proposal will be sent End-to-End encrypted with the shared secret.
		/// </summary>
		/// <param name="Contract">Proposed contract.</param>
		/// <param name="Role">Proposed role of recipient.</param>
		/// <param name="To">Recipient Address (Bare or Full JID).</param>
		public Task SendContractProposal(Contract Contract, string Role, string To)
		{
			return this.SendContractProposal(Contract, Role, To, string.Empty);
		}

		/// <summary>
		/// Sends a contract proposal to a recipient. If the contract contains encrypted parameters, and End-to-End encryption is
		/// enabled, the proposal will be sent End-to-End encrypted with the shared secret.
		/// </summary>
		/// <param name="Contract">Proposed contract.</param>
		/// <param name="Role">Proposed role of recipient.</param>
		/// <param name="To">Recipient Address (Bare or Full JID).</param>
		/// <param name="Message">Optional message included in message.</param>
		public async Task SendContractProposal(Contract Contract, string Role, string To, string Message)
		{
			if (Contract.HasEncryptedParameters)
			{
				Tuple<SymmetricCipherAlgorithms, string, byte[]> T = await this.TryLoadContractSharedSecret(Contract.ContractId);

				if (!(T is null))
				{
					await this.SendContractProposal(Contract.ContractId, Role, To, Message, T.Item3, T.Item1);
					return;
				}
			}

			await this.SendContractProposal(Contract.ContractId, Role, To, Message, null, SymmetricCipherAlgorithms.Aes256);
		}

		/// <summary>
		/// Sends a contract proposal to a recipient.
		/// </summary>
		/// <param name="ContractId">ID of proposed contract.</param>
		/// <param name="Role">Proposed role of recipient.</param>
		/// <param name="To">Recipient Address (Bare or Full JID).</param>
		public Task SendContractProposal(string ContractId, string Role, string To)
		{
			return this.SendContractProposal(ContractId, Role, To, string.Empty);
		}

		/// <summary>
		/// Sends a contract proposal to a recipient.
		/// </summary>
		/// <param name="ContractId">ID of proposed contract.</param>
		/// <param name="Role">Proposed role of recipient.</param>
		/// <param name="To">Recipient Address (Bare or Full JID).</param>
		/// <param name="Message">Optional message included in message.</param>
		public Task SendContractProposal(string ContractId, string Role, string To, string Message)
		{
			return this.SendContractProposal(ContractId, Role, To, Message, null, SymmetricCipherAlgorithms.Aes256);
		}

		/// <summary>
		/// Sends a contract proposal to a recipient.
		/// </summary>
		/// <param name="ContractId">ID of proposed contract.</param>
		/// <param name="Role">Proposed role of recipient.</param>
		/// <param name="To">Recipient Address (Bare or Full JID).</param>
		/// <param name="Message">Optional message included in message.</param>
		/// <param name="Key">Key used to protect confidential parameters in contract.</param>
		/// <param name="KeyAlgorithm">Key algorithm used to protect confidential parameters in contract.</param>
		public async Task SendContractProposal(string ContractId, string Role, string To, string Message, byte[] Key,
			SymmetricCipherAlgorithms KeyAlgorithm)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<contractProposal xmlns=\"");
			Xml.Append(NamespaceSmartContractsCurrent);
			Xml.Append("\" contractId=\"");
			Xml.Append(XML.Encode(ContractId));
			Xml.Append("\" role=\"");
			Xml.Append(XML.Encode(Role));

			if (!string.IsNullOrEmpty(Message))
			{
				Xml.Append("\" message=\"");
				Xml.Append(XML.Encode(Message));
			}

			Xml.Append('"');

			if (Key is null)
			{
				Xml.Append("/>");

				if (this.localE2eEndpoint is null)
				{
					await this.client.SendMessage(MessageType.Normal, To, Xml.ToString(), string.Empty, string.Empty, string.Empty,
						string.Empty, string.Empty);
				}
				else
				{
					await this.localE2eEndpoint.SendMessage(this.client, E2ETransmission.NormalIfNotE2E, QoSLevel.Unacknowledged, MessageType.Normal,
						string.Empty, To, Xml.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, null, null);
				}
			}
			else
			{
				Xml.Append("><sharedSecret key=\"");
				Xml.Append(Convert.ToBase64String(Key));
				Xml.Append("\" algorithm=\"");

				switch (KeyAlgorithm)
				{
					case SymmetricCipherAlgorithms.Aes256:
						Xml.Append("aes");
						break;

					case SymmetricCipherAlgorithms.ChaCha20:
						Xml.Append("cha");
						break;

					case SymmetricCipherAlgorithms.AeadChaCha20Poly1305:
						Xml.Append("acp");
						break;

					default:
						throw new ArgumentException("Algorithm not recognized.", nameof(KeyAlgorithm));
				}

				Xml.Append("\"/></contractProposal>");

				if (this.localE2eEndpoint is null)
					throw new InvalidOperationException("End-to-End encryption not enabled.");

				if (XmppClient.GetBareJID(To) == To)
				{
					RosterItem Item = this.client[To]
						?? throw new ArgumentException("Recipient not in roster.", nameof(To));

					To = Item.LastPresenceFullJid;
					if (string.IsNullOrEmpty(To))
						throw new ArgumentException("Recipient not online.", nameof(To));
				}

				await this.localE2eEndpoint.SendMessage(this.client, E2ETransmission.AssertE2E, QoSLevel.Unacknowledged, MessageType.Normal,
					string.Empty, To, Xml.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, null, null);
			}
		}

		private async Task ContractProposalMessageHandler(object Sender, MessageEventArgs e)
		{
			string ContractId = XML.Attribute(e.Content, "contractId");
			string Role = XML.Attribute(e.Content, "role");
			string Message = XML.Attribute(e.Content, "message");
			byte[] Key = null;
			SymmetricCipherAlgorithms KeyAlgorithm = SymmetricCipherAlgorithms.Aes256;

			foreach (XmlNode N in e.Content.ChildNodes)
			{
				if (N is XmlElement E && E.LocalName == "sharedSecret" && E.NamespaceURI == e.Content.NamespaceURI)
				{
					if (!e.UsesE2eEncryption)
					{
						this.client.Error("Confidential Proposal not sent using end-to-end encryption. Message discarded.");
						return;
					}

					try
					{
						Key = Convert.FromBase64String(XML.Attribute(E, "key"));
					}
					catch (Exception)
					{
						this.client.Error("Invalid base64-encoded shared secret. Message discarded.");
						return;
					}

					string Cipher = XML.Attribute(E, "algorithm");

					switch (Cipher)
					{
						case "aes":
							KeyAlgorithm = SymmetricCipherAlgorithms.Aes256;
							break;

						case "cha":
							KeyAlgorithm = SymmetricCipherAlgorithms.ChaCha20;
							break;

						case "acp":
							KeyAlgorithm = SymmetricCipherAlgorithms.AeadChaCha20Poly1305;
							break;

						default:
							this.client.Error("Unrecognized key algorithm. Message discarded.");
							return;
					}
				}
			}

			if (!(Key is null))
				await this.SaveContractSharedSecret(ContractId, e.FromBareJID, Key, KeyAlgorithm, true);

			await this.ContractProposalReceived.Raise(this, new ContractProposalEventArgs(e, ContractId, Role, Message, Key, KeyAlgorithm));
		}

		internal async Task<bool> SaveContractSharedSecret(string ContractId, string CreatorJid, byte[] Key,
			SymmetricCipherAlgorithms KeyAlgorithm, bool OnlyIfNew)
		{
			string Name = this.contractKeySettingsPrefix + ContractId;

			if (OnlyIfNew)
			{
				string s = await RuntimeSettings.GetAsync(Name, string.Empty);
				if (!string.IsNullOrEmpty(s))
					return false;
			}

			string Value = KeyAlgorithm.ToString() + "|" + CreatorJid + "|" + Convert.ToBase64String(Key);

			await RuntimeSettings.SetAsync(Name, Value);

			return true;
		}

		internal async Task<Tuple<SymmetricCipherAlgorithms, string, byte[]>> TryLoadContractSharedSecret(string ContractId)
		{
			string Name = this.contractKeySettingsPrefix + ContractId;
			string s = await RuntimeSettings.GetAsync(Name, string.Empty);

			if (string.IsNullOrEmpty(s))
				return null;

			string[] Parts = s.Split('|');
			if (Parts.Length != 3)
				return null;

			if (!Enum.TryParse(Parts[0], out SymmetricCipherAlgorithms Algorithm))
				return null;

			string CreatorJid = Parts[1];
			byte[] Key;

			try
			{
				Key = Convert.FromBase64String(Parts[2]);
			}
			catch (Exception)
			{
				return null;
			}

			return new Tuple<SymmetricCipherAlgorithms, string, byte[]>(Algorithm, CreatorJid, Key);
		}

		/// <summary>
		/// Event raised when a new contract proposal has been received.
		/// </summary>
		public event EventHandlerAsync<ContractProposalEventArgs> ContractProposalReceived = null;

		#endregion

		#region Get Schemas

		/// <summary>
		/// Gets available schemas.
		/// </summary>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetSchemas(EventHandlerAsync<SchemaReferencesEventArgs> Callback, object State)
		{
			return this.GetSchemas(this.componentAddress, Callback, State);
		}

		/// <summary>
		/// Gets available schemas.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetSchemas(string Address, EventHandlerAsync<SchemaReferencesEventArgs> Callback, object State)
		{
			return this.client.SendIqGet(Address, "<getSchemas xmlns='" + NamespaceSmartContractsCurrent + "'/>",
				async (Sender, e) =>
				{
					XmlElement E = e.FirstElement;
					List<SchemaReference> Schemas = new List<SchemaReference>();

					if (e.Ok && !(E is null) && E.LocalName == "schemas")
					{
						foreach (XmlNode N in E.ChildNodes)
						{
							if (N is XmlElement E2 && E2.LocalName == "schemaRef")
							{
								string Namespace = XML.Attribute(E2, "namespace");
								List<SchemaDigest> Digests = new List<SchemaDigest>();

								foreach (XmlNode N2 in E2.ChildNodes)
								{
									if (N2 is XmlElement E3 && E3.LocalName == "digest")
									{
										if (!Enum.TryParse(XML.Attribute(E3, "function"), out HashFunction Function))
											continue;

										byte[] Digest = Convert.FromBase64String(E3.InnerText);

										Digests.Add(new SchemaDigest(Function, Digest));
									}
								}

								Schemas.Add(new SchemaReference(Namespace, Digests.ToArray()));
							}
						}
					}
					else
						e.Ok = false;

					await Callback.Raise(this, new SchemaReferencesEventArgs(e, Schemas.ToArray()));

				}, State);
		}

		/// <summary>
		/// Gets available schemas.
		/// </summary>
		/// <returns>XML Schema references.</returns>
		public Task<SchemaReference[]> GetSchemasAsync()
		{
			return this.GetSchemasAsync(this.componentAddress);
		}

		/// <summary>
		/// Gets available schemas.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <returns>XML Schema references.</returns>
		public async Task<SchemaReference[]> GetSchemasAsync(string Address)
		{
			TaskCompletionSource<SchemaReference[]> Result = new TaskCompletionSource<SchemaReference[]>();

			await this.GetSchemas(Address, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.References);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to get schemas."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		#endregion

		#region Get Schema

		/// <summary>
		/// Gets a schema.
		/// </summary>
		/// <param name="Namespace">Namespace of schema to get.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetSchema(string Namespace, EventHandlerAsync<SchemaEventArgs> Callback, object State)
		{
			return this.GetSchema(this.componentAddress, Namespace, null, Callback, State);
		}

		/// <summary>
		/// Gets a schema.
		/// </summary>
		/// <param name="Namespace">Namespace of schema to get.</param>
		/// <param name="Digest">Specifies a specific schema version. If not provided (or null), the most recently recorded schema will be returned.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetSchema(string Namespace, SchemaDigest Digest, EventHandlerAsync<SchemaEventArgs> Callback, object State)
		{
			return this.GetSchema(this.componentAddress, Namespace, Digest, Callback, State);
		}

		/// <summary>
		/// Gets a schema.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Namespace">Namespace of schema to get.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetSchema(string Address, string Namespace, EventHandlerAsync<SchemaEventArgs> Callback, object State)
		{
			return this.GetSchema(Address, Namespace, null, Callback, State);
		}

		/// <summary>
		/// Gets a schema.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Namespace">Namespace of schema to get.</param>
		/// <param name="Digest">Specifies a specific schema version. If not provided (or null), the most recently recorded schema will be returned.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetSchema(string Address, string Namespace, SchemaDigest Digest, EventHandlerAsync<SchemaEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getSchema xmlns='");
			Xml.Append(NamespaceSmartContractsCurrent);
			Xml.Append("' namespace='");
			Xml.Append(XML.Encode(Namespace));

			if (Digest is null)
				Xml.Append("'/>");
			else
			{
				Xml.Append("'><digest function='");
				Xml.Append(Digest.Function.ToString());
				Xml.Append("'>");
				Xml.Append(Convert.ToBase64String(Digest.Digest));
				Xml.Append("</digest></getSchema>");
			}

			return this.client.SendIqGet(Address, Xml.ToString(),
				async (Sender, e) =>
				{
					XmlElement E = e.FirstElement;
					byte[] Schema = null;

					if (e.Ok && !(E is null) && E.LocalName == "schema")
						Schema = Convert.FromBase64String(E.InnerText);
					else
						e.Ok = false;

					await Callback.Raise(this, new SchemaEventArgs(e, Schema));

				}, State);
		}

		/// <summary>
		/// Gets a schema.
		/// </summary>
		/// <param name="Namespace">Namespace of schema to get.</param>
		/// <returns>Binary XML schema.</returns>
		public Task<byte[]> GetSchemaAsync(string Namespace)
		{
			return this.GetSchemaAsync(this.componentAddress, Namespace, null);
		}

		/// <summary>
		/// Gets a schema.
		/// </summary>
		/// <param name="Namespace">Namespace of schema to get.</param>
		/// <param name="Digest">Specifies a specific schema version. If not provided (or null), the most recently recorded schema will be returned.</param>
		/// <returns>Binary XML schema.</returns>
		public Task<byte[]> GetSchemaAsync(string Namespace, SchemaDigest Digest)
		{
			return this.GetSchemaAsync(this.componentAddress, Namespace, Digest);
		}

		/// <summary>
		/// Gets a schema.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Namespace">Namespace of schema to get.</param>
		/// <returns>Binary XML schema.</returns>
		public Task<byte[]> GetSchemaAsync(string Address, string Namespace)
		{
			return this.GetSchemaAsync(Address, Namespace, null);
		}

		/// <summary>
		/// Gets a schema.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Namespace">Namespace of schema to get.</param>
		/// <param name="Digest">Specifies a specific schema version. If not provided (or null), the most recently recorded schema will be returned.</param>
		/// <returns>Binary XML schema.</returns>
		public async Task<byte[]> GetSchemaAsync(string Address, string Namespace, SchemaDigest Digest)
		{
			TaskCompletionSource<byte[]> Result = new TaskCompletionSource<byte[]>();

			await this.GetSchema(Address, Namespace, Digest, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Schema);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to get schema."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		#endregion

		#region Get Legal Identities of a contract

		/// <summary>
		/// Gets available legal identities related to a contract.
		/// </summary>
		/// <param name="ContractId">Get legal identities related to the contract identified by this identity.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetContractLegalIdentities(string ContractId, EventHandlerAsync<LegalIdentitiesEventArgs> Callback, object State)
		{
			return this.GetContractLegalIdentities(this.GetTrustProvider(ContractId), ContractId, false, true, Callback, State);
		}

		/// <summary>
		/// Gets available legal identities related to a contract.
		/// </summary>
		/// <param name="ContractId">Get legal identities related to the contract identified by this identity.</param>
		/// <param name="Current">If current legal identities are to be returned. (Default=false).</param>
		/// <param name="Historic">If legal identities at the time of signature are to be returned. (Default=true).</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetContractLegalIdentities(string ContractId, bool Current, bool Historic, EventHandlerAsync<LegalIdentitiesEventArgs> Callback, object State)
		{
			return this.GetContractLegalIdentities(this.GetTrustProvider(ContractId), ContractId, Current, Historic, Callback, State);
		}

		/// <summary>
		/// Gets available legal identities related to a contract.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ContractId">Get legal identities related to the contract identified by this identity.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetContractLegalIdentities(string Address, string ContractId, EventHandlerAsync<LegalIdentitiesEventArgs> Callback, object State)
		{
			return this.GetContractLegalIdentities(Address, ContractId, false, true, Callback, State);
		}

		/// <summary>
		/// Gets available legal identities related to a contract.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ContractId">Get legal identities related to the contract identified by this identity.</param>
		/// <param name="Current">If current legal identities are to be returned. (Default=false).</param>
		/// <param name="Historic">If legal identities at the time of signature are to be returned. (Default=true).</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetContractLegalIdentities(string Address, string ContractId, bool Current, bool Historic, EventHandlerAsync<LegalIdentitiesEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getLegalIdentities xmlns='");
			Xml.Append(NamespaceSmartContractsCurrent);
			Xml.Append("' contractId='");
			Xml.Append(XML.Encode(ContractId));
			Xml.Append("' current='");
			Xml.Append(CommonTypes.Encode(Current));
			Xml.Append("' historic='");
			Xml.Append(CommonTypes.Encode(Historic));
			Xml.Append("'/>");

			return this.client.SendIqGet(Address, Xml.ToString(), this.IdentitiesResponse, new object[] { Callback, State });
		}

		/// <summary>
		/// Gets available legal identities related to a contract.
		/// </summary>
		/// <param name="ContractId">Get legal identities related to the contract identified by this identity.</param>
		/// <returns>Legal identities.</returns>
		public Task<LegalIdentity[]> GetContractLegalIdentitiesAsync(string ContractId)
		{
			return this.GetContractLegalIdentitiesAsync(this.GetTrustProvider(ContractId), ContractId, false, true);
		}

		/// <summary>
		/// Gets available legal identities related to a contract.
		/// </summary>
		/// <param name="ContractId">Get legal identities related to the contract identified by this identity.</param>
		/// <param name="Current">If current legal identities are to be returned. (Default=false).</param>
		/// <param name="Historic">If legal identities at the time of signature are to be returned. (Default=true).</param>
		/// <returns>Legal identities.</returns>
		public Task<LegalIdentity[]> GetContractLegalIdentitiesAsync(string ContractId, bool Current, bool Historic)
		{
			return this.GetContractLegalIdentitiesAsync(this.GetTrustProvider(ContractId), ContractId, Current, Historic);
		}

		/// <summary>
		/// Gets available legal identities related to a contract.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ContractId">Get legal identities related to the contract identified by this identity.</param>
		/// <returns>Legal identities.</returns>
		public Task<LegalIdentity[]> GetContractLegalIdentitiesAsync(string Address, string ContractId)
		{
			return this.GetContractLegalIdentitiesAsync(Address, ContractId, false, true);
		}

		/// <summary>
		/// Gets available legal identities related to a contract.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ContractId">Get legal identities related to the contract identified by this identity.</param>
		/// <param name="Current">If current legal identities are to be returned. (Default=false).</param>
		/// <param name="Historic">If legal identities at the time of signature are to be returned. (Default=true).</param>
		/// <returns>Legal identities.</returns>
		public async Task<LegalIdentity[]> GetContractLegalIdentitiesAsync(string Address, string ContractId, bool Current, bool Historic)
		{
			TaskCompletionSource<LegalIdentity[]> Result = new TaskCompletionSource<LegalIdentity[]>();

			await this.GetContractLegalIdentities(Address, ContractId, Current, Historic, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Identities);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to get legal identities."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		#endregion

		#region Get Network Identities of a contract

		/// <summary>
		/// Gets available network identities related to a contract.
		/// </summary>
		/// <param name="ContractId">Get network identities related to the contract identified by this identity.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetContractNetworkIdentities(string ContractId, EventHandlerAsync<NetworkIdentitiesEventArgs> Callback, object State)
		{
			return this.GetContractNetworkIdentities(this.GetTrustProvider(ContractId), ContractId, Callback, State);
		}

		/// <summary>
		/// Gets available network identities related to a contract.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ContractId">Get network identities related to the contract identified by this identity.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task GetContractNetworkIdentities(string Address, string ContractId, EventHandlerAsync<NetworkIdentitiesEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getNetworkIdentities xmlns='");
			Xml.Append(NamespaceSmartContractsCurrent);
			Xml.Append("' contractId='");
			Xml.Append(XML.Encode(ContractId));
			Xml.Append("'/>");

			return this.client.SendIqGet(Address, Xml.ToString(), async (Sender, e) =>
			{
				NetworkIdentity[] Identities = null;
				XmlElement E;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "networkIdentities")
				{
					List<NetworkIdentity> IdentitiesList = new List<NetworkIdentity>();

					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2 && E2.LocalName == "networkIdentity")
						{
							string BareJid = XML.Attribute(E2, "bareJid");
							string LegalId = XML.Attribute(E2, "legalId");

							IdentitiesList.Add(new NetworkIdentity(BareJid, LegalId));
						}
					}

					Identities = IdentitiesList.ToArray();
				}
				else
					e.Ok = false;

				await Callback.Raise(this, new NetworkIdentitiesEventArgs(e, Identities));
			}, State);
		}

		/// <summary>
		/// Gets available network identities related to a contract.
		/// </summary>
		/// <param name="ContractId">Get network identities related to the contract identified by this identity.</param>
		/// <returns>Network identities.</returns>
		public Task<NetworkIdentity[]> GetContractNetworkIdentitiesAsync(string ContractId)
		{
			return this.GetContractNetworkIdentitiesAsync(this.GetTrustProvider(ContractId), ContractId);
		}

		/// <summary>
		/// Gets available network identities related to a contract.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ContractId">Get network identities related to the contract identified by this identity.</param>
		/// <returns>Network identities.</returns>
		public async Task<NetworkIdentity[]> GetContractNetworkIdentitiesAsync(string Address, string ContractId)
		{
			TaskCompletionSource<NetworkIdentity[]> Result = new TaskCompletionSource<NetworkIdentity[]>();

			await this.GetContractNetworkIdentities(Address, ContractId, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Identities);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to get network identities."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		#endregion

		#region Search Public Contracts

		/// <summary>
		/// Performs a search of public smart contracts.
		/// </summary>
		/// <param name="Filter">Search filters.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task Search(SearchFilter[] Filter, EventHandlerAsync<SearchResultEventArgs> Callback, object State)
		{
			return this.Search(this.componentAddress, 0, int.MaxValue, Filter, Callback, State);
		}

		/// <summary>
		/// Performs a search of public smart contracts.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Filter">Search filters.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task Search(string Address, SearchFilter[] Filter, EventHandlerAsync<SearchResultEventArgs> Callback, object State)
		{
			return this.Search(Address, 0, int.MaxValue, Filter, Callback, State);
		}

		/// <summary>
		/// Performs a search of public smart contracts.
		/// </summary>
		/// <param name="Offset">Result will start with the response at this offset into result set.</param>
		/// <param name="MaxCount">Result will be limited to this number of items.</param>
		/// <param name="Filter">Search filters.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task Search(int Offset, int MaxCount, SearchFilter[] Filter, EventHandlerAsync<SearchResultEventArgs> Callback, object State)
		{
			return this.Search(this.componentAddress, Offset, MaxCount, Filter, Callback, State);
		}

		/// <summary>
		/// Performs a search of public smart contracts.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Offset">Result will start with the response at this offset into result set.</param>
		/// <param name="MaxCount">Result will be limited to this number of items.</param>
		/// <param name="Filter">Search filters.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task Search(string Address, int Offset, int MaxCount, SearchFilter[] Filter, EventHandlerAsync<SearchResultEventArgs> Callback, object State)
		{
			if (Offset < 0)
				throw new ArgumentException("Offsets cannot be negative.", nameof(Offset));

			if (MaxCount <= 0)
				throw new ArgumentException("Must be postitive.", nameof(MaxCount));

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<searchPublicContracts xmlns='");
			Xml.Append(NamespaceSmartContractsCurrent);

			if (Offset > 0)
			{
				Xml.Append("' offset='");
				Xml.Append(Offset.ToString());
			}

			if (MaxCount < int.MaxValue)
			{
				Xml.Append("' maxCount='");
				Xml.Append(MaxCount.ToString());
			}

			Xml.Append("'>");

			Filter = (SearchFilter[])Filter.Clone();
			Array.Sort(Filter, (f1, f2) => f1.Order - f2.Order);

			int PrevOrder = 0;
			int PrevOrderCount = 0;
			int Order;

			foreach (SearchFilter F in Filter)
			{
				Order = F.Order;
				if (Order != PrevOrder)
				{
					PrevOrder = Order;
					PrevOrderCount = 1;
				}
				else
				{
					PrevOrderCount++;
					if (PrevOrderCount >= F.MaxOccurs)
					{
						throw new ArgumentException("Maximum number of occurrences of " + F.GetType().FullName + " in a search is " +
							F.MaxOccurs.ToString() + ".", nameof(Filter));
					}
				}

				F.Serialize(Xml);
			}

			Xml.Append("</searchPublicContracts>");

			return this.client.SendIqGet(Address, Xml.ToString(), async (Sender, e) =>
			{
				XmlElement E = e.FirstElement;
				List<string> IDs = null;
				bool More = false;

				if (e.Ok && !(E is null) && E.LocalName == "searchResult")
				{
					More = XML.Attribute(E, "more", false);
					IDs = new List<string>();

					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2 && E2.LocalName == "ref")
						{
							string Id = XML.Attribute(E2, "id");
							IDs.Add(Id);
						}
					}
				}
				else
					e.Ok = false;

				await Callback.Raise(this, new SearchResultEventArgs(e, Offset, MaxCount, More, IDs?.ToArray()));
			}, State);
		}

		/// <summary>
		/// Performs a search of public smart contracts.
		/// </summary>
		/// <param name="Filter">Search filters.</param>
		/// <returns>Search result.</returns>
		public Task<SearchResultEventArgs> SearchAsync(SearchFilter[] Filter)
		{
			return this.SearchAsync(this.componentAddress, 0, int.MaxValue, Filter);
		}

		/// <summary>
		/// Performs a search of public smart contracts.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Filter">Search filters.</param>
		public Task<SearchResultEventArgs> SearchAsync(string Address, SearchFilter[] Filter)
		{
			return this.SearchAsync(Address, 0, int.MaxValue, Filter);
		}

		/// <summary>
		/// Performs a search of public smart contracts.
		/// </summary>
		/// <param name="Offset">Result will start with the response at this offset into result set.</param>
		/// <param name="MaxCount">Result will be limited to this number of items.</param>
		/// <param name="Filter">Search filters.</param>
		public Task<SearchResultEventArgs> SearchAsync(int Offset, int MaxCount, SearchFilter[] Filter)
		{
			return this.SearchAsync(this.componentAddress, Offset, MaxCount, Filter);
		}

		/// <summary>
		/// Performs a search of public smart contracts.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Offset">Result will start with the response at this offset into result set.</param>
		/// <param name="MaxCount">Result will be limited to this number of items.</param>
		/// <param name="Filter">Search filters.</param>
		public async Task<SearchResultEventArgs> SearchAsync(string Address, int Offset, int MaxCount, SearchFilter[] Filter)
		{
			TaskCompletionSource<SearchResultEventArgs> Result = new TaskCompletionSource<SearchResultEventArgs>();

			await this.Search(Address, Offset, MaxCount, Filter, (Sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, null);

			return await Result.Task;
		}

		#endregion

		#region Identity petitions

		/// <summary>
		/// Sends a petition to the owner of a legal identity, to access the information in the identity. The petition is not
		/// guaranteed to return a response. Response is returned if the recipient accepts the petition.
		/// When a petition is received, the <see cref="PetitionForIdentityReceived"/> event is raised.
		/// When a response to a petition is received, the <see cref="PetitionedIdentityResponseReceived"/> event is raised.
		/// </summary>
		/// <param name="LegalId">Legal Identity to petition.</param>
		/// <param name="PetitionId">A petition identifier. This identifier will follow the petition, and can be used
		/// to identify the petition request.</param>
		/// <param name="Purpose">Purpose string to show to the owner.</param>
		public Task PetitionIdentityAsync(string LegalId, string PetitionId, string Purpose)
		{
			return this.PetitionIdentityAsync(this.GetTrustProvider(LegalId), LegalId, PetitionId, Purpose, null, null, null);
		}

		/// <summary>
		/// Sends a petition to the owner of a legal identity, to access the information in the identity. The petition is not
		/// guaranteed to return a response. Response is returned if the recipient accepts the petition.
		/// When a petition is received, the <see cref="PetitionForIdentityReceived"/> event is raised.
		/// When a response to a petition is received, the <see cref="PetitionedIdentityResponseReceived"/> event is raised.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="LegalId">Legal Identity to petition.</param>
		/// <param name="PetitionId">A petition identifier. This identifier will follow the petition, and can be used
		/// to identify the petition request.</param>
		/// <param name="Purpose">Purpose string to show to the owner.</param>
		public Task PetitionIdentityAsync(string Address, string LegalId, string PetitionId, string Purpose)
		{
			return this.PetitionIdentityAsync(Address, LegalId, PetitionId, Purpose, null, null, null);
		}

		/// <summary>
		/// Sends a petition to the owner of a legal identity, to access the information in the identity. The petition is not
		/// guaranteed to return a response. Response is returned if the recipient accepts the petition.
		/// When a petition is received, the <see cref="PetitionForIdentityReceived"/> event is raised.
		/// When a response to a petition is received, the <see cref="PetitionedIdentityResponseReceived"/> event is raised.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="LegalId">Legal Identity to petition.</param>
		/// <param name="PetitionId">A petition identifier. This identifier will follow the petition, and can be used
		/// to identify the petition request.</param>
		/// <param name="Purpose">Purpose string to show to the owner.</param>
		/// <param name="ContextXml">Any machine-readable context XML element you want to include in the petition.</param>
		public Task PetitionIdentityAsync(string Address, string LegalId, string PetitionId, string Purpose, string ContextXml)
		{
			return this.PetitionIdentityAsync(Address, LegalId, PetitionId, Purpose, ContextXml, null, null);
		}

		/// <summary>
		/// Sends a petition to the owner of a legal identity, to access the information in the identity. The petition is not
		/// guaranteed to return a response. Response is returned if the recipient accepts the petition.
		/// When a petition is received, the <see cref="PetitionForIdentityReceived"/> event is raised.
		/// When a response to a petition is received, the <see cref="PetitionedIdentityResponseReceived"/> event is raised.
		/// </summary>
		/// <param name="LegalId">Legal Identity to petition.</param>
		/// <param name="PetitionId">A petition identifier. This identifier will follow the petition, and can be used
		/// to identify the petition request.</param>
		/// <param name="Purpose">Purpose string to show to the owner.</param>
		/// <param name="Properties">Optional property hints to provide to the remote party,
		/// highlighting which properties will be used in the response.</param>
		/// <param name="Attachments">Optional attachment hints to provide to the remote party,
		/// highlighting which attachments will be used in the response.</param>
		public Task PetitionIdentityAsync(string LegalId, string PetitionId, string Purpose,
			string[] Properties, string[] Attachments)
		{
			return this.PetitionIdentityAsync(this.GetTrustProvider(LegalId), LegalId, PetitionId, Purpose, null,
				Properties, Attachments);
		}

		/// <summary>
		/// Sends a petition to the owner of a legal identity, to access the information in the identity. The petition is not
		/// guaranteed to return a response. Response is returned if the recipient accepts the petition.
		/// When a petition is received, the <see cref="PetitionForIdentityReceived"/> event is raised.
		/// When a response to a petition is received, the <see cref="PetitionedIdentityResponseReceived"/> event is raised.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="LegalId">Legal Identity to petition.</param>
		/// <param name="PetitionId">A petition identifier. This identifier will follow the petition, and can be used
		/// to identify the petition request.</param>
		/// <param name="Purpose">Purpose string to show to the owner.</param>
		/// <param name="Properties">Optional property hints to provide to the remote party,
		/// highlighting which properties will be used in the response.</param>
		/// <param name="Attachments">Optional attachment hints to provide to the remote party,
		/// highlighting which attachments will be used in the response.</param>
		public Task PetitionIdentityAsync(string Address, string LegalId, string PetitionId, string Purpose,
			string[] Properties, string[] Attachments)
		{
			return this.PetitionIdentityAsync(Address, LegalId, PetitionId, Purpose, null,
				Properties, Attachments);
		}

		/// <summary>
		/// Sends a petition to the owner of a legal identity, to access the information in the identity. The petition is not
		/// guaranteed to return a response. Response is returned if the recipient accepts the petition.
		/// When a petition is received, the <see cref="PetitionForIdentityReceived"/> event is raised.
		/// When a response to a petition is received, the <see cref="PetitionedIdentityResponseReceived"/> event is raised.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="LegalId">Legal Identity to petition.</param>
		/// <param name="PetitionId">A petition identifier. This identifier will follow the petition, and can be used
		/// to identify the petition request.</param>
		/// <param name="Purpose">Purpose string to show to the owner.</param>
		/// <param name="ContextXml">Any machine-readable context XML element you want to include in the petition.</param>
		/// <param name="Properties">Optional property hints to provide to the remote party,
		/// highlighting which properties will be used in the response.</param>
		/// <param name="Attachments">Optional attachment hints to provide to the remote party,
		/// highlighting which attachments will be used in the response.</param>
		public async Task PetitionIdentityAsync(string Address, string LegalId, string PetitionId, string Purpose, string ContextXml,
			string[] Properties, string[] Attachments)
		{
			StringBuilder Xml = new StringBuilder();
			byte[] Nonce = this.RandomBytes(32);

			string NonceStr = Convert.ToBase64String(Nonce);
			byte[] Data = Encoding.UTF8.GetBytes(PetitionId + ":" + LegalId + ":" + Purpose + ":" + NonceStr + ":" + this.client.BareJID.ToLower());
			byte[] Signature = await this.SignAsync(Data, SignWith.LatestApprovedId);

			Xml.Append("<petitionIdentity xmlns='");
			Xml.Append(NamespaceLegalIdentitiesCurrent);
			Xml.Append("' id='");
			Xml.Append(XML.Encode(LegalId));
			Xml.Append("' pid='");
			Xml.Append(XML.Encode(PetitionId));
			Xml.Append("' purpose='");
			Xml.Append(XML.Encode(Purpose));
			Xml.Append("' nonce='");
			Xml.Append(NonceStr);
			Xml.Append("' s='");
			Xml.Append(Convert.ToBase64String(Signature));

			if (string.IsNullOrEmpty(ContextXml))
				Xml.Append("'/>");
			else
			{
				Xml.Append("'>");
				AppendHints(Xml, Properties, Attachments);
				Xml.Append(ContextXml);
				Xml.Append("</petitionIdentity>");
			}

			await this.client.IqSetAsync(Address, Xml.ToString());
		}

		private static void AppendHints(StringBuilder Xml, string[] Properties, string[] Attachments)
		{
			if (!(Properties is null))
			{
				Xml.Append("<properties>");

				foreach (string Property in Properties)
				{
					Xml.Append("<property>");
					Xml.Append(XML.Encode(Property));
					Xml.Append("</property>");
				}

				Xml.Append("</properties>");
			}

			if (!(Attachments is null))
			{
				Xml.Append("<attachments>");

				foreach (string Attachment in Attachments)
				{
					Xml.Append("<attachment>");
					Xml.Append(XML.Encode(Attachment));
					Xml.Append("</attachment>");
				}

				Xml.Append("</attachments>");
			}
		}

		/// <summary>
		/// Sends a response to a petition for information about a legal identity.
		/// When a petition is received, the <see cref="PetitionForIdentityReceived"/> event is raised.
		/// When a response to a petition is received, the <see cref="PetitionedIdentityResponseReceived"/> event is raised.
		/// </summary>
		/// <param name="LegalId">Legal Identity petitioned.</param>
		/// <param name="PetitionId">A petition identifier. This identifier will follow the petition, and can be used
		/// to identify the petition request.</param>
		/// <param name="RequestorFullJid">Full JID of requestor.</param>
		/// <param name="Response">If the petition is accepted (true) or rejected (false).</param>
		public Task PetitionIdentityResponseAsync(string LegalId, string PetitionId, string RequestorFullJid, bool Response)
		{
			return this.PetitionIdentityResponseAsync(this.GetTrustProvider(LegalId), LegalId, PetitionId, RequestorFullJid, Response, null);
		}

		/// <summary>
		/// Sends a response to a petition for information about a legal identity.
		/// When a petition is received, the <see cref="PetitionForIdentityReceived"/> event is raised.
		/// When a response to a petition is received, the <see cref="PetitionedIdentityResponseReceived"/> event is raised.
		/// </summary>
		/// <param name="LegalId">Legal Identity petitioned.</param>
		/// <param name="PetitionId">A petition identifier. This identifier will follow the petition, and can be used
		/// to identify the petition request.</param>
		/// <param name="RequestorFullJid">Full JID of requestor.</param>
		/// <param name="Response">If the petition is accepted (true) or rejected (false).</param>
		/// <param name="ContextXml">Any machine-readable context XML element you want to include in the petition response.</param>
		public Task PetitionIdentityResponseAsync(string LegalId, string PetitionId, string RequestorFullJid, bool Response, string ContextXml)
		{
			return this.PetitionIdentityResponseAsync(this.GetTrustProvider(LegalId), LegalId, PetitionId, RequestorFullJid, Response, ContextXml);
		}

		/// <summary>
		/// Sends a response to a petition for information about a legal identity.
		/// When a petition is received, the <see cref="PetitionForIdentityReceived"/> event is raised.
		/// When a response to a petition is received, the <see cref="PetitionedIdentityResponseReceived"/> event is raised.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="LegalId">Legal Identity petitioned.</param>
		/// <param name="PetitionId">A petition identifier. This identifier will follow the petition, and can be used
		/// to identify the petition request.</param>
		/// <param name="RequestorFullJid">Full JID of requestor.</param>
		/// <param name="Response">If the petition is accepted (true) or rejected (false).</param>
		public Task PetitionIdentityResponseAsync(string Address, string LegalId, string PetitionId, string RequestorFullJid, bool Response)
		{
			return this.PetitionIdentityResponseAsync(Address, LegalId, PetitionId, RequestorFullJid, Response, null);
		}

		/// <summary>
		/// Sends a response to a petition for information about a legal identity.
		/// When a petition is received, the <see cref="PetitionForIdentityReceived"/> event is raised.
		/// When a response to a petition is received, the <see cref="PetitionedIdentityResponseReceived"/> event is raised.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="LegalId">Legal Identity petitioned.</param>
		/// <param name="PetitionId">A petition identifier. This identifier will follow the petition, and can be used
		/// to identify the petition request.</param>
		/// <param name="RequestorFullJid">Full JID of requestor.</param>
		/// <param name="Response">If the petition is accepted (true) or rejected (false).</param>
		/// <param name="ContextXml">Any machine-readable context XML element you want to include in the petition response.</param>
		public async Task PetitionIdentityResponseAsync(string Address, string LegalId, string PetitionId, string RequestorFullJid, bool Response,
			string ContextXml)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<petitionIdentityResponse xmlns='");
			Xml.Append(NamespaceLegalIdentitiesCurrent);
			Xml.Append("' id='");
			Xml.Append(XML.Encode(LegalId));
			Xml.Append("' pid='");
			Xml.Append(XML.Encode(PetitionId));
			Xml.Append("' jid='");
			Xml.Append(XML.Encode(RequestorFullJid));
			Xml.Append("' response='");
			Xml.Append(CommonTypes.Encode(Response));

			if (string.IsNullOrEmpty(ContextXml))
				Xml.Append("'/>");
			else
			{
				Xml.Append("'>");
				Xml.Append(ContextXml);
				Xml.Append("</petitionIdentityResponse>");
			}

			await this.client.IqSetAsync(Address, Xml.ToString());
		}

		private async Task PetitionIdentityMessageHandler(object Sender, MessageEventArgs e)
		{
			string LegalId = XML.Attribute(e.Content, "id");
			string PetitionId = XML.Attribute(e.Content, "pid");
			string Purpose = XML.Attribute(e.Content, "purpose");
			string From = XML.Attribute(e.Content, "from");
			string ClientEndpoint = XML.Attribute(e.Content, "clientEp");

			if (!TryGetContext(e.Content, out XmlElement Context, out string _,
				out string[] Properties, out string[] Attachments, out LegalIdentity Identity))
			{
				this.client.Error("Invalid context. Ignoring message.");
				return;
			}

			if (Identity is null)
			{
				this.client.Error("No identity in message. Ignoring message.");
				return;
			}

			if (string.Compare(e.FromBareJID, this.componentAddress, true) == 0)
			{
				await this.Validate(Identity, false, async (sender2, e2) =>
				{
					if (e2.Status != IdentityStatus.Valid)
					{
						this.client.Error("Invalid legal identity received and discarded.");

						Log.Warning("Invalid legal identity received and discarded.", this.client.BareJID, e.From,
							new KeyValuePair<string, object>("Status", e2.Status));
						return;
					}

					await this.PetitionForIdentityReceived.Raise(this, new LegalIdentityPetitionEventArgs(e,
						Identity, From, LegalId, PetitionId, Purpose, ClientEndpoint, Context, Properties, Attachments));
				}, null);
			}
		}

		private static bool TryGetContext(XmlElement Query, out XmlElement Context,
			out string Content, out string[] Properties, out string[] Attachments,
			out LegalIdentity Identity)
		{
			ChunkedList<string> PropertyList = null;
			ChunkedList<string> AttachmentList = null;
			Context = null;
			Properties = null;
			Attachments = null;
			Content = null;
			Identity = null;

			foreach (XmlNode N in Query)
			{
				if (!(N is XmlElement E))
					continue;

				if (E.NamespaceURI == Query.NamespaceURI)
				{
					switch (E.LocalName)
					{
						case "identity":
							Identity = LegalIdentity.Parse(E);
							continue;

						case "content":
							if (string.IsNullOrEmpty(Content))
							{
								Content = E.InnerText;
								continue;
							}
							else
								return false;

						case "properties":
							foreach (XmlNode N2 in E.ChildNodes)
							{
								if (!(N2 is XmlElement E2))
									continue;

								if (E2.LocalName == "property")
								{
									PropertyList ??= new ChunkedList<string>();
									PropertyList.Add(E2.InnerText);
								}
								else
									return false;
							}
							continue;

						case "attachments":
							foreach (XmlNode N2 in E.ChildNodes)
							{
								if (!(N2 is XmlElement E2))
									continue;

								if (E2.LocalName == "attachment")
								{
									AttachmentList ??= new ChunkedList<string>();
									AttachmentList.Add(E2.InnerText);
								}
								else
									return false;
							}
							continue;
					}
				}

				if (Context is null)
					Context = E;
				else
					return false;
				break;
			}

			Properties = PropertyList?.ToArray();
			Attachments = AttachmentList?.ToArray();

			return true;
		}

		/// <summary>
		/// Event raised when someone requests access to one of the legal identities owned by the client.
		/// </summary>
		public event EventHandlerAsync<LegalIdentityPetitionEventArgs> PetitionForIdentityReceived = null;

		private async Task PetitionIdentityResponseMessageHandler(object Sender, MessageEventArgs e)
		{
			string PetitionId = XML.Attribute(e.Content, "pid");
			bool Response = XML.Attribute(e.Content, "response", false);
			string ClientEndpoint = XML.Attribute(e.Content, "clientEp");
			LegalIdentity Identity = null;
			XmlElement Context = null;

			foreach (XmlNode N in e.Content.ChildNodes)
			{
				if (N is XmlElement E)
				{
					if (E.LocalName == "identity" && E.NamespaceURI == e.Content.NamespaceURI)
						Identity = LegalIdentity.Parse(E);
					else if (!(Context is null))
						return;
					else
						Context = E;
				}
			}

			if (!Response || string.Compare(e.FromBareJID, Identity?.Provider ?? string.Empty, true) == 0)
				await this.PetitionedIdentityResponseReceived.Raise(this, new LegalIdentityPetitionResponseEventArgs(e, Identity, PetitionId, Response, ClientEndpoint, Context));
		}

		/// <summary>
		/// Event raised when a response to an identity petition has been received by the client.
		/// </summary>
		public event EventHandlerAsync<LegalIdentityPetitionResponseEventArgs> PetitionedIdentityResponseReceived = null;

		#endregion

		#region Signature petitions

		/// <summary>
		/// Sends a petition to a third party to request a digital signature of some content. The petition is not
		/// guaranteed to return a response. Response is returned if the recipient accepts the petition.
		/// When a petition is received, the <see cref="PetitionForSignatureReceived"/> event is raised.
		/// When a response to a petition is received, the <see cref="PetitionedSignatureResponseReceived"/> event is raised.
		/// </summary>
		/// <param name="LegalId">Legal Identity to petition.</param>
		/// <param name="Content">Content to be signed.</param>
		/// <param name="PetitionId">A petition identifier. This identifier will follow the petition, and can be used
		/// to identify the petition request.</param>
		/// <param name="Purpose">Purpose string to show to the owner.</param>
		public Task PetitionSignatureAsync(string LegalId, byte[] Content, string PetitionId, string Purpose)
		{
			return this.PetitionSignatureAsync(this.GetTrustProvider(LegalId), LegalId, Content, PetitionId, Purpose, false, null, null, null);
		}

		/// <summary>
		/// Sends a petition to a third party to request a digital signature of some content. The petition is not
		/// guaranteed to return a response. Response is returned if the recipient accepts the petition.
		/// When a petition is received, the <see cref="PetitionForSignatureReceived"/> event is raised.
		/// When a response to a petition is received, the <see cref="PetitionedSignatureResponseReceived"/> event is raised.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="LegalId">Legal Identity to petition.</param>
		/// <param name="Content">Content to be signed.</param>
		/// <param name="PetitionId">A petition identifier. This identifier will follow the petition, and can be used
		/// to identify the petition request.</param>
		/// <param name="Purpose">Purpose string to show to the owner.</param>
		public Task PetitionSignatureAsync(string Address, string LegalId, byte[] Content, string PetitionId, string Purpose)
		{
			return this.PetitionSignatureAsync(Address, LegalId, Content, PetitionId, Purpose, false, null, null, null);
		}

		/// <summary>
		/// Sends a petition to a third party to request a digital signature of some content. The petition is not
		/// guaranteed to return a response. Response is returned if the recipient accepts the petition.
		/// When a petition is received, the <see cref="PetitionForSignatureReceived"/> event is raised.
		/// When a response to a petition is received, the <see cref="PetitionedSignatureResponseReceived"/> event is raised.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="LegalId">Legal Identity to petition.</param>
		/// <param name="Content">Content to be signed.</param>
		/// <param name="PetitionId">A petition identifier. This identifier will follow the petition, and can be used
		/// to identify the petition request.</param>
		/// <param name="Purpose">Purpose string to show to the owner.</param>
		/// <param name="ContextXml">Any machine-readable context XML element you want to include in the petition.</param>
		public Task PetitionSignatureAsync(string Address, string LegalId, byte[] Content, string PetitionId, string Purpose, string ContextXml)
		{
			return this.PetitionSignatureAsync(Address, LegalId, Content, PetitionId, Purpose, false, ContextXml, null, null);
		}

		/// <summary>
		/// Sends a petition to a third party to request a digital signature of some content. The petition is not
		/// guaranteed to return a response. Response is returned if the recipient accepts the petition.
		/// When a petition is received, the <see cref="PetitionForSignatureReceived"/> event is raised.
		/// When a response to a petition is received, the <see cref="PetitionedSignatureResponseReceived"/> event is raised.
		/// </summary>
		/// <param name="LegalId">Legal Identity to petition.</param>
		/// <param name="Content">Content to be signed.</param>
		/// <param name="PetitionId">A petition identifier. This identifier will follow the petition, and can be used
		/// to identify the petition request.</param>
		/// <param name="Purpose">Purpose string to show to the owner.</param>
		/// <param name="Properties">Optional property hints to provide to the remote party,
		/// highlighting which properties will be used in the response.</param>
		/// <param name="Attachments">Optional attachment hints to provide to the remote party,
		/// highlighting which attachments will be used in the response.</param>
		public Task PetitionSignatureAsync(string LegalId, byte[] Content, string PetitionId, string Purpose,
			string[] Properties, string[] Attachments)
		{
			return this.PetitionSignatureAsync(this.GetTrustProvider(LegalId), LegalId, Content, PetitionId, Purpose, false, null,
				Properties, Attachments);
		}

		/// <summary>
		/// Sends a petition to a third party to request a digital signature of some content. The petition is not
		/// guaranteed to return a response. Response is returned if the recipient accepts the petition.
		/// When a petition is received, the <see cref="PetitionForSignatureReceived"/> event is raised.
		/// When a response to a petition is received, the <see cref="PetitionedSignatureResponseReceived"/> event is raised.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="LegalId">Legal Identity to petition.</param>
		/// <param name="Content">Content to be signed.</param>
		/// <param name="PetitionId">A petition identifier. This identifier will follow the petition, and can be used
		/// to identify the petition request.</param>
		/// <param name="Purpose">Purpose string to show to the owner.</param>
		/// <param name="Properties">Optional property hints to provide to the remote party,
		/// highlighting which properties will be used in the response.</param>
		/// <param name="Attachments">Optional attachment hints to provide to the remote party,
		/// highlighting which attachments will be used in the response.</param>
		public Task PetitionSignatureAsync(string Address, string LegalId, byte[] Content, string PetitionId, string Purpose,
			string[] Properties, string[] Attachments)
		{
			return this.PetitionSignatureAsync(Address, LegalId, Content, PetitionId, Purpose, false, null,
				Properties, Attachments);
		}

		/// <summary>
		/// Sends a petition to a third party to request a digital signature of some content. The petition is not
		/// guaranteed to return a response. Response is returned if the recipient accepts the petition.
		/// When a petition is received, the <see cref="PetitionForSignatureReceived"/> event is raised.
		/// When a response to a petition is received, the <see cref="PetitionedSignatureResponseReceived"/> event is raised.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="LegalId">Legal Identity to petition.</param>
		/// <param name="Content">Content to be signed.</param>
		/// <param name="PetitionId">A petition identifier. This identifier will follow the petition, and can be used
		/// to identify the petition request.</param>
		/// <param name="Purpose">Purpose string to show to the owner.</param>
		/// <param name="ContextXml">Any machine-readable context XML element you want to include in the petition.</param>
		/// <param name="Properties">Optional property hints to provide to the remote party,
		/// highlighting which properties will be used in the response.</param>
		/// <param name="Attachments">Optional attachment hints to provide to the remote party,
		/// highlighting which attachments will be used in the response.</param>
		public Task PetitionSignatureAsync(string Address, string LegalId, byte[] Content, string PetitionId, string Purpose, string ContextXml,
			string[] Properties, string[] Attachments)
		{
			return this.PetitionSignatureAsync(Address, LegalId, Content, PetitionId, Purpose, false, ContextXml,
				Properties, Attachments);
		}

		private async Task PetitionSignatureAsync(string Address, string LegalId, byte[] Content, string PetitionId,
			string Purpose, bool PeerReview, string ContextXml, string[] Properties, string[] Attachments)
		{
			if (this.contentPerPid.TryGetValue(PetitionId, out KeyValuePair<byte[], bool> Rec))
			{
				if (Convert.ToBase64String(Content) == Convert.ToBase64String(Rec.Key) && PeerReview == Rec.Value)
					return;

				throw new InvalidOperationException("Petition ID must be unique for outstanding petitions.");
			}

			this.contentPerPid[PetitionId] = new KeyValuePair<byte[], bool>(Content, PeerReview);

			StringBuilder Xml = new StringBuilder();
			byte[] Nonce = this.RandomBytes(32);

			string NonceStr = Convert.ToBase64String(Nonce);
			string ContentStr = Convert.ToBase64String(Content);
			byte[] Data = Encoding.UTF8.GetBytes(PetitionId + ":" + LegalId + ":" + Purpose + ":" + NonceStr + ":" + this.client.BareJID.ToLower() + ":" + ContentStr);
			byte[] Signature = await this.SignAsync(Data, PeerReview ? SignWith.CurrentKeys : SignWith.LatestApprovedId);

			Xml.Append("<petitionSignature xmlns='");
			Xml.Append(NamespaceLegalIdentitiesCurrent);
			Xml.Append("' id='");
			Xml.Append(XML.Encode(LegalId));
			Xml.Append("' pid='");
			Xml.Append(XML.Encode(PetitionId));
			Xml.Append("' purpose='");
			Xml.Append(XML.Encode(Purpose));
			Xml.Append("' nonce='");
			Xml.Append(NonceStr);
			Xml.Append("' s='");
			Xml.Append(Convert.ToBase64String(Signature));
			Xml.Append("'>");
			AppendHints(Xml, Properties, Attachments);

			if (!string.IsNullOrEmpty(ContentStr))
			{
				Xml.Append("<content>");
				Xml.Append(ContentStr);
				Xml.Append("</content>");
			}

			if (!string.IsNullOrEmpty(ContextXml))
				Xml.Append(ContextXml);

			Xml.Append("</petitionSignature>");

			await this.client.IqSetAsync(Address, Xml.ToString());
		}

		/// <summary>
		/// Sends a response to a petition for a signature by the client.
		/// When a petition is received, the <see cref="PetitionForSignatureReceived"/> event is raised.
		/// When a response to a petition is received, the <see cref="PetitionedSignatureResponseReceived"/> event is raised.
		/// </summary>
		/// <param name="LegalId">Legal Identity petitioned.</param>
		/// <param name="Content">Content to be signed.</param>
		/// <param name="Signature">Digital signature of content, made by the legal identity.</param>
		/// <param name="PetitionId">A petition identifier. This identifier will follow the petition, and can be used
		/// to identify the petition request.</param>
		/// <param name="RequestorFullJid">Full JID of requestor.</param>
		/// <param name="Response">If the petition is accepted (true) or rejected (false).</param>
		public Task PetitionSignatureResponseAsync(string LegalId, byte[] Content,
			byte[] Signature, string PetitionId, string RequestorFullJid, bool Response)
		{
			return this.PetitionSignatureResponseAsync(this.GetTrustProvider(LegalId), LegalId, Content, Signature, PetitionId,
				RequestorFullJid, Response, null);
		}

		/// <summary>
		/// Sends a response to a petition for a signature by the client.
		/// When a petition is received, the <see cref="PetitionForSignatureReceived"/> event is raised.
		/// When a response to a petition is received, the <see cref="PetitionedSignatureResponseReceived"/> event is raised.
		/// </summary>
		/// <param name="LegalId">Legal Identity petitioned.</param>
		/// <param name="Content">Content to be signed.</param>
		/// <param name="Signature">Digital signature of content, made by the legal identity.</param>
		/// <param name="PetitionId">A petition identifier. This identifier will follow the petition, and can be used
		/// to identify the petition request.</param>
		/// <param name="RequestorFullJid">Full JID of requestor.</param>
		/// <param name="Response">If the petition is accepted (true) or rejected (false).</param>
		/// <param name="ContextXml">Any machine-readable context XML element you want to include in the petition response.</param>
		public Task PetitionSignatureResponseAsync(string LegalId, byte[] Content,
			byte[] Signature, string PetitionId, string RequestorFullJid, bool Response, string ContextXml)
		{
			return this.PetitionSignatureResponseAsync(this.GetTrustProvider(LegalId), LegalId, Content, Signature, PetitionId,
				RequestorFullJid, Response, ContextXml);
		}

		/// <summary>
		/// Sends a response to a petition for a signature by the client.
		/// When a petition is received, the <see cref="PetitionForSignatureReceived"/> event is raised.
		/// When a response to a petition is received, the <see cref="PetitionedSignatureResponseReceived"/> event is raised.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="LegalId">Legal Identity petitioned.</param>
		/// <param name="Content">Content to be signed.</param>
		/// <param name="Signature">Digital signature of content, made by the legal identity.</param>
		/// <param name="PetitionId">A petition identifier. This identifier will follow the petition, and can be used
		/// to identify the petition request.</param>
		/// <param name="RequestorFullJid">Full JID of requestor.</param>
		/// <param name="Response">If the petition is accepted (true) or rejected (false).</param>
		public Task PetitionSignatureResponseAsync(string Address, string LegalId, byte[] Content, byte[] Signature,
			string PetitionId, string RequestorFullJid, bool Response)
		{
			return this.PetitionSignatureResponseAsync(Address, LegalId, Content, Signature, PetitionId, RequestorFullJid, Response, null);
		}

		/// <summary>
		/// Sends a response to a petition for a signature by the client.
		/// When a petition is received, the <see cref="PetitionForSignatureReceived"/> event is raised.
		/// When a response to a petition is received, the <see cref="PetitionedSignatureResponseReceived"/> event is raised.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="LegalId">Legal Identity petitioned.</param>
		/// <param name="Content">Content to be signed.</param>
		/// <param name="Signature">Digital signature of content, made by the legal identity.</param>
		/// <param name="PetitionId">A petition identifier. This identifier will follow the petition, and can be used
		/// to identify the petition request.</param>
		/// <param name="RequestorFullJid">Full JID of requestor.</param>
		/// <param name="Response">If the petition is accepted (true) or rejected (false).</param>
		/// <param name="ContextXml">Any machine-readable context XML element you want to include in the petition response.</param>
		public async Task PetitionSignatureResponseAsync(string Address, string LegalId, byte[] Content, byte[] Signature,
			string PetitionId, string RequestorFullJid, bool Response, string ContextXml)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<petitionSignatureResponse xmlns='");
			Xml.Append(NamespaceLegalIdentitiesCurrent);
			Xml.Append("' id='");
			Xml.Append(XML.Encode(LegalId));
			Xml.Append("' pid='");
			Xml.Append(XML.Encode(PetitionId));
			Xml.Append("' jid='");
			Xml.Append(XML.Encode(RequestorFullJid));
			Xml.Append("' response='");
			Xml.Append(CommonTypes.Encode(Response));
			Xml.Append("'><content>");
			Xml.Append(Convert.ToBase64String(Content));
			Xml.Append("</content><signature>");
			Xml.Append(Convert.ToBase64String(Signature));
			Xml.Append("</signature>");

			if (!string.IsNullOrEmpty(ContextXml))
				Xml.Append(ContextXml);

			Xml.Append("</petitionSignatureResponse>");

			await this.client.IqSetAsync(Address, Xml.ToString());
		}

		private async Task PetitionSignatureMessageHandler(object Sender, MessageEventArgs e)
		{
			string LegalId = XML.Attribute(e.Content, "id");
			string PetitionId = XML.Attribute(e.Content, "pid");
			string Purpose = XML.Attribute(e.Content, "purpose");
			string From = XML.Attribute(e.Content, "from");
			string ClientEndpoint = XML.Attribute(e.Content, "clientEp");
			byte[] Content;
			bool PeerReview = false;

			if (!TryGetContext(e.Content, out XmlElement Context, out string ContentStr,
				out string[] Properties, out string[] Attachments, out LegalIdentity Identity))
			{
				this.client.Error("Invalid context. Ignoring message.");
				return;
			}

			if (string.IsNullOrEmpty(ContentStr))
			{
				this.client.Error("No content in message to sign. Ignoring message.");
				return;
			}

			try
			{
				Content = Convert.FromBase64String(ContentStr);
			}
			catch (Exception)
			{
				this.client.Error("Invalid BASE64-encoded content in message to sign. Ignoring message.");
				return;
			}

			if (Identity is null)
			{
				string s = Encoding.UTF8.GetString(Content);
				if (s.StartsWith("<identity") && s.EndsWith("</identity>"))
				{
					try
					{
						XmlDocument Doc = new XmlDocument();
						Doc.LoadXml(s);

						if (Doc.DocumentElement.LocalName == "identity")
						{
							LegalIdentity TempId = LegalIdentity.Parse(Doc.DocumentElement);

							if (TempId.State == IdentityState.Created &&
								string.Compare(TempId["JID"], XmppClient.GetBareJID(From), true) == 0)
							{
								Identity = TempId;
								PeerReview = true;
							}
						}
					}
					catch (Exception)
					{
						// Ignore
					}
				}

				if (Identity is null)
					return;
			}

			if (string.Compare(e.FromBareJID, this.componentAddress, true) != 0 &&
				string.Compare(e.FromBareJID, XmppClient.GetDomain(Identity.Id), true) != 0)
			{
				return;
			}

			EventHandlerAsync<SignaturePetitionEventArgs> h = PeerReview ? this.PetitionForPeerReviewIDReceived : this.PetitionForSignatureReceived;

			await this.Validate(Identity, false, async (sender2, e2) =>
			{
				if (e2.Status != IdentityStatus.Valid && e2.Status != IdentityStatus.NoProviderSignature)
				{
					this.client.Error("Invalid legal identity received and discarded.");

					Log.Warning("Invalid legal identity received and discarded.", this.client.BareJID, e.From,
						new KeyValuePair<string, object>("Status", e2.Status));

					return;
				}

				await h.Raise(this, new SignaturePetitionEventArgs(e, Identity, From, LegalId,
					PetitionId, Purpose, Content, ClientEndpoint, Context, Properties, Attachments));

			}, null);
		}

		/// <summary>
		/// Event raised when someone requests access to one of the legal identities owned by the client.
		/// </summary>
		public event EventHandlerAsync<SignaturePetitionEventArgs> PetitionForSignatureReceived = null;

		private async Task PetitionSignatureResponseMessageHandler(object Sender, MessageEventArgs e)
		{
			string PetitionId = XML.Attribute(e.Content, "pid");
			bool Response = XML.Attribute(e.Content, "response", false);
			string ClientEndpoint = XML.Attribute(e.Content, "clientEp");
			string SignatureStr = string.Empty;
			byte[] Signature = null;
			LegalIdentity Identity = null;
			XmlElement Context = null;

			foreach (XmlNode N in e.Content.ChildNodes)
			{
				if (N is XmlElement E)
				{
					switch (E.LocalName)
					{
						case "identity":
							Identity = LegalIdentity.Parse(E);
							break;

						case "signature":
							SignatureStr = E.InnerText;
							Signature = Convert.FromBase64String(SignatureStr);
							break;

						default:
							if (!(Context is null))
								return;

							Context = E;
							break;
					}
				}
			}

			if (!this.contentPerPid.TryGetValue(PetitionId, out KeyValuePair<byte[], bool> P))
			{
				this.client.Warning("Petition ID not recognized: " + PetitionId + ".  Response ignored.");
				return;
			}

			EventHandlerAsync<SignaturePetitionResponseEventArgs> h = P.Value ? this.PetitionedPeerReviewIDResponseReceived : this.PetitionedSignatureResponseReceived;

			if (Response)
			{
				if (Identity is null)
				{
					this.client.Warning("Identity missing. Response ignored.");
					return;
				}

				if (Signature is null)
				{
					this.client.Warning("Signature missing. Response ignored.");
					return;
				}

				bool? Result = this.ValidateSignature(Identity, P.Key, Signature);
				if (!Result.HasValue)
				{
					this.client.Warning("Unable to validate signature. Response ignored.");
					return;
				}

				if (!Result.Value)
				{
					this.client.Warning("Invalid signature. Response ignored.");
					return;
				}
			}

			if (!Response || string.Compare(e.FromBareJID, Identity?.Provider ?? string.Empty, true) == 0)
			{
				try
				{
					this.Client.Information(h.Method.Name);

					await h.Raise(this, new SignaturePetitionResponseEventArgs(e, Identity, PetitionId, Signature, Response, ClientEndpoint, Context));
				}
				finally
				{
					this.contentPerPid.Remove(PetitionId);
				}
			}
			else
				this.client.Warning("Sender invalid. Response ignored.");
		}

		/// <summary>
		/// Event raised when a response to a signature petition has been received by the client.
		/// </summary>
		public event EventHandlerAsync<SignaturePetitionResponseEventArgs> PetitionedSignatureResponseReceived = null;

		#endregion

		#region Peer Review of IDs

		/// <summary>
		/// Sends a petition to a third party to peer review a new legal identity. The petition is not
		/// guaranteed to return a response. Response is returned if the recipient accepts the petition.
		/// When a petition is received, the <see cref="PetitionForPeerReviewIDReceived"/> event is raised.
		/// When a response to a petition is received, the <see cref="PetitionedPeerReviewIDResponseReceived"/> event is raised.
		/// 
		/// Note: This is a special case of requesting a signature from a third party. In the normal
		/// case, both parties have valid legal identities. In this case, the requestor does not need a
		/// valid legal identity. However, the legal identity must be in an applied state, and the request
		/// properly signed. The identity also needs a JID meta-data tag equal to the bare JID of the
		/// client making the petition.
		/// </summary>
		/// <param name="LegalId">Legal Identity to petition.</param>
		/// <param name="Identity">Legal Identity to peer review.</param>
		/// <param name="PetitionId">A petition identifier. This identifier will follow the petition, and can be used
		/// to identify the petition request.</param>
		/// <param name="Purpose">Purpose string to show to the owner.</param>
		public Task PetitionPeerReviewIDAsync(string LegalId, LegalIdentity Identity, string PetitionId, string Purpose)
		{
			return this.PetitionPeerReviewIDAsync(this.GetTrustProvider(LegalId), LegalId, Identity, PetitionId, Purpose);
		}

		/// <summary>
		/// Sends a petition to a third party to peer review a new legal identity. The petition is not
		/// guaranteed to return a response. Response is returned if the recipient accepts the petition.
		/// When a petition is received, the <see cref="PetitionForPeerReviewIDReceived"/> event is raised.
		/// When a response to a petition is received, the <see cref="PetitionedPeerReviewIDResponseReceived"/> event is raised.
		/// 
		/// Note: This is a special case of requesting a signature from a third party. In the normal
		/// case, both parties have valid legal identities. In this case, the requestor does not need a
		/// valid legal identity. However, the legal identity must be in an applied state, and the request
		/// properly signed. The identity also needs a JID meta-data tag equal to the bare JID of the
		/// client making the petition.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="LegalId">Legal Identity to petition.</param>
		/// <param name="Identity">Legal Identity to peer review.</param>
		/// <param name="PetitionId">A petition identifier. This identifier will follow the petition, and can be used
		/// to identify the petition request.</param>
		/// <param name="Purpose">Purpose string to show to the owner.</param>
		public Task PetitionPeerReviewIDAsync(string Address, string LegalId, LegalIdentity Identity, string PetitionId, string Purpose)
		{
			StringBuilder Xml = new StringBuilder();
			Identity.Serialize(Xml, true, true, true, true, true, true, true);
			byte[] Content = Encoding.UTF8.GetBytes(Xml.ToString());

			return this.PetitionSignatureAsync(Address, LegalId, Content, PetitionId, Purpose, true, null, null, null);
		}

		/// <summary>
		/// Event raised when someone makes a request to one of the legal identities owned by the client, for a peer review of a newly created legal identity.
		/// </summary>
		public event EventHandlerAsync<SignaturePetitionEventArgs> PetitionForPeerReviewIDReceived = null;

		/// <summary>
		/// Event raised when a response to a ID Peer Review petition has been received by the client.
		/// </summary>
		public event EventHandlerAsync<SignaturePetitionResponseEventArgs> PetitionedPeerReviewIDResponseReceived = null;

		/// <summary>
		/// Adds an attachment to a legal identity with information about a peer review of the identity.
		/// </summary>
		/// <param name="Identity">Legal Identity being reviewed.</param>
		/// <param name="ReviewerLegalIdentity">Identity of reviewer.</param>
		/// <param name="PeerSignature">Signature made by reviewer.</param>
		/// <returns>Updated identity.</returns>
		public async Task<LegalIdentity> AddPeerReviewIDAttachment(LegalIdentity Identity,
			LegalIdentity ReviewerLegalIdentity, byte[] PeerSignature)
		{
			if (!this.client.TryGetExtension(out HttpFileUploadClient HttpFileUploadClient))
				throw new InvalidOperationException("No HTTP File Upload extension added to the XMPP Client.");

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<peerReview s='");
			Xml.Append(Convert.ToBase64String(PeerSignature));
			Xml.Append("' tp='");
			Xml.Append(XML.Encode(DateTime.UtcNow));
			Xml.Append("' xmlns='");
			Xml.Append(NamespaceLegalIdentitiesCurrent);
			Xml.Append("'><reviewed>");
			Identity.Serialize(Xml, true, true, true, true, true, true, true);
			Xml.Append("</reviewed><reviewer>");
			ReviewerLegalIdentity.Serialize(Xml, true, true, true, true, true, true, true);
			Xml.Append("</reviewer></peerReview>");

			byte[] Data = Encoding.UTF8.GetBytes(Xml.ToString());
			byte[] Signature = await this.SignAsync(Data, SignWith.CurrentKeys);
			string FileName = ReviewerLegalIdentity.Id + ".xml";
			string ContentType = "text/xml; charset=utf-8";

			HttpFileUploadEventArgs e2 = await HttpFileUploadClient.RequestUploadSlotAsync(FileName, ContentType, Data.Length);
			if (!e2.Ok)
				throw new IOException("Unable to upload Peer Review attachment to broker.");

			await e2.PUT(Data, ContentType, 10000);

			return await this.AddLegalIdAttachmentAsync(Identity.Id, e2.GetUrl, Signature);
		}

		#endregion

		#region Contract petitions

		/// <summary>
		/// Sends a petition to the parts of a smart contract, to access the information in the contract. The petition is not
		/// guaranteed to return a response. Response is returned if one of the parts accepts the petition.
		/// When a petition for a contract is received, the <see cref="PetitionForContractReceived"/> event is raised.
		/// When a response to a petition is received, the <see cref="PetitionedContractResponseReceived"/> event is raised.
		/// </summary>
		/// <param name="ContractId">Smart Contract to petition.</param>
		/// <param name="PetitionId">A petition identifier. This identifier will follow the petition, and can be used
		/// to identify the petition request.</param>
		/// <param name="Purpose">Purpose string to show to the owner.</param>
		public Task PetitionContractAsync(string ContractId, string PetitionId, string Purpose)
		{
			return this.PetitionContractAsync(this.GetTrustProvider(ContractId), ContractId, PetitionId, Purpose, null);
		}

		/// <summary>
		/// Sends a petition to the parts of a smart contract, to access the information in the contract. The petition is not
		/// guaranteed to return a response. Response is returned if one of the parts accepts the petition.
		/// When a petition for a contract is received, the <see cref="PetitionForContractReceived"/> event is raised.
		/// When a response to a petition is received, the <see cref="PetitionedContractResponseReceived"/> event is raised.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ContractId">Smart Contract to petition.</param>
		/// <param name="PetitionId">A petition identifier. This identifier will follow the petition, and can be used
		/// to identify the petition request.</param>
		/// <param name="Purpose">Purpose string to show to the owner.</param>
		public Task PetitionContractAsync(string Address, string ContractId, string PetitionId, string Purpose)
		{
			return this.PetitionContractAsync(Address, ContractId, PetitionId, Purpose, null);
		}

		/// <summary>
		/// Sends a petition to the parts of a smart contract, to access the information in the contract. The petition is not
		/// guaranteed to return a response. Response is returned if one of the parts accepts the petition.
		/// When a petition for a contract is received, the <see cref="PetitionForContractReceived"/> event is raised.
		/// When a response to a petition is received, the <see cref="PetitionedContractResponseReceived"/> event is raised.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ContractId">Smart Contract to petition.</param>
		/// <param name="PetitionId">A petition identifier. This identifier will follow the petition, and can be used
		/// to identify the petition request.</param>
		/// <param name="Purpose">Purpose string to show to the owner.</param>
		/// <param name="ContextXml">Any machine-readable context XML element you want to include in the petition.</param>
		public async Task PetitionContractAsync(string Address, string ContractId, string PetitionId, string Purpose, string ContextXml)
		{
			StringBuilder Xml = new StringBuilder();
			byte[] Nonce = this.RandomBytes(32);

			string NonceStr = Convert.ToBase64String(Nonce);
			byte[] Data = Encoding.UTF8.GetBytes(PetitionId + ":" + ContractId + ":" + Purpose + ":" + NonceStr + ":" + this.client.BareJID.ToLower());
			byte[] Signature = await this.SignAsync(Data, SignWith.LatestApprovedId);

			Xml.Append("<petitionContract xmlns='");
			Xml.Append(NamespaceSmartContractsCurrent);
			Xml.Append("' id='");
			Xml.Append(XML.Encode(ContractId));
			Xml.Append("' pid='");
			Xml.Append(XML.Encode(PetitionId));
			Xml.Append("' purpose='");
			Xml.Append(XML.Encode(Purpose));
			Xml.Append("' nonce='");
			Xml.Append(NonceStr);
			Xml.Append("' s='");
			Xml.Append(Convert.ToBase64String(Signature));

			if (string.IsNullOrEmpty(ContextXml))
				Xml.Append("'/>");
			else
			{
				Xml.Append("'>");
				Xml.Append(ContextXml);
				Xml.Append("</petitionContract>");
			}

			await this.client.IqSetAsync(Address, Xml.ToString());
		}

		/// <summary>
		/// Sends a response to a petition to access a smart contract.
		/// When a petition for a contract is received, the <see cref="PetitionForContractReceived"/> event is raised.
		/// When a response to a petition is received, the <see cref="PetitionedContractResponseReceived"/> event is raised.
		/// </summary>
		/// <param name="ContractId">Smart Contract to petition.</param>
		/// <param name="PetitionId">A petition identifier. This identifier will follow the petition, and can be used
		/// to identify the petition request.</param>
		/// <param name="RequestorFullJid">Full JID of requestor.</param>
		/// <param name="Response">If the petition is accepted (true) or rejected (false).</param>
		public Task PetitionContractResponseAsync(string ContractId, string PetitionId, string RequestorFullJid, bool Response)
		{
			return this.PetitionContractResponseAsync(this.GetTrustProvider(ContractId), ContractId, PetitionId, RequestorFullJid, Response, null);
		}

		/// <summary>
		/// Sends a response to a petition to access a smart contract.
		/// When a petition for a contract is received, the <see cref="PetitionForContractReceived"/> event is raised.
		/// When a response to a petition is received, the <see cref="PetitionedContractResponseReceived"/> event is raised.
		/// </summary>
		/// <param name="ContractId">Smart Contract to petition.</param>
		/// <param name="PetitionId">A petition identifier. This identifier will follow the petition, and can be used
		/// to identify the petition request.</param>
		/// <param name="RequestorFullJid">Full JID of requestor.</param>
		/// <param name="Response">If the petition is accepted (true) or rejected (false).</param>
		/// <param name="ContextXml">Any machine-readable context XML element you want to include in the petition response.</param>
		public Task PetitionContractResponseAsync(string ContractId, string PetitionId, string RequestorFullJid, bool Response, string ContextXml)
		{
			return this.PetitionContractResponseAsync(this.GetTrustProvider(ContractId), ContractId, PetitionId, RequestorFullJid, Response, ContextXml);
		}

		/// <summary>
		/// Sends a response to a petition to access a smart contract.
		/// When a petition for a contract is received, the <see cref="PetitionForContractReceived"/> event is raised.
		/// When a response to a petition is received, the <see cref="PetitionedContractResponseReceived"/> event is raised.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ContractId">Smart Contract to petition.</param>
		/// <param name="PetitionId">A petition identifier. This identifier will follow the petition, and can be used
		/// to identify the petition request.</param>
		/// <param name="RequestorFullJid">Full JID of requestor.</param>
		/// <param name="Response">If the petition is accepted (true) or rejected (false).</param>
		public Task PetitionContractResponseAsync(string Address, string ContractId, string PetitionId, string RequestorFullJid, bool Response)
		{
			return this.PetitionContractResponseAsync(Address, ContractId, PetitionId, RequestorFullJid, Response, null);
		}

		/// <summary>
		/// Sends a response to a petition to access a smart contract.
		/// When a petition for a contract is received, the <see cref="PetitionForContractReceived"/> event is raised.
		/// When a response to a petition is received, the <see cref="PetitionedContractResponseReceived"/> event is raised.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ContractId">Smart Contract to petition.</param>
		/// <param name="PetitionId">A petition identifier. This identifier will follow the petition, and can be used
		/// to identify the petition request.</param>
		/// <param name="RequestorFullJid">Full JID of requestor.</param>
		/// <param name="Response">If the petition is accepted (true) or rejected (false).</param>
		/// <param name="ContextXml">Any machine-readable context XML element you want to include in the petition response.</param>
		public async Task PetitionContractResponseAsync(string Address, string ContractId, string PetitionId, string RequestorFullJid,
			bool Response, string ContextXml)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<petitionContractResponse xmlns='");
			Xml.Append(NamespaceSmartContractsCurrent);
			Xml.Append("' id='");
			Xml.Append(XML.Encode(ContractId));
			Xml.Append("' pid='");
			Xml.Append(XML.Encode(PetitionId));
			Xml.Append("' jid='");
			Xml.Append(XML.Encode(RequestorFullJid));
			Xml.Append("' response='");
			Xml.Append(CommonTypes.Encode(Response));

			if (string.IsNullOrEmpty(ContextXml))
				Xml.Append("'/>");
			else
			{
				Xml.Append("'>");
				Xml.Append(ContextXml);
				Xml.Append("</petitionContractResponse>");
			}

			await this.client.IqSetAsync(Address, Xml.ToString());
		}

		private async Task PetitionContractMessageHandler(object Sender, MessageEventArgs e)
		{
			string ContractId = XML.Attribute(e.Content, "id");
			string PetitionId = XML.Attribute(e.Content, "pid");
			string Purpose = XML.Attribute(e.Content, "purpose");
			string From = XML.Attribute(e.Content, "from");
			string ClientEndpoint = XML.Attribute(e.Content, "clientEp");
			int i = ContractId.IndexOf('@');

			if (!TryGetContext(e.Content, out XmlElement Context, out string ContentStr,
				out string[] Properties, out string[] Attachments, out LegalIdentity Identity))
			{
				this.client.Error("Invalid context. Ignoring message.");
				return;
			}

			if (Identity is null)
			{
				this.client.Error("No identity in message. Ignoring message.");
				return;
			}

			if (!this.IsFromTrustProvider(ContractId, e.FromBareJID))
			{
				this.client.Error("Contract not hosted on trust provider. Ignoring message.");
				return;
			}

			await this.Validate(Identity, false, async (sender2, e2) =>
			{
				if (e2.Status != IdentityStatus.Valid)
				{
					this.client.Error("Invalid identity received and discarded.");

					Log.Warning("Invalid identity received and discarded.", this.client.BareJID, e.From,
						new KeyValuePair<string, object>("Status", e2.Status));
					return;
				}

				await this.PetitionForContractReceived.Raise(this, new ContractPetitionEventArgs(e,
					Identity, From, ContractId, PetitionId, Purpose, ClientEndpoint, Context, Properties, Attachments));

			}, null);
		}

		/// <summary>
		/// Event raised when someone requests access to a smart contract to which the client is part.
		/// </summary>
		public event EventHandlerAsync<ContractPetitionEventArgs> PetitionForContractReceived = null;

		private async Task PetitionContractResponseMessageHandler(object Sender, MessageEventArgs e)
		{
			string PetitionId = XML.Attribute(e.Content, "pid");
			bool Response = XML.Attribute(e.Content, "response", false);
			string ClientEndpoint = XML.Attribute(e.Content, "clientEp");
			Contract Contract = null;
			XmlElement Context = null;

			foreach (XmlNode N in e.Content.ChildNodes)
			{
				if (!(N is XmlElement E))
					continue;

				if (E.LocalName == "contract" && E.NamespaceURI == e.Content.NamespaceURI)
				{
					ParsedContract Parsed = await Contract.Parse(E, this, false);
					Contract = Parsed?.Contract;
				}
				else if (!(Context is null))
					return;
				else
					Context = E;
			}

			if (!Response || string.Compare(e.FromBareJID, Contract?.Provider ?? string.Empty, true) == 0)
				await this.PetitionedContractResponseReceived.Raise(this, new ContractPetitionResponseEventArgs(e, Contract, PetitionId, Response, ClientEndpoint, Context));
		}

		/// <summary>
		/// Event raised when a response to a contract petition has been received by the client.
		/// </summary>
		public event EventHandlerAsync<ContractPetitionResponseEventArgs> PetitionedContractResponseReceived = null;

		#endregion

		#region Attachments

		/// <summary>
		/// Adds an attachment to a newly created legal identity.
		/// </summary>
		/// <param name="LegalId">ID of Legal Identity.</param>
		/// <param name="GetUrl">The GET URL of the attachment to associate with the legal identity.
		/// The URL might previously have been provided by the HTTP Upload Service.</param>
		/// <param name="Signature">Signature of the content of the attachment, made by the same private key used when
		/// creating the legal identity object.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task AddLegalIdAttachment(string LegalId, string GetUrl, byte[] Signature, EventHandlerAsync<LegalIdentityEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<addAttachment xmlns='");
			Xml.Append(NamespaceLegalIdentitiesCurrent);
			Xml.Append("' id='");
			Xml.Append(XML.Encode(LegalId));
			Xml.Append("' getUrl='");
			Xml.Append(XML.Encode(GetUrl));
			Xml.Append("' s='");
			Xml.Append(Convert.ToBase64String(Signature));
			Xml.Append("'/>");

			return this.client.SendIqSet(this.componentAddress, Xml.ToString(), async (Sender, e) =>
			{
				LegalIdentity Identity = null;
				XmlElement E;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "identity")
					Identity = LegalIdentity.Parse(E);
				else
					e.Ok = false;

				await Callback.Raise(this, new LegalIdentityEventArgs(e, Identity));
			}, State);
		}

		/// <summary>
		/// Adds an attachment to a newly created legal identity.
		/// </summary>
		/// <param name="LegalId">ID of Legal Identity.</param>
		/// <param name="GetUrl">The GET URL of the attachment to associate with the legal identity.
		/// The URL is previously provided by the HTTP Upload Service.</param>
		/// <param name="Signature">Signature of the content of the attachment, made by the same private key used when
		/// creating the legal identity object.</param>
		public async Task<LegalIdentity> AddLegalIdAttachmentAsync(string LegalId, string GetUrl, byte[] Signature)
		{
			TaskCompletionSource<LegalIdentity> Result = new TaskCompletionSource<LegalIdentity>();

			await this.AddLegalIdAttachment(LegalId, GetUrl, Signature, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Identity);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to add attachment."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		/// <summary>
		/// Adds an attachment to a proposed or approved contract before it is being signed.
		/// </summary>
		/// <param name="ContractId">ID of Smart Contract.</param>
		/// <param name="GetUrl">The GET URL of the attachment to associate with the smart contract.
		/// The URL might previously have been provided by the HTTP Upload Service.</param>
		/// <param name="Signature">Signature of the content of the attachment, made by an approved legal identity of the sender.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task AddContractAttachment(string ContractId, string GetUrl, byte[] Signature, EventHandlerAsync<SmartContractEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<addAttachment xmlns='");
			Xml.Append(NamespaceSmartContractsCurrent);
			Xml.Append("' contractId='");
			Xml.Append(XML.Encode(ContractId));
			Xml.Append("' getUrl='");
			Xml.Append(XML.Encode(GetUrl));
			Xml.Append("' s='");
			Xml.Append(Convert.ToBase64String(Signature));
			Xml.Append("'/>");

			return this.client.SendIqSet(this.componentAddress, Xml.ToString(), async (Sender, e) =>
			{
				Contract Contract = null;
				XmlElement E;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "contract")
				{
					ParsedContract Parsed = await Contract.Parse(E, this, false);
					if (Parsed is null)
						e.Ok = false;
					else
						Contract = Parsed.Contract;
				}
				else
					e.Ok = false;

				await Callback.Raise(this, new SmartContractEventArgs(e, Contract));
			}, State);
		}

		/// <summary>
		/// Adds an attachment to a proposed or approved contract before it is being signed.
		/// </summary>
		/// <param name="ContractId">ID of Smart Contract.</param>
		/// <param name="GetUrl">The GET URL of the attachment to associate with the smart contract.
		/// The URL might previously have been provided by the HTTP Upload Service.</param>
		/// <param name="Signature">Signature of the content of the attachment, made by an approved legal identity of the sender.</param>
		public async Task<Contract> AddContractAttachmentAsync(string ContractId, string GetUrl, byte[] Signature)
		{
			TaskCompletionSource<Contract> Result = new TaskCompletionSource<Contract>();

			await this.AddContractAttachment(ContractId, GetUrl, Signature, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Contract);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to add attachment."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		/// <summary>
		/// Gets an attachment from a Trust Provider
		/// </summary>
		/// <param name="Url">URL to attachment.</param>
		/// <param name="SignWith">What keys that can be used to sign the data.</param>
		/// <returns>Content-Type, and attachment.</returns>
		public Task<KeyValuePair<string, TemporaryFile>> GetAttachmentAsync(string Url, SignWith SignWith)
		{
			return this.GetAttachmentAsync(Url, SignWith, 30000);
		}

		/// <summary>
		/// Gets an attachment from a Trust Provider
		/// </summary>
		/// <param name="Url">URL to attachment.</param>
		/// <param name="SignWith">What keys that can be used to sign the data.</param>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <returns>Content-Type, and attachment.</returns>
		public async Task<KeyValuePair<string, TemporaryFile>> GetAttachmentAsync(string Url, SignWith SignWith, int Timeout)
		{
			using HttpClient HttpClient = new HttpClient()
			{
				Timeout = TimeSpan.FromMilliseconds(Timeout)
			};
			HttpRequestMessage Request;
			HttpResponseMessage Response = null;

			Request = new HttpRequestMessage()
			{
				RequestUri = new Uri(Url),
				Method = HttpMethod.Get
			};

			try
			{
				Response = await HttpClient.SendAsync(Request);

				if (Response.StatusCode == System.Net.HttpStatusCode.Unauthorized &&
					!(Response.Headers.WwwAuthenticate is null))
				{
					foreach (AuthenticationHeaderValue Header in Response.Headers.WwwAuthenticate)
					{
						if (Header.Scheme == "NeuroFoundation.Sign")
						{
							KeyValuePair<string, string>[] Parameters = CommonTypes.ParseFieldValues(Header.Parameter);
							string Realm = null;
							string NonceStr = null;
							byte[] Nonce = null;

							foreach (KeyValuePair<string, string> P in Parameters)
							{
								switch (P.Key)
								{
									case "realm":
										Realm = P.Value;
										break;

									case "n":
										NonceStr = P.Value;
										Nonce = Convert.FromBase64String(NonceStr);
										break;
								}
							}

							if (!string.IsNullOrEmpty(Realm) && !string.IsNullOrEmpty(NonceStr))
							{
								byte[] Signature = await this.SignAsync(Nonce, SignWith);
								StringBuilder sb = new StringBuilder();

								sb.Append("jid=\"");
								sb.Append(this.client.FullJID);
								sb.Append("\", realm=\"");
								sb.Append(Realm);
								sb.Append("\", n=\"");
								sb.Append(NonceStr);
								sb.Append("\", s=\"");
								sb.Append(Convert.ToBase64String(Signature));
								sb.Append("\"");

								Request.Dispose();
								Request = new HttpRequestMessage()
								{
									RequestUri = new Uri(Url),
									Method = HttpMethod.Get
								};

								Request.Headers.Authorization = new AuthenticationHeaderValue(Header.Scheme, sb.ToString());

								Response.Dispose();
								Response = null;
								Response = await HttpClient.SendAsync(Request);
							}
							break;
						}
					}
				}

				if (!Response.IsSuccessStatusCode)
				{
					ContentResponse Temp = await Content.Getters.WebGetter.ProcessResponse(Response, Request.RequestUri);
					Temp.AssertOk();
				}

				string ContentType = Response.Content.Headers.ContentType.ToString();
				TemporaryFile File = new TemporaryFile();
				try
				{
					await Response.Content.CopyToAsync(File);
				}
				catch (Exception ex)
				{
					File.Dispose();
					File = null;

					ExceptionDispatchInfo.Capture(ex).Throw();
				}

				return new KeyValuePair<string, TemporaryFile>(ContentType, File);
			}
			finally
			{
				Request?.Dispose();
				Response?.Dispose();
			}
		}

		/// <summary>
		/// Removes an attachment from a newly created legal identity.
		/// </summary>
		/// <param name="AttachmentId">ID of Attachment.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task RemoveLegalIdAttachment(string AttachmentId, EventHandlerAsync<LegalIdentityEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<removeAttachment xmlns='");
			Xml.Append(NamespaceLegalIdentitiesCurrent);
			Xml.Append("' attachmentId='");
			Xml.Append(XML.Encode(AttachmentId));
			Xml.Append("'/>");

			return this.client.SendIqSet(this.componentAddress, Xml.ToString(), async (Sender, e) =>
			{
				LegalIdentity Identity = null;
				XmlElement E;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "identity")
					Identity = LegalIdentity.Parse(E);
				else
					e.Ok = false;

				await Callback.Raise(this, new LegalIdentityEventArgs(e, Identity));
			}, State);
		}

		/// <summary>
		/// Removes an attachment from a newly created legal identity.
		/// </summary>
		/// <param name="AttachmentId">ID of Attachment.</param>
		public async Task<LegalIdentity> RemoveLegalIdAttachmentAsync(string AttachmentId)
		{
			TaskCompletionSource<LegalIdentity> Result = new TaskCompletionSource<LegalIdentity>();

			await this.RemoveLegalIdAttachment(AttachmentId, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Identity);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to remove attachment."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		/// <summary>
		/// Removes an attachment from a proposed or approved contract before it is being signed.
		/// </summary>
		/// <param name="AttachmentId">ID of Attachment.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task RemoveContractAttachment(string AttachmentId, EventHandlerAsync<SmartContractEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<removeAttachment xmlns='");
			Xml.Append(NamespaceSmartContractsCurrent);
			Xml.Append("' attachmentId='");
			Xml.Append(XML.Encode(AttachmentId));
			Xml.Append("'/>");

			return this.client.SendIqSet(this.componentAddress, Xml.ToString(), async (Sender, e) =>
			{
				Contract Contract = null;
				XmlElement E;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "contract")
				{
					ParsedContract Parsed = await Contract.Parse(E, this, false);
					if (Parsed is null)
						e.Ok = false;
					else
						Contract = Parsed.Contract;
				}
				else
					e.Ok = false;

				await Callback.Raise(this, new SmartContractEventArgs(e, Contract));
			}, State);
		}

		/// <summary>
		/// Removes an attachment from a proposed or approved contract before it is being signed.
		/// </summary>
		/// <param name="AttachmentId">ID of Attachment.</param>
		public async Task<Contract> RemoveContractAttachmentAsync(string AttachmentId)
		{
			TaskCompletionSource<Contract> Result = new TaskCompletionSource<Contract>();

			await this.RemoveContractAttachment(AttachmentId, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Contract);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to remove attachment."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		#endregion

		#region Encryption

		/// <summary>
		/// Encrypts a message for an intended recipient given its public key and
		/// key algorithm name.
		/// </summary>
		/// <param name="Message">Message to encrypt.</param>
		/// <param name="Nonce">Nonce-value to use during encryption. Size does not matter,
		/// but must be unique for every message.</param>
		/// <param name="RecipientPublicKey">Public key of recipient.</param>
		/// <param name="RecipientPublicKeyName">Name of key algorithm of recipient.</param>
		/// <returns>Encrypted message, together with the public key used to obtain the shared secret.</returns>
		public (byte[], byte[]) Encrypt(byte[] Message, byte[] Nonce, byte[] RecipientPublicKey, string RecipientPublicKeyName)
		{
			return this.Encrypt(Message, Nonce, RecipientPublicKey, RecipientPublicKeyName, string.Empty);
		}

		/// <summary>
		/// Encrypts a message for an intended recipient given its public key and
		/// key algorithm name.
		/// </summary>
		/// <param name="Message">Message to encrypt.</param>
		/// <param name="Nonce">Nonce-value to use during encryption. Size does not matter,
		/// but must be unique for every message.</param>
		/// <param name="RecipientPublicKey">Public key of recipient.</param>
		/// <param name="RecipientPublicKeyName">Name of key algorithm of recipient.</param>
		/// <param name="RecipientPublicKeyNamespace">Namespace of key algorithm of recipient.</param>
		/// <returns>Encrypted message, together with the public key used to obtain the shared secret.</returns>
		public (byte[], byte[]) Encrypt(byte[] Message, byte[] Nonce, byte[] RecipientPublicKey, string RecipientPublicKeyName, string RecipientPublicKeyNamespace)
		{
			IE2eEndpoint LocalEndpoint = this.localKeys.FindLocalEndpoint(RecipientPublicKeyName, RecipientPublicKeyNamespace) ?? throw new NotSupportedException("Unable to find matching local key.");
			IE2eEndpoint RemoteEndpoint = LocalEndpoint.CreatePublic(RecipientPublicKey);
			byte[] LocalPublicKey = LocalEndpoint.PublicKey;
			byte[] Secret = LocalEndpoint.GetSharedSecret(RemoteEndpoint);
			byte[] Digest = Hashes.ComputeSHA256Hash(Secret);
			byte[] NonceDigest = Hashes.ComputeSHA256Hash(Nonce);
			byte[] Key = new byte[16];
			byte[] IV = new byte[16];
			byte[] Encrypted;
			byte[] ToEncrypt;
			int i, j, c;

			for (i = 0; i < 32; i++)
				Secret[i] ^= NonceDigest[i];

			i = Message.Length;
			c = 0;

			do
			{
				i >>= 7;
				c++;
			}
			while (i > 0);

			i = c + Message.Length;
			c = (i + 15) & ~0xf;

			ToEncrypt = new byte[c];
			i = Message.Length;
			j = 0;

			do
			{
				ToEncrypt[j] = (byte)(i & 127);
				i >>= 7;
				if (i > 0)
					ToEncrypt[j] |= 0x80;

				j++;
			}
			while (i > 0);

			Array.Copy(Message, 0, ToEncrypt, j, Message.Length);
			j += Message.Length;

			if (j < c)
				this.rnd.GetBytes(ToEncrypt, j, c - j);

			Array.Copy(Digest, 0, Key, 0, 16);
			Array.Copy(Digest, 16, IV, 0, 16);

			lock (this.aes)
			{
				using ICryptoTransform Aes = this.aes.CreateEncryptor(Key, IV);
				Encrypted = Aes.TransformFinalBlock(ToEncrypt, 0, c);
			}

			return (Encrypted, LocalPublicKey);
		}

		/// <summary>
		/// Decrypts a message that was aimed at the client using the current keys.
		/// </summary>
		/// <param name="EncryptedMessage">Encrypted message.</param>
		/// <param name="SenderPublicKey">Public key used by the sender.</param>
		/// <param name="Nonce">Nonce-value to use during decryption. Must be the same
		/// as the one used during encryption.</param>
		/// <returns>Decrypted message.</returns>
		public async Task<byte[]> Decrypt(byte[] EncryptedMessage, byte[] SenderPublicKey, byte[] Nonce)
		{
			IE2eEndpoint LocalEndpoint = await this.GetMatchingLocalKeyAsync();
			IE2eEndpoint RemoteEndpoint = LocalEndpoint.CreatePublic(SenderPublicKey);
			byte[] Secret = LocalEndpoint.GetSharedSecret(RemoteEndpoint);
			byte[] NonceDigest = Hashes.ComputeSHA256Hash(Nonce);
			byte[] Key = new byte[16];
			byte[] IV = new byte[16];
			byte[] Decrypted;
			int i, c;
			byte b;

			for (i = 0; i < 32; i++)
				Secret[i] ^= NonceDigest[i];

			byte[] Digest = Hashes.ComputeSHA256Hash(Secret);

			Array.Copy(Digest, 0, Key, 0, 16);
			Array.Copy(Digest, 16, IV, 0, 16);

			lock (this.aes)
			{
				using ICryptoTransform Aes = this.aes.CreateDecryptor(Key, IV);
				Decrypted = Aes.TransformFinalBlock(EncryptedMessage, 0, EncryptedMessage.Length);
			}

			i = 0;
			c = 0;
			do
			{
				b = Decrypted[i++];
				c <<= 7;
				c |= b & 0x7f;
			}
			while ((b & 0x80) != 0);

			if (c < 0 || c > Decrypted.Length - i)
				throw new InvalidOperationException("Unable to decrypt message.");

			byte[] Message = new byte[c];

			Array.Copy(Decrypted, i, Message, 0, c);

			return Message;
		}

		#endregion

		#region Explicit authorization of access to Legal IDs

		/// <summary>
		/// Authorizes access to (or revokes access to) a Legal ID of the caller.
		/// </summary>
		/// <param name="LegalId">ID of Legal ID</param>
		/// <param name="RemoteId">ID of Legal ID of remote party</param>
		/// <param name="Authorized">If the remote party is authorized access to the referenced Legal ID or not. (Setting false, revokes earlier authorization.)</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task AuthorizeAccessToId(string LegalId, string RemoteId, bool Authorized, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.AuthorizeAccessToId(this.GetTrustProvider(LegalId), LegalId, RemoteId, Authorized, Callback, State);
		}

		/// <summary>
		/// Authorizes access to (or revokes access to) a Legal ID of the caller.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="LegalId">ID of Legal ID</param>
		/// <param name="RemoteId">ID of Legal ID of remote party</param>
		/// <param name="Authorized">If the remote party is authorized access to the referenced Legal ID or not. (Setting false, revokes earlier authorization.)</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task AuthorizeAccessToId(string Address, string LegalId, string RemoteId, bool Authorized, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<authorizeAccess xmlns='");
			Xml.Append(NamespaceLegalIdentitiesCurrent);
			Xml.Append("' id='");
			Xml.Append(XML.Encode(LegalId));
			Xml.Append("' remoteId='");
			Xml.Append(XML.Encode(RemoteId));
			Xml.Append("' auth='");
			Xml.Append(CommonTypes.Encode(Authorized));
			Xml.Append("'/>");

			return this.client.SendIqSet(Address, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// Authorizes access to (or revokes access to) a Legal ID of the caller.
		/// </summary>
		/// <param name="LegalId">ID of Legal ID</param>
		/// <param name="RemoteId">ID of Legal ID of remote party</param>
		/// <param name="Authorized">If the remote party is authorized access to the referenced Legal ID or not. (Setting false, revokes earlier authorization.)</param>
		public Task AuthorizeAccessToIdAsync(string LegalId, string RemoteId, bool Authorized)
		{
			return this.AuthorizeAccessToIdAsync(this.GetTrustProvider(LegalId), LegalId, RemoteId, Authorized);
		}

		/// <summary>
		/// Authorizes access to (or revokes access to) a Legal ID of the caller.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="LegalId">ID of Legal ID</param>
		/// <param name="RemoteId">ID of Legal ID of remote party</param>
		/// <param name="Authorized">If the remote party is authorized access to the referenced Legal ID or not. (Setting false, revokes earlier authorization.)</param>
		public async Task AuthorizeAccessToIdAsync(string Address, string LegalId, string RemoteId, bool Authorized)
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();

			await this.AuthorizeAccessToId(Address, LegalId, RemoteId, Authorized, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(true);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to authorize access to legal identity."));

				return Task.CompletedTask;

			}, null);

			await Result.Task;
		}

		#endregion

		#region Explicit authorization of access to Contracts

		/// <summary>
		/// Authorizes access to (or revokes access to) a Contract of which the caller is part and can access.
		/// </summary>
		/// <param name="ContractId">ID of Contract</param>
		/// <param name="RemoteId">ID of Legal ID of remote party</param>
		/// <param name="Authorized">If the remote party is authorized access to the referenced Contract or not. (Setting false, revokes earlier authorization.)</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task AuthorizeAccessToContract(string ContractId, string RemoteId, bool Authorized, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.AuthorizeAccessToContract(this.GetTrustProvider(ContractId), ContractId, RemoteId, Authorized, Callback, State);
		}

		/// <summary>
		/// Authorizes access to (or revokes access to) a Contract of which the caller is part and can access.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ContractId">ID of Contract</param>
		/// <param name="RemoteId">ID of Legal ID of remote party</param>
		/// <param name="Authorized">If the remote party is authorized access to the referenced Contract or not. (Setting false, revokes earlier authorization.)</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task AuthorizeAccessToContract(string Address, string ContractId, string RemoteId, bool Authorized, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<authorizeAccess xmlns='");
			Xml.Append(NamespaceSmartContractsCurrent);
			Xml.Append("' id='");
			Xml.Append(XML.Encode(ContractId));
			Xml.Append("' remoteId='");
			Xml.Append(XML.Encode(RemoteId));
			Xml.Append("' auth='");
			Xml.Append(CommonTypes.Encode(Authorized));
			Xml.Append("'/>");

			return this.client.SendIqSet(Address, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// Authorizes access to (or revokes access to) a Contract of which the caller is part and can access.
		/// </summary>
		/// <param name="ContractId">ID of Contract</param>
		/// <param name="RemoteId">ID of Legal ID of remote party</param>
		/// <param name="Authorized">If the remote party is authorized access to the referenced Contract or not. (Setting false, revokes earlier authorization.)</param>
		public Task AuthorizeAccessToContractAsync(string ContractId, string RemoteId, bool Authorized)
		{
			return this.AuthorizeAccessToContractAsync(this.GetTrustProvider(ContractId), ContractId, RemoteId, Authorized);
		}

		/// <summary>
		/// Authorizes access to (or revokes access to) a Contract of which the caller is part and can access.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ContractId">ID of Contract</param>
		/// <param name="RemoteId">ID of Legal ID of remote party</param>
		/// <param name="Authorized">If the remote party is authorized access to the referenced Contract or not. (Setting false, revokes earlier authorization.)</param>
		public async Task AuthorizeAccessToContractAsync(string Address, string ContractId, string RemoteId, bool Authorized)
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();

			await this.AuthorizeAccessToContract(Address, ContractId, RemoteId, Authorized, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(true);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to authorize access to legal identity."));

				return Task.CompletedTask;

			}, null);

			await Result.Task;
		}

		#endregion

		#region Peer-review service providers

		/// <summary>
		/// Gets available service providers who can help review an ID application.
		/// </summary>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetPeerReviewIdServiceProviders(EventHandlerAsync<ServiceProvidersEventArgs<ServiceProviderWithLegalId>> Callback, object State)
		{
			return this.GetPeerReviewIdServiceProviders(this.componentAddress, Callback, State);
		}

		/// <summary>
		/// Gets available service providers who can help review an ID application.
		/// </summary>
		/// <param name="ComponentAddress">Address of component.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetPeerReviewIdServiceProviders(string ComponentAddress,
			EventHandlerAsync<ServiceProvidersEventArgs<ServiceProviderWithLegalId>> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<reviewIdProviders xmlns='");
			Xml.Append(NamespaceLegalIdentitiesCurrent);
			Xml.Append("'/>");

			return this.client.SendIqGet(ComponentAddress, Xml.ToString(), async (Sender, e) =>
			{
				List<ServiceProviderWithLegalId> Providers = null;
				XmlElement E;

				if (e.Ok &&
					!((E = e.FirstElement) is null) &&
					E.LocalName == "providers")
				{
					Providers = new List<ServiceProviderWithLegalId>();

					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2 && E2.LocalName == "provider")
						{
							ServiceProviderWithLegalId Provider = this.ParseServiceProviderWithLegalId(E2);

							if (!(Provider is null))
								Providers.Add(Provider);
						}
					}
				}
				else
					e.Ok = false;

				await Callback.Raise(this, new ServiceProvidersEventArgs<ServiceProviderWithLegalId>(e, Providers?.ToArray()));

			}, State);
		}

		private ServiceProviderWithLegalId ParseServiceProviderWithLegalId(XmlElement Xml)
		{
			string Id = null;
			string Type = null;
			string Name = null;
			string IconUrl = null;
			string LegalId = null;
			int IconWidth = -1;
			int IconHeight = -1;
			bool External = false;

			foreach (XmlAttribute Attr in Xml.Attributes)
			{
				switch (Attr.Name)
				{
					case "id":
						Id = Attr.Value;
						break;

					case "type":
						Type = Attr.Value;
						break;

					case "name":
						Name = Attr.Value;
						break;

					case "iconUrl":
						IconUrl = Attr.Value;
						break;

					case "iconWidth":
						if (!int.TryParse(Attr.Value, out IconWidth))
							return null;
						break;

					case "iconHeight":
						if (!int.TryParse(Attr.Value, out IconHeight))
							return null;
						break;

					case "legalId":
						LegalId = Attr.Value;
						break;

					case "external":
						if (!CommonTypes.TryParse(Attr.Value, out External))
							return null;
						break;
				}
			}

			if (Id is null || Type is null || Name is null)
				return null;

			if (string.IsNullOrEmpty(IconUrl))
				return new ServiceProviderWithLegalId(Id, Type, Name, LegalId, External);
			else
			{
				if (IconWidth < 0 || IconHeight < 0)
					return null;

				return new ServiceProviderWithLegalId(Id, Type, Name, LegalId, External, IconUrl, IconWidth, IconHeight);
			}
		}

		/// <summary>
		/// Gets available service providers who can help review an ID application.
		/// </summary>
		/// <returns>Peer Review Services available.</returns>
		public Task<ServiceProviderWithLegalId[]> GetPeerReviewIdServiceProvidersAsync()
		{
			return this.GetPeerReviewIdServiceProvidersAsync(this.componentAddress);
		}

		/// <summary>
		/// Gets available service providers who can help review an ID application.
		/// </summary>
		/// <param name="ComponentAddress">Address of component.</param>
		/// <returns>Peer Review Services available.</returns>
		public async Task<ServiceProviderWithLegalId[]> GetPeerReviewIdServiceProvidersAsync(string ComponentAddress)
		{
			TaskCompletionSource<ServiceProviderWithLegalId[]> Providers = new TaskCompletionSource<ServiceProviderWithLegalId[]>();

			await this.GetPeerReviewIdServiceProviders(ComponentAddress, (Sender, e) =>
			{
				if (e.Ok)
					Providers.TrySetResult(e.ServiceProviders);
				else
					Providers.TrySetException(e.StanzaError ?? new Exception("Unable to get service providers."));

				return Task.CompletedTask;

			}, null);

			return await Providers.Task;
		}

		#endregion

		#region Select Peer-review service

		/// <summary>
		/// Selects a service provider for peer review. This needs to be done before requesting the trust provider
		/// (given its JID) to peer review an identity application. Such service providers are returned by calling
		/// the GetPeerReviewIdServiceProviders method (or overloads), looking for results with External=false.
		/// </summary>
		/// <param name="Provider">Identifies the Peer Review Service Provider on the server.</param>
		/// <param name="ServiceId">Identifies the Peer Review Service hosted by the service provider.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task SelectPeerReviewService(string Provider, string ServiceId, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.SelectPeerReviewService(this.componentAddress, Provider, ServiceId, Callback, State);
		}

		/// <summary>
		/// Selects a service provider for peer review. This needs to be done before requesting the trust provider
		/// (given its JID) to peer review an identity application. Such service providers are returned by calling
		/// the GetPeerReviewIdServiceProviders method (or overloads), looking for results with External=false.
		/// </summary>
		/// <param name="ComponentAddress">Address of component.</param>
		/// <param name="Provider">Identifies the Peer Review Service Provider on the server.</param>
		/// <param name="ServiceId">Identifies the Peer Review Service hosted by the service provider.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task SelectPeerReviewService(string ComponentAddress, string Provider, string ServiceId,
			EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<selectReviewService xmlns='");
			Xml.Append(NamespaceLegalIdentitiesCurrent);
			Xml.Append("' provider='");
			Xml.Append(XML.Encode(Provider));
			Xml.Append("' serviceId='");
			Xml.Append(XML.Encode(ServiceId));
			Xml.Append("'/>");

			return this.client.SendIqSet(ComponentAddress, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// Selects a service provider for peer review. This needs to be done before requesting the trust provider
		/// (given its JID) to peer review an identity application. Such service providers are returned by calling
		/// the GetPeerReviewIdServiceProviders method (or overloads), looking for results with External=false.
		/// </summary>
		/// <param name="Provider">Identifies the Peer Review Service Provider on the server.</param>
		/// <param name="ServiceId">Identifies the Peer Review Service hosted by the service provider.</param>
		public Task SelectPeerReviewServiceAsync(string Provider, string ServiceId)
		{
			return this.SelectPeerReviewServiceAsync(this.componentAddress, Provider, ServiceId);
		}

		/// <summary>
		/// Selects a service provider for peer review. This needs to be done before requesting the trust provider
		/// (given its JID) to peer review an identity application. Such service providers are returned by calling
		/// the GetPeerReviewIdServiceProviders method (or overloads), looking for results with External=false.
		/// </summary>
		/// <param name="ComponentAddress">Address of component.</param>
		/// <param name="Provider">Identifies the Peer Review Service Provider on the server.</param>
		/// <param name="ServiceId">Identifies the Peer Review Service hosted by the service provider.</param>
		public async Task SelectPeerReviewServiceAsync(string ComponentAddress, string Provider, string ServiceId)
		{
			TaskCompletionSource<bool> Providers = new TaskCompletionSource<bool>();

			await this.SelectPeerReviewService(ComponentAddress, Provider, ServiceId, (Sender, e) =>
			{
				if (e.Ok)
					Providers.TrySetResult(true);
				else
					Providers.TrySetException(e.StanzaError ?? new Exception("Unable to select peer review service."));

				return Task.CompletedTask;

			}, null);

			await Providers.Task;
		}

		#endregion

		#region Petition Client URL event

		private Task PetitionClientUrlEventHandler(object Sender, MessageEventArgs e)
		{
			string PetitionId = XML.Attribute(e.Content, "pid");
			string Url = XML.Attribute(e.Content, "url");

			return this.PetitionClientUrlReceived.Raise(this, new PetitionClientUrlEventArgs(e, PetitionId, Url));
		}

		/// <summary>
		/// Event raised when a Client URL has been sent to the client as part of a
		/// petition process. Such an URL must be opened by the client to complete 
		/// the petition.
		/// </summary>
		public event EventHandlerAsync<PetitionClientUrlEventArgs> PetitionClientUrlReceived;

		#endregion
	}
}
