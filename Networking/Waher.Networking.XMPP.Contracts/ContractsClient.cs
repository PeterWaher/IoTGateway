using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP.Contracts.HumanReadable;
using Waher.Networking.XMPP.Contracts.Search;
using Waher.Networking.XMPP.P2P;
using Waher.Networking.XMPP.P2P.E2E;
using Waher.Runtime.Settings;
using Waher.Security;
using Waher.Security.EllipticCurves;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Adds support for legal identities, smart contracts and signatures to an XMPP client.
	/// 
	/// The interface is defined in the IEEE XMPP IoT extensions:
	/// https://gitlab.com/IEEE-SA/XMPPI/IoT
	/// </summary>
	public class ContractsClient : XmppExtension
	{
		/// <summary>
		/// urn:ieee:iot:leg:id:1.0
		/// </summary>
		public const string NamespaceLegalIdentities = "urn:ieee:iot:leg:id:1.0";

		/// <summary>
		/// urn:ieee:iot:leg:sc:1.0
		/// </summary>
		public const string NamespaceSmartContracts = "urn:ieee:iot:leg:sc:1.0";

		private readonly Dictionary<string, KeyEventArgs> publicKeys = new Dictionary<string, KeyEventArgs>();
		private readonly Dictionary<string, KeyEventArgs> matchingKeys = new Dictionary<string, KeyEventArgs>();
		private EndpointSecurity localEndpoint;
		private readonly string componentAddress;

		/// <summary>
		/// Adds support for legal identities, smart contracts and signatures to an XMPP client.
		/// 
		/// The interface is defined in the IEEE XMPP IoT extensions:
		/// https://gitlab.com/IEEE-SA/XMPPI/IoT
		/// </summary>
		/// <param name="Client">XMPP Client to use.</param>
		/// <param name="ComponentAddress">Address to the contracts component.</param>
		public ContractsClient(XmppClient Client, string ComponentAddress)
			: base(Client)
		{
			this.componentAddress = ComponentAddress;
			this.localEndpoint = null;
			Task T = this.LoadKeys();

			this.client.RegisterMessageHandler("identity", NamespaceLegalIdentities, this.IdentityMessageHandler, true);
			this.client.RegisterMessageHandler("contractSigned", NamespaceSmartContracts, this.ContractSignedMessageHandler, true);
			this.client.RegisterMessageHandler("contractUpdated", NamespaceSmartContracts, this.ContractUpdatedMessageHandler, false);
			this.client.RegisterMessageHandler("contractDeleted", NamespaceSmartContracts, this.ContractDeletedMessageHandler, false);
		}

		private async Task<EndpointSecurity> LoadKeys()
		{
			List<IE2eEndpoint> Keys = new List<IE2eEndpoint>();
			string Name = typeof(ContractsClient).FullName;

			foreach (EcAes256 Curve in EndpointSecurity.CreateEndpoints(256, 192, int.MaxValue,
				typeof(EcAes256)))
			{
				string d = await RuntimeSettings.GetAsync(Name + "." + Curve.LocalName, string.Empty);
                byte[] Key;

                try
                {
                    if (string.IsNullOrEmpty(d))
                        Key = null;
                    else
                        Key = Convert.FromBase64String(d);
                }
                catch (Exception)
                {
                    Key = null;
                }

				if (Key is null)
				{
					Key = this.GetKey(Curve.Curve);
					await RuntimeSettings.SetAsync(Name + "." + Curve.LocalName, Convert.ToBase64String(Key));
					Keys.Add(Curve);
				}
				else
				{
					Keys.Add(Curve.CreatePrivate(Key));
					Curve.Dispose();
				}
			}

			this.localEndpoint = new EndpointSecurity(null, 128, Keys.ToArray());

			return this.localEndpoint;
		}

		private byte[] GetKey(EllipticCurve Curve)
		{
			string s = Curve.Export();
			XmlDocument Doc = new XmlDocument();
			Doc.LoadXml(s);
			s = Doc.DocumentElement.GetAttribute("d");
			return Convert.FromBase64String(s);
		}

		/// <summary>
		/// Disposes of the extension.
		/// </summary>
		public override void Dispose()
		{
			this.client.UnregisterMessageHandler("identity", NamespaceLegalIdentities, this.IdentityMessageHandler, true);
			this.client.UnregisterMessageHandler("contractSigned", NamespaceSmartContracts, this.ContractSignedMessageHandler, true);
			this.client.UnregisterMessageHandler("contractUpdated", NamespaceSmartContracts, this.ContractUpdatedMessageHandler, false);
			this.client.UnregisterMessageHandler("contractDeleted", NamespaceSmartContracts, this.ContractDeletedMessageHandler, false);

			this.localEndpoint?.Dispose();
			this.localEndpoint = null;

			base.Dispose();
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[] { };

		#region Server Public Keys

		/// <summary>
		/// Gets the server public key.
		/// </summary>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public void GetServerPublicKey(KeyEventHandler Callback, object State)
		{
			this.GetServerPublicKey(this.componentAddress, Callback, State);
		}

		/// <summary>
		/// Gets the server public key.
		/// </summary>
		/// <param name="Address">Address of entity whose public key is requested.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public void GetServerPublicKey(string Address, KeyEventHandler Callback, object State)
		{
			KeyEventArgs e0;

			lock (this.publicKeys)
			{
				if (!this.publicKeys.TryGetValue(Address, out e0))
					e0 = null;
			}

			if (e0 != null)
			{
				try
				{
					e0 = new KeyEventArgs(e0, e0.Key)
					{
						State = State
					};

					Callback?.Invoke(this, e0);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
			else
			{
				this.client.SendIqGet(Address, "<getPublicKey xmlns=\"" + NamespaceLegalIdentities + "\"/>", (sender, e) =>
				{
					IE2eEndpoint ServerKey = null;
					XmlElement E;

					if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "publicKey" && E.NamespaceURI == NamespaceLegalIdentities)
					{
						foreach (XmlNode N in E.ChildNodes)
						{
							if (N is XmlElement E2)
							{
								ServerKey = EndpointSecurity.ParseE2eKey(E2);
								if (ServerKey != null)
									break;
							}
						}

						e.Ok = ServerKey != null;
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

					Callback?.Invoke(this, e0);
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
		public Task<IE2eEndpoint> GetServerPublicKeyAsync(string Address)
		{
			TaskCompletionSource<IE2eEndpoint> Result = new TaskCompletionSource<IE2eEndpoint>();

			this.GetServerPublicKey(Address, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.Key);
				else
				{
					Result.SetException(new IOException(string.IsNullOrEmpty(e.ErrorText) ?
						"Unable to get public key." : e.ErrorText));
				}
			}, null);

			return Result.Task;
		}

		#endregion

		#region Matching Local Keys

		/// <summary>
		/// Get the local key that matches the server key.
		/// </summary>
		/// <param name="Callback">Method called when response is available.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetMatchingLocalKey(KeyEventHandler Callback, object State)
		{
			this.GetMatchingLocalKey(this.componentAddress, Callback, State);
		}

		/// <summary>
		/// Get the local key that matches a given server key.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Callback">Method called when response is available.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetMatchingLocalKey(string Address, KeyEventHandler Callback, object State)
		{
			KeyEventArgs e0;

			lock (this.matchingKeys)
			{
				if (!this.matchingKeys.TryGetValue(Address, out e0))
					e0 = null;
			}

			if (e0 != null)
			{
				try
				{
					e0 = new KeyEventArgs(e0, e0.Key)
					{
						State = State
					};

					Callback?.Invoke(this, e0);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
			else
			{
				this.GetServerPublicKey(Address, (sender, e) =>
				{
					IE2eEndpoint LocalKey = null;

					if (e.Ok)
					{
						LocalKey = this.localEndpoint.GetLocalKey(e.Key);
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

					Callback?.Invoke(this, e0);

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
		public Task<IE2eEndpoint> GetMatchingLocalKeyAsync(string Address)
		{
			TaskCompletionSource<IE2eEndpoint> Result = new TaskCompletionSource<IE2eEndpoint>();

			this.GetMatchingLocalKey(Address, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.Key);
				else
				{
					Result.SetException(new IOException(string.IsNullOrEmpty(e.ErrorText) ?
						"Unable to get matching local key." : e.ErrorText));
				}
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
		public void Apply(Property[] Properties, LegalIdentityEventHandler Callback,
			object State)
		{
			this.Apply(this.componentAddress, Properties, Callback, State);
		}

		/// <summary>
		/// Applies for a legal identity to be registered.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Properties">Properties of the legal identity.</param>
		/// <param name="Callback">Method to call when registration response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void Apply(string Address, Property[] Properties,
			LegalIdentityEventHandler Callback, object State)
		{
			this.GetMatchingLocalKey(Address, (sender, e) =>
			{
				if (e.Ok)
				{
					StringBuilder Xml = new StringBuilder();

					Xml.Append("<apply xmlns=\"");
					Xml.Append(NamespaceLegalIdentities);
					Xml.Append("\">");

					StringBuilder Identity = new StringBuilder();

					Identity.Append("<identity><clientPublicKey>");
					e.Key.ToXml(Identity, NamespaceLegalIdentities);
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

					this.client.SendIqSet(Address, Xml.ToString(), (sender2, e2) =>
					{
						LegalIdentity Identity2 = null;
						XmlElement E;

						if (e2.Ok && (E = e2.FirstElement) != null &&
							E.LocalName == "identity" &&
							E.NamespaceURI == NamespaceLegalIdentities)
						{
							Identity2 = LegalIdentity.Parse(E);
						}
						else
							e2.Ok = false;

						Callback?.Invoke(this, new LegalIdentityEventArgs(e2, Identity2));
					}, e.State);
				}
				else
					Callback?.Invoke(this, new LegalIdentityEventArgs(e, null));
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
		public Task<LegalIdentity> ApplyAsync(string Address, Property[] Properties)
		{
			TaskCompletionSource<LegalIdentity> Result = new TaskCompletionSource<LegalIdentity>();

			this.Apply(Address, Properties, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.Identity);
				else
				{
					Result.SetException(new IOException(string.IsNullOrEmpty(e.ErrorText) ?
						"Unable to apply for a legal identity to be registered." : e.ErrorText));
				}
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
		public void Validate(LegalIdentity Identity, IdentityValidationEventHandler Callback, object State)
		{
			this.Validate(Identity, true, Callback, State);
		}

		/// <summary>
		/// Validates a legal identity.
		/// </summary>
		/// <param name="Identity">Legal identity to validate</param>
		/// <param name="ValidateState">If the state attribute should be validated. (Default=true)</param>
		/// <param name="Callback">Method to call when validation is completed</param>
		/// <param name="State">State object to pass to callback method.</param>
		public void Validate(LegalIdentity Identity, bool ValidateState, IdentityValidationEventHandler Callback, object State)
		{
			if (Identity is null)
			{
				this.ReturnStatus(IdentityStatus.IdentityUndefined, Callback, State);
				return;
			}

			if (ValidateState && Identity.State != IdentityState.Approved)
			{
				this.ReturnStatus(IdentityStatus.NotApproved, Callback, State);
				return;
			}

			DateTime Now = DateTime.Now;

			if (Now < Identity.From)
			{
				this.ReturnStatus(IdentityStatus.NotValidYet, Callback, State);
				return;
			}

			if (Now.Date > Identity.To)
			{
				this.ReturnStatus(IdentityStatus.NotValidAnymore, Callback, State);
				return;
			}

			if (string.IsNullOrEmpty(Identity.Provider))
			{
				this.ReturnStatus(IdentityStatus.NoTrustProvider, Callback, State);
				return;
			}

			if (string.IsNullOrEmpty(Identity.ClientKeyName) ||
				Identity.ClientPubKey is null || Identity.ClientPubKey.Length == 0)
			{
				this.ReturnStatus(IdentityStatus.NoClientPublicKey, Callback, State);
				return;
			}

			if (Identity.ClientSignature is null || Identity.ClientSignature.Length == 0)
			{
				this.ReturnStatus(IdentityStatus.NoClientSignature, Callback, State);
				return;
			}

			StringBuilder Xml = new StringBuilder();
			Identity.Serialize(Xml, false, false, false, false, false);
			byte[] Data = Encoding.UTF8.GetBytes(Xml.ToString());

			if (Identity.ClientKeyName.StartsWith("RSA") &&
				int.TryParse(Identity.ClientKeyName.Substring(3), out int KeySize))
			{
				if (!RsaAes.Verify(Data, Identity.ClientSignature, KeySize, Identity.ClientPubKey))
				{
					this.ReturnStatus(IdentityStatus.ClientSignatureInvalid, Callback, State);
					return;
				}
			}
			else if (EndpointSecurity.TryGetEndpoint(Identity.ClientKeyName,
				EndpointSecurity.IoTHarmonizationE2E, out IE2eEndpoint LocalKey) &&
				LocalKey is EcAes256 LocalEc)
			{
				if (!LocalEc.Verify(Data, Identity.ClientPubKey, Identity.ClientSignature))
				{
					this.ReturnStatus(IdentityStatus.ClientSignatureInvalid, Callback, State);
					return;
				}
			}
			else
			{
				this.ReturnStatus(IdentityStatus.ClientKeyNotRecognized, Callback, State);
				return;
			}

			if (Identity.ServerSignature is null || Identity.ServerSignature.Length == 0)
			{
				this.ReturnStatus(IdentityStatus.NoProviderSignature, Callback, State);
				return;
			}

			Xml.Clear();
			Identity.Serialize(Xml, false, true, true, true, false);
			Data = Encoding.UTF8.GetBytes(Xml.ToString());

			bool HasOldPublicKey;

			lock (this.publicKeys)
			{
				HasOldPublicKey = this.publicKeys.ContainsKey(Identity.Provider);
			}

			this.GetServerPublicKey(Identity.Provider, (sender, e) =>
			{
				if (e.Ok && e.Key != null)
				{
					bool Valid = e.Key.Verify(Data, Identity.ServerSignature);

					if (Valid)
					{
						this.ReturnStatus(IdentityStatus.Valid, Callback, State);
						return;
					}

					if (!HasOldPublicKey)
					{
						this.ReturnStatus(IdentityStatus.ProviderSignatureInvalid, Callback, State);
						return;
					}

					lock (this.publicKeys)
					{
						this.publicKeys.Remove(Identity.Provider);
					}

					this.GetServerPublicKey(Identity.Provider, (sender2, e2) =>
					{
						if (e2.Ok && e2.Key != null)
						{
							if (e.Key.Equals(e2.Key))
							{
								this.ReturnStatus(IdentityStatus.ProviderSignatureInvalid, Callback, State);
								return;
							}

							Valid = e2.Key.Verify(Data, Identity.ServerSignature);

							if (Valid)
							{
								this.ReturnStatus(IdentityStatus.Valid, Callback, State);
								return;
							}
							else
							{
								this.ReturnStatus(IdentityStatus.ProviderSignatureInvalid, Callback, State);
								return;
							}
						}
						else
							this.ReturnStatus(IdentityStatus.NoProviderPublicKey, Callback, State);

					}, State);

					{
						this.ReturnStatus(IdentityStatus.ProviderSignatureInvalid, Callback, State);
						return;
					}
				}
				else
					this.ReturnStatus(IdentityStatus.NoProviderPublicKey, Callback, State);

			}, State);
		}

		private void ReturnStatus(IdentityStatus Status,
			IdentityValidationEventHandler Callback, object State)
		{
			try
			{
				Callback?.Invoke(this, new IdentityValidationEventArgs(Status, State));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
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
		public Task<IdentityStatus> ValidateAsync(LegalIdentity Identity, bool ValidateState)
		{
			TaskCompletionSource<IdentityStatus> Result = new TaskCompletionSource<IdentityStatus>();

			this.Validate(Identity, ValidateState, (sender, e) =>
			{
				Result.SetResult(e.Status);
			}, null);

			return Result.Task;
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

		private void IdentityMessageHandler(object Sender, MessageEventArgs e)
		{
			LegalIdentityEventHandler h = this.IdentityUpdated;

			if (h != null)
			{
				LegalIdentity Identity = LegalIdentity.Parse(e.Content);

				if (!this.IsFromTrustProvider(Identity.Id, e.From))
					return;

				if (string.Compare(e.FromBareJID, Identity.Provider, true) == 0)
				{
					this.Validate(Identity, false, (sender2, e2) =>
					{
						if (e2.Status != IdentityStatus.Valid)
						{
							Client.Error("Invalid legal identity received and discarded.");

							Log.Warning("Invalid legal identity received and discarded.", this.client.BareJID, e.From,
								new KeyValuePair<string, object>("Status", e2.Status));
							return;
						}

						try
						{
							IdentityUpdated?.Invoke(this, new LegalIdentityEventArgs(new IqResultEventArgs(e.Message, e.Id, e.To, e.From, e.Ok, null), Identity));
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}
					}, null);
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
			this.client.SendIqGet(Address, "<getLegalIdentities xmlns=\"" + NamespaceLegalIdentities + "\"/>",
				this.IdentitiesResponse, new object[] { Callback, State });
		}

		private void IdentitiesResponse(object Sender, IqResultEventArgs e)
		{
			object[] P = (object[])e.State;
			LegalIdentitiesEventHandler Callback = (LegalIdentitiesEventHandler)P[0];
			LegalIdentity[] Identities = null;
			XmlElement E;

			if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "identities" && E.NamespaceURI == NamespaceLegalIdentities)
			{
				List<LegalIdentity> IdentitiesList = new List<LegalIdentity>();

				foreach (XmlNode N in E.ChildNodes)
				{
					if (N is XmlElement E2 &&
						E2.LocalName == "identity" &&
						E2.NamespaceURI == E.NamespaceURI)
					{
						IdentitiesList.Add(LegalIdentity.Parse(E2));
					}
				}

				Identities = IdentitiesList.ToArray();
			}
			else
				e.Ok = false;

			e.State = P[1];
			Callback?.Invoke(this, new LegalIdentitiesEventArgs(e, Identities));
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
				{
					Result.SetException(new IOException(string.IsNullOrEmpty(e.ErrorText) ?
						"Unable to get legal identities." : e.ErrorText));
				}
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
			this.GetLegalIdentity(this.componentAddress, LegalIdentityId, Callback, State);
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
				NamespaceLegalIdentities + "\"/>", (sender, e) =>
			{
				LegalIdentity Identity = null;
				XmlElement E;

				if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "identity" && E.NamespaceURI == NamespaceLegalIdentities)
					Identity = LegalIdentity.Parse(E);
				else
					e.Ok = false;

				Callback?.Invoke(this, new LegalIdentityEventArgs(e, Identity));
			}, State);
		}

		/// <summary>
		/// Gets legal identity registered with the account.
		/// </summary>
		/// <param name="LegalIdentityId">ID of the legal identity to get.</param>
		/// <returns>Legal identity object corresponding to <paramref name="LegalIdentityId"/>.</returns>
		public Task<LegalIdentity> GetLegalIdentityAsync(string LegalIdentityId)
		{
			return this.GetLegalIdentityAsync(this.componentAddress, LegalIdentityId);
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
				{
					Result.SetException(new IOException(string.IsNullOrEmpty(e.ErrorText) ?
						"Unable to get legal identity." : e.ErrorText));
				}
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
			this.ObsoleteLegalIdentity(this.componentAddress, LegalIdentityId, Callback, State);
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
			this.client.SendIqSet(Address, "<obsoleteLegalIdentity id=\"" + XML.Encode(LegalIdentityId) + "\" xmlns=\"" +
				NamespaceLegalIdentities + "\"/>", (sender, e) =>
				{
					LegalIdentity Identity = null;
					XmlElement E;

					if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "identity" && E.NamespaceURI == NamespaceLegalIdentities)
						Identity = LegalIdentity.Parse(E);
					else
						e.Ok = false;

					Callback?.Invoke(this, new LegalIdentityEventArgs(e, Identity));
				}, State);
		}

		/// <summary>
		/// Obsoletes one of the legal identities of the account, given its ID.
		/// </summary>
		/// <param name="LegalIdentityId">ID of the legal identity to obsolete.</param>
		/// <returns>Legal identity object corresponding to <paramref name="LegalIdentityId"/>.</returns>
		public Task<LegalIdentity> ObsoleteLegalIdentityAsync(string LegalIdentityId)
		{
			return this.ObsoleteLegalIdentityAsync(this.componentAddress, LegalIdentityId);
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
				{
					Result.SetException(new IOException(string.IsNullOrEmpty(e.ErrorText) ?
						"Unable to obsolete legal identity." : e.ErrorText));
				}
			}, null);

			return Result.Task;
		}

		#endregion

		#region Compromized Legal Identity

		/// <summary>
		/// Reports as Compromized one of the legal identities of the account, given its ID.
		/// </summary>
		/// <param name="LegalIdentityId">ID of the legal identity to compromize.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public void CompromizedLegalIdentity(string LegalIdentityId, LegalIdentityEventHandler Callback, object State)
		{
			this.CompromizedLegalIdentity(this.componentAddress, LegalIdentityId, Callback, State);
		}

		/// <summary>
		/// Reports as Compromized one of the legal identities of the account, given its ID.
		/// </summary>
		/// <param name="Address">Address of entity on which the legal identity are registered.</param>
		/// <param name="LegalIdentityId">ID of the legal identity to compromize.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public void CompromizedLegalIdentity(string Address, string LegalIdentityId, LegalIdentityEventHandler Callback, object State)
		{
			this.client.SendIqSet(Address, "<compromizedLegalIdentity id=\"" + XML.Encode(LegalIdentityId) + "\" xmlns=\"" +
				NamespaceLegalIdentities + "\"/>", (sender, e) =>
				{
					LegalIdentity Identity = null;
					XmlElement E;

					if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "identity" && E.NamespaceURI == NamespaceLegalIdentities)
						Identity = LegalIdentity.Parse(E);
					else
						e.Ok = false;

					Callback?.Invoke(this, new LegalIdentityEventArgs(e, Identity));
				}, State);
		}

		/// <summary>
		/// Reports as Compromized one of the legal identities of the account, given its ID.
		/// </summary>
		/// <param name="LegalIdentityId">ID of the legal identity to compromize.</param>
		/// <returns>Legal identity object corresponding to <paramref name="LegalIdentityId"/>.</returns>
		public Task<LegalIdentity> CompromizedLegalIdentityAsync(string LegalIdentityId)
		{
			return this.CompromizedLegalIdentityAsync(this.componentAddress, LegalIdentityId);
		}

		/// <summary>
		/// Reports as Compromized one of the legal identities of the account, given its ID.
		/// </summary>
		/// <param name="Address">Address of entity on which the legal identity are registered.</param>
		/// <param name="LegalIdentityId">ID of the legal identity to compromize.</param>
		/// <returns>Legal identity object corresponding to <paramref name="LegalIdentityId"/>.</returns>
		public Task<LegalIdentity> CompromizedLegalIdentityAsync(string Address, string LegalIdentityId)
		{
			TaskCompletionSource<LegalIdentity> Result = new TaskCompletionSource<LegalIdentity>();

			this.CompromizedLegalIdentity(Address, LegalIdentityId, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.Identity);
				else
				{
					Result.SetException(new IOException(string.IsNullOrEmpty(e.ErrorText) ?
						"Unable to compromize legal identity." : e.ErrorText));
				}
			}, null);

			return Result.Task;
		}

		#endregion

		#region Signatures

		/// <summary>
		/// Signs binary data with the corresponding private key.
		/// </summary>
		/// <param name="Data">Binary data to sign-</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public void Sign(byte[] Data, SignatureEventHandler Callback, object State)
		{
			this.Sign(this.componentAddress, Data, Callback, State);
		}

		/// <summary>
		/// Signs binary data with the corresponding private key.
		/// </summary>
		/// <param name="Address">Address of entity on which the legal identity are registered.</param>
		/// <param name="Data">Binary data to sign-</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public void Sign(string Address, byte[] Data, SignatureEventHandler Callback, object State)
		{
			this.GetMatchingLocalKey(Address, (sender, e) =>
			{
				byte[] Signature = null;

				if (e.Ok)
					Signature = e.Key.Sign(Data);

				Callback?.Invoke(this, new SignatureEventArgs(e, Signature));

			}, State);
		}

		/// <summary>
		/// Signs binary data with the corresponding private key.
		/// </summary>
		/// <param name="Data">Binary data to sign-</param>
		/// <returns>Digital signature.</returns>
		public Task<byte[]> SignAsync(byte[] Data)
		{
			return this.SignAsync(this.componentAddress, Data);
		}

		/// <summary>
		/// Signs binary data with the corresponding private key.
		/// </summary>
		/// <param name="Address">Address of entity on which the legal identity are registered.</param>
		/// <param name="Data">Binary data to sign-</param>
		/// <returns>Digital signature.</returns>
		public Task<byte[]> SignAsync(string Address, byte[] Data)
		{
			TaskCompletionSource<byte[]> Result = new TaskCompletionSource<byte[]>();

			this.Sign(Address, Data, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.Signature);
				else
					Result.SetException(new IOException(string.IsNullOrEmpty(e.ErrorText) ? "Unable to sign data." : e.ErrorText));
			}, null);

			return Result.Task;
		}

        #endregion

        #region Validating Signatures

        /// <summary>
        /// Validates a signature of binary data.
        /// </summary>
        /// <param name="LegalId">Legal identity used to create the signature. If empty, current approved legal identities will be used to validate the signature.</param>
        /// <param name="Data">Binary data to sign-</param>
        /// <param name="Signature">Digital signature of data</param>
        /// <param name="Callback">Method to call when response is returned.</param>
        /// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
        public void ValidateSignature(string LegalId, byte[] Data, byte[] Signature, LegalIdentityEventHandler Callback, object State)
		{
			this.ValidateSignature(this.componentAddress, LegalId, Data, Signature, Callback, State);
		}

        /// <summary>
        /// Validates a signature of binary data.
        /// </summary>
        /// <param name="Address">Address of entity on which the legal identity are registered.</param>
        /// <param name="LegalId">Legal identity used to create the signature. If empty, current approved legal identities will be used to validate the signature.</param>
        /// <param name="Data">Binary data to sign-</param>
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
			Xml.Append(NamespaceLegalIdentities);
			Xml.Append("\"/>");

			this.client.SendIqGet(Address, Xml.ToString(), (sender, e) =>
			{
				LegalIdentity Identity = null;
				XmlElement E;

				if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "identity" && E.NamespaceURI == NamespaceLegalIdentities)
					Identity = LegalIdentity.Parse(E);
				else
					e.Ok = false;

				Callback?.Invoke(this, new LegalIdentityEventArgs(e, Identity));
			}, State);
		}

        /// <summary>
        /// Validates a signature of binary data.
        /// </summary>
        /// <param name="LegalId">Legal identity used to create the signature. If empty, current approved legal identities will be used to validate the signature.</param>
        /// <param name="Data">Binary data to sign-</param>
        /// <param name="Signature">Digital signature of data</param>
        /// <returns>Legal identity object.</returns>
        public Task<LegalIdentity> ValidateSignatureAsync(string LegalId, byte[] Data, byte[] Signature)
		{
			return this.ValidateSignatureAsync(this.componentAddress, LegalId, Data, Signature);
		}

        /// <summary>
        /// Validates a signature of binary data.
        /// </summary>
        /// <param name="Address">Address of entity on which the legal identity are registered.</param>
        /// <param name="LegalId">Legal identity used to create the signature. If empty, current approved legal identities will be used to validate the signature.</param>
        /// <param name="Data">Binary data to sign-</param>
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
					Result.SetException(new IOException(string.IsNullOrEmpty(e.ErrorText) ? "Unable to sign data." : e.ErrorText));
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
			Part[] Parts, Parameter[] Parameters, ContractVisibility Visibility, ContractParts PartsMode, Duration Duration,
			Duration ArchiveRequired, Duration ArchiveOptional, DateTime? SignAfter, DateTime? SignBefore, bool CanActAsTemplate,
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
			Part[] Parts, Parameter[] Parameters, ContractVisibility Visibility, ContractParts PartsMode, Duration Duration,
			Duration ArchiveRequired, Duration ArchiveOptional, DateTime? SignAfter, DateTime? SignBefore, bool CanActAsTemplate,
			SmartContractEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<createContract xmlns=\"");
			Xml.Append(NamespaceSmartContracts);
			Xml.Append("\">");

			Contract Contract = new Contract()
			{
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

			Contract.Serialize(Xml, false, false, false, false, false);

			Xml.Append("</createContract>");

			this.client.SendIqSet(Address, Xml.ToString(), this.ContractResponse, new object[] { Callback, State });
		}

		private void ContractResponse(object Sender, IqResultEventArgs e)
		{
			object[] P = (object[])e.State;
			SmartContractEventHandler Callback = (SmartContractEventHandler)P[0];
			Contract Contract = null;
			XmlElement E;

			if (e.Ok && (E = e.FirstElement) != null &&
				E.LocalName == "contract" &&
				E.NamespaceURI == NamespaceSmartContracts)
			{
				Contract = Contract.Parse(E, out bool HasStatus);
			}
			else
				e.Ok = false;

			e.State = P[1];
			Callback?.Invoke(this, new SmartContractEventArgs(e, Contract));
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
			Part[] Parts, Parameter[] Parameters, ContractVisibility Visibility, ContractParts PartsMode, Duration Duration,
			Duration ArchiveRequired, Duration ArchiveOptional, DateTime? SignAfter, DateTime? SignBefore, bool CanActAsTemplate)
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
			Part[] Parts, Parameter[] Parameters, ContractVisibility Visibility, ContractParts PartsMode, Duration Duration,
			Duration ArchiveRequired, Duration ArchiveOptional, DateTime? SignAfter, DateTime? SignBefore, bool CanActAsTemplate)
		{
			TaskCompletionSource<Contract> Result = new TaskCompletionSource<Contract>();

			this.CreateContract(Address, ForMachines, ForHumans, Roles, Parts, Parameters, Visibility, PartsMode, Duration,
				ArchiveRequired, ArchiveOptional, SignAfter, SignBefore, CanActAsTemplate, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.Contract);
				else
				{
					Result.SetException(new IOException(string.IsNullOrEmpty(e.ErrorText) ?
						"Unable to create the contract." : e.ErrorText));
				}
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
			ContractParts PartsMode, Duration Duration, Duration ArchiveRequired, Duration ArchiveOptional, DateTime? SignAfter,
			DateTime? SignBefore, bool CanActAsTemplate, SmartContractEventHandler Callback, object State)
		{
			this.CreateContract(this.componentAddress, TemplateId, Parts, Parameters, Visibility, PartsMode,
				Duration, ArchiveRequired, ArchiveOptional, SignAfter, SignBefore, CanActAsTemplate, Callback, State);
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
			ContractParts PartsMode, Duration Duration, Duration ArchiveRequired, Duration ArchiveOptional, DateTime? SignAfter,
			DateTime? SignBefore, bool CanActAsTemplate, SmartContractEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<createContract xmlns=\"");
			Xml.Append(NamespaceSmartContracts);
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
					if (Parts != null)
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

			if (Parameters != null && Parameters.Length > 0)
			{
				Xml.Append("<parameters>");

				foreach (Parameter Parameter in Parameters)
					Parameter.Serialize(Xml);

				Xml.Append("</parameters>");
			}

			Xml.Append("</template></createContract>");

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
			ContractParts PartsMode, Duration Duration, Duration ArchiveRequired, Duration ArchiveOptional, DateTime? SignAfter,
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
		public Task<Contract> CreateContractAsync(string Address, string TemplateId, Part[] Parts, Parameter[] Parameters,
			ContractVisibility Visibility, ContractParts PartsMode, Duration Duration, Duration ArchiveRequired, Duration ArchiveOptional,
			DateTime? SignAfter, DateTime? SignBefore, bool CanActAsTemplate)
		{
			TaskCompletionSource<Contract> Result = new TaskCompletionSource<Contract>();

			this.CreateContract(Address, TemplateId, Parts, Parameters, Visibility, PartsMode, Duration,
				ArchiveRequired, ArchiveOptional, SignAfter, SignBefore, CanActAsTemplate, (sender, e) =>
				{
					if (e.Ok)
						Result.SetResult(e.Contract);
					else
					{
						Result.SetException(new IOException(string.IsNullOrEmpty(e.ErrorText) ?
							"Unable to create the contract." : e.ErrorText));
					}
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
		public void GetCreatedContracts(IdReferencesEventHandler Callback, object State)
		{
			this.GetCreatedContracts(this.componentAddress, Callback, State);
		}

		/// <summary>
		/// Get contracts the account has created.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetCreatedContracts(string Address, IdReferencesEventHandler Callback, object State)
		{
			this.client.SendIqGet(Address, "<getCreatedContracts xmlns='" + NamespaceSmartContracts + "'/>",
				this.IdReferencesResponse, new object[] { Callback, State });
		}

		private void IdReferencesResponse(object Sender, IqResultEventArgs e)
		{
			object[] P = (object[])e.State;
			IdReferencesEventHandler Callback = (IdReferencesEventHandler)P[0];
			XmlElement E = e.FirstElement;
			List<string> IDs = new List<string>();

			if (e.Ok && E != null)
			{
				foreach (XmlNode N in E.ChildNodes)
				{
					if (N is XmlElement E2 && E2.LocalName == "ref" && E2.NamespaceURI == NamespaceSmartContracts)
					{
						string Id = XML.Attribute(E2, "id");
						IDs.Add(Id);
					}
				}
			}
			else
				e.Ok = false;

			e.State = P[1];
			Callback?.Invoke(this, new IdReferencesEventArgs(e, IDs.ToArray()));
		}

		/// <summary>
		/// Get contracts the account has created.
		/// </summary>
		/// <returns>Contract IDs</returns>
		public Task<string[]> GetCreatedContractsAsync()
		{
			return this.GetCreatedContractsAsync(this.componentAddress);
		}

		/// <summary>
		/// Get contracts the account has created.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <returns>Contract IDs</returns>
		public Task<string[]> GetCreatedContractsAsync(string Address)
		{
			TaskCompletionSource<string[]> Result = new TaskCompletionSource<string[]>();

			this.GetCreatedContracts(Address, (sender, e) =>
				{
					if (e.Ok)
						Result.SetResult(e.References);
					else
					{
						Result.SetException(new IOException(string.IsNullOrEmpty(e.ErrorText) ?
							"Unable to get created contracts." : e.ErrorText));
					}
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
		public void SignContract(Contract Contract, string Role, bool Transferable, SmartContractEventHandler Callback, object State)
		{
			this.SignContract(this.componentAddress, Contract, Role, Transferable, Callback, State);
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
		public void SignContract(string Address, Contract Contract, string Role, bool Transferable,
			SmartContractEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();
			Contract.Serialize(Xml, false, false, false, false, false);
			byte[] Data = Encoding.UTF8.GetBytes(Xml.ToString());

			this.Sign(Address, Data, (sender, e) =>
			{
				if (e.Ok)
				{
					Xml.Clear();
					Xml.Append("<signContract xmlns='");
					Xml.Append(NamespaceSmartContracts);
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
				else
					Callback?.Invoke(this, new SmartContractEventArgs(e, null));
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
			return this.SignContractAsync(this.componentAddress, Contract, Role, Transferable);
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
		public Task<Contract> SignContractAsync(string Address, Contract Contract, string Role, bool Transferable)
		{
			TaskCompletionSource<Contract> Result = new TaskCompletionSource<Contract>();

			this.SignContract(Address, Contract, Role, Transferable, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.Contract);
				else
				{
					Result.SetException(new IOException(string.IsNullOrEmpty(e.ErrorText) ?
						"Unable to sign the contract." : e.ErrorText));
				}
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
		public void GetSignedContracts(IdReferencesEventHandler Callback, object State)
		{
			this.GetSignedContracts(this.componentAddress, Callback, State);
		}

		/// <summary>
		/// Get contracts the account has signed.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetSignedContracts(string Address, IdReferencesEventHandler Callback, object State)
		{
			this.client.SendIqGet(Address, "<getSignedContracts xmlns='" + NamespaceSmartContracts + "'/>",
				this.IdReferencesResponse, new object[] { Callback, State });
		}

		/// <summary>
		/// Get contracts the account has signed.
		/// </summary>
		/// <returns>Contract IDs</returns>
		public Task<string[]> GetSignedContractsAsync()
		{
			return this.GetSignedContractsAsync(this.componentAddress);
		}

		/// <summary>
		/// Get contracts the account has signed.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <returns>Contract IDs</returns>
		public Task<string[]> GetSignedContractsAsync(string Address)
		{
			TaskCompletionSource<string[]> Result = new TaskCompletionSource<string[]>();

			this.GetSignedContracts(Address, (sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e.References);
				else
				{
					Result.SetException(new IOException(string.IsNullOrEmpty(e.ErrorText) ?
						"Unable to get signed contracts." : e.ErrorText));
				}
			}, null);

			return Result.Task;
		}

		#endregion

		#region Contract Signature event

		private void ContractSignedMessageHandler(object Sender, MessageEventArgs e)
		{
			ContractSignedEventHandler h = this.ContractSigned;

			if (h != null)
			{
				string ContractId = XML.Attribute(e.Content, "contractId");
				string LegalId = XML.Attribute(e.Content, "legalId");

				h(this, new ContractSignedEventArgs(ContractId, LegalId));
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
			this.GetContract(this.componentAddress, ContractId, Callback, State);
		}

		/// <summary>
		/// Gets a contract
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ContractId">ID of contract to get.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetContract(string Address, string ContractId,
			SmartContractEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getContract xmlns='");
			Xml.Append(NamespaceSmartContracts);
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
			return this.GetContractAsync(this.componentAddress, ContractId);
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
				{
					Result.SetException(new IOException(string.IsNullOrEmpty(e.ErrorText) ?
						"Unable to get the contract." : e.ErrorText));
				}
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
			this.ObsoleteContract(this.componentAddress, ContractId, Callback, State);
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
			Xml.Append(NamespaceSmartContracts);
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
			return this.ObsoleteContractAsync(this.componentAddress, ContractId);
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
				{
					Result.SetException(new IOException(string.IsNullOrEmpty(e.ErrorText) ?
						"Unable to obsolete the contract." : e.ErrorText));
				}
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
			this.DeleteContract(this.componentAddress, ContractId, Callback, State);
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
			Xml.Append(NamespaceSmartContracts);
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
			return this.DeleteContractAsync(this.componentAddress, ContractId);
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
				{
					Result.SetException(new IOException(string.IsNullOrEmpty(e.ErrorText) ?
						"Unable to delete the contract." : e.ErrorText));
				}
			}, null);

			return Result.Task;
		}

		#endregion

		#region Contract Updated event

		private void ContractUpdatedMessageHandler(object Sender, MessageEventArgs e)
		{
			ContractReferenceEventHandler h = this.ContractUpdated;

			if (h != null)
			{
				string ContractId = XML.Attribute(e.Content, "contractId");

				if (!this.IsFromTrustProvider(ContractId, e.From))
					return;

				h(this, new ContractReferenceEventArgs(ContractId));
			}
		}

		/// <summary>
		/// Event raised whenever a contract has been updated.
		/// </summary>
		public event ContractReferenceEventHandler ContractUpdated = null;

		#endregion

		#region Contract Deleted event

		private void ContractDeletedMessageHandler(object Sender, MessageEventArgs e)
		{
			ContractReferenceEventHandler h = this.ContractDeleted;

			if (h != null)
			{
				string ContractId = XML.Attribute(e.Content, "contractId");

				if (!this.IsFromTrustProvider(ContractId, e.From))
					return;

				h(this, new ContractReferenceEventArgs(ContractId));
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
			this.UpdateContract(this.componentAddress, Contract, Callback, State);
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
			Xml.Append(NamespaceSmartContracts);
			Xml.Append("'>");

			Contract.Serialize(Xml, false, true, true, false, false);

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
			return this.UpdateContractAsync(this.componentAddress, Contract);
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
				{
					Result.SetException(new IOException(string.IsNullOrEmpty(e.ErrorText) ?
						"Unable to update the contract." : e.ErrorText));
				}
			}, null);

			return Result.Task;
		}

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
			this.client.SendIqGet(Address, "<getSchemas xmlns='" + NamespaceSmartContracts + "'/>",
				(sender, e) =>
				{
					XmlElement E = e.FirstElement;
					List<SchemaReference> Schemas = new List<SchemaReference>();

					if (e.Ok && E != null && E.LocalName == "schemas" && E.NamespaceURI == NamespaceSmartContracts)
					{
						foreach (XmlNode N in E.ChildNodes)
						{
							if (N is XmlElement E2 && E2.LocalName == "schemaRef" && E2.NamespaceURI == NamespaceSmartContracts)
							{
								string Namespace = XML.Attribute(E2, "namespace");
								List<SchemaDigest> Digests = new List<SchemaDigest>();

								foreach (XmlNode N2 in E2.ChildNodes)
								{
									if (N2 is XmlElement E3 && E3.LocalName == "digest" && E3.NamespaceURI == NamespaceSmartContracts)
									{
										if (!Enum.TryParse<Waher.Security.HashFunction>(XML.Attribute(E3, "function"), out Security.HashFunction Function))
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

					Callback?.Invoke(this, new SchemaReferencesEventArgs(e, Schemas.ToArray()));

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
				{
					Result.SetException(new IOException(string.IsNullOrEmpty(e.ErrorText) ?
						"Unable to get schemas." : e.ErrorText));
				}
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
			Xml.Append(NamespaceSmartContracts);
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
				(sender, e) =>
				{
					XmlElement E = e.FirstElement;
					byte[] Schema = null;

					if (e.Ok && E != null && E.LocalName == "schema" && E.NamespaceURI == NamespaceSmartContracts)
						Schema = Convert.FromBase64String(E.InnerText);
					else
						e.Ok = false;

					Callback?.Invoke(this, new SchemaEventArgs(e, Schema));

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
				{
					Result.SetException(new IOException(string.IsNullOrEmpty(e.ErrorText) ?
						"Unable to get schema." : e.ErrorText));
				}
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
			this.GetContractLegalIdentities(this.componentAddress, ContractId, false, true, Callback, State);
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
			this.GetContractLegalIdentities(this.componentAddress, ContractId, Current, Historic, Callback, State);
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
			Xml.Append(NamespaceSmartContracts);
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
			return this.GetContractLegalIdentitiesAsync(this.componentAddress, ContractId, false, true);
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
			return this.GetContractLegalIdentitiesAsync(this.componentAddress, ContractId, Current, Historic);
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
				{
					Result.SetException(new IOException(string.IsNullOrEmpty(e.ErrorText) ?
						"Unable to get legal identities." : e.ErrorText));
				}
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
			this.GetContractNetworkIdentities(this.componentAddress, ContractId, Callback, State);
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
			Xml.Append(NamespaceSmartContracts);
			Xml.Append("' contractId='");
			Xml.Append(XML.Encode(ContractId));
			Xml.Append("'/>");

			this.client.SendIqGet(Address, Xml.ToString(), (sender, e) =>
			{
				NetworkIdentity[] Identities = null;
				XmlElement E;

				if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "networkIdentities" && E.NamespaceURI == NamespaceSmartContracts)
				{
					List<NetworkIdentity> IdentitiesList = new List<NetworkIdentity>();

					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2 &&
							E2.LocalName == "networkIdentity" &&
							E2.NamespaceURI == E.NamespaceURI)
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

				Callback?.Invoke(this, new NetworkIdentitiesEventArgs(e, Identities));
			}, State);
		}

		/// <summary>
		/// Gets available network identities related to a contract.
		/// </summary>
		/// <param name="ContractId">Get network identities related to the contract identified by this identity.</param>
		/// <returns>Network identities.</returns>
		public Task<NetworkIdentity[]> GetContractNetworkIdentitiesAsync(string ContractId)
		{
			return this.GetContractNetworkIdentitiesAsync(this.componentAddress, ContractId);
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
				{
					Result.SetException(new IOException(string.IsNullOrEmpty(e.ErrorText) ?
						"Unable to get network identities." : e.ErrorText));
				}
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
			Xml.Append(NamespaceSmartContracts);

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
			Array.Sort<SearchFilter>(Filter, (f1, f2) => f1.Order - f2.Order);

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

			this.client.SendIqGet(Address, Xml.ToString(), (sender, e) =>
			{
				XmlElement E = e.FirstElement;
				List<string> IDs = null;
				bool More = false;

				if (e.Ok && E != null && E.LocalName == "searchResult" && E.NamespaceURI == NamespaceSmartContracts)
				{
					More = XML.Attribute(E, "more", false);
					IDs = new List<string>();

					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2 && E2.LocalName == "ref" && E2.NamespaceURI == NamespaceSmartContracts)
						{
							string Id = XML.Attribute(E2, "id");
							IDs.Add(Id);
						}
					}
				}
				else
					e.Ok = false;

				Callback?.Invoke(this, new SearchResultEventArgs(e, Offset, MaxCount, More, IDs?.ToArray()));
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
			}, null);

			return Result.Task;
		}

		#endregion
	}
}
