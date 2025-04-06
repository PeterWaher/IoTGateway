using System.Threading.Tasks;
using Waher.Content.Markdown.Rendering;
using Waher.Runtime.Collections;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Automatic Link (e-Mail)
	/// </summary>
	public class AutomaticLinkMail : MarkdownElement
	{
		private readonly string eMail;

		/// <summary>
		/// Inline HTML.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="EMail">Automatic e-Mail link.</param>
		public AutomaticLinkMail(MarkdownDocument Document, string EMail)
			: base(Document)
		{
			this.eMail = EMail;
		}

		/// <summary>
		/// e-Mail
		/// </summary>
		public string EMail => this.eMail;

		/// <summary>
		/// Renders the element.
		/// </summary>
		/// <param name="Output">Renderer</param>
		public override Task Render(IRenderer Output) => Output.Render(this);

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.eMail;
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
			return obj is AutomaticLinkMail x &&
				this.eMail == x.eMail &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.eMail?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			if (Statistics.IntMailHyperlinks is null)
				Statistics.IntMailHyperlinks = new ChunkedList<string>();

			if (!Statistics.IntMailHyperlinks.Contains(this.eMail))
				Statistics.IntMailHyperlinks.Add(this.eMail);

			Statistics.NrMailHyperLinks++;
			Statistics.NrHyperLinks++;
		}

	}
}
