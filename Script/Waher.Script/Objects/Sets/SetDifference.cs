using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Model;

namespace Waher.Script.Objects.Sets
{
    /// <summary>
    /// Represents a set difference A\B.
    /// </summary>
    public sealed class SetDifference : Set
    {
        private ISet set1;
        private ISet set2;

        /// <summary>
        /// Represents a Intersection of two sets.
        /// </summary>
        /// <param name="Set1">Set 1.</param>
        /// <param name="Set2">Set 2.</param>
        public SetDifference(ISet Set1, ISet Set2)
        {
            this.set1 = Set1;
            this.set2 = Set2;
        }

        /// <summary>
        /// Checks if the set contains an element.
        /// </summary>
        /// <param name="Element">Element.</param>
        /// <returns>If the element is contained in the set.</returns>
        public override bool Contains(IElement Element)
        {
            return this.set1.Contains(Element) && !this.set2.Contains(Element);
        }

        /// <summary>
        /// Compares the element to another.
        /// </summary>
        /// <param name="obj">Other element to compare against.</param>
        /// <returns>If elements are equal.</returns>
        public override bool Equals(object obj)
        {
            SetDifference S = obj as SetDifference;
            if (S is null)
                return false;

            return (this.set1.Equals(S.set1) && this.set2.Equals(S.set2));

            // TODO: This is not mathematically correct. For the time being, it serves the purpose of allowing the object to be included in dictionaries, etc.
        }

        /// <summary>
        /// Calculates a hash code of the element.
        /// </summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode()
        {
            return this.set1.GetHashCode() ^ this.set2.GetHashCode();
        }

        /// <summary>
        /// An enumeration of child elements. If the element is a scalar, this property will return null.
        /// </summary>
        public override ICollection<IElement> ChildElements
        {
            get
            {
                if (!(this.elements is null))
                    return this.elements.Keys;

                ICollection<IElement> E1 = this.set1.ChildElements;
                if (E1 is null)
                    return null;

                Dictionary<IElement, bool> Elements = new Dictionary<IElement, bool>();

                foreach (IElement E in E1)
                {
                    if (!this.set2.Contains(E))
                        Elements[E] = true;
                }

                this.elements = Elements;
                return Elements.Keys;
            }
        }

        private Dictionary<IElement, bool> elements = null;

        /// <summary>
        /// <see cref="Object.ToString()"/>
        /// </summary>
        public override string ToString()
        {
            return this.set1.ToString() + "\\" + this.set2.ToString();
        }

        /// <summary>
        /// Associated object value.
        /// </summary>
        public override object AssociatedObjectValue
        {
            get
            {
                ICollection<IElement> Elements = this.ChildElements;
                if (Elements is null)
                    return this;

                object[] Elements2 = new object[Elements.Count];
                int i = 0;

                foreach (IElement E in Elements)
                    Elements2[i++] = E.AssociatedObjectValue;

                return Elements2;
            }
        }

        /// <summary>
        /// Size of set, if finite and known, otherwise null is returned.
        /// </summary>
        public override int? Size
        {
            get
            {
                ICollection<IElement> ChildElements = this.ChildElements;
                if (ChildElements is null)
                    return null;
                else
                    return ChildElements.Count;
            }
        }
    }
}
