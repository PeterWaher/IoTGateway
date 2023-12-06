using System;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Content.Rss
{
	/// <summary>
	/// Globally unique identifier
	/// </summary>
	public class RssGuid
	{
		/// <summary>
		/// Globally unique identifier
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		public RssGuid(XmlElement Xml)
		{
			if (Xml is null)
				throw new ArgumentNullException(nameof(Xml));

			this.Id = Xml.InnerText;
			this.IsPermaLink = XML.Attribute(Xml, "isPermaLink", true);
		}

		/// <summary>
		/// If the guid element has an attribute named isPermaLink with a value of true, the reader 
		/// may assume that it is a permalink to the item, that is, a url that can be opened in a Web 
		/// browser, that points to the full item described by the &lt;item&gt; element.
		/// </summary>
		public bool IsPermaLink { get; }

		/// <summary>
		/// ID of Guid
		/// </summary>
		public string Id { get; }
	}
}
