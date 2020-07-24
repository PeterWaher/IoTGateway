using System;
using System.Text.RegularExpressions;

namespace Waher.Persistence.Filters
{
	/// <summary>
	/// This filter selects objects that have a named field matching a given regular expression.
	/// </summary>
	public class FilterFieldLikeRegEx : FilterField
	{
		private readonly string regularExpression;

		/// <summary>
		/// This filter selects objects that have a named field matching a given regular expression.
		/// </summary>
		/// <param name="FieldName">Field Name.</param>
		/// <param name="RegularExpression">Regular expression.</param>
		public FilterFieldLikeRegEx(string FieldName, string RegularExpression)
			: base(FieldName)
		{
			this.regularExpression = RegularExpression;
		}

		/// <summary>
		/// Regular expression.
		/// </summary>
		public string RegularExpression
		{
			get
			{
				return this.regularExpression;
			}
		}

		/// <summary>
		/// Calculates the logical inverse of the filter.
		/// </summary>
		/// <returns>Logical inverse of the filter.</returns>
		public override Filter Negate()
		{
			return new FilterNot(this.Copy());
		}

		/// <summary>
		/// Creates a copy of the filter.
		/// </summary>
		/// <returns>Copy of filter.</returns>
		public override Filter Copy()
		{
			return new FilterFieldLikeRegEx(this.FieldName, this.regularExpression);
		}

		/// <summary>
		/// Returns a normalized filter.
		/// </summary>
		/// <returns>Normalized filter.</returns>
		public override Filter Normalize()
		{
			return this.Copy();
		}

		/// <summary>
		/// <see cref="object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.FieldName + " LIKE " + this.regularExpression.ToString();
		}

		/// <summary>
		/// Performs a comparison on the object with the field value <paramref name="Value"/>.
		/// </summary>
		/// <param name="Value">Field value for comparison.</param>
		/// <returns>Result of comparison.</returns>
		public override bool Compare(object Value)
		{
			if (Value is null)
				return false;

			string s = Value?.ToString();

			if (this.regex is null)
				this.regex = new Regex(this.regularExpression, RegexOptions.Singleline);

			Match M = this.regex.Match(s);

			return M.Success && M.Index == 0 && M.Length == s.Length;
		}

		private Regex regex = null;

	}
}
