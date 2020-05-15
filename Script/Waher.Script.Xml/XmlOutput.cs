using System;
using System.Text;
using System.Xml;
using Waher.Runtime.Inventory;
using Waher.Script.Output;

namespace Waher.Script.Xml
{
	/// <summary>
	/// Formats an XmlDocument as an expression.
	/// </summary>
	public class XmlOutput : ICustomStringOutput
	{
		/// <summary>
		/// If the interface understands objects such as <paramref name="Object"/>.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>How well objects of this type are supported.</returns>
		public Grade Supports(Type Object) => Object == typeof(XmlDocument) ? Grade.Ok : Grade.NotAtAll;

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
