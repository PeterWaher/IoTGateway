using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content.Json;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;

namespace Waher.Content.Xml.Text
{
	/// <summary>
	/// Encodes an XML Element as a JSON object.
	/// </summary>
	public class XmlElementJsonEncoder : IJsonEncoder
	{
		/// <summary>
		/// Encodes an XML Element as a JSON object.
		/// </summary>
		public XmlElementJsonEncoder()
		{
		}

		/// <summary>
		/// Encodes the <paramref name="Object"/> to JSON.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Any indentation to apply.</param>
		/// <param name="Json">JSON output.</param>
		public void Encode(object Object, int? Indent, StringBuilder Json)
		{
			XmlElement Xml = (XmlElement)Object;
			Encode(Xml, Indent, Json);
		}

		/// <summary>
		/// Encodes an <see cref="XmlElement"/> to JSON.
		/// </summary>
		/// <param name="Xml">XML to encode.</param>
		/// <param name="Indent">Any indentation to apply.</param>
		/// <param name="Json">JSON output.</param>
		public static void Encode(XmlElement Xml, int? Indent, StringBuilder Json)
		{
			bool First = true;

			Json.Append('{');

			if (Indent.HasValue)
				Indent++;

			AppendProperty("__name", Xml.LocalName, Indent, ref First, Json);
			AppendProperty("__ns", Xml.NamespaceURI, Indent, ref First, Json);

			foreach (XmlAttribute Attr in Xml.Attributes)
				AppendProperty(Attr.Name, Attr.Value, Indent, ref First, Json);

			if (Xml.HasChildNodes)
			{
				Dictionary<string, ChunkedList<XmlElement>> ChildElements = null;
				StringBuilder InnerText = null;

				foreach (XmlNode N in Xml.ChildNodes)
				{
					if (N is XmlElement E)
					{
						if (ChildElements is null)
							ChildElements = new Dictionary<string, ChunkedList<XmlElement>>();

						if (!ChildElements.TryGetValue(E.LocalName, out	ChunkedList<XmlElement> Elements))
						{
							Elements = new ChunkedList<XmlElement>();
							ChildElements[E.LocalName] = Elements;
						}

						Elements.Add(E);
					}
					else if (N is XmlText ||
						N is XmlCDataSection ||
						N is XmlWhitespace ||
						N is XmlSignificantWhitespace)
					{
						if (InnerText is null)
							InnerText = new StringBuilder();

						InnerText.Append(N.InnerText);
					}
				}

				if (!(ChildElements is null))
				{
					foreach (KeyValuePair<string, ChunkedList<XmlElement>> P in ChildElements)
					{
						if (P.Value.Count == 1)
							AppendProperty(P.Key, P.Value.FirstItem, Indent, ref First, Json);
						else
							AppendProperty(P.Key, P.Value, Indent, ref First, Json);
					}

					if (!(InnerText is null))
					{
						AppendProperty(ChildElements.ContainsKey("value") ? "__value" : "value",
							InnerText.ToString(), Indent, ref First, Json);
					}
				}
				else if (!(InnerText is null))
					AppendProperty("value", InnerText.ToString(), Indent, ref First, Json);
			}

			if (Indent.HasValue)
			{
				Indent--;

				if (!First)
				{
					Json.AppendLine();
					Json.Append(new string('\t', Indent.Value));
				}
			}

			Json.Append('}');
		}

		private static void AppendProperty(string Name, object Value, int? Indent, 
			ref bool First, StringBuilder Json)
		{
			if (First)
				First = false;
			else
				Json.Append(',');

			if (Indent.HasValue)
			{
				Json.AppendLine();
				Json.Append(new string('\t', Indent.Value));
			}

			Json.Append('"');
			Json.Append(JSON.Encode(Name));
			Json.Append("\":");

			if (Indent.HasValue)
				Json.Append(' ');

			JSON.Encode(Value, Indent, Json);
		}

		/// <summary>
		/// How well the JSON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public Grade Supports(Type ObjectType)
		{
			return ObjectType == typeof(XmlElement) ? Grade.Ok : Grade.NotAtAll;
		}

	}
}
