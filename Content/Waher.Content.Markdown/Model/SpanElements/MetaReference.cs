using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Markdown.Rendering;
using Waher.Script;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Meta-data reference
	/// </summary>
	public class MetaReference : MarkdownElement
	{
		private readonly string key;

		/// <summary>
		/// Meta-data reference
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Key">Meta-data key.</param>
		public MetaReference(MarkdownDocument Document, string Key)
			: base(Document)
		{
			this.key = Key;
		}

		/// <summary>
		/// Meta-data key
		/// </summary>
		public string Key => this.key;

		/// <summary>
		/// Renders the element.
		/// </summary>
		/// <param name="Output">Renderer</param>
		public override Task Render(IRenderer Output) => Output.Render(this);

		/// <summary>
		/// Tries to get meta-data from the document.
		/// </summary>
		/// <param name="Values">Values, if found.</param>
		/// <returns>If meta-data was found.</returns>
		public bool TryGetMetaData(out KeyValuePair<string, bool>[] Values)
		{
			if (this.Document?.TryGetMetaData(this.key, out Values) ?? false)
				return true;

			Variables Variables = this.Document.Settings.Variables;
			if (!(Variables is null) && Variables.TryGetVariable(this.key, out Variable Variable))
			{
				Values = new KeyValuePair<string, bool>[] { new KeyValuePair<string, bool>(Variable.ValueObject?.ToString() ?? string.Empty, false) };
				return true;
			}

			Values = null;
			return false;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.key;
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		public override bool InlineSpanElement => true;

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is MetaReference x &&
				this.key == x.key &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.key?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrMetaReference++;
		}

	}
}
