using System;
using System.Threading.Tasks;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Geo
{
	/// <summary>
	/// Adds support for geo-spatial publish/subscribe communication pattern to an XMPP client.
	/// 
	/// The interface is defined in the Neuro-Foundation XMPP IoT extensions:
	/// https://neuro-foundation.io
	/// </summary>
	public class GeoClient : XmppExtension
	{
		private readonly string componentAddress;

		/// <summary>
		/// urn:nf:iot:geo:1.0
		/// </summary>
		public const string NamespaceGeoSpatialNeuroFoundationV1 = "urn:nf:iot:geo:1.0";

		/// <summary>
		/// Namespaces supported for legal identities.
		/// </summary>
		public static readonly string[] NamespacesGeoSpatial = new string[]
		{
			NamespaceGeoSpatialNeuroFoundationV1
		};

		/// <summary>
		/// If a namespace corresponds to a geo-spatial namespace.
		/// </summary>
		/// <param name="Namespace">Namespace</param>
		/// <returns>If the namespace corresponds to a geo-spatial namespace.</returns>
		public static bool IsNamespaceGeoSpatial(string Namespace)
		{
			return Array.IndexOf(NamespacesGeoSpatial, Namespace) >= 0;
		}

		#region Construction

		/// <summary>
		/// Adds support for geo-spatial publish/subscribe communication pattern to an XMPP client.
		/// 
		/// The interface is defined in the Neuro-Foundation XMPP IoT extensions:
		/// https://neuro-foundation.io
		/// </summary>
		/// <param name="Client">XMPP Client to use.</param>
		/// <param name="ComponentAddress">Address to the geo-spatial component.</param>
		public GeoClient(XmppClient Client, string ComponentAddress)
			: base(Client)
		{
			this.componentAddress = ComponentAddress;


			#region NeuroFoundation V1

			this.client.RegisterMessageHandler("added", NamespaceGeoSpatialNeuroFoundationV1, this.AddedMessageHandler, true);
			this.client.RegisterMessageHandler("updated", NamespaceGeoSpatialNeuroFoundationV1, this.UpdatedMessageHandler, false);
			this.client.RegisterMessageHandler("removed", NamespaceGeoSpatialNeuroFoundationV1, this.RemovedMessageHandler, false);

			#endregion
		}

		/// <summary>
		/// Disposes of the extension.
		/// </summary>
		public override void Dispose()
		{
			#region NeuroFoundation V1

			this.client.UnregisterMessageHandler("added", NamespaceGeoSpatialNeuroFoundationV1, this.AddedMessageHandler, true);
			this.client.UnregisterMessageHandler("updated", NamespaceGeoSpatialNeuroFoundationV1, this.UpdatedMessageHandler, false);
			this.client.UnregisterMessageHandler("removed", NamespaceGeoSpatialNeuroFoundationV1, this.RemovedMessageHandler, false);

			#endregion

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

		#region Event Notifications

		private async Task AddedMessageHandler(object Sender, MessageEventArgs e)
		{
			// TODO
		}

		private async Task UpdatedMessageHandler(object Sender, MessageEventArgs e)
		{
			// TODO
		}

		private async Task RemovedMessageHandler(object Sender, MessageEventArgs e)
		{
			// TODO
		}

		#endregion
	}
}
