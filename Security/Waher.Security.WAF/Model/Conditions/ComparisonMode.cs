namespace Waher.Security.WAF.Model.Conditions
{
	/// <summary>
	/// Condition comparison mode.
	/// </summary>
	public enum ComparisonMode
	{
		/// <summary>
		/// Comparison is done against a regular expression. The value must contain a match.
		/// </summary>
		ContainsRegex,

		/// <summary>
		/// Comparison is done against a regular expression. The start of the value must 
		/// match the regular expression.
		/// </summary>
		StartsWithRegex,

		/// <summary>
		/// Comparison is done against a regular expression. The end of the value must 
		/// match the regular expression.
		/// </summary>
		EndsWithRegex,

		/// <summary>
		/// Comparison is done against a regular expression. The entire value must match
		/// the regular expression.
		/// </summary>
		ExactRegex,

		/// <summary>
		/// Case-insensitive comparison is done against a regular expression. The value 
		/// must contain a match.
		/// </summary>
		ContainsRegexIgnoreCase,

		/// <summary>
		/// Case-insensitive comparison is done against a regular expression. The start of 
		/// the value must match the regular expression.
		/// </summary>
		StartsWithRegexIgnoreCase,

		/// <summary>
		/// Case-insensitive comparison is done against a regular expression. The end of 
		/// the value must match the regular expression.
		/// </summary>
		EndsWithRegexIgnoreCase,

		/// <summary>
		/// Case-insensitive comparison is done against a regular expression. The entire 
		/// value must match the regular expression.
		/// </summary>
		ExactRegexIgnoreCase,

		/// <summary>
		/// Comparison is done against a string constant. The value must contain a match.
		/// </summary>
		ContainsConstant,

		/// <summary>
		/// Comparison is done against a string constant. The start of the value must 
		/// match the constant.
		/// </summary>
		StartsWithConstant,

		/// <summary>
		/// Comparison is done against a string constant. The end of the value must 
		/// match the constant.
		/// </summary>
		EndsWithConstant,

		/// <summary>
		/// Comparison is done against a string constant. The entire value must match the 
		/// constant.
		/// </summary>
		ExactConstant,

		/// <summary>
		/// Case-insensitive comparison is done against a string constant. The value must 
		/// contain a match.
		/// </summary>
		ContainsConstantIgnoreCase,

		/// <summary>
		/// Case-insensitive comparison is done against a string constant. The start of the value 
		/// must match the constant.
		/// </summary>
		StartsWithConstantIgnoreCase,

		/// <summary>
		/// Case-insensitive comparison is done against a string constant. The end of the value 
		/// must match the constant.
		/// </summary>
		EndsWithConstantIgnoreCase,

		/// <summary>
		/// Case-insensitive comparison is done against a string constant. The entire value must 
		/// match the constant.
		/// </summary>
		ExactConstantIgnoreCase,

		/// <summary>
		/// The value is compared against a script, that examines the value.
		/// </summary>
		Script,

		/// <summary>
		/// The value is compared against a list of comma-separated values.
		/// </summary>
		List,

		/// <summary>
		/// The case-insensitive value is compared against a list of comma-separated values.
		/// </summary>
		ListIgnoreCase,

		/// <summary>
		/// The value is compared against the contents of a list available in a file.
		/// </summary>
		FileList,

		/// <summary>
		/// The value is case-insensitively compared against the contents of a list 
		/// available in a file.
		/// </summary>
		FileListIgnoreCase,

		/// <summary>
		/// The value is compared against the contents of a list available in the
		/// internal database.
		/// </summary>
		DatabaseList,

		/// <summary>
		/// The value is compared against the contents of a case-insensitive list available 
		/// in the internal database.
		/// </summary>
		DatabaseListIgnoreCase
	}
}
