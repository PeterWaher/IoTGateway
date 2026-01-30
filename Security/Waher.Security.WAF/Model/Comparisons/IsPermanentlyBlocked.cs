using System.Xml;

namespace Waher.Security.WAF.Model.Comparisons
{
	/// <summary>
	/// Checks if the remote endpoint of the request is permanently blocked.
	/// </summary>
	public class IsPermanentlyBlocked : WafComparison
	{
		/// <summary>
		/// Checks if the remote endpoint of the request is permanently blocked.
		/// </summary>
		public IsPermanentlyBlocked()
			: base()
		{
		}

		/// <summary>
		/// Checks if the remote endpoint of the request is permanently blocked.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public IsPermanentlyBlocked(XmlElement Xml)
			: base(Xml)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(IsPermanentlyBlocked);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml) => new IsPermanentlyBlocked(Xml);
	}
}
