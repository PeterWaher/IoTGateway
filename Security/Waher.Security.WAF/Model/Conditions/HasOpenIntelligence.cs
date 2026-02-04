using System.Threading.Tasks;
using System.Xml;
using Waher.Networking.HTTP.Interfaces;
using Waher.Persistence.Serialization;
using Waher.Security.WAF.Model.Actions;

namespace Waher.Security.WAF.Model.Conditions
{
	/// <summary>
	/// Checks if an endpoint has an Open Intelligence record associated with it.
	/// </summary>
	public class HasOpenIntelligence : WafActionOpenIntelligence
	{
		/// <summary>
		/// Checks if an endpoint has an Open Intelligence record associated with it.
		/// </summary>
		public HasOpenIntelligence()
			: base()
		{
		}

		/// <summary>
		/// Checks if an endpoint has an Open Intelligence record associated with it.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public HasOpenIntelligence(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(HasOpenIntelligence);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new HasOpenIntelligence(Xml, Parent, Document);

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Result to return, if any.</returns>
		public override async Task<WafResult?> Review(ProcessingState State)
		{
			GenericObject[] Objects = await this.FindObjects(State);

			if (Objects.Length > 0)
				return await this.ReviewChildren(State);
			else
				return null;
		}
	}
}
