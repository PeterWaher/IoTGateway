using System.Text;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.BlockElements
{
	/// <summary>
	/// Bullet list
	/// </summary>
	public class BulletList : ItemList 
	{
		/// <summary>
		/// Serializes the element in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public override void Serialize(StringBuilder Xml)
		{
			Serialize(Xml, this.Items, "bulletItems");
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
			if (!(this.Items is null))
			{
				foreach (Item Item in this.Items)
				{
					Markdown.Indent(Indentation);
					Markdown.Append("*\t");
					Item.GenerateMarkdown(Markdown, SectionLevel, Indentation + 1, Settings);

					if (!Markdown.EmptyRow)
						Markdown.AppendLine();
				}

				Markdown.Indent(Indentation);
				Markdown.AppendLine();
			}
		}
	}
}
