using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Text;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP.P2P;
using Waher.Networking.XMPP.P2P.E2E;
using Waher.Runtime.Settings;
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
			s = Doc.DocumentElement["d"].ToString();
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
			this.client.SendIqGet(Address, "<getPublicKey xmlns='" + NamespaceLegalIdentities + "'/>", (sender, e) =>
			{
				IE2eEndpoint ServerEndpoint = null;
				XmlElement E;

				if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "publicKey" && E.NamespaceURI == NamespaceLegalIdentities)
				{
					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2)
						{
							ServerEndpoint = EndpointSecurity.ParseE2eKey(E2);
							if (ServerEndpoint != null)
								break;
						}
					}

					e.Ok = ServerEndpoint != null;
				}
				else
					e.Ok = false;

				Callback?.Invoke(this, new KeyEventArgs(e, ServerEndpoint));
			}, State);
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

				lock (this.matchingKeys)
				{
					this.matchingKeys[Address] = e0;
				}

				Callback?.Invoke(this, e0);

			}, null);
		}

		public void Apply(params Property[] Properties)
		{
			this.Apply(this.componentAddress, Properties);
		}

		public void Apply(string Address, params Property[] Properties)
		{
			this.GetMatchingLocalKey(Address, (sender, e) =>
			{
				if (e.Ok)
				{
					StringBuilder Xml = new StringBuilder();

					Xml.Append("<apply xmlns='");
					Xml.Append(NamespaceLegalIdentities);
					Xml.Append("'><identity><clientPublicKey>");
					e.Key.ToXml(Xml);
					Xml.Append("</clientPublicKey>");

					foreach (Property Property in Properties)
					{
						Xml.Append("<property name='");
						Xml.Append(XML.Encode(Property.Name));
						Xml.Append("' value='");
						Xml.Append(XML.Encode(Property.Value));
						Xml.Append("'/>");
					}

					// TODO: clientSignature

					Xml.Append("</apply");

					// TODO: IQ
				}
				else
				{
					// TODO
				}
			}, null);
		}

	}
}
