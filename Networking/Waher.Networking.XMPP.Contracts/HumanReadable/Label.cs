using System.Text;
using Waher.Content.Xml;
using Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements;

namespace Waher.Networking.XMPP.Contracts.HumanReadable
{
	/// <summary>
	/// Label
	/// </summary>
	public class Label : Formatting
	{
		private string language;

		/// <summary>
		/// Language of label
		/// </summary>
		public string Language
		{
			get => this.language;
			set => this.language = value;
		}

		/// <summary>
		/// Serializes the element in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public override void Serialize(StringBuilder Xml)
		{
			Xml.Append("<label");
		
			if(!string.IsNullOrEmpty(this.language))
			{
				Xml.Append(" xml:lang=\"");
				Xml.Append(XML.Encode(this.language));
				Xml.Append('"');
			}

			Xml.Append('>');

			if (!(this.Elements is null))
			{
				foreach (HumanReadableElement E in this.Elements)
					E.Serialize(Xml);
			}
		
			Xml.Append("</label>");
		}

		/// <summary>
		/// Class representing human-readable text.
		/// </summary>
		/// <param name="Xml">XML representation.</param>
		/// <returns>Human-readable text.</returns>
		public static Label Parse(System.Xml.XmlElement Xml)
		{
			Label Result = new Label()
			{
				language = XML.Attribute(Xml, "xml:lang"),
				Elements = InlineElement.ParseChildren(Xml)
			};

			return Result;
		}

		/// <summary>
		/// Parses simplified human readable text.
		/// </summary>
		/// <param name="Xml">XML representation.</param>
		/// <returns>Human-readable text.</returns>
		public static Label ParseSimplified(System.Xml.XmlElement Xml)
		{
			Label Result = new Label()
			{
				language = XML.Attribute(Xml, "lang"),
				Elements = new InlineElement[]
				{
					new Text()
					{
						Value = Xml.InnerText
					}
				}
			};

			return Result;
		}

	}
}
