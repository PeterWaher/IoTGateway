using System;
using System.Xml;

namespace Waher.Layout.Layout2D.Model
{
	/// <summary>
	/// Base interface for all layout elements.
	/// </summary>
	public interface ILayoutElement : IDisposable
	{
		/// <summary>
		/// Local name of type of element.
		/// </summary>
		string LocalName
		{
			get;
		}

		/// <summary>
		/// Namespace of type of element.
		/// </summary>
		string Namespace
		{
			get;
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Containing document.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent);

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		void FromXml(XmlElement Input);

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		void ToXml(XmlWriter Output);
	}
}
