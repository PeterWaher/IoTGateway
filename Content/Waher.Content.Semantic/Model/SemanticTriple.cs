using System.Text;

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
	}
}
