using System.Xml;

namespace Waher.Security.WAF.Model.Results
{
	/// <summary>
	/// Forbids the request to proceed.
	/// </summary>
	public class Forbid : WafAction
	{
		/// <summary>
		/// Forbids the request to proceed.
		/// </summary>
		public Forbid()
			: base()
		{
		}

		/// <summary>
		/// Forbids the request to proceed.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public Forbid(XmlElement Xml)
			: base(Xml)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(Forbid);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml) => new Forbid(Xml);
	}
}
