using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// Language Data Type (xs:language)
	/// </summary>
	public class LanguageDataType : DataType
	{
		private readonly static Regex pattern = new Regex(@"^[a-zA-Z]{1,8}(-[a-zA-Z0-9]{1,8})*$", RegexOptions.Singleline | RegexOptions.Compiled);

		/// <summary>
		/// Language Data Type (xs:language)
		/// </summary>
		/// <param name="TypeName">Type Name</param>
		public LanguageDataType(string DataType)
			: base(DataType)
		{
		}

		/// <summary>
		/// Parses a string.
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <returns>Parsed value, if possible, null otherwise.</returns>
		public override object Parse(string Value)
		{
			Match M = pattern.Match(Value);
			if (M.Success && M.Index == 0 || M.Length == Value.Length)
				return Value;
			else
				return null;
		}
	}
}
