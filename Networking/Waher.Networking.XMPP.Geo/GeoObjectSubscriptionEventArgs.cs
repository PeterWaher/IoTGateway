using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Geo
{
	/// <summary>
	/// Event arguments for events related to Geo-spatial objects.
	/// </summary>
	public class GeoObjectSubscriptionEventArgs : MessageEventArgs
	{
		/// <summary>
		/// Event arguments for events related to Geo-spatial objects.
		/// </summary>
		/// <param name="e">Message event arguments.</param>
		/// <param name="SubscriptionId">Geo-spatial subscription identifier.</param>
		/// <param name="Reference">Reference to geo-spatial object.</param>
		public GeoObjectSubscriptionEventArgs(MessageEventArgs e, string SubscriptionId, GeoObjectReference Reference)
			: base(e)
		{
			this.SubscriptionId = SubscriptionId;
			this.Reference = Reference;
		}

		/// <summary>
		/// Geo-spatial subscription identifier.
		/// </summary>
		public string SubscriptionId { get; }

		/// <summary>
		/// Reference to geo-spatial object.
		/// </summary>
		public GeoObjectReference Reference { get; }
	}
}
