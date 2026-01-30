using System.Xml;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Security.WAF.Model.Comparisons
{
	/// <summary>
	/// Abstract base class for limit comparisons.
	/// </summary>
	public abstract class LimitComparison : WafComparison
	{
		private readonly Duration duration;
		private readonly long limit;

		/// <summary>
		/// Abstract base class for limit comparisons.
		/// </summary>
		public LimitComparison()
			: base()
		{
		}

		/// <summary>
		/// Abstract base class for limit comparisons.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public LimitComparison(XmlElement Xml)
			: base(Xml)
		{
			this.limit = XML.Attribute(Xml, "limit", 0L);
		}

		/// <summary>
		/// Rate limit during <see cref="Duration"/>.
		/// </summary>
		public long Limit => this.limit;
	}
}