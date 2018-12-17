using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Limits searches to items with values like this value, using a regular expression.
	/// </summary>
	public class LikeRegEx : SearchFilterStringOperand
	{
		/// <summary>
		/// Limits searches to items with values like this value, using a regular expression.
		/// </summary>
		/// <param name="Value">Regular expression</param>
		public LikeRegEx(string Value)
			: base(Value)
		{
		}

		/// <summary>
		/// Local XML element name of string operand.
		/// </summary>
		public override string OperandName => "like";
	}
}
