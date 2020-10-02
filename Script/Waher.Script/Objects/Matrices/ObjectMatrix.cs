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
	/// Object-valued matrix.
	/// </summary>
	public sealed class ObjectMatrix : RingElement, IVector, IMatrix
	{
		private IElement[,] values;
		private ICollection<IElement> elements;
		private string[] columnNames = null;
		private readonly int rows;
		private readonly int columns;

		/// <summary>
		/// Object-valued matrix.
		/// </summary>
		/// <param name="Values">Object value.</param>
		public ObjectMatrix(IElement[,] Values)
		{
			this.values = Values;
			this.elements = null;
			this.rows = Values.GetLength(0);
			this.columns = Values.GetLength(1);
		}

		/// <summary>
		/// Object-valued matrix.
		/// </summary>
		/// <param name="Values">Object value.</param>
		public ObjectMatrix(object[,] Values)
		{
			int i, j, c, d;
			IElement[,] Values2 = new IElement[c = Values.GetLength(0), d = Values.GetLength(1)];

			for (i = 0; i < c; i++)
			{
				for (j = 0; j < d; j++)
					Values2[i, j] = Expression.Encapsulate(Values[i, j]);
			}

			this.values = Values2;
			this.elements = null;
			this.rows = Values.GetLength(0);
			this.columns = Values.GetLength(1);
		}

		/// <summary>
		/// Object-valued vector.
		/// </summary>
		/// <param name="Rows">Number of rows.</param>
		/// <param name="Columns">Number of columns.</param>
		/// <param name="Elements">Elements.</param>
		public ObjectMatrix(int Rows, int Columns, ICollection<IElement> Elements)
		{
			this.values = null;
			this.elements = Elements;
			this.rows = Rows;
			this.columns = Columns;
		}

		/// <summary>
		/// Matrix element values.
		/// </summary>
		public IElement[,] Values
		{
			get
			{
				if (this.values is null)
				{
					IElement[,] v = new IElement[this.rows, this.columns];
					int x = 0;
					int y = 0;

					foreach (IElement Element in this.elements)
					{
						v[y, x++] = Element;
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
				if (this.elements is null)
				{
					int x, y, i = 0;
					IElement[] v = new IElement[this.rows * this.columns];

					for (y = 0; y < this.rows; y++)
					{
						for (x = 0; x < this.columns; x++)
							v[i++] = this.values[y, x];
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
		/// Contains optional column names.
		/// </summary>
		public string[] ColumnNames
		{
			get { return this.columnNames; }
			set
			{
				if (!(value is null) && value.Length != this.columns)
					throw new ArgumentException("Number of columns does not match actual number of columns.", nameof(value));

				this.columnNames = value;
			}
		}

		/// <summary>
		/// Returns a named column vector.
		/// </summary>
		/// <param name="ColumnName">Name of column. Comparison is case insensitive.</param>
		/// <returns>Column vector.</returns>
		/// <exception cref="ArgumentException">If no named column with the same name was found.</exception>
		public IElement this[string ColumnName]
		{
			get
			{
				if (!(this.columnNames is null))
				{
					int i, c = this.columnNames.Length;

					for (i = 0; i < c; i++)
					{
						if (string.Compare(this.columnNames[i], ColumnName, true) == 0)
							return this.GetColumn(i);
					}
				}

				throw new ArgumentException("No column named " + ColumnName + " found.");
			}
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			IElement[,] v = this.Values;
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

					sb.Append(v[y, x].ToString());
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
					this.associatedMatrixSpace = new ObjectMatrices(this.rows, this.columns);

				return this.associatedMatrixSpace;
			}
		}

		private ObjectMatrices associatedMatrixSpace = null;

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
			IElement[,] Values = this.Values;
			IElement[,] v;
			IElement n;
			int x, y, z;

			if (Element.IsScalar)
			{
				v = new IElement[this.rows, this.columns];

				for (y = 0; y < this.rows; y++)
				{
					for (x = 0; x < this.columns; x++)
						v[y, x] = Operators.Arithmetics.Multiply.EvaluateMultiplication(Element, Values[y, x], null);
				}

				return new ObjectMatrix(v);
			}
			else if (Element is ObjectMatrix Matrix)
			{
				if (Matrix.columns != this.rows)
					return null;

				IElement[,] Values2 = Matrix.Values;

				v = new IElement[Matrix.rows, this.columns];

				for (y = 0; y < Matrix.rows; y++)
				{
					for (x = 0; x < this.columns; x++)
					{
						n = null;

						for (z = 0; z < this.rows; z++)
						{
							if (n is null)
								n = Operators.Arithmetics.Multiply.EvaluateMultiplication(Values2[y, z], Values[z, x], null);
							else
								n = Operators.Arithmetics.Add.EvaluateAddition(n,
									Operators.Arithmetics.Multiply.EvaluateMultiplication(Values2[y, z], Values[z, x], null), null);
						}

						v[y, x] = n;
					}
				}

				return new ObjectMatrix(v);
			}
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
			IElement[,] Values = this.Values;
			IElement[,] v;
			IElement n;
			int x, y, z;

			if (Element.IsScalar)
			{
				v = new IElement[this.rows, this.columns];

				for (y = 0; y < this.rows; y++)
				{
					for (x = 0; x < this.columns; x++)
						v[y, x] = Operators.Arithmetics.Multiply.EvaluateMultiplication(Element, Values[y, x], null);
				}

				return new ObjectMatrix(v);
			}
			else if (Element is ObjectMatrix Matrix)
			{
				if (this.columns != Matrix.rows)
					return null;

				IElement[,] Values2 = Matrix.Values;

				v = new IElement[this.rows, Matrix.columns];

				for (y = 0; y < this.rows; y++)
				{
					for (x = 0; x < Matrix.columns; x++)
					{
						n = null;

						for (z = 0; z < this.columns; z++)
						{
							if (n is null)
								n = Operators.Arithmetics.Multiply.EvaluateMultiplication(Values[y, z], Values2[z, x], null);
							else
							{
								n = Operators.Arithmetics.Add.EvaluateAddition(n,
									Operators.Arithmetics.Multiply.EvaluateMultiplication(Values[y, z], Values2[z, x], null), null);
							}
						}

						v[y, x] = n;
					}
				}

				return new ObjectMatrix(v);
			}
			else
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
			IElement[,] Values = this.Values;
			IElement[,] v;
			int x, y;

			if (Element.IsScalar)
			{
				v = new IElement[this.rows, this.columns];

				for (y = 0; y < this.rows; y++)
				{
					for (x = 0; x < this.columns; x++)
						v[y, x] = Operators.Arithmetics.Add.EvaluateAddition(Element, this.values[y, x], null);
				}

				return new ObjectMatrix(v);
			}
			else if (Element is ObjectMatrix Matrix)
			{
				if (this.columns != Matrix.columns || this.rows != Matrix.rows)
					return null;

				IElement[,] Values2 = Matrix.Values;

				v = new IElement[this.rows, this.columns];
				for (y = 0; y < this.rows; y++)
				{
					for (x = 0; x < this.columns; x++)
						v[y, x] = Operators.Arithmetics.Add.EvaluateAddition(Values[y, x], Values2[y, x], null);
				}

				return new ObjectMatrix(v);
			}
			else
				return null;
		}

		/// <summary>
		/// Negates the element.
		/// </summary>
		/// <returns>Negation of current element.</returns>
		public override IGroupElement Negate()
		{
			IElement[,] Values = this.Values;
			IElement[,] v = new IElement[this.rows, this.columns];
			int x, y;

			for (y = 0; y < this.rows; y++)
			{
				for (x = 0; x < this.columns; x++)
					v[y, x] = Operators.Arithmetics.Negate.EvaluateNegation(Values[y, x]);
			}

			return new ObjectMatrix(v);
		}

		/// <summary>
		/// Compares the element to another.
		/// </summary>
		/// <param name="obj">Other element to compare against.</param>
		/// <returns>If elements are equal.</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is ObjectMatrix Matrix))
				return false;

			if (this.columns != Matrix.columns || this.rows != Matrix.rows)
				return false;

			IElement[,] V1 = this.Values;
			IElement[,] V2 = Matrix.Values;
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

			for (y = 0; y < this.rows; y++)
			{
				for (x = 0; x < this.columns; x++)
					Result ^= this.values[y, x].GetHashCode();
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
			get { throw new ScriptException("Zero element not defined for generic object matrices."); }
		}

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
				if (!(this.rowVectors is null))
					return this.rowVectors;

				LinkedList<IElement> Rows = new LinkedList<IElement>();
				int x, y;

				if (!(this.values is null))
				{
					for (y = 0; y < this.rows; y++)
					{
						LinkedList<IElement> Row = new LinkedList<IElement>();

						for (x = 0; x < this.columns; x++)
							Row.AddLast(this.values[y, x]);

						Rows.AddLast(Operators.Vectors.VectorDefinition.Encapsulate(Row, false, null));
					}
				}
				else
				{
					LinkedList<IElement> Row = null;

					x = 0;
					foreach (IElement Element in this.elements)
					{
						if (Row is null)
							Row = new LinkedList<IElement>();

						Row.AddLast(Element);
						x++;
						if (x >= this.columns)
						{
							Rows.AddLast(Operators.Vectors.VectorDefinition.Encapsulate(Row, false, null));
							Row = null;
							x = 0;
						}
					}

					if (!(Row is null))
						Rows.AddLast(Operators.Vectors.VectorDefinition.Encapsulate(Row, false, null));
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
			IElement[,] v = new IElement[this.columns, this.rows];
			IElement[,] Values = this.Values;
			int x, y;

			for (y = 0; y < this.rows; y++)
			{
				for (x = 0; x < this.columns; x++)
					v[x, y] = Values[y, x];
			}

			return new ObjectMatrix(v);
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

			IElement[,] M = this.Values;
			IElement[] V = new IElement[this.columns];
			int i;

			for (i = 0; i < this.columns; i++)
				V[i] = M[Index, i];

			return Operators.Vectors.VectorDefinition.Encapsulate(V, false, null);
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

			if (!(Value is IVector V))
				throw new ScriptException("Invalid row vector.");

			if (V.Dimension != this.columns)
				throw new ScriptException("Dimension mismatch.");

			IElement[,] M = this.Values;
			this.elements = null;

			int i = 0;

			foreach (IElement E in V.ChildElements)
				M[Index, i++] = E;
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

			return this.Values[Row, Column];
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

			IElement[,] M = this.Values;
			this.elements = null;

			M[Row, Column] = Value;
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

			IElement[,] M = this.Values;
			IElement[] V = new IElement[this.columns];
			int i;

			for (i = 0; i < this.columns; i++)
				V[i] = M[Row, i];

			return Operators.Vectors.VectorDefinition.Encapsulate(V, false, null);
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

			IElement[,] M = this.Values;
			IElement[] V = new IElement[this.rows];
			int i;

			for (i = 0; i < this.rows; i++)
				V[i] = M[i, Column];

			return Operators.Vectors.VectorDefinition.Encapsulate(V, false, null);
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

			IElement[,] M = this.Values;
			this.elements = null;
			int i = 0;

			foreach (IElement E in Vector.VectorElements)
				M[Row, i++] = E;
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

			IElement[,] M = this.Values;
			this.elements = null;
			int i = 0;

			foreach (IElement E in Vector.VectorElements)
				M[i++, Column] = E;
		}
	}
}
