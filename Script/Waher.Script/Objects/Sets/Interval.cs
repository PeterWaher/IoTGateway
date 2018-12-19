using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Objects.Sets
{
	/// <summary>
	/// Represents an interval.
	/// </summary>
	public sealed class Interval : Set, IVector
	{
		private double from;
		private double to;
		private double? stepSize;
		private bool includesFrom;
		private bool includesTo;

		/// <summary>
		/// Interval
		/// </summary>
		/// <param name="From">Start value.</param>
		/// <param name="To">Stop value.</param>
		/// <param name="IncludesFrom">If the start value is included in the interval.</param>
		/// <param name="IncludesTo">If the stop value is included in the interval.</param>
		/// <param name="StepSize">Optional step size, if the interval is to be enumerated.</param>
		public Interval(double From, double To, bool IncludesFrom, bool IncludesTo, double? StepSize)
		{
			if (From <= To)
			{
				this.from = From;
				this.to = To;
				this.includesFrom = IncludesFrom;
				this.includesTo = IncludesTo;
			}
			else
			{
				this.from = To;
				this.to = From;
				this.includesFrom = IncludesTo;
				this.includesTo = IncludesFrom;
			}

			if (!StepSize.HasValue || StepSize.Value == 0)
				this.stepSize = null;
			else
				this.stepSize = Math.Abs(StepSize.Value);
		}

		/// <summary>
		/// Start of interval.
		/// </summary>
		public double From
		{
			get { return this.from; }
		}

		/// <summary>
		/// End of interval.
		/// </summary>
		public double To
		{
			get { return this.to; }
		}

		/// <summary>
		/// Optional step size, if enumeration of values from the interval is important.
		/// </summary>
		public double? StepSize
		{
			get { return this.stepSize; }
		}

		/// <summary>
		/// If the <see cref="From"/> value is included in the interval or not.
		/// </summary>
		public bool IncludesFrom
		{
			get { return this.includesFrom; }
		}

		/// <summary>
		/// If the <see cref="To"/> value is included in the interval or not.
		/// </summary>
		public bool IncludesTo
		{
			get { return this.includesTo; }
		}

		/// <summary>
		/// Checks if the set contains an element.
		/// </summary>
		/// <param name="Element">Element.</param>
		/// <returns>If the element is contained in the set.</returns>
		public override bool Contains(IElement Element)
		{
			if (!(Element is DoubleNumber n))
				return false;

			double d = n.Value;

			if (d == this.from)
				return this.includesFrom;
			else if (d == this.to)
				return this.includesTo;
			else
				return d > this.from && d < this.to;
		}

		/// <summary>
		/// Compares the element to another.
		/// </summary>
		/// <param name="obj">Other element to compare against.</param>
		/// <returns>If elements are equal.</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is Interval I))
				return false;
			else
			{
				return this.from == I.from &&
					this.to == I.to &&
					this.stepSize == I.stepSize &&
					this.includesFrom == I.includesFrom &&
					this.includesTo == I.includesTo;
			}
		}

		/// <summary>
		/// Calculates a hash code of the element.
		/// </summary>
		/// <returns>Hash code.</returns>
		public override int GetHashCode()
		{
			return this.from.GetHashCode() ^
				this.to.GetHashCode() ^
				this.stepSize.GetHashCode() ^
				this.includesFrom.GetHashCode() ^
				this.includesTo.GetHashCode();
		}

		/// <summary>
		/// An enumeration of child elements. If the element is a scalar, this property will return null.
		/// </summary>
		public override ICollection<IElement> ChildElements
		{
			get
			{
				if (this.childElements is null)
					this.GenerateChildElements();

				return this.childElements;
			}
		}

		private void GenerateChildElements()
		{
			List<IElement> Result = new List<IElement>();

			double d = this.from;
			double s = this.stepSize ?? 1;

			if (this.includesFrom)
				Result.Add(new DoubleNumber(d));

			int i = 1;
			d = this.from + s;
			while (d < this.to)
			{
				Result.Add(new DoubleNumber(d));
				d = this.from + s * ++i;	// Don't use += s, to avoid round-off errors.
			}

			if (d == this.to && this.IncludesTo)
				Result.Add(new DoubleNumber(d));

			this.childElements = Result.ToArray();
		}

		/// <summary>
		/// Gets a double-valued array of all elements enumerable in the interval.
		/// </summary>
		/// <returns>Double-valued array</returns>
		public double[] GetArray()
		{
			if (this.vector is null)
			{
				List<double> Result = new List<double>();

				double d = this.from;
				double s = this.stepSize ?? 1;

				if (this.includesFrom)
					Result.Add(d);

				int i = 1;
				d = this.from + s;
				while (d < this.to)
				{
					Result.Add(d);
					d = this.from + s * ++i;    // Don't use += s, to avoid round-off errors.
				}

				if (d == this.to && this.IncludesTo)
					Result.Add(d);

				this.vector = Result.ToArray();
			}

			return this.vector;
		}

		private IElement[] childElements = null;
		private double[] vector = null;

		/// <summary>
		/// Encapsulates a set of elements into a similar structure as that provided by the current element.
		/// </summary>
		/// <param name="Elements">New set of child elements, not necessarily of the same type as the child elements of the current object.</param>
		/// <param name="Node">Script node from where the encapsulation is done.</param>
		/// <returns>Encapsulated object of similar type as the current object.</returns>
		public override IElement Encapsulate(ICollection<IElement> Elements, ScriptNode Node)
		{
			return Operators.Vectors.VectorDefinition.Encapsulate(Elements, true, Node);
		}

		/// <summary>
		/// Size of set, if finite and known, otherwise null is returned.
		/// </summary>
		public override int? Size
		{
			get
			{
				return this.Dimension;
			}
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			StringBuilder Result = new StringBuilder();

			Result.Append(Expression.ToString(this.from));
			Result.Append("..");
			Result.Append(Expression.ToString(this.to));

			if (this.stepSize.HasValue)
			{
				Result.Append('|');
				Result.Append(Expression.ToString(this.stepSize.Value));
			}

			return Result.ToString();
		}

		/// <summary>
		/// Dimension of vector.
		/// </summary>
		public int Dimension
		{
			get
			{
				return this.ChildElements.Count;
			}
		}

		/// <summary>
		/// An enumeration of vector elements.
		/// </summary>
		public ICollection<IElement> VectorElements
		{
			get
			{
				return this.ChildElements;
			}
		}

		/// <summary>
		/// Gets an element of the vector.
		/// </summary>
		/// <param name="Index">Zero-based index into the vector.</param>
		/// <returns>Vector element.</returns>
		public IElement GetElement(int Index)
		{
			if (this.childElements is null)
				this.GenerateChildElements();

			return this.childElements[Index];
		}

		/// <summary>
		/// Sets an element in the vector.
		/// </summary>
		/// <param name="Index">Index.</param>
		/// <param name="Value">Element to set.</param>
		public void SetElement(int Index, IElement Value)
		{
			throw new ScriptException("Interval vector is read-only.");
		}
	}
}
