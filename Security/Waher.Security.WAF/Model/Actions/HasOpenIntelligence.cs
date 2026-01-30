using System.Xml;

namespace Waher.Security.WAF.Model.Actions
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
		public HasOpenIntelligence(XmlElement Xml)
			: base(Xml)
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
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml) => new HasOpenIntelligence(Xml);
	}
}
