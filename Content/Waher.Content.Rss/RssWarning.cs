using System.Xml;

namespace Waher.Content.Rss
{
	/// <summary>
	/// Contains a warning message.
	/// </summary>
	public class RssWarning
	{
		/// <summary>
		/// Contains a warning message.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		public RssWarning(XmlElement Xml)
			: this(Xml, "Element not recognized: " + Xml.OuterXml)
		{
		}

		/// <summary>
		/// Contains a warning message.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		/// <param name="Message">Warning message.</param>
		public RssWarning(XmlElement Xml, string Message)
		{
			this.Xml = Xml;
			this.Message = Message;
		}

		/// <summary>
		/// XML Definition
		/// </summary>
		public XmlElement Xml { get; }

		/// <summary>
		/// Warning message.
		/// </summary>
		public string Message { get; }

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.Message;
		}
	}
}
