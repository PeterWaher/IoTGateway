using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements
{
	/// <summary>
	/// Abstract base class for inline elements.
	/// </summary>
	public abstract class InlineElement : HumanReadableElement	
	{
		/// <summary>
		/// Parses XML for a set of inline elements.
		/// </summary>
		/// <param name="Xml">XML representation</param>
		/// <returns>Array of inline elements</returns>
		public static InlineElement[] ParseChildren(XmlElement Xml)
		{
			List<InlineElement> Result = new List<InlineElement>();
			ParseChildren(Xml, Result);
			return Result.ToArray();
		}

		private static void ParseChildren(XmlElement Xml, List<InlineElement> Result)
		{ 
			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (N is XmlElement E)
				{
					InlineElement Element = TryParse(E);

					if (Element is null)
					{
						foreach (XmlNode N2 in E.ChildNodes)
						{
							if (N2 is XmlElement E2)
								ParseChildren(E2, Result);
						}
					}
					else
						Result.Add(Element);
				}
			}
		}

		/// <summary>
		/// Tries to parse a single inline element from its XML definition..
		/// </summary>
		/// <param name="Xml">XML representation</param>
		/// <returns>Inline element, or null if not recognized.</returns>
		public static InlineElement TryParse(XmlElement Xml)
		{
			switch (Xml.LocalName)
			{
				case "text":
					return new Text()
					{
						Value = Xml.InnerText
					};

				case "parameter":
					return new Parameter()
					{
						Name = XML.Attribute(Xml, "name")
					};

				case "bold":
					return new Bold()
					{
						Elements = ParseChildren(Xml)
					};

				case "italic":
					return new Italic()
					{
						Elements = ParseChildren(Xml)
					};

				case "underline":
					return new Underline()
					{
						Elements = ParseChildren(Xml)
					};

				case "strikeThrough":
					return new StrikeThrough()
					{
						Elements = ParseChildren(Xml)
					};

				case "super":
					return new Superscript()
					{
						Elements = ParseChildren(Xml)
					};

				case "sub":
					return new Subscript()
					{
						Elements = ParseChildren(Xml)
					};

				case "lineBreak":
					return new LineBreak();

				case "imageInline":
					Image Image = new Image()
					{
						ContentType = XML.Attribute(Xml, "contentType"),
						Width = XML.Attribute(Xml, "width", 0),
						Height = XML.Attribute(Xml, "height", 0)
					};

					foreach (XmlNode N in Xml.ChildNodes)
					{
						if (!(N is XmlElement E))
							continue;

						switch (E.LocalName)
						{
							case "binary":
								try
								{
									Image.Data = Convert.FromBase64String(E.InnerText);
								}
								catch (Exception)
								{
									return null;
								}
								break;

							case "caption":
								Image.Elements = ParseChildren(E);
								break;

							default:
								return null;
						}
					}

					return Image;

				default:
					return null;
			}
		}

	}
}
