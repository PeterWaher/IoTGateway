namespace Waher.Persistence.Filters
{
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
