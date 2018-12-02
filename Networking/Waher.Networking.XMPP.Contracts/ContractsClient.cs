using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;
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

		private readonly Dictionary<string, KeyEventArgs> publicKeys = new Dictionary<string, KeyEventArgs>();
		private readonly Dictionary<string, KeyEventArgs> matchingKeys = new Dictionary<string, KeyEventArgs>();
		private EndpointSecurity localEndpoint;
		private readonly Task<EndpointSecurity> localEndpointTask;
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
			this.localEndpointTask = this.LoadKeys();
		}

		private async Task<EndpointSecurity> LoadKeys()
		{
			List<IE2eEndpoint> Keys = new List<IE2eEndpoint>();
			string Name = typeof(ContractsClient).FullName;

			foreach (EcAes256 Curve in EndpointSecurity.CreateEndpoints(256, 192, int.MaxValue,
				typeof(EcAes256)))
			{
				string d = await RuntimeSettings.GetAsync(Name + "." + Curve.LocalName, string.Empty);
				if (string.IsNullOrEmpty(d) || !BigInteger.TryParse(d, out BigInteger D))
				{
					D = this.GetKey(Curve.Curve);
					await RuntimeSettings.SetAsync(Name + "." + Curve.LocalName, D.ToString());
					Keys.Add(Curve);
				}
				else
				{
					Keys.Add(Curve.Create(D));
					Curve.Dispose();
				}
			}

			this.localEndpoint = new EndpointSecurity(null, 128, Keys.ToArray());

			return this.localEndpoint;
		}

		private BigInteger GetKey(CurvePrimeField Curve)
		{
			string s = Curve.Export();
			XmlDocument Doc = new XmlDocument();
			Doc.LoadXml(s);
			s = Doc.DocumentElement.GetAttribute("d");
			return BigInteger.Parse(s);
		}

		private EndpointSecurity LocalEndpoint
		{
			get
			{
				if (this.localEndpoint == null)
					return this.localEndpointTask.Result;
				else
					return this.localEndpoint;
			}
		}

		/// <summary>
		/// Disposes of the extension.
		/// </summary>
		public override void Dispose()
		{
			this.localEndpoint?.Dispose();
			this.localEndpoint = null;

			base.Dispose();
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[] { };

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
		public Task<IE2eEndpoint> GetServerPublicKeyAsync()
		{
			return this.GetServerPublicKeyAsync(this.componentAddress);
		}

		/// <summary>
		/// Gets the server public key.
		/// </summary>
		/// <param name="Address">Address of entity whose public key is requested.</param>
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
						if (LocalKey == null)
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
		public Task<IE2eEndpoint> GetMatchingLocalKeyAsync()
		{
			return this.GetMatchingLocalKeyAsync(this.componentAddress);
		}

		/// <summary>
		/// Get the local key that matches a given server key.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
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
					e.Key.ToXml(Identity);
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

					if (e.Key is EcAes256 EcAes256)
					{
						KeyValuePair<byte[], byte[]> Signature = EcAes256.Sign(Bin, Security.HashFunction.SHA256);

						Xml.Append("<clientSignature s1=\"");
						Xml.Append(Convert.ToBase64String(Signature.Key));
						Xml.Append("\" s2=\"");
						Xml.Append(Convert.ToBase64String(Signature.Value));
						Xml.Append("\"/>");
					}
					else if (e.Key is RsaAes RsaAes)
					{
						byte[] Signature = RsaAes.Sign(Bin);

						Xml.Append("<clientSignature s1=\"");
						Xml.Append(Convert.ToBase64String(Signature));
						Xml.Append("\"/>");
					}

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
		public Task<LegalIdentity> ApplyAsync(Property[] Properties)
		{
			return this.ApplyAsync(this.componentAddress, Properties);
		}

		/// <summary>
		/// Applies for a legal identity to be registered.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="Properties">Properties of the legal identity.</param>
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

		/// <summary>
		/// Validates a legal identity.
		/// </summary>
		/// <param name="Identity">Legal identity to validate</param>
		/// <param name="Callback">Method to call when validation is completed</param>
		/// <param name="State">State object to pass to callback method.</param>
		public void Validate(LegalIdentity Identity, IdentityValidationEventHandler Callback, object State)
		{
			if (Identity == null)
			{
				this.ReturnStatus(IdentityStatus.IdentityUndefined, Callback, State);
				return;
			}

			if (Identity.State != IdentityState.Approved)
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

			if (Now > Identity.To)
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
				Identity.ClientPubKey1 == null ||
				Identity.ClientPubKey1.Length == 0 ||
				Identity.ClientPubKey2 == null ||
				Identity.ClientPubKey2.Length == 0)
			{
				this.ReturnStatus(IdentityStatus.NoClientPublicKey, Callback, State);
				return;
			}

			StringBuilder Xml = new StringBuilder();
			Identity.Serialize(Xml, false, false, false, false, false);
			byte[] Data = Encoding.UTF8.GetBytes(Xml.ToString());

			if (Identity.ClientKeyName.StartsWith("RSA") &&
				int.TryParse(Identity.ClientKeyName.Substring(3), out int KeySize))
			{
				if (Identity.ClientSignature1 == null ||
					Identity.ClientSignature1.Length == 0)
				{
					this.ReturnStatus(IdentityStatus.NoClientSignature, Callback, State);
					return;
				}

				if (!RsaAes.Verify(Data, Identity.ClientSignature1, KeySize,
					Identity.ClientPubKey1, Identity.ClientPubKey2))
				{
					this.ReturnStatus(IdentityStatus.ClientSignatureInvalid, Callback, State);
					return;
				}
			}
			else if (EndpointSecurity.TryGetEndpoint(Identity.ClientKeyName,
				EndpointSecurity.IoTHarmonizationE2E, out IE2eEndpoint LocalKey) &&
				LocalKey is EcAes256 LocalEc)
			{
				if (Identity.ClientSignature1 == null ||
					Identity.ClientSignature1.Length == 0 ||
					Identity.ClientSignature2 == null ||
					Identity.ClientSignature2.Length == 0)
				{
					this.ReturnStatus(IdentityStatus.NoClientSignature, Callback, State);
					return;
				}

				if (!LocalEc.Verify(Data, Identity.ClientPubKey1, Identity.ClientPubKey2,
					Identity.ClientSignature1, Identity.ClientSignature2, HashFunction.SHA256))
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

			if (Identity.ServerSignature1 == null ||
				Identity.ServerSignature1.Length == 0)
			{
				this.ReturnStatus(IdentityStatus.NoProviderSignature, Callback, State);
				return;
			}

			this.GetServerPublicKey(Identity.Provider, (sender, e) =>
			{
				if (e.Ok && e.Key != null)
				{
					if (e.Key is RsaAes RsaAes)
					{
						if (!RsaAes.Verify(Data, Identity.ServerSignature1, RsaAes.KeySize,
							RsaAes.Modulus, RsaAes.Exponent))
						{
							this.ReturnStatus(IdentityStatus.ProviderSignatureInvalid, Callback, State);
							return;
						}
					}
					else if (e.Key is EcAes256 LocalEc)
					{
						if (Identity.ServerSignature2 == null ||
							Identity.ServerSignature2.Length == 0)
						{
							this.ReturnStatus(IdentityStatus.NoProviderSignature, Callback, State);
							return;
						}

						if (!LocalEc.Verify(Data, LocalEc.PublicKey,
							Identity.ServerSignature1, Identity.ServerSignature2, HashFunction.SHA256))
						{
							this.ReturnStatus(IdentityStatus.ProviderSignatureInvalid, Callback, State);
							return;
						}
					}
					else
					{
						this.ReturnStatus(IdentityStatus.ProviderKeyNotRecognized, Callback, State);
						return;
					}

					this.ReturnStatus(IdentityStatus.Valid, Callback, State);
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
		public Task<IdentityStatus> ValidateAsync(LegalIdentity Identity)
		{
			TaskCompletionSource<IdentityStatus> Result = new TaskCompletionSource<IdentityStatus>();

			this.Validate(Identity, (sender, e) =>
			{
				Result.SetResult(e.Status);
			}, null);

			return Result.Task;
		}

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
			this.client.SendIqGet(Address, "<getLegalIdentities xmlns=\"" + NamespaceLegalIdentities + "\"/>", (sender, e) =>
			{
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

				Callback?.Invoke(this, new LegalIdentitiesEventArgs(e, Identities));
			}, State);
		}

		/// <summary>
		/// Gets legal identities registered with the account.
		/// </summary>
		public Task<LegalIdentity[]> GetLegalIdentitiesAsync()
		{
			return this.GetLegalIdentitiesAsync(this.componentAddress);
		}

		/// <summary>
		/// Gets legal identities registered with the account.
		/// </summary>
		/// <param name="Address">Address of entity on which the legal identities are registered.</param>
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



	}
}
