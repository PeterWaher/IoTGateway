using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Filters
{
	/// <summary>
	/// This filter selects objects that have a named field lesser than a given value.
	/// </summary>
	public class FilterFieldLesserThan : FilterFieldValue
	{
		/// <summary>
		/// This filter selects objects that have a named field lesser than a given value.
		/// </summary>
		/// <param name="FieldName">Field Name.</param>
		/// <param name="Value">Value.</param>
		public FilterFieldLesserThan(string FieldName, object Value)
			: base(FieldName, Value)
		{
		}

		/// <summary>
		/// Calculates the logical inverse of the filter.
		/// </summary>
		/// <returns>Logical inerse of the filter.</returns>
		public override Filter Negate()
		{
			return new FilterFieldGreaterOrEqualTo(this.FieldName, this.Value);
		}

		/// <summary>
		/// Creates a copy of the filter.
		/// </summary>
		/// <returns>Copy of filter.</returns>
		public override Filter Copy()
		{
			return new FilterFieldLesserThan(this.FieldName, this.Value);
		}

		/// <summary>
		/// <see cref="object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.FieldName + "<" + this.Value?.ToString();
		}
	}
}
