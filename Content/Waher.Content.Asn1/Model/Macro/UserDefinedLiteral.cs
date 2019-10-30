using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Macro
{
	/// <summary>
	/// String literal
	/// </summary>
	public class UserDefinedLiteral : UserDefinedItem
	{
		private readonly string value;

		/// <summary>
		/// String literal
		/// </summary>
		/// <param name="Value">Literal value.</param>
		public UserDefinedLiteral(string Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// String literal
		/// </summary>
		public string Value => this.value;
	}
}
