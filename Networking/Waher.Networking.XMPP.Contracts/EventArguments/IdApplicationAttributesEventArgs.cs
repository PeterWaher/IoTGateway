using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Content.Xml;
using Waher.Networking.XMPP.Events;
using Waher.Runtime.Collections;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
	/// <summary>
	/// Event arguments for callback methods to ID Application attributes queries.
	/// </summary>
	public class IdApplicationAttributesEventArgs : IqResultEventArgs
	{
		private readonly string[] requiredProperties;
		private readonly AuthenticatorService[] authenticatorServices;
		private readonly PeerReviewService[] peerReviewServices;
		private readonly int nrReviewers;
		private readonly int nrPhotos;
		private readonly bool peerReview;
		private readonly bool iso3166;
		private readonly int reviewTimeout;

		/// <summary>
		/// Event arguments for callback methods to ID Application attributes queries.
		/// </summary>
		/// <param name="e">IQ Response</param>
		public IdApplicationAttributesEventArgs(IqResultEventArgs e)
			: base(e)
		{
			if (e.Ok)
			{
				ChunkedList<string> Required = null;
				ChunkedList<AuthenticatorService> AuthenticatorServices = null;
				ChunkedList<PeerReviewService> PeerReviewServices = null;

				this.peerReview = XML.Attribute(e.FirstElement, "peerReview", false);
				this.nrReviewers = XML.Attribute(e.FirstElement, "nrReviewers", 0);
				this.nrPhotos = XML.Attribute(e.FirstElement, "nrPhotos", 0);
				this.iso3166 = XML.Attribute(e.FirstElement, "iso3166", false);
				this.reviewTimeout = XML.Attribute(e.FirstElement, "reviewTimeout", 3600);

				foreach (XmlNode N in e.FirstElement.ChildNodes)
				{
					if (!(N is XmlElement E))
						continue;

					switch (E.LocalName)
					{
						case "required":
							Required ??= new ChunkedList<string>();
							Required.Add(E.InnerText);
							break;

						case "authenticator":
						case "peerReviewService":
							string Id = XML.Attribute(E, "id");
							string Name = XML.Attribute(E, "name");
							string FullName = XML.Attribute(E, "fqn");
							string IconUrl = XML.Attribute(E, "iconUrl");
							int IconWidth = XML.Attribute(E, "iconWidth", 0);
							int IconHeight = XML.Attribute(E, "iconHeight", 0);
							Dictionary<string, bool> Properties = new Dictionary<string, bool>();
							Dictionary<string, bool> Attachments = new Dictionary<string, bool>();

							foreach (XmlNode N2 in E.ChildNodes)
							{
								if (!(N2 is XmlElement E2))
									continue;

								bool IsProperties = E2.LocalName == "properties";

								foreach (XmlNode N3 in E.ChildNodes)
								{
									if (!(N3 is XmlElement E3))
										continue;

									bool IsRequired = E3.LocalName == "required";

									if (IsProperties)
										Properties[E3.InnerText] = IsRequired;
									else
										Attachments[E3.InnerText] = IsRequired;
								}
							}

							if (E.LocalName == "authenticator")
							{
								AuthenticatorServices ??= new ChunkedList<AuthenticatorService>();
								AuthenticatorServices.Add(new AuthenticatorService(Id, Name,
									FullName, IconUrl, IconWidth, IconHeight, Properties,
									Attachments));
							}
							else
							{
								PeerReviewServices ??= new ChunkedList<PeerReviewService>();
								PeerReviewServices.Add(new PeerReviewService(Id, Name,
									FullName, IconUrl, IconWidth, IconHeight, Properties,
									Attachments));
							}
							break;
					}
				}

				this.requiredProperties = Required?.ToArray() ?? Array.Empty<string>();
				this.authenticatorServices = AuthenticatorServices?.ToArray() ?? Array.Empty<AuthenticatorService>();
				this.peerReviewServices = PeerReviewServices?.ToArray() ?? Array.Empty<PeerReviewService>();
			}
			else
			{
				this.peerReview = false;
				this.nrReviewers = 0;
				this.nrPhotos = 0;
				this.iso3166 = false;
				this.reviewTimeout = 3600;
				this.requiredProperties = null;
				this.authenticatorServices = null;
				this.peerReviewServices = null;
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
		/// Number of seconds the client has to sign and add an identity review
		/// attachment to a recently approved identity application.
		/// </summary>
		public int ReviewTimeout => this.reviewTimeout;

		/// <summary>
		/// Required properties in an ID application for peer-review.
		/// </summary>
		public string[] RequiredProperties => this.requiredProperties;

		/// <summary>
		/// Authenticator services available for ID applications.
		/// </summary>
		/// <returns>Authenticator services available for ID applications.</returns>
		public AuthenticatorService[] AuthenticatorServices => this.authenticatorServices;

		/// <summary>
		/// Peer review services available for ID applications.
		/// </summary>
		/// <returns>Peer review services available for ID applications.</returns>
		public PeerReviewService[] PeerReviewServices => this.peerReviewServices;
	}
}
