using System;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Limits searches to items with values equal to this value.
	/// </summary>
	public class EqualToTime : SearchFilterTimeOperand
	{
		/// <summary>
		/// Limits searches to items with values equal to this value.
		/// </summary>
		/// <param name="Value">Time value</param>
		public EqualToTime(TimeSpan Value)
			: base(Value)
		{
		}

		/// <summary>
		/// Local XML element name of double operand.
		/// </summary>
		public override string OperandName => "eq";
	}
}
