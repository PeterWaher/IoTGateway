using System;
using System.Text;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Abstract base class for Date Smart Contract Search filter operands.
	/// </summary>
	public abstract class SearchFilterDateOperand : SearchFilterOperand
	{
		private readonly DateTime value;

		/// <summary>
		/// Abstract base class for Date Smart Contract Search filter operands.
		/// </summary>
		/// <param name="Value">String value</param>
		public SearchFilterDateOperand(DateTime Value)
			: base()
		{
			this.value = Value.Date;
		}

		/// <summary>
		/// Date value.
		/// </summary>
		public DateTime Value => this.value;

		/// <summary>
		/// Local XML element suffix.
		/// </summary>
		public override string ElementSuffix => "D";

		/// <summary>
		/// Serializes the search filter to XML.
		/// </summary>
		/// <param name="Xml">XML output.</param>
		/// <param name="ElementSuffix">Suffix, to append to element name during serialization.</param>
		public override void Serialize(StringBuilder Xml, string ElementSuffix)
		{
			Xml.Append('<');
			Xml.Append(this.OperandName);
			Xml.Append(ElementSuffix);
			Xml.Append('>');
			Xml.Append(XML.Encode(this.value, true));
			Xml.Append("</");
			Xml.Append(this.OperandName);
			Xml.Append(ElementSuffix);
			Xml.Append('>');
		}

	}
}
