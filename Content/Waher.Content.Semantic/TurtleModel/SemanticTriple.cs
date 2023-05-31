using System.Text;

namespace Waher.Content.Semantic.TurtleModel
{
	/// <summary>
	/// Implements a semantic triple.
	/// </summary>
	public class SemanticTriple : ISemanticTriple
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
	}
}
