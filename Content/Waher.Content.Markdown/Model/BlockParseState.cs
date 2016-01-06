using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model
{
	internal class BlockParseState
	{
		private string[] rows;
		private string currentRow;
		private int current;
		private int end;
		private int pos;
		private int len;
		private bool lineBreakAfter;

		public BlockParseState(string[] Rows, int Start, int End)
		{
			this.rows = Rows;
			this.current = Start;
			this.end = End;
			this.currentRow = this.rows[this.current];
			this.lineBreakAfter = this.currentRow.EndsWith("  ");
			this.pos = 0;
			this.len = this.currentRow.Length;	// >= 1

			if (this.lineBreakAfter)
			{
				this.currentRow = this.currentRow.Substring(0, this.len - 2);
				this.len -= 2;
			}
		}

		public char NextNonWhitespaceChar()
		{
			char ch = this.NextChar();

			while (ch > (char)0 && ch <= ' ')
				ch = this.NextChar();

			return ch;
		}

		public char NextNonWhitespaceCharSameRow()
		{
			char ch = this.NextCharSameRow();

			while (ch > (char)0 && ch <= ' ')
				ch = this.NextCharSameRow();

			return ch;
		}

		public char NextCharSameRow()
		{
			if (this.pos >= this.len)
				return (char)0;
			else
				return this.currentRow[this.pos++];
		}

		public char PeekNextNonWhitespaceCharSameRow()
		{
			char ch = this.PeekNextCharSameRow();

			while (ch > 0 && ch <= ' ')
			{
				this.NextCharSameRow();
				ch = this.PeekNextCharSameRow();
			}

			return ch;
		}

		public char PeekNextNonWhitespaceChar()
		{
			char ch = this.PeekNextChar();

			while (ch > 0 && ch <= ' ')
			{
				this.NextChar();
				ch = this.PeekNextChar();
			}

			return ch;
		}

		public char PeekNextCharSameRow()
		{
			if (this.pos >= this.len)
				return (char)0;
			else
				return this.currentRow[this.pos];
		}

		public char PeekNextChar()
		{
			int PosBak = this.pos;
			int LenBak = this.len;
			int CurrentBak = this.current;
			string CurrentRowBak = this.currentRow;
			bool LineBreakAfterBak = this.lineBreakAfter;

			char ch = this.NextChar();

			this.pos = PosBak;
			this.len = LenBak;
			this.current = CurrentBak;
			this.currentRow = CurrentRowBak;
			this.lineBreakAfter = LineBreakAfterBak;
			
			return ch;
		}

		public char NextChar()
		{
			char ch;

			if (this.pos >= this.len)
			{
				this.current++;
				if (this.current > this.end)
				{
					this.pos = 0;
					this.len = 0;

					ch = (char)0;
				}
				else
				{
					if (this.lineBreakAfter)
						ch = '\n';
					else
						ch = ' ';

					this.currentRow = this.rows[this.current];
					this.pos = 0;
					this.len = this.currentRow.Length;
					this.lineBreakAfter = this.currentRow.EndsWith("  ");

					if (this.lineBreakAfter)
					{
						this.currentRow = this.currentRow.Substring(0, this.len - 2);
						this.len -= 2;
					}
				}
			}
			else
				ch = this.currentRow[this.pos++];

			return ch;
		}

		public bool IsFirstCharOnLine
		{
			get
			{
				int i = this.pos - 2;

				if (i == -1)
					return true;

				if (i < 0 || i >= this.len)
					return false;

				while (i >= 0 && this.currentRow[i] <= ' ')
					i--;

				return i < 0;
			}
		}

	}
}
