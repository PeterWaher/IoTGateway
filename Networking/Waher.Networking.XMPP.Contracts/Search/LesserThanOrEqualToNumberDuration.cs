using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Limits searches to items with values lesser than or equal to this value.
	/// </summary>
	public class LesserThanOrEqualToDuration : SearchFilterDurationOperand
	{
		/// <summary>
		/// Limits searches to items with values lesser than or equal to this value.
		/// </summary>
		/// <param name="Value">Duration value</param>
		public LesserThanOrEqualToDuration(Duration Value)
			: base(Value)
		{
		}

		/// <summary>
		/// Local XML element name of string operand.
		/// </summary>
		public override string OperandName => "lte";
	}
}
