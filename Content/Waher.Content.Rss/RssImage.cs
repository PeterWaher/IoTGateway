using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Content.Rss
{
	/// <summary>
	/// Specifies a GIF, JPEG or PNG image that can be displayed with the channel.
	/// </summary>
	public class RssImage
	{
		/// <summary>
		/// Specifies a GIF, JPEG or PNG image that can be displayed with the channel.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		/// <param name="BaseUri">Base URI</param>
		public RssImage(XmlElement Xml, Uri BaseUri)
		{
			if (Xml is null)
				throw new ArgumentNullException(nameof(Xml));

			List<RssWarning> Warnings = new List<RssWarning>();

			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (!(N is XmlElement E))
					continue;

				if (E.NamespaceURI == Xml.NamespaceURI)
				{
					switch (E.LocalName)
					{
						case "url":
							if (Uri.TryCreate(BaseUri, E.InnerText, out Uri Url))
								this.Url = Url;
							else
								Warnings.Add(new RssWarning(E, "Invalid URI: " + E.InnerText));
							break;

						case "title":
							this.Title = E.InnerText;
							break;

						case "link":
							if (Uri.TryCreate(BaseUri, E.InnerText, out Uri Link))
								this.Link = Link;
							else
								Warnings.Add(new RssWarning(E, "Invalid URI: " + E.InnerText));
							break;

						case "width":
							if (int.TryParse(E.InnerText, out int Width) && Width > 0 && Width <= 144)
								this.Width = Width;
							else
								Warnings.Add(new RssWarning(E, "Invalid Width: " + E.InnerText));
							break;

						case "height":
							if (int.TryParse(E.InnerText, out int Height) && Height > 0 && Height <= 400)
								this.Height = Height;
							else
								Warnings.Add(new RssWarning(E, "Invalid Height: " + E.InnerText));
							break;

						case "description":
							this.Description = E.InnerText;
							break;

						default:
							Warnings.Add(new RssWarning(E));
							break;
					}
				}
				else
					Warnings.Add(new RssWarning(E));
			}

			this.Warnings = Warnings.ToArray();
		}

		/// <summary>
		/// Any warning messages created during parsing.
		/// </summary>
		public RssWarning[] Warnings { get; }

		/// <summary>
		/// URL of a GIF, JPEG or PNG image that represents the channel.
		/// </summary>
		public Uri Url { get; } = null;

		/// <summary>
		/// Describes the image, it's used in the ALT attribute of the HTML &lt;img&gt; tag when the 
		/// channel is rendered in HTML.
		/// </summary>
		public string Title { get; }

		/// <summary>
		/// The URL of the site, when the channel is rendered, the image is a link to the 
		/// site. (Note, in practice the image &lt;title&gt; and &lt;link&gt; should have the same value as 
		/// the channel's &lt;title&gt; and &lt;link&gt;.
		/// </summary>
		public Uri Link { get; } = null;

		/// <summary>
		/// Width of image in pixels.
		/// </summary>
		public int Width { get; } = 88;

		/// <summary>
		/// Height of image in pixels.
		/// </summary>
		public int Height { get; } = 31;

		/// <summary>
		/// Contains text that is included in the TITLE attribute of the link formed around the 
		/// image in the HTML rendering.
		/// </summary>
		public string Description { get; }
	}
}
