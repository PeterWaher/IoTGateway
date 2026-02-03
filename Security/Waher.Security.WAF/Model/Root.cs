using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Networking.HTTP.Interfaces;

namespace Waher.Security.WAF.Model
{
	/// <summary>
	/// Root object of Web Application Firewall rules.
	/// </summary>
	public class Root : WafActions
	{
		private readonly WafResult defaultResult;

		/// <summary>
		/// Root object of Web Application Firewall rules.
		/// </summary>
		public Root()
			: base()
		{
		}

		/// <summary>
		/// Root object of Web Application Firewall rules.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Document">Web Application Firewall document.</param>
		public Root(XmlElement Xml, WebApplicationFirewall Document)
			: base(Xml, null, Document)
		{
			this.defaultResult = XML.Attribute(Xml, "defaultResult", WafResult.Allow);
		}

		/// <summary>
		/// Default result.
		/// </summary>
		public WafResult DefaultResult => this.defaultResult;

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(Root);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new Root(Xml, Document);

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Result to return, if any.</returns>
		public override Task<WafResult?> Review(ProcessingState State)
		{
			return this.ReviewChildren(State);
		}
	}
}
