using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Files.Serialization;
using F = Waher.Persistence.Filters;

namespace Waher.Persistence.Files.Searching
{
	/// <summary>
	/// This filter selects objects that have a named field not equal to a given value.
	/// </summary>
	public class FilterFieldNotEqualTo : F.FilterFieldNotEqualTo, IApplicableFilter
	{
		/// <summary>
		/// This filter selects objects that have a named field not equal to a given value.
		/// </summary>
		/// <param name="FieldName">Field Name.</param>
		/// <param name="Value">Value.</param>
		public FilterFieldNotEqualTo(string FieldName, object Value)
			: base(FieldName, Value)
		{
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

			int? ComparisonResult = Comparison.Compare(Value, this.Value);

			return ComparisonResult.HasValue && ComparisonResult.Value != 0;
		}
	}
}
