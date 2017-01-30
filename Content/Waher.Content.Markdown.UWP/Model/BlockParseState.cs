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
		private int start;
		private int end;
		private int pos;
		private int len;
		private bool lineBreakAfter;
		private bool preserveCrLf;
		private char lastChar = (char)0;

		public BlockParseState(string[] Rows, int Start, int End, bool PreserveCrLf)
		{
			this.rows = Rows;
			this.current = this.start = Start;
			this.end = End;
			this.currentRow = this.rows[this.current];
			this.lineBreakAfter = this.currentRow.EndsWith("  ");
			this.pos = 0;
			this.len = this.currentRow.Length;	// >= 1
			this.preserveCrLf = PreserveCrLf;

			if (this.lineBreakAfter)
			{
				this.currentRow = this.currentRow.Substring(0, this.len - 2);
				this.len -= 2;
			}
		}

		public string[] Rows
		{
			get { return this.rows; }
		}

		public int Start
		{
			get { return this.start; }
		}

		public int End
		{
			get { return this.end; }
		}

		public int Current
		{
			get { return this.current; }
		}

		public bool PreserveCrLf
		{
			get { return this.preserveCrLf; }
		}

		public char NextNonWhitespaceChar()
		{
			char ch = this.NextChar();

			while (ch > (char)0 && (ch <= ' ' || ch == 160))
				ch = this.NextChar();

			return ch;
		}

		public char NextNonWhitespaceCharSameRow()
		{
			char ch = this.NextCharSameRow();

			while (ch > (char)0 && (ch <= ' ' || ch == 160))
				ch = this.NextCharSameRow();

			return ch;
		}

		public char NextCharSameRow()
		{
			if (this.pos >= this.len)
				return this.lastChar = (char)0;
			else
				return this.lastChar = this.currentRow[this.pos++];
		}

		public char PeekNextNonWhitespaceCharSameRow()
		{
			char ch = this.PeekNextCharSameRow();

			while (ch > 0 && (ch <= ' ' || ch == 160))
			{
				this.NextCharSameRow();
				ch = this.PeekNextCharSameRow();
			}

			return ch;
		}

		public char PeekNextNonWhitespaceChar()
		{
			char ch = this.PeekNextChar();

			while (ch > 0 && (ch <= ' ' || ch == 160))
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

		private class StateBackup
		{
			public int Pos;
			public int Len;
			public int Current;
			public string CurrentRow;
			public bool LineBreakAfter;
		}

		private LinkedList<StateBackup> backup = null;

		public void BackupState()
		{
			StateBackup Backup = new StateBackup();
			Backup.Pos = this.pos;
			Backup.Len = this.len;
			Backup.Current = this.current;
			Backup.CurrentRow = this.currentRow;
			Backup.LineBreakAfter = this.lineBreakAfter;

			if (this.backup == null)
				this.backup = new LinkedList<StateBackup>();

			this.backup.AddFirst(Backup);
		}

		public void RestoreState()
		{
			if (this.backup == null || this.backup.First == null)
				throw new Exception("No state backup to restore.");

			StateBackup Backup = this.backup.First.Value;
			this.backup.RemoveFirst();

			this.pos = Backup.Pos;
			this.len = Backup.Len;
			this.current = Backup.Current;
			this.currentRow = Backup.CurrentRow;
			this.lineBreakAfter = Backup.LineBreakAfter;
		}

		public void DiscardBackup()
		{
			if (this.backup == null || this.backup.First == null)
				throw new Exception("No state backup to discard.");

			this.backup.RemoveFirst();
		}

		public void SkipWhitespaceSameRow(int MaxSpaces)
		{
			char ch;

			while ((((ch = this.PeekNextCharSameRow()) <= ' ' && ch > 0) || ch == 160) && MaxSpaces > 0)
			{
				this.NextCharSameRow();

				if (ch == ' ' || ch == 160)
					MaxSpaces--;
				else if (ch == '\t')
					MaxSpaces -= 4;
			}
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
					else if (this.preserveCrLf)
						ch = '\r';
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

			return this.lastChar = ch;
		}

		public string RestOfRow()
		{
			string Result;

			if (this.pos >= this.len)
				Result = string.Empty;
			else
				Result = this.currentRow.Substring(this.pos);

			this.current++;
			if (this.current > this.end)
			{
				this.pos = 0;
				this.len = 0;
			}
			else
			{
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

			return Result;
		}

		public bool IsFirstCharOnLine
		{
			get
			{
				int i = this.pos - 2;
				char ch;

				if (i == -1)
					return true;

				if (i < 0 || i >= this.len)
					return false;

				while (i >= 0 && ((ch = this.currentRow[i]) <= ' ' || ch == 160))
					i--;

				return i < 0;
			}
		}

		public bool EOF
		{
			get
			{
				return (this.pos >= this.len && this.current > this.end);
			}
		}

		public string CurrentRow
		{
			get { return this.currentRow; }
		}

		public char LastCharacter
		{
			get { return this.lastChar; }
		}

	}
}
