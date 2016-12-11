using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Filters
{
	/// <summary>
	/// This filter selects objects that conform to all child-filters provided.
	/// </summary>
	public class FilterAnd : FilterChildren
	{
		/// <summary>
		/// This filter selects objects that conform to all child-filters provided.
		/// </summary>
		/// <param name="Filters">Child filters.</param>
		public FilterAnd(params Filter[] Filters)
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

			return new FilterOr(NewChildren);
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

			return new FilterAnd(NewChildren);
		}

		/// <summary>
		/// Returns a normalized filter.
		/// </summary>
		/// <returns>Normalized filter.</returns>
		public override Filter Normalize()
		{
			List<Filter> Children = new List<Filter>();
			Filter[] ChildFilters = this.ChildFilters;
			List<Filter[]> Ors = null;
			Filter Filter;

			if (ChildFilters.Length == 1)
				return ChildFilters[0].Normalize();

			foreach (Filter F in ChildFilters)
			{
				Filter = F.Normalize();

				if (Filter is FilterAnd)
				{
					foreach (Filter F2 in ((FilterAnd)Filter).ChildFilters)
						Children.Add(F2);
				}
				else if (Filter is FilterOr)
				{
					if (Ors == null)
						Ors = new List<Filter[]>();

					Ors.Add(((FilterOr)Filter).ChildFilters);
				}
				else
					Children.Add(Filter);
			}

			if (Ors == null)
				return new FilterAnd(Children.ToArray());

			int c = Ors.Count;
			int d = Children.Count;
			int[] N = new int[c];
			int[] Pos = new int[c];
			int i, j;
			int NrSegments = 1;

			for (i = 0; i < c; i++)
			{
				N[i] = j = Ors[i].Length;
				NrSegments *= j;
			}

			Filter[] Segments = new Filter[NrSegments];

			for (i = 0; i < NrSegments; i++)
			{
				Filter[] SegmentChildren = new Filter[d + c];
				Children.CopyTo(SegmentChildren, 0);

				for (j = 0; j < c; j++)
					SegmentChildren[d + j] = Ors[j][Pos[j]];

				Segments[i] = new FilterAnd(SegmentChildren);

				j = 0;
				while (j < c && ++Pos[j] >= N[j])
				{
					Pos[j] = 0;
					j++;
				}
			}

			return new FilterOr(Segments);
		}
	}
}
