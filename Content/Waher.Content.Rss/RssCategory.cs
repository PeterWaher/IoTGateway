using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Content.Rss
{
	/// <summary>
	/// Identifies a categorization taxonomy.
	/// </summary>
	public class RssCategory
	{
		/// <summary>
		/// Identifies a categorization taxonomy.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		/// <param name="BaseUri">Base URI</param>
		public RssCategory(XmlElement Xml, Uri BaseUri)
		{
			if (Xml is null)
				throw new ArgumentNullException(nameof(Xml));

			List<RssWarning> Warnings = new List<RssWarning>();

			this.Name = Xml.InnerText;

			if (Xml.HasAttribute("url"))
			{
				string s = XML.Attribute(Xml, "url");
				if (Uri.TryCreate(BaseUri, s, out Uri Url))
					this.Domain = Url;
				else
					Warnings.Add(new RssWarning(Xml, "Invalid URI: " + s));
			}
			else
				this.Domain = null;

			this.Warnings = Warnings.ToArray();
		}

		/// <summary>
		/// Any warning messages created during parsing.
		/// </summary>
		public RssWarning[] Warnings { get; }

		/// <summary>
		/// Domain URL.
		/// </summary>
		public Uri Domain { get; } = null;

		/// <summary>
		/// Name of category
		/// </summary>
		public string Name { get; }
	}
}
