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

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is UriNode Typed &&
				Typed.Uri == this.Uri;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.Uri.GetHashCode();
		}
	}
}
