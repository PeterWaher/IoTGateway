using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.BlockElements
{
	/// <summary>
	/// Abstract base class for sets of blocks.
	/// </summary>
	public abstract class Blocks : BlockElement
	{
		private BlockElement[] body;

		/// <summary>
		/// Body elements
		/// </summary>
		public BlockElement[] Body
		{
			get => this.body;
			set => this.body = value;
		}

		/// <summary>
		/// Checks if the element is well-defined.
		/// </summary>
		public override bool IsWellDefined
		{
			get
			{
				foreach (BlockElement E in this.body)
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
		/// <param name="Contract">Contract, of which the human-readable text is part.</param>
		public override void GenerateMarkdown(StringBuilder Markdown, int SectionLevel, Contract Contract)
		{
			foreach (HumanReadableElement E in this.Body)
				E.GenerateMarkdown(Markdown, SectionLevel, Contract);
		}
	}
}
