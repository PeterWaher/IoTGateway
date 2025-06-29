using Waher.Content.Xml;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Geo
{
	/// <summary>
	/// Event arguments for the result of a subscription request.
	/// </summary>
	public class SubscribedEventArgs : IqResultEventArgs
	{
		/// <summary>
		/// Event arguments for the result of a subscription request.
		/// </summary>
		/// <param name="e">Response</param>
		public SubscribedEventArgs(IqResultEventArgs e)
			: base(e)
		{
			if (e.Ok &&
				!(e.FirstElement is null) &&
				e.FirstElement.LocalName == "subscribed" &&
				GeoClient.IsNamespaceGeoSpatial(e.FirstElement.NamespaceURI))
			{
				this.SubscriptionId = XML.Attribute(e.FirstElement, "id");
				this.Ttl = XML.Attribute(e.FirstElement, "ttl", 0);
			}
			else
				e.Ok = false;
		}

		/// <summary>
		/// Identifier of the subscription.
		/// </summary>
		public string SubscriptionId { get; }

		/// <summary>
		/// Number of seconds before the subscription gets automatically unsubscribed, unless updated.
		/// </summary>
		public int Ttl { get; }
	}
}
