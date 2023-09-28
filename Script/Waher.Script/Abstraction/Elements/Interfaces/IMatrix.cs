namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Basic interface for matrices.
	/// </summary>
	public interface IMatrix : IVector
	{
		/// <summary>
		/// Number of rows.
		/// </summary>
		int Rows { get; }

		/// <summary>
		/// Number of columns.
		/// </summary>
		int Columns { get; }

		/// <summary>
		/// Matrix elements
		/// </summary>
		IElement[,] MatrixElements { get; }

		/// <summary>
		/// Returns a transposed matrix.
		/// </summary>
		/// <returns>Transposed matrix.</returns>
		IMatrix Transpose();

		/// <summary>
		/// Returns a conjugate transposed matrix.
		/// </summary>
		/// <returns>Conjugate transposed matrix.</returns>
		IMatrix ConjugateTranspose();

		/// <summary>
		/// Reduces a matrix.
		/// </summary>
		/// <param name="Eliminate">By default, reduction produces an
		/// upper triangular matrix. By using elimination, upwards reduction
		/// is also performed.</param>
		/// <param name="BreakIfZero">If elimination process should break if a
		/// zero-row is encountered.</param>
		/// <param name="Rank">Rank of matrix, or -1 if process broken.</param>
		/// <returns>Reduced matrix, or null if process broken</returns>
		IMatrix Reduce(bool Eliminate, bool BreakIfZero, out int Rank);

		/// <summary>
		/// Gets an element of the matrix.
		/// </summary>
		/// <param name="Column">Zero-based column index into the matrix.</param>
		/// <param name="Row">Zero-based row index into the matrix.</param>
		/// <returns>Matrix element.</returns>
		IElement GetElement(int Column, int Row);

		/// <summary>
		/// Sets an element in the matrix.
		/// </summary>
		/// <param name="Column">Zero-based column index into the matrix.</param>
		/// <param name="Row">Zero-based row index into the matrix.</param>
		/// <param name="Value">Element value.</param>
		void SetElement(int Column, int Row, IElement Value);

		/// <summary>
		/// Gets a row vector from the matrix.
		/// </summary>
		/// <param name="Row">Zero-based row index into the matrix.</param>
		/// <returns>Vector element.</returns>
		IVector GetRow(int Row);

		/// <summary>
		/// Gets a column vector from the matrix.
		/// </summary>
		/// <param name="Column">Zero-based column index into the matrix.</param>
		/// <returns>Vector element.</returns>
		IVector GetColumn(int Column);

		/// <summary>
		/// Gets a row vector from the matrix.
		/// </summary>
		/// <param name="Row">Zero-based row index into the matrix.</param>
		/// <param name="Vector">New row vector.</param>
		void SetRow(int Row, IVector Vector);

		/// <summary>
		/// Gets a column vector from the matrix.
		/// </summary>
		/// <param name="Column">Zero-based column index into the matrix.</param>
		/// <param name="Vector">New column vector.</param>
		void SetColumn(int Column, IVector Vector);
	}
}
