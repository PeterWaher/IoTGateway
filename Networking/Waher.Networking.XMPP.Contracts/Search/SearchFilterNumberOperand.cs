using System.Text;
using Waher.Content;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Abstract base class for Numerical Smart Contract Search filter operands.
	/// </summary>
	public abstract class SearchFilterNumberOperand : SearchFilterOperand
	{
		private readonly decimal value;

		/// <summary>
		/// Abstract base class for Numerical Smart Contract Search filter operands.
		/// </summary>
		/// <param name="Value">Numerical value</param>
		public SearchFilterNumberOperand(decimal Value)
			: base()
		{
			this.value = Value;
		}

		/// <summary>
		/// Numerical value.
		/// </summary>
		public decimal Value => this.value;

		/// <summary>
		/// Local XML element suffix.
		/// </summary>
		public override string ElementSuffix => "Num";

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
