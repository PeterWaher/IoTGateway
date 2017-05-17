using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using F = Waher.Persistence.Filters;
using Waher.Persistence.Files.Serialization;

namespace Waher.Persistence.Files.Searching
{
	/// <summary>
	/// This filter selects objects that have a named field matching a given regular expression.
	/// </summary>
	public class FilterFieldLikeRegEx : F.FilterFieldLikeRegEx, IApplicableFilter
	{
		private Regex regex;

		/// <summary>
		/// This filter selects objects that have a named field matching a given regular expression.
		/// </summary>
		/// <param name="FieldName">Field Name.</param>
		/// <param name="RegularExpression">Regular expression.</param>
		public FilterFieldLikeRegEx(string FieldName, string RegularExpression)
			: base(FieldName, RegularExpression)
		{
			this.regex = new Regex(RegularExpression, RegexOptions.Singleline);
		}

		/// <summary>
		/// Checks if the filter applies to the object.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Serializer">Corresponding object serializer.</param>
		/// <returns>If the filter can be applied.</returns>
		public bool AppliesTo(object Object, IObjectSerializer Serializer)
		{
			object Value;

			if (!Serializer.TryGetFieldValue(this.FieldName, Object, out Value))
				return false;

			string s = Value.ToString();
			Match M = this.regex.Match(s);

			return M.Success && M.Index == 0 && M.Length == s.Length;
		}
		
		/// <summary>
		/// Parsed regular expression.
		/// </summary>
		public Regex Regex
		{
			get { return this.regex; }
		}
	}
}
