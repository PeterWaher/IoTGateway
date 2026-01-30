using System.Xml;

namespace Waher.Security.WAF.Model.Results
{
	/// <summary>
	/// Allows the request to proceed.
	/// </summary>
	public class Allow : WafAction
	{
		/// <summary>
		/// Allows the request to proceed.
		/// </summary>
		public Allow()
			: base()
		{
		}

		/// <summary>
		/// Allows the request to proceed.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public Allow(XmlElement Xml)
			: base(Xml)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(Allow);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml) => new Allow(Xml);
	}
}
