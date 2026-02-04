using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml.Attributes;

namespace Waher.Security.WAF.Model.Conditions
{
	/// <summary>
	/// Abstract base class for certificate Web Application Firewall conditions.
	/// </summary>
	public abstract class WafCertificateCondition : WafCondition
	{
		private readonly BooleanAttribute trust;

		/// <summary>
		/// Abstract base class for certificate Web Application Firewall conditions.
		/// </summary>
		public WafCertificateCondition()
			: base()
		{
		}

		/// <summary>
		/// Abstract base class for Web Application Firewall conditions.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public WafCertificateCondition(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
			this.trust = new BooleanAttribute(Xml, "trust");
		}

		/// <summary>
		/// Gets the value of the trust attribute.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>If a certificate can be trusted.</returns>
		public Task<bool> GetTrust(ProcessingState State)
		{
			return this.trust.EvaluateAsync(State.Variables, false);
		}
	}
}
