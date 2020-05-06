using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Objects.Matrices
{
    /// <summary>
    /// Pseudo-ring of Complex-valued matrices.
    /// </summary>
    public sealed class ComplexMatrices : Ring
    {
        private ComplexMatrix zero = null;
        private readonly int rows;
        private readonly int columns;

		/// <summary>
		/// Pseudo-ring of Complex-valued matrices.
		/// </summary>
		/// <param name="Rows">Number of rows.</param>
		/// <param name="Columns">Number of columns.</param>
		public ComplexMatrices(int Rows, int Columns)
        {
            this.rows = Rows;
            this.columns = Columns;
        }

        /// <summary>
        /// Number of rows.
        /// </summary>
        public int Rows
        {
            get { return this.rows; }
        }

        /// <summary>
        /// Number of columns.
        /// </summary>
        public int Columns
        {
            get { return this.columns; }
        }

        /// <summary>
        /// Returns the zero element of the group.
        /// </summary>
        public override IAbelianGroupElement Zero
        {
            get
            {
                if (this.zero is null)
                {
                    Complex[,] v = new Complex[this.rows, this.columns];
                    int x, y;

                    for (y = 0; y < this.rows; y++)
                    {
                        for (x = 0; x < this.columns; x++)
                            v[y, x] = Complex.Zero;
                    }

                    this.zero = new ComplexMatrix(v);
                }

                return this.zero;
            }
        }

        /// <summary>
        /// If the ring * operator is commutative or not.
        /// </summary>
        public override bool IsCommutative
        {
            get { return this.columns == 1 && this.rows == 1; }
        }

        /// <summary>
        /// Checks if the set contains an element.
        /// </summary>
        /// <param name="Element">Element.</param>
        /// <returns>If the element is contained in the set.</returns>
        public override bool Contains(IElement Element)
        {
            if (Element is ComplexMatrix M)
                return M.Rows == this.rows && M.Columns == this.columns;
            else
                return false;
        }

        /// <summary>
        /// Compares the element to another.
        /// </summary>
        /// <param name="obj">Other element to compare against.</param>
        /// <returns>If elements are equal.</returns>
        public override bool Equals(object obj)
        {
			return (obj is ComplexMatrices S && S.rows == this.rows && S.columns == this.columns);
		}

        /// <summary>
        /// Calculates a hash code of the element.
        /// </summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode()
        {
            return this.rows.GetHashCode() ^ (this.columns.GetHashCode() << 16);
        }

    }
}
