using System.Threading.Tasks;
using Waher.Content.Markdown.Rendering;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a task item in a task list.
	/// </summary>
	public class TaskItem : BlockElementSingleChild
	{
		private readonly int checkPosition;
		private readonly bool isChecked;

		/// <summary>
		/// Represents a task item in a task list.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="IsChecked">If the item is checked or not.</param>
		/// <param name="CheckPosition">Position of the checkmark in the original markdown text document.</param>
		/// <param name="Child">Child element.</param>
		public TaskItem(MarkdownDocument Document, bool IsChecked, int CheckPosition, MarkdownElement Child)
			: base(Document, Child)
		{
			this.isChecked = IsChecked;
			this.checkPosition = CheckPosition;
		}

		/// <summary>
		/// If the item is checked or not.
		/// </summary>
		public bool IsChecked => this.isChecked;

		/// <summary>
		/// Position of the checkmark in the original markdown text document.
		/// </summary>
		public int CheckPosition => this.checkPosition;

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
			return new TaskItem(Document, this.isChecked, this.checkPosition, Child);
		}

		/// <summary>
		/// If the current object has same meta-data as <paramref name="E"/>
		/// (but not necessarily same content).
		/// </summary>
		/// <param name="E">Element to compare to.</param>
		/// <returns>If same meta-data as <paramref name="E"/>.</returns>
		public override bool SameMetaData(MarkdownElement E)
		{
			return E is TaskItem x &&
				x.isChecked == this.isChecked &&
				x.checkPosition == this.checkPosition &&
				base.SameMetaData(E);
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is TaskItem x &&
				this.isChecked == x.isChecked &&
				this.checkPosition == x.checkPosition &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.isChecked.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;
			h2 = this.checkPosition.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrTaskItems++;
			Statistics.NrListItems++;
		}

	}
}
