using System;
using System.Text;
using System.Xml;
using Waher.Script.Output;

namespace Waher.Script.Xml
{
	/// <summary>
	/// Formats an XmlDocument as an expression.
	/// </summary>
	public class XmlOutput : ICustomStringOutput
	{
		/// <summary>
		/// Type
		/// </summary>
		public Type Type => typeof(XmlDocument);

		/// <summary>
		/// Gets a string representing a value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>Expression string.</returns>
		public string GetString(object Value)
		{
			XmlDocument Doc = (XmlDocument)Value;
			return Doc.OuterXml;
		}
	}
}
