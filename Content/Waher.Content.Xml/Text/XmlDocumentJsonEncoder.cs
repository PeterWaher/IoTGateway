using System;
using System.Text;
using System.Xml;
using Waher.Content.Json;
using Waher.Runtime.Inventory;

namespace Waher.Content.Xml.Text
{
	/// <summary>
	/// Encodes an XML Document as a JSON object.
	/// </summary>
	public class XmlDocumentJsonEncoder : IJsonEncoder
	{
		/// <summary>
		/// Encodes an XML Document as a JSON object.
		/// </summary>
		public XmlDocumentJsonEncoder()
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
			XmlDocument Xml = (XmlDocument)Object;

			if (Xml.DocumentElement is null)
				Json.Append("null");
			else
				XmlElementJsonEncoder.Encode(Xml.DocumentElement, Indent, Json);
		}

		/// <summary>
		/// How well the JSON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public Grade Supports(Type ObjectType)
		{
			return ObjectType == typeof(XmlDocument) ? Grade.Ok : Grade.NotAtAll;
		}
	}
}
