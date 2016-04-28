using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Filters
{
	/// <summary>
	/// Abstract base class for filters having a single child-filters.
	/// </summary>
	public abstract class FilterChild : Filter
	{
		private Filter filter;

		/// <summary>
		/// Abstract base class for filters having a single child-filters.
		/// </summary>
		/// <param name="Filters">Child filter.</param>
		public FilterChild(Filter Filter)
		{
			this.filter = Filter;
			this.filter.ParentFilter = this;
		}

		/// <summary>
		/// Child filter.
		/// </summary>
		public Filter ChildFilter
		{
			get { return this.filter; }
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

			return this.filter.ForAll(Callback, State);
		}
	}
}
