using System.Text;

namespace Waher.Networking.XMPP.Contracts.HumanReadable
{
	/// <summary>
	/// Builds Markdown
	/// </summary>
	public class MarkdownOutput
	{
		private readonly StringBuilder output = new StringBuilder();
		private int indentation = 0;
		private bool emptyRow = true;

		/// <summary>
		/// Current indentation.
		/// </summary>
		public int Indentation => this.indentation;

		/// <summary>
		/// If current row is empty.
		/// </summary>
		public bool EmptyRow => this.emptyRow;

		/// <summary>
		/// Appends text to the Markdown output.
		/// </summary>
		/// <param name="s">Text to append.</param>
		public void Append(string s)
		{
			this.output.Append(s);
			this.emptyRow = false;
		}

		/// <summary>
		/// Appends a character to the Markdown output.
		/// </summary>
		/// <param name="ch">Character to append.</param>
		public void Append(char ch)
		{
			this.output.Append(ch);
			this.emptyRow = false;
		}

		/// <summary>
		/// Appends a new line.
		/// </summary>
		public void AppendLine()
		{
			this.output.AppendLine();
			this.indentation = 0;
			this.emptyRow = true;
		}

		/// <summary>
		/// Indents the coming row.
		/// </summary>
		/// <param name="Indentation">Indentation</param>
		public void Indent(int Indentation)
		{
			if (this.emptyRow && Indentation > this.indentation)
			{
				int i = Indentation - this.indentation;
				this.indentation = Indentation;

				while (i-- > 0)
					this.output.Append('\t');
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.output.ToString();
		}
	}
}
