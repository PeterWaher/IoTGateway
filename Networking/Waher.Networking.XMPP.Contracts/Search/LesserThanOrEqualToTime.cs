using System;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Limits searches to items with values lesser than or equal to this value.
	/// </summary>
	public class LesserThanOrEqualToTime : SearchFilterTimeOperand
	{
		/// <summary>
		/// Limits searches to items with values lesser than or equal to this value.
		/// </summary>
		/// <param name="Value">Time value</param>
		public LesserThanOrEqualToTime(TimeSpan Value)
			: base(Value)
		{
		}

		/// <summary>
		/// Local XML element name of string operand.
		/// </summary>
		public override string OperandName => "lte";
	}
}
