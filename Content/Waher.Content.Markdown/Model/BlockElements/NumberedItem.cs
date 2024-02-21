using System.Threading.Tasks;
using Waher.Content.Markdown.Rendering;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a numbered item in an ordered list.
	/// </summary>
	public class NumberedItem : BlockElementSingleChild
	{
		private int number;
		private readonly bool numberExplicit;

		/// <summary>
		/// Represents a numbered item in an ordered list.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Number">Number associated with item.</param>
		/// <param name="NumberExplicit">If number is provided explicitly</param>
		/// <param name="Child">Child element.</param>
		public NumberedItem(MarkdownDocument Document, int Number, bool NumberExplicit, MarkdownElement Child)
			: base(Document, Child)
		{
			this.number = Number;
			this.numberExplicit = NumberExplicit;
		}

		/// <summary>
		/// Number associated with item.
		/// </summary>
		public int Number
		{
			get => this.number;
			internal set => this.number = value;
		}

		/// <summary>
		/// If number is explicitly provided (true) or inferred (false).
		/// </summary>
		public bool NumberExplicit => this.numberExplicit;

		/// <summary>
		/// Renders the element.
		/// </summary>
		/// <param name="Output">Renderer</param>
		public override Task Render(IRenderer Output) => Output.Render(this);

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		public override bool InlineSpanElement => this.Child.InlineSpanElement;

		/// <summary>
		/// Creates an object of the same type, and meta-data, as the current object,
		/// but with content defined by <paramref name="Child"/>.
		/// </summary>
		/// <param name="Child">New content.</param>
		/// <param name="Document">Document that will contain the element.</param>
		/// <returns>Object of same type and meta-data, but with new content.</returns>
		public override MarkdownElementSingleChild Create(MarkdownElement Child, MarkdownDocument Document)
		{
			return new NumberedItem(Document, this.number, this.numberExplicit, Child);
		}

		/// <summary>
		/// If the current object has same meta-data as <paramref name="E"/>
		/// (but not necessarily same content).
		/// </summary>
		/// <param name="E">Element to compare to.</param>
		/// <returns>If same meta-data as <paramref name="E"/>.</returns>
		public override bool SameMetaData(MarkdownElement E)
		{
			return E is NumberedItem x &&
				x.number == this.number &&
				x.numberExplicit == this.numberExplicit &&
				base.SameMetaData(E);
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is NumberedItem x &&
				this.number == x.number &&
				this.numberExplicit == x.numberExplicit &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.number.GetHashCode();
			int h3 = this.numberExplicit.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;
			h1 = ((h1 << 5) + h1) ^ h3;

			return h1;
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrNumberedItems++;
			Statistics.NrListItems++;
		}

	}
}
