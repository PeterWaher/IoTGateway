namespace Waher.Script.Units
{
	/// <summary>
	/// Interface for a category of units.
	/// </summary>
	public interface IUnitCategory
	{
		/// <summary>
		/// Name of unit category.
		/// </summary>
		string Name
		{
			get;
		}

		/// <summary>
		/// Reference unit for category.
		/// </summary>
		Unit Reference { get; }
	}
}
