using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Limits searches to items with values equal to this value.
	/// </summary>
	public class EqualToDuration : SearchFilterDurationOperand
	{
		/// <summary>
		/// Limits searches to items with values equal to this value.
		/// </summary>
		/// <param name="Value">Duration value</param>
		public EqualToDuration(Duration Value)
			: base(Value)
		{
		}

		/// <summary>
		/// Local XML element name of double operand.
		/// </summary>
		public override string OperandName => "eq";
	}
}
