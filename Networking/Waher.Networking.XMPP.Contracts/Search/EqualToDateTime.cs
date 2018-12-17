using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Limits searches to items with values equal to this value.
	/// </summary>
	public class EqualToDateTime : SearchFilterDateTimeOperand
	{
		/// <summary>
		/// Limits searches to items with values equal to this value.
		/// </summary>
		/// <param name="Value">DateTime value</param>
		public EqualToDateTime(DateTime Value)
			: base(Value)
		{
		}

		/// <summary>
		/// Local XML element name of double operand.
		/// </summary>
		public override string OperandName => "eq";
	}
}
