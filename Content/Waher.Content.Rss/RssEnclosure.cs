using System;
using System.Xml;
using Waher.Content.Xml;
using Waher.Runtime.Collections;

namespace Waher.Content.Rss
{
	/// <summary>
	/// Describes content enclosed in an RSS item.
	/// </summary>
	public class RssEnclosure
	{
		/// <summary>
		/// Describes content enclosed in an RSS item.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		/// <param name="BaseUri">Base URI</param>
		public RssEnclosure(XmlElement Xml, Uri BaseUri)
		{
			if (Xml is null)
				throw new ArgumentNullException(nameof(Xml));

			ChunkedList<RssWarning> Warnings = new ChunkedList<RssWarning>();

			string s = XML.Attribute(Xml, "url");
			if (Uri.TryCreate(BaseUri, s, out Uri Url))
				this.Url = Url;
			else
				Warnings.Add(new RssWarning(Xml, "Invalid URL: " + Xml.OuterXml));

			this.Length = XML.Attribute(Xml, "length", 0L);
			if (this.Length <= 0)
				Warnings.Add(new RssWarning(Xml, "Invalid Length: " + Xml.OuterXml));

			this.ContentType = XML.Attribute(Xml, "type");
			if (string.IsNullOrEmpty(this.ContentType))
				Warnings.Add(new RssWarning(Xml, "Invalid Content-Type: " + Xml.OuterXml));

			this.Warnings = Warnings.ToArray();
		}

		/// <summary>
		/// Any warning messages created during parsing.
		/// </summary>
		public RssWarning[] Warnings { get; }

		/// <summary>
		/// URL to the content.
		/// </summary>
		public Uri Url { get; } = null;

		/// <summary>
		/// Length of content.
		/// </summary>
		public long Length { get; }

		/// <summary>
		/// Content-Type of content
		/// </summary>
		public string ContentType { get; }
	}
}
