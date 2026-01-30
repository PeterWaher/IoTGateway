using System.Xml;

namespace Waher.Security.WAF.Model.Comparisons
{
	/// <summary>
	/// Checks if the remote endpoint has sent too much bytes.
	/// </summary>
	public class BytesExceeded : RateLimitComparison
	{
		/// <summary>
		/// Checks if the remote endpoint has sent too much bytes.
		/// </summary>
		public BytesExceeded()
			: base()
		{
		}

		/// <summary>
		/// Checks if the remote endpoint has sent too much bytes.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public BytesExceeded(XmlElement Xml)
			: base(Xml)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(BytesExceeded);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml) => new BytesExceeded(Xml);
	}
}
