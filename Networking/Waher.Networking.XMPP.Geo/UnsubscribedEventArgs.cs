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
				!(e.FirstElement is null) &&
				e.FirstElement.LocalName == "unsubscribed" &&
				GeoClient.IsNamespaceGeoSpatial(e.FirstElement.NamespaceURI))
			{
				this.SubscriptionId = XML.Attribute(e.FirstElement, "id");
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
