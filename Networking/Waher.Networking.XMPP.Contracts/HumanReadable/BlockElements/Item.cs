using System.Text;
using System.Threading.Tasks;
using Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.BlockElements
{
	/// <summary>
	/// An item in an item list.
	/// </summary>
	public class Item : BlockElement
	{
		private InlineElement[] inlineElements;
		private BlockElement[] blockElements;

		/// <summary>
		/// Inline elements
		/// </summary>
		public InlineElement[] InlineElements
		{
			get => this.inlineElements;
			set => this.inlineElements = value;
		}

		/// <summary>
		/// Block elements
		/// </summary>
		public BlockElement[] BlockElements
		{
			get => this.blockElements;
			set => this.blockElements = value;
		}

		/// <summary>
		/// Checks if the element is well-defined.
		/// </summary>
		public override async Task<bool> IsWellDefined()
		{
			if (!(this.inlineElements is null))
			{
				if (!(this.blockElements is null))
					return false;

				foreach (InlineElement E in this.inlineElements)
				{
					if (E is null || !await E.IsWellDefined())
						return false;
				}
			}
			else if (!(this.blockElements is null))
			{
				foreach (BlockElement E in this.blockElements)
				{
					if (E is null || !await E.IsWellDefined())
						return false;
				}
			}
			else
				return false;

			return true;
		}

		/// <summary>
		/// Serializes the element in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public override void Serialize(StringBuilder Xml)
		{
			if (!(this.inlineElements is null))
				Serialize(Xml, this.inlineElements, "item");
			else if (!(this.blockElements is null))
				Serialize(Xml, this.blockElements, "item");
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
			if (!(this.inlineElements is null))
			{
				foreach (InlineElement E in this.inlineElements)
					E.GenerateMarkdown(Markdown, SectionLevel, Indentation, Settings);
			}
			else if (!(this.blockElements is null))
			{
				foreach (BlockElement E in this.blockElements)
					E.GenerateMarkdown(Markdown, SectionLevel, Indentation, Settings);
			}
		}
	}
}
