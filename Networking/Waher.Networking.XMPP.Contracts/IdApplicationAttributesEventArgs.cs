using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Delegate for ID Application attributes callback methods.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments</param>
	public delegate Task IdApplicationAttributesEventHandler(object Sender, IdApplicationAttributesEventArgs e);

	/// <summary>
	/// Event arguments for callback methods to ID Application attributes queries.
	/// </summary>
	public class IdApplicationAttributesEventArgs : IqResultEventArgs
	{
		private readonly string[] requiredProperties;
		private readonly int nrReviewers;
		private readonly int nrPhotos;
		private readonly bool peerReview;
		private readonly bool iso3166;

		/// <summary>
		/// Event arguments for callback methods to ID Application attributes queries.
		/// </summary>
		/// <param name="e">IQ Response</param>
		public IdApplicationAttributesEventArgs(IqResultEventArgs e)
			: base(e)
		{
			if (e.Ok)
			{
				this.peerReview = XML.Attribute(e.FirstElement, "peerReview", false);
				this.nrReviewers = XML.Attribute(e.FirstElement, "nrReviewers", 0);
				this.nrPhotos = XML.Attribute(e.FirstElement, "nrPhotos", 0);
				this.iso3166 = XML.Attribute(e.FirstElement, "iso3166", false);

				List<string> Required = new List<string>();

				foreach (XmlNode N2 in e.FirstElement.ChildNodes)
				{
					if (N2 is XmlElement E && E.LocalName == "required")
						Required.Add(E.InnerText);
				}

				this.requiredProperties = Required.ToArray();
			}
			else
			{
				this.peerReview = false;
				this.nrReviewers = 0;
				this.nrPhotos = 0;
				this.iso3166 = false;
				this.requiredProperties = null;
			}
		}

		/// <summary>
		/// If peer-review is allowed as a mechanism to approve ID applications.
		/// </summary>
		public bool PeerReview => this.peerReview;

		/// <summary>
		/// Number of peer reviewers required to get an ID approved using peer review.
		/// </summary>
		public int NrReviewers => this.nrReviewers;

		/// <summary>
		/// Number of photos required in a peer-review.
		/// </summary>
		public int NrPhotos => this.nrPhotos;

		/// <summary>
		/// If ISO 3166 country codes are mandated in peer-review.
		/// </summary>
		public bool Iso3166 => this.iso3166;

		/// <summary>
		/// Required properties in an ID application for peer-review.
		/// </summary>
		public string[] RequiredProperties => this.requiredProperties;
	}
}
