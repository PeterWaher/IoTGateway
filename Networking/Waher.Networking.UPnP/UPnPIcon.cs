using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Networking.UPnP
{
	/// <summary>
	/// Contains information about an icon.
	/// </summary>
	public class UPnPIcon
	{
		private readonly XmlElement xml;
		private readonly Uri uri;
		private readonly string mimetype;
		private readonly string url;
		private readonly int width;
		private readonly int height;
		private readonly int depth;

		internal UPnPIcon(XmlElement Xml, Uri BaseUri)
		{
			this.xml = Xml;

			foreach (XmlNode N in Xml.ChildNodes)
			{
				switch (N.LocalName)
				{
					case "mimetype":
						this.mimetype = N.InnerText;
						break;

					case "width":
						this.width = int.Parse(N.InnerText);
						break;

					case "height":
						this.height = int.Parse(N.InnerText);
						break;

					case "depth":
						this.depth = int.Parse(N.InnerText);
						break;

					case "url":
						this.url = N.InnerText;
						this.uri = new Uri(BaseUri, this.url);
						break;
				}
			}
		}

		/// <summary>
		/// Underlying XML definition.
		/// </summary>
		public XmlElement Xml
		{
			get { return this.xml; }
		}

		/// <summary>
		/// URI to image
		/// </summary>
		public Uri Uri { get { return this.uri; } }

		/// <summary>
		/// Internet Content Type
		/// </summary>
		public string Mimetype { get { return this.mimetype; } }

		/// <summary>
		/// URL to image
		/// </summary>
		public string Url { get { return this.url; } }

		/// <summary>
		/// Width of icon
		/// </summary>
		public int Width { get { return this.width; } }

		/// <summary>
		/// Height of icon
		/// </summary>
		public int Height { get { return this.height; } }

		/// <summary>
		/// Color depth of icon
		/// </summary>
		public int Depth { get { return this.depth; } }

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.url;
		}
	}
}
