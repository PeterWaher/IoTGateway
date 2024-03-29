﻿using System;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Objects.Matrices
{
	/// <summary>
	/// Pseudo-ring of Boolean-valued matrices.
	/// </summary>
	public sealed class BooleanMatrices : Ring
	{
		private BooleanMatrix zero = null;
		private readonly int rows;
		private readonly int columns;

		/// <summary>
		/// Pseudo-ring of Boolean-valued matrices.
		/// </summary>
		/// <param name="Rows">Number of rows.</param>
		/// <param name="Columns">Number of columns.</param>
		public BooleanMatrices(int Rows, int Columns)
		{
			this.rows = Rows;
			this.columns = Columns;
		}

		/// <summary>
		/// Number of rows.
		/// </summary>
		public int Rows => this.rows;

		/// <summary>
		/// Number of columns.
		/// </summary>
		public int Columns => this.columns;

		/// <summary>
		/// Returns the zero element of the group.
		/// </summary>
		public override IAbelianGroupElement Zero
		{
			get 
			{
				if (this.zero is null)
					this.zero = new BooleanMatrix(new bool[this.rows, this.columns]);
				
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
			if (Element is BooleanMatrix M)
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
			return (obj is BooleanMatrices S && S.rows == this.rows && S.columns == this.columns);
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
