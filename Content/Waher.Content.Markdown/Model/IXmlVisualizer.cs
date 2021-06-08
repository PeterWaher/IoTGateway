using System;
using System.Xml;
using Waher.Runtime.Inventory;

namespace Waher.Content.Markdown.Model
{
	/// <summary>
	/// Interface for all XML visalizers.
	/// </summary>
	public interface IXmlVisualizer
	{
		/// <summary>
		/// Checks how well the handler supports multimedia content of a given type.
		/// </summary>
		/// <param name="Xml">XML Document</param>
		/// <returns>How well the handler supports the content.</returns>
		Grade Supports(XmlDocument Xml);

		/// <summary>
		/// Transforms the XML document before visualizing it.
		/// </summary>
		/// <param name="Xml">XML Document.</param>
		/// <returns>Transformed object.</returns>
		object TransformXml(XmlDocument Xml);
	}
}
