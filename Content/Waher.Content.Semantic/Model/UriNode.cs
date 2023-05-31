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
	}
}
