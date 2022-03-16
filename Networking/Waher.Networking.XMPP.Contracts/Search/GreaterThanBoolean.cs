using System;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Limits searches to items with values greater than this value.
	/// </summary>
	public class GreaterThanBoolean : SearchFilterBooleanOperand
	{
		/// <summary>
		/// Limits searches to items with values greater than this value.
		/// </summary>
		/// <param name="Value">Boolean value</param>
		public GreaterThanBoolean(bool Value)
			: base(Value)
		{
		}

		/// <summary>
		/// Local XML element name of string operand.
		/// </summary>
		public override string OperandName => "gt";
	}
}
