using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Model;

namespace Waher.Content.Semantic.Model
{
	/// <summary>
	/// Implements a semantic triple.
	/// </summary>
	public class SemanticTriple : ISemanticTriple, ISemanticElement
	{
		/// <summary>
		/// Implements a semantic triple.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		public SemanticTriple(ISemanticElement Subject, ISemanticElement Predicate, ISemanticElement Object)
		{
			this.Subject = Subject;
			this.Predicate = Predicate;
			this.Object = Object;
		}

		/// <summary>
		/// Subject element
		/// </summary>
		public ISemanticElement Subject { get; }

		/// <summary>
		/// Predicate element
		/// </summary>
		public ISemanticElement Predicate { get; }

		/// <summary>
		/// Object element
		/// </summary>
		public ISemanticElement Object { get; }

		/// <summary>
		/// Property used by processor, to tag information to an element.
		/// </summary>
		public object Tag { get; set; }

		/// <summary>
		/// If element is a literal.
		/// </summary>
		public bool IsLiteral => this.Subject.IsLiteral && this.Predicate.IsLiteral && this.Object.IsLiteral;

		/// <summary>
		/// Associated Set.
		/// </summary>
		public ISet AssociatedSet => SemanticElements.Instance;

		/// <summary>
		/// Associated object value.
		/// </summary>
		public object AssociatedObjectValue => this;

		/// <summary>
		/// Underlying element value represented by the semantic element.
		/// </summary>
		public object ElementValue => this;

		/// <summary>
		/// If the element represents a scalar value.
		/// </summary>
		public bool IsScalar => false;

		/// <summary>
		/// Encapsulates a set of elements into a similar structure as that provided by the current element.
		/// </summary>
		/// <param name="Elements">New set of child elements, not necessarily of the same type as the child elements of the current object.</param>
		/// <param name="Node">Script node from where the encapsulation is done.</param>
		/// <returns>Encapsulated object of similar type as the current object.</returns>
		public IElement Encapsulate(ICollection<IElement> Elements, ScriptNode Node)
		{
			if (Elements.Count != 3)
				return null;

			ISemanticElement Subject = null;
			ISemanticElement Predicate = null;
			ISemanticElement Object = null;

			foreach (IElement E in Elements)
			{
				if (!(E is ISemanticElement SemanticElement))
					SemanticElement = SemanticElements.Encapsulate(E.AssociatedObjectValue);

				if (Subject is null)
					Subject = SemanticElement;
				else if (Predicate is null)
					Predicate = SemanticElement;
				else 
					Object = SemanticElement;
			}

			return new SemanticTriple(Subject, Predicate, Object);
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

		/// <summary>
		/// An enumeration of child elements. If the element is a scalar, this property will return null.
		/// </summary>
		public ICollection<IElement> ChildElements
		{
			get
			{
				return new IElement[]
				{
					this.Subject,
					this.Predicate,
					this.Object
				};
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(this.Subject);
			sb.Append('\t');
			sb.Append(this.Predicate);
			sb.Append('\t');
			sb.Append(this.Object);

			return sb.ToString();
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is SemanticTriple Typed &&
				Typed.Subject.Equals(this.Subject) &&
				Typed.Predicate.Equals(this.Predicate) &&
				Typed.Object.Equals(this.Object);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = this.Subject.GetHashCode();
			Result ^= Result << 5 ^ this.Predicate.GetHashCode();
			Result ^= Result << 5 ^ this.Object.GetHashCode();

			return Result;
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
		public int CompareTo(object obj)
		{
			if (!(obj is SemanticTriple T))
				return this.ToString().CompareTo(obj?.ToString() ?? string.Empty);

			int i = this.Subject.CompareTo(T.Subject);
			if (i != 0)
				return i;

			i = this.Predicate.CompareTo(T.Predicate);
			if (i != 0)
				return i;

			return this.Object.CompareTo(T.Object);
		}

		/// <summary>
		/// Access to elements: 0=Subject, 1=Predicate, 2=Object.
		/// </summary>
		/// <param name="Index"></param>
		/// <returns>Semantic element.</returns>
		public ISemanticElement this[int Index]
		{
			get
			{
				switch (Index)
				{
					case 0: return this.Subject;
					case 1: return this.Predicate;
					case 2: return this.Object;
					default: return null;
				}
			}
		}
	}
}
