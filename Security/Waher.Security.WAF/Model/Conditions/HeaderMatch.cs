using System.Threading.Tasks;
using System.Xml;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.Interfaces;

namespace Waher.Security.WAF.Model.Conditions
{
	/// <summary>
	/// Checks for a match against the Header of the request.
	/// </summary>
	public class HeaderMatch : WafNamedCondition
	{
		/// <summary>
		/// Checks for a match against the Header of the request.
		/// </summary>
		public HeaderMatch()
			: base()
		{
		}

		/// <summary>
		/// Checks for a match against the Header of the request.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public HeaderMatch(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(HeaderMatch);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new HeaderMatch(Xml, Parent, Document);

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Result to return, if any.</returns>
		public override Task<WafResult?> Review(ProcessingState State)
		{
			if (State.Request.Header.TryGetHeaderField(this.Name, out HttpField Field))
				return this.Review(State, Field.Value);
			else
				return Task.FromResult<WafResult?>(null);
		}
	}
}
