using System;
using System.Text;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements
{
	/// <summary>
	/// Is replaced by parameter value
	/// </summary>
	public class Parameter : InlineElement
	{
		private string name;

		/// <summary>
		/// Name of parameter
		/// </summary>
		public string Name
		{
			get => this.name;
			set => this.name = value;
		}

		/// <summary>
		/// Checks if the element is well-defined.
		/// </summary>
		public override bool IsWellDefined => !string.IsNullOrEmpty(this.name);

		/// <summary>
		/// Serializes the element in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public override void Serialize(StringBuilder Xml)
		{
			Xml.Append("<parameter name=\"");
			Xml.Append(XML.Encode(this.name));
			Xml.Append("\"/>");
		}
	}
}
