using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.CoAP.CoRE
{
	/// <summary>
	/// CoRE Link Document, as defined in RFC 6690: https://tools.ietf.org/html/rfc6690
	/// </summary>
	public class LinkDocument
	{
		private Link[] links;
		private Uri baseUri;
		private string text;
		private int pos;
		private int len;

		/// <summary>
		/// CoRE Link Document, as defined in RFC 6690: https://tools.ietf.org/html/rfc6690
		/// </summary>
		/// <param name="Text">Text representation of document.</param>
		/// <param name="BaseUri">Base URI.</param>
		public LinkDocument(string Text, Uri BaseUri)
		{
			this.text = Text;
			this.baseUri = BaseUri;
			this.len = Text.Length;
			this.pos = 0;

			this.links = this.ParseLinkValueList();

			if (this.pos < this.len)
				throw new ArgumentException("Unexpected character found at position " + this.pos.ToString());
		}

		/// <summary>
		/// Links in link document.
		/// </summary>
		public Link[] Links
		{
			get { return this.links; }
		}

		/// <summary>
		/// Base URI
		/// </summary>
		public Uri BaseUri
		{
			get { return this.baseUri; }
		}

		private char NextChar()
		{
			if (this.pos >= this.len)
				return (char)0;
			else
				return this.text[this.pos++];
		}

		private char NextNonWhitespaceChar()
		{
			char ch;

			do
			{
				ch = this.NextChar();
				if (ch == 0)
					return ch;
			}
			while (ch <= ' ' || ch == 160);

			return ch;
		}

		private char PeekNextNonWhitespaceChar()
		{
			char ch = this.NextNonWhitespaceChar();
			if (ch != 0)
				this.pos--;

			return ch;
		}

		private void SkipWhitespace()
		{
			char ch;

			while (this.pos < this.len && ((ch = this.text[this.pos]) <= ' ' || ch == 160))
				this.pos++;
		}

		private Link[] ParseLinkValueList()
		{
			List<Link> Links = new List<Link>();
			Link Link;
			char ch;

			Link = this.ParseLinkValue();
			if (Link != null)
				Links.Add(Link);

			ch = this.NextNonWhitespaceChar();
			while (ch == ',')
			{
				Link = this.ParseLinkValue();
				if (Link != null)
					Links.Add(Link);

				ch = this.NextNonWhitespaceChar();
			}

			return Links.ToArray();
		}

		private Link ParseLinkValue()
		{
			char ch = this.NextNonWhitespaceChar();
			if (ch == 0)
				return null;

			if (ch != '<')
				throw new Exception("< expected at position " + (this.pos - 1).ToString() + ".");

			int Start = this.pos;
			while (this.pos < this.len && this.text[this.pos] != '>')
				this.pos++;

			if (this.pos >= this.len)
				throw new Exception("> expected.");

			string RelUri = this.text.Substring(Start, this.pos - Start);
			this.pos++;

			Uri Uri = new Uri(this.baseUri, RelUri);
			Link Link = new Link(Uri);

			while (this.PeekNextNonWhitespaceChar() == ';')
			{
				this.pos++;
				this.SkipWhitespace();

				Start = this.pos;
				while (this.pos < this.len && (ch = this.PeekNextNonWhitespaceChar()) != '=' && ch != ';' && ch != ',')
					this.pos++;

				string LinkName = this.text.Substring(Start, this.pos - Start);
				string LinkValue = string.Empty;

				if (ch == '=')
				{
					this.pos++;
					this.SkipWhitespace();

					if (this.PeekNextNonWhitespaceChar() == '"')
					{
						this.pos++;
						Start = this.pos;

						while (this.pos < this.len && (ch = this.text[this.pos]) != '"')
							this.pos++;

						if (this.pos >= this.len)
							throw new Exception("> expected.");

						LinkValue = this.text.Substring(Start, this.pos - Start);
						this.pos++;
					}
					else
					{
						Start = this.pos;
						while (this.pos < this.len && (ch = this.PeekNextNonWhitespaceChar()) != ';' && ch != ',')
							this.pos++;

						LinkValue = this.text.Substring(Start, this.pos - Start);
					}
				}

				Link.AddParameter(LinkName, LinkValue);

				switch (LinkName)
				{
					case "rel":
						Link.RelationTypes = LinkValue.Split(space, StringSplitOptions.RemoveEmptyEntries);
						break;

					case "anchor":
						Link.Anchor = new Uri(this.baseUri, LinkValue);
						break;

					case "rev":
						Link.Rev = LinkValue.Split(space, StringSplitOptions.RemoveEmptyEntries);
						break;

					case "hreflang":
						Link.Language = LinkValue;
						break;

					case "media":
						Link.Media = LinkValue;
						break;

					case "title":
						Link.Title = LinkValue;
						break;

					case "type":
						Link.Type = LinkValue;
						break;

					case "rt":
						Link.ResourceTypes = LinkValue.Split(space, StringSplitOptions.RemoveEmptyEntries);
						break;

					case "if":
						Link.InterfaceDescriptions = LinkValue.Split(space, StringSplitOptions.RemoveEmptyEntries);
						break;

					case "sz":
						ulong l;

						if (ulong.TryParse(LinkValue, out l))
							Link.Size = l;

						break;

					case "obs":
						Link.Observable = true;
						break;
				}
			}

			return Link;
		}

		private static readonly char[] space = new char[] { ' ' };

		/// <summary>
		/// Text representation of document.
		/// </summary>
		public string Text
		{
			get { return this.text; }
		}

		/// <summary>
		/// <see cref="object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.text;
		}
	}
}
