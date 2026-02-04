using System.Threading.Tasks;
using System.Xml;
using Waher.Networking.HTTP.Interfaces;

namespace Waher.Security.WAF.Model.Results
{
	/// <summary>
	/// Reports the resource could not be found.
	/// </summary>
	public class NotFound : WafAction
	{
		/// <summary>
		/// Reports the resource could not be found.
		/// </summary>
		public NotFound()
			: base()
		{
		}

		/// <summary>
		/// Reports the resource could not be found.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public NotFound(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(NotFound);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new NotFound(Xml, Parent, Document);

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Result to return, if any.</returns>
		public override Task<WafResult?> Review(ProcessingState State)
		{
			return Task.FromResult<WafResult?>(WafResult.NotFound);
		}
	}
}
