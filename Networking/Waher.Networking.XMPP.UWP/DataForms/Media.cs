using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.DataForms
{
	/// <summary>
	/// Class containing information about media content in a data form.
	/// 
	/// Specified in XEP-0221:
	///
	/// XEP-0221: Data Forms Media Element
	/// http://xmpp.org/extensions/xep-0221.html
	/// </summary>
	public class Media
	{
		private readonly KeyValuePair<string, Uri>[] uris;
		private readonly int? width;
		private readonly int? height;
		private string contentType;
		private byte[] bin = null;
		private string url = null;

		/// <summary>
		/// Class containing information about media content in a data form.
		/// 
		/// Specified in XEP-0221:
		///
		/// XEP-0221: Data Forms Media Element
		/// http://xmpp.org/extensions/xep-0221.html
		/// </summary>
		/// <param name="Url">Media URL</param>
		/// <param name="ContentType">Content-Type</param>
		public Media(string Url, string ContentType)
			: this(Url, ContentType, null, null)
		{
		}

		/// <summary>
		/// Class containing information about media content in a data form.
		/// 
		/// Specified in XEP-0221:
		///
		/// XEP-0221: Data Forms Media Element
		/// http://xmpp.org/extensions/xep-0221.html
		/// </summary>
		/// <param name="Url">Media URL</param>
		/// <param name="ContentType">Content-Type</param>
		/// <param name="Width">Width</param>
		/// <param name="Height">Height</param>
		public Media(string Url, string ContentType, int? Width, int? Height)
		{
			this.uris = new KeyValuePair<string, Uri>[] { new KeyValuePair<string, Uri>(ContentType, new Uri(Url)) };
			this.width = Width;
			this.height = Height;
			this.contentType = ContentType;
			this.url = Url;
		}

		internal Media(XmlElement E)
		{
			if (E.HasAttribute("width"))
				this.width = XML.Attribute(E, "width", 0);
			else
				this.width = null;

			if (E.HasAttribute("height"))
				this.height = XML.Attribute(E, "height", 0);
			else
				this.height = null;

			List<KeyValuePair<string, Uri>> URIs = new List<KeyValuePair<string, Uri>>();
			Uri URI;
			string Type;

			foreach (XmlNode N in E.ChildNodes)
			{
				if (N.LocalName == "uri")
				{
					Type = XML.Attribute((XmlElement)N, "type");
					try
					{
						URI = new Uri(N.InnerText);
					}
					catch (Exception)
					{
						continue;
					}

					URIs.Add(new KeyValuePair<string, Uri>(Type, URI));
				}
			}

			this.uris = URIs.ToArray();
			this.contentType = null;
		}

		/// <summary>
		/// An array of (Content-Type, URI) pairs pointing to media content.
		/// </summary>
		public KeyValuePair<string, Uri>[] URIs { get { return this.uris; } }

		/// <summary>
		/// Width, if specified, null if unspecified.
		/// </summary>
		public int? Width { get { return this.width; } }

		/// <summary>
		/// Height, if specified, null if unspecified.
		/// </summary>
		public int? Height { get { return this.height; } }

		/// <summary>
		/// Binary content, if available in the form.
		/// </summary>
		public byte[] Binary
		{
			get { return this.bin; }
			internal set { this.bin = value; }
		}

		/// <summary>
		/// Content-Type of data, if available.
		/// </summary>
		public string ContentType
		{
			get { return this.contentType; }
			internal set { this.contentType = value; }
		}

		/// <summary>
		/// Any web-based URL for the media.
		/// </summary>
		public string URL
		{
			get { return this.url; }
			internal set { this.url = value; }
		}

		internal void AnnotateField(StringBuilder Output, bool ValuesOnly, bool IncludeLabels)
		{
			Output.Append("<media xmlns='urn:xmpp:media-element'");

			if (this.height.HasValue)
			{
				Output.Append(" height='");
				Output.Append(this.height.Value.ToString());
				Output.Append('\'');
			}

			if (this.width.HasValue)
			{
				Output.Append(" width='");
				Output.Append(this.width.Value.ToString());
				Output.Append('\'');
			}

			Output.Append('>');

			foreach (KeyValuePair<string, Uri> Uri in this.uris)
			{
				Output.Append("<uri type='");
				Output.Append(XML.Encode(Uri.Key));
				Output.Append("'>");
				Output.Append(XML.Encode(Uri.Value.ToString()));
				Output.Append("</uri>");
			}

			Output.Append("</media>");
		}

	}
}
