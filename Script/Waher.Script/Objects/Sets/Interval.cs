using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Model;

namespace Waher.Script.Objects.Sets
{
    /// <summary>
    /// Represents an interval.
    /// </summary>
    public sealed class Interval : Set
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
            DoubleNumber n = Element as DoubleNumber;
            if (n == null)
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
            Interval I = obj as Interval;
            if (I == null)
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
                if (this.childElements != null)
                    return this.childElements;

                LinkedList<IElement> Result = new LinkedList<IElement>();

                double d = this.from;
                double s = this.stepSize.HasValue ? this.stepSize.Value : 1;

                if (this.includesFrom)
                    Result.AddLast(new DoubleNumber(d));

                d += s;
                while (d < this.to)
                {
                    Result.AddLast(new DoubleNumber(d));
                    d += s;
                }

                if (d==this.to && this.IncludesTo)
                    Result.AddLast(new DoubleNumber(d));

                this.childElements = Result;

                return Result;
            }
        }

        private LinkedList<IElement> childElements = null;

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
    }
}
