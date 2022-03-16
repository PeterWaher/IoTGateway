using System;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Limits searches to items with values lesser than this value.
	/// </summary>
	public class LesserThanTime : SearchFilterTimeOperand
	{
		/// <summary>
		/// Limits searches to items with values lesser than this value.
		/// </summary>
		/// <param name="Value">Time value</param>
		public LesserThanTime(TimeSpan Value)
			: base(Value)
		{
		}

		/// <summary>
		/// Local XML element name of string operand.
		/// </summary>
		public override string OperandName => "lt";
	}
}
