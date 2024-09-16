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
using Waher.Networking.XMPP.Contracts.HumanReadable;
using Waher.Networking.XMPP.Contracts.Search;
using Waher.Networking.XMPP.P2P;
using Waher.Networking.XMPP.P2P.E2E;
using Waher.Networking.XMPP.StanzaErrors;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Runtime.Cache;
using Waher.Runtime.Inventory;
using Waher.Runtime.Profiling;
using Waher.Runtime.Settings;
using Waher.Runtime.Temporary;
using Waher.Script;
using Waher.Security;
using Waher.Security.CallStack;
using Waher.Security.EllipticCurves;
using Waher.Networking.XMPP.HttpFileUpload;
using Waher.Networking.XMPP.P2P.SymmetricCiphers;
using Waher.Content.Html.Elements;

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
		private object[] approvedSources = null;
		private readonly string componentAddress;
		private string keySettingsPrefix;
		private string contractKeySettingsPrefix;
		private bool keySettingsPrefixLocked = false;
		private bool localKeysForE2e = false;
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
					string LocalName = Setting.Key.Substring(this.keySettingsPrefix.Length);

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
				string Name = Setting.Key.Substring(this.keySettingsPrefix.Length);

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
				string Name = Setting.Key.Substring(this.contractKeySettingsPrefix.Length);

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
		public async Task EnableE2eEncryption(bool UseLocalKeys)
		{
			bool Reload = !(this.localKeys is null);

			this.localKeysForE2e = UseLocalKeys;

			if (Reload)
				await this.LoadKeys(true);
		}

		/// <summary>
		/// Enables End-to-End encryption with a separate set of keys.
		/// </summary>
		/// <param name="E2eEndpoint">Endpoint managing the keys on the network.</param>
		public async Task EnableE2eEncryption(EndpointSecurity E2eEndpoint)
		{
			bool Reload = !(this.localKeys is null);

			this.localKeysForE2e = false;
			this.localE2eEndpoint = E2eEndpoint;

			if (Reload)
				await this.LoadKeys(true);
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
		public Task GetServerPublicKey(KeyEventHandler Callback, object State)
		{
			return this.GetServerPublicKey(this.componentAddress, Callback, State);
		}

		/// <summary>
		/// Gets the server public key.
		/// </summary>
		/// <param name="Address">Address of entity whose public key is requested.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public async Task GetServerPublicKey(string Address, KeyEventHandler Callback, object State)
		{
			KeyEventArgs e0;

			lock (this.publicKeys)
			{
				if (!this.publicKeys.TryGetValue(Address, out e0))
					e0 = null;
			}

			if (!(e0 is null))
			{
				try
				{
					e0 = new KeyEventArgs(e0, e0.Key)
					{
						State = State
					};

					if (!(Callback is null))
						await Callback(this, e0);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}
			else
			{
				this.client.SendIqGet(Address, "<getPublicKey xmlns=\"" + NamespaceLegalIdentitiesCurrent + "\"/>", async (sender, e) =>
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

					if (!(Callback is null))
						await Callback(this, e0);
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

			await this.GetServerPublicKey(Address, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.Key);
				else
					Result.SetException(e.StanzaError ?? new Exception("Unable to get public key."));

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
		public Task GetMatchingLocalKey(KeyEventHandler Callback, object State)
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
		public async Task GetMatchingLocalKey(string Address, KeyEventHandler Callback, object State)
		{
			KeyEventArgs e0;

			lock (this.matchingKeys)
			{
				if (!this.matchingKeys.TryGetValue(Address, out e0))
					e0 = null;
			}

			if (!(e0 is null))
			{
				try
				{
					e0 = new KeyEventArgs(e0, e0.Key)
					{
						State = State
					};

					if (!(Callback is null))
						await Callback(this, e0);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}
			else
			{
				await this.GetServerPublicKey(Address, async (sender, e) =>
				{
					IE2eEndpoint LocalKey = null;

					if (e.Ok)
					{
						LocalKey = this.LocalEndpoint.GetLocalKey(e.Key);
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

					if (!(Callback is null))
						await Callback(this, e0);

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

			await this.GetMatchingLocalKey(Address, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.Key);
				else
					Result.SetException(e.StanzaError ?? new Exception("Unable to get matching local key."));

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
		public void GetIdApplicationAttributes(IdApplicationAttributesEventHandler Callback, object State)
		{
			this.client.SendIqGet(this.componentAddress, "<applicationAttributes xmlns='" + NamespaceLegalIdentitiesCurrent + "'/>", (sender, e) =>
			{
				try
				{
					Callback?.Invoke(this, new IdApplicationAttributesEventArgs(e));
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}

				return Task.CompletedTask;

			}, State);
		}

		/// <summary>
		/// Gets attributes relevant for application for legal identities on the broker.
		/// </summary>
		/// <returns>ID Application attributes</returns>
		public Task<IdApplicationAttributesEventArgs> GetIdApplicationAttributesAsync()
		{
			TaskCompletionSource<IdApplicationAttributesEventArgs> Result = new TaskCompletionSource<IdApplicationAttributesEventArgs>();

			this.GetIdApplicationAttributes((sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to get ID Application attributes."));

				return Task.CompletedTask;
			}, null);

			return Result.Task;
		}

		#endregion

		#region Apply for a Legal Identity

		/// <summary>
		/// Applies for a legal identity to be registered.
		/// </summary>
		/// <param name="Properties">Properties of the legal identity.</param>
		/// <param name="Callback">Method to call when registration response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task Apply(Property[] Properties, LegalIdentityEventHandler Callback, object State)
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
		public async Task Apply(string Address, Property[] Properties, LegalIdentityEventHandler Callback, object State)
		{
			this.AssertAllowed();

			await this.GetMatchingLocalKey(Address, async (sender, e) =>
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

					this.client.SendIqSet(Address, Xml.ToString(), async (sender2, e2) =>
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

						if (!(Callback is null))
							await Callback(this, new LegalIdentityEventArgs(e2, Identity2));
					}, e.State);
				}
				else if (!(Callback is null))
					await Callback(this, new LegalIdentityEventArgs(e, null));
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

			await this.Apply(Address, Properties, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.Identity);
				else
					Result.SetException(e.StanzaError ?? new Exception("Unable to apply for a legal identity to be registered."));

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
		public void ReadyForApproval(string LegalIdentityId, IqResultEventHandlerAsync Callback, object State)
		{
			this.ReadyForApproval(this.componentAddress, LegalIdentityId, Callback, State);
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
		public void ReadyForApproval(string Address, string LegalIdentityId, IqResultEventHandlerAsync Callback, object State)
		{
			this.AssertAllowed();

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<readyForApproval xmlns=\"");
			Xml.Append(NamespaceLegalIdentitiesCurrent);
			Xml.Append("\" id=\"");
			Xml.Append(XML.Encode(LegalIdentityId));
			Xml.Append("\"/>");

			this.client.SendIqSet(Address, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// Marks an Identity as Ready for Approval. Call this after necessary attachments have been
		/// added. If automatic KYC modules exist on the server, they may at this point process
		/// available information, and choose to automatically approve or reject the application.
		/// </summary>
		/// <param name="LegalIdentityId">ID of Legal Identity that is ready for approval.</param>
		/// <returns>Identity object representing the application.</returns>
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
		/// <returns>Identity object representing the application.</returns>
		public Task ReadyForApprovalAsync(string Address, string LegalIdentityId)
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();

			this.ReadyForApproval(Address, LegalIdentityId, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(true);
				else
					Result.SetException(e.StanzaError ?? new Exception("Unable to flag identity as ready for approval."));

				return Task.CompletedTask;

			}, null);

			return Result.Task;
		}

		#endregion

		#region Validate Legal Identity

		/// <summary>
		/// Validates a legal identity.
		/// </summary>
		/// <param name="Identity">Legal identity to validate</param>
		/// <param name="Callback">Method to call when validation is completed</param>
		/// <param name="State">State object to pass to callback method.</param>
		public Task Validate(LegalIdentity Identity, IdentityValidationEventHandler Callback, object State)
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
		public async Task Validate(LegalIdentity Identity, bool ValidateState, IdentityValidationEventHandler Callback, object State)
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

						using (TemporaryFile File = P.Value)
						{
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

			await this.GetServerPublicKey(Identity.Provider, async (sender, e) =>
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
				int.TryParse(Identity.ClientKeyName.Substring(3), out int KeySize))
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
				int.TryParse(Identity.ClientKeyName.Substring(3), out int KeySize))
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

		private async Task ReturnStatus(IdentityStatus Status, IdentityValidationEventHandler Callback, object State)
		{
			if (!(Callback is null))
			{
				try
				{
					await Callback(this, new IdentityValidationEventArgs(Status, State));
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}
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

			await this.Validate(Identity, ValidateState, (sender, e) =>
			{
				Result.SetResult(e.Status);
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

			Id = Id.Substring(i + 1);

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

				LegalIdentityEventHandler h = this.IdentityUpdated;
				if (!(h is null))
				{
					try
					{
						await h(this, new LegalIdentityEventArgs(new IqResultEventArgs(e.Message, e.Id, e.To, e.From, e.Ok, null), Identity));
					}
					catch (Exception ex)
					{
						this.client.Exception(ex);
					}
				}
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

			IE2eEndpoint Endpoint = this.LocalEndpoint.GetLocalKey(State.PublicKey);

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

				IE2eEndpoint Endpoint = this.LocalEndpoint.GetLocalKey(State.PublicKey);
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

				IE2eEndpoint Endpoint = this.LocalEndpoint.GetLocalKey(State.PublicKey);
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
						Types.UnregisterSingleton(StateObj, Identity.Id);
						Types.RegisterSingleton(StateObj2, Identity.Id);
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
		public event LegalIdentityEventHandler IdentityUpdated = null;

		#endregion

		#region Get Legal Identities

		/// <summary>
		/// Gets legal identities registered with the account.
		/// </summary>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public void GetLegalIdentities(LegalIdentitiesEventHandler Callback, object State)
		{
			this.GetLegalIdentities(this.componentAddress, Callback, State);
		}

		/// <summary>
		/// Gets legal identities registered with the account.
		/// </summary>
		/// <param name="Address">Address of entity on which the legal identities are registered.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public void GetLegalIdentities(string Address, LegalIdentitiesEventHandler Callback, object State)
		{
			this.client.SendIqGet(Address, "<getLegalIdentities xmlns=\"" + NamespaceLegalIdentitiesCurrent + "\"/>",
				this.IdentitiesResponse, new object[] { Callback, State });
		}

		private async Task IdentitiesResponse(object Sender, IqResultEventArgs e)
		{
			object[] P = (object[])e.State;
			LegalIdentitiesEventHandler Callback = (LegalIdentitiesEventHandler)P[0];
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
			if (!(Callback is null))
				await Callback(this, new LegalIdentitiesEventArgs(e, Identities));
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
		public Task<LegalIdentity[]> GetLegalIdentitiesAsync(string Address)
		{
			TaskCompletionSource<LegalIdentity[]> Result = new TaskCompletionSource<LegalIdentity[]>();

			this.GetLegalIdentities(Address, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.Identities);
				else
					Result.SetException(e.StanzaError ?? new Exception("Unable to get legal identities."));

				return Task.CompletedTask;

			}, null);

			return Result.Task;
		}

		#endregion

		#region Get Legal Identity

		/// <summary>
		/// Gets information about a legal identity given its ID.
		/// </summary>
		/// <param name="LegalIdentityId">ID of the legal identity to get.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public void GetLegalIdentity(string LegalIdentityId, LegalIdentityEventHandler Callback, object State)
		{
			this.GetLegalIdentity(this.GetTrustProvider(LegalIdentityId), LegalIdentityId, Callback, State);
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
				return EntityId.Substring(i + 1);
		}

		/// <summary>
		/// Gets information about a legal identity given its ID.
		/// </summary>
		/// <param name="Address">Address of entity on which the legal identity are registered.</param>
		/// <param name="LegalIdentityId">ID of the legal identity to get.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public void GetLegalIdentity(string Address, string LegalIdentityId, LegalIdentityEventHandler Callback, object State)
		{
			this.client.SendIqGet(Address, "<getLegalIdentity id=\"" + XML.Encode(LegalIdentityId) + "\" xmlns=\"" +
				NamespaceLegalIdentitiesCurrent + "\"/>", async (sender, e) =>
			{
				LegalIdentity Identity = null;
				XmlElement E;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "identity")
					Identity = LegalIdentity.Parse(E);
				else
					e.Ok = false;

				if (!(Callback is null))
					await Callback(this, new LegalIdentityEventArgs(e, Identity));
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
		public Task<LegalIdentity> GetLegalIdentityAsync(string Address, string LegalIdentityId)
		{
			TaskCompletionSource<LegalIdentity> Result = new TaskCompletionSource<LegalIdentity>();

			this.GetLegalIdentity(Address, LegalIdentityId, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.Identity);
				else
					Result.SetException(e.StanzaError ?? new Exception("Unable to get legal identity."));

				return Task.CompletedTask;

			}, null);

			return Result.Task;
		}

		#endregion

		#region Obsolete Legal Identity

		/// <summary>
		/// Obsoletes one of the legal identities of the account, given its ID.
		/// </summary>
		/// <param name="LegalIdentityId">ID of the legal identity to obsolete.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public void ObsoleteLegalIdentity(string LegalIdentityId, LegalIdentityEventHandler Callback, object State)
		{
			this.ObsoleteLegalIdentity(this.GetTrustProvider(LegalIdentityId), LegalIdentityId, Callback, State);
		}

		/// <summary>
		/// Obsoletes one of the legal identities of the account, given its ID.
		/// </summary>
		/// <param name="Address">Address of entity on which the legal identity are registered.</param>
		/// <param name="LegalIdentityId">ID of the legal identity to obsolete.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public void ObsoleteLegalIdentity(string Address, string LegalIdentityId, LegalIdentityEventHandler Callback, object State)
		{
			this.AssertAllowed();

			this.client.SendIqSet(Address, "<obsoleteLegalIdentity id=\"" + XML.Encode(LegalIdentityId) + "\" xmlns=\"" +
				NamespaceLegalIdentitiesCurrent + "\"/>", async (sender, e) =>
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

					if (!(Callback is null))
						await Callback(this, new LegalIdentityEventArgs(e, Identity));
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
		public Task<LegalIdentity> ObsoleteLegalIdentityAsync(string Address, string LegalIdentityId)
		{
			TaskCompletionSource<LegalIdentity> Result = new TaskCompletionSource<LegalIdentity>();

			this.ObsoleteLegalIdentity(Address, LegalIdentityId, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.Identity);
				else
					Result.SetException(e.StanzaError ?? new Exception("Unable to obsolete legal identity."));

				return Task.CompletedTask;

			}, null);

			return Result.Task;
		}

		#endregion

		#region Compromised Legal Identity

		/// <summary>
		/// Reports as Compromised one of the legal identities of the account, given its ID.
		/// </summary>
		/// <param name="LegalIdentityId">ID of the legal identity to compromise.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public void CompromisedLegalIdentity(string LegalIdentityId, LegalIdentityEventHandler Callback, object State)
		{
			this.CompromisedLegalIdentity(this.GetTrustProvider(LegalIdentityId), LegalIdentityId, Callback, State);
		}

		/// <summary>
		/// Reports as Compromised one of the legal identities of the account, given its ID.
		/// </summary>
		/// <param name="Address">Address of entity on which the legal identity are registered.</param>
		/// <param name="LegalIdentityId">ID of the legal identity to compromise.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public void CompromisedLegalIdentity(string Address, string LegalIdentityId, LegalIdentityEventHandler Callback, object State)
		{
			this.AssertAllowed();

			this.client.SendIqSet(Address, "<compromisedLegalIdentity id=\"" + XML.Encode(LegalIdentityId) + "\" xmlns=\"" +
				NamespaceLegalIdentitiesCurrent + "\"/>", async (sender, e) =>
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

					if (!(Callback is null))
						await Callback(this, new LegalIdentityEventArgs(e, Identity));
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
		public Task<LegalIdentity> CompromisedLegalIdentityAsync(string Address, string LegalIdentityId)
		{
			TaskCompletionSource<LegalIdentity> Result = new TaskCompletionSource<LegalIdentity>();

			this.CompromisedLegalIdentity(Address, LegalIdentityId, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.Identity);
				else
					Result.SetException(e.StanzaError ?? new Exception("Unable to compromise legal identity."));

				return Task.CompletedTask;

			}, null);

			return Result.Task;
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
		public Task Sign(byte[] Data, SignWith SignWith, SignatureEventHandler Callback, object State)
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
		public async Task Sign(string Address, byte[] Data, SignWith SignWith, SignatureEventHandler Callback, object State)
		{
			this.AssertAllowed();

			byte[] Signature = null;
			IE2eEndpoint Key;

			switch (SignWith)
			{
				case SignWith.CurrentKeys:
					Key = null;
					break;

				case SignWith.LatestApprovedId:
					Key = await this.GetLatestApprovedKey(true);
					break;

				case SignWith.LatestApprovedIdOrCurrentKeys:
				default:
					Key = await this.GetLatestApprovedKey(false);
					break;
			}

			if (Key is null)
			{
				await this.GetMatchingLocalKey(Address, async (sender, e) =>
				{
					if (e.Ok)
						Signature = e.Key.Sign(Data);

					if (!(Callback is null))
						await Callback(this, new SignatureEventArgs(e, Signature));

				}, State);
			}
			else
			{
				Signature = Key.Sign(Data);

				if (!(Callback is null))
					await Callback(this, new SignatureEventArgs(Key, Signature, State));
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

			await this.Sign(Address, Data, SignWith, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.Signature);
				else
					Result.SetException(e.StanzaError ?? new Exception("Unable to sign data."));

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
		public Task Sign(Stream Data, SignWith SignWith, SignatureEventHandler Callback, object State)
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
		public async Task Sign(string Address, Stream Data, SignWith SignWith, SignatureEventHandler Callback, object State)
		{
			this.AssertAllowed();

			IE2eEndpoint Key = SignWith == SignWith.CurrentKeys ? null : await this.GetLatestApprovedKey(true);
			byte[] Signature = null;

			if (Key is null)
			{
				await this.GetMatchingLocalKey(Address, async (sender, e) =>
				{
					if (e.Ok)
						Signature = e.Key.Sign(Data);

					if (!(Callback is null))
						await Callback(this, new SignatureEventArgs(e, Signature));

				}, State);
			}
			else
			{
				Signature = Key.Sign(Data);

				if (!(Callback is null))
					await Callback(this, new SignatureEventArgs(Key, Signature, State));
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

			await this.Sign(Address, Data, SignWith, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.Signature);
				else
					Result.SetException(e.StanzaError ?? new Exception("Unable to sign data."));

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
		public void ValidateSignature(string LegalId, byte[] Data, byte[] Signature, LegalIdentityEventHandler Callback, object State)
		{
			this.ValidateSignature(this.GetTrustProvider(LegalId), LegalId, Data, Signature, Callback, State);
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
		public void ValidateSignature(string Address, string LegalId, byte[] Data, byte[] Signature, LegalIdentityEventHandler Callback, object State)
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

			this.client.SendIqGet(Address, Xml.ToString(), async (sender, e) =>
			{
				LegalIdentity Identity = null;
				XmlElement E;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "identity")
					Identity = LegalIdentity.Parse(E);
				else
					e.Ok = false;

				if (!(Callback is null))
					await Callback(this, new LegalIdentityEventArgs(e, Identity));
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
		public Task<LegalIdentity> ValidateSignatureAsync(string Address, string LegalId, byte[] Data, byte[] Signature)
		{
			TaskCompletionSource<LegalIdentity> Result = new TaskCompletionSource<LegalIdentity>();

			this.ValidateSignature(Address, LegalId, Data, Signature, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.Identity);
				else
					Result.SetException(e.StanzaError ?? new Exception("Unable to sign data."));

				return Task.CompletedTask;

			}, null);

			return Result.Task;
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
		public void CreateContract(XmlElement ForMachines, HumanReadableText[] ForHumans, Role[] Roles,
			Part[] Parts, Parameter[] Parameters, ContractVisibility Visibility, ContractParts PartsMode, Duration? Duration,
			Duration? ArchiveRequired, Duration? ArchiveOptional, DateTime? SignAfter, DateTime? SignBefore, bool CanActAsTemplate,
			SmartContractEventHandler Callback, object State)
		{
			this.CreateContract(this.componentAddress, ForMachines, ForHumans, Roles, Parts, Parameters, Visibility, PartsMode,
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
		public void CreateContract(string Address, XmlElement ForMachines, HumanReadableText[] ForHumans, Role[] Roles,
			Part[] Parts, Parameter[] Parameters, ContractVisibility Visibility, ContractParts PartsMode, Duration? Duration,
			Duration? ArchiveRequired, Duration? ArchiveOptional, DateTime? SignAfter, DateTime? SignBefore, bool CanActAsTemplate,
			SmartContractEventHandler Callback, object State)
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

			this.client.SendIqSet(Address, Xml.ToString(), this.ContractResponse, new object[] { Callback, State });
		}

		private async Task ContractResponse(object Sender, IqResultEventArgs e)
		{
			object[] P = (object[])e.State;
			SmartContractEventHandler Callback = (SmartContractEventHandler)P[0];
			Contract Contract = null;
			XmlElement E;

			if (e.Ok && !((E = e.FirstElement) is null) &&
				E.LocalName == "contract")
			{
				ParsedContract Parsed = await Contract.Parse(E, this, false);
				Contract = Parsed?.Contract;
				if (Contract is null)
					e.Ok = false;
			}
			else
				e.Ok = false;

			e.State = P[1];
			if (!(Callback is null))
				await Callback(this, new SmartContractEventArgs(e, Contract));
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
		public Task<Contract> CreateContractAsync(string Address, XmlElement ForMachines, HumanReadableText[] ForHumans, Role[] Roles,
			Part[] Parts, Parameter[] Parameters, ContractVisibility Visibility, ContractParts PartsMode, Duration? Duration,
			Duration? ArchiveRequired, Duration? ArchiveOptional, DateTime? SignAfter, DateTime? SignBefore, bool CanActAsTemplate)
		{
			TaskCompletionSource<Contract> Result = new TaskCompletionSource<Contract>();

			this.CreateContract(Address, ForMachines, ForHumans, Roles, Parts, Parameters, Visibility, PartsMode, Duration,
				ArchiveRequired, ArchiveOptional, SignAfter, SignBefore, CanActAsTemplate, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.Contract);
				else
					Result.SetException(e.StanzaError ?? new Exception("Unable to create the contract."));

				return Task.CompletedTask;

			}, null);

			return Result.Task;
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
		public void CreateContract(string TemplateId, Part[] Parts, Parameter[] Parameters, ContractVisibility Visibility,
			ContractParts PartsMode, Duration? Duration, Duration? ArchiveRequired, Duration? ArchiveOptional, DateTime? SignAfter,
			DateTime? SignBefore, bool CanActAsTemplate, SmartContractEventHandler Callback, object State)
		{
			this.CreateContract(this.componentAddress, TemplateId, Parts, Parameters, Visibility, PartsMode,
				Duration, ArchiveRequired, ArchiveOptional, SignAfter, SignBefore, CanActAsTemplate, null, Callback, State);
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
		public void CreateContract(string Address, string TemplateId, Part[] Parts, Parameter[] Parameters, ContractVisibility Visibility,
			ContractParts PartsMode, Duration? Duration, Duration? ArchiveRequired, Duration? ArchiveOptional, DateTime? SignAfter,
			DateTime? SignBefore, bool CanActAsTemplate, SmartContractEventHandler Callback, object State)
		{
			this. CreateContract(Address, TemplateId, Parts, Parameters, Visibility,
				PartsMode, Duration, ArchiveRequired, ArchiveOptional, SignAfter,
				SignBefore, CanActAsTemplate, null, Callback, State);
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
		/// <param name="Nonce">An optional nonce value that is used when encrypting protected parameter values.</param>
		/// <param name="Callback">Method to call when registration response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void CreateContract(string TemplateId, Part[] Parts, Parameter[] Parameters, ContractVisibility Visibility,
			ContractParts PartsMode, Duration? Duration, Duration? ArchiveRequired, Duration? ArchiveOptional, DateTime? SignAfter,
			DateTime? SignBefore, bool CanActAsTemplate, byte[] Nonce, SmartContractEventHandler Callback, object State)
		{
			this.CreateContract(this.componentAddress, TemplateId, Parts, Parameters, Visibility, PartsMode,
				Duration, ArchiveRequired, ArchiveOptional, SignAfter, SignBefore, CanActAsTemplate, Nonce, Callback, State);
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
		/// <param name="Nonce">An optional nonce value that is used when encrypting protected parameter values.</param>
		/// <param name="Callback">Method to call when registration response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void CreateContract(string Address, string TemplateId, Part[] Parts, Parameter[] Parameters, ContractVisibility Visibility,
			ContractParts PartsMode, Duration? Duration, Duration? ArchiveRequired, Duration? ArchiveOptional, DateTime? SignAfter,
			DateTime? SignBefore, bool CanActAsTemplate, byte[] Nonce, SmartContractEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			if (Nonce is null)
				Nonce = Guid.NewGuid().ToByteArray();

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
			Xml.Append(Convert.ToBase64String(Nonce));
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
						if (Parameter.ProtectedValue is null)
							Parameter.ProtectedValue = Guid.NewGuid().ToByteArray();

						if (TransientParameters is null)
							TransientParameters = new LinkedList<Parameter>();

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

			this.client.SendIqSet(Address, Xml.ToString(), this.ContractResponse, new object[] { Callback, State });
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
				PartsMode, Duration, ArchiveRequired, ArchiveOptional, SignAfter, SignBefore, CanActAsTemplate, null);
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
		public Task<Contract> CreateContractAsync(string Address, string TemplateId, Part[] Parts, Parameter[] Parameters,
			ContractVisibility Visibility, ContractParts PartsMode, Duration? Duration, Duration? ArchiveRequired, Duration? ArchiveOptional,
			DateTime? SignAfter, DateTime? SignBefore, bool CanActAsTemplate)
		{
			return this.CreateContractAsync(Address, TemplateId, Parts, Parameters, Visibility,
				PartsMode, Duration, ArchiveRequired, ArchiveOptional, SignAfter,
				SignBefore, CanActAsTemplate, null);
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
		/// <param name="Nonce">An optional nonce value that is used when encrypting protected parameter values.</param>
		/// <returns>Contract.</returns>
		public Task<Contract> CreateContractAsync(string TemplateId, Part[] Parts, Parameter[] Parameters, ContractVisibility Visibility,
			ContractParts PartsMode, Duration? Duration, Duration? ArchiveRequired, Duration? ArchiveOptional, DateTime? SignAfter,
			DateTime? SignBefore, bool CanActAsTemplate, byte[] Nonce)
		{
			return this.CreateContractAsync(this.componentAddress, TemplateId, Parts, Parameters, Visibility,
				PartsMode, Duration, ArchiveRequired, ArchiveOptional, SignAfter, SignBefore, CanActAsTemplate, Nonce);
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
		/// <param name="Nonce">An optional nonce value that is used when encrypting protected parameter values.</param>
		/// <returns>Contract.</returns>
		public Task<Contract> CreateContractAsync(string Address, string TemplateId, Part[] Parts, Parameter[] Parameters,
			ContractVisibility Visibility, ContractParts PartsMode, Duration? Duration, Duration? ArchiveRequired, Duration? ArchiveOptional,
			DateTime? SignAfter, DateTime? SignBefore, bool CanActAsTemplate, byte[] Nonce)
		{
			TaskCompletionSource<Contract> Result = new TaskCompletionSource<Contract>();

			this.CreateContract(Address, TemplateId, Parts, Parameters, Visibility, PartsMode, Duration,
				ArchiveRequired, ArchiveOptional, SignAfter, SignBefore, CanActAsTemplate, Nonce, (sender, e) =>
				{
					if (e.Ok)
						Result.SetResult(e.Contract);
					else
						Result.SetException(e.StanzaError ?? new Exception("Unable to create the contract."));

					return Task.CompletedTask;

				}, null);

			return Result.Task;
		}

		#endregion

		#region Get Created Contract References

		/// <summary>
		/// Get references to contracts the account has created.
		/// </summary>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetCreatedContractReferences(IdReferencesEventHandler Callback, object State)
		{
			this.GetCreatedContractReferences(this.componentAddress, 0, int.MaxValue, Callback, State);
		}

		/// <summary>
		/// Get references to contracts the account has created.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetCreatedContractReferences(string Address, IdReferencesEventHandler Callback, object State)
		{
			this.GetCreatedContractReferences(Address, 0, int.MaxValue, Callback, State);
		}

		/// <summary>
		/// Get references to contracts the account has created.
		/// </summary>
		/// <param name="Offset">Result will start with the response at this offset into result set.</param>
		/// <param name="MaxCount">Result will be limited to this number of items.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetCreatedContractReferences(int Offset, int MaxCount, IdReferencesEventHandler Callback, object State)
		{
			this.GetCreatedContractReferences(this.componentAddress, Offset, MaxCount, Callback, State);
		}

		/// <summary>
		/// Get references to contracts the account has created.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Offset">Result will start with the response at this offset into result set.</param>
		/// <param name="MaxCount">Result will be limited to this number of items.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetCreatedContractReferences(string Address, int Offset, int MaxCount, IdReferencesEventHandler Callback, object State)
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

			this.client.SendIqGet(Address, Xml.ToString(), this.IdReferencesResponse, new object[] { Callback, State });
		}

		private async Task IdReferencesResponse(object Sender, IqResultEventArgs e)
		{
			object[] P = (object[])e.State;
			IdReferencesEventHandler Callback = (IdReferencesEventHandler)P[0];
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
			if (!(Callback is null))
				await Callback(this, new IdReferencesEventArgs(e, IDs.ToArray()));
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
		public Task<string[]> GetCreatedContractReferencesAsync(string Address, int Offset, int MaxCount)
		{
			TaskCompletionSource<string[]> Result = new TaskCompletionSource<string[]>();

			this.GetCreatedContractReferences(Address, Offset, MaxCount, (sender, e) =>
				{
					if (e.Ok)
						Result.SetResult(e.References);
					else
						Result.SetException(e.StanzaError ?? new Exception("Unable to get created contract references."));

					return Task.CompletedTask;

				}, null);

			return Result.Task;
		}

		#endregion

		#region Get Created Contracts

		/// <summary>
		/// Get contracts the account has created.
		/// </summary>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetCreatedContracts(ContractsEventHandler Callback, object State)
		{
			this.GetCreatedContracts(this.componentAddress, 0, int.MaxValue, Callback, State);
		}

		/// <summary>
		/// Get contracts the account has created.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetCreatedContracts(string Address, ContractsEventHandler Callback, object State)
		{
			this.GetCreatedContracts(Address, 0, int.MaxValue, Callback, State);
		}

		/// <summary>
		/// Get contracts the account has created.
		/// </summary>
		/// <param name="Offset">Result will start with the response at this offset into result set.</param>
		/// <param name="MaxCount">Result will be limited to this number of items.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetCreatedContracts(int Offset, int MaxCount, ContractsEventHandler Callback, object State)
		{
			this.GetCreatedContracts(this.componentAddress, Offset, MaxCount, Callback, State);
		}

		/// <summary>
		/// Get contracts the account has created.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Offset">Result will start with the response at this offset into result set.</param>
		/// <param name="MaxCount">Result will be limited to this number of items.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetCreatedContracts(string Address, int Offset, int MaxCount, ContractsEventHandler Callback, object State)
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

			this.client.SendIqGet(Address, Xml.ToString(), this.ContractsResponse, new object[] { Callback, State });
		}

		private async Task ContractsResponse(object Sender, IqResultEventArgs e)
		{
			object[] P = (object[])e.State;
			ContractsEventHandler Callback = (ContractsEventHandler)P[0];
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
			if (!(Callback is null))
				await Callback(this, new ContractsEventArgs(e, Contracts.ToArray(), References.ToArray()));
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
		public Task<ContractsEventArgs> GetCreatedContractsAsync(string Address, int Offset, int MaxCount)
		{
			TaskCompletionSource<ContractsEventArgs> Result = new TaskCompletionSource<ContractsEventArgs>();

			this.GetCreatedContracts(Address, Offset, MaxCount, (sender, e) =>
			{
				Result.SetResult(e);
				return Task.CompletedTask;

			}, null);

			return Result.Task;
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
		public Task SignContract(Contract Contract, string Role, bool Transferable, SmartContractEventHandler Callback, object State)
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
			SmartContractEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();
			Contract.Serialize(Xml, false, false, false, false, false, false, false);
			byte[] Data = Encoding.UTF8.GetBytes(Xml.ToString());

			Log.Debug("```\r\n" + Xml.ToString() + "\r\n```");  // TODO: Remove.

			await this.Sign(Address, Data, SignWith.LatestApprovedId, async (sender, e) =>
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

					this.client.SendIqSet(Address, Xml.ToString(), this.ContractResponse, new object[] { Callback, State });
				}
				else if (!(Callback is null))
					await Callback(this, new SmartContractEventArgs(e, null));
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

			await this.SignContract(Address, Contract, Role, Transferable, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.Contract);
				else
					Result.SetException(e.StanzaError ?? new Exception("Unable to sign the contract."));

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
		public void GetSignedContractReferences(IdReferencesEventHandler Callback, object State)
		{
			this.GetSignedContractReferences(this.componentAddress, 0, int.MaxValue, Callback, State);
		}

		/// <summary>
		/// Get references to contracts the account has signed.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetSignedContractReferences(string Address, IdReferencesEventHandler Callback, object State)
		{
			this.GetSignedContractReferences(Address, 0, int.MaxValue, Callback, State);
		}

		/// <summary>
		/// Get references to contracts the account has signed.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Offset">Result will start with the response at this offset into result set.</param>
		/// <param name="MaxCount">Result will be limited to this number of items.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetSignedContractReferences(string Address, int Offset, int MaxCount, IdReferencesEventHandler Callback, object State)
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

			this.client.SendIqGet(Address, Xml.ToString(), this.IdReferencesResponse, new object[] { Callback, State });
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
		public Task<string[]> GetSignedContractReferencesAsync(string Address, int Offset, int MaxCount)
		{
			TaskCompletionSource<string[]> Result = new TaskCompletionSource<string[]>();

			this.GetSignedContractReferences(Address, Offset, MaxCount, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.References);
				else
					Result.SetException(e.StanzaError ?? new Exception("Unable to get signed contract references."));

				return Task.CompletedTask;

			}, null);

			return Result.Task;
		}

		#endregion

		#region Get Signed Contracts

		/// <summary>
		/// Get contracts the account has signed.
		/// </summary>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetSignedContracts(ContractsEventHandler Callback, object State)
		{
			this.GetSignedContracts(this.componentAddress, 0, int.MaxValue, Callback, State);
		}

		/// <summary>
		/// Get contracts the account has signed.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetSignedContracts(string Address, ContractsEventHandler Callback, object State)
		{
			this.GetSignedContracts(Address, 0, int.MaxValue, Callback, State);
		}

		/// <summary>
		/// Get contracts the account has signed.
		/// </summary>
		/// <param name="Offset">Result will start with the response at this offset into result set.</param>
		/// <param name="MaxCount">Result will be limited to this number of items.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetSignedContracts(int Offset, int MaxCount, ContractsEventHandler Callback, object State)
		{
			this.GetSignedContracts(this.componentAddress, Offset, MaxCount, Callback, State);
		}

		/// <summary>
		/// Get contracts the account has signed.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Offset">Result will start with the response at this offset into result set.</param>
		/// <param name="MaxCount">Result will be limited to this number of items.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetSignedContracts(string Address, int Offset, int MaxCount, ContractsEventHandler Callback, object State)
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

			this.client.SendIqGet(Address, Xml.ToString(), this.ContractsResponse, new object[] { Callback, State });
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
		public Task<ContractsEventArgs> GetSignedContractsAsync(string Address, int Offset, int MaxCount)
		{
			TaskCompletionSource<ContractsEventArgs> Result = new TaskCompletionSource<ContractsEventArgs>();

			this.GetSignedContracts(Address, Offset, MaxCount, (sender, e) =>
			{
				Result.SetResult(e);
				return Task.CompletedTask;

			}, null);

			return Result.Task;
		}

		#endregion

		#region Contract Signature event

		private async Task ContractSignedMessageHandler(object Sender, MessageEventArgs e)
		{
			ContractSignedEventHandler h = this.ContractSigned;

			if (!(h is null))
			{
				string ContractId = XML.Attribute(e.Content, "contractId");
				string LegalId = XML.Attribute(e.Content, "legalId");
				string Role = XML.Attribute(e.Content, "role");

				await h(this, new ContractSignedEventArgs(ContractId, LegalId, Role));
			}
		}

		/// <summary>
		/// Event raised whenever a contract has been signed.
		/// </summary>
		public event ContractSignedEventHandler ContractSigned = null;

		#endregion

		#region Get Contract

		/// <summary>
		/// Gets a contract
		/// </summary>
		/// <param name="ContractId">ID of contract to get.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetContract(string ContractId, SmartContractEventHandler Callback, object State)
		{
			this.GetContract(this.GetTrustProvider(ContractId), ContractId, Callback, State);
		}

		/// <summary>
		/// Gets a contract
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ContractId">ID of contract to get.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetContract(string Address, string ContractId, SmartContractEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getContract xmlns='");
			Xml.Append(NamespaceSmartContractsCurrent);
			Xml.Append("' id='");
			Xml.Append(XML.Encode(ContractId));
			Xml.Append("'/>");

			this.client.SendIqGet(Address, Xml.ToString(), this.ContractResponse, new object[] { Callback, State });
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
		public Task<Contract> GetContractAsync(string Address, string ContractId)
		{
			TaskCompletionSource<Contract> Result = new TaskCompletionSource<Contract>();

			this.GetContract(Address, ContractId, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.Contract);
				else
					Result.SetException(e.StanzaError ?? new Exception("Unable to get the contract."));

				return Task.CompletedTask;

			}, null);

			return Result.Task;
		}

		#endregion

		#region Get Contracts

		/// <summary>
		/// Gets a collection of contracts
		/// </summary>
		/// <param name="ContractIds">IDs of contracts to get.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetContracts(string[] ContractIds, ContractsEventHandler Callback, object State)
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
				this.GetContracts(P.Key, P.Value.ToArray(), (sender, e) =>
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
							return Task.CompletedTask;
					}

					if (!(Callback is null))
					{
						try
						{
							ContractsEventArgs e2 = new ContractsEventArgs(e, Contracts.ToArray(), References.ToArray())
							{
								Ok = Ok
							};

							Callback(this, e2);
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
						}
					}

					return Task.CompletedTask;
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
		public void GetContracts(string Address, string[] ContractIds, ContractsEventHandler Callback, object State)
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

			this.client.SendIqGet(Address, Xml.ToString(), this.ContractsResponse, new object[] { Callback, State });
		}

		/// <summary>
		/// Gets a collection of contracts
		/// </summary>
		/// <param name="ContractIds">IDs of contracts to get.</param>
		/// <returns>Contract</returns>
		public Task<ContractsEventArgs> GetContractsAsync(string[] ContractIds)
		{
			TaskCompletionSource<ContractsEventArgs> Result = new TaskCompletionSource<ContractsEventArgs>();

			this.GetContracts(ContractIds, (sender, e) =>
			{
				Result.SetResult(e);
				return Task.CompletedTask;

			}, null);

			return Result.Task;
		}

		/// <summary>
		/// Gets a collection of contracts
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ContractIds">IDs of contracts to get.</param>
		/// <returns>Contracts that could be retrieved, and references for the IDs that could not be retrieved.</returns>
		public Task<ContractsEventArgs> GetContractsAsync(string Address, string[] ContractIds)
		{
			TaskCompletionSource<ContractsEventArgs> Result = new TaskCompletionSource<ContractsEventArgs>();

			this.GetContracts(Address, ContractIds, (sender, e) =>
			{
				Result.SetResult(e);
				return Task.CompletedTask;

			}, null);

			return Result.Task;
		}

		#endregion

		#region Obsolete Contract

		/// <summary>
		/// Obsoletes a contract
		/// </summary>
		/// <param name="ContractId">ID of contract to obsolete.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void ObsoleteContract(string ContractId, SmartContractEventHandler Callback, object State)
		{
			this.ObsoleteContract(this.GetTrustProvider(ContractId), ContractId, Callback, State);
		}

		/// <summary>
		/// Obsoletes a contract
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ContractId">ID of contract to obsolete.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void ObsoleteContract(string Address, string ContractId,
			SmartContractEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<obsoleteContract xmlns='");
			Xml.Append(NamespaceSmartContractsCurrent);
			Xml.Append("' id='");
			Xml.Append(XML.Encode(ContractId));
			Xml.Append("'/>");

			this.client.SendIqSet(Address, Xml.ToString(), this.ContractResponse, new object[] { Callback, State });
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
		public Task<Contract> ObsoleteContractAsync(string Address, string ContractId)
		{
			TaskCompletionSource<Contract> Result = new TaskCompletionSource<Contract>();

			this.ObsoleteContract(Address, ContractId, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.Contract);
				else
					Result.SetException(e.StanzaError ?? new Exception("Unable to obsolete the contract."));

				return Task.CompletedTask;

			}, null);

			return Result.Task;
		}

		#endregion

		#region Delete Contract

		/// <summary>
		/// Deletes a contract
		/// </summary>
		/// <param name="ContractId">ID of contract to delete.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void DeleteContract(string ContractId, SmartContractEventHandler Callback, object State)
		{
			this.DeleteContract(this.GetTrustProvider(ContractId), ContractId, Callback, State);
		}

		/// <summary>
		/// Deletes a contract
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ContractId">ID of contract to delete.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void DeleteContract(string Address, string ContractId,
			SmartContractEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<deleteContract xmlns='");
			Xml.Append(NamespaceSmartContractsCurrent);
			Xml.Append("' id='");
			Xml.Append(XML.Encode(ContractId));
			Xml.Append("'/>");

			this.client.SendIqSet(Address, Xml.ToString(), this.ContractResponse, new object[] { Callback, State });
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
		public Task<Contract> DeleteContractAsync(string Address, string ContractId)
		{
			TaskCompletionSource<Contract> Result = new TaskCompletionSource<Contract>();

			this.DeleteContract(Address, ContractId, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.Contract);
				else
					Result.SetException(e.StanzaError ?? new Exception("Unable to delete the contract."));

				return Task.CompletedTask;

			}, null);

			return Result.Task;
		}

		#endregion

		#region Contract Created event

		private async Task ContractCreatedMessageHandler(object Sender, MessageEventArgs e)
		{
			ContractReferenceEventHandler h = this.ContractCreated;

			if (!(h is null))
			{
				string ContractId = XML.Attribute(e.Content, "contractId");

				if (!this.IsFromTrustProvider(ContractId, e.From))
					return;

				await h(this, new ContractReferenceEventArgs(ContractId));
			}
		}

		/// <summary>
		/// Event raised whenever a contract has been created.
		/// </summary>
		public event ContractReferenceEventHandler ContractCreated = null;

		#endregion

		#region Contract Updated event

		private async Task ContractUpdatedMessageHandler(object Sender, MessageEventArgs e)
		{
			ContractReferenceEventHandler h = this.ContractUpdated;

			if (!(h is null))
			{
				string ContractId = XML.Attribute(e.Content, "contractId");

				if (!this.IsFromTrustProvider(ContractId, e.From))
					return;

				await h(this, new ContractReferenceEventArgs(ContractId));
			}
		}

		/// <summary>
		/// Event raised whenever a contract has been updated.
		/// </summary>
		public event ContractReferenceEventHandler ContractUpdated = null;

		#endregion

		#region Contract Deleted event

		private async Task ContractDeletedMessageHandler(object Sender, MessageEventArgs e)
		{
			ContractReferenceEventHandler h = this.ContractDeleted;

			if (!(h is null))
			{
				string ContractId = XML.Attribute(e.Content, "contractId");

				if (!this.IsFromTrustProvider(ContractId, e.From))
					return;

				await h(this, new ContractReferenceEventArgs(ContractId));
			}
		}

		/// <summary>
		/// Event raised whenever a contract has been deleted.
		/// </summary>
		public event ContractReferenceEventHandler ContractDeleted = null;

		#endregion

		#region Update Contract

		/// <summary>
		/// Updates a contract
		/// </summary>
		/// <param name="Contract">Contract to update.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void UpdateContract(Contract Contract, SmartContractEventHandler Callback, object State)
		{
			this.UpdateContract(this.GetTrustProvider(Contract.ContractId), Contract, Callback, State);
		}

		/// <summary>
		/// Updates a contract
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Contract">Contract to update.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void UpdateContract(string Address, Contract Contract,
			SmartContractEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<updateContract xmlns='");
			Xml.Append(NamespaceSmartContractsCurrent);
			Xml.Append("'>");

			Contract.Serialize(Xml, false, true, true, true, false, false, false);

			Xml.Append("</updateContract>");

			this.client.SendIqSet(Address, Xml.ToString(), this.ContractResponse, new object[] { Callback, State });
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
		public Task<Contract> UpdateContractAsync(string Address, Contract Contract)
		{
			TaskCompletionSource<Contract> Result = new TaskCompletionSource<Contract>();

			this.UpdateContract(Address, Contract, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.Contract);
				else
					Result.SetException(e.StanzaError ?? new Exception("Unable to update the contract."));

				return Task.CompletedTask;

			}, null);

			return Result.Task;
		}

		#endregion

		#region Validate Contract

		/// <summary>
		/// Validates a smart contract.
		/// </summary>
		/// <param name="Contract">Contract to validate</param>
		/// <param name="Callback">Method to call when validation is completed</param>
		/// <param name="State">State object to pass to callback method.</param>
		public Task Validate(Contract Contract, ContractValidationEventHandler Callback, object State)
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
		public async Task Validate(Contract Contract, bool ValidateState, ContractValidationEventHandler Callback, object State)
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
				ContractComponent = Contract.ContractId.Substring(i + 1);

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

				this.GetSchema(ContractComponent, Namespace, SchemaDigest, (_, e) =>
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

						using (TemporaryFile File = P.Value)
						{
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

			await this.GetServerPublicKey(Contract.Provider, async (sender, e) =>
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

		private async Task ReturnStatus(ContractStatus Status, ContractValidationEventHandler Callback, object State)
		{
			if (!(Callback is null))
			{
				try
				{
					await Callback(this, new ContractValidationEventArgs(Status, State));
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}
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

			await this.Validate(Contract, ValidateState, (sender, e) =>
			{
				Result.SetResult(e.Status);
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
		public Task<bool> CanSignAs(CaseInsensitiveString ReferenceId, CaseInsensitiveString SignatoryId)
		{
			string ReferenceDomain = XmppClient.GetDomain(ReferenceId);
			string SignatoryDomain = XmppClient.GetDomain(SignatoryId);

			if (ReferenceDomain != SignatoryDomain)
				return Task.FromResult(false);

			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<canSignAs xmlns='");
			Xml.Append(NamespaceLegalIdentitiesCurrent);
			Xml.Append("' referenceId='");
			Xml.Append(XML.Encode(ReferenceId));
			Xml.Append("' signatoryId='");
			Xml.Append(XML.Encode(SignatoryId));
			Xml.Append("'/>");

			this.client.SendIqGet(ReferenceDomain, Xml.ToString(), (_, e) =>
			{
				Result.TrySetResult(e.Ok);
				return Task.CompletedTask;
			}, null);

			return Result.Task;
		}

		#endregion

		#region SendContractProposal

		/// <summary>
		/// Sends a contract proposal to a recipient.
		/// </summary>
		/// <param name="ContractId">ID of proposed contract.</param>
		/// <param name="Role">Proposed role of recipient.</param>
		/// <param name="To">Recipient Address (Bare or Full JID).</param>
		public void SendContractProposal(string ContractId, string Role, string To)
		{
			this.SendContractProposal(ContractId, Role, To, string.Empty);
		}

		/// <summary>
		/// Sends a contract proposal to a recipient.
		/// </summary>
		/// <param name="ContractId">ID of proposed contract.</param>
		/// <param name="Role">Proposed role of recipient.</param>
		/// <param name="To">Recipient Address (Bare or Full JID).</param>
		/// <param name="Message">Optional message included in message.</param>
		public void SendContractProposal(string ContractId, string Role, string To, string Message)
		{
			this.SendContractProposal(ContractId, Role, To, Message, null, SymmetricCipherAlgorithms.Aes256);
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
		public void SendContractProposal(string ContractId, string Role, string To, string Message, byte[] Key,
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
					this.client.SendMessage(MessageType.Normal, To, Xml.ToString(), string.Empty, string.Empty, string.Empty,
						string.Empty, string.Empty);
				}
				else
				{
					this.localE2eEndpoint.SendMessage(this.client, E2ETransmission.NormalIfNotE2E, QoSLevel.Unacknowledged, MessageType.Normal,
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

				this.localE2eEndpoint.SendMessage(this.client, E2ETransmission.AssertE2E, QoSLevel.Unacknowledged, MessageType.Normal,
					string.Empty, To, Xml.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, null, null);
			}
		}

		private async Task ContractProposalMessageHandler(object Sender, MessageEventArgs e)
		{
			ContractProposalEventHandler h = this.ContractProposalReceived;
			if (!(h is null))
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

				try
				{
					if (!(Key is null))
						await this.SaveContractSharedSecret(ContractId, e.FromBareJID, Key, KeyAlgorithm, true);

					await h(this, new ContractProposalEventArgs(e, ContractId, Role, Message, Key, KeyAlgorithm));
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}
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

			if (Enum.TryParse(Parts[0], out SymmetricCipherAlgorithms Algorithm))
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
		public event ContractProposalEventHandler ContractProposalReceived = null;

		#endregion

		#region Get Schemas

		/// <summary>
		/// Gets available schemas.
		/// </summary>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetSchemas(SchemaReferencesEventHandler Callback, object State)
		{
			this.GetSchemas(this.componentAddress, Callback, State);
		}

		/// <summary>
		/// Gets available schemas.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetSchemas(string Address, SchemaReferencesEventHandler Callback, object State)
		{
			this.client.SendIqGet(Address, "<getSchemas xmlns='" + NamespaceSmartContractsCurrent + "'/>",
				async (sender, e) =>
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

					if (!(Callback is null))
						await Callback(this, new SchemaReferencesEventArgs(e, Schemas.ToArray()));

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
		public Task<SchemaReference[]> GetSchemasAsync(string Address)
		{
			TaskCompletionSource<SchemaReference[]> Result = new TaskCompletionSource<SchemaReference[]>();

			this.GetSchemas(Address, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.References);
				else
					Result.SetException(e.StanzaError ?? new Exception("Unable to get schemas."));

				return Task.CompletedTask;

			}, null);

			return Result.Task;
		}

		#endregion

		#region Get Schema

		/// <summary>
		/// Gets a schema.
		/// </summary>
		/// <param name="Namespace">Namespace of schema to get.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetSchema(string Namespace, SchemaEventHandler Callback, object State)
		{
			this.GetSchema(this.componentAddress, Namespace, null, Callback, State);
		}

		/// <summary>
		/// Gets a schema.
		/// </summary>
		/// <param name="Namespace">Namespace of schema to get.</param>
		/// <param name="Digest">Specifies a specific schema version. If not provided (or null), the most recently recorded schema will be returned.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetSchema(string Namespace, SchemaDigest Digest, SchemaEventHandler Callback, object State)
		{
			this.GetSchema(this.componentAddress, Namespace, Digest, Callback, State);
		}

		/// <summary>
		/// Gets a schema.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Namespace">Namespace of schema to get.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetSchema(string Address, string Namespace, SchemaEventHandler Callback, object State)
		{
			this.GetSchema(Address, Namespace, null, Callback, State);
		}

		/// <summary>
		/// Gets a schema.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Namespace">Namespace of schema to get.</param>
		/// <param name="Digest">Specifies a specific schema version. If not provided (or null), the most recently recorded schema will be returned.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetSchema(string Address, string Namespace, SchemaDigest Digest, SchemaEventHandler Callback, object State)
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

			this.client.SendIqGet(Address, Xml.ToString(),
				async (sender, e) =>
				{
					XmlElement E = e.FirstElement;
					byte[] Schema = null;

					if (e.Ok && !(E is null) && E.LocalName == "schema")
						Schema = Convert.FromBase64String(E.InnerText);
					else
						e.Ok = false;

					if (!(Callback is null))
						await Callback(this, new SchemaEventArgs(e, Schema));

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
		public Task<byte[]> GetSchemaAsync(string Address, string Namespace, SchemaDigest Digest)
		{
			TaskCompletionSource<byte[]> Result = new TaskCompletionSource<byte[]>();

			this.GetSchema(Address, Namespace, Digest, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.Schema);
				else
					Result.SetException(e.StanzaError ?? new Exception("Unable to get schema."));

				return Task.CompletedTask;

			}, null);

			return Result.Task;
		}

		#endregion

		#region Get Legal Identities of a contract

		/// <summary>
		/// Gets available legal identities related to a contract.
		/// </summary>
		/// <param name="ContractId">Get legal identities related to the contract identified by this identity.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetContractLegalIdentities(string ContractId, LegalIdentitiesEventHandler Callback, object State)
		{
			this.GetContractLegalIdentities(this.GetTrustProvider(ContractId), ContractId, false, true, Callback, State);
		}

		/// <summary>
		/// Gets available legal identities related to a contract.
		/// </summary>
		/// <param name="ContractId">Get legal identities related to the contract identified by this identity.</param>
		/// <param name="Current">If current legal identities are to be returned. (Default=false).</param>
		/// <param name="Historic">If legal identities at the time of signature are to be returned. (Default=true).</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetContractLegalIdentities(string ContractId, bool Current, bool Historic, LegalIdentitiesEventHandler Callback, object State)
		{
			this.GetContractLegalIdentities(this.GetTrustProvider(ContractId), ContractId, Current, Historic, Callback, State);
		}

		/// <summary>
		/// Gets available legal identities related to a contract.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ContractId">Get legal identities related to the contract identified by this identity.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetContractLegalIdentities(string Address, string ContractId, LegalIdentitiesEventHandler Callback, object State)
		{
			this.GetContractLegalIdentities(Address, ContractId, false, true, Callback, State);
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
		public void GetContractLegalIdentities(string Address, string ContractId, bool Current, bool Historic, LegalIdentitiesEventHandler Callback, object State)
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

			this.client.SendIqGet(Address, Xml.ToString(), this.IdentitiesResponse, new object[] { Callback, State });
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
		public Task<LegalIdentity[]> GetContractLegalIdentitiesAsync(string Address, string ContractId, bool Current, bool Historic)
		{
			TaskCompletionSource<LegalIdentity[]> Result = new TaskCompletionSource<LegalIdentity[]>();

			this.GetContractLegalIdentities(Address, ContractId, Current, Historic, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.Identities);
				else
					Result.SetException(e.StanzaError ?? new Exception("Unable to get legal identities."));

				return Task.CompletedTask;

			}, null);

			return Result.Task;
		}

		#endregion

		#region Get Network Identities of a contract

		/// <summary>
		/// Gets available network identities related to a contract.
		/// </summary>
		/// <param name="ContractId">Get network identities related to the contract identified by this identity.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetContractNetworkIdentities(string ContractId, NetworkIdentitiesEventHandler Callback, object State)
		{
			this.GetContractNetworkIdentities(this.GetTrustProvider(ContractId), ContractId, Callback, State);
		}

		/// <summary>
		/// Gets available network identities related to a contract.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ContractId">Get network identities related to the contract identified by this identity.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetContractNetworkIdentities(string Address, string ContractId, NetworkIdentitiesEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getNetworkIdentities xmlns='");
			Xml.Append(NamespaceSmartContractsCurrent);
			Xml.Append("' contractId='");
			Xml.Append(XML.Encode(ContractId));
			Xml.Append("'/>");

			this.client.SendIqGet(Address, Xml.ToString(), async (sender, e) =>
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

				if (!(Callback is null))
					await Callback(this, new NetworkIdentitiesEventArgs(e, Identities));
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
		public Task<NetworkIdentity[]> GetContractNetworkIdentitiesAsync(string Address, string ContractId)
		{
			TaskCompletionSource<NetworkIdentity[]> Result = new TaskCompletionSource<NetworkIdentity[]>();

			this.GetContractNetworkIdentities(Address, ContractId, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.Identities);
				else
					Result.SetException(e.StanzaError ?? new Exception("Unable to get network identities."));

				return Task.CompletedTask;

			}, null);

			return Result.Task;
		}

		#endregion

		#region Search Public Contracts

		/// <summary>
		/// Performs a search of public smart contracts.
		/// </summary>
		/// <param name="Filter">Search filters.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void Search(SearchFilter[] Filter, SearchResultEventHandler Callback, object State)
		{
			this.Search(this.componentAddress, 0, int.MaxValue, Filter, Callback, State);
		}

		/// <summary>
		/// Performs a search of public smart contracts.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Filter">Search filters.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void Search(string Address, SearchFilter[] Filter, SearchResultEventHandler Callback, object State)
		{
			this.Search(Address, 0, int.MaxValue, Filter, Callback, State);
		}

		/// <summary>
		/// Performs a search of public smart contracts.
		/// </summary>
		/// <param name="Offset">Result will start with the response at this offset into result set.</param>
		/// <param name="MaxCount">Result will be limited to this number of items.</param>
		/// <param name="Filter">Search filters.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void Search(int Offset, int MaxCount, SearchFilter[] Filter, SearchResultEventHandler Callback, object State)
		{
			this.Search(this.componentAddress, Offset, MaxCount, Filter, Callback, State);
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
		public void Search(string Address, int Offset, int MaxCount, SearchFilter[] Filter, SearchResultEventHandler Callback, object State)
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

			this.client.SendIqGet(Address, Xml.ToString(), async (sender, e) =>
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

				if (!(Callback is null))
					await Callback(this, new SearchResultEventArgs(e, Offset, MaxCount, More, IDs?.ToArray()));
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
		public Task<SearchResultEventArgs> SearchAsync(string Address, int Offset, int MaxCount, SearchFilter[] Filter)
		{
			TaskCompletionSource<SearchResultEventArgs> Result = new TaskCompletionSource<SearchResultEventArgs>();

			this.Search(Address, Offset, MaxCount, Filter, (sender, e) =>
			{
				Result.SetResult(e);
				return Task.CompletedTask;
			}, null);

			return Result.Task;
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
			return this.PetitionIdentityAsync(this.GetTrustProvider(LegalId), LegalId, PetitionId, Purpose, null);
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
			return this.PetitionIdentityAsync(Address, LegalId, PetitionId, Purpose, null);
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
		public async Task PetitionIdentityAsync(string Address, string LegalId, string PetitionId, string Purpose, string ContextXml)
		{
			StringBuilder Xml = new StringBuilder();
			byte[] Nonce = new byte[32];
			this.rnd.GetBytes(Nonce);

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
				Xml.Append(ContextXml);
				Xml.Append("</petitionIdentity>");
			}

			await this.client.IqSetAsync(Address, Xml.ToString());
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
			LegalIdentityPetitionEventHandler h = this.PetitionForIdentityReceived;

			if (!(h is null))
			{
				string LegalId = XML.Attribute(e.Content, "id");
				string PetitionId = XML.Attribute(e.Content, "pid");
				string Purpose = XML.Attribute(e.Content, "purpose");
				string From = XML.Attribute(e.Content, "from");
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

				if (Identity is null)
					return;

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

						try
						{
							await h(this, new LegalIdentityPetitionEventArgs(e, Identity, From, LegalId, PetitionId, Purpose, ClientEndpoint, Context));
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
						}
					}, null);
				}
			}
		}

		/// <summary>
		/// Event raised when someone requests access to one of the legal identities owned by the client.
		/// </summary>
		public event LegalIdentityPetitionEventHandler PetitionForIdentityReceived = null;

		private async Task PetitionIdentityResponseMessageHandler(object Sender, MessageEventArgs e)
		{
			LegalIdentityPetitionResponseEventHandler h = this.PetitionedIdentityResponseReceived;

			if (!(h is null))
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
				{
					try
					{
						await h(this, new LegalIdentityPetitionResponseEventArgs(e, Identity, PetitionId, Response, ClientEndpoint, Context));
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}
				}
			}
		}

		/// <summary>
		/// Event raised when a response to an identity petition has been received by the client.
		/// </summary>
		public event LegalIdentityPetitionResponseEventHandler PetitionedIdentityResponseReceived = null;

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
			return this.PetitionSignatureAsync(this.GetTrustProvider(LegalId), LegalId, Content, PetitionId, Purpose, false, null);
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
			return this.PetitionSignatureAsync(Address, LegalId, Content, PetitionId, Purpose, false, null);
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
			return this.PetitionSignatureAsync(Address, LegalId, Content, PetitionId, Purpose, false, ContextXml);
		}

		private async Task PetitionSignatureAsync(string Address, string LegalId, byte[] Content, string PetitionId,
			string Purpose, bool PeerReview, string ContextXml)
		{
			if (this.contentPerPid.TryGetValue(PetitionId, out KeyValuePair<byte[], bool> Rec))
			{
				if (Convert.ToBase64String(Content) == Convert.ToBase64String(Rec.Key) && PeerReview == Rec.Value)
					return;

				throw new InvalidOperationException("Petition ID must be unique for outstanding petitions.");
			}

			this.contentPerPid[PetitionId] = new KeyValuePair<byte[], bool>(Content, PeerReview);

			StringBuilder Xml = new StringBuilder();
			byte[] Nonce = new byte[32];
			this.rnd.GetBytes(Nonce);

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

			if (string.IsNullOrEmpty(ContextXml))
				Xml.Append(ContentStr);
			else
			{
				Xml.Append("<content>");
				Xml.Append(ContentStr);
				Xml.Append("</content>");
				Xml.Append(ContextXml);
			}

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
			string ContentStr = string.Empty;
			byte[] Content = null;
			LegalIdentity Identity = null;
			XmlElement Context = null;
			bool PeerReview = false;

			foreach (XmlNode N in e.Content.ChildNodes)
			{
				if (!(N is XmlElement E))
					continue;

				switch (E.LocalName)
				{
					case "content":
						ContentStr = E.InnerText;
						Content = Convert.FromBase64String(ContentStr);
						break;

					case "identity":
						Identity = LegalIdentity.Parse(E);
						break;

					default:
						if (!(Context is null))
							return;

						Context = E;
						break;
				}
			}

			if (Content is null)
				return;

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
								TempId["JID"].ToLower() == XmppClient.GetBareJID(From).ToLower())
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

			SignaturePetitionEventHandler h = PeerReview ? this.PetitionForPeerReviewIDReceived : this.PetitionForSignatureReceived;

			if (!(h is null))
			{
				await this.Validate(Identity, false, async (sender2, e2) =>
				{
					if (e2.Status != IdentityStatus.Valid && e2.Status != IdentityStatus.NoProviderSignature)
					{
						this.client.Error("Invalid legal identity received and discarded.");

						Log.Warning("Invalid legal identity received and discarded.", this.client.BareJID, e.From,
							new KeyValuePair<string, object>("Status", e2.Status));

						return;
					}

					try
					{
						await h(this, new SignaturePetitionEventArgs(e, Identity, From, LegalId, PetitionId, Purpose, Content, ClientEndpoint, Context));
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}
				}, null);
			}
		}

		/// <summary>
		/// Event raised when someone requests access to one of the legal identities owned by the client.
		/// </summary>
		public event SignaturePetitionEventHandler PetitionForSignatureReceived = null;

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

			SignaturePetitionResponseEventHandler h = P.Value ? this.PetitionedPeerReviewIDResponseReceived : this.PetitionedSignatureResponseReceived;

			if (!(h is null))
			{
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

						await h(this, new SignaturePetitionResponseEventArgs(e, Identity, PetitionId, Signature, Response, ClientEndpoint, Context));
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}
					finally
					{
						this.contentPerPid.Remove(PetitionId);
					}
				}
				else
				{
					this.client.Warning("Sender invalid. Response ignored.");
					return;
				}
			}
		}

		/// <summary>
		/// Event raised when a response to a signature petition has been received by the client.
		/// </summary>
		public event SignaturePetitionResponseEventHandler PetitionedSignatureResponseReceived = null;

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

			return this.PetitionSignatureAsync(Address, LegalId, Content, PetitionId, Purpose, true, null);
		}

		/// <summary>
		/// Event raised when someone makes a request to one of the legal identities owned by the client, for a peer review of a newly created legal identity.
		/// </summary>
		public event SignaturePetitionEventHandler PetitionForPeerReviewIDReceived = null;

		/// <summary>
		/// Event raised when a response to a ID Peer Review petition has been received by the client.
		/// </summary>
		public event SignaturePetitionResponseEventHandler PetitionedPeerReviewIDResponseReceived = null;

		/// <summary>
		/// Adds an attachment to a legal identity with information about a peer review of the identity.
		/// </summary>
		/// <param name="Identity"></param>
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
			byte[] Nonce = new byte[32];
			this.rnd.GetBytes(Nonce);

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
			ContractPetitionEventHandler h = this.PetitionForContractReceived;

			if (!(h is null))
			{
				string ContractId = XML.Attribute(e.Content, "id");
				string PetitionId = XML.Attribute(e.Content, "pid");
				string Purpose = XML.Attribute(e.Content, "purpose");
				string From = XML.Attribute(e.Content, "from");
				string ClientEndpoint = XML.Attribute(e.Content, "clientEp");
				int i = ContractId.IndexOf('@');
				LegalIdentity Identity = null;
				XmlElement Context = null;

				foreach (XmlNode N in e.Content.ChildNodes)
				{
					if (!(N is XmlElement E))
						continue;

					if (E.LocalName == "identity" && E.NamespaceURI == e.Content.NamespaceURI)
						Identity = LegalIdentity.Parse(E);
					else if (!(Context is null))
						return;
					else
						Context = E;
				}

				if (Identity is null)
					return;

				if (!this.IsFromTrustProvider(ContractId, e.FromBareJID))
					return;

				await this.Validate(Identity, false, async (sender2, e2) =>
				{
					if (e2.Status != IdentityStatus.Valid)
					{
						this.client.Error("Invalid identity received and discarded.");

						Log.Warning("Invalid identity received and discarded.", this.client.BareJID, e.From,
							new KeyValuePair<string, object>("Status", e2.Status));
						return;
					}

					try
					{
						await h(this, new ContractPetitionEventArgs(e, Identity, From, ContractId, PetitionId, Purpose, ClientEndpoint, Context));
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}
				}, null);
			}
		}

		/// <summary>
		/// Event raised when someone requests access to a smart contract to which the client is part.
		/// </summary>
		public event ContractPetitionEventHandler PetitionForContractReceived = null;

		private async Task PetitionContractResponseMessageHandler(object Sender, MessageEventArgs e)
		{
			ContractPetitionResponseEventHandler h = this.PetitionedContractResponseReceived;

			if (!(h is null))
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
				{
					try
					{
						await h(this, new ContractPetitionResponseEventArgs(e, Contract, PetitionId, Response, ClientEndpoint, Context));
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}
				}
			}
		}

		/// <summary>
		/// Event raised when a response to a contract petition has been received by the client.
		/// </summary>
		public event ContractPetitionResponseEventHandler PetitionedContractResponseReceived = null;

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
		public void AddLegalIdAttachment(string LegalId, string GetUrl, byte[] Signature, LegalIdentityEventHandler Callback, object State)
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

			this.client.SendIqSet(this.componentAddress, Xml.ToString(), async (sender, e) =>
			{
				LegalIdentity Identity = null;
				XmlElement E;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "identity")
					Identity = LegalIdentity.Parse(E);
				else
					e.Ok = false;

				if (!(Callback is null))
					await Callback(this, new LegalIdentityEventArgs(e, Identity));
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
		public Task<LegalIdentity> AddLegalIdAttachmentAsync(string LegalId, string GetUrl, byte[] Signature)
		{
			TaskCompletionSource<LegalIdentity> Result = new TaskCompletionSource<LegalIdentity>();

			this.AddLegalIdAttachment(LegalId, GetUrl, Signature, (sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Identity);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to add attachment."));

				return Task.CompletedTask;

			}, null);

			return Result.Task;
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
		public void AddContractAttachment(string ContractId, string GetUrl, byte[] Signature, SmartContractEventHandler Callback, object State)
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

			this.client.SendIqSet(this.componentAddress, Xml.ToString(), async (sender, e) =>
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

				if (!(Callback is null))
					await Callback(this, new SmartContractEventArgs(e, Contract));
			}, State);
		}

		/// <summary>
		/// Adds an attachment to a proposed or approved contract before it is being signed.
		/// </summary>
		/// <param name="ContractId">ID of Smart Contract.</param>
		/// <param name="GetUrl">The GET URL of the attachment to associate with the smart contract.
		/// The URL might previously have been provided by the HTTP Upload Service.</param>
		/// <param name="Signature">Signature of the content of the attachment, made by an approved legal identity of the sender.</param>
		public Task<Contract> AddContractAttachmentAsync(string ContractId, string GetUrl, byte[] Signature)
		{
			TaskCompletionSource<Contract> Result = new TaskCompletionSource<Contract>();

			this.AddContractAttachment(ContractId, GetUrl, Signature, (sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Contract);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to add attachment."));

				return Task.CompletedTask;

			}, null);

			return Result.Task;
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
			using (HttpClient HttpClient = new HttpClient()
			{
				Timeout = TimeSpan.FromMilliseconds(Timeout)
			})
			{
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
						await Content.Getters.WebGetter.ProcessResponse(Response, Request.RequestUri);

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
		}

		/// <summary>
		/// Removes an attachment from a newly created legal identity.
		/// </summary>
		/// <param name="AttachmentId">ID of Attachment.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void RemoveLegalIdAttachment(string AttachmentId, LegalIdentityEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<removeAttachment xmlns='");
			Xml.Append(NamespaceLegalIdentitiesCurrent);
			Xml.Append("' attachmentId='");
			Xml.Append(XML.Encode(AttachmentId));
			Xml.Append("'/>");

			this.client.SendIqSet(this.componentAddress, Xml.ToString(), async (sender, e) =>
			{
				LegalIdentity Identity = null;
				XmlElement E;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "identity")
					Identity = LegalIdentity.Parse(E);
				else
					e.Ok = false;

				if (!(Callback is null))
					await Callback(this, new LegalIdentityEventArgs(e, Identity));
			}, State);
		}

		/// <summary>
		/// Removes an attachment from a newly created legal identity.
		/// </summary>
		/// <param name="AttachmentId">ID of Attachment.</param>
		public Task<LegalIdentity> RemoveLegalIdAttachmentAsync(string AttachmentId)
		{
			TaskCompletionSource<LegalIdentity> Result = new TaskCompletionSource<LegalIdentity>();

			this.RemoveLegalIdAttachment(AttachmentId, (sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Identity);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to remove attachment."));

				return Task.CompletedTask;

			}, null);

			return Result.Task;
		}

		/// <summary>
		/// Removes an attachment from a proposed or approved contract before it is being signed.
		/// </summary>
		/// <param name="AttachmentId">ID of Attachment.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void RemoveContractAttachment(string AttachmentId, SmartContractEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<removeAttachment xmlns='");
			Xml.Append(NamespaceSmartContractsCurrent);
			Xml.Append("' attachmentId='");
			Xml.Append(XML.Encode(AttachmentId));
			Xml.Append("'/>");

			this.client.SendIqSet(this.componentAddress, Xml.ToString(), async (sender, e) =>
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

				if (!(Callback is null))
					await Callback(this, new SmartContractEventArgs(e, Contract));
			}, State);
		}

		/// <summary>
		/// Removes an attachment from a proposed or approved contract before it is being signed.
		/// </summary>
		/// <param name="AttachmentId">ID of Attachment.</param>
		public Task<Contract> RemoveContractAttachmentAsync(string AttachmentId)
		{
			TaskCompletionSource<Contract> Result = new TaskCompletionSource<Contract>();

			this.RemoveContractAttachment(AttachmentId, (sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Contract);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to remove attachment."));

				return Task.CompletedTask;

			}, null);

			return Result.Task;
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
				using (ICryptoTransform Aes = this.aes.CreateEncryptor(Key, IV))
				{
					Encrypted = Aes.TransformFinalBlock(ToEncrypt, 0, c);
				}
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
				using (ICryptoTransform Aes = this.aes.CreateDecryptor(Key, IV))
				{
					Decrypted = Aes.TransformFinalBlock(EncryptedMessage, 0, EncryptedMessage.Length);
				}
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
		public void AuthorizeAccessToId(string LegalId, string RemoteId, bool Authorized, IqResultEventHandlerAsync Callback, object State)
		{
			this.AuthorizeAccessToId(this.GetTrustProvider(LegalId), LegalId, RemoteId, Authorized, Callback, State);
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
		public void AuthorizeAccessToId(string Address, string LegalId, string RemoteId, bool Authorized, IqResultEventHandlerAsync Callback, object State)
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

			this.client.SendIqSet(Address, Xml.ToString(), Callback, State);
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

			this.AuthorizeAccessToId(Address, LegalId, RemoteId, Authorized, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(true);
				else
					Result.SetException(e.StanzaError ?? new Exception("Unable to authorize access to legal identity."));

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
		public void AuthorizeAccessToContract(string ContractId, string RemoteId, bool Authorized, IqResultEventHandlerAsync Callback, object State)
		{
			this.AuthorizeAccessToContract(this.GetTrustProvider(ContractId), ContractId, RemoteId, Authorized, Callback, State);
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
		public void AuthorizeAccessToContract(string Address, string ContractId, string RemoteId, bool Authorized, IqResultEventHandlerAsync Callback, object State)
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

			this.client.SendIqSet(Address, Xml.ToString(), Callback, State);
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

			this.AuthorizeAccessToContract(Address, ContractId, RemoteId, Authorized, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(true);
				else
					Result.SetException(e.StanzaError ?? new Exception("Unable to authorize access to legal identity."));

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
		public void GetPeerReviewIdServiceProviders(ServiceProvidersEventHandler<ServiceProviderWithLegalId> Callback, object State)
		{
			this.GetPeerReviewIdServiceProviders(this.componentAddress, Callback, State);
		}

		/// <summary>
		/// Gets available service providers who can help review an ID application.
		/// </summary>
		/// <param name="ComponentAddress">Address of component.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetPeerReviewIdServiceProviders(string ComponentAddress,
			ServiceProvidersEventHandler<ServiceProviderWithLegalId> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<reviewIdProviders xmlns='");
			Xml.Append(NamespaceLegalIdentitiesCurrent);
			Xml.Append("'/>");

			this.client.SendIqGet(ComponentAddress, Xml.ToString(), async (sender, e) =>
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

				if (!(Callback is null))
				{
					try
					{
						await Callback(this, new ServiceProvidersEventArgs<ServiceProviderWithLegalId>(e, Providers?.ToArray()));
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}
				}

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
		public Task<ServiceProviderWithLegalId[]> GetPeerReviewIdServiceProvidersAsync(string ComponentAddress)
		{
			TaskCompletionSource<ServiceProviderWithLegalId[]> Providers = new TaskCompletionSource<ServiceProviderWithLegalId[]>();

			this.GetPeerReviewIdServiceProviders(ComponentAddress, (sender, e) =>
			{
				if (e.Ok)
					Providers.TrySetResult(e.ServiceProviders);
				else
					Providers.TrySetException(e.StanzaError ?? new Exception("Unable to get service providers."));

				return Task.CompletedTask;

			}, null);

			return Providers.Task;
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
		public void SelectPeerReviewService(string Provider, string ServiceId, IqResultEventHandlerAsync Callback, object State)
		{
			this.SelectPeerReviewService(this.componentAddress, Provider, ServiceId, Callback, State);
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
		public void SelectPeerReviewService(string ComponentAddress, string Provider, string ServiceId,
			IqResultEventHandlerAsync Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<selectReviewService xmlns='");
			Xml.Append(NamespaceLegalIdentitiesCurrent);
			Xml.Append("' provider='");
			Xml.Append(XML.Encode(Provider));
			Xml.Append("' serviceId='");
			Xml.Append(XML.Encode(ServiceId));
			Xml.Append("'/>");

			this.client.SendIqSet(ComponentAddress, Xml.ToString(), Callback, State);
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
		public Task SelectPeerReviewServiceAsync(string ComponentAddress, string Provider, string ServiceId)
		{
			TaskCompletionSource<bool> Providers = new TaskCompletionSource<bool>();

			this.SelectPeerReviewService(ComponentAddress, Provider, ServiceId, (sender, e) =>
			{
				if (e.Ok)
					Providers.TrySetResult(true);
				else
					Providers.TrySetException(e.StanzaError ?? new Exception("Unable to select peer review service."));

				return Task.CompletedTask;

			}, null);

			return Providers.Task;
		}

		#endregion

		#region Petition Client URL event

		private async Task PetitionClientUrlEventHandler(object Sender, MessageEventArgs e)
		{
			string PetitionId = XML.Attribute(e.Content, "pid");
			string Url = XML.Attribute(e.Content, "url");

			PetitionClientUrlEventHandler h = this.PetitionClientUrlReceived;

			if (!(h is null))
			{
				try
				{
					await h(this, new PetitionClientUrlEventArgs(e, PetitionId, Url));
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}
		}

		/// <summary>
		/// Event raised when a Client URL has been sent to the client as part of a
		/// petition process. Such an URL must be opened by the client to complete 
		/// the petition.
		/// </summary>
		public event PetitionClientUrlEventHandler PetitionClientUrlReceived;

		#endregion
	}
}
