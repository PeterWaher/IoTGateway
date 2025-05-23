using Waher.Content.Xml;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Geo
{
	/// <summary>
	/// Event arguments for the result of an unsubscription request.
	/// </summary>
	public class UnsubscribedEventArgs : IqResultEventArgs
	{
		/// <summary>
		/// Event arguments for the result of an unsubscription request.
		/// </summary>
		/// <param name="e">Response</param>
		public UnsubscribedEventArgs(IqResultEventArgs e)
			: base(e)
		{
			if (e.Ok &&
				!(e.Response is null) &&
				e.Response.LocalName == "unsubscribed" &&
				e.Response.NamespaceURI == GeoClient.NamespaceGeoSpatialNeuroFoundationV1)
			{
				this.SubscriptionId = XML.Attribute(e.Response, "id");
			}
			else
				e.Ok = false;
		}

		/// <summary>
		/// Identifier of the subscription.
		/// </summary>
		public string SubscriptionId { get; }
	}
}
