using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Xml;
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
		public void GetServerPublicKey(PublicKeyEventHandler Callback, object State)
		{
			this.GetServerPublicKey(this.componentAddress, Callback, State);
		}

		/// <summary>
		/// Gets the server public key.
		/// </summary>
		/// <param name="Address">Address of entity whose public key is requested.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public void GetServerPublicKey(string Address, PublicKeyEventHandler Callback, object State)
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

				Callback?.Invoke(this, new PublicKeyEventArgs(e, ServerEndpoint));
			}, State);
		}

		public void GetMatchingLocalKey()
		{
			this.GetMatchingLocalKey(this.componentAddress);
		}

		public void GetMatchingLocalKey(string Address)
		{
			lock (this.matchingKeys)
			{
				if (this.matchingKeys.TryGetValue(Address, out IE2eEndpoint Endpoint))
					return Endpoint;
			}

			this.GetServerPublicKey(Address, (sender, e) =>
			{

			}, null);
		}

		private readonly Dictionary<string, IE2eEndpoint> matchingKeys = new Dictionary<string, IE2eEndpoint>();

		public void Apply()
		{
			this.Apply(this.componentAddress);
		}

		public void Apply(string Address)
		{
		}

	}
}
