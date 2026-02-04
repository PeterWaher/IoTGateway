using System.Threading.Tasks;
using System.Xml;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.Interfaces;

namespace Waher.Security.WAF.Model.Conditions
{
	/// <summary>
	/// Checks for a match against the Certificate Subject of the request.
	/// </summary>
	public class CertificateSubjectMatch : WafCertificateCondition
	{
		/// <summary>
		/// Checks for a match against the Certificate Subject of the request.
		/// </summary>
		public CertificateSubjectMatch()
			: base()
		{
		}

		/// <summary>
		/// Checks for a match against the Certificate Subject of the request.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public CertificateSubjectMatch(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(CertificateSubjectMatch);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new CertificateSubjectMatch(Xml, Parent, Document);

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Result to return, if any.</returns>
		public override async Task<WafResult?> Review(ProcessingState State)
		{
			HttpRequest Request = State.Request;

			if (!(Request.RemoteCertificate is null) && (Request.RemoteCertificateValid || await this.GetTrust(State)))
				return await this.Review(State, Request.RemoteCertificate.Subject);
			else
				return null;
		}
	}
}
