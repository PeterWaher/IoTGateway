using System.Threading.Tasks;

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
		/// <returns>Returns first failing element, if found.</returns>
		public override async Task<HumanReadableElement> IsWellDefined()
		{
			if (this.elements is null)
				return this;

			foreach (InlineElement E in this.elements)
			{
				if (E is null)
					return this;

				HumanReadableElement E2 = await E.IsWellDefined();
				if (!(E2 is null))
					return E2;
			}

			return null;
		}

		/// <summary>
		/// Generates markdown for the human-readable text.
		/// </summary>
		/// <param name="Markdown">Markdown output.</param>
		/// <param name="SectionLevel">Current section level.</param>
		/// <param name="Indentation">Current indentation.</param>
		/// <param name="Settings">Settings used for Markdown generation of human-readable text.</param>
		public override Task GenerateMarkdown(MarkdownOutput Markdown, int SectionLevel, int Indentation, MarkdownSettings Settings)
		{
			return this.GenerateMarkdown(Markdown, SectionLevel, Indentation, Settings, false);
		}

		/// <summary>
		/// Generates markdown for the human-readable text.
		/// </summary>
		/// <param name="Markdown">Markdown output.</param>
		/// <param name="SectionLevel">Current section level.</param>
		/// <param name="Indentation">Current indentation.</param>
		/// <param name="Settings">Settings used for Markdown generation of human-readable text.</param>
		/// <param name="EspaceFirstSpace">If escaping of the first character should be done, if it is a space character.</param>
		public async Task GenerateMarkdown(MarkdownOutput Markdown, int SectionLevel, int Indentation, MarkdownSettings Settings,
			bool EspaceFirstSpace)
		{
			if (!(this.elements is null))
			{
				if (EspaceFirstSpace)
				{
					MarkdownOutput Markdown2 = new MarkdownOutput();

					foreach (InlineElement E in this.elements)
						await E.GenerateMarkdown(Markdown2, SectionLevel, Indentation, Settings);

					string Segment = Markdown2.ToString();

					if (!string.IsNullOrEmpty(Segment))
					{
						if (Segment[0] == ' ')
						{
							Markdown.Append("&nbsp;");
							Segment = Segment.Substring(1);
						}

						Markdown.Append(Segment);
					}
				}
				else
				{
					foreach (InlineElement E in this.elements)
						await E.GenerateMarkdown(Markdown, SectionLevel, Indentation, Settings);
				}
			}
		}
	}
}
