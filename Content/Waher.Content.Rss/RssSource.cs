using System;
using System.Xml;
using Waher.Content.Xml;
using Waher.Runtime.Collections;

namespace Waher.Content.Rss
{
	/// <summary>
	/// RSS channel that the item came from.
	/// </summary>
	public class RssSource
	{
		/// <summary>
		/// RSS channel that the item came from.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		/// <param name="BaseUri">Base URI</param>
		public RssSource(XmlElement Xml, Uri BaseUri)
		{
			if (Xml is null)
				throw new ArgumentNullException(nameof(Xml));

			ChunkedList<RssWarning> Warnings = new ChunkedList<RssWarning>();

			this.Title = Xml.InnerText;

			if (Xml.HasAttribute("url"))
			{
				string s = XML.Attribute(Xml, "url");
				if (Uri.TryCreate(BaseUri, s, out Uri Url))
					this.Url = Url;
				else
					Warnings.Add(new RssWarning(Xml, "Invalid URI: " + s));
			}
			else
				this.Url = null;

			this.Warnings = Warnings.ToArray();
		}

		/// <summary>
		/// Any warning messages created during parsing.
		/// </summary>
		public RssWarning[] Warnings { get; }

		/// <summary>
		/// URL to the source.
		/// </summary>
		public Uri Url { get; } = null;

		/// <summary>
		/// Title of the source.
		/// </summary>
		public string Title { get; }
	}
}
