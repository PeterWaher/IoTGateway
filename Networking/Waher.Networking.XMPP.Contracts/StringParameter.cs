using System;
using System.Text;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// String-valued contractual parameter
	/// </summary>
	public class StringParameter : Parameter
	{
		private string value;

		/// <summary>
		/// Parameter value
		/// </summary>
		public string Value
		{
			get => this.value;
			set => this.value = value;
		}

		/// <summary>
		/// Parameter value.
		/// </summary>
		public override object ObjectValue => this.value;

		/// <summary>
		/// Serializes the parameter, in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output</param>
		public override void Serialize(StringBuilder Xml)
		{
			Xml.Append("<stringParameter name=\"");
			Xml.Append(XML.Encode(this.Name));
			Xml.Append("\" value=\"");
			Xml.Append(XML.Encode(this.value));
			Xml.Append("\"/>");
		}
	}
}
