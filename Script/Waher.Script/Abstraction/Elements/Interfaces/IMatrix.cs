using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Script.Abstraction.Elements.Interfaces
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
	}
}
