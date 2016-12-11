using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Filters
{
	/// <summary>
	/// This filter selects objects that does not conform to the child-filter provided.
	/// </summary>
	public class FilterNot : FilterChild 
	{
		/// <summary>
		/// This filter selects objects that does not conform to the child-filter provided.
		/// </summary>
		/// <param name="Filters">Child filter.</param>
		public FilterNot(Filter Filter)
			: base(Filter)
		{
		}

		/// <summary>
		/// Calculates the logical inverse of the filter.
		/// </summary>
		/// <returns>Logical inerse of the filter.</returns>
		public override Filter Negate()
		{
			return this.ChildFilter.Copy();
		}

		/// <summary>
		/// Creates a copy of the filter.
		/// </summary>
		/// <returns>Copy of filter.</returns>
		public override Filter Copy()
		{
			return new FilterNot(this.ChildFilter.Copy());
		}

		/// <summary>
		/// Returns a normalized filter.
		/// </summary>
		/// <returns>Normalized filter.</returns>
		public override Filter Normalize()
		{
			return this.ChildFilter.Negate();
		}
	}
}
