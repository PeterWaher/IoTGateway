using System;
using System.Collections.Generic;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Model;

namespace Waher.Content.Semantic.Model
{
	/// <summary>
	/// Contains a vector of semantic elements.
	/// </summary>
	public class SemanticElementVector : ISemanticElement, IVector
	{
		private readonly List<IElement> elements = new List<IElement>();

		/// <summary>
		/// Contains a vector of semantic elements.
		/// </summary>
		public SemanticElementVector()
		{
		}

		/// <summary>
		/// Adds a semantic element to the vector.
		/// </summary>
		/// <param name="Element">Element</param>
		public void Add(ISemanticElement Element)
		{
			this.elements.Add(Element);
		}

		/// <summary>
		/// Property used by processor, to tag information to an element.
		/// </summary>
		public object Tag { get; set; }

		/// <summary>
		/// If element is a literal.
		/// </summary>
		public bool IsLiteral => false;

		/// <summary>
		/// Associated Set.
		/// </summary>
		public ISet AssociatedSet => SemanticElements.Instance;

		/// <summary>
		/// Associated object value.
		/// </summary>
		public object AssociatedObjectValue => this.elements;

		/// <summary>
		/// If the element represents a scalar value.
		/// </summary>
		public bool IsScalar => false;

		/// <summary>
		/// An enumeration of child elements. If the element is a scalar, this property will return null.
		/// </summary>
		public ICollection<IElement> ChildElements => this.elements;

		/// <summary>
		/// Encapsulates a set of elements into a similar structure as that provided by the current element.
		/// </summary>
		/// <param name="Elements">New set of child elements, not necessarily of the same type as the child elements of the current object.</param>
		/// <param name="Node">Script node from where the encapsulation is done.</param>
		/// <returns>Encapsulated object of similar type as the current object.</returns>
		public IElement Encapsulate(ICollection<IElement> Elements, ScriptNode Node)
		{
			SemanticElementVector Result = new SemanticElementVector();
			Result.elements.AddRange(Elements);
			return Result;
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
		public override bool Equals(object obj)
		{
			if (!(obj is SemanticElementVector Typed) ||
				this.elements.Count != Typed.elements.Count)
			{
				return false;
			}

			IEnumerator<IElement> e1 = this.elements.GetEnumerator();
			IEnumerator<IElement> e2 = Typed.elements.GetEnumerator();

			while (e1.MoveNext() && e2.MoveNext())
			{
				if (!e1.Current.Equals(e2.Current))
					return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = this.elements.Count.GetHashCode();

			foreach (IElement Element in this.elements)
				Result ^= Result << 5 ^ Element.GetHashCode();

			return Result;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return Expression.ToString(this);
		}

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

		/// <summary>
		/// Dimension of vector.
		/// </summary>
		public int Dimension => this.elements.Count;

		/// <summary>
		/// An enumeration of vector elements.
		/// </summary>
		public ICollection<IElement> VectorElements => this.elements;

		/// <summary>
		/// Gets an element of the vector.
		/// </summary>
		/// <param name="Index">Zero-based index into the vector.</param>
		/// <returns>Vector element.</returns>
		public IElement GetElement(int Index) => this.elements[Index];

		/// <summary>
		/// Sets an element in the vector.
		/// </summary>
		/// <param name="Index">Index.</param>
		/// <param name="Value">Element to set.</param>
		public void SetElement(int Index, IElement Value)
		{
			this.elements[Index] = Value;
		}
	}
}
