using System;
using System.Reflection;
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
		public Grade Supports(Type Object)
		{
			return typeof(XmlNode).GetTypeInfo().IsAssignableFrom(Object.GetTypeInfo()) ? Grade.Ok : Grade.NotAtAll;
		}

		/// <summary>
		/// Gets a string representing a value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>Expression string.</returns>
		public string GetString(object Value)
		{
			XmlNode Node = (XmlNode)Value;
			return Node.OuterXml;
		}
	}
}
