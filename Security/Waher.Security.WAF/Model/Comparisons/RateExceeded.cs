using System.Xml;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Security.WAF.Model.Comparisons
{
	/// <summary>
	/// Abstract base class for rate comparisons.
	/// </summary>
	public abstract class RateExceeded : WafComparison
	{
		private readonly Duration duration;
		private readonly long limit;

		/// <summary>
		/// Abstract base class for rate comparisons.
		/// </summary>
		public RateExceeded()
			: base()
		{
		}

		/// <summary>
		/// Abstract base class for rate comparisons.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public RateExceeded(XmlElement Xml)
			: base(Xml)
		{
			this.duration = XML.Attribute(Xml, "duration", Duration.Zero);
			this.limit = XML.Attribute(Xml, "limit", 0L);
		}

		/// <summary>
		/// Window duration.
		/// </summary>
		public Duration Duration => this.duration;

		/// <summary>
		/// Rate limit during <see cref="Duration"/>.
		/// </summary>
		public long Limit => this.limit;
	}
}