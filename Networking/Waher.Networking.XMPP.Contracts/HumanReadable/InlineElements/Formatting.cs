namespace Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements
{
	/// <summary>
	/// Abstract base class for inline formatting elements.
	/// </summary>
	public abstract class Formatting : InlineElement
	{
		private InlineElement[] elements;

		/// <summary>
		/// Embedded elements
		/// </summary>
		public InlineElement[] Elements
		{
			get => this.elements;
			set => this.elements = value;
		}

		/// <summary>
		/// Checks if the element is well-defined.
		/// </summary>
		public override bool IsWellDefined
		{
			get
			{
				if (this.elements is null)
					return false;

				foreach (InlineElement E in this.elements)
				{
					if (E is null || !E.IsWellDefined)
						return false;
				}

				return true;
			}
		}

		/// <summary>
		/// Generates markdown for the human-readable text.
		/// </summary>
		/// <param name="Markdown">Markdown output.</param>
		/// <param name="SectionLevel">Current section level.</param>
		/// <param name="Indentation">Current indentation.</param>
		/// <param name="Settings">Settings used for Markdown generation of human-readable text.</param>
		public override void GenerateMarkdown(MarkdownOutput Markdown, int SectionLevel, int Indentation, MarkdownSettings Settings)
		{
			if (!(this.elements is null))
			{
				foreach (InlineElement E in this.elements)
					E.GenerateMarkdown(Markdown, SectionLevel, Indentation, Settings);
			}
		}
	}
}
