using System;

namespace Waher.Script.Abstraction.Elements
{
    /// <summary>
    /// Basic interface for matrices.
    /// </summary>
    public interface IMatrix : IElement
	{
        /// <summary>
        /// Number of rows.
        /// </summary>
        int Rows
        {
            get;
        }

        /// <summary>
        /// Number of columns.
        /// </summary>
        int Columns
        {
            get;
        }

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
