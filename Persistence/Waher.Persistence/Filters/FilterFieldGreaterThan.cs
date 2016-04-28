using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Filters
{
	/// <summary>
	/// This filter selects objects that have a named field greater than a given value.
	/// </summary>
	public class FilterFieldGreaterThan : FilterFieldValue
	{
		/// <summary>
		/// This filter selects objects that have a named field greater than a given value.
		/// </summary>
		/// <param name="FieldName">Field Name.</param>
		/// <param name="Value">Value.</param>
		public FilterFieldGreaterThan(string FieldName, object Value)
			: base(FieldName, Value)
		{
		}
	}
}
