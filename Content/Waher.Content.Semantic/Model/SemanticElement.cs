using System;
using Waher.Persistence.Attributes;

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
		/// Underlying element value represented by the semantic element.
		/// </summary>
		public abstract object ElementValue { get; }

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
