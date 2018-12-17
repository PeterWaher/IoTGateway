using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Limits searches to items with values note equal to this value.
	/// </summary>
	public class NotEqualToDuration : SearchFilterDurationOperand
	{
		/// <summary>
		/// Limits searches to items with values note equal to this value.
		/// </summary>
		/// <param name="Value">Duration value</param>
		public NotEqualToDuration(Duration Value)
			: base(Value)
		{
		}

		/// <summary>
		/// Local XML element name of string operand.
		/// </summary>
		public override string OperandName => "neq";
	}
}
