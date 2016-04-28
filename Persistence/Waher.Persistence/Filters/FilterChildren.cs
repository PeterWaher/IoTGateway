using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Filters
{
	/// <summary>
	/// Abstract base class for filters having a variable number of child-filters.
	/// </summary>
	public abstract class FilterChildren : Filter
	{
		private Filter[] filters;

		/// <summary>
		/// Abstract base class for filters having a variable number of child-filters.
		/// </summary>
		/// <param name="Filters">Child filters.</param>
		public FilterChildren(params Filter[] Filters)
		{
			this.filters = Filters;

			foreach (Filter Filter in this.filters)
				Filter.ParentFilter = this;
		}

		/// <summary>
		/// Child filters.
		/// </summary>
		public Filter[] ChildFilters
		{
			get { return this.filters; }
		}

		/// <summary>
		/// Iterates through all nodes in the filter.
		/// </summary>
		/// <param name="Callback">Callback method that will be called for each node in the filter.</param>
		/// <param name="State">State object passed on to the callback method.</param>
		/// <returns>If all nodes were processed (true), or if the process was broken by the callback method (false).</returns>
		public override bool ForAll(FilterDelegate Callback, object State)
		{
			if (!base.ForAll(Callback, State))
				return false;

			foreach (Filter Filter in this.filters)
			{
				if (!Filter.ForAll(Callback, State))
					return false;
			}

			return true;
		}
	}
}
