using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Model;

namespace Waher.Script.Abstraction.Elements
{
    /// <summary>
    /// Delegate for encapsulation methods.
    /// </summary>
    /// <param name="Elements">New set of child elements, not necessarily of the same type as the child elements of the current object.</param>
    /// <param name="Node">Script node from where the encapsulation is done.</param>
    /// <returns>Encapsulated object of similar type as the current object.</returns>
    public delegate IElement Encapsulation(ICollection<IElement> Elements, ScriptNode Node);

    /// <summary>
    /// Basic interface for all types of elements.
    /// </summary>
    public interface IElement
	{
		/// <summary>
		/// Associated Set.
		/// </summary>
		ISet AssociatedSet
		{
			get;
		}

		/// <summary>
		/// Associated object value.
		/// </summary>
		object AssociatedObjectValue
		{
			get;
		}

		/// <summary>
		/// If the element represents a scalar value.
		/// </summary>
		bool IsScalar
		{
			get;
		}

		/// <summary>
		/// An enumeration of child elements. If the element is a scalar, this property will return null.
		/// </summary>
		ICollection<IElement> ChildElements
		{
			get;
		}

		/// <summary>
		/// Encapsulates a set of elements into a similar structure as that provided by the current element.
		/// </summary>
		/// <param name="Elements">New set of child elements, not necessarily of the same type as the child elements of the current object.</param>
		/// <param name="Node">Script node from where the encapsulation is done.</param>
		/// <returns>Encapsulated object of similar type as the current object.</returns>
		IElement Encapsulate(ICollection<IElement> Elements, ScriptNode Node);

        /// <summary>
        /// Converts the value to a .NET type.
        /// </summary>
        /// <param name="DesiredType">Desired .NET type.</param>
        /// <param name="Value">Converted value.</param>
        /// <returns>If conversion was possible.</returns>
        bool TryConvertTo(Type DesiredType, out object Value);
	}
}
