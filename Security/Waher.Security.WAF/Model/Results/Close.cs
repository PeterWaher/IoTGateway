using System.Xml;

namespace Waher.Security.WAF.Model.Results
{
	/// <summary>
	/// Closes the connection.
	/// </summary>
	public class Close : WafAction
	{
		/// <summary>
		/// Closes the connection.
		/// </summary>
		public Close()
			: base()
		{
		}

		/// <summary>
		/// Closes the connection.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public Close(XmlElement Xml)
			: base(Xml)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(Close);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml) => new Close(Xml);
	}
}
