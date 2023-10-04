using System.Text;
using System.Threading.Tasks;
using Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.BlockElements
{
	/// <summary>
	/// A section consisting of a header and a body.
	/// </summary>
	public class Section : Blocks
	{
		private InlineElement[] header;

		/// <summary>
		/// Header elements
		/// </summary>
		public InlineElement[] Header
		{
			get => this.header;
			set => this.header = value;
		}

		/// <summary>
		/// Checks if the element is well-defined.
		/// </summary>
		public override async Task<bool> IsWellDefined()
		{
			if (this.header is null)
				return false;

			foreach (InlineElement E in this.header)
			{
				if (E is null || !await E.IsWellDefined())
					return false;
			}

			return await base.IsWellDefined();
		}

		/// <summary>
		/// Serializes the element in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public override void Serialize(StringBuilder Xml)
		{
			Xml.Append("<section>");
			Serialize(Xml, this.Header, "header");
			Serialize(Xml, this.Body, "body");
			Xml.Append("</section>");
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
			Markdown.Indent(Indentation);
			Markdown.Append(new string('#', SectionLevel));
			Markdown.Append(' ');

			if (!(this.header is null))
			{
				foreach (InlineElement E in this.header)
					E.GenerateMarkdown(Markdown, SectionLevel, Indentation, Settings);
			}

			Markdown.AppendLine();

			Markdown.Indent(Indentation);
			Markdown.AppendLine();

			base.GenerateMarkdown(Markdown, SectionLevel + 1, Indentation, Settings);
		}

	}
}
