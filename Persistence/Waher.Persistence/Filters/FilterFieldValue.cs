using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Filters
{
	/// <summary>
	/// Abstract base class for all field filters operating on a constant value.
	/// </summary>
	public abstract class FilterFieldValue : FilterField
	{
		private readonly object value;

		/// <summary>
		/// Abstract base class for all field filters operating on a constant value.
		/// </summary>
		/// <param name="FieldName">Field Name.</param>
		/// <param name="Value">Value.</param>
		public FilterFieldValue(string FieldName, object Value)
			: base(FieldName)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value.
		/// </summary>
		public object Value
		{
			get { return this.value; }
		}

		/// <summary>
		/// Returns a normalized filter.
		/// </summary>
		/// <returns>Normalized filter.</returns>
		public override Filter Normalize()
		{
			return this.Copy();
		}

	}
}
