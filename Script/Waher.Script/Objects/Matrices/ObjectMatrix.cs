using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Operators.Matrices;

namespace Waher.Script.Objects.Matrices
{
	/// <summary>
	/// Object-valued matrix.
	/// </summary>
	public sealed class ObjectMatrix : RingElement
	{
		private IElement[,] values;
		private ICollection<IElement> elements;
		private int rows;
		private int columns;

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
				if (this.values == null)
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
				if (this.elements == null)
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

					sb.Append(v[y, x].ToString());
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
			get { return this.Values; }
		}

		/// <summary>
		/// Tries to multiply an element to the current element, from the left.
		/// </summary>
		/// <param name="Element">Element to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override IRingElement MultiplyLeft(IRingElement Element)
		{
			IElement[,] Values = this.Values;
			ObjectMatrix Matrix;
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
			else if ((Matrix = Element as ObjectMatrix) != null)
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
							if (n == null)
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
			ObjectMatrix Matrix;
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
			else if ((Matrix = Element as ObjectMatrix) != null)
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
							if (n == null)
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
			ObjectMatrix Matrix;
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
			else if ((Matrix = Element as ObjectMatrix) != null)
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
			ObjectMatrix Matrix = obj as ObjectMatrix;
			if (Matrix == null)
				return false;

			if (this.columns != Matrix.columns || this.rows != Matrix.rows)
				return false;

			int x, y;

			for (y = 0; y < this.rows; y++)
			{
				for (x = 0; x < this.columns; x++)
				{
					if (this.values[y, x] != Matrix.values[y, x])
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
	}
}
