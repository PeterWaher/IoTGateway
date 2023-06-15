using System;
using System.Collections.Generic;
using Waher.Persistence.Attributes;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Model;

namespace Waher.Content.Semantic.Model
{
	/// <summary>
	/// Abstract base class for semantic elements.
	/// </summary>
	[TypeName(TypeNameSerialization.FullName)]
	public abstract class SemanticElement : ISemanticElement
	{
		/// <summary>
		/// Abstract base class for semantic elements.
		/// </summary>
		public SemanticElement()
		{
		}

		/// <summary>
		/// Property used by processor, to tag information to an element.
		/// </summary>
		[IgnoreMember]
		public object Tag { get; set; }

		/// <summary>
		/// If element is a literal.
		/// </summary>
		public abstract bool IsLiteral { get; }

		/// <summary>
		/// Associated Set.
		/// </summary>
		public ISet AssociatedSet => SemanticElements.Instance;

		/// <summary>
		/// Associated object value.
		/// </summary>
		public abstract object AssociatedObjectValue { get; }

		/// <summary>
		/// If the element represents a scalar value.
		/// </summary>
		public bool IsScalar => true;

		/// <summary>
		/// An enumeration of child elements. If the element is a scalar, this property will return null.
		/// </summary>
		public ICollection<IElement> ChildElements => null;

		/// <summary>
		/// Encapsulates a set of elements into a similar structure as that provided by the current element.
		/// </summary>
		/// <param name="Elements">New set of child elements, not necessarily of the same type as the child elements of the current object.</param>
		/// <param name="Node">Script node from where the encapsulation is done.</param>
		/// <returns>Encapsulated object of similar type as the current object.</returns>
		public IElement Encapsulate(ICollection<IElement> Elements, ScriptNode Node)
		{
			return null;
		}

		/// <summary>
		/// Converts the value to a .NET type.
		/// </summary>
		/// <param name="DesiredType">Desired .NET type.</param>
		/// <param name="Value">Converted value.</param>
		/// <returns>If conversion was possible.</returns>
		public virtual bool TryConvertTo(Type DesiredType, out object Value)
		{
			Value = null;
			return false;
		}

		/// <inheritdoc/>
		public override abstract bool Equals(object obj);

		/// <inheritdoc/>
		public override abstract int GetHashCode();

		/// <summary>
		/// Compares the current instance with another object of the same type and returns
		/// an integer that indicates whether the current instance precedes, follows, or
		/// occurs in the same position in the sort order as the other object.
		/// </summary>
		/// <param name="obj">An object to compare with this instance.</param>
		/// <returns>A value that indicates the relative order of the objects being compared. The
		/// return value has these meanings: Value Meaning Less than zero This instance precedes
		/// obj in the sort order. Zero This instance occurs in the same position in the
		/// sort order as obj. Greater than zero This instance follows obj in the sort order.</returns>
		/// <exception cref="ArgumentException">obj is not the same type as this instance.</exception>
		public virtual int CompareTo(object obj)
		{
			return this.ToString().CompareTo(obj?.ToString() ?? string.Empty);
		}
	}
}
