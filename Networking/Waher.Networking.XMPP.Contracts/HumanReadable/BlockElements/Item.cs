using System.Collections.Generic;
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
		/// Inline elements
		/// </summary>
		public HumanReadableElement[] Elements
		{
			set
			{
				List<InlineElement> InlineElements = null;
				List<BlockElement> BlockElements = null;

				foreach (HumanReadableElement E in value)
				{
					if (E is InlineElement InlineElement)
					{
						if (InlineElements is null)
							InlineElements = new List<InlineElement>();

						InlineElements.Add(InlineElement);
					}
					else if (E is BlockElement BlockElement)
					{
						if (BlockElements is null)
							BlockElements = new List<BlockElement>();

						if (!(InlineElements is null))
						{
							BlockElements.Add(new Paragraph()
							{
								Elements = InlineElements.ToArray()
							});

							InlineElements = null;
						}

						BlockElements.Add(BlockElement);
					}
				}

				if (!(BlockElements is null))
				{
					if (!(InlineElements is null))
					{
						BlockElements.Add(new Paragraph()
						{
							Elements = InlineElements.ToArray()
						});
					}

					this.BlockElements = BlockElements.ToArray();
					this.InlineElements = null;
				}
				else if (!(InlineElements is null))
				{
					this.InlineElements = InlineElements.ToArray();
					this.BlockElements = null;
				}
				else
				{
					this.InlineElements = null;
					this.BlockElements = null;
				}
			}
		}

		/// <summary>
		/// Checks if the element is well-defined.
		/// </summary>
		/// <returns>Returns first failing element, if found.</returns>
		public override async Task<HumanReadableElement> IsWellDefined()
		{
			if (!(this.inlineElements is null))
			{
				if (!(this.blockElements is null))
					return this;

				foreach (InlineElement E in this.inlineElements)
				{
					if (E is null)
						return this;

					HumanReadableElement E2 = await E.IsWellDefined();
					if (!(E2 is null))
						return E2;
				}
			}
			else if (!(this.blockElements is null))
			{
				foreach (BlockElement E in this.blockElements)
				{
					if (E is null)
						return this;

					HumanReadableElement E2 = await E.IsWellDefined();
					if (!(E2 is null))
						return E2;
				}
			}
			else if (!this.CanBeEmpty)
				return this;

			return null;
		}

		/// <summary>
		/// If item can be empty.
		/// </summary>
		public virtual bool CanBeEmpty => false;

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
		public override async Task GenerateMarkdown(MarkdownOutput Markdown, int SectionLevel, int Indentation, MarkdownSettings Settings)
		{
			if (!(this.inlineElements is null))
			{
				foreach (InlineElement E in this.inlineElements)
					await E.GenerateMarkdown(Markdown, SectionLevel, Indentation, Settings);
			}
			else if (!(this.blockElements is null))
			{
				foreach (BlockElement E in this.blockElements)
					await E.GenerateMarkdown(Markdown, SectionLevel, Indentation, Settings);
			}
		}
	}
}
