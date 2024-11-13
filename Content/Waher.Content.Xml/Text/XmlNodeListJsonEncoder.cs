using System;
using System.Text;
using System.Xml;
using Waher.Content.Json;
using Waher.Runtime.Inventory;

namespace Waher.Content.Xml.Text
{
	/// <summary>
	/// Encodes a list of XML nodes to a JSON object.
	/// </summary>
	public class XmlNodeListJsonEncoder : IJsonEncoder
	{
		/// <summary>
		/// Encodes a list of XML nodes to a JSON object.
		/// </summary>
		public XmlNodeListJsonEncoder()
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
			XmlNodeList Xml = (XmlNodeList)Object;
			bool First = true;

			Json.Append('[');

			if (Indent.HasValue)
			{
				Indent++;
				Json.AppendLine();
				Json.Append(new string('\t', Indent.Value));
			}

			foreach (XmlNode XmlNode in Xml)
			{
				if (First)
					First = false;
				else
				{
					Json.Append(',');

					if (Indent.HasValue)
					{
						Json.AppendLine();
						Json.Append(new string('\t', Indent.Value));
					}
				}

				JSON.Encode(XmlNode, Indent, Json);
			}

			if (Indent.HasValue)
			{
				Indent--;
				Json.AppendLine();
				Json.Append(new string('\t', Indent.Value));
			}

			Json.Append(']');
		}

		/// <summary>
		/// How well the JSON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public Grade Supports(Type ObjectType)
		{
			return ObjectType == typeof(XmlNodeList) ? Grade.Ok : Grade.NotAtAll;
		}
	}
}
