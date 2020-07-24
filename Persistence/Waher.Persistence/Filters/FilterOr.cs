using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Persistence.Filters
{
	/// <summary>
	/// This filter selects objects that conform to any of the child-filters provided.
	/// </summary>
	public class FilterOr : FilterChildren, ICustomFilter
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
		/// <returns>Logical inverse of the filter.</returns>
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
			Filter[] ChildFilters = this.ChildFilters;
			Filter Filter;

			if (ChildFilters.Length == 1)
				return ChildFilters[0].Normalize();

			foreach (Filter F in ChildFilters)
			{
				Filter = F.Normalize();

				if (Filter is FilterOr Or)
				{
					foreach (Filter F2 in Or.ChildFilters)
						Children.Add(F2);
				}
				else
					Children.Add(Filter);
			}

			return new FilterOr(Children.ToArray());
		}

		/// <summary>
		/// <see cref="object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			bool First = true;

			foreach (Filter F in this.ChildFilters)
			{
				if (First)
					First = false;
				else
					sb.Append(" OR ");

				sb.Append('(');
				sb.Append(F.ToString());
				sb.Append(')');
			}

			return sb.ToString();
		}

		/// <summary>
		/// Checks if an object passes the test or not.
		/// </summary>
		/// <param name="Object">Untyped object</param>
		/// <returns>If the object passes the test.</returns>
		public bool Passes(object Object)
		{
			foreach (Filter F in this.ChildFilters)
			{
				if (F is ICustomFilter CustomFilter && CustomFilter.Passes(Object))
					return true;
			}

			return false;
		}
	}
}
