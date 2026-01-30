using System.Xml;

namespace Waher.Security.WAF.Model.Results
{
	/// <summary>
	/// Ignores the request.
	/// </summary>
	public class Ignore : WafAction
	{
		/// <summary>
		/// Ignores the request.
		/// </summary>
		public Ignore()
			: base()
		{
		}

		/// <summary>
		/// Ignores the request.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public Ignore(XmlElement Xml)
			: base(Xml)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(Ignore);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml) => new Ignore(Xml);
	}
}
