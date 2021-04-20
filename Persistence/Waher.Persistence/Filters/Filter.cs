using System;

namespace Waher.Persistence.Filters
{
	/// <summary>
	/// Delegate for filter callback methods.
	/// </summary>
	/// <param name="Filter">Filter node.</param>
	/// <param name="State">State object passed to the original request.</param>
	/// <returns>true if iteration should continue, false if it should be stopped.</returns>
	public delegate bool FilterDelegate(Filter Filter, object State);

	/// <summary>
	/// Base class for all filter classes.
	/// </summary>
	public abstract class Filter
	{
		private Filter parent;
		private object tag;

		/// <summary>
		/// Base class for all filter classes.
		/// </summary>
		public Filter()
		{
			this.parent = null;
			this.tag = null;
		}

		/// <summary>
		/// Parent filter.
		/// </summary>
		public Filter ParentFilter
		{
			get => this.parent;
			internal set => this.parent = value;
		}

		/// <summary>
		/// Tag object that can be attached to the filter by caller.
		/// </summary>
		public object Tag
		{
			get => this.tag;
			set => this.tag = value;
		}

		/// <summary>
		/// Iterates through all nodes in the filter.
		/// </summary>
		/// <param name="Callback">Callback method that will be called for each node in the filter.</param>
		/// <param name="State">State object passed on to the callback method.</param>
		/// <returns>If all nodes were processed (true), or if the process was broken by the callback method (false).</returns>
		public virtual bool ForAll(FilterDelegate Callback, object State)
		{
			return Callback(this, State);
		}

		/// <summary>
		/// Calculates the logical inverse of the filter.
		/// </summary>
		/// <returns>Logical inverse of the filter.</returns>
		public abstract Filter Negate();

		/// <summary>
		/// Creates a copy of the filter.
		/// </summary>
		/// <returns>Copy of filter.</returns>
		public abstract Filter Copy();

		/// <summary>
		/// Returns a normalized filter.
		/// </summary>
		/// <returns>Normalized filter.</returns>
		public abstract Filter Normalize();
	}
}
