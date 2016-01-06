using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Model.BlockElements;
using Waher.Content.Markdown.Model.SpanElements;

namespace Waher.Content.Markdown
{
	/// <summary>
	/// Contains a markdown document. This markdown document class supports original markdown, as well as several markdown extensions, as
	/// defined in the following links.
	/// 
	/// Original Markdown was invented by John Gruber at Daring Fireball.
	/// http://daringfireball.net/projects/markdown/basics
	/// http://daringfireball.net/projects/markdown/syntax
	/// 
	/// The Smarty Pants addition for markdown is also supported:
	/// http://daringfireball.net/projects/smartypants/
	/// 
	/// There are however some exceptions to the rule, and some definitions where the implementation in <see cref="Waher.Content.Markdown"/> differ:
	/// 
	/// - Markdown syntax within HTML constructs is allowed.
	/// - Numbered lists retain the number used in the text.
	/// - Lazy numbering supported by the use of "#."
	/// - _underline_ underlines text.
	/// - __inserted__ displays inserted text.
	/// - ~strike through~ strikes through text.
	/// - ~~deleted~~ displays deleted text.
	/// 
	/// - Any multimedia, not just images, can be inserted using the ! syntax, including audio and video. The architecture is pluggable and allows for 
	///   customization of inclusion of content, including web content such as YouTube videos, etc. Multimedia can have additional width and height 
	///   information. Multimedia handler is selected based on URL or file extension. If no particular multimedia handler is found, the source is 
	///   considered to be an image.
	///   
	///   Examples:
	///   
	///	    ![some text](/some/url "some title" WIDTH HEIGHT) where WIDTH and HEIGHT are positive integers.
	///     ![Your browser does not support the audio tag](/local/music.mp3)            (is rendered using the &lt;audio&gt; tag)
	///     ![Your browser does not support the video tag](/local/video.mp4 320 200)    (is rendered using the &lt;video&gt; tag)
	///     ![Your browser does not support the iframe tag](https://www.youtube.com/watch?v=whBPLc8m4SU 800 600)
	///
	///   Width and Height can also be defined in referenced content. Example: ![some text][someref]
	///   [someref]: some/url "some title" WIDTH HEIGHT
	/// </summary>
	public class MarkdownDocument
	{
		private Dictionary<string, Multimedia> references = new Dictionary<string, Multimedia>();
		private LinkedList<MarkdownElement> elements;
		private string markdownText;

		public MarkdownDocument(string MarkdownText)
		{
			this.markdownText = MarkdownText;

			List<KeyValuePair<int, string[]>> Blocks = this.ParseBlocks(MarkdownText);
			this.elements = this.ParseBlocks(Blocks, 0, Blocks.Count - 1);
		}

		private LinkedList<MarkdownElement> ParseBlocks(List<KeyValuePair<int, string[]>> Blocks, int StartBlock, int EndBlock)
		{
			LinkedList<MarkdownElement> Elements = new LinkedList<MarkdownElement>();
			LinkedList<MarkdownElement> Content;
			KeyValuePair<int, string[]> Block;
			string[] Rows;
			string s;
			int BlockIndex;
			int i, c, d;
			char ch;

			for (BlockIndex = StartBlock; BlockIndex <= EndBlock; BlockIndex++)
			{
				Block = Blocks[BlockIndex];
				Rows = Block.Value;

				c = Rows.Length;	// Must be >= 1

				// Header?

				if (c > 1)
				{
					s = Rows[c - 1];

					if (this.IsUnderline(s, '='))
					{
						Elements.AddLast(new Header(this, 1, this.ParseBlock(Rows, 0, c - 2)));
						continue;
					}
					else if (this.IsUnderline(s, '-'))
					{
						Elements.AddLast(new Header(this, 2, this.ParseBlock(Rows, 0, c - 2)));
						continue;
					}
				}

				s = Rows[0];
				if (this.IsPrefixedBy(s, '#', out d) && d < s.Length)
				{
					if ((ch = s[d]) <= ' ')
					{
						Rows[0] = Rows[0].Substring(d + 1);

						s = Rows[c - 1];
						d = s.Length;
						i = d - 1;
						while (i >= 0 && s[i] == '#')
							i--;

						if (i < d - 1)
							Rows[c - 1] = s.Substring(0, i + 1);

						Elements.AddLast(new Header(this, d, this.ParseBlock(Rows, 0, c - 1)));
						continue;
					}
					else if (ch == '.')
					{
						// TODO: Lazy numbered bullet list.
					}
				}

				Content = this.ParseBlock(Rows, 0, c - 1);
				if (Content.First != null)
				{
					if (Content.First.Value is InlineHTML && Content.Last.Value is InlineHTML)
						Elements.AddLast(new HtmlBlock(this, Content));
					else
						Elements.AddLast(new Paragraph(this, Content));
				}
			}

			return Elements;
		}

		private LinkedList<MarkdownElement> ParseBlock(string[] Rows, int StartRow, int EndRow)
		{
			LinkedList<MarkdownElement> Elements = new LinkedList<MarkdownElement>();
			BlockParseState State = new BlockParseState(Rows, StartRow, EndRow);

			this.ParseBlock(State, (char)0, 1, Elements);

			return Elements;
		}

		private bool ParseBlock(BlockParseState State, char TerminationCharacter, int TerminationCharacterCount, LinkedList<MarkdownElement> Elements)
		{
			LinkedList<MarkdownElement> ChildElements;
			StringBuilder Text = new StringBuilder();
			string Url, Title;
			int NrTerminationCharacters = 0;
			char ch, ch2, ch3;
			int? Width;
			int? Height;
			bool FirstCharOnLine;

			while ((ch = State.NextChar()) != (char)0)
			{
				if (ch == TerminationCharacter)
				{
					NrTerminationCharacters++;
					if (NrTerminationCharacters >= TerminationCharacterCount)
						break;
					else
						continue;
				}
				else
				{
					while (NrTerminationCharacters > 0)
					{
						Text.Append(TerminationCharacter);
						NrTerminationCharacters--;
					}
				}

				switch (ch)
				{
					case '\n':
						this.AppendAnyText(Elements, Text);
						Elements.AddLast(new LineBreak(this));
						break;

					case '*':
						if (State.PeekNextCharSameRow() <= ' ')
						{
							Text.Append('*');
							break;
						}

						this.AppendAnyText(Elements, Text);
						ChildElements = new LinkedList<MarkdownElement>();
						ch2 = State.PeekNextCharSameRow();
						if (ch2 == '*')
						{
							State.NextCharSameRow();

							if (this.ParseBlock(State, '*', 2, ChildElements))
								Elements.AddLast(new Strong(this, ChildElements));
							else
								this.FixSyntaxError(Elements, "**", ChildElements);
						}
						else
						{
							if (this.ParseBlock(State, '*', 1, ChildElements))
								Elements.AddLast(new Emphasize(this, ChildElements));
							else
								this.FixSyntaxError(Elements, "*", ChildElements);
						}
						break;

					case '_':
						if (State.PeekNextCharSameRow() <= ' ')
						{
							Text.Append('_');
							break;
						}

						this.AppendAnyText(Elements, Text);
						ChildElements = new LinkedList<MarkdownElement>();
						ch2 = State.PeekNextCharSameRow();
						if (ch2 == '_')
						{
							State.NextCharSameRow();

							if (this.ParseBlock(State, '_', 2, ChildElements))
								Elements.AddLast(new Insert(this, ChildElements));
							else
								this.FixSyntaxError(Elements, "__", ChildElements);
						}
						else
						{
							if (this.ParseBlock(State, '_', 1, ChildElements))
								Elements.AddLast(new Underline(this, ChildElements));
							else
								this.FixSyntaxError(Elements, "_", ChildElements);
						}
						break;

					case '~':
						if (State.PeekNextCharSameRow() <= ' ')
						{
							Text.Append('~');
							break;
						}

						this.AppendAnyText(Elements, Text);
						ChildElements = new LinkedList<MarkdownElement>();
						ch2 = State.PeekNextCharSameRow();
						if (ch2 == '~')
						{
							State.NextCharSameRow();

							if (this.ParseBlock(State, '~', 2, ChildElements))
								Elements.AddLast(new Delete(this, ChildElements));
							else
								this.FixSyntaxError(Elements, "~~", ChildElements);
						}
						else
						{
							if (this.ParseBlock(State, '~', 1, ChildElements))
								Elements.AddLast(new StrikeThrough(this, ChildElements));
							else
								this.FixSyntaxError(Elements, "~", ChildElements);
						}
						break;

					case '`':
						this.AppendAnyText(Elements, Text);
						ChildElements = new LinkedList<MarkdownElement>();
						ch2 = State.PeekNextCharSameRow();
						if (ch2 == '`')
						{
							State.NextCharSameRow();

							if (this.ParseBlock(State, '`', 2, ChildElements))
								Elements.AddLast(new InlineCode(this, ChildElements));
							else
								this.FixSyntaxError(Elements, "``", ChildElements);
						}
						else
						{
							if (this.ParseBlock(State, '`', 1, ChildElements))
								Elements.AddLast(new InlineCode(this, ChildElements));
							else
								this.FixSyntaxError(Elements, "`", ChildElements);
						}
						break;

					case '[':
					case '!':
						if (ch == '!')
						{
							ch2 = State.PeekNextCharSameRow();
							if (ch2 != '[')
							{
								Text.Append('!');
								break;
							}

							State.NextCharSameRow();
						}

						ChildElements = new LinkedList<MarkdownElement>();
						FirstCharOnLine = State.IsFirstCharOnLine;

						this.AppendAnyText(Elements, Text);

						if (this.ParseBlock(State, ']', 1, ChildElements))
						{
							ch2 = State.NextNonWhitespaceChar();
							if (ch2 == '(')
							{
								Title = string.Empty;

								while ((ch2 = State.NextChar()) != 0 && ch2 > ' ' && ch2 != ')')
									Text.Append(ch2);

								Url = Text.ToString();
								Text.Clear();

								if (Url.StartsWith("<") && Url.EndsWith(">"))
									Url = Url.Substring(1, Url.Length - 2);

								if (ch2 <= ' ')
								{
									ch2 = State.PeekNextNonWhitespaceChar();

									if (ch2 == '"' || ch2 == '\'')
									{
										State.NextChar();
										while ((ch3 = State.NextCharSameRow()) != 0 && ch3 != ch2)
											Text.Append(ch3);

										ch2 = ch3;
										Title = Text.ToString();
										Text.Clear();
									}
									else
										Title = string.Empty;
								}

								if (ch == '!')
									this.ParseWidthHeight(State, out Width, out Height);
								else
									Width = Height = null;

								while (ch2 != 0 && ch2 != ')')
									ch2 = State.NextCharSameRow();

								if (ch == '!')
									Elements.AddLast(new Multimedia(this, ChildElements, Url, Title, Width, Height));
								else
									Elements.AddLast(new Link(this, ChildElements, Url, Title));
							}
							else if (ch2 == ':' && FirstCharOnLine)
							{
								ch2 = State.NextChar();
								while (ch2 != 0 && ch2 <= ' ')
									ch2 = State.NextChar();

								if (ch2 > ' ')
								{
									Text.Append(ch2);
									while ((ch2 = State.NextCharSameRow()) != 0 && ch2 > ' ')
										Text.Append(ch2);

									Url = Text.ToString();
									Text.Clear();

									if (Url.StartsWith("<") && Url.EndsWith(">"))
										Url = Url.Substring(1, Url.Length - 2);

									ch2 = State.PeekNextNonWhitespaceChar();

									if (ch2 == '"' || ch2 == '\'' || ch2 == '(')
									{
										State.NextChar();
										if (ch2 == '(')
											ch2 = ')';

										while ((ch3 = State.NextCharSameRow()) != 0 && ch3 != ch2)
											Text.Append(ch3);

										ch2 = ch3;
										Title = Text.ToString();
										Text.Clear();
									}
									else
										Title = string.Empty;

									this.ParseWidthHeight(State, out Width, out Height);

									foreach (MarkdownElement E in ChildElements)
										E.GeneratePlainText(Text);

									this.references[Text.ToString().ToLower()] = new Multimedia(this, null, Url, Title, Width, Height);
									Text.Clear();
								}
							}
							else if (ch2 == '[')
							{
								while ((ch2 = State.NextCharSameRow()) != 0 && ch2 != ']')
									Text.Append(ch2);

								Title = Text.ToString();
								Text.Clear();

								if (string.IsNullOrEmpty(Title))
								{
									foreach (MarkdownElement E in ChildElements)
										E.GeneratePlainText(Text);

									Title = Text.ToString();
									Text.Clear();
								}

								if (ch == '!')
									Elements.AddLast(new MultimediaReference(this, ChildElements, Title));
								else
									Elements.AddLast(new LinkReference(this, ChildElements, Title));
							}
							else
							{
								this.FixSyntaxError(Elements, ch == '!' ? "![" : "[", ChildElements);
								Elements.AddLast(new InlineText(this, "]"));
							}
						}
						else
							this.FixSyntaxError(Elements, ch == '!' ? "![" : "[", ChildElements);
						break;

					case '<':
						if (!char.IsLetter(ch2 = State.PeekNextCharSameRow()) && ch2 != '/')
						{
							Text.Append(ch);
							break;
						}

						this.AppendAnyText(Elements, Text);
						Text.Append(ch);

						while ((ch2 = State.NextChar()) != 0 && ch2 != '>')
							Text.Append(ch2);

						if (ch2 == 0)
							break;

						Text.Append(ch2);
						Url = Text.ToString();

						if (Url.StartsWith("</") || Url.IndexOf(' ') >= 0)
							Elements.AddLast(new InlineHTML(this, Url));
						else if (Url.IndexOf(':') >= 0)
							Elements.AddLast(new AutomaticLinkUrl(this, Url.Substring(1, Url.Length - 2)));
						else if (Url.IndexOf('@') >= 0)
							Elements.AddLast(new AutomaticLinkMail(this, Url.Substring(1, Url.Length - 2)));
						else
							Elements.AddLast(new InlineHTML(this, Url));

						Text.Clear();

						break;

					case '\\':
						switch (ch2 = State.PeekNextCharSameRow())
						{
							case '*':
							case '_':
							case '~':
								Text.Append(ch2);
								State.NextCharSameRow();
								break;

							default:
								Text.Append('\\');
								break;
						}
						break;

					default:
						Text.Append(ch);
						break;
				}
			}

			this.AppendAnyText(Elements, Text);

			return (ch == TerminationCharacter);
		}

		private void ParseWidthHeight(BlockParseState State, out int? Width, out int? Height)
		{
			Width = null;
			Height = null;

			char ch = State.PeekNextNonWhitespaceCharSameRow();
			if (ch >= '0' && ch <= '9')
			{
				StringBuilder Text = new StringBuilder();
				int i;

				Text.Append(ch);
				State.NextCharSameRow();

				ch = State.PeekNextCharSameRow();
				while (ch >= '0' && ch <= '9')
				{
					Text.Append(ch);
					State.NextCharSameRow();
					ch = State.PeekNextCharSameRow();
				}

				if (int.TryParse(Text.ToString(), out i))
				{
					Width = i;
					Text.Clear();

					ch = State.PeekNextNonWhitespaceCharSameRow();
					if (ch >= '0' && ch <= '9')
					{
						Text.Append(ch);
						State.NextCharSameRow();

						ch = State.PeekNextCharSameRow();
						while (ch >= '0' && ch <= '9')
						{
							Text.Append(ch);
							State.NextCharSameRow();
							ch = State.PeekNextCharSameRow();
						}

						if (int.TryParse(Text.ToString(), out i))
							Height = i;
					}
				}
			}
		}

		private void AppendAnyText(LinkedList<MarkdownElement> Elements, StringBuilder Text)
		{
			if (Text.Length > 0)
			{
				string s = Text.ToString();
				Text.Clear();

				if (Elements.First != null || !string.IsNullOrEmpty(s.Trim()))
					Elements.AddLast(new InlineText(this, s));
			}
		}

		private void FixSyntaxError(LinkedList<MarkdownElement> Elements, string Prefix, LinkedList<MarkdownElement> ChildElements)
		{
			Elements.AddLast(new InlineText(this, "**"));
			foreach (MarkdownElement E in ChildElements)
				Elements.AddLast(E);
		}

		private bool IsPrefixedBy(string s, char ch, out int Count)
		{
			int c = s.Length;

			Count = 0;
			while (Count < c && s[Count] == ch)
				Count++;

			return Count > 0;
		}

		private bool IsUnderline(string s, char ch)
		{
			int i, c = s.Length;

			for (i = 0; i < c; i++)
			{
				if (s[i] != ch)
					return false;
			}

			return true;
		}

		private List<KeyValuePair<int, string[]>> ParseBlocks(string MarkdownText)
		{
			List<KeyValuePair<int, string[]>> Blocks = new List<KeyValuePair<int, string[]>>();
			List<string> Rows = new List<string>();
			int FirstLineIndent = 0;
			int RowStart = 0;
			int RowEnd = 0;
			int Pos, Len;
			char ch;
			bool InBlock = false;
			bool InRow = false;

			MarkdownText = MarkdownText.Replace("\r\n", "\n").Replace('\r', '\n');
			Len = MarkdownText.Length;

			for (Pos = 0; Pos < Len; Pos++)
			{
				ch = MarkdownText[Pos];

				if (ch == '\n')
				{
					if (InBlock)
					{
						if (InRow)
						{
							Rows.Add(MarkdownText.Substring(RowStart, RowEnd - RowStart + 1));
							InRow = false;
						}
						else
						{
							Blocks.Add(new KeyValuePair<int, string[]>(FirstLineIndent, Rows.ToArray()));
							Rows.Clear();
							InBlock = false;
							FirstLineIndent = 0;
						}
					}
					else
						FirstLineIndent = 0;
				}
				else if (ch <= ' ')
				{
					if (InBlock)
					{
						if (InRow)
							RowEnd = Pos;
					}
					else
						FirstLineIndent++;
				}
				else
				{
					if (!InRow)
					{
						InRow = true;
						InBlock = true;
						RowStart = Pos;
					}

					RowEnd = Pos;
				}
			}

			if (InBlock)
			{
				if (InRow)
					Rows.Add(MarkdownText.Substring(RowStart, RowEnd - RowStart + 1));

				Blocks.Add(new KeyValuePair<int, string[]>(FirstLineIndent, Rows.ToArray()));
			}

			return Blocks;
		}

		/// <summary>
		/// Generates HTML from the markdown text.
		/// </summary>
		/// <returns>HTML</returns>
		public string GenerateHTML()
		{
			StringBuilder Output = new StringBuilder();
			this.GenerateHTML(Output);
			return Output.ToString();
		}

		/// <summary>
		/// Generates HTML from the markdown text.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public void GenerateHTML(StringBuilder Output)
		{
			Output.AppendLine("<html>");
			Output.AppendLine("<head>");
			Output.AppendLine("<title />");
			Output.AppendLine("</head>");
			Output.AppendLine("<body>");

			foreach (MarkdownElement E in this.elements)
				E.GenerateHTML(Output);

			Output.AppendLine("</body>");
			Output.Append("</html>");
		}

		/// <summary>
		/// Generates Plain Text from the markdown text.
		/// </summary>
		/// <returns>PlainText</returns>
		public string GeneratePlainText()
		{
			StringBuilder Output = new StringBuilder();
			this.GeneratePlainText(Output);
			return Output.ToString();
		}

		/// <summary>
		/// Generates Plain Text from the markdown text.
		/// </summary>
		/// <param name="Output">PlainText will be output here.</param>
		public void GeneratePlainText(StringBuilder Output)
		{
			foreach (MarkdownElement E in this.elements)
				E.GeneratePlainText(Output);
		}

		internal Multimedia GetReference(string Label)
		{
			Multimedia Multimedia;

			if (this.references.TryGetValue(Label.ToLower(), out Multimedia))
				return Multimedia;
			else
				return null;
		}

		// Different from XML.Encode, in that it does not encode the aposotrophe.
		internal static string HtmlEncode(string s)
		{
			if (s.IndexOfAny(specialCharacters) < 0)
				return s;

			return s.
				Replace("&", "&amp;").
				Replace("<", "&lt;").
				Replace(">", "&gt;").
				Replace("\"", "&quot;");
		}

		private static readonly char[] specialCharacters = new char[] { '<', '>', '&', '"' };


	}
}
