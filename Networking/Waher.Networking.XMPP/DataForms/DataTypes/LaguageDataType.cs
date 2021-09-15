using System;
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
		/// Public instance of data type.
		/// </summary>
		public static readonly LanguageDataType Instance = new LanguageDataType();

		/// <summary>
		/// Language Data Type (xs:language)
		/// </summary>
		public LanguageDataType()
			: this("xs:language")
		{
		}

		/// <summary>
		/// Language Data Type (xs:language)
		/// </summary>
		/// <param name="DataType">Data Type</param>
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
