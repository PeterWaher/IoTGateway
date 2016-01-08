using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model
{
	internal class Block
	{
		private string[] rows;
		private int indent;
		private int start;
		private int end;

		public Block(string[] Rows, int Indent)
			: this(Rows, Indent, 0, Rows.Length - 1)
		{
		}

		public Block(string[] Rows, int Indent, int Start, int End)
		{
			this.rows = Rows;
			this.indent = Indent;
			this.start = Start;
			this.end = End;
		}

		public string[] Rows
		{
			get { return this.rows; }
		}

		public int Indent
		{
			get { return this.indent; }
			set { this.indent = value; }
		}

		public int Start
		{
			get { return this.start; }
		}

		public int End
		{
			get { return this.end; }
		}

		public bool IsPrefixedBy(string Prefix, bool MustHaveWhiteSpaceAfter)
		{
			return MarkdownDocument.IsPrefixedBy(this.rows[this.start], Prefix, MustHaveWhiteSpaceAfter);
		}

		public bool IsPrefixedByNumber(out int Numeral)
		{
			return MarkdownDocument.IsPrefixedByNumber(this.rows[this.start], out Numeral);
		}

		public List<Block> RemovePrefix(string Prefix, int NrCharacters)
		{
			List<Block> Result = new List<Block>();
			List<string> Rows = new List<string>();
			string s;
			int Indent = 0;
			int i, j, k;
			int d = Prefix.Length;
			bool FirstRow = true;

			for (i = this.start; i <= this.end; i++)
			{
				s = this.rows[i];

				if (s.StartsWith(Prefix))
				{
					s = s.Substring(d);
					j = d;
				}
				else
					j = 0;

				k = 0;
				foreach (char ch in s)
				{
					if (ch > ' ')
						break;
					else
					{
						k++;

						if (ch == ' ')
							j++;
						else if (ch == '\t')
							j += 4;
					}
				}

				if (k > 0)
					s = s.Substring(k);

				if (string.IsNullOrEmpty(s))
				{
					if (!FirstRow)
					{
						Result.Add(new Block(Rows.ToArray(), Indent));
						Rows.Clear();
						Indent = 0;
						FirstRow = true;
					}
				}
				else
				{
					if (FirstRow)
					{
						FirstRow = false;
						Indent = (j - NrCharacters) / 4;
					}

					Rows.Add(s);
				}
			}

			if (!FirstRow)
				Result.Add(new Block(Rows.ToArray(), Indent));

			return Result;
		}
	}
}
