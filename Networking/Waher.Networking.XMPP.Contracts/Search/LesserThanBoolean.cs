using System;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Limits searches to items with values lesser than this value.
	/// </summary>
	public class LesserThanBoolean : SearchFilterBooleanOperand
	{
		/// <summary>
		/// Limits searches to items with values lesser than this value.
		/// </summary>
		/// <param name="Value">Boolean value</param>
		public LesserThanBoolean(bool Value)
			: base(Value)
		{
		}

		/// <summary>
		/// Local XML element name of string operand.
		/// </summary>
		public override string OperandName => "lt";
	}
}
