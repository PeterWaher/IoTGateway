namespace Waher.Script.Objects
{
	/// <summary>
	/// Interface for objects that can be represented as a physical quantity.
	/// </summary>
	public interface IPhysicalQuantity
	{
		/// <summary>
		/// Converts underlying object to a physical quantity.
		/// </summary>
		/// <returns>Physical quantity</returns>
		PhysicalQuantity ToPhysicalQuantity();
	}
}
