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
		public static InlineElement[] Parse(XmlElement Xml)
		{
			List<InlineElement> Result = new List<InlineElement>();
			Parse(Xml, Result);
			return Result.ToArray();
		}

		private static void Parse(XmlElement Xml, List<InlineElement> Result)
		{ 
			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (N is XmlElement E)
				{
					switch (E.LocalName)
					{
						case "text":
							Result.Add(new Text()
							{
								Value = E.InnerText
							});
							break;

						case "parameter":
							Result.Add(new Parameter()
							{
								Name = XML.Attribute(E, "name")
							});
							break;

						case "bold":
							Result.Add(new Bold()
							{
								Elements = Parse(E)
							});
							break;

						case "italic":
							Result.Add(new Italic()
							{
								Elements = Parse(E)
							});
							break;

						case "underline":
							Result.Add(new Underline()
							{
								Elements = Parse(E)
							});
							break;

						case "strikeThrough":
							Result.Add(new StrikeThrough()
							{
								Elements = Parse(E)
							});
							break;

						case "super":
							Result.Add(new Superscript()
							{
								Elements = Parse(E)
							});
							break;

						case "sub":
							Result.Add(new Subscript()
							{
								Elements = Parse(E)
							});
							break;

						default:
							foreach (XmlNode N2 in E.ChildNodes)
							{
								if (N2 is XmlElement E2)
									Parse(E2, Result);
							}
							break;
					}
				}
			}
		}
	}
}
