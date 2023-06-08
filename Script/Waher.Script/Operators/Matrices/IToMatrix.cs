using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Operators.Matrices
{
	/// <summary>
	/// Interface for objects that can be converted into matrices.
	/// </summary>
	public interface IToMatrix
	{
		/// <summary>
		/// Converts the object to a matrix.
		/// </summary>
		/// <returns>Matrix.</returns>
		IMatrix ToMatrix();
	}
}
