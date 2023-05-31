using System;
using System.Text;

namespace Waher.Content.Semantic.Model
{
	/// <summary>
	/// Represents a URI
	/// </summary>
	public class UriNode : ISemanticElement
	{
		/// <summary>
		/// Predefined predicate "a".
		/// </summary>
		public readonly static UriNode RdfA = new UriNode(new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#type"));

		/// <summary>
		/// Predefined reference to first element in a collection.
		/// </summary>
		public readonly static UriNode RdfFirst = new UriNode(new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#first"));

		/// <summary>
		/// Predefined reference to next element in a collection.
		/// </summary>
		public readonly static UriNode RdfNext = new UriNode(new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#rest"));

		/// <summary>
		/// Predefined reference to end of collection.
		/// </summary>
		public readonly static UriNode RdfNil = new UriNode(new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#nil"));

		/// <summary>
		/// Represents a URI
		/// </summary>
		/// <param name="Uri">URI</param>
		public UriNode(Uri Uri)
		{
			this.Uri = Uri;
		}

		/// <summary>
		/// URI
		/// </summary>
		public Uri Uri { get; }

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append('<');
			sb.Append(this.Uri.AbsoluteUri);
			sb.Append('>');

			return sb.ToString();
		}
	}
}
