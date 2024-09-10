using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Operators.Vectors
{
	/// <summary>
	/// Interface for objects that can be converted into matrices.
	/// </summary>
	public interface IToVector
	{
		/// <summary>
		/// Converts the object to a vector.
		/// </summary>
		/// <returns>Vector.</returns>
		IElement ToVector();
	}
}
