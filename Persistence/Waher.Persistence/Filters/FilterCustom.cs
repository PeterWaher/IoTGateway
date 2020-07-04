using System;
using Waher.Events;

namespace Waher.Persistence.Filters
{
	/// <summary>
	/// Custom filter used to filter objects using an external expression.
	/// </summary>
	public class FilterCustom<T> : Filter, ICustomFilter
		where T : class
	{
		private readonly PassTest<T> test;

		/// <summary>
		/// Custom filter used to filter objects using an external expression.
		/// </summary>
		public FilterCustom(PassTest<T> Test)
		{
			this.test = Test;
		}

		/// <summary>
		/// Test to apply to objects.
		/// </summary>
		public PassTest<T> Test => this.test;

		/// <summary>
		/// Checks if an object passes the test or not.
		/// </summary>
		/// <param name="Object">Untyped object</param>
		/// <returns>If the object passes the test.</returns>
		public bool Passes(object Object)
		{
			if (Object is T TypedObject)
			{
				try
				{
					return this.test(TypedObject);
				}
				catch (Exception)
				{
					return false;
				}
			}
			else
				return false;
		}

		/// <summary>
		/// Creates a copy of the filter.
		/// </summary>
		/// <returns>Copy of filter.</returns>
		public override Filter Copy()
		{
			return new FilterCustom<T>(this.test);
		}

		/// <summary>
		/// Calculates the logical inverse of the filter.
		/// </summary>
		/// <returns>Logical inverse of the filter.</returns>
		public override Filter Negate()
		{
			return new FilterNot(this.Copy());
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
