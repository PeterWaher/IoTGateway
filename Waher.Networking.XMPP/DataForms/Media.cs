using System;
using System.Collections.Generic;
using System.Xml;

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
		private KeyValuePair<string, Uri>[] uris;
		private int? width;
		private int? height;

		internal Media(XmlElement E)
		{
			if (E.HasAttribute("width"))
				this.width = XmppClient.XmlAttribute(E, "width", 0);
			else
				this.width = null;

			if (E.HasAttribute("height"))
				this.height = XmppClient.XmlAttribute(E, "height", 0);
			else
				this.height = null;

			List<KeyValuePair<string, Uri>> URIs = new List<KeyValuePair<string, Uri>>();
			Uri URI;
			string Type;

			foreach (XmlNode N in E.ChildNodes)
			{
				if (N.LocalName == "uri")
				{
					Type = XmppClient.XmlAttribute((XmlElement)N, "type");
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
		}

		/// <summary>
		/// An array of (Content Type,URI) pairs pointing to media content.
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

	}
}
