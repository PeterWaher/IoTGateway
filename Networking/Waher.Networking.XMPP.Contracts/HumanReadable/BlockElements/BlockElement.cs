using System;
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
		public static BlockElement[] Parse(XmlElement Xml)
		{
			List<BlockElement> Result = new List<BlockElement>();

			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (N is XmlElement E)
				{
					switch (E.LocalName)
					{
						case "paragraph":
							Result.Add(new Paragraph()
							{
								Elements = InlineElement.Parse(E)
							});
							break;

						case "section":
							Section Section = new Section();

							foreach (XmlNode N2 in E.ChildNodes)
							{
								if (N2 is XmlElement E2)
								{
									switch (E2.LocalName)
									{
										case "header":
											Section.Header = InlineElement.Parse(E2);
											break;

										case "body":
											Section.Body = BlockElement.Parse(E2);
											break;
									}
								}
							}

							Result.Add(Section);
							break;

						case "bulletItems":
							List<Item> Items = new List<Item>();

							foreach (XmlNode N2 in E.ChildNodes)
							{
								if (N2 is XmlElement E2 && E2.LocalName == "item")
								{
									Items.Add(new Item()
									{
										Elements = InlineElement.Parse(E2)
									});
								}
							}

							Result.Add(new BulletList()
							{
								Items = Items.ToArray()
							});
							break;

						case "numberedItems":
							Items = new List<Item>();

							foreach (XmlNode N2 in E.ChildNodes)
							{
								if (N2 is XmlElement E2 && E2.LocalName == "item")
								{
									Items.Add(new Item()
									{
										Elements = InlineElement.Parse(E2)
									});
								}
							}

							Result.Add(new NumberedList()
							{
								Items = Items.ToArray()
							});
							break;

						default:
							return null;
					}
				}
			}

			return Result.ToArray();
		}
	}
}
