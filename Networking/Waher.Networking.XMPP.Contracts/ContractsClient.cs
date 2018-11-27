using System;
using System.Threading.Tasks;
using System.Xml;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.P2P;
using Waher.Networking.XMPP.P2P.E2E;

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
			this.localEndpoint = new EndpointSecurity(Client, 128);
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
			this.client.SendIqGet(this.componentAddress, "<getPublicKey xmlns='" + NamespaceLegalIdentities + "'/>", (sender, e) =>
			{
				E2eEndpoint ServerEndpoint = null;
				XmlElement E;

				if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "publicKey" && E.NamespaceURI == NamespaceLegalIdentities)
				{
					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2)
						{
							ServerEndpoint = EndpointSecurity.ParseE2eKey(E2, this.localEndpoint);
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

	}
}
