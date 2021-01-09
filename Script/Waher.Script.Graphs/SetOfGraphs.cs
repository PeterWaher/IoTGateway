using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Graphs
{
    /// <summary>
    /// Set containing all graphs.
    /// </summary>
    public sealed class SetOfGraphs : SemiGroup
    {
        private static readonly int hashCode = typeof(SetOfGraphs).GetHashCode();

        /// <summary>
        /// Set containing all graphs.
        /// </summary>
        public SetOfGraphs()
        {
        }

        /// <summary>
        /// Instance of the set of all graphs.
        /// </summary>
        public static readonly SetOfGraphs Instance = new SetOfGraphs();

        /// <summary>
        /// Checks if the set contains an element.
        /// </summary>
        /// <param name="Element">Element.</param>
        /// <returns>If the element is contained in the set.</returns>
        public override bool Contains(IElement Element)
        {
            return Element is Graph;
        }

        /// <summary>
        /// Compares the element to another.
        /// </summary>
        /// <param name="obj">Other element to compare against.</param>
        /// <returns>If elements are equal.</returns>
        public override bool Equals(object obj)
        {
            return obj is SetOfGraphs;
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
