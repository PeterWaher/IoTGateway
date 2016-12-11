using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Filters
{
	/// <summary>
	/// This filter selects objects that conform to any of the child-filters provided.
	/// </summary>
	public class FilterOr : FilterChildren
	{
		/// <summary>
		/// This filter selects objects that conform to any of the child-filters provided.
		/// </summary>
		/// <param name="Filters">Child filters.</param>
		public FilterOr(params Filter[] Filters)
			: base(Filters)
		{
		}

		/// <summary>
		/// Calculates the logical inverse of the filter.
		/// </summary>
		/// <returns>Logical inerse of the filter.</returns>
		public override Filter Negate()
		{
			Filter[] Children = this.ChildFilters;
			int i, c = Children.Length;
			Filter[] NewChildren = new Filter[c];

			for (i = 0; i < c; i++)
				NewChildren[i] = Children[i].Negate();

			return new FilterAnd(NewChildren);
		}

		/// <summary>
		/// Creates a copy of the filter.
		/// </summary>
		/// <returns>Copy of filter.</returns>
		public override Filter Copy()
		{
			Filter[] Children = this.ChildFilters;
			int i, c = Children.Length;
			Filter[] NewChildren = new Filter[c];

			for (i = 0; i < c; i++)
				NewChildren[i] = Children[i].Copy();

			return new FilterOr(NewChildren);
		}

		/// <summary>
		/// Returns a normalized filter.
		/// </summary>
		/// <returns>Normalized filter.</returns>
		public override Filter Normalize()
		{
			List<Filter> Children = new List<Filter>();
			Filter Filter;

			foreach (Filter F in this.ChildFilters)
			{
				Filter = F.Normalize();

				if (Filter is FilterOr)
				{
					foreach (Filter F2 in ((FilterOr)Filter).ChildFilters)
						Children.Add(F2);
				}
				else
					Children.Add(Filter);
			}

			return new FilterOr(Children.ToArray());
		}
	}
}
