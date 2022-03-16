using System;
using System.Text;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Abstract base class for Time Smart Contract Search filter operands.
	/// </summary>
	public abstract class SearchFilterTimeOperand : SearchFilterOperand
	{
		private readonly TimeSpan value;

		/// <summary>
		/// Abstract base class for Time Smart Contract Search filter operands.
		/// </summary>
		/// <param name="Value">String value</param>
		public SearchFilterTimeOperand(TimeSpan Value)
			: base()
		{
			this.value = Value;
		}

		/// <summary>
		/// Date value.
		/// </summary>
		public TimeSpan Value => this.value;

		/// <summary>
		/// Local XML element suffix.
		/// </summary>
		public override string ElementSuffix => "T";

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
			Xml.Append(this.value.ToString());
			Xml.Append("</");
			Xml.Append(this.OperandName);
			Xml.Append(ElementSuffix);
			Xml.Append('>');
		}

	}
}
