using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Objects.Sets
{
    /// <summary>
    /// Set containing all real numbers.
    /// </summary>
    public sealed class RealNumbers : Set
    {
        private static readonly int hashCode = typeof(RealNumbers).GetHashCode();

        /// <summary>
        /// Set containing all real numbers.
        /// </summary>
        public RealNumbers()
        {
        }

        /// <summary>
        /// Instance of the set of real numbers.
        /// </summary>
        public static readonly RealNumbers Instance = new RealNumbers();

        /// <summary>
        /// Checks if the set contains an element.
        /// </summary>
        /// <param name="Element">Element.</param>
        /// <returns>If the element is contained in the set.</returns>
        public override bool Contains(IElement Element)
        {
            return Element is DoubleNumber;
        }

        /// <summary>
        /// Compares the element to another.
        /// </summary>
        /// <param name="obj">Other element to compare against.</param>
        /// <returns>If elements are equal.</returns>
        public override bool Equals(object obj)
        {
            return obj is RealNumbers;
        }

        /// <summary>
        /// Calculates a hash code of the element.
        /// </summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode()
        {
            return hashCode;
        }
    }
}
