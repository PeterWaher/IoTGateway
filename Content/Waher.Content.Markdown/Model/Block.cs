using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Waher.Content.Markdown.Model.BlockElements;

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

		private static readonly Regex caption = new Regex(@"^\s*([\[](?'Caption'[^\]]*)[\]])?([\[](?'Id'[^\]]*)[\]])\s*$", RegexOptions.Compiled);

		public bool IsTable(out TableInformation TableInformation)
		{
			string Caption = string.Empty;
			string Id = string.Empty;
			string s;
			int Columns = 0;
			int UnderlineRow = -1;
			int i, j;
			int End = this.end;
			bool IsUnderline;
			Match M;

			TableInformation = null;

			for (i = this.start; i <= End; i++)
			{
				j = 0;
				IsUnderline = true;

				s = this.rows[i];

				if (i == End && (M = caption.Match(s)).Success)
				{
					End--;
					Caption = M.Groups["Caption"].Value;
					Id = M.Groups["Id"].Value;
				}
				else
				{
					if (s.StartsWith("|"))
						s = s.TrimEnd().Substring(1);

					if (s.EndsWith("|"))
						s = s.Substring(0, s.Length - 1);

					this.rows[i] = s;

					foreach (char ch in s)
					{
						if (ch == '|')
							j++;
						else if (ch != '-' && ch != ':' && ch > ' ')
							IsUnderline = false;
					}

					if (IsUnderline && UnderlineRow < 0)
						UnderlineRow = i;

					if (j == 0)
						return false;
					else if (i == this.start)
						Columns = j + 1;
					else if (Columns != j + 1)
						return false;
				}
			}

			if (UnderlineRow < 0)
				return false;

			s = this.rows[UnderlineRow];
			string[] Parts = s.Split('|');

			TableCellAlignment[] Alignments = new TableCellAlignment[Columns];
			bool Left;
			bool Right;

			for (j = 0; j < Columns; j++)
			{
				s = Parts[j].Trim();

				Left = s.StartsWith(":");
				Right = s.EndsWith(":");

				if (Left && Right)
					Alignments[j] = TableCellAlignment.Center;
				else if (Right)
					Alignments[j] = TableCellAlignment.Right;
				else
					Alignments[j] = TableCellAlignment.Left;

				if (Left)
					s = s.Substring(1);

				if (Right)
					s = s.Substring(0, s.Length - 1);

				foreach (char ch in s)
				{
					if (ch != '-')
						return false;
				}
			}

			TableInformation = new TableInformation();
			TableInformation.Columns = Columns;
			TableInformation.NrHeaderRows = (UnderlineRow - this.start);
			TableInformation.NrDataRows = End - UnderlineRow;
			TableInformation.Headers = new string[TableInformation.NrHeaderRows][];
			TableInformation.Rows = new string[TableInformation.NrDataRows][];
			TableInformation.Alignments = Alignments;
			TableInformation.Caption = Caption;
			TableInformation.Id = Id;

			for (i = 0; i < TableInformation.NrHeaderRows; i++)
			{
				TableInformation.Headers[i] = this.rows[this.start + i].Split('|');
				for (j = 0; j < Columns; j++)
				{
					s = TableInformation.Headers[i][j];
					if (string.IsNullOrEmpty(s))
						s = null;
					else
						s = s.Trim();

					TableInformation.Headers[i][j] = s;
				}
			}

			for (i = 0; i < TableInformation.NrDataRows; i++)
			{
				TableInformation.Rows[i] = this.rows[UnderlineRow + i + 1].Split('|');
				for (j = 0; j < Columns; j++)
				{
					s = TableInformation.Rows[i][j];
					if (string.IsNullOrEmpty(s))
						s = null;
					else
						s = s.Trim();

					TableInformation.Rows[i][j] = s;
				}
			}

			return true;
		}

		public bool IsFootnote(out string Label)
		{
			string s;
			int i, j, c;
			char ch;

			Label = null;
			s = this.rows[this.start];

			if (!s.StartsWith("[^"))
				return false;

			i = s.IndexOf("]:");
			if (i < 0)
				return false;

			Label = s.Substring(2, i - 2);

			i += 2;
			j = 0;
			c = s.Length;

			while (i < c && j < 3 && (ch = s[i]) <= ' ')
			{
				i++;

				if (ch == ' ')
					j++;
				else if (ch == '\t')
					j += 4;
			}

			this.rows[this.start] = s.Substring(i);

			return true;
		}

	}
}
