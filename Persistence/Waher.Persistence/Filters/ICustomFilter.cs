using System;

namespace Waher.Persistence.Filters
{
	/// <summary>
	/// Delegate for custom filters.
	/// </summary>
	/// <typeparam name="T">Type of object</typeparam>
	/// <param name="Object">Object</param>
	/// <returns>If object passes test.</returns>
	public delegate bool PassTest<T>(T Object)
		where T : class;

	/// <summary>
	/// Interface for custom filters.
	/// </summary>
	public interface ICustomFilter
	{
		/// <summary>
		/// Checks if an object passes the test or not.
		/// </summary>
		/// <param name="Object">Untyped object</param>
		/// <returns>If the object passes the test.</returns>
		bool Passes(object Object);
	}
}
