using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects.VectorSpaces;
using Waher.Script.Operators.Matrices;

namespace Waher.Script.Objects.Matrices
{
	/// <summary>
	/// Boolean-valued matrix.
	/// </summary>
	public sealed class BooleanMatrix : RingElement, IVector, IMatrix
	{
		private bool[,] values;
		private ICollection<IElement> elements;
		private int rows;
		private int columns;

		/// <summary>
		/// Boolean-valued matrix.
		/// </summary>
		/// <param name="Values">Boolean value.</param>
		public BooleanMatrix(bool[,] Values)
		{
			this.values = Values;
			this.elements = null;
			this.rows = Values.GetLength(0);
			this.columns = Values.GetLength(1);
		}

		/// <summary>
		/// Boolean-valued vector.
		/// </summary>
		/// <param name="Rows">Number of rows.</param>
		/// <param name="Columns">Number of columns.</param>
		/// <param name="Elements">Elements.</param>
		public BooleanMatrix(int Rows, int Columns, ICollection<IElement> Elements)
		{
			this.values = null;
			this.elements = Elements;
			this.rows = Rows;
			this.columns = Columns;
		}

		/// <summary>
		/// Matrix element values.
		/// </summary>
		public bool[,] Values
		{
			get
			{
				if (this.values == null)
				{
					bool[,] v = new bool[this.rows, this.columns];
					int x = 0;
					int y = 0;

					foreach (BooleanValue Element in this.elements)
					{
						v[y, x++] = Element.Value;
						if (x >= this.columns)
						{
							y++;
							x = 0;
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
				if (this.elements == null)
				{
					int x, y, i = 0;
					IElement[] v = new IElement[this.rows * this.columns];

					for (y = 0; y < this.rows; y++)
					{
						for (x = 0; x < this.columns; x++)
							v[i++] = new BooleanValue(this.values[y, x]);
					}

					this.elements = v;
				}

				return this.elements;
			}
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
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			bool[,] v = this.Values;
			StringBuilder sb = null;
			bool First;
			int x, y;

			for (y = 0; y < this.rows; y++)
			{
				if (sb == null)
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

			if (sb == null)
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
				if (this.associatedMatrixSpace == null)
					this.associatedMatrixSpace = new BooleanMatrices(this.rows, this.columns);

				return this.associatedMatrixSpace;
			}
		}

		private BooleanMatrices associatedMatrixSpace = null;

		/// <summary>
		/// Associated object value.
		/// </summary>
		public override object AssociatedObjectValue
		{
			get { return this; }
		}

		/// <summary>
		/// Tries to multiply an element to the current element, from the left.
		/// </summary>
		/// <param name="Element">Element to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override IRingElement MultiplyLeft(IRingElement Element)
		{
			return null;
		}

		/// <summary>
		/// Tries to multiply an element to the current element, from the right.
		/// </summary>
		/// <param name="Element">Element to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override IRingElement MultiplyRight(IRingElement Element)
		{
			return null;
		}

		/// <summary>
		/// Inverts the element, if possible.
		/// </summary>
		/// <returns>Inverted element, or null if not possible.</returns>
		public override IRingElement Invert()
		{
			return null;
		}

		/// <summary>
		/// Tries to add an element to the current element.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override IAbelianGroupElement Add(IAbelianGroupElement Element)
		{
			return null;
		}

		/// <summary>
		/// Negates the element.
		/// </summary>
		/// <returns>Negation of current element.</returns>
		public override IGroupElement Negate()
		{
			return this;
		}

		/// <summary>
		/// Compares the element to another.
		/// </summary>
		/// <param name="obj">Other element to compare against.</param>
		/// <returns>If elements are equal.</returns>
		public override bool Equals(object obj)
		{
			BooleanMatrix Matrix = obj as BooleanMatrix;
			if (Matrix == null)
				return false;

			if (this.columns != Matrix.columns || this.rows != Matrix.rows)
				return false;

			bool[,] V1 = this.Values;
			bool[,] V2 = Matrix.Values;
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

		/// <summary>
		/// Calculates a hash code of the element.
		/// </summary>
		/// <returns>Hash code.</returns>
		public override int GetHashCode()
		{
			int Result = 0;
			int x, y;
			int i = 0;

			for (y = 0; y < this.rows; y++)
			{
				for (x = 0; x < this.columns; x++)
				{
					if (this.values[y, x])
						Result ^= (1 << i);

					i++;
					i &= 31;
				}
			}

			return Result;
		}

		/// <summary>
		/// If the element represents a scalar value.
		/// </summary>
		public override bool IsScalar
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// An enumeration of child elements. If the element is a scalar, this property will return null.
		/// </summary>
		public override ICollection<IElement> ChildElements
		{
			get
			{
				return this.Elements;
			}
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
				if (this.zero == null)
					this.zero = new BooleanMatrix(new bool[this.rows, this.columns]);

				return this.zero;
			}
		}

		private BooleanMatrix zero = null;

		/// <summary>
		/// Dimension of matrix, if seen as a vector of row vectors.
		/// </summary>
		public int Dimension
		{
			get { return this.rows; }
		}

		/// <summary>
		/// Vector of row vectors.
		/// </summary>
		public ICollection<IElement> VectorElements
		{
			get
			{
				if (this.rowVectors != null)
					return this.rowVectors;

				bool[,] v = this.Values;
				LinkedList<IElement> Rows = new LinkedList<IElement>();
				int x, y;
				bool[] r;

				for (y = 0; y < this.rows; y++)
				{
					r = new bool[this.columns];

					for (x = 0; x < this.columns; x++)
						r[x] = v[y, x];

					Rows.AddLast(new BooleanVector(r));
				}

				this.rowVectors = Rows;
				return Rows;
			}
		}

		private LinkedList<IElement> rowVectors = null;

		/// <summary>
		/// Returns a transposed matrix.
		/// </summary>
		/// <returns>Transposed matrix.</returns>
		public IMatrix Transpose()
		{
			bool[,] v = new bool[this.columns, this.rows];
			bool[,] Values = this.Values;
			int x, y;

			for (y = 0; y < this.rows; y++)
			{
				for (x = 0; x < this.columns; x++)
					v[x, y] = Values[y, x];
			}

			return new BooleanMatrix(v);
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

			bool[,] M = this.Values;
			bool[] V = new bool[this.columns];
			int i;

			for (i = 0; i < this.columns; i++)
				V[i] = M[Index, i];

			return new BooleanVector(V);
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

			BooleanVector V = Value as BooleanVector;
			if (V == null)
				throw new ScriptException("Row vectors in a boolean matrix are required to be boolean vectors.");

			if (V.Dimension != this.columns)
				throw new ScriptException("Dimension mismatch.");

			bool[] V2 = V.Values;
			bool[,] M = this.Values;
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

			return new BooleanValue(this.Values[Row, Columns]);
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

			BooleanValue V = Value as BooleanValue;
			if (V == null)
				throw new ScriptException("Elements in a boolean matrix must be boolean values.");

			bool[,] M = this.Values;
			this.elements = null;

			M[Row, Column] = V.Value;
		}

		/// <summary>
		/// Gets a row vector from the matrix.
		/// </summary>
		/// <param name="Row">Zero-based row index into the matrix.</param>
		/// <returns>Vector element.</returns>
		public IElement GetRow(int Row)
		{
			if (Row < 0 || Row >= this.rows)
				throw new ScriptException("Index out of bounds.");

			bool[,] M = this.Values;
			bool[] V = new bool[this.columns];
			int i;

			for (i = 0; i < this.columns; i++)
				V[i] = M[Row, i];

			return new BooleanVector(V);
		}

		/// <summary>
		/// Gets a column vector from the matrix.
		/// </summary>
		/// <param name="Column">Zero-based column index into the matrix.</param>
		/// <returns>Vector element.</returns>
		public IElement GetColumn(int Column)
		{
			if (Column < 0 || Column >= this.columns)
				throw new ScriptException("Index out of bounds.");

			bool[,] M = this.Values;
			bool[] V = new bool[this.rows];
			int i;

			for (i = 0; i < this.rows; i++)
				V[i] = M[i, Column];

			return new BooleanVector(V);
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

			BooleanVector V = Vector as BooleanVector;
			if (V == null)
				throw new ScriptException("Row vectors in a boolean matrix must be boolean vectors.");

			bool[] V2 = V.Values;
			bool[,] M = this.Values;
			this.elements = null;
			int i = 0;

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

			BooleanVector V = Vector as BooleanVector;
			if (V == null)
				throw new ScriptException("Column vectors in a boolean matrix must be boolean vectors.");

			bool[] V2 = V.Values;
			bool[,] M = this.Values;
			this.elements = null;
			int i = 0;

			for (i = 0; i < this.rows; i++)
				M[i, Column] = V2[i];
		}

	}
}
