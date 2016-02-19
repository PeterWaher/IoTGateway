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
    /// The empty set.
    /// </summary>
    public sealed class EmptySet : Set
    {
        private static readonly int hashCode = typeof(EmptySet).GetHashCode();

        /// <summary>
        /// The empty set.
        /// </summary>
        public EmptySet()
        {
        }

        /// <summary>
        /// Instance of the empty set.
        /// </summary>
        public static readonly EmptySet Instance = new EmptySet();

        /// <summary>
        /// Checks if the set contains an element.
        /// </summary>
        /// <param name="Element">Element.</param>
        /// <returns>If the element is contained in the set.</returns>
        public override bool Contains(IElement Element)
        {
            return false;
        }

        /// <summary>
        /// Compares the element to another.
        /// </summary>
        /// <param name="obj">Other element to compare against.</param>
        /// <returns>If elements are equal.</returns>
        public override bool Equals(object obj)
        {
            return obj is EmptySet;
        }

        /// <summary>
        /// Calculates a hash code of the element.
        /// </summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode()
        {
            return hashCode;
        }

        /// <summary>
        /// An enumeration of child elements. If the element is a scalar, this property will return null.
        /// </summary>
        public override ICollection<IElement> ChildElements
        {
            get
            {
                return noElements;
            }
        }

        private static readonly IElement[] noElements = new IElement[0];

    }
}
