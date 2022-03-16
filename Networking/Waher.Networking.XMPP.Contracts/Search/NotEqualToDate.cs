using System;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Limits searches to items with values not equal to this value.
	/// </summary>
	public class NotEqualToDate : SearchFilterDateOperand
	{
		/// <summary>
		/// Limits searches to items with values not equal to this value.
		/// </summary>
		/// <param name="Value">Date value</param>
		public NotEqualToDate(DateTime Value)
			: base(Value)
		{
		}

		/// <summary>
		/// Local XML element name of string operand.
		/// </summary>
		public override string OperandName => "neq";
	}
}
