using System.Threading.Tasks;
using System.Xml;
using Waher.Networking.HTTP.Interfaces;
using Waher.Persistence;
using Waher.Persistence.Serialization;

namespace Waher.Security.WAF.Model.Actions
{
	/// <summary>
	/// Adds a record to the Open Intelligence database
	/// </summary>
	public class AddOpenIntelligence : WafActionOpenIntelligence
	{
		/// <summary>
		/// Adds a record to the Open Intelligence database
		/// </summary>
		public AddOpenIntelligence()
			: base()
		{
		}

		/// <summary>
		/// Adds a record to the Open Intelligence database
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public AddOpenIntelligence(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(AddOpenIntelligence);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new AddOpenIntelligence(Xml, Parent, Document);

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Result to return, if any.</returns>
		public override async Task<WafResult?> Review(ProcessingState State)
		{
			GenericObject Obj = await this.EvaluateObject(State);

			await Database.Insert(Obj);

			return null;
		}
	}
}
