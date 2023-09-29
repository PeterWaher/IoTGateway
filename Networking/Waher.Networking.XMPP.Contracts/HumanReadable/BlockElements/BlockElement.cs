using System.Collections.Generic;
using System.Xml;
using Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.BlockElements
{
	/// <summary>
	/// Abstract base class for inline elements.
	/// </summary>
	public abstract class BlockElement : HumanReadableElement
	{
		/// <summary>
		/// Parses XML for a set of block elements.
		/// </summary>
		/// <param name="Xml">XML representation</param>
		/// <returns>Array of block elements</returns>
		public static BlockElement[] ParseChildren(XmlElement Xml)
		{
			List<BlockElement> Result = new List<BlockElement>();
			ParseChildren(Xml, Result);
			return Result.ToArray();
		}

		private static void ParseChildren(XmlElement Xml, List<BlockElement> Result)
		{
			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (N is XmlElement E)
				{
					BlockElement Element = TryParse(E);

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
		/// Tries to parse a single block element from its XML definition..
		/// </summary>
		/// <param name="Xml">XML representation</param>
		/// <returns>Block element, or null if not recognized.</returns>
		public static BlockElement TryParse(XmlElement Xml)
		{
			switch (Xml.LocalName)
			{
				case "paragraph":
					return new Paragraph()
					{
						Elements = InlineElement.ParseChildren(Xml)
					};

				case "section":
					Section Section = new Section();

					foreach (XmlNode N2 in Xml.ChildNodes)
					{
						if (N2 is XmlElement E2)
						{
							switch (E2.LocalName)
							{
								case "header":
									Section.Header = InlineElement.ParseChildren(E2);
									break;

								case "body":
									Section.Body = ParseChildren(E2);
									break;
							}
						}
					}

					return Section;

				case "bulletItems":
					List<Item> Items = new List<Item>();

					foreach (XmlNode N2 in Xml.ChildNodes)
					{
						if (N2 is XmlElement E2 && E2.LocalName == "item")
						{
							Items.Add(new Item()
							{
								Elements = ParseBlockOrInlineChildren(E2)
							});
						}
					}

					return new BulletList()
					{
						Items = Items.ToArray()
					};

				case "numberedItems":
					Items = new List<Item>();

					foreach (XmlNode N2 in Xml.ChildNodes)
					{
						if (N2 is XmlElement E2 && E2.LocalName == "item")
						{
							Items.Add(new Item()
							{
								Elements = ParseBlockOrInlineChildren(E2)
							});
						}
					}

					return new NumberedList()
					{
						Items = Items.ToArray()
					};

				default:
					return null;
			}
		}
	}
}
