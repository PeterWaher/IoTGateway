using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Content.Rss
{
	/// <summary>
	/// Contains information from an RSS feed.
	/// </summary>
	public class RssDocument
	{
		private readonly XmlDocument xml;

		/// <summary>
		/// Contains information from an RSS feed.
		/// </summary>
		/// <param name="Xml">RSS XML document.</param>
		/// <param name="BaseUri">Base Uri</param>
		public RssDocument(XmlDocument Xml, Uri BaseUri)
		{
			if (Xml is null)
				throw new ArgumentNullException(nameof(Xml));

			if (Xml.DocumentElement is null)
				throw new ArgumentException("Missing XML.", nameof(Xml));

			if (Xml.DocumentElement.LocalName != "rss")
				throw new ArgumentException("Not an RSS document.", nameof(Xml));

			this.xml = Xml;
			this.Version = XML.Attribute(Xml.DocumentElement, "version", 0.0);

			List<RssChannel> Channels = new List<RssChannel>();
			List<RssWarning> Warnings = new List<RssWarning>();

			foreach (XmlNode N in Xml.DocumentElement.ChildNodes)
			{
				if (!(N is XmlElement E))
					continue;

				if (E.LocalName == "channel" && E.NamespaceURI == Xml.NamespaceURI)
				{
					RssChannel Channel = new RssChannel(E, BaseUri);
					Channels.Add(Channel);
					Warnings.AddRange(Channel.Warnings);
				}
				else
					Warnings.Add(new RssWarning(E));
			}

			this.Channels = Channels.ToArray();
			this.Warnings = Warnings.ToArray();
		}

		/// <summary>
		/// RSS XML document.
		/// </summary>
		public XmlDocument Xml => this.xml;

		/// <summary>
		/// Version of RSS document.
		/// </summary>
		public double Version { get; }

		/// <summary>
		/// Channels
		/// </summary>
		public RssChannel[] Channels { get; }

		/// <summary>
		/// Any warning messages created during parsing.
		/// </summary>
		public RssWarning[] Warnings { get; }
	}
}
