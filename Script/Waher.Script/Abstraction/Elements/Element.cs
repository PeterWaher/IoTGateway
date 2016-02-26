using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Model;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Base class for all types of elements.
	/// </summary>
	public abstract class Element : IElement
	{
		/// <summary>
		/// Base class for all types of elements.
		/// </summary>
		public Element()
		{
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
		/// Associated Set.
		/// </summary>
		public abstract ISet AssociatedSet
		{
			get;
		}

		/// <summary>
		/// Associated object value.
		/// </summary>
		public abstract object AssociatedObjectValue
		{
			get;
		}

		/// <summary>
		/// If the element represents a scalar value.
		/// </summary>
		public virtual bool IsScalar
		{
			get { return true; }
		}

		/// <summary>
		/// An enumeration of child elements. If the element is a scalar, this property will return null.
		/// </summary>
		public virtual ICollection<IElement> ChildElements
		{
			get { return null; }
		}

		/// <summary>
		/// Encapsulates a set of elements into a similar structure as that provided by the current element.
		/// </summary>
		/// <param name="Elements">New set of child elements, not necessarily of the same type as the child elements of the current object.</param>
		/// <param name="Node">Script node from where the encapsulation is done.</param>
		/// <returns>Encapsulated object of similar type as the current object.</returns>
		public virtual IElement Encapsulate(ICollection<IElement> Elements, ScriptNode Node)
		{
			throw new Exceptions.ScriptRuntimeException("Object is a scalar and cannot encapsulate child elements.", Node);
		}

        /// <summary>
        /// Converts the value to a .NET type.
        /// </summary>
        /// <param name="DesiredType">Desired .NET type.</param>
        /// <param name="Value">Converted value.</param>
        /// <returns>If conversion was possible.</returns>
        public virtual bool TryConvertTo(Type DesiredType, out object Value)
        {
            if (DesiredType.IsAssignableFrom(this.GetType()))
            {
                Value = this;
                return true;
            }

            object Obj = this.AssociatedObjectValue;
            if (DesiredType.IsAssignableFrom(Obj.GetType()))
            {
                Value = Obj;
                return true;
            }

            Value = null;
            return false;
        }

    }
}
