using System.Text;
using Waher.Content;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Abstract base class for Boolean Smart Contract Search filter operands.
	/// </summary>
	public abstract class SearchFilterBooleanOperand : SearchFilterOperand
	{
		private readonly bool value;

		/// <summary>
		/// Abstract base class for Boolean Smart Contract Search filter operands.
		/// </summary>
		/// <param name="Value">Boolean value</param>
		public SearchFilterBooleanOperand(bool Value)
			: base()
		{
			this.value = Value;
		}

		/// <summary>
		/// Boolean value.
		/// </summary>
		public bool Value => this.value;

		/// <summary>
		/// Local XML element suffix.
		/// </summary>
		public override string ElementSuffix => "B";

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
			Xml.Append(CommonTypes.Encode(this.value));
			Xml.Append("</");
			Xml.Append(this.OperandName);
			Xml.Append(ElementSuffix);
			Xml.Append('>');
		}

	}
}
