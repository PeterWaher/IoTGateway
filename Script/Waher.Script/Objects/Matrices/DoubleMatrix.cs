using System;
using System.Collections.Generic;
using System.Text;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects.VectorSpaces;
using Waher.Script.Operators.Matrices;

namespace Waher.Script.Objects.Matrices
{
	/// <summary>
	/// Double-valued matrix.
	/// </summary>
	public sealed class DoubleMatrix : RingElement, IMatrix
    {
		private double[,] values;
		private IElement[,] matrixElements;
		private ICollection<IElement> elements;
		private readonly int rows;
		private readonly int columns;

		/// <summary>
		/// Double-valued matrix.
		/// </summary>
		/// <param name="Values">Double value.</param>
		public DoubleMatrix(double[,] Values)
		{
			this.values = Values;
			this.elements = null;
			this.matrixElements = null;
			this.rows = Values.GetLength(0);
			this.columns = Values.GetLength(1);
		}

		/// <summary>
		/// Double-valued vector.
		/// </summary>
		/// <param name="Rows">Number of rows.</param>
		/// <param name="Columns">Number of columns.</param>
		/// <param name="Elements">Elements.</param>
		public DoubleMatrix(int Rows, int Columns, ICollection<IElement> Elements)
		{
			this.values = null;
			this.matrixElements = null;
			this.elements = Elements;
			this.rows = Rows;
			this.columns = Columns;
		}

		/// <summary>
		/// Matrix element values.
		/// </summary>
		public double[,] Values
		{
			get
			{
				if (this.values is null)
				{
					double[,] v = new double[this.rows, this.columns];
					int x = 0;
					int y = 0;

					if (this.elements is ChunkedList<IElement> Values)
					{
						ChunkNode<IElement> Loop = Values.FirstChunk;
						int i, c;

						while (!(Loop is null))
						{
							for (i = Loop.Start, c = Loop.Pos; i < c; i++)
							{
								if (!(Loop[i].AssociatedObjectValue is double d))
									d = 0;

								v[y, x++] = d;
								if (x >= this.columns)
								{
									y++;
									x = 0;
								}
							}

							Loop = Loop.Next;
						}
					}
					else
					{
						foreach (IElement E in this.elements)
						{
							if (!(E.AssociatedObjectValue is double d))
								d = 0;

							v[y, x++] = d;
							if (x >= this.columns)
							{
								y++;
								x = 0;
							}
						}
					}

					this.values = v;
				}

				return this.values;
			}
		}

		/// <summary>
		/// Matrix elements.
		/// </summary>
		public ICollection<IElement> Elements
		{
			get
			{
				if (this.elements is null)
				{
					int x, y, i = 0;
					IElement[] v = new IElement[this.rows * this.columns];

					for (y = 0; y < this.rows; y++)
					{
						for (x = 0; x < this.columns; x++)
							v[i++] = new DoubleNumber(this.values[y, x]);
					}

					this.elements = v;
				}

				return this.elements;
			}
		}

		/// <summary>
		/// Matrix elements
		/// </summary>
		public IElement[,] MatrixElements
		{
			get
			{
				if (this.matrixElements is null)
				{
					IElement[,] v = new IElement[this.rows, this.columns];
					int x = 0;
					int y = 0;

					foreach (IElement E in this.Elements)
					{
						v[y, x++] = E;
						if (x >= this.columns)
						{
							y++;
							x = 0;
						}
					}

					this.matrixElements = v;
				}

				return this.matrixElements;
			}
		}

		/// <summary>
		/// Number of rows.
		/// </summary>
		public int Rows => this.rows;

		/// <summary>
		/// Number of columns.
		/// </summary>
		public int Columns => this.columns;

		/// <inheritdoc/>
		public override string ToString()
		{
			double[,] v = this.Values;
			StringBuilder sb = null;
			bool First;
			int x, y;

			for (y = 0; y < this.rows; y++)
			{
				if (sb is null)
					sb = new StringBuilder("[[");
				else
					sb.Append(",\r\n [");

				First = true;
				for (x = 0; x < this.columns; x++)
				{
					if (First)
						First = false;
					else
						sb.Append(", ");

					sb.Append(Expression.ToString(v[y, x]));
				}

				sb.Append(']');
			}

			if (sb is null)
				sb = new StringBuilder("[[]]");
			else
				sb.Append(']');

			return sb.ToString();
		}

		/// <summary>
		/// Associated Ring.
		/// </summary>
		public override IRing AssociatedRing
		{
			get
			{
				if (this.associatedMatrixSpace is null)
					this.associatedMatrixSpace = new DoubleMatrices(this.rows, this.columns);

				return this.associatedMatrixSpace;
			}
		}

		private DoubleMatrices associatedMatrixSpace = null;

		/// <summary>
		/// Associated object value.
		/// </summary>
		public override object AssociatedObjectValue => this;

		/// <summary>
		/// Tries to multiply an element to the current element, from the left.
		/// </summary>
		/// <param name="Element">Element to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override IRingElement MultiplyLeft(IRingElement Element)
		{
			double[,] v;
			int x, y, z;

			if (Element.AssociatedObjectValue is double n)
			{
				double[,] Values = this.Values;

				v = new double[this.rows, this.columns];

				for (y = 0; y < this.rows; y++)
				{
					for (x = 0; x < this.columns; x++)
						v[y, x] = n * Values[y, x];
				}

				return new DoubleMatrix(v);
			}
			else if (Element is DoubleMatrix Matrix)
			{
				if (Matrix.columns != this.rows)
					return null;

				double[,] Values = this.Values;
				double[,] Values2 = Matrix.Values;

				v = new double[Matrix.rows, this.columns];
				for (y = 0; y < Matrix.rows; y++)
				{
					for (x = 0; x < this.columns; x++)
					{
						n = 0;

						for (z = 0; z < this.rows; z++)
							n += Values2[y, z] * Values[z, x];

						v[y, x] = n;
					}
				}

				return new DoubleMatrix(v);
			}
			else if (Element is IMatrix)
				return new ObjectMatrix(this.MatrixElements).MultiplyLeft(Element);
			else
				return null;
		}

		/// <summary>
		/// Tries to multiply an element to the current element, from the right.
		/// </summary>
		/// <param name="Element">Element to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override IRingElement MultiplyRight(IRingElement Element)
		{
			double[,] v;
			int x, y, z;

			if (Element.AssociatedObjectValue is double n)
			{
				double[,] Values = this.Values;

				v = new double[this.rows, this.columns];

				for (y = 0; y < this.rows; y++)
				{
					for (x = 0; x < this.columns; x++)
						v[y, x] = n * Values[y, x];
				}

				return new DoubleMatrix(v);
			}
			else if (Element is DoubleMatrix Matrix)
			{
				if (this.columns != Matrix.rows)
					return null;

				double[,] Values = this.Values;
				double[,] Values2 = Matrix.Values;

                v = new double[this.rows, Matrix.columns];
				for (y = 0; y < this.rows; y++)
				{
					for (x = 0; x < Matrix.columns; x++)
					{
						n = 0;

						for (z = 0; z < this.columns; z++)
							n += Values[y, z] * Values2[z, x];

						v[y, x] = n;
					}
				}

				return new DoubleMatrix(v);
			}
			else if (Element is IMatrix)
				return new ObjectMatrix(this.MatrixElements).MultiplyRight(Element);
			else
				return null;
		}

		/// <summary>
		/// Inverts the element, if possible.
		/// </summary>
		/// <returns>Inverted element, or null if not possible.</returns>
		public override IRingElement Invert()
		{
			if (this.rows != this.columns)
				return null;

			double[,] Values = this.Values;
			int c2 = this.columns << 1;
			double[,] v = new double[this.rows, c2];
			int x, y;

			for (y = 0; y < this.rows; y++)
			{
				for (x = 0; x < this.columns; x++)
				{
					v[y, x] = Values[y, x];
					v[y, x + this.columns] = (x == y ? 1 : 0);
				}
			}

			if (Reduce(v, true, true, out _) < 0)
				return null;

			double[,] v2 = new double[this.rows, this.columns];

			for (y = 0; y < this.rows; y++)
			{
				for (x = 0; x < this.columns; x++)
					v2[y, x] = v[y, x + this.columns];
			}

			return new DoubleMatrix(v2);
		}

		/// <summary>
		/// Reduces a matrix.
		/// </summary>
		/// <param name="Eliminate">By default, reduction produces an
		/// upper triangular matrix. By using elimination, upwards reduction
		/// is also performed.</param>
		/// <param name="BreakIfZero">If elimination process should break if a
		/// zero-row is encountered.</param>
		/// <param name="Rank">Rank of matrix, or -1 if process broken.</param>
		/// <param name="Factor">Multiplication factor for determinant of resulting matrix.</param>
		/// <returns>Reduced matrix</returns>
		public IMatrix Reduce(bool Eliminate, bool BreakIfZero, out int Rank, out ICommutativeRingWithIdentityElement Factor)
		{
			double[,] M = (double[,])this.Values.Clone();
			Rank = Reduce(M, Eliminate, BreakIfZero, out double c);
			Factor = new DoubleNumber(c);
			return new DoubleMatrix(M);
		}

		/// <summary>
		/// Reduces a matrix.
		/// </summary>
		/// <param name="Matrix">Matrix to be reduced.</param>
		/// <param name="Eliminate">By default, reduction produces an
		/// upper triangular matrix. By using elimination, upwards reduction
		/// is also performed.</param>
		/// <param name="BreakIfZero">If elimination process should break if a
		/// zero-row is encountered.</param>
		/// <param name="Factor">Multiplication factor for determinant of resulting matrix.</param>
		/// <returns>Rank of matrix, or -1 if process broken.</returns>
		public static int Reduce(double[,] Matrix, bool Eliminate, bool BreakIfZero,
			out double Factor)
		{
			int x, y, u, z;
			int Rows = Matrix.GetLength(0);
			int Columns = Matrix.GetLength(1);
			int MinCount = Math.Min(Rows, Columns);
			double a, b;
			int Rank = 0;

			Factor = 1;

			for (x = 0; x < MinCount; x++)
			{
				a = Math.Abs(Matrix[x, x]);
				z = x;
				for (y = x + 1; y < Rows; y++)
				{
					b = Math.Abs(Matrix[y, x]);
					if (b > a)
					{
						a = b;
						z = y;
					}
				}

				if (z != x)
				{
					for (u = x; u < Columns; u++)
					{
						a = Matrix[x, u];
						Matrix[x, u] = Matrix[z, u];
						Matrix[z, u] = a;
					}

					Factor = -Factor;
				}

				a = Matrix[x, x];
				if (a == 0)
				{
					if (BreakIfZero)
						return -1;
				}
				else
				{
					Rank++;

					if (a != 1)
					{
						for (u = x; u < Columns; u++)
							Matrix[x, u] /= a;

						Factor *= a;
					}

					for (y = Eliminate ? 0 : x + 1; y < Rows; y++)
					{
						if (y != x && (a = Matrix[y, x]) != 0)
						{
							for (u = x; u < Columns; u++)
								Matrix[y, u] -= a * Matrix[x, u];
						}
					}
				}
			}

			return Rank;
		}

		/// <summary>
		/// Tries to add an element to the current element.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override IAbelianGroupElement Add(IAbelianGroupElement Element)
		{
			double[,] v;
			int x, y;

			if (Element.AssociatedObjectValue is double d)
			{
				double[,] Values = this.Values;

				v = new double[this.rows, this.columns];

				for (y = 0; y < this.rows; y++)
				{
					for (x = 0; x < this.columns; x++)
						v[y, x] = d + Values[y, x];
				}

				return new DoubleMatrix(v);
			}
			else if (Element is DoubleMatrix Matrix)
			{
				if (this.columns != Matrix.columns || this.rows != Matrix.rows)
					return null;

				double[,] Values = this.Values;
				double[,] Values2 = Matrix.Values;

                v = new double[this.rows, this.columns];
				for (y = 0; y < this.rows; y++)
				{
					for (x = 0; x < this.columns; x++)
						v[y, x] = Values[y, x] + Values2[y, x];
				}

				return new DoubleMatrix(v);
			}
			else if (Element is IMatrix)
				return new ObjectMatrix(this.MatrixElements).Add(Element);
			else
				return null;
		}

		/// <summary>
		/// Negates the element.
		/// </summary>
		/// <returns>Negation of current element.</returns>
		public override IGroupElement Negate()
		{
			double[,] Values = this.Values;
			double[,] v = new double[this.rows, this.columns];
			int x, y;

			for (y = 0; y < this.rows; y++)
			{
				for (x = 0; x < this.columns; x++)
					v[y, x] = -Values[y, x];
			}

			return new DoubleMatrix(v);
		}

		/// <summary>
		/// Compares the element to another.
		/// </summary>
		/// <param name="obj">Other element to compare against.</param>
		/// <returns>If elements are equal.</returns>
		public override bool Equals(object obj)
		{
			if (obj is DoubleMatrix Matrix)
			{
				if (this.columns != Matrix.columns || this.rows != Matrix.rows)
					return false;

				double[,] V1 = this.Values;
				double[,] V2 = Matrix.Values;
				int x, y;

				for (y = 0; y < this.rows; y++)
				{
					for (x = 0; x < this.columns; x++)
					{
						if (V1[y, x] != V2[y, x])
							return false;
					}
				}

				return true;
			}
			else if (obj is IMatrix)
				return new ObjectMatrix(this.MatrixElements).Equals(obj);
			else
				return false;
		}

		/// <summary>
		/// Calculates a hash code of the element.
		/// </summary>
		/// <returns>Hash code.</returns>
		public override int GetHashCode()
		{
			double[,] Values = this.Values;
			int Result = 0;
			int x, y;

			for (y = 0; y < this.rows; y++)
			{
				for (x = 0; x < this.columns; x++)
					Result ^= Values[y, x].GetHashCode();
			}

			return Result;
		}

		/// <summary>
		/// If the element represents a scalar value.
		/// </summary>
		public override bool IsScalar => false;

		/// <summary>
		/// An enumeration of child elements. If the element is a scalar, this property will return null.
		/// </summary>
		public override ICollection<IElement> ChildElements => this.Elements;

		/// <summary>
		/// Encapsulates a set of elements into a similar structure as that provided by the current element.
		/// </summary>
		/// <param name="Elements">New set of child elements, not necessarily of the same type as the child elements of the current object.</param>
		/// <param name="Node">Script node from where the encapsulation is done.</param>
		/// <returns>Encapsulated object of similar type as the current object.</returns>
		public override IElement Encapsulate(ChunkedList<IElement> Elements, ScriptNode Node)
		{
			return MatrixDefinition.Encapsulate(Elements, this.rows, this.columns, Node);
		}

		/// <summary>
		/// Encapsulates a set of elements into a similar structure as that provided by the current element.
		/// </summary>
		/// <param name="Elements">New set of child elements, not necessarily of the same type as the child elements of the current object.</param>
		/// <param name="Node">Script node from where the encapsulation is done.</param>
		/// <returns>Encapsulated object of similar type as the current object.</returns>
		public override IElement Encapsulate(ICollection<IElement> Elements, ScriptNode Node)
		{
			return MatrixDefinition.Encapsulate(Elements, this.rows, this.columns, Node);
		}

		/// <summary>
		/// Returns the zero element of the group.
		/// </summary>
		public override IAbelianGroupElement Zero
		{
			get
			{
				if (this.zero is null)
					this.zero = new DoubleMatrix(new double[this.rows, this.columns]);

				return this.zero;
			}
		}

		private DoubleMatrix zero = null;

		/// <summary>
		/// Dimension of matrix, if seen as a vector of row vectors.
		/// </summary>
		public int Dimension => this.rows;

		/// <summary>
		/// Vector of row vectors.
		/// </summary>
		public ICollection<IElement> VectorElements
		{
			get
			{
				if (!(this.rowVectors is null))
					return this.rowVectors;

				double[,] v = this.Values;
				ChunkedList<IElement> Rows = new ChunkedList<IElement>();
				int x, y;
				double[] r;

				for (y = 0; y < this.rows; y++)
				{
					r = new double[this.columns];

					for (x = 0; x < this.columns; x++)
						r[x] = v[y, x];

					Rows.Add(new DoubleVector(r));
				}

				this.rowVectors = Rows;
				return Rows;
			}
		}

		private ChunkedList<IElement> rowVectors = null;

        /// <summary>
        /// Returns a transposed matrix.
        /// </summary>
        /// <returns>Transposed matrix.</returns>
        public IMatrix Transpose()
        {
            double[,] v = new double[this.columns, this.rows];
            double[,] Values = this.Values;
            int x, y;

            for (y = 0; y < this.rows; y++)
            {
                for (x = 0; x < this.columns; x++)
                    v[x, y] = Values[y, x];
            }

            return new DoubleMatrix(v);
        }

        /// <summary>
        /// Returns a conjugate transposed matrix.
        /// </summary>
        /// <returns>Conjugate transposed matrix.</returns>
        public IMatrix ConjugateTranspose()
        {
            return this.Transpose();
        }

        /// <summary>
        /// Gets an element of the vector.
        /// </summary>
        /// <param name="Index">Zero-based index into the vector.</param>
        /// <returns>Vector element.</returns>
        public IElement GetElement(int Index)
        {
            if (Index < 0 || Index >= this.rows)
                throw new ScriptException("Index out of bounds.");

            double[,] M = this.Values;
            double[] V = new double[this.columns];
            int i;

            for (i = 0; i < this.columns; i++)
                V[i] = M[Index, i];

            return new DoubleVector(V);
        }

        /// <summary>
        /// Sets an element in the vector.
        /// </summary>
        /// <param name="Index">Index.</param>
        /// <param name="Value">Element to set.</param>
        public void SetElement(int Index, IElement Value)
        {
            if (Index < 0 || Index >= this.rows)
                throw new ScriptException("Index out of bounds.");

			if (!(Value is DoubleVector V))
				throw new ScriptException("Row vectors in a double matrix are required to be double vectors.");

			if (V.Dimension != this.columns)
                throw new ScriptException("Dimension mismatch.");

            double[] V2 = V.Values;
            double[,] M = this.Values;
            this.elements = null;

            int i;

            for (i = 0; i < this.columns; i++)
                M[Index, i] = V2[i];
        }

        /// <summary>
        /// Gets an element of the matrix.
        /// </summary>
        /// <param name="Column">Zero-based column index into the matrix.</param>
        /// <param name="Row">Zero-based row index into the matrix.</param>
        /// <returns>Vector element.</returns>
        public IElement GetElement(int Column, int Row)
        {
            if (Column < 0 || Column >= this.columns || Row < 0 || Row >= this.rows)
                throw new ScriptException("Index out of bounds.");

            return new DoubleNumber(this.Values[Row, Column]);
        }

        /// <summary>
        /// Sets an element in the matrix.
        /// </summary>
        /// <param name="Column">Zero-based column index into the matrix.</param>
        /// <param name="Row">Zero-based row index into the matrix.</param>
        /// <param name="Value">Element value.</param>
        public void SetElement(int Column, int Row, IElement Value)
        {
            if (Column < 0 || Column >= this.columns || Row < 0 || Row >= this.rows)
                throw new ScriptException("Index out of bounds.");

			if (!(Value.AssociatedObjectValue is double d))
				throw new ScriptException("Elements in a double matrix must be double values.");

			double[,] M = this.Values;
            this.elements = null;

            M[Row, Column] = d;
        }

        /// <summary>
        /// Gets a row vector from the matrix.
        /// </summary>
        /// <param name="Row">Zero-based row index into the matrix.</param>
        /// <returns>Vector element.</returns>
        public IVector GetRow(int Row)
        {
            if (Row < 0 || Row >= this.rows)
                throw new ScriptException("Index out of bounds.");

            double[,] M = this.Values;
            double[] V = new double[this.columns];
            int i;

            for (i = 0; i < this.columns; i++)
                V[i] = M[Row, i];

            return new DoubleVector(V);
        }

        /// <summary>
        /// Gets a column vector from the matrix.
        /// </summary>
        /// <param name="Column">Zero-based column index into the matrix.</param>
        /// <returns>Vector element.</returns>
        public IVector GetColumn(int Column)
        {
            if (Column < 0 || Column >= this.columns)
                throw new ScriptException("Index out of bounds.");

            double[,] M = this.Values;
            double[] V = new double[this.rows];
            int i;

            for (i = 0; i < this.rows; i++)
                V[i] = M[i, Column];

            return new DoubleVector(V);
        }

        /// <summary>
        /// Gets a row vector from the matrix.
        /// </summary>
        /// <param name="Row">Zero-based row index into the matrix.</param>
        /// <param name="Vector">New row vector.</param>
        public void SetRow(int Row, IVector Vector)
        {
            if (Row < 0 || Row >= this.rows)
                throw new ScriptException("Index out of bounds.");

            if (Vector.Dimension != this.columns)
                throw new ScriptException("Vector dimension does not match number of columns");

			if (!(Vector is DoubleVector V))
				throw new ScriptException("Row vectors in a double matrix must be double vectors.");

			double[] V2 = V.Values;
            double[,] M = this.Values;
            this.elements = null;
            int i;

            for (i = 0; i < this.columns; i++)
                M[Row, i] = V2[i];
        }

        /// <summary>
        /// Gets a column vector from the matrix.
        /// </summary>
        /// <param name="Column">Zero-based column index into the matrix.</param>
        /// <param name="Vector">New column vector.</param>
        public void SetColumn(int Column, IVector Vector)
        {
            if (Column < 0 || Column >= this.columns)
                throw new ScriptException("Index out of bounds.");

            if (Vector.Dimension != this.rows)
                throw new ScriptException("Vector dimension does not match number of rows");

			if (!(Vector is DoubleVector V))
				throw new ScriptException("Column vectors in a double matrix must be double vectors.");

			double[] V2 = V.Values;
            double[,] M = this.Values;
            this.elements = null;
            int i;

            for (i = 0; i < this.rows; i++)
                M[i, Column] = V2[i];
        }
    }
}
