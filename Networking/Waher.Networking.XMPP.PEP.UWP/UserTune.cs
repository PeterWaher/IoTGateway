using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.PEP
{
	/// <summary>
	/// User Tune event, as defined in XEP-0118:
	/// https://xmpp.org/extensions/xep-0118.html
	/// </summary>
	public class UserTune : PersonalEvent
	{
		private string artist = null;
		private string source = null;
		private string title = null;
		private string track = null;
		private Uri uri = null;
		private ushort? length = null;
		private byte? rating = null;

		/// <summary>
		/// User Tune event, as defined in XEP-0118:
		/// https://xmpp.org/extensions/xep-0118.html
		/// </summary>
		public UserTune()
		{
		}

		/// <summary>
		/// Local name of the event element.
		/// </summary>
		public override string LocalName => "tune";

		/// <summary>
		/// Namespace of the event element.
		/// </summary>
		public override string Namespace => "http://jabber.org/protocol/tune";

		/// <summary>
		/// XML representation of the event.
		/// </summary>
		public override string PayloadXml
		{
			get
			{
				StringBuilder Xml = new StringBuilder();

				Xml.Append("<tune xmlns='");
				Xml.Append(this.Namespace);
				Xml.Append("'>");

				this.Append(Xml, "artist", this.artist);
				this.Append(Xml, "length", this.length);
				this.Append(Xml, "rating", this.rating);
				this.Append(Xml, "source", this.source);
				this.Append(Xml, "title", this.title);
				this.Append(Xml, "track", this.track);
				this.Append(Xml, "uri", this.uri?.ToString());

				Xml.Append("</tune>");

				return Xml.ToString();
			}
		}

		/// <summary>
		/// Parses a personal event from its XML representation
		/// </summary>
		/// <param name="E">XML representation of personal event.</param>
		/// <returns>Personal event object.</returns>
		public override IPersonalEvent Parse(XmlElement E)
		{
			UserTune Result = new UserTune();

			foreach (XmlNode N in E.ChildNodes)
			{
				if (N is XmlElement E2)
				{
					switch (E2.LocalName)
					{
						case "artist":
							Result.artist = E2.InnerText;
							break;

						case "length":
							if (ushort.TryParse(E2.InnerText, out ushort us))
								Result.length = us;
							break;

						case "rating":
							if (byte.TryParse(E2.InnerText, out byte b))
								Result.rating = b;
							break;

						case "source":
							Result.source = E2.InnerText;
							break;

						case "title":
							Result.title = E2.InnerText;
							break;

						case "track":
							Result.track = E2.InnerText;
							break;

						case "uri":
							try
							{
								Result.uri = new Uri(E2.InnerText);
							}
							catch (Exception)
							{
								// Ignore.
							}
							break;
					}
				}
			}

			return Result;
		}

		/// <summary>
		/// The artist or performer of the song or piece
		/// </summary>
		public string Artist
		{
			get => this.artist;
			set => this.artist = value;
		}

		/// <summary>
		/// The collection (e.g., album) or other source (e.g., a band website that hosts streams or audio files)
		/// </summary>
		public string Source
		{
			get => this.source;
			set => this.source = value;
		}

		/// <summary>
		/// The title of the song or piece
		/// </summary>
		public string Title
		{
			get => this.title;
			set => this.title = value;
		}

		/// <summary>
		/// A unique identifier for the tune; e.g., the track number within a collection or the specific URI for the object (e.g., a stream or audio file)
		/// </summary>
		public string Track
		{
			get => this.track;
			set => this.track = value;
		}

		/// <summary>
		/// A URI or URL pointing to information about the song, collection, or artist
		/// </summary>
		public Uri Uri
		{
			get => this.uri;
			set => this.uri = value;
		}

		/// <summary>
		/// The duration of the song or piece in seconds
		/// </summary>
		public ushort? Length
		{
			get => this.length;
			set => this.length = value;
		}

		/// <summary>
		/// The user's rating of the song or piece, from 1 (lowest) to 10 (highest).
		/// </summary>
		public byte? Rating
		{
			get => this.rating;
			set => this.rating = value;
		}

	}
}
