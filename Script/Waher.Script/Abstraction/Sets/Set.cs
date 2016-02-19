using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects.Sets;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Base class for all types of sets.
	/// </summary>
	public abstract class Set : Element, ISet
	{
		/// <summary>
		/// Base class for all types of sets.
		/// </summary>
		public Set()
		{
		}

        /// <summary>
        /// Checks if the set contains an element.
        /// </summary>
        /// <param name="Element">Element.</param>
        /// <returns>If the element is contained in the set.</returns>
        public abstract bool Contains(IElement Element);

        public override IElement Encapsulate(ICollection<IElement> Elements, ScriptNode Node)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Compares the element to another.
        /// </summary>
        /// <param name="obj">Other element to compare against.</param>
        /// <returns>If elements are equal.</returns>
        public override abstract bool Equals(object obj);

		/// <summary>
		/// Calculates a hash code of the element.
		/// </summary>
		/// <returns>Hash code.</returns>
		public override abstract int GetHashCode();

        /// <summary>
        /// Associated object value.
        /// </summary>
        public override object AssociatedObjectValue
        {
            get
            {
                return this;
            }
        }

        /// <summary>
        /// Associated Set.
        /// </summary>
        public override ISet AssociatedSet
        {
            get
            {
                return SetOfSets.Instance;
            }
        }

        /// <summary>
        /// An enumeration of child elements. If the element is a scalar, this property will return null.
        /// </summary>
        public override ICollection<IElement> ChildElements
        {
            get
            {
                throw new ScriptException("Set not enumerable.");
            }
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
    }
}
