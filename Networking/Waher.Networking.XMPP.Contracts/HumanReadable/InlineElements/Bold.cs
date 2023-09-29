using System.Text;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements
{
	/// <summary>
	/// Bold text
	/// </summary>
	public class Bold : Formatting
	{
		/// <summary>
		/// Serializes the element in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public override void Serialize(StringBuilder Xml)
		{
			Serialize(Xml, this.Elements, "bold");
		}

		/// <summary>
		/// Generates markdown for the human-readable text.
		/// </summary>
		/// <param name="Markdown">Markdown output.</param>
		/// <param name="SectionLevel">Current section level.</param>
		/// <param name="Indentation">Current indentation.</param>
		/// <param name="Settings">Settings used for Markdown generation of human-readable text.</param>
		public override void GenerateMarkdown(StringBuilder Markdown, int SectionLevel, int Indentation, MarkdownSettings Settings)
		{
			Markdown.Append("**");

			if (!(this.Elements is null))
			{
				foreach (InlineElement E in this.Elements)
					E.GenerateMarkdown(Markdown, SectionLevel, Indentation, Settings);
			}

			Markdown.Append("**");
		}
	}
}
