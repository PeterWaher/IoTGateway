using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Networking.HTTP.Interfaces;

namespace Waher.Security.WAF.Model.Comparisons
{
	/// <summary>
	/// Checks if the request is encrypted.
	/// </summary>
	public class IsEncrypted : WafComparison
	{
		private readonly int minSecurityStrength;

		/// <summary>
		/// Checks if the request is encrypted.
		/// </summary>
		public IsEncrypted()
			: base()
		{
		}

		/// <summary>
		/// Checks if the request is encrypted.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public IsEncrypted(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
			this.minSecurityStrength = XML.Attribute(Xml, "minSecurityStrength", 0);
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(IsEncrypted);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new IsEncrypted(Xml, Parent, Document);

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Result to return, if any.</returns>
		public override Task<WafResult?> Review(ProcessingState State)
		{
			if (State.Request.Encrypted && State.Request.CipherStrength >= this.minSecurityStrength)
				return this.ReviewChildren(State);
			else
				return Task.FromResult<WafResult?>(null);
		}
	}
}
