using System.Xml;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Security.WAF.Model.Comparisons
{
	/// <summary>
	/// Abstract base class for rate limit comparisons.
	/// </summary>
	public abstract class RateLimitComparison : LimitComparison
	{
		private readonly Duration duration;

		/// <summary>
		/// Abstract base class for rate limit comparisons.
		/// </summary>
		public RateLimitComparison()
			: base()
		{
		}

		/// <summary>
		/// Abstract base class for rate limit comparisons.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public RateLimitComparison(XmlElement Xml)
			: base(Xml)
		{
			this.duration = XML.Attribute(Xml, "duration", Duration.Zero);
		}

		/// <summary>
		/// Window duration.
		/// </summary>
		public Duration Duration => this.duration;
	}
}