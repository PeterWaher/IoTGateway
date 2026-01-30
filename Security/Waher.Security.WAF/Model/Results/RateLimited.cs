using System.Xml;

namespace Waher.Security.WAF.Model.Results
{
	/// <summary>
	/// Reports the resource is rate limited.
	/// </summary>
	public class RateLimited : WafAction
	{
		/// <summary>
		/// Reports the resource is rate limited.
		/// </summary>
		public RateLimited()
			: base()
		{
		}

		/// <summary>
		/// Reports the resource is rate limited.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public RateLimited(XmlElement Xml)
			: base(Xml)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(RateLimited);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml) => new RateLimited(Xml);
	}
}
