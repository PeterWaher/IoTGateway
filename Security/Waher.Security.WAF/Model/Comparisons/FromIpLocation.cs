using System.Xml;
using Waher.Content.Xml;

namespace Waher.Security.WAF.Model.Comparisons
{
	/// <summary>
	/// Checks if the requests come from a specific location, using available locale 
	/// information derived from the IP address of the caller.
	/// </summary>
	public class FromIpLocation : WafComparison
	{
		private readonly string country;
		private readonly string region;
		private readonly string city;
		private readonly string area;

		/// <summary>
		/// Checks if the requests come from a specific location, using available locale 
		/// information derived from the IP address of the caller.
		/// </summary>
		public FromIpLocation()
			: base()
		{
		}

		/// <summary>
		/// Checks if the requests come from a specific location, using available locale 
		/// information derived from the IP address of the caller.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public FromIpLocation(XmlElement Xml)
			: base(Xml)
		{
			this.country = XML.Attribute(Xml, "country");
			this.region = XML.Attribute(Xml, "region");
			this.city = XML.Attribute(Xml, "city");
			this.area = XML.Attribute(Xml, "area");
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(FromIpLocation);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml) => new FromIpLocation(Xml);
	}
}
