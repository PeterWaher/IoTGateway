using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Filters
{
	/// <summary>
	/// This filter selects objects that have a named field matching a given regular expression.
	/// </summary>
	public class FilterFieldLikeRegEx : FilterField
	{
		private string regularExpression;

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
		/// <returns>Logical inerse of the filter.</returns>
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
	}
}
