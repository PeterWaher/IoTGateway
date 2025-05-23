using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP.Events;
using Waher.Runtime.Geo;

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

		#region Subscribe

		/// <summary>
		/// Subscribes to geo-spatial information, or updates an existing subscription.
		/// 
		/// The client subscribes to geo-spatial information using a bounding box.
		/// The bounding box is defined using to coordinates using the Mercator projection, 
		/// which is a cylindrical map projection. 
		/// 
		/// If the minimum longitude is greater than the maximum longitude value, the 
		/// bounding box is assumed to wrap around the antimeridian, or the +- 180 degree 
		/// longitude meridian.
		/// </summary>
		/// <param name="Min">Lower left-hand corner of bounding box, using the Mercator projection.</param>
		/// <param name="Max">Upper right-hand corner of bounding box, using the Mercator projection.</param>
		/// <param name="Callback">Called when a response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task Subscribe(GeoPosition Min, GeoPosition Max,
			EventHandlerAsync<SubscribedEventArgs> Callback, object State)
		{
			return this.Subscribe(string.Empty, Min, Max, null, Callback, State);
		}

		/// <summary>
		/// Subscribes to geo-spatial information, or updates an existing subscription.
		/// 
		/// The client subscribes to geo-spatial information using a bounding box.
		/// The bounding box is defined using to coordinates using the Mercator projection, 
		/// which is a cylindrical map projection. 
		/// 
		/// If the minimum longitude is greater than the maximum longitude value, the 
		/// bounding box is assumed to wrap around the antimeridian, or the +- 180 degree 
		/// longitude meridian.
		/// </summary>
		/// <param name="Min">Lower left-hand corner of bounding box, using the Mercator projection.</param>
		/// <param name="Max">Upper right-hand corner of bounding box, using the Mercator projection.</param>
		/// <param name="Ttl">Number of seconds the subscription should be alive, unless
		/// the connection is dropped (for a local subscription) or subscription is updated.</param>
		/// <param name="Callback">Called when a response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task Subscribe(GeoPosition Min, GeoPosition Max, int Ttl,
			EventHandlerAsync<SubscribedEventArgs> Callback, object State)
		{
			return this.Subscribe(string.Empty, Min, Max, Ttl, Callback, State);
		}

		/// <summary>
		/// Subscribes to geo-spatial information, or updates an existing subscription.
		/// 
		/// The client subscribes to geo-spatial information using a bounding box.
		/// The bounding box is defined using to coordinates using the Mercator projection, 
		/// which is a cylindrical map projection. 
		/// 
		/// If the minimum longitude is greater than the maximum longitude value, the 
		/// bounding box is assumed to wrap around the antimeridian, or the +- 180 degree 
		/// longitude meridian.
		/// </summary>
		/// <param name="SubscriptionId">Identifier of existing subscription, if updating it.</param>
		/// <param name="Min">Lower left-hand corner of bounding box, using the Mercator projection.</param>
		/// <param name="Max">Upper right-hand corner of bounding box, using the Mercator projection.</param>
		/// <param name="Ttl">Number of seconds the subscription should be alive, unless
		/// the connection is dropped (for a local subscription) or subscription is updated.</param>
		/// <param name="Callback">Called when a response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task Subscribe(string SubscriptionId, GeoPosition Min, GeoPosition Max, int? Ttl, 
			EventHandlerAsync<SubscribedEventArgs> Callback, object State)
		{
			return this.Subscribe(this.componentAddress, SubscriptionId, Min, Max, Ttl, Callback, State);
		}

		/// <summary>
		/// Subscribes to geo-spatial information, or updates an existing subscription.
		/// 
		/// The client subscribes to geo-spatial information using a bounding box.
		/// The bounding box is defined using to coordinates using the Mercator projection, 
		/// which is a cylindrical map projection. 
		/// 
		/// If the minimum longitude is greater than the maximum longitude value, the 
		/// bounding box is assumed to wrap around the antimeridian, or the +- 180 degree 
		/// longitude meridian.
		/// </summary>
		/// <param name="ComponentAddress">Geo-spatial component address.</param>
		/// <param name="SubscriptionId">Identifier of existing subscription, if updating it.</param>
		/// <param name="Min">Lower left-hand corner of bounding box, using the Mercator projection.</param>
		/// <param name="Max">Upper right-hand corner of bounding box, using the Mercator projection.</param>
		/// <param name="Ttl">Number of seconds the subscription should be alive, unless
		/// the connection is dropped (for a local subscription) or subscription is updated.</param>
		/// <param name="Callback">Called when a response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public async Task Subscribe(string ComponentAddress, string SubscriptionId,
			GeoPosition Min, GeoPosition Max, int? Ttl,
			EventHandlerAsync<SubscribedEventArgs> Callback, object State)
		{
			if (Min is null)
				throw new ArgumentNullException(nameof(Min));

			if (Max is null)
				throw new ArgumentNullException(nameof(Max));

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<subscribe xmlns='");
			Xml.Append(NamespaceGeoSpatialNeuroFoundationV1);

			if (!string.IsNullOrEmpty(SubscriptionId))
			{
				Xml.Append("' id='");
				Xml.Append(XML.Encode(ComponentAddress));
			}

			Xml.Append("' minLat='");
			Xml.Append(CommonTypes.Encode(Min.Latitude));

			Xml.Append("' minLon='");
			Xml.Append(CommonTypes.Encode(Min.Longitude));

			if (Min.Altitude.HasValue)
			{
				Xml.Append("' minAlt='");
				Xml.Append(CommonTypes.Encode(Min.Altitude.Value));
			}

			Xml.Append("' maxLat='");
			Xml.Append(CommonTypes.Encode(Max.Latitude));

			Xml.Append("' maxLon='");
			Xml.Append(CommonTypes.Encode(Max.Longitude));

			if (Max.Altitude.HasValue)
			{
				Xml.Append("' maxAlt='");
				Xml.Append(CommonTypes.Encode(Max.Altitude.Value));
			}

			if (Ttl.HasValue)
			{
				Xml.Append("' ttl='");
				Xml.Append(Ttl.Value);
			}

			Xml.Append("'/>");

			await this.client.SendIqSet(ComponentAddress, Xml.ToString(), async (sender, e) =>
			{
				await Callback.Raise(this, new SubscribedEventArgs(e));
			}, State);
		}

		/// <summary>
		/// Subscribes to geo-spatial information, or updates an existing subscription.
		/// 
		/// The client subscribes to geo-spatial information using a bounding box.
		/// The bounding box is defined using to coordinates using the Mercator projection, 
		/// which is a cylindrical map projection. 
		/// 
		/// If the minimum longitude is greater than the maximum longitude value, the 
		/// bounding box is assumed to wrap around the antimeridian, or the +- 180 degree 
		/// longitude meridian.
		/// </summary>
		/// <param name="Min">Lower left-hand corner of bounding box, using the Mercator projection.</param>
		/// <param name="Max">Upper right-hand corner of bounding box, using the Mercator projection.</param>
		/// <returns>Subscription result.</returns>
		/// <exception cref="Exception">In case an error is returned.</exception>
		public Task<SubscribedEventArgs> SubscribeAsync(GeoPosition Min, GeoPosition Max)
		{
			return this.SubscribeAsync(string.Empty, Min, Max, null);
		}

		/// <summary>
		/// Subscribes to geo-spatial information, or updates an existing subscription.
		/// 
		/// The client subscribes to geo-spatial information using a bounding box.
		/// The bounding box is defined using to coordinates using the Mercator projection, 
		/// which is a cylindrical map projection. 
		/// 
		/// If the minimum longitude is greater than the maximum longitude value, the 
		/// bounding box is assumed to wrap around the antimeridian, or the +- 180 degree 
		/// longitude meridian.
		/// </summary>
		/// <param name="Min">Lower left-hand corner of bounding box, using the Mercator projection.</param>
		/// <param name="Max">Upper right-hand corner of bounding box, using the Mercator projection.</param>
		/// <param name="Ttl">Number of seconds the subscription should be alive, unless
		/// the connection is dropped (for a local subscription) or subscription is updated.</param>
		/// <returns>Subscription result.</returns>
		/// <exception cref="Exception">In case an error is returned.</exception>
		public Task<SubscribedEventArgs> SubscribeAsync(GeoPosition Min, GeoPosition Max, int Ttl)
		{
			return this.SubscribeAsync(string.Empty, Min, Max, Ttl);
		}

		/// <summary>
		/// Subscribes to geo-spatial information, or updates an existing subscription.
		/// 
		/// The client subscribes to geo-spatial information using a bounding box.
		/// The bounding box is defined using to coordinates using the Mercator projection, 
		/// which is a cylindrical map projection. 
		/// 
		/// If the minimum longitude is greater than the maximum longitude value, the 
		/// bounding box is assumed to wrap around the antimeridian, or the +- 180 degree 
		/// longitude meridian.
		/// </summary>
		/// <param name="SubscriptionId">Identifier of existing subscription, if updating it.</param>
		/// <param name="Min">Lower left-hand corner of bounding box, using the Mercator projection.</param>
		/// <param name="Max">Upper right-hand corner of bounding box, using the Mercator projection.</param>
		/// <param name="Ttl">Number of seconds the subscription should be alive, unless
		/// the connection is dropped (for a local subscription) or subscription is updated.</param>
		/// <returns>Subscription result.</returns>
		/// <exception cref="Exception">In case an error is returned.</exception>
		public Task<SubscribedEventArgs> SubscribeAsync(string SubscriptionId, GeoPosition Min, GeoPosition Max, int? Ttl)
		{
			return this.SubscribeAsync(this.componentAddress, SubscriptionId, Min, Max, Ttl);
		}

		/// <summary>
		/// Subscribes to geo-spatial information, or updates an existing subscription.
		/// 
		/// The client subscribes to geo-spatial information using a bounding box.
		/// The bounding box is defined using to coordinates using the Mercator projection, 
		/// which is a cylindrical map projection. 
		/// 
		/// If the minimum longitude is greater than the maximum longitude value, the 
		/// bounding box is assumed to wrap around the antimeridian, or the +- 180 degree 
		/// longitude meridian.
		/// </summary>
		/// <param name="ComponentAddress">Geo-spatial component address.</param>
		/// <param name="SubscriptionId">Identifier of existing subscription, if updating it.</param>
		/// <param name="Min">Lower left-hand corner of bounding box, using the Mercator projection.</param>
		/// <param name="Max">Upper right-hand corner of bounding box, using the Mercator projection.</param>
		/// <param name="Ttl">Number of seconds the subscription should be alive, unless
		/// the connection is dropped (for a local subscription) or subscription is updated.</param>
		/// <returns>Subscription result.</returns>
		/// <exception cref="Exception">In case an error is returned.</exception>
		public async Task<SubscribedEventArgs> SubscribeAsync(string ComponentAddress, string SubscriptionId,
			GeoPosition Min, GeoPosition Max, int? Ttl)
		{
			TaskCompletionSource<SubscribedEventArgs> Result = new TaskCompletionSource<SubscribedEventArgs>();

			await this.Subscribe(ComponentAddress, SubscriptionId, Min, Max, Ttl, (sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to perform subscription."));

				return Task.CompletedTask;
			}, null);

			return await Result.Task;
		}

		#endregion

		#region Unsubscribe 

		public async Task Unsubscribe()
		{
			// TODO
		}

		public async Task UnsubscribeAsync()
		{
			// TODO
		}

		#endregion

		#region Publications

		public async Task Publish()
		{
			// TODO
		}

		public async Task PublishAsync()
		{
			// TODO
		}

		public async Task Delete()
		{
			// TODO
		}

		public async Task DeleteAsync()
		{
			// TODO
		}

		#endregion

		#region Search

		public async Task Search()
		{
			// TODO
		}

		public async Task SearchAsync()
		{
			// TODO
		}

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
