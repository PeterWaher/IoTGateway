using System;
using System.Text;
using Waher.Persistence.Attributes;

namespace Waher.Content.Semantic.Model
{
	/// <summary>
	/// Represents a URI
	/// </summary>
	public class UriNode : SemanticElement
	{
		private Uri parsed;
		private string uri;
		private string shortName;

		/// <summary>
		/// Represents a URI
		/// </summary>
		public UriNode()
		{
		}

		/// <summary>
		/// Represents a URI
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Short">Short name</param>
		public UriNode(Uri Uri, string Short)
		{
			this.parsed = Uri;
			this.uri = Uri.ToString();
			this.shortName = Short;
		}

		/// <summary>
		/// URI
		/// </summary>
		[IgnoreMember]
		public Uri Uri => this.parsed;

		/// <summary>
		/// Short name, if available.
		/// </summary>
		public string ShortName
		{
			get => this.shortName;
			set => this.shortName = value;
		}

		/// <summary>
		/// If element is a literal.
		/// </summary>
		public override bool IsLiteral => false;

		/// <summary>
		/// If URI has a short name.
		/// </summary>
		public bool HasShortName => !string.IsNullOrEmpty(this.shortName);

		/// <summary>
		/// Associated object value.
		/// </summary>
		public override object AssociatedObjectValue => this.uri;

		/// <summary>
		/// URI string
		/// </summary>
		public string UriString 
		{
			get => this.uri;
			set
			{
				this.parsed = new Uri(value);
				this.uri = value;
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append('<');
			sb.Append(this.uri);
			sb.Append('>');

			return sb.ToString();
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is UriNode Typed &&
				Typed.parsed.ToString() == this.parsed.ToString();
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.parsed.ToString().GetHashCode();
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
		public override int CompareTo(object obj)
		{
			if (obj is UriNode Typed)
				return this.parsed.ToString().CompareTo(Typed.parsed.ToString());
			else
				return base.CompareTo(obj);
		}
	}
}
