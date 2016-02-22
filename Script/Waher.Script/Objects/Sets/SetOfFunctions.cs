using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Model;

namespace Waher.Script.Objects.Sets
{
    /// <summary>
    /// Set containing all functions.
    /// </summary>
    public sealed class SetOfFunctions : Set
    {
        private static readonly int hashCode = typeof(SetOfFunctions).GetHashCode();

        /// <summary>
        /// Set containing all functions.
        /// </summary>
        public SetOfFunctions()
        {
        }

        /// <summary>
        /// Instance of the set of all functions.
        /// </summary>
        public static readonly SetOfFunctions Instance = new SetOfFunctions();

        /// <summary>
        /// Checks if the set contains an element.
        /// </summary>
        /// <param name="Element">Element.</param>
        /// <returns>If the element is contained in the set.</returns>
        public override bool Contains(IElement Element)
        {
            return Element is Operators.LambdaDefinition;
        }

        /// <summary>
        /// Compares the element to another.
        /// </summary>
        /// <param name="obj">Other element to compare against.</param>
        /// <returns>If elements are equal.</returns>
        public override bool Equals(object obj)
        {
            return obj is SetOfFunctions;
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
