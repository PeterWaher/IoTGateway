using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Waher.Content.Emoji;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Model.Atoms;
using Waher.Content.Markdown.Model.BlockElements;
using Waher.Content.Markdown.Model.SpanElements;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Script;
using Waher.Runtime.Text;
using System.Collections;

namespace Waher.Content.Markdown
{
	/// <summary>
	/// Delegate for markdown element callback methods.
	/// </summary>
	/// <param name="Element">Markdown element</param>
	/// <param name="State">State object.</param>
	/// <returns>If process should continue.</returns>
	public delegate bool MarkdownElementHandler(MarkdownElement Element, object State);

	/// <summary>
	/// Contains a markdown document. This markdown document class supports original markdown, as well as several markdown extensions.
	/// See the markdown reference documentation provided with the library for more information.
	/// </summary>
	public class MarkdownDocument : IFileNameResource, IEnumerable<MarkdownElement>
	{
		private readonly Dictionary<string, Multimedia> references = new Dictionary<string, Multimedia>();
		private readonly Dictionary<string, KeyValuePair<string, bool>[]> metaData = new Dictionary<string, KeyValuePair<string, bool>[]>();
		private Dictionary<string, int> footnoteNumbers = null;
		private Dictionary<string, Footnote> footnotes = null;
		private SortedDictionary<int, string> toInsert = null;
		private readonly Type[] transparentExceptionTypes;
		private List<string> footnoteOrder = null;
		private readonly LinkedList<MarkdownElement> elements;
		private readonly List<Header> headers = new List<Header>();
		private readonly IEmojiSource emojiSource;
		private string markdownText;
		private string fileName = string.Empty;
		private string resourceName = string.Empty;
		private string url = string.Empty;
		private MarkdownDocument master = null;
		private MarkdownDocument detail = null;
		private readonly MarkdownSettings settings;
		private int lastFootnote = 0;
		private bool footnoteBacklinksAdded = false;
		private bool syntaxHighlighting = false;
		private bool includesTableOfContents = false;
		private bool isDynamic = false;
		private bool? allowScriptTag = null;

		/// <summary>
		/// Contains a markdown document. This markdown document class supports original markdown, as well as several markdown extensions.
		/// </summary>
		/// <param name="MarkdownText">Markdown text.</param>
		/// <param name="TransparentExceptionTypes">If an exception is thrown when processing script in markdown, and the exception is of
		/// any of these types, the exception will be rethrown, instead of shown as an error in the generated output.</param>
		public MarkdownDocument(string MarkdownText, params Type[] TransparentExceptionTypes)
			: this(MarkdownText, new MarkdownSettings(), string.Empty, string.Empty, string.Empty, TransparentExceptionTypes)
		{
		}

		/// <summary>
		/// Contains a markdown document. This markdown document class supports original markdown, as well as several markdown extensions.
		/// </summary>
		/// <param name="MarkdownText">Markdown text.</param>
		/// <param name="Settings">Parser settings.</param>
		/// <param name="TransparentExceptionTypes">If an exception is thrown when processing script in markdown, and the exception is of
		/// any of these types, the exception will be rethrown, instead of shown as an error in the generated output.</param>
		public MarkdownDocument(string MarkdownText, MarkdownSettings Settings, params Type[] TransparentExceptionTypes)
			: this(MarkdownText, Settings, string.Empty, string.Empty, string.Empty, TransparentExceptionTypes)
		{
		}

		/// <summary>
		/// Contains a markdown document. This markdown document class supports original markdown, as well as several markdown extensions.
		/// </summary>
		/// <param name="MarkdownText">Markdown text.</param>
		/// <param name="Settings">Parser settings.</param>
		/// <param name="FileName">If the content is coming from a file, this parameter contains the name of that file. 
		/// Otherwise, the parameter is the empty string.</param>
		/// <param name="ResourceName">Local resource name of file, if accessed from a web server.</param>
		/// <param name="URL">Full URL of resource hosting the content, if accessed from a web server.</param>
		/// <param name="TransparentExceptionTypes">If an exception is thrown when processing script in markdown, and the exception is of
		/// any of these types, the exception will be rethrown, instead of shown as an error in the generated output.</param>
		public MarkdownDocument(string MarkdownText, MarkdownSettings Settings, string FileName, string ResourceName, string URL,
			params Type[] TransparentExceptionTypes)
		{
			this.markdownText = MarkdownText;
			this.emojiSource = Settings.EmojiSource;
			this.settings = Settings;
			this.fileName = FileName;
			this.resourceName = ResourceName;
			this.url = URL;
			this.transparentExceptionTypes = TransparentExceptionTypes;

			if (Settings.Variables != null)
				this.markdownText = Preprocess(this.markdownText, Settings, this.fileName, out this.isDynamic, TransparentExceptionTypes);

			this.markdownText = this.markdownText.Replace("\r\n", "\n").Replace('\r', '\n');

			List<Block> Blocks = this.ParseTextToBlocks(this.markdownText);
			List<KeyValuePair<string, bool>> Values = new List<KeyValuePair<string, bool>>();
			Block Block;
			KeyValuePair<string, bool>[] Prev;
			string s, s2;
			string Key = null;
			int Start = 0;
			int End = Blocks.Count - 1;
			int i, j;

			if (Settings.ParseMetaData && Blocks.Count > 0)
			{
				Block = Blocks[0];
				for (i = Block.Start; i <= Block.End; i++)
				{
					s = Block.Rows[i];

					j = s.IndexOf(':');
					if (j < 0)
					{
						if (string.IsNullOrEmpty(Key))
							break;

						Values.Add(new KeyValuePair<string, bool>(s.Trim(), s.EndsWith("  ")));
					}
					else
					{
						s2 = s.Substring(0, j).TrimEnd().ToUpper();

						if (string.IsNullOrEmpty(Key))
						{
							foreach (char ch in s2)
							{
								if (!char.IsLetter(ch) && !char.IsWhiteSpace(ch))
								{
									s2 = null;
									break;
								}
							}

							if (s2 is null)
								break;
						}
						else
						{
							if (this.metaData.TryGetValue(Key, out Prev))
								Values.InsertRange(0, Prev);

							this.metaData[Key] = Values.ToArray();
						}

						Values.Clear();
						Key = s2;
						Values.Add(new KeyValuePair<string, bool>(s.Substring(j + 1).Trim(), s.EndsWith("  ")));
					}
				}

				if (!string.IsNullOrEmpty(Key))
				{
					if (this.metaData.TryGetValue(Key, out Prev))
						Values.InsertRange(0, Prev);
					else if (string.Compare(Key, "Login", true) == 0)
						this.isDynamic = true;

					this.metaData[Key] = Values.ToArray();
					Start++;
				}
			}

			this.elements = this.ParseBlocks(Blocks, Start, End);

			if (this.toInsert != null)
			{
				foreach (KeyValuePair<int, string> P in this.toInsert)
					this.markdownText = this.markdownText.Insert(P.Key, P.Value);
			}
		}

		/// <summary>
		/// Markdown text. This text might differ slightly from the original text passed to the document.
		/// </summary>
		public string MarkdownText
		{
			get
			{
				if (this.markdownText is null)
				{
					StringBuilder Output = new StringBuilder();

					// TODO: Meta-data

					foreach (MarkdownElement E in this.elements)
						E.GenerateMarkdown(Output);

					// TODO: Footnotes

					this.markdownText = Output.ToString();
				}

				return this.markdownText;
			}
		}

		internal static Regex endOfHeader = new Regex(@"\n\s*\n", RegexOptions.Multiline | RegexOptions.Compiled);
		internal static Regex scriptHeader = new Regex(@"^[Ss][Cc][Rr][Ii][Pp][Tt]:\s*(?'ScriptFile'[^\r\n]*)", RegexOptions.Multiline | RegexOptions.Compiled);

		/// <summary>
		/// Preprocesses markdown text.
		/// </summary>
		/// <param name="Markdown">Markdown text</param>
		/// <param name="Settings">Markdown settings.</param>
		/// <param name="TransparentExceptionTypes">If an exception is thrown when processing script in markdown, and the exception is of
		/// any of these types, the exception will be rethrown, instead of shown as an error in the generated output.</param>
		/// <returns>Preprocessed markdown.</returns>
		public static string Preprocess(string Markdown, MarkdownSettings Settings, params Type[] TransparentExceptionTypes)
		{
			return Preprocess(Markdown, Settings, string.Empty, out bool _, TransparentExceptionTypes);
		}

		/// <summary>
		/// Preprocesses markdown text.
		/// </summary>
		/// <param name="Markdown">Markdown text</param>
		/// <param name="Settings">Markdown settings.</param>
		/// <param name="FileName">Filename of markdown.</param>
		/// <param name="TransparentExceptionTypes">If an exception is thrown when processing script in markdown, and the exception is of
		/// any of these types, the exception will be rethrown, instead of shown as an error in the generated output.</param>
		/// <returns>Preprocessed markdown.</returns>
		public static string Preprocess(string Markdown, MarkdownSettings Settings, string FileName, params Type[] TransparentExceptionTypes)
		{
			return Preprocess(Markdown, Settings, FileName, out bool _, TransparentExceptionTypes);
		}

		/// <summary>
		/// Preprocesses markdown text.
		/// </summary>
		/// <param name="Markdown">Markdown text</param>
		/// <param name="Settings">Markdown settings.</param>
		/// <param name="FileName">Filename of markdown.</param>
		/// <param name="IsDynamic">If the markdown contained preprocessed script.</param>
		/// <param name="TransparentExceptionTypes">If an exception is thrown when processing script in markdown, and the exception is of
		/// any of these types, the exception will be rethrown, instead of shown as an error in the generated output.</param>
		/// <returns>Preprocessed markdown.</returns>
		public static string Preprocess(string Markdown, MarkdownSettings Settings, string FileName, out bool IsDynamic, params Type[] TransparentExceptionTypes)
		{
			Variables Variables = Settings.Variables;
			Expression Exp;
			string Script, s2;
			object Result;
			int i, j;

			IsDynamic = false;

			if (!string.IsNullOrEmpty(FileName))
			{
				Match M = endOfHeader.Match(Markdown);
				if (M.Success)
				{
					s2 = Markdown.Substring(0, M.Index);

					foreach (Match M2 in scriptHeader.Matches(s2))
					{
						if (M.Success)
						{
							string FileName2 = Settings.GetFileName(FileName, M2.Groups["ScriptFile"].Value);
							Script = File.ReadAllText(FileName2);

							if (!IsDynamic)
							{
								IsDynamic = true;
								Variables.Add(" MarkdownSettings ", Settings);
							}

							Exp = new Expression(Script);
							Exp.Evaluate(Variables);
						}
					}
				}
			}

			i = Markdown.IndexOf("{{");

			while (i >= 0)
			{
				j = Markdown.IndexOf("}}", i + 2);
				if (j < 0)
					break;

				Script = Markdown.Substring(i + 2, j - i - 2);
				Markdown = Markdown.Remove(i, j - i + 2);

				try
				{
					Exp = new Expression(Script);

					if (!IsDynamic)
					{
						IsDynamic = true;
						Variables.Add(" MarkdownSettings ", Settings);
					}

					if (Exp.ContainsImplicitPrint)
					{
						TextWriter Bak = Variables.ConsoleOut;
						StringBuilder sb = new StringBuilder();

						Variables.Lock();
						Variables.ConsoleOut = new StringWriter(sb);
						try
						{
							Exp.Evaluate(Variables);
						}
						finally
						{
							Variables.ConsoleOut.Flush();
							Variables.ConsoleOut = Bak;
							Variables.Release();
						}

						Result = sb.ToString();
					}
					else
						Result = Exp.Evaluate(Variables);
				}
				catch (Exception ex)
				{
					ex = Log.UnnestException(ex);

					if (ex is AggregateException ex2)
					{
						StringBuilder sb = new StringBuilder();

						foreach (Exception ex3 in ex2.InnerExceptions)
						{
							CheckException(ex3, TransparentExceptionTypes);

							Log.Critical(ex3);

							sb.Append("<p><font style=\"color:red\">");
							sb.Append(XML.HtmlValueEncode(ex3.Message));
							sb.Append("</font></p>");
						}

						Result = sb.ToString();
					}
					else
					{
						CheckException(ex, TransparentExceptionTypes);

						Log.Critical(ex);

						Result = "<font style=\"color:red\">" + XML.HtmlValueEncode(ex.Message) + "</font>";
					}
				}

				if (Result != null)
				{
					if (!(Result is string s3))
					{
						StringBuilder Html = new StringBuilder();
						InlineScript.GenerateHTML(Result, Html, false, Variables);
						s3 = Html.ToString();
					}

					Markdown = Markdown.Insert(i, s3);
					i += s3.Length;
				}

				i = Markdown.IndexOf("{{", i);
			}

			return Markdown;
		}

		internal void CheckException(Exception ex)
		{
			CheckException(ex, this.transparentExceptionTypes);
		}

		internal static void CheckException(Exception ex, Type[] TransparentExceptionTypes)
		{
			TypeInfo ExceptionType = ex.GetType().GetTypeInfo();

			foreach (Type T in TransparentExceptionTypes)
			{
				if (T.GetTypeInfo().IsAssignableFrom(ExceptionType))
					System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
			}
		}

		private LinkedList<MarkdownElement> ParseBlocks(List<Block> Blocks)
		{
			return this.ParseBlocks(Blocks, 0, Blocks.Count - 1);
		}

		private LinkedList<MarkdownElement> ParseBlocks(List<Block> Blocks, int StartBlock, int EndBlock)
		{
			LinkedList<MarkdownElement> Elements = new LinkedList<MarkdownElement>();
			LinkedList<MarkdownElement> Content;
			Block Block;
			string[] Rows;
			string s, s2;
			string InitialSectionSeparator = null;
			int BlockIndex;
			int i, j, c, d;
			int Index;
			int SectionNr = 0;
			int InitialNrColumns = 1;
			bool HasSections = false;

			for (BlockIndex = StartBlock; BlockIndex <= EndBlock; BlockIndex++)
			{
				Block = Blocks[BlockIndex];

				if (Block.Indent > 0)
				{
					c = Block.Indent;
					i = BlockIndex + 1;
					while (i <= EndBlock && (j = Blocks[i].Indent) > 0)
					{
						i++;
						if (j < c)
							c = j;
					}

					if (i == BlockIndex + 1)
						Elements.AddLast(new CodeBlock(this, Block.Rows, Block.Start, Block.End, c - 1));
					else
					{
						List<string> CodeBlock = new List<string>();

						while (BlockIndex < i)
						{
							if (CodeBlock.Count > 0)
								CodeBlock.Add(string.Empty);

							Block = Blocks[BlockIndex++];

							if (Block.Indent == c)
							{
								for (j = Block.Start; j <= Block.End; j++)
									CodeBlock.Add(Block.Rows[j]);
							}
							else
							{
								s = new string('\t', Block.Indent - c);
								for (j = Block.Start; j <= Block.End; j++)
									CodeBlock.Add(s + Block.Rows[j]);
							}
						}

						Elements.AddLast(new CodeBlock(this, CodeBlock.ToArray(), 0, CodeBlock.Count - 1, c - 1));
						BlockIndex--;
					}
					continue;
				}
				else if (Block.IsPrefixedBy("```", false))
				{
					i = BlockIndex;
					while (i <= EndBlock && (!(Block = Blocks[i]).Rows[Block.End].StartsWith("```") || (i == BlockIndex && Block.Start == Block.End)))
						i++;

					if (i <= EndBlock)
					{
						List<string> Code = new List<string>();

						for (j = BlockIndex; j <= i; j++)
						{
							Block = Blocks[j];
							if (j == BlockIndex)
								Index = Block.Start + 1;
							else
							{
								Code.Add(string.Empty);
								Index = Block.Start;
							}

							if (j == i)
								c = Block.End - 1;
							else
								c = Block.End;

							while (Index <= c)
							{
								Code.Add(Block.Rows[Index]);
								Index++;
							}
						}

						Block = Blocks[BlockIndex];
						s = Block.Rows[Block.Start].Substring(3).Trim('`', ' ', '\t');
						Elements.AddLast(new CodeBlock(this, Code.ToArray(), 0, Code.Count - 1, 0, s));

						if (!string.IsNullOrEmpty(s))
							this.syntaxHighlighting = true;

						BlockIndex = i;
						continue;
					}
				}

				if (Block.IsPrefixedBy(">", false))
				{
					Content = this.ParseBlocks(Block.RemovePrefix(">", 2));

					if (Elements.Last != null && Elements.Last.Value is BlockQuote BlockQuote)
						BlockQuote.AddChildren(Content);
					else
						Elements.AddLast(new BlockQuote(this, Content));

					continue;
				}
				else if (Block.IsPrefixedBy("+>", false))
				{
					Content = this.ParseBlocks(Block.RemovePrefix("+>", 3));

					if (Elements.Last != null && Elements.Last.Value is InsertBlocks InsertBlocks)
						InsertBlocks.AddChildren(Content);
					else
						Elements.AddLast(new InsertBlocks(this, Content));

					continue;
				}
				else if (Block.IsPrefixedBy("->", false))
				{
					Content = this.ParseBlocks(Block.RemovePrefix("->", 3));

					if (Elements.Last != null && Elements.Last.Value is DeleteBlocks DeleteBlocks)
						DeleteBlocks.AddChildren(Content);
					else
						Elements.AddLast(new DeleteBlocks(this, Content));

					continue;
				}
				else if (Block.End == Block.Start && (this.IsUnderline(Block.Rows[0], '-', true, true) || this.IsUnderline(Block.Rows[0], '*', true, true)))
				{
					Elements.AddLast(new HorizontalRule(this, Block.Rows[0]));
					continue;
				}
				else if (Block.End == Block.Start && (this.IsUnderline(Block.Rows[0], '=', true, false)))
				{
					int NrColumns = Block.Rows[0].Split(whiteSpace, StringSplitOptions.RemoveEmptyEntries).Length;
					HasSections = true;

					if (Elements.First is null)
					{
						InitialNrColumns = NrColumns;
						InitialSectionSeparator = Block.Rows[0];
					}
					else
						Elements.AddLast(new SectionSeparator(this, ++SectionNr, NrColumns, Block.Rows[0]));
					continue;
				}
				else if (Block.End == Block.Start && this.IsUnderline(Block.Rows[0], '~', false, false))
				{
					Elements.AddLast(new InvisibleBreak(this, Block.Rows[0]));
					continue;
				}
				else if (Block.IsPrefixedBy(s2 = "*", true) || Block.IsPrefixedBy(s2 = "+", true) || Block.IsPrefixedBy(s2 = "-", true))
				{
					LinkedList<Block> Segments = null;
					i = 0;
					c = Block.End;

					for (d = Block.Start + 1; d <= c; d++)
					{
						s = Block.Rows[d];
						if (IsPrefixedBy(s, s2, true))
						{
							if (Segments is null)
								Segments = new LinkedList<Block>();

							Segments.AddLast(new Block(Block.Rows, Block.Positions, 0, i, d - 1));
							i = d;
						}
					}

					if (Segments != null)
						Segments.AddLast(new Block(Block.Rows, Block.Positions, 0, i, c));

					if (Segments is null)
					{
						LinkedList<MarkdownElement> Items = this.ParseBlocks(Block.RemovePrefix(s2, 4));

						i = BlockIndex;
						while (BlockIndex < EndBlock && (Block = Blocks[BlockIndex + 1]).Indent > 0)
						{
							BlockIndex++;
							Block.Indent--;
						}

						if (BlockIndex > i)
						{
							foreach (MarkdownElement E in this.ParseBlocks(Blocks, i + 1, BlockIndex))
								Items.AddLast(E);
						}

						if (Elements.Last != null && Elements.Last.Value is BulletList BulletList)
							BulletList.AddChildren(new UnnumberedItem(this, s2 + " ", new NestedBlock(this, Items)));
						else
							Elements.AddLast(new BulletList(this, new UnnumberedItem(this, s2 + " ", new NestedBlock(this, Items))));

						continue;
					}
					else
					{
						LinkedList<MarkdownElement> Items = new LinkedList<MarkdownElement>();

						foreach (Block Segment in Segments)
						{
							foreach (Block SegmentItem in Segment.RemovePrefix(s2, 4))
							{
								Items.AddLast(new UnnumberedItem(this, s2 + " ", new NestedBlock(this,
									this.ParseBlock(SegmentItem.Rows, SegmentItem.Positions, SegmentItem.Start, SegmentItem.End))));
							}
						}

						if (Elements.Last != null && Elements.Last.Value is BulletList BulletList)
							BulletList.AddChildren(Items);
						else
							Elements.AddLast(new BulletList(this, Items));

						continue;
					}
				}
				else if (Block.IsPrefixedBy("#.", true))
				{
					LinkedList<Block> Segments = null;
					i = 0;
					c = Block.End;

					for (d = Block.Start + 1; d <= c; d++)
					{
						s = Block.Rows[d];
						if (IsPrefixedBy(s, "#.", true))
						{
							if (Segments is null)
								Segments = new LinkedList<Block>();

							Segments.AddLast(new Block(Block.Rows, Block.Positions, 0, i, d - 1));
							i = d;
						}
					}

					if (Segments != null)
						Segments.AddLast(new Block(Block.Rows, Block.Positions, 0, i, c));

					if (Segments is null)
					{
						LinkedList<MarkdownElement> Items = this.ParseBlocks(Block.RemovePrefix("#.", 4));

						i = BlockIndex;
						while (BlockIndex < EndBlock && (Block = Blocks[BlockIndex + 1]).Indent > 0)
						{
							BlockIndex++;
							Block.Indent--;
						}

						if (BlockIndex > i)
						{
							foreach (MarkdownElement E in this.ParseBlocks(Blocks, i + 1, BlockIndex))
								Items.AddLast(E);
						}

						if (Elements.Last != null && Elements.Last.Value is NumberedList NumberedList)
							NumberedList.AddChildren(new UnnumberedItem(this, "#. ", new NestedBlock(this, Items)));
						else
							Elements.AddLast(new NumberedList(this, new UnnumberedItem(this, "#. ", new NestedBlock(this, Items))));

						continue;
					}
					else
					{
						LinkedList<MarkdownElement> Items = new LinkedList<MarkdownElement>();

						foreach (Block Segment in Segments)
						{
							foreach (Block SegmentItem in Segment.RemovePrefix("#.", 4))
							{
								Items.AddLast(new UnnumberedItem(this, "#. ", new NestedBlock(this,
									this.ParseBlock(SegmentItem.Rows, SegmentItem.Positions, SegmentItem.Start, SegmentItem.End))));
							}
						}

						if (Elements.Last != null && Elements.Last.Value is NumberedList NumberedList)
							NumberedList.AddChildren(Items);
						else
							Elements.AddLast(new NumberedList(this, Items));

						continue;
					}
				}
				else if (Block.IsPrefixedBy(s2 = "[ ]", true) || Block.IsPrefixedBy(s2 = "[x]", true) || Block.IsPrefixedBy(s2 = "[X]", true))
				{
					LinkedList<Tuple<Block, string, int>> Segments = null;
					int CheckPosition = Block.Positions[0] + 1;
					string s3;
					i = 0;
					c = Block.End;

					for (d = Block.Start + 1; d <= c; d++)
					{
						s = Block.Rows[d];
						if (IsPrefixedBy(s, s3 = "[ ]", true) || IsPrefixedBy(s, s3 = "[x]", true) || IsPrefixedBy(s, s3 = "[X]", true))
						{
							if (Segments is null)
								Segments = new LinkedList<Tuple<Block, string, int>>();

							Segments.AddLast(new Tuple<Block, string, int>(new Block(Block.Rows, Block.Positions, 0, i, d - 1), s2, CheckPosition));
							s2 = s3;
							i = d;
							CheckPosition = Block.Positions[d] + 1;
						}
					}

					if (Segments != null)
						Segments.AddLast(new Tuple<Block, string, int>(new Block(Block.Rows, Block.Positions, 0, i, c), s2, CheckPosition));

					if (Segments is null)
					{
						LinkedList<MarkdownElement> Items = this.ParseBlocks(Block.RemovePrefix(s2, 4));

						i = BlockIndex;
						while (BlockIndex < EndBlock && (Block = Blocks[BlockIndex + 1]).Indent > 0)
						{
							BlockIndex++;
							Block.Indent--;
						}

						if (BlockIndex > i)
						{
							foreach (MarkdownElement E in this.ParseBlocks(Blocks, i + 1, BlockIndex))
								Items.AddLast(E);
						}

						if (Elements.Last != null && Elements.Last.Value is TaskList TaskList)
							TaskList.AddChildren(new TaskItem(this, s2 != "[ ]", CheckPosition, new NestedBlock(this, Items)));
						else
							Elements.AddLast(new TaskList(this, new TaskItem(this, s2 != "[ ]", CheckPosition, new NestedBlock(this, Items))));

						continue;
					}
					else
					{
						LinkedList<MarkdownElement> Items = new LinkedList<MarkdownElement>();

						foreach (Tuple<Block, string, int> Segment in Segments)
						{
							foreach (Block SegmentItem in Segment.Item1.RemovePrefix(Segment.Item2, 4))
							{
								Items.AddLast(new TaskItem(this, Segment.Item2 != "[ ]", Segment.Item3, new NestedBlock(this,
									this.ParseBlock(SegmentItem.Rows, SegmentItem.Positions, SegmentItem.Start, SegmentItem.End))));
							}
						}

						if (Elements.Last != null && Elements.Last.Value is TaskList TaskList)
							TaskList.AddChildren(Items);
						else
							Elements.AddLast(new TaskList(this, Items));

						continue;
					}
				}
				else if (Block.IsPrefixedByNumber(out Index))
				{
					LinkedList<KeyValuePair<int, Block>> Segments = null;
					i = 0;
					c = Block.End;

					for (d = Block.Start + 1; d <= c; d++)
					{
						s = Block.Rows[d];
						if (IsPrefixedByNumber(s, out j))
						{
							if (Segments is null)
								Segments = new LinkedList<KeyValuePair<int, Block>>();

							Segments.AddLast(new KeyValuePair<int, Block>(Index, new Block(Block.Rows, Block.Positions, 0, i, d - 1)));
							i = d;
							Index = j;
						}
					}

					if (Segments != null)
						Segments.AddLast(new KeyValuePair<int, Block>(Index, new Block(Block.Rows, Block.Positions, 0, i, c)));

					if (Segments is null)
					{
						s = Index.ToString();
						LinkedList<MarkdownElement> Items = this.ParseBlocks(Block.RemovePrefix(s + ".", Math.Max(4, s.Length + 2)));

						i = BlockIndex;
						while (BlockIndex < EndBlock && (Block = Blocks[BlockIndex + 1]).Indent > 0)
						{
							BlockIndex++;
							Block.Indent--;
						}

						if (BlockIndex > i)
						{
							foreach (MarkdownElement E in this.ParseBlocks(Blocks, i + 1, BlockIndex))
								Items.AddLast(E);
						}

						if (Elements.Last != null && Elements.Last.Value is NumberedList NumberedList)
							NumberedList.AddChildren(new NumberedItem(this, Index, new NestedBlock(this, Items)));
						else
							Elements.AddLast(new NumberedList(this, new NumberedItem(this, Index, new NestedBlock(this, Items))));

						continue;
					}
					else
					{
						LinkedList<MarkdownElement> Items = new LinkedList<MarkdownElement>();

						foreach (KeyValuePair<int, Block> Segment in Segments)
						{
							s = Segment.Key.ToString();
							foreach (Block SegmentItem in Segment.Value.RemovePrefix(s + ".", Math.Max(4, s.Length + 2)))
							{
								Items.AddLast(new NumberedItem(this, Segment.Key, new NestedBlock(this,
									this.ParseBlock(SegmentItem.Rows, SegmentItem.Positions, SegmentItem.Start, SegmentItem.End))));
							}
						}

						if (Elements.Last != null && Elements.Last.Value is NumberedList NumberedList)
							NumberedList.AddChildren(Items);
						else
							Elements.AddLast(new NumberedList(this, Items));

						continue;
					}
				}
				else if (Block.IsTable(out TableInformation TableInformation))
				{
					MarkdownElement[][] Headers = new MarkdownElement[TableInformation.NrHeaderRows][];
					MarkdownElement[][] DataRows = new MarkdownElement[TableInformation.NrDataRows][];
					LinkedList<MarkdownElement> CellElements;
					string[] Row;
					int[] Positions;

					c = TableInformation.Columns;

					for (j = 0; j < TableInformation.NrHeaderRows; j++)
					{
						Row = TableInformation.Headers[j];
						Positions = TableInformation.HeaderPositions[j];

						Headers[j] = new MarkdownElement[c];

						for (i = 0; i < c; i++)
						{
							s = Row[i];
							if (s is null)
								Headers[j][i] = null;
							else
							{
								CellElements = this.ParseBlock(new string[] { Row[i] }, new int[] { Positions[i] });

								if (CellElements.First != null && CellElements.First.Next is null)
									Headers[j][i] = CellElements.First.Value;
								else
									Headers[j][i] = new NestedBlock(this, CellElements);
							}
						}
					}

					for (j = 0; j < TableInformation.NrDataRows; j++)
					{
						Row = TableInformation.Rows[j];
						Positions = TableInformation.RowPositions[j];

						DataRows[j] = new MarkdownElement[c];

						for (i = 0; i < c; i++)
						{
							s = Row[i];
							if (s is null)
								DataRows[j][i] = null;
							else
							{
								CellElements = this.ParseBlock(new string[] { Row[i] }, new int[] { Positions[i] });

								if (CellElements.First != null && CellElements.First.Next is null)
									DataRows[j][i] = CellElements.First.Value;
								else
									DataRows[j][i] = new NestedBlock(this, CellElements);
							}
						}
					}

					Elements.AddLast(new Table(this, c, Headers, DataRows, TableInformation.Alignments, TableInformation.AlignmentDefinitions,
						TableInformation.Caption, TableInformation.Id));

					continue;
				}
				else if (Block.IsPrefixedBy(":", true) && Elements.Last != null)
				{
					LinkedList<MarkdownElement> Description = this.ParseBlocks(Block.RemovePrefix(":", 4));
					DefinitionDescriptions DefinitionDescriptions;

					i = BlockIndex;
					while (BlockIndex < EndBlock && (Block = Blocks[BlockIndex + 1]).Indent > 0)
					{
						BlockIndex++;
						Block.Indent--;
					}

					if (BlockIndex > i)
					{
						foreach (MarkdownElement E in this.ParseBlocks(Blocks, i + 1, BlockIndex))
							Description.AddLast(E);
					}

					if (Description.First is null)
						continue;

					if (Description.First.Next is null)
						DefinitionDescriptions = new DefinitionDescriptions(this, Description);
					else
						DefinitionDescriptions = new DefinitionDescriptions(this, new NestedBlock(this, Description));

					if (Elements.Last.Value is DefinitionDescriptions DefinitionDescriptions2)
						DefinitionDescriptions2.AddChildren(DefinitionDescriptions.Children);
					else if (Elements.Last.Value is DefinitionTerms DefinitionTerms)
						Elements.Last.Value = new DefinitionList(this, DefinitionTerms, DefinitionDescriptions);
					else if (Elements.Last.Value is DefinitionList DefinitionList)
						DefinitionList.AddChildren(DefinitionDescriptions);
					else
						Elements.AddLast(new DefinitionList(this, DefinitionDescriptions));

					continue;
				}
				else if (BlockIndex < EndBlock && Blocks[BlockIndex + 1].IsPrefixedBy(":", true))
				{
					LinkedList<MarkdownElement> Terms = new LinkedList<MarkdownElement>();
					LinkedList<MarkdownElement> Term;

					Rows = Block.Rows;
					c = Block.End;
					for (i = Block.Start; i <= c; i++)
					{
						Term = this.ParseBlock(Rows, Block.Positions, i, i);
						if (Term.First is null)
							continue;

						if (Term.First.Next is null)
							Terms.AddLast(Term.First.Value);
						else
							Terms.AddLast(new NestedBlock(this, Term));
					}

					if (Elements.Last != null && Elements.Last.Value is DefinitionList DefinitionList)
						DefinitionList.AddChildren(new DefinitionTerms(this, Terms));
					else
						Elements.AddLast(new DefinitionTerms(this, Terms));

					continue;
				}
				else if (Block.IsFootnote(out s))
				{
					Footnote Footnote = new Footnote(this, s, this.ParseBlocks(Blocks, BlockIndex, BlockIndex));

					i = BlockIndex;
					while (BlockIndex < EndBlock && (Block = Blocks[BlockIndex + 1]).Indent > 0)
					{
						BlockIndex++;
						Block.Indent--;
					}

					if (BlockIndex > i)
						Footnote.AddChildren(this.ParseBlocks(Blocks, i + 1, BlockIndex));

					if (this.footnoteNumbers is null)
					{
						this.footnoteNumbers = new Dictionary<string, int>();
						this.footnoteOrder = new List<string>();
						this.footnotes = new Dictionary<string, Footnote>();
					}

					this.footnotes[Footnote.Key] = Footnote;

					continue;
				}

				Rows = Block.Rows;
				c = Block.End;

				if (c >= 1)
				{
					s = Rows[c];

					if (this.IsUnderline(s, '=', false, false))
					{
						Header Header = new Header(this, 1, false, s, this.ParseBlock(Rows, Block.Positions, 0, c - 1));
						Elements.AddLast(Header);
						this.headers.Add(Header);
						continue;
					}
					else if (this.IsUnderline(s, '-', false, false))
					{
						Header Header = new Header(this, 2, false, s, this.ParseBlock(Rows, Block.Positions, 0, c - 1));
						Elements.AddLast(Header);
						this.headers.Add(Header);
						continue;
					}
				}

				s = Rows[Block.Start];
				if (this.IsPrefixedBy(s, '#', out d) && d < s.Length)
				{
					string Prefix = s.Substring(0, d);
					Rows[Block.Start] = s.Substring(d).Trim();

					s = Rows[c];
					i = s.Length - 1;
					while (i >= 0 && s[i] == '#')
						i--;

					if (++i < s.Length)
						Rows[c] = s.Substring(0, i).TrimEnd();

					Header Header = new Header(this, d, true, Prefix, this.ParseBlock(Rows, Block.Positions, Block.Start, c));
					Elements.AddLast(Header);
					this.headers.Add(Header);
					continue;
				}

				Content = this.ParseBlock(Rows, Block.Positions, Block.Start, c);
				if (Content.First != null)
				{
					if (Content.First.Value is InlineHTML && Content.Last.Value is InlineHTML)
						Elements.AddLast(new HtmlBlock(this, Content));
					else if (Content.First.Next is null && Content.First.Value.OutsideParagraph)
					{
						if (Content.First.Value is MarkdownElementChildren MarkdownElementChildren &&
							MarkdownElementChildren.JoinOverParagraphs && Elements.Last != null &&
							Elements.Last.Value is MarkdownElementChildren MarkdownElementChildrenLast)
						{
							MarkdownElementChildrenLast.AddChildren(MarkdownElementChildren.Children);
						}
						else
							Elements.AddLast(Content.First.Value);
					}
					else
						Elements.AddLast(new Paragraph(this, Content));
				}
			}

			if (HasSections)
			{
				LinkedList<MarkdownElement> Sections = new LinkedList<MarkdownElement>();
				Sections.AddLast(new Sections(this, InitialNrColumns, InitialSectionSeparator, Elements));
				return Sections;
			}
			else
				return Elements;
		}

		private LinkedList<MarkdownElement> ParseBlock(string[] Rows, int[] Positions)
		{
			return this.ParseBlock(Rows, Positions, 0, Rows.Length - 1);
		}

		private LinkedList<MarkdownElement> ParseBlock(string[] Rows, int[] Positions, int StartRow, int EndRow)
		{
			LinkedList<MarkdownElement> Elements = new LinkedList<MarkdownElement>();
			bool PreserveCrLf = Rows[StartRow].StartsWith("<") && Rows[EndRow].EndsWith(">");
			BlockParseState State = new BlockParseState(Rows, Positions, StartRow, EndRow, PreserveCrLf);

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
			char PrevChar = ' ';
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

					case '\r':
						Text.AppendLine();
						break;

					case '*':
						if (((ch2 = State.PeekNextCharSameRow()) <= ' ' && ch2 > 0) || ch2 == 160)
						{
							if (State.IsFirstCharOnLine)
							{
								this.AppendAnyText(Elements, Text);

								while (((ch2 = State.PeekNextCharSameRow()) <= ' ' && ch2 > 0) || ch2 == 160)
									State.NextCharSameRow();

								UnnumberedItem Item;
								List<string> Rows = new List<string>();
								List<int> Positions = new List<int>()
								{
									State.CurrentPosition
								};

								Rows.Add(State.RestOfRow());

								while (!State.EOF)
								{
									if ((ch2 = State.PeekNextCharSameRow()) == '*' || ch2 == '+' || ch2 == '-')
									{
										Item = new UnnumberedItem(this, ch + " ", new NestedBlock(this, this.ParseBlock(Rows.ToArray(), Positions.ToArray())));

										if (Elements.Last != null && Elements.Last.Value is BulletList BulletList)
											BulletList.AddChildren(Item);
										else
											Elements.AddLast(new BulletList(this, Item));

										State.NextCharSameRow();
										State.SkipWhitespaceSameRow(3);

										Rows.Clear();
										Positions.Clear();

										Positions.Add(State.CurrentPosition);
										Rows.Add(State.RestOfRow());
									}
									else
									{
										State.SkipWhitespaceSameRow(4);

										Positions.Add(State.CurrentPosition);
										Rows.Add(State.RestOfRow());
									}
								}

								if (Rows.Count > 0)
								{
									Item = new UnnumberedItem(this, ch + " ", new NestedBlock(this, this.ParseBlock(Rows.ToArray(), Positions.ToArray())));

									if (Elements.Last != null && Elements.Last.Value is BulletList BulletList)
										BulletList.AddChildren(Item);
									else
										Elements.AddLast(new BulletList(this, Item));
								}
							}
							else
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
							if (this.emojiSource is null)
								ch2 = (char)0;

							switch (ch2)
							{
								case ')':
									State.NextCharSameRow();
									Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_wink));
									break;

								case '-':
									State.BackupState();
									State.NextCharSameRow();

									if (State.PeekNextCharSameRow() == ')')
									{
										State.DiscardBackup();
										State.NextCharSameRow();
										Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_wink));
									}
									else
									{
										State.RestoreState();

										if (this.ParseBlock(State, '*', 1, ChildElements))
											Elements.AddLast(new Emphasize(this, ChildElements));
										else
											this.FixSyntaxError(Elements, "*", ChildElements);
									}
									break;

								case '\\':
									State.BackupState();
									State.NextCharSameRow();
									if ((ch3 = State.NextCharSameRow()) == '0' || ch3 == 'O')
									{
										if (State.NextCharSameRow() == '/')
										{
											if (State.NextCharSameRow() == '*')
											{
												State.DiscardBackup();
												this.AppendAnyText(Elements, Text);
												Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_ok_woman));
												break;
											}
										}
									}

									State.RestoreState();
									if (this.ParseBlock(State, '*', 1, ChildElements))
										Elements.AddLast(new Emphasize(this, ChildElements));
									else
										this.FixSyntaxError(Elements, "*", ChildElements);

									break;

								default:
									if (this.ParseBlock(State, '*', 1, ChildElements))
										Elements.AddLast(new Emphasize(this, ChildElements));
									else
										this.FixSyntaxError(Elements, "*", ChildElements);
									break;
							}
						}
						break;

					case '_':
						if ((ch2 = State.PeekNextCharSameRow()) <= ' ' || ch2 == 160)
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
						if ((ch2 = State.PeekNextCharSameRow()) <= ' ' || ch2 == 160)
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
						ch2 = State.PeekNextCharSameRow();
						if (ch2 == '`')
						{
							State.NextCharSameRow();

							while ((ch2 = State.NextChar()) != 0)
							{
								if (ch2 == '`' && State.PeekNextCharSameRow() == '`')
								{
									State.NextCharSameRow();
									break;
								}

								Text.Append(ch2);
							}
						}
						else
						{
							while ((ch2 = State.NextChar()) != '`' && ch2 != 0)
								Text.Append(ch2);
						}

						Elements.AddLast(new InlineCode(this, Text.ToString()));
						Text.Clear();
						break;

					case '[':
					case '!':
						FirstCharOnLine = State.IsFirstCharOnLine;

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
						else
						{
							ch2 = State.PeekNextCharSameRow();
							if (ch2 == '[')
							{
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.AddLast(new HtmlEntity(this, "LeftDoubleBracket"));
								break;
							}
							else if (ch2 == '%')
							{
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);

								while ((ch3 = State.NextCharSameRow()) != ']' && ch3 != 0)
									Text.Append(ch3);

								if (ch3 == ']')
								{
									Url = Text.ToString();
									if (string.Compare(Url, "Details", true) == 0)
										Elements.AddLast(new DetailsReference(this, Url));
									else
										Elements.AddLast(new MetaReference(this, Url));
									Text.Clear();
								}
								else
									Text.Insert(0, "[%");

								break;
							}
							else if (ch2 == '^')
							{
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);

								while ((ch3 = State.NextChar()) != ']' && ch3 != 0)
									Text.Append(ch3);

								if (ch3 == ']')
								{
									Url = Text.ToString();
									Text.Clear();

									if (this.footnoteNumbers is null)
									{
										this.footnoteNumbers = new Dictionary<string, int>();
										this.footnoteOrder = new List<string>();
										this.footnotes = new Dictionary<string, Footnote>();
									}

									try
									{
										Title = Url.ToLower();
										Elements.AddLast(new FootnoteReference(this, XmlConvert.VerifyNCName(Title)));
										if (!this.footnoteNumbers.ContainsKey(Title))
										{
											this.footnoteNumbers[Title] = ++this.lastFootnote;
											this.footnoteOrder.Add(Title);
										}
									}
									catch
									{
										Title = Guid.NewGuid().ToString();

										Elements.AddLast(new FootnoteReference(this, Title));
										this.footnoteNumbers[Title] = ++this.lastFootnote;
										this.footnoteOrder.Add(Title);
										this.footnotes[Title] = new Footnote(this, Title, new Paragraph(this, this.ParseBlock(new string[] { Url }, new int[] { State.CurrentPosition - 1 - Url.Length })));
									}
								}
								else
									Text.Insert(0, "[^");

								break;
							}
						}

						char[] chs;

						if (FirstCharOnLine && (((chs = State.PeekNextChars(3))[0] == ' ' || chs[0] == 'x' || chs[0] == 'X') && chs[1] == ']' && ((chs[2] <= ' ' && chs[2] > 0) || chs[2] == 160)))
						{
							int CheckPosition = State.CurrentPosition;

							State.NextChar();
							State.NextChar();
							State.NextChar();

							this.AppendAnyText(Elements, Text);

							while (((ch2 = State.PeekNextCharSameRow()) <= ' ' && ch2 > 0) || ch2 == 160)
								State.NextCharSameRow();

							TaskItem Item;
							List<string> Rows = new List<string>()
							{
								State.RestOfRow()
							};
							List<int> Positions = new List<int>()
							{
								State.CurrentPosition
							};
							bool Checked = (chs[0] != ' ');

							while (!State.EOF)
							{
								if ((chs = State.PeekNextChars(4))[0] == '[' &&
									(chs[1] == ' ' || chs[1] == 'x' || chs[1] == 'X') &&
									chs[2] == ']' && ((chs[3] <= ' ' && chs[3] > 0) || chs[3] == 160))
								{
									Item = new TaskItem(this, Checked, CheckPosition,
										new NestedBlock(this, this.ParseBlock(Rows.ToArray(), Positions.ToArray())));

									if (Elements.Last != null && Elements.Last.Value is TaskList TaskList)
										TaskList.AddChildren(Item);
									else
										Elements.AddLast(new TaskList(this, Item));

									Rows.Clear();
									Positions.Clear();

									State.NextChar();

									CheckPosition = State.CurrentPosition;

									State.NextChar();
									State.NextChar();
									State.NextChar();

									while (((ch2 = State.PeekNextCharSameRow()) <= ' ' && ch2 > 0) || ch2 == 160)
										State.NextCharSameRow();

									Positions.Add(State.CurrentPosition);
									Rows.Add(State.RestOfRow());

									Checked = (chs[1] != ' ');
								}
								else
								{
									State.SkipWhitespaceSameRow(4);
									Positions.Add(State.CurrentPosition);
									Rows.Add(State.RestOfRow());
								}
							}

							if (Rows.Count > 0)
							{
								Item = new TaskItem(this, Checked, CheckPosition,
									new NestedBlock(this, this.ParseBlock(Rows.ToArray(), Positions.ToArray())));

								if (Elements.Last != null && Elements.Last.Value is TaskList TaskList)
									TaskList.AddChildren(Item);
								else
									Elements.AddLast(new TaskList(this, Item));
							}

							break;
						}

						ChildElements = new LinkedList<MarkdownElement>();
						this.AppendAnyText(Elements, Text);

						if (this.ParseBlock(State, ']', 1, ChildElements))
						{
							ch2 = State.PeekNextNonWhitespaceChar();
							if (ch2 == '(')
							{
								State.NextNonWhitespaceChar();
								Title = string.Empty;

								while ((ch2 = State.NextCharSameRow()) != 0 && ch2 > ' ' && ch2 != ')' && ch2 != 160)
									Text.Append(ch2);

								Url = Text.ToString();
								Text.Clear();

								if (Url.StartsWith("<") && Url.EndsWith(">"))
									Url = Url.Substring(1, Url.Length - 2);

								if (ch2 <= ' ' || ch2 == 160)
								{
									ch2 = State.PeekNextNonWhitespaceChar();

									if (ch2 == '"' || ch2 == '\'')
									{
										State.NextNonWhitespaceChar();
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
								{
									List<MultimediaItem> Items = new List<MultimediaItem>()
									{
										new MultimediaItem(this, Url, Title, Width, Height)
									};

									if (!this.includesTableOfContents && string.Compare(Url, "ToC", true) == 0)
										this.includesTableOfContents = true;

									State.BackupState();
									ch2 = State.NextNonWhitespaceChar();
									while (ch2 == '(')
									{
										Title = string.Empty;

										while ((ch2 = State.NextCharSameRow()) != 0 && ch2 > ' ' && ch2 != ')' && ch2 != 160)
											Text.Append(ch2);

										Url = Text.ToString();
										Text.Clear();

										if (Url.StartsWith("<") && Url.EndsWith(">"))
											Url = Url.Substring(1, Url.Length - 2);

										if (ch2 <= ' ' || ch2 == 160)
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

										this.ParseWidthHeight(State, out Width, out Height);

										while (ch2 != 0 && ch2 != ')')
											ch2 = State.NextCharSameRow();

										Items.Add(new MultimediaItem(this, Url, Title, Width, Height));

										State.DiscardBackup();
										State.BackupState();
										ch2 = State.NextNonWhitespaceChar();
									}

									State.RestoreState();
									Elements.AddLast(new Multimedia(this, ChildElements, Elements.First is null && State.PeekNextChar() == 0,
										Items.ToArray()));
								}
								else
									Elements.AddLast(new Link(this, ChildElements, Url, Title));
							}
							else if (ch2 == ':' && FirstCharOnLine)
							{
								State.NextNonWhitespaceChar();
								ch2 = State.NextChar();
								while ((ch2 != 0 && ch2 <= ' ') || ch2 == 160)
									ch2 = State.NextChar();

								if (ch2 > ' ' && ch2 != 160)
								{
									List<MultimediaItem> Items = new List<MultimediaItem>();

									Text.Append(ch2);

									while (ch2 > ' ' && ch2 != 160 && ch2 != '[')
									{
										while ((ch2 = State.NextCharSameRow()) != 0 && ch2 > ' ' && ch2 != 160)
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

											Title = Text.ToString();
											Text.Clear();
										}
										else
											Title = string.Empty;

										this.ParseWidthHeight(State, out Width, out Height);

										Items.Add(new MultimediaItem(this, Url, Title, Width, Height));
										if (!this.includesTableOfContents && string.Compare(Url, "ToC", true) == 0)
											this.includesTableOfContents = true;

										ch2 = State.PeekNextNonWhitespaceChar();
									}

									foreach (MarkdownElement E in ChildElements)
										E.GeneratePlainText(Text);

									this.references[Text.ToString().ToLower()] = new Multimedia(this, null,
										Elements.First is null && State.PeekNextChar() == 0, Items.ToArray());

									Text.Clear();
								}
							}
							else if (ch2 == '[')
							{
								State.NextNonWhitespaceChar();
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
								{
									Elements.AddLast(new MultimediaReference(this, ChildElements, Title,
										Elements.First is null && State.PeekNextChar() == 0));
								}
								else
									Elements.AddLast(new LinkReference(this, ChildElements, Title));
							}
							else if (ch != '!')
								Elements.AddLast(new SubScript(this, ChildElements));
							else
							{
								this.FixSyntaxError(Elements, "![", ChildElements);
								Elements.AddLast(new InlineText(this, "]" + ch2));
							}
						}
						else
							this.FixSyntaxError(Elements, ch == '!' ? "![" : "[", ChildElements);
						break;

					case ']':
						ch2 = State.PeekNextCharSameRow();
						if (ch2 == ']')
						{
							State.NextCharSameRow();
							this.AppendAnyText(Elements, Text);
							Elements.AddLast(new HtmlEntity(this, "RightDoubleBracket"));
						}
						else
							Text.Append(']');
						break;

					case '<':
						ch2 = State.PeekNextCharSameRow();
						if (ch2 == '<')
						{
							State.NextCharSameRow();
							this.AppendAnyText(Elements, Text);

							ch3 = State.PeekNextCharSameRow();
							if (ch3 == '<')
							{
								State.NextCharSameRow();
								Elements.AddLast(new HtmlEntity(this, "Ll"));
							}
							else
								Elements.AddLast(new HtmlEntity(this, "laquo"));
							break;
						}
						else if (ch2 == '-')
						{
							State.NextCharSameRow();
							ch3 = State.PeekNextCharSameRow();

							if (ch3 == '-')
							{
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);

								ch3 = State.PeekNextCharSameRow();
								if (ch3 == '>')
								{
									State.NextCharSameRow();
									Elements.AddLast(new HtmlEntity(this, "harr"));
								}
								else
									Elements.AddLast(new HtmlEntity(this, "larr"));
							}
							else
								Text.Append("<-");
							break;
						}
						else if (ch2 == '=')
						{
							State.NextCharSameRow();
							this.AppendAnyText(Elements, Text);
							ch3 = State.PeekNextCharSameRow();

							if (ch3 == '=')
							{
								State.NextCharSameRow();

								ch3 = State.PeekNextCharSameRow();
								if (ch3 == '>')
								{
									State.NextCharSameRow();
									Elements.AddLast(new HtmlEntity(this, "hArr"));
								}
								else
									Elements.AddLast(new HtmlEntity(this, "lArr"));
							}
							else
								Elements.AddLast(new HtmlEntity(this, "leq"));
							break;
						}
						else if (ch2 == '>')
						{
							State.NextCharSameRow();
							this.AppendAnyText(Elements, Text);
							Elements.AddLast(new HtmlEntity(this, "ne"));
							break;
						}
						else if (ch2 == '3' && this.emojiSource != null)
						{
							State.NextCharSameRow();
							this.AppendAnyText(Elements, Text);
							Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_heart));
							break;
						}
						else if (ch2 == '/')
						{
							State.NextCharSameRow();
							if (this.emojiSource != null && State.PeekNextCharSameRow() == '3')
							{
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_broken_heart));
								break;
							}
						}

						if (!char.IsLetter(ch2) && ch2 != '/')
						{
							Text.Append(ch);
							break;
						}

						this.AppendAnyText(Elements, Text);
						Text.Append(ch);

						if (ch2 == '/')
							Text.Append(ch2);

						while ((ch2 = State.NextChar()) != 0 && ch2 != '>')
						{
							if (ch2 == '\r')
								Text.AppendLine();
							else
								Text.Append(ch2);
						}

						if (ch2 == 0)
							break;

						Text.Append(ch2);
						Url = Text.ToString();

						if (!this.allowScriptTag.HasValue)
						{
							this.allowScriptTag = this.metaData.TryGetValue("ALLOWSCRIPTTAG", out KeyValuePair<string, bool>[] Value) &&
								Value.Length > 0 &&
								CommonTypes.TryParse(Value[0].Key, out bool b) &&
								b;
						}

						if ((!this.settings.AllowScriptTag || !this.allowScriptTag.Value) &&
							(Url.StartsWith("<script", StringComparison.CurrentCultureIgnoreCase) ||
							Url.StartsWith("</script", StringComparison.CurrentCultureIgnoreCase)))
						{
							Elements.AddLast(new InlineCode(this, Url));
						}
						else if (Url.StartsWith("</") || Url.IndexOf(' ') >= 0)
							Elements.AddLast(new InlineHTML(this, Url));
						else if (Url.IndexOf(':') >= 0)
							Elements.AddLast(new AutomaticLinkUrl(this, Url.Substring(1, Url.Length - 2)));
						else if (Url.IndexOf('@') >= 0)
							Elements.AddLast(new AutomaticLinkMail(this, Url.Substring(1, Url.Length - 2)));
						else
							Elements.AddLast(new InlineHTML(this, Url));

						Text.Clear();

						break;

					case '>':
						switch (State.PeekNextCharSameRow())
						{
							case '>':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);

								ch3 = State.PeekNextCharSameRow();
								if (ch3 == '>')
								{
									State.NextCharSameRow();
									Elements.AddLast(new HtmlEntity(this, "Gg"));
								}
								else
									Elements.AddLast(new HtmlEntity(this, "raquo"));
								break;

							case '=':
								this.AppendAnyText(Elements, Text);
								State.NextCharSameRow();

								if (this.emojiSource != null && State.PeekNextCharSameRow() == ')')
								{
									State.NextCharSameRow();
									Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_laughing));
								}
								else
									Elements.AddLast(new HtmlEntity(this, "geq"));
								break;

							case ':':
								if (this.emojiSource != null)
								{
									State.NextCharSameRow();
									switch (State.PeekNextCharSameRow())
									{
										case ')':
											State.NextCharSameRow();
											this.AppendAnyText(Elements, Text);
											Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_laughing));
											break;

										case '(':
											State.NextCharSameRow();
											this.AppendAnyText(Elements, Text);
											Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_angry));
											break;

										case '[':
											State.NextCharSameRow();
											this.AppendAnyText(Elements, Text);
											Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_disappointed));
											break;

										case 'O':
											State.NextCharSameRow();
											this.AppendAnyText(Elements, Text);
											Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_open_mouth));
											break;

										case 'P':
										case 'p':
										case 'b':
										case 'Þ':
										case 'þ':
											State.NextCharSameRow();
											this.AppendAnyText(Elements, Text);
											Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_stuck_out_tongue_winking_eye));
											break;

										case '/':
										case '\\':
										case 'L':
											State.NextCharSameRow();
											this.AppendAnyText(Elements, Text);
											Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_confused));
											break;

										case 'X':
										case 'x':
										case '#':
											State.NextCharSameRow();
											this.AppendAnyText(Elements, Text);
											Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_open_mouth));
											break;

										case '-':
											State.NextCharSameRow();
											switch (State.PeekNextCharSameRow())
											{
												case ')':
													State.NextCharSameRow();
													this.AppendAnyText(Elements, Text);
													Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_laughing));
													break;

												case '(':
													State.NextCharSameRow();
													this.AppendAnyText(Elements, Text);
													Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_angry));
													break;

												default:
													Text.Append(">:-");
													break;
											}
											break;

										default:
											Text.Append(">:");
											break;
									}
								}
								else
									Text.Append('>');
								break;

							case ';':
								if (this.emojiSource != null)
								{
									State.NextCharSameRow();
									if (State.PeekNextCharSameRow() == ')')
									{
										State.NextCharSameRow();
										this.AppendAnyText(Elements, Text);
										Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_laughing));
									}
									else
										Text.Append(">;");
								}
								else
									Text.Append('>');
								break;

							case '.':
								if (this.emojiSource != null)
								{
									State.NextCharSameRow();
									if (State.PeekNextCharSameRow() == '<')
									{
										State.NextCharSameRow();
										this.AppendAnyText(Elements, Text);
										Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_persevere));
									}
									else
										Text.Append(">.");
								}
								else
									Text.Append('>');
								break;

							default:
								Text.Append('>');
								break;
						}
						break;

					case '{':
						if (this.settings.Variables is null)
						{
							int Pos = State.CurrentPosition - 1;
							if (Pos < this.markdownText.Length && this.markdownText[Pos] == '{')
							{
								if (this.toInsert is null)
									this.toInsert = new SortedDictionary<int, string>(new ReversePosition());

								this.toInsert[Pos] = "\\";
								if (Pos > 0 && ((ch2 = this.markdownText[Pos - 1]) == ':' || ch2 == '*' || ch2 == '='))
									this.toInsert[Pos - 1] = "\\";  // To avoid creating a smiley.
							}
							Text.Append(ch);
							break;
						}

						this.AppendAnyText(Elements, Text);
						State.BackupState();

						int StartPosition = State.CurrentPosition - 1;

						while ((ch2 = State.NextChar()) != '}' && ch2 != 0)
							Text.Append(ch2);

						int EndPosition = State.CurrentPosition;

						if (ch2 == 0)
						{
							State.RestoreState();
							Text.Clear();
							Text.Append(ch);
							break;
						}

						try
						{
							Expression Exp = new Expression(Text.ToString());
							State.DiscardBackup();
							Elements.AddLast(new InlineScript(this, Exp, this.settings.Variables,
								Elements.First is null && State.PeekNextChar() == 0, StartPosition, EndPosition));
							Text.Clear();
							this.isDynamic = true;
						}
						catch (Exception ex)
						{
							ex = Log.UnnestException(ex);

							if (ex is AggregateException ex2)
							{
								StringBuilder sb = new StringBuilder();

								foreach (Exception ex3 in ex2.InnerExceptions)
								{
									this.CheckException(ex3);

									Log.Critical(ex3);

									sb.Append("<p><font style=\"color:red\">");
									sb.Append(XML.HtmlValueEncode(ex3.Message));
									sb.Append("</font></p>");
								}

								string[] Rows = ex.Message.Replace("\r\n", "\n").Split(CommonTypes.CRLF);
								Elements.AddLast(new CodeBlock(this, Rows, 0, Rows.Length - 1, 0));
							}
							else
							{
								this.CheckException(ex);

								Log.Critical(ex);

								string[] Rows = ex.Message.Replace("\r\n", "\n").Split(CommonTypes.CRLF);
								Elements.AddLast(new CodeBlock(this, Rows, 0, Rows.Length - 1, 0));
							}

							this.CheckException(ex);
						}

						break;

					case '-':
						ch2 = State.PeekNextCharSameRow();
						if (ch2 == '-')
						{
							State.NextCharSameRow();
							this.AppendAnyText(Elements, Text);

							ch3 = State.PeekNextCharSameRow();

							if (ch3 == '>')
							{
								State.NextCharSameRow();
								Elements.AddLast(new HtmlEntity(this, "rarr"));
							}
							else if (ch3 == '-')
							{
								State.NextCharSameRow();
								Elements.AddLast(new HtmlEntity(this, "mdash"));
							}
							else
								Elements.AddLast(new HtmlEntity(this, "ndash"));
						}
						else if (ch2 == '+')
						{
							State.NextCharSameRow();
							this.AppendAnyText(Elements, Text);
							Elements.AddLast(new HtmlEntity(this, "MinusPlus"));
						}
						else if (ch2 == '_')
						{
							if (this.emojiSource != null)
							{
								State.BackupState();
								while ((ch2 = State.NextCharSameRow()) == '_')
									;

								if (ch2 == '-')
								{
									State.DiscardBackup();
									State.NextCharSameRow();
									this.AppendAnyText(Elements, Text);
									Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_expressionless));
								}
								else
								{
									State.RestoreState();
									Text.Append(ch);
								}
							}
							else
								Text.Append(ch);
						}
						else if ((ch2 <= ' ' && ch2 > 0) || ch2 == 160)
						{
							if (State.IsFirstCharOnLine)
							{
								this.AppendAnyText(Elements, Text);

								while (((ch2 = State.PeekNextCharSameRow()) <= ' ' && ch2 > 0) || ch2 == 160)
									State.NextCharSameRow();

								UnnumberedItem Item;
								List<string> Rows = new List<string>();
								List<int> Positions = new List<int>()
								{
									State.CurrentPosition
								};

								Rows.Add(State.RestOfRow());

								while (!State.EOF)
								{
									if ((ch2 = State.PeekNextCharSameRow()) == '*' || ch2 == '+' || ch2 == '-')
									{
										Item = new UnnumberedItem(this, ch + " ", new NestedBlock(this, this.ParseBlock(Rows.ToArray(), Positions.ToArray())));

										if (Elements.Last != null && Elements.Last.Value is BulletList BulletList)
											BulletList.AddChildren(Item);
										else
											Elements.AddLast(new BulletList(this, Item));

										State.NextCharSameRow();
										State.SkipWhitespaceSameRow(3);

										Rows.Clear();
										Positions.Clear();

										Positions.Add(State.CurrentPosition);
										Rows.Add(State.RestOfRow());
									}
									else
									{
										State.SkipWhitespaceSameRow(4);

										Positions.Add(State.CurrentPosition);
										Rows.Add(State.RestOfRow());
									}
								}

								if (Rows.Count > 0)
								{
									Item = new UnnumberedItem(this, ch + " ", new NestedBlock(this, this.ParseBlock(Rows.ToArray(), Positions.ToArray())));

									if (Elements.Last != null && Elements.Last.Value is BulletList BulletList)
										BulletList.AddChildren(Item);
									else
										Elements.AddLast(new BulletList(this, Item));
								}
							}
							else
								Text.Append('-');
						}
						else
							Text.Append('-');
						break;

					case '+':
						ch2 = State.PeekNextCharSameRow();
						if (ch2 == '-')
						{
							State.NextCharSameRow();
							this.AppendAnyText(Elements, Text);
							Elements.AddLast(new HtmlEntity(this, "PlusMinus"));
						}
						else if ((ch2 <= ' ' && ch2 > 0) || ch2 == 160)
						{
							if (State.IsFirstCharOnLine)
							{
								this.AppendAnyText(Elements, Text);

								while (((ch2 = State.PeekNextCharSameRow()) <= ' ' && ch2 > 0) || ch2 == 160)
									State.NextCharSameRow();

								UnnumberedItem Item;
								List<string> Rows = new List<string>();
								List<int> Positions = new List<int>()
								{
									State.CurrentPosition
								};

								Rows.Add(State.RestOfRow());

								while (!State.EOF)
								{
									if ((ch2 = State.PeekNextCharSameRow()) == '*' || ch2 == '+' || ch2 == '-')
									{
										Item = new UnnumberedItem(this, ch + " ", new NestedBlock(this, this.ParseBlock(Rows.ToArray(), Positions.ToArray())));

										if (Elements.Last != null && Elements.Last.Value is BulletList BulletList)
											BulletList.AddChildren(Item);
										else
											Elements.AddLast(new BulletList(this, Item));

										State.NextCharSameRow();
										State.SkipWhitespaceSameRow(3);

										Rows.Clear();
										Positions.Clear();

										Positions.Add(State.CurrentPosition);
										Rows.Add(State.RestOfRow());
									}
									else
									{
										State.SkipWhitespaceSameRow(4);

										Positions.Add(State.CurrentPosition);
										Rows.Add(State.RestOfRow());
									}
								}

								if (Rows.Count > 0)
								{
									Item = new UnnumberedItem(this, ch + " ", new NestedBlock(this, this.ParseBlock(Rows.ToArray(), Positions.ToArray())));

									if (Elements.Last != null && Elements.Last.Value is BulletList BulletList)
										BulletList.AddChildren(Item);
									else
										Elements.AddLast(new BulletList(this, Item));
								}
							}
							else
								Text.Append('+');
						}
						else
							Text.Append('+');
						break;

					case '#':
						if (State.IsFirstCharOnLine && State.PeekNextCharSameRow() == '.')
						{
							State.NextCharSameRow();

							if (((ch2 = State.PeekNextCharSameRow()) <= ' ' && ch2 > 0) || ch2 == 160)
							{
								this.AppendAnyText(Elements, Text);

								while (((ch2 = State.PeekNextCharSameRow()) <= ' ' && ch2 > 0) || ch2 == 160)
									State.NextCharSameRow();

								UnnumberedItem Item;
								List<string> Rows = new List<string>();
								List<int> Positions = new List<int>()
								{
									State.CurrentPosition
								};

								Rows.Add(State.RestOfRow());

								while (!State.EOF)
								{
									if (State.PeekNextCharSameRow() == '#')
									{
										State.NextCharSameRow();
										if (State.PeekNextCharSameRow() == '.')
										{
											Item = new UnnumberedItem(this, "#. ", new NestedBlock(this, this.ParseBlock(Rows.ToArray(), Positions.ToArray())));

											if (Elements.Last != null && Elements.Last.Value is NumberedList NumberedList)
												NumberedList.AddChildren(Item);
											else
												Elements.AddLast(new NumberedList(this, Item));

											State.NextCharSameRow();
											State.SkipWhitespaceSameRow(3);

											Rows.Clear();
											Positions.Clear();

											Positions.Add(State.CurrentPosition);
											Rows.Add(State.RestOfRow());
										}
										else
										{
											State.SkipWhitespaceSameRow(4);

											Positions.Add(State.CurrentPosition - 1);
											Rows.Add("#" + State.RestOfRow());
										}
									}
									else
									{
										State.SkipWhitespaceSameRow(4);

										Positions.Add(State.CurrentPosition);
										Rows.Add(State.RestOfRow());
									}
								}

								if (Rows.Count > 0)
								{
									Item = new UnnumberedItem(this, "#. ", new NestedBlock(this, this.ParseBlock(Rows.ToArray(), Positions.ToArray())));

									if (Elements.Last != null && Elements.Last.Value is NumberedList NumberedList)
										NumberedList.AddChildren(Item);
									else
										Elements.AddLast(new NumberedList(this, Item));
								}
							}
							else
								Text.Append("#.");
						}
						else if (this.emojiSource != null)
						{
							switch (State.PeekNextCharSameRow())
							{
								case '-':
									State.BackupState();
									State.NextCharSameRow();
									switch (State.PeekNextCharSameRow())
									{
										case ')':
											State.DiscardBackup();
											State.NextCharSameRow();
											this.AppendAnyText(Elements, Text);
											Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_dizzy_face));
											break;

										default:
											State.RestoreState();
											Text.Append(ch);
											break;
									}
									break;

								case ')':
									State.NextCharSameRow();
									this.AppendAnyText(Elements, Text);
									Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_dizzy_face));
									break;

								default:
									Text.Append('#');
									break;
							}
						}
						else
							Text.Append('#');
						break;

					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9':
						if (this.emojiSource != null && (ch == '8' || ch == '0') && (char.IsPunctuation(PrevChar) || char.IsWhiteSpace(PrevChar)))
						{
							if (ch == '0')
							{
								switch (ch2 = State.PeekNextCharSameRow())
								{
									case ':':
										State.BackupState();
										State.NextCharSameRow();
										switch (State.PeekNextCharSameRow())
										{
											case '3':
											case ')':
												State.DiscardBackup();
												State.NextCharSameRow();
												this.AppendAnyText(Elements, Text);
												Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_innocent));
												ch2 = (char)0xffff;
												break;

											case '-':
												State.NextCharSameRow();
												switch (State.PeekNextCharSameRow())
												{
													case '3':
													case ')':
														State.DiscardBackup();
														State.NextCharSameRow();
														this.AppendAnyText(Elements, Text);
														Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_innocent));
														ch2 = (char)0xffff;
														break;

													default:
														State.RestoreState();
														break;
												}
												break;

											default:
												State.RestoreState();
												break;
										}
										break;

									case ';':
										State.BackupState();
										State.NextCharSameRow();
										switch (State.PeekNextCharSameRow())
										{
											case '-':
											case '^':
												State.NextCharSameRow();
												switch (State.PeekNextCharSameRow())
												{
													case ')':
														State.DiscardBackup();
														State.NextCharSameRow();
														this.AppendAnyText(Elements, Text);
														Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_innocent));
														ch2 = (char)0xffff;
														break;

													default:
														State.RestoreState();
														break;
												}
												break;

											default:
												State.RestoreState();
												break;
										}
										break;

									default:
										break;
								}
							}
							else
							{
								switch (ch2 = State.PeekNextCharSameRow())
								{
									case '-':
										State.BackupState();
										State.NextCharSameRow();
										switch (State.PeekNextCharSameRow())
										{
											case ')':
											case 'D':
												State.DiscardBackup();
												State.NextCharSameRow();
												this.AppendAnyText(Elements, Text);
												Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_sunglasses));
												ch2 = (char)0xffff;
												break;

											default:
												State.RestoreState();
												break;
										}
										break;

									case ')':
										State.NextCharSameRow();
										this.AppendAnyText(Elements, Text);
										Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_sunglasses));
										ch2 = (char)0xffff;
										break;

									default:
										break;
								}
							}

							if (ch2 == (char)0xffff)
								break;
						}

						if (State.IsFirstCharOnLine)
						{
							StringBuilder sb = new StringBuilder();
							sb.Append(ch);

							while ((ch2 = State.PeekNextCharSameRow()) >= '0' && ch2 <= '9')
							{
								State.NextCharSameRow();
								sb.Append(ch2);
							}

							if (ch2 == '.')
							{
								State.NextCharSameRow();
								if ((((ch2 = State.PeekNextCharSameRow()) <= ' ' && ch2 > 0) || ch2 == 160) &&
									int.TryParse(sb.ToString(), out int Index))
								{
									this.AppendAnyText(Elements, Text);

									while (((ch2 = State.PeekNextCharSameRow()) <= ' ' && ch2 > 0) || ch2 == 160)
										State.NextCharSameRow();

									NumberedItem Item;
									List<string> Rows = new List<string>();
									List<int> Positions = new List<int>()
									{
										State.CurrentPosition
									};

									Rows.Add(State.RestOfRow());

									while (!State.EOF)
									{
										if ((ch2 = State.PeekNextCharSameRow()) >= '0' && ch2 <= '9')
										{
											sb.Clear();
											while ((ch2 = State.NextCharSameRow()) >= '0' && ch2 <= '9')
												sb.Append(ch2);

											if (ch2 == '.' && int.TryParse(sb.ToString(), out int Index2))
											{
												Item = new NumberedItem(this, Index, new NestedBlock(this, this.ParseBlock(Rows.ToArray(), Positions.ToArray())));

												if (Elements.Last != null && Elements.Last.Value is NumberedList NumberedList)
													NumberedList.AddChildren(Item);
												else
													Elements.AddLast(new NumberedList(this, Item));

												State.NextCharSameRow();
												State.SkipWhitespaceSameRow(3);

												Rows.Clear();
												Positions.Clear();

												Positions.Add(State.CurrentPosition);
												Rows.Add(State.RestOfRow());
												Index = Index2;
											}
											else
											{
												State.SkipWhitespaceSameRow(4);

												string s = sb.ToString();
												Positions.Add(State.CurrentPosition - 1 - s.Length);
												Rows.Add(s + ch2 + State.RestOfRow());
											}
										}
										else
										{
											State.SkipWhitespaceSameRow(4);

											Positions.Add(State.CurrentPosition);
											Rows.Add(State.RestOfRow());
										}
									}

									if (Rows.Count > 0)
									{
										Item = new NumberedItem(this, Index, new NestedBlock(this, this.ParseBlock(Rows.ToArray(), Positions.ToArray())));

										if (Elements.Last != null && Elements.Last.Value is NumberedList NumberedList)
											NumberedList.AddChildren(Item);
										else
											Elements.AddLast(new NumberedList(this, Item));
									}
								}
								else
								{
									Text.Append(sb.ToString());
									Text.Append('.');
								}
							}
							else
								Text.Append(sb.ToString());
						}
						else
							Text.Append(ch);
						break;

					case '=':
						ch2 = State.PeekNextCharSameRow();
						if (this.emojiSource is null && ch2 != '=')
							ch2 = (char)0;

						switch (ch2)
						{
							case '=':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);

								ch3 = State.NextCharSameRow();
								if (ch3 == '>')
								{
									State.NextCharSameRow();
									Elements.AddLast(new HtmlEntity(this, "rArr"));
								}
								else
									Elements.AddLast(new HtmlEntity(this, "equiv"));
								break;

							case 'D':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_smiley));
								break;

							case ')':
							case ']':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_smile));
								break;

							case '*':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_kissing_heart));
								break;

							case '(':
							case '[':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_disappointed));
								break;

							case '$':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_flushed));
								break;

							case '/':
							case '\\':
							case 'L':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_confused));
								break;

							case 'P':
							case 'p':
							case 'b':
							case 'Þ':
							case 'þ':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_stuck_out_tongue));
								break;

							case 'X':
							case 'x':
							case '#':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_no_mouth));
								break;

							default:
								Text.Append('=');
								break;
						}
						break;

					case '&':
						if (char.IsLetter(ch2 = State.PeekNextCharSameRow()))
						{
							this.AppendAnyText(Elements, Text);

							Text.Append('&');
							while (char.IsLetter(ch2 = State.NextCharSameRow()))
								Text.Append(ch2);

							if (ch2 != 0)
								Text.Append(ch2);

							if (ch2 != ';')
								break;

							Url = Text.ToString();
							Text.Clear();

							Elements.AddLast(new HtmlEntity(this, Url.Substring(1, Url.Length - 2)));
						}
						else if (ch2 == '#')
						{
							int Code;

							this.AppendAnyText(Elements, Text);
							State.NextCharSameRow();

							Text.Append("&#");

							if ((ch3 = State.PeekNextCharSameRow()) == 'x' || ch3 == 'X')
							{
								Text.Append(ch3);
								State.NextCharSameRow();

								while (((ch3 = char.ToUpper(State.PeekNextCharSameRow())) >= '0' && ch3 <= '9') || (ch3 >= 'A' && ch3 <= 'F'))
								{
									State.NextCharSameRow();
									Text.Append(ch3);
								}

								if (ch3 == ';' && int.TryParse(Text.ToString().Substring(3), System.Globalization.NumberStyles.HexNumber, null, out Code))
								{
									State.NextCharSameRow();
									Text.Clear();

									Elements.AddLast(new HtmlEntityUnicode(this, Code));
								}
							}
							else if (char.IsDigit(ch3))
							{
								while (char.IsDigit(ch3 = State.PeekNextCharSameRow()))
								{
									State.NextCharSameRow();
									Text.Append(ch3);
								}

								if (ch3 == ';' && int.TryParse(Text.ToString().Substring(2), out Code))
								{
									State.NextCharSameRow();
									Text.Clear();

									Elements.AddLast(new HtmlEntityUnicode(this, Code));
								}
							}
						}
						else
							Text.Append(ch);

						break;

					case '"':
						this.AppendAnyText(Elements, Text);
						if (this.IsLeftQuote(PrevChar, State.PeekNextCharSameRow()))
							Elements.AddLast(new HtmlEntity(this, "ldquo"));
						else
							Elements.AddLast(new HtmlEntity(this, "rdquo"));
						break;

					case '\'':
						this.AppendAnyText(Elements, Text);

						if (this.emojiSource != null)
							ch2 = State.PeekNextCharSameRow();
						else
							ch2 = (char)0;

						switch (ch2)
						{
							case ':':
								State.NextCharSameRow();
								switch (State.PeekNextCharSameRow())
								{
									case ')':
									case 'D':
										State.NextCharSameRow();
										Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_sweat_smile));
										break;

									case '(':
										State.NextCharSameRow();
										Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_sweat));
										break;

									case '-':
										State.NextCharSameRow();
										switch (State.PeekNextCharSameRow())
										{
											case ')':
											case 'D':
												State.NextCharSameRow();
												Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_sweat_smile));
												break;

											case '(':
												State.NextCharSameRow();
												Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_sweat));
												break;

											default:
												if (this.IsLeftQuote(PrevChar, State.PeekNextCharSameRow()))
													Elements.AddLast(new HtmlEntity(this, "lsquo"));
												else
													Elements.AddLast(new HtmlEntity(this, "rsquo"));

												Text.Append(":-");
												break;
										}
										break;

									default:
										if (this.IsLeftQuote(PrevChar, State.PeekNextCharSameRow()))
											Elements.AddLast(new HtmlEntity(this, "lsquo"));
										else
											Elements.AddLast(new HtmlEntity(this, "rsquo"));

										Text.Append(':');
										break;
								}
								break;

							case '=':
								State.NextCharSameRow();
								switch (State.PeekNextCharSameRow())
								{
									case ')':
									case 'D':
										State.NextCharSameRow();
										Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_sweat_smile));
										break;

									case '(':
										State.NextCharSameRow();
										Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_sweat));
										break;

									default:
										if (this.IsLeftQuote(PrevChar, State.PeekNextCharSameRow()))
											Elements.AddLast(new HtmlEntity(this, "lsquo"));
										else
											Elements.AddLast(new HtmlEntity(this, "rsquo"));

										Text.Append('=');
										break;
								}
								break;

							default:
								if (this.IsLeftQuote(PrevChar, State.PeekNextCharSameRow()))
									Elements.AddLast(new HtmlEntity(this, "lsquo"));
								else
									Elements.AddLast(new HtmlEntity(this, "rsquo"));
								break;
						}
						break;

					case '.':
						if (State.PeekNextCharSameRow() == '.')
						{
							State.NextCharSameRow();
							if (State.PeekNextCharSameRow() == '.')
							{
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);

								Elements.AddLast(new HtmlEntity(this, "hellip"));
							}
							else
								Text.Append("..");
						}
						else
							Text.Append('.');
						break;

					case '(':
						ch2 = State.PeekNextCharSameRow();
						ch3 = char.ToLower(ch2);
						if (ch3 == 'c' || ch3 == 'r' || ch3 == 'p' || ch3 == 's')
						{
							State.NextCharSameRow();
							if (State.PeekNextCharSameRow() == ')')
							{
								State.NextCharSameRow();

								this.AppendAnyText(Elements, Text);
								switch (ch2)
								{
									case 'c':
										Url = "copy";
										break;

									case 'C':
										Url = "COPY";
										break;

									case 'r':
										Url = "reg";
										break;

									case 'R':
										Url = "REG";
										break;

									case 'p':
										Url = "copysr";
										break;

									case 'P':
										Url = "copysr";
										break;

									case 's':
										Url = "oS";
										break;

									case 'S':
										Url = "circledS";
										break;

									default:
										Url = null;
										break;
								}

								Elements.AddLast(new HtmlEntity(this, Url));
							}
							else
							{
								Text.Append('(');
								Text.Append(ch2);
							}
						}
						else
							Text.Append('(');
						break;

					case '%':
						switch (State.PeekNextCharSameRow())
						{
							case '0':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);

								ch3 = State.PeekNextCharSameRow();
								if (ch3 == '0')
								{
									State.NextCharSameRow();
									Elements.AddLast(new HtmlEntity(this, "pertenk"));
								}
								else
									Elements.AddLast(new HtmlEntity(this, "permil"));
								break;

							case '-':
								if (this.emojiSource != null)
								{
									State.BackupState();
									State.NextCharSameRow();
									switch (State.PeekNextCharSameRow())
									{
										case ')':
											State.DiscardBackup();
											State.NextCharSameRow();
											this.AppendAnyText(Elements, Text);
											Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_dizzy_face));
											break;

										default:
											State.RestoreState();
											Text.Append(ch);
											break;
									}
								}
								else
									Text.Append('%');
								break;

							case ')':
								if (this.emojiSource != null)
								{
									State.NextCharSameRow();
									this.AppendAnyText(Elements, Text);
									Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_dizzy_face));
								}
								else
									Text.Append('%');
								break;

							default:
								Text.Append('%');
								break;
						}
						break;

					case '^':
						ch2 = State.PeekNextCharSameRow();
						switch (ch2)
						{
							case 'a':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.AddLast(new HtmlEntity(this, "ordf"));
								break;

							case 'o':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.AddLast(new HtmlEntity(this, "ordm"));
								break;

							case '0':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.AddLast(new HtmlEntity(this, "deg"));
								break;

							case '1':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.AddLast(new HtmlEntityUnicode(this, 185));
								break;

							case '2':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.AddLast(new HtmlEntityUnicode(this, 178));
								break;

							case '3':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.AddLast(new HtmlEntityUnicode(this, 179));
								break;

							case '4':
							case '5':
							case '6':
							case '7':
							case '8':
							case '9':
							case 'b':
							case 'c':
							case 'd':
							case 'e':
							case 'f':
							case 'g':
							case 'h':
							case 'i':
							case 'j':
							case 'k':
							case 'l':
							case 'm':
							case 'p':
							case 'q':
							case 'u':
							case 'v':
							case 'w':
							case 'x':
							case 'y':
							case 'z':
							case 'A':
							case 'B':
							case 'C':
							case 'D':
							case 'E':
							case 'F':
							case 'G':
							case 'H':
							case 'I':
							case 'J':
							case 'K':
							case 'L':
							case 'M':
							case 'N':
							case 'O':
							case 'P':
							case 'Q':
							case 'R':
							case 'S':
							case 'U':
							case 'V':
							case 'W':
							case 'X':
							case 'Y':
							case 'Z':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.AddLast(new SuperScript(this, new string(ch2, 1)));
								break;

							case 'T':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);

								if (State.PeekNextCharSameRow() == 'M')
								{
									State.NextCharSameRow();
									Elements.AddLast(new HtmlEntity(this, "trade"));
								}
								else
									Elements.AddLast(new SuperScript(this, "T"));
								break;

							case 's':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);

								if (State.PeekNextCharSameRow() == 't')
								{
									State.NextCharSameRow();
									Elements.AddLast(new SuperScript(this, "st"));
								}
								else
									Elements.AddLast(new SuperScript(this, "s"));
								break;

							case 'n':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);

								if (State.PeekNextCharSameRow() == 'd')
								{
									State.NextCharSameRow();
									Elements.AddLast(new SuperScript(this, "nd"));
								}
								else
									Elements.AddLast(new SuperScript(this, "n"));
								break;

							case 'r':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);

								if (State.PeekNextCharSameRow() == 'd')
								{
									State.NextCharSameRow();
									Elements.AddLast(new SuperScript(this, "rd"));
								}
								else
									Elements.AddLast(new SuperScript(this, "r"));
								break;

							case 't':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);

								if (State.PeekNextCharSameRow() == 'h')
								{
									State.NextCharSameRow();
									Elements.AddLast(new SuperScript(this, "th"));
								}
								else
									Elements.AddLast(new SuperScript(this, "t"));
								break;

							case '(':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);

								ChildElements = new LinkedList<MarkdownElement>();

								if (this.ParseBlock(State, ')', 1, ChildElements))
									Elements.AddLast(new SuperScript(this, ChildElements));
								else
									this.FixSyntaxError(Elements, "^(", ChildElements);
								break;

							case '[':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);

								ChildElements = new LinkedList<MarkdownElement>();

								if (this.ParseBlock(State, ']', 1, ChildElements))
									Elements.AddLast(new SuperScript(this, ChildElements));
								else
									this.FixSyntaxError(Elements, "^]", ChildElements);
								break;

							default:
								Text.Append('^');
								break;
						}
						break;

					case ':':
						if ((ch2 = State.PeekNextCharSameRow()) <= ' ' || ch2 == 160)
						{
							if (State.IsFirstCharOnLine && ch2 > 0)
							{
								LinkedList<MarkdownElement> TotItem = null;
								LinkedList<MarkdownElement> Item;
								DefinitionList DefinitionList = new DefinitionList(this);
								int i;

								for (i = State.Start; i < State.Current; i++)
								{
									Item = this.ParseBlock(State.Rows, State.Positions, i, i);
									if (Item.First is null)
										continue;

									if (TotItem is null)
									{
										if (Item.First.Next is null)
											TotItem = Item;
										else
											TotItem.AddLast(Item.First.Value);
									}
									else
									{
										if (TotItem is null)
											TotItem = new LinkedList<MarkdownElement>();

										TotItem.AddLast(new NestedBlock(this, Item));
									}
								}

								DefinitionList.AddChildren(new DefinitionTerms(this, TotItem));

								Text.Clear();
								Elements.Clear();
								Elements.AddLast(DefinitionList);

								while (((ch2 = State.PeekNextCharSameRow()) <= ' ' && ch2 > 0) || ch2 == 160)
									State.NextCharSameRow();

								List<string> Rows = new List<string>();
								List<int> Positions = new List<int>()
								{
									State.CurrentPosition
								};

								Rows.Add(State.RestOfRow());

								while (!State.EOF)
								{
									if (State.PeekNextCharSameRow() == ':')
									{
										DefinitionList.AddChildren(new DefinitionDescriptions(this, new NestedBlock(this, this.ParseBlock(Rows.ToArray(), Positions.ToArray()))));

										State.NextCharSameRow();
										State.SkipWhitespaceSameRow(3);

										Rows.Clear();
										Positions.Clear();

										Positions.Add(State.CurrentPosition);
										Rows.Add(State.RestOfRow());
									}
									else
									{
										State.SkipWhitespaceSameRow(4);

										Positions.Add(State.CurrentPosition);
										Rows.Add(State.RestOfRow());
									}
								}

								if (Rows.Count > 0)
									DefinitionList.AddChildren(new DefinitionDescriptions(this, new NestedBlock(this, this.ParseBlock(Rows.ToArray(), Positions.ToArray()))));
							}
							else
								Text.Append(ch);
						}
						else if (this.emojiSource != null)
						{
							int LeftLevel = 1;
							while (ch2 == ':')
							{
								LeftLevel++;
								State.NextCharSameRow();
								ch2 = State.PeekNextCharSameRow();
							}

							if (char.IsLetter(ch2) || char.IsDigit(ch2) || ch2 == '+')
							{
								this.AppendAnyText(Elements, Text);
								State.NextCharSameRow();

								ch3 = State.PeekNextCharSameRow();
								if (char.IsLetter(ch3) || char.IsDigit(ch3) || ch3 == '_' || ch3 == '-' || ch3 == ':')
									Text.Append(ch2);
								else
								{
									switch (ch2)
									{
										case 'D':
											State.NextCharSameRow();
											if (LeftLevel > 1)
												Text.Append(new string(':', LeftLevel - 1));

											this.AppendAnyText(Elements, Text);
											Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_smiley));
											break;

										case 'L':
											State.NextCharSameRow();
											if (LeftLevel > 1)
												Text.Append(new string(':', LeftLevel - 1));

											this.AppendAnyText(Elements, Text);
											Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_confused));
											break;

										case 'P':
										case 'p':
										case 'b':
										case 'Þ':
										case 'þ':
											State.NextCharSameRow();
											if (LeftLevel > 1)
												Text.Append(new string(':', LeftLevel - 1));

											this.AppendAnyText(Elements, Text);
											Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_stuck_out_tongue));
											break;

										case 'O':
										case 'o':
											State.NextCharSameRow();
											if (LeftLevel > 1)
												Text.Append(new string(':', LeftLevel - 1));

											this.AppendAnyText(Elements, Text);
											Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_open_mouth));
											break;

										case 'X':
										case 'x':
											State.NextCharSameRow();
											if (LeftLevel > 1)
												Text.Append(new string(':', LeftLevel - 1));

											this.AppendAnyText(Elements, Text);
											Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_no_mouth));
											break;

										default:
											Text.Append(ch2);
											ch2 = (char)0;
											break;
									}

									if (ch2 != 0)
										break;
								}

								while (char.IsLetter(ch3 = State.PeekNextCharSameRow()) || char.IsDigit(ch3) || ch3 == '_' || ch3 == '-')
								{
									State.NextCharSameRow();
									Text.Append(ch3);
								}

								if (ch3 == ':')
								{
									int RightLevel = 0;

									while (ch3 == ':' && RightLevel < LeftLevel)
									{
										RightLevel++;
										State.NextCharSameRow();
										ch3 = State.PeekNextCharSameRow();
									}

									Title = Text.ToString().ToLower();

									if (EmojiUtilities.TryGetEmoji(Title, out EmojiInfo Emoji))
									{
										if (LeftLevel > RightLevel)
											Elements.AddLast(new InlineText(this, new string(':', LeftLevel - RightLevel)));

										Elements.AddLast(new EmojiReference(this, Emoji, RightLevel));
										Text.Clear();
									}
									else
									{
										Text.Insert(0, new string(':', LeftLevel));
										Text.Append(new string(':', RightLevel));
									}
								}
								else
									Text.Insert(0, new string(':', LeftLevel));
							}
							else
							{
								if (LeftLevel > 1)
									Text.Append(new string(':', LeftLevel - 1));

								switch (ch2)
								{
									case '\'':
										State.NextCharSameRow();

										switch (State.PeekNextCharSameRow())
										{
											case ')':
												State.NextCharSameRow();
												this.AppendAnyText(Elements, Text);
												Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_joy));
												break;

											case '(':
												State.NextCharSameRow();
												this.AppendAnyText(Elements, Text);
												Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_cry));
												break;

											case '-':
												State.NextCharSameRow();
												if ((ch3 = State.PeekNextCharSameRow()) == ')')
												{
													State.NextCharSameRow();
													this.AppendAnyText(Elements, Text);
													Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_joy));
												}
												else if (ch3 == '(')
												{
													State.NextCharSameRow();
													this.AppendAnyText(Elements, Text);
													Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_cry));
												}
												else
													Text.Append(":'-");
												break;

											default:
												Text.Append(":'");
												break;
										}
										break;

									case '-':
										State.NextCharSameRow();

										switch (State.PeekNextCharSameRow())
										{
											case ')':
											case ']':
												State.NextCharSameRow();
												this.AppendAnyText(Elements, Text);
												Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_smile));
												break;

											case '(':
											case '[':
												State.NextCharSameRow();
												this.AppendAnyText(Elements, Text);
												Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_disappointed));
												break;

											case 'D':
												State.NextCharSameRow();
												this.AppendAnyText(Elements, Text);
												Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_smiley));
												break;

											case '*':
												State.NextCharSameRow();
												this.AppendAnyText(Elements, Text);
												Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_kissing_heart));
												break;

											case '/':
											case '.':
											case '\\':
											case 'L':
												State.NextCharSameRow();
												this.AppendAnyText(Elements, Text);
												Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_confused));
												break;

											case 'P':
											case 'p':
											case 'b':
											case 'Þ':
											case 'þ':
												State.NextCharSameRow();
												this.AppendAnyText(Elements, Text);
												Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_stuck_out_tongue));
												break;

											case 'O':
											case 'o':
												State.NextCharSameRow();
												this.AppendAnyText(Elements, Text);
												Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_open_mouth));
												break;

											case 'X':
											case 'x':
											case '#':
												State.NextCharSameRow();
												this.AppendAnyText(Elements, Text);
												Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_no_mouth));
												break;

											case '-':
												State.NextCharSameRow();
												if ((ch3 = State.PeekNextCharSameRow()) == ')')
												{
													State.NextCharSameRow();
													this.AppendAnyText(Elements, Text);
													Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_joy));
												}
												else if (ch3 == '(')
												{
													State.NextCharSameRow();
													this.AppendAnyText(Elements, Text);
													Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_cry));
												}
												else
													Text.Append(":--");
												break;

											case '1':
												State.NextCharSameRow();
												if (State.PeekNextCharSameRow() == ':')
												{
													State.NextCharSameRow();
													this.AppendAnyText(Elements, Text);
													Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji__1));
												}
												else
													Text.Append(":-1");
												break;

											default:
												Text.Append(":-");
												break;
										}
										break;

									case ')':
									case ']':
										State.NextCharSameRow();
										this.AppendAnyText(Elements, Text);
										Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_smile));
										break;

									case '(':
									case '[':
										State.NextCharSameRow();
										this.AppendAnyText(Elements, Text);
										Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_disappointed));
										break;

									case '*':
										State.NextCharSameRow();
										this.AppendAnyText(Elements, Text);
										Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_kissing_heart));
										break;

									case '/':
									case '\\':
										State.NextCharSameRow();
										this.AppendAnyText(Elements, Text);
										Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_confused));
										break;

									case '#':
										State.NextCharSameRow();
										this.AppendAnyText(Elements, Text);
										Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_no_mouth));
										break;

									case '@':
										State.NextCharSameRow();
										this.AppendAnyText(Elements, Text);
										Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_angry));
										break;

									case '$':
										State.NextCharSameRow();
										this.AppendAnyText(Elements, Text);
										Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_flushed));
										break;

									case '^':
										State.NextCharSameRow();
										if (State.PeekNextCharSameRow() == '*')
										{
											State.NextCharSameRow();
											this.AppendAnyText(Elements, Text);
											Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_kissing_heart));
										}
										else
											Text.Append(":^");
										break;

									default:
										Text.Append(ch);
										break;
								}
							}
						}
						else
							Text.Append(ch);
						break;

					case ';':
						if (this.emojiSource != null)
						{
							switch (State.PeekNextCharSameRow())
							{
								case ')':
								case ']':
								case 'D':
									State.NextCharSameRow();
									this.AppendAnyText(Elements, Text);
									Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_wink));
									break;

								case '(':
								case '[':
									State.NextCharSameRow();
									this.AppendAnyText(Elements, Text);
									Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_cry));
									break;

								case '-':
									State.NextCharSameRow();
									switch (State.PeekNextCharSameRow())
									{
										case ')':
										case ']':
											State.NextCharSameRow();
											this.AppendAnyText(Elements, Text);
											Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_wink));
											break;

										case '(':
										case '[':
											State.NextCharSameRow();
											this.AppendAnyText(Elements, Text);
											Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_cry));
											break;

										default:
											Text.Append(";-");
											break;
									}
									break;

								case '^':
									State.NextCharSameRow();
									if (State.PeekNextCharSameRow() == ')')
									{
										State.NextCharSameRow();
										this.AppendAnyText(Elements, Text);
										Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_wink));
									}
									else
										Text.Append(";^");
									break;

								default:
									Text.Append(';');
									break;
							}
						}
						else
							Text.Append(ch);
						break;

					case 'X':
					case 'x':
						if (this.emojiSource != null && (char.IsPunctuation(PrevChar) || char.IsWhiteSpace(PrevChar)))
						{
							switch (State.PeekNextCharSameRow())
							{
								case '-':
									State.BackupState();
									State.NextCharSameRow();
									switch (State.PeekNextCharSameRow())
									{
										case 'P':
										case 'p':
										case 'b':
										case 'Þ':
										case 'þ':
											State.DiscardBackup();
											State.NextCharSameRow();
											this.AppendAnyText(Elements, Text);
											Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_stuck_out_tongue_winking_eye));
											break;

										case ')':
											State.DiscardBackup();
											State.NextCharSameRow();
											this.AppendAnyText(Elements, Text);
											Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_dizzy_face));
											break;

										default:
											State.RestoreState();
											Text.Append(ch);
											break;
									}
									break;

								case ')':
									State.NextCharSameRow();
									this.AppendAnyText(Elements, Text);
									Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_dizzy_face));
									break;

								default:
									Text.Append(ch);
									break;
							}
						}
						else
							Text.Append(ch);
						break;

					case 'B':
						if (this.emojiSource != null && (char.IsPunctuation(PrevChar) || char.IsWhiteSpace(PrevChar)))
						{
							switch (State.PeekNextCharSameRow())
							{
								case '-':
									State.BackupState();
									State.NextCharSameRow();
									switch (State.PeekNextCharSameRow())
									{
										case ')':
										case 'D':
											State.DiscardBackup();
											State.NextCharSameRow();
											this.AppendAnyText(Elements, Text);
											Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_sunglasses));
											break;

										default:
											State.RestoreState();
											Text.Append(ch);
											break;
									}
									break;

								case ')':
									State.NextCharSameRow();
									this.AppendAnyText(Elements, Text);
									Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_sunglasses));
									break;

								default:
									Text.Append(ch);
									break;
							}
						}
						else
							Text.Append(ch);
						break;

					case 'd':
						if (this.emojiSource != null && (char.IsPunctuation(PrevChar) || char.IsWhiteSpace(PrevChar)) && State.PeekNextCharSameRow() == ':')
						{
							State.NextCharSameRow();
							this.AppendAnyText(Elements, Text);
							Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_stuck_out_tongue));
						}
						else
							Text.Append(ch);
						break;

					case 'O':
						if (this.emojiSource != null && (char.IsPunctuation(PrevChar) || char.IsWhiteSpace(PrevChar)))
						{
							switch (State.PeekNextCharSameRow())
							{
								case ':':
									State.BackupState();
									State.NextCharSameRow();
									switch (State.NextCharSameRow())
									{
										case ')':
										case '3':
											State.DiscardBackup();
											this.AppendAnyText(Elements, Text);
											Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_innocent));
											break;

										case '-':
											if ((ch3 = State.NextCharSameRow()) == ')' || ch3 == '3')
											{
												State.DiscardBackup();
												this.AppendAnyText(Elements, Text);
												Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_innocent));
											}
											else
											{
												State.RestoreState();
												Text.Append(ch);
											}
											break;

										default:
											State.RestoreState();
											Text.Append(ch);
											break;
									}
									break;

								case ';':
									State.BackupState();
									State.NextCharSameRow();
									if (State.NextCharSameRow() == '-')
									{
										if (State.NextCharSameRow() == ')')
										{
											State.DiscardBackup();
											this.AppendAnyText(Elements, Text);
											Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_innocent));
											break;
										}
									}

									State.RestoreState();
									Text.Append(ch);
									break;

								case '=':
									State.BackupState();
									State.NextCharSameRow();
									if (State.PeekNextCharSameRow() == ')')
									{
										State.NextCharSameRow();
										State.DiscardBackup();
										this.AppendAnyText(Elements, Text);
										Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_innocent));
									}
									else
									{
										State.RestoreState();
										Text.Append(ch);
									}
									break;

								case '_':
									State.BackupState();
									State.NextCharSameRow();
									if (State.PeekNextCharSameRow() == 'O')
									{
										State.NextCharSameRow();
										State.DiscardBackup();
										this.AppendAnyText(Elements, Text);
										Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_open_mouth));
									}
									else
									{
										State.RestoreState();
										Text.Append(ch);
									}
									break;

								default:
									Text.Append(ch);
									break;
							}
						}
						else
							Text.Append(ch);
						break;

					case 'h':
						if (char.IsWhiteSpace(PrevChar) && State.PeekNextCharSameRow() == 't')
						{
							FirstCharOnLine = State.IsFirstCharOnLine;

							chs = State.PeekNextChars(7);
							if (chs[1] == 't' && chs[2] == 'p' &&
								((chs[3] == ':' && chs[4] == '/' && chs[5] == '/') ||
								(chs[3] == 's' && chs[4] == ':' && chs[5] == '/' && chs[6] == '/')))
							{
								this.AppendAnyText(Elements, Text);

								Text.Clear();
								Text.Append('h');

								if (chs[3] == ':')
									ch2 = (char)6;
								else
									ch2 = (char)7;

								while (ch2 > 0)
								{
									Text.Append(State.NextChar());
									ch2--;
								}

								while ((ch2 = State.PeekNextCharSameRow()) > ' ' && ch2 != 160)
								{
									Text.Append(ch2);
									State.NextChar();
								}

								Url = Text.ToString();
								Text.Clear();

								if (FirstCharOnLine && State.PeekNextNonWhitespaceCharSameRow() == 0)
								{
									IMultimediaContent Handler = Multimedia.GetMultimediaHandler(Url);
									if (Handler != null && Handler.EmbedInlineLink(Url))
									{
										ChildElements = new LinkedList<MarkdownElement>();
										ChildElements.AddLast(new InlineText(this, Url));

										Elements.AddLast(new Multimedia(this, ChildElements, true,
											new MultimediaItem(this, Url, string.Empty, null, null)));

										break;
									}
								}

								Elements.AddLast(new AutomaticLinkUrl(this, Url));
							}
							else
								Text.Append(ch);
						}
						else
							Text.Append(ch);
						break;

					case '\\':
						switch (ch2 = State.PeekNextCharSameRow())
						{
							case '*':
							case '_':
							case '~':
							case '\\':
							case '`':
							case '{':
							case '}':
							case '[':
							case ']':
							case '(':
							case ')':
							case '<':
							case '>':
							case '#':
							case '+':
							case '-':
							case '.':
							case '!':
							case '\'':
							case '"':
							case '^':
							case '%':
							case '&':
							case '=':
							case ':':
							case '|':
							case 'h':
								Text.Append(ch2);
								State.NextCharSameRow();
								break;

							case '0':
							case 'O':
								if (this.emojiSource != null)
								{
									State.BackupState();
									State.NextCharSameRow();
									if (State.PeekNextCharSameRow() == '/')
									{
										State.DiscardBackup();
										State.NextCharSameRow();
										this.AppendAnyText(Elements, Text);
										Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_ok_woman));
									}
									else
									{
										State.RestoreState();
										Text.Append('\\');
									}
								}
								else
									Text.Append('\\');
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

				PrevChar = State.LastCharacter;
			}

			this.AppendAnyText(Elements, Text);

			return (ch == TerminationCharacter);
		}

		private bool IsLeftQuote(char PrevChar, char NextChar)
		{
			bool Left = (PrevChar <= ' ' || PrevChar == 160 || char.IsPunctuation(PrevChar) || char.IsSeparator(PrevChar));
			bool Right = (NextChar <= ' ' || NextChar == 160 || char.IsPunctuation(NextChar) || char.IsSeparator(NextChar));

			if (Left && Right)
			{
				if (char.IsSeparator(PrevChar))
					return true;
				else if (char.IsSeparator(NextChar))
					return false;
				else if (PrevChar == ')' || PrevChar == ']' || PrevChar == '}')
					return false;
				else if (NextChar == '(' || NextChar == '[' || NextChar == '[')
					return true;
				else
					return false;
			}
			else
				return Left;
		}

		private void ParseWidthHeight(BlockParseState State, out int? Width, out int? Height)
		{
			Width = null;
			Height = null;

			char ch = State.PeekNextNonWhitespaceCharSameRow();
			if (ch >= '0' && ch <= '9')
			{
				StringBuilder Text = new StringBuilder();

				Text.Append(ch);
				State.NextCharSameRow();

				ch = State.PeekNextCharSameRow();
				while (ch >= '0' && ch <= '9')
				{
					Text.Append(ch);
					State.NextCharSameRow();
					ch = State.PeekNextCharSameRow();
				}

				if (int.TryParse(Text.ToString(), out int i))
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
			Elements.AddLast(new InlineText(this, Prefix));
			foreach (MarkdownElement E in ChildElements)
				Elements.AddLast(E);
		}

		internal static bool IsPrefixedByNumber(string s, out int Numeral)
		{
			int i, c = s.Length;
			char ch;

			i = 0;
			while (i < c && char.IsDigit(s[i]))
				i++;

			if (i == 0)
			{
				Numeral = 0;
				return false;
			}

			if (!int.TryParse(s.Substring(0, i), out Numeral) || i == c || s[i] != '.')
				return false;

			i++;
			if (i < c && (ch = s[i]) > ' ' && ch != 160)
				return false;

			return true;
		}

		internal static bool IsPrefixedBy(string s, string Prefix, bool MustHaveWhiteSpaceAfter)
		{
			int i;
			char ch;

			if (!s.StartsWith(Prefix))
				return false;

			if (MustHaveWhiteSpaceAfter)
			{
				if (s.Length == (i = Prefix.Length))
					return false;

				return (ch = s[i]) <= ' ' || ch == 160;
			}
			else
				return true;
		}

		private bool IsPrefixedBy(string s, char ch, out int Count)
		{
			int c = s.Length;

			Count = 0;
			while (Count < c && s[Count] == ch)
				Count++;

			return Count > 0;
		}

		private bool IsUnderline(string s, char ch, bool AllowSpaces, bool OnlyOneSpace)
		{
			int i, c = s.Length;
			bool LastSpace = true;
			int Count = 0;
			char ch2;

			for (i = 0; i < c; i++)
			{
				ch2 = s[i];
				if (ch2 == ch)
				{
					Count++;
					LastSpace = false;
				}
				else if (ch2 == ' ' || ch2 == 160)
				{
					if (OnlyOneSpace && (!AllowSpaces || LastSpace))
						return false;

					LastSpace = true;
				}
				else
					return false;
			}

			return Count >= 3;
		}

		private List<Block> ParseTextToBlocks(string MarkdownText)
		{
			List<Block> Blocks = new List<Block>();
			List<string> Rows = new List<string>();
			List<int> Positions = new List<int>();
			int FirstLineIndent = 0;
			int LineIndent = 0;
			int RowStart = 0;
			int RowEnd = 0;
			int Pos, Len;
			char ch;
			bool InBlock = false;
			bool InRow = false;
			bool NonWhitespaceInRow = false;

			Len = MarkdownText.Length;

			for (Pos = 0; Pos < Len; Pos++)
			{
				ch = MarkdownText[Pos];

				if (ch == '\n')
				{
					if (InBlock)
					{
						if (InRow && NonWhitespaceInRow)
						{
							Positions.Add(RowStart);
							Rows.Add(MarkdownText.Substring(RowStart, RowEnd - RowStart + 1));
							InRow = false;
						}
						else
						{
							Blocks.Add(new Block(Rows.ToArray(), Positions.ToArray(), FirstLineIndent / 4));
							Rows.Clear();
							Positions.Clear();
							InBlock = false;
							InRow = false;
							FirstLineIndent = 0;
						}
					}
					else
						FirstLineIndent = 0;

					LineIndent = 0;
					NonWhitespaceInRow = false;
				}
				else if (ch <= ' ' || ch == 160)
				{
					if (InBlock)
					{
						if (InRow)
							RowEnd = Pos;
						else
						{
							if (LineIndent >= FirstLineIndent)
							{
								InRow = true;
								RowStart = RowEnd = Pos;
							}

							if (ch == '\t')
								LineIndent += 4;
							else if (ch == ' ' || ch == 160)
								LineIndent++;
						}
					}
					else if (ch == '\t')
						FirstLineIndent += 4;
					else if (ch == ' ' || ch == 160)
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
					NonWhitespaceInRow = true;
				}
			}

			if (InBlock)
			{
				if (InRow && NonWhitespaceInRow)
				{
					Positions.Add(RowStart);
					Rows.Add(MarkdownText.Substring(RowStart, RowEnd - RowStart + 1));
				}

				Blocks.Add(new Block(Rows.ToArray(), Positions.ToArray(), FirstLineIndent / 4));
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
			if (!string.IsNullOrEmpty(this.fileName) && this.metaData.TryGetValue("MASTER", out KeyValuePair<string, bool>[] Master) && Master.Length == 1)
			{
				this.LoadMasterIfNotLoaded(Master[0].Key);
				this.master.GenerateHTML(Output, false);
			}
			else
				this.GenerateHTML(Output, false);
		}

		private void LoadMasterIfNotLoaded(string MasterMetaValue)
		{
			if (this.master is null)
			{
				string FileName = this.settings.GetFileName(this.fileName, MasterMetaValue);
				string MarkdownText = File.ReadAllText(FileName);
				this.master = new MarkdownDocument(MarkdownText, this.settings)
				{
					fileName = FileName
				};

				this.master.syntaxHighlighting |= this.syntaxHighlighting;

				if (this.master.metaData.ContainsKey("MASTER"))
					throw new Exception("Master documents are not allowed to be embedded in other master documents.");

				CopyMetaDataTags(this, this.master);

				this.master.detail = this;
			}
		}

		internal static void CopyMetaDataTags(MarkdownDocument From, MarkdownDocument To)
		{
			foreach (KeyValuePair<string, KeyValuePair<string, bool>[]> Meta in From.metaData)
			{
				if (To.metaData.TryGetValue(Meta.Key, out KeyValuePair<string, bool>[] Meta0))
					To.metaData[Meta.Key] = Concat(Meta0, Meta.Value);
				else
					To.metaData[Meta.Key] = Meta.Value;
			}
		}

		private static KeyValuePair<string, bool>[] Concat(KeyValuePair<string, bool>[] Meta1, KeyValuePair<string, bool>[] Meta2)
		{
			int c = Meta1.Length;
			int d = Meta2.Length;
			KeyValuePair<string, bool>[] Result = new KeyValuePair<string, bool>[c + d];

			Array.Copy(Meta1, 0, Result, 0, c);
			Array.Copy(Meta2, 0, Result, c, d);

			return Result;
		}

		/// <summary>
		/// Generates HTML from the markdown text.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		/// <param name="Inclusion">If the HTML is to be included in another document (true), or if it is a standalone document (false).</param>
		internal void GenerateHTML(StringBuilder Output, bool Inclusion)
		{
			if (!Inclusion)
			{
				StringBuilder sb = null;
				string Title;
				string Description;
				string s2;
				bool First;

				Output.AppendLine("<!DOCTYPE html>");
				Output.AppendLine("<html itemscope itemtype=\"http://schema.org/WebPage\">");
				Output.AppendLine("<head>");

				if (this.metaData.TryGetValue("TITLE", out KeyValuePair<string, bool>[] Values))
				{
					foreach (KeyValuePair<string, bool> P in Values)
					{
						if (sb is null)
							sb = new StringBuilder();
						else
							sb.Append(' ');

						sb.Append(P.Key);
					}

					if (this.metaData.TryGetValue("SUBTITLE", out Values))
					{
						sb.Append(" -");
						foreach (KeyValuePair<string, bool> P in Values)
						{
							sb.Append(' ');
							sb.Append(P.Key);
						}
					}

					Title = XML.HtmlAttributeEncode(sb.ToString());
					sb = null;

					Output.Append("<title>");
					if (string.IsNullOrEmpty(Title))
						Output.Append(' ');
					else
						Output.Append(Title);
					Output.AppendLine("</title>");

					Output.Append("<meta itemprop=\"name\" content=\"");
					Output.Append(Title);
					Output.AppendLine("\"/>");

					Output.Append("<meta name=\"twitter:title\" content=\"");
					Output.Append(Title);
					Output.AppendLine("\"/>");

					Output.Append("<meta name=\"og:title\" content=\"");
					Output.Append(Title);
					Output.AppendLine("\"/>");
				}
				else
					Output.AppendLine("<title> </title>");

				if (this.metaData.TryGetValue("DESCRIPTION", out Values))
				{
					foreach (KeyValuePair<string, bool> P in Values)
					{
						if (sb is null)
							sb = new StringBuilder();
						else
							sb.Append(" ");

						sb.Append(P.Key);
					}

					if (sb != null)
					{
						Description = XML.HtmlAttributeEncode(sb.ToString());

						Output.Append("<meta itemprop=\"description\" content=\"");
						Output.Append(Description);
						Output.AppendLine("\"/>");

						Output.Append("<meta name=\"twitter:description\" content=\"");
						Output.Append(Description);
						Output.AppendLine("\"/>");

						Output.Append("<meta name=\"og:description\" content=\"");
						Output.Append(Description);
						Output.AppendLine("\"/>");
					}
				}

				if (this.metaData.TryGetValue("AUTHOR", out Values))
				{
					if (sb is null || string.IsNullOrEmpty(s2 = sb.ToString()))
						sb = new StringBuilder("Author:");
					else
					{
						char ch = s2[s2.Length - 1];

						if (!char.IsPunctuation(ch))
							sb.Append(',');

						sb.Append(" Author:");
					}

					foreach (KeyValuePair<string, bool> P in Values)
					{
						sb.Append(' ');
						sb.Append(P.Key);
					}
				}

				if (this.metaData.TryGetValue("DATE", out Values))
				{
					if (sb is null || string.IsNullOrEmpty(s2 = sb.ToString()))
						sb = new StringBuilder("Date:");
					else
					{
						char ch = s2[s2.Length - 1];

						if (!char.IsPunctuation(ch))
							sb.Append(',');

						sb.Append(" Date:");
					}

					foreach (KeyValuePair<string, bool> P in Values)
					{
						sb.Append(' ');
						sb.Append(P.Key);
					}
				}

				if (sb != null)
				{
					Output.Append("<meta name=\"description\" content=\"");
					Output.Append(XML.HtmlAttributeEncode(sb.ToString()));
					Output.AppendLine("\"/>");
				}

				foreach (KeyValuePair<string, KeyValuePair<string, bool>[]> MetaData in this.metaData)
				{
					switch (MetaData.Key)
					{
						case "ACCESS-CONTROL-ALLOW-ORIGIN":
						case "ALLOWSCRIPTTAG":
						case "ALTERNATE":
						case "AUDIOAUTOPLAY":
						case "AUDIOCONTROLS":
						case "CONTENT-SECURITY-POLICY":
						case "COPYRIGHT":
						case "CACHE-CONTROL":
						case "CSS":
						case "DATE":
						case "DESCRIPTION":
						case "HELP":
						case "ICON":
						case "JAVASCRIPT":
						case "LOGIN":
						case "MASTER":
						case "NEXT":
						case "PARAMETER":
						case "PUBLIC-KEY-PINS":
						case "PREV":
						case "PREVIOUS":
						case "PRIVILEGE":
						case "SCRIPT":
						case "STRICT-TRANSPORT-SECURITY":
						case "SUBTITLE":
						case "SUNSET":
						case "TITLE":
						case "USERVARIABLE":
						case "VARY":
						case "VIDEOAUTOPLAY":
						case "VIDEOCONTROLS":
							break;

						case "KEYWORDS":
							First = true;
							Output.Append("<meta name=\"keywords\" content=\"");

							foreach (KeyValuePair<string, bool> P in MetaData.Value)
							{
								if (First)
									First = false;
								else
									Output.Append(", ");

								Output.Append(XML.HtmlAttributeEncode(P.Key));
							}

							Output.AppendLine("\"/>");
							break;

						case "AUTHOR":
							foreach (KeyValuePair<string, bool> P in MetaData.Value)
							{
								Output.Append("<meta name=\"author\" content=\"");
								Output.Append(XML.HtmlAttributeEncode(P.Key));
								Output.AppendLine("\"/>");
							}
							break;

						case "IMAGE":
							foreach (KeyValuePair<string, bool> P in MetaData.Value)
							{
								s2 = XML.HtmlAttributeEncode(P.Key);

								Output.Append("<meta itemprop=\"image\" content=\"");
								Output.Append(s2);
								Output.AppendLine("\"/>");

								Output.Append("<meta name=\"twitter:image\" content=\"");
								Output.Append(s2);
								Output.AppendLine("\"/>");

								Output.Append("<meta name=\"og:image\" content=\"");
								Output.Append(s2);
								Output.AppendLine("\"/>");
							}
							break;

						case "WEB":
							foreach (KeyValuePair<string, bool> P in MetaData.Value)
							{
								Output.Append("<meta name=\"og:url\" content=\"");
								Output.Append(XML.HtmlAttributeEncode(P.Key));
								Output.AppendLine("\"/>");
							}
							break;

						case "REFRESH":
							foreach (KeyValuePair<string, bool> P in MetaData.Value)
							{
								Output.Append("<meta http-equiv=\"refresh\" content=\"");
								Output.Append(XML.HtmlAttributeEncode(P.Key));
								Output.AppendLine("\"/>");
							}
							break;

						default:
							foreach (KeyValuePair<string, bool> P in MetaData.Value)
							{
								Output.Append("<meta name=\"");
								Output.Append(XML.HtmlAttributeEncode(MetaData.Key));
								Output.Append("\" content=\"");
								Output.Append(XML.HtmlAttributeEncode(P.Key));
								Output.AppendLine("\"/>");
							}
							break;
					}
				}

				foreach (KeyValuePair<string, KeyValuePair<string, bool>[]> MetaData in this.metaData)
				{
					switch (MetaData.Key)
					{
						case "COPYRIGHT":
							foreach (KeyValuePair<string, bool> P in MetaData.Value)
							{
								Output.Append("<link rel=\"copyright\" href=\"");
								Output.Append(XML.HtmlAttributeEncode(this.CheckURL(P.Key, null)));
								Output.AppendLine("\"/>");
							}
							break;

						case "PREVIOUS":
						case "PREV":
							foreach (KeyValuePair<string, bool> P in MetaData.Value)
							{
								Output.Append("<link rel=\"prev\" href=\"");
								Output.Append(XML.HtmlAttributeEncode(this.CheckURL(P.Key, null)));
								Output.AppendLine("\"/>");
							}
							break;

						case "NEXT":
							foreach (KeyValuePair<string, bool> P in MetaData.Value)
							{
								Output.Append("<link rel=\"next\" href=\"");
								Output.Append(XML.HtmlAttributeEncode(this.CheckURL(P.Key, null)));
								Output.AppendLine("\"/>");
							}
							break;

						case "ALTERNATE":
							foreach (KeyValuePair<string, bool> P in MetaData.Value)
							{
								Output.Append("<link rel=\"alternate\" href=\"");
								Output.Append(XML.HtmlAttributeEncode(this.CheckURL(P.Key, null)));
								Output.AppendLine("\"/>");
							}
							break;

						case "HELP":
							foreach (KeyValuePair<string, bool> P in MetaData.Value)
							{
								Output.Append("<link rel=\"help\" href=\"");
								Output.Append(XML.HtmlAttributeEncode(this.CheckURL(P.Key, null)));
								Output.AppendLine("\"/>");
							}
							break;

						case "ICON":
							foreach (KeyValuePair<string, bool> P in MetaData.Value)
							{
								Output.Append("<link rel=\"shortcut icon\" href=\"");
								Output.Append(XML.HtmlAttributeEncode(this.CheckURL(P.Key, null)));
								Output.AppendLine("\"/>");
							}
							break;

						case "CSS":
							foreach (KeyValuePair<string, bool> P in MetaData.Value)
							{
								Output.Append("<link rel=\"stylesheet\" href=\"");
								Output.Append(XML.HtmlAttributeEncode(this.CheckURL(P.Key, null)));
								Output.AppendLine("\"/>");
							}
							break;

						case "JAVASCRIPT":
							foreach (KeyValuePair<string, bool> P in MetaData.Value)
							{
								Output.Append("<script type=\"application/javascript\" src=\"");
								Output.Append(XML.HtmlAttributeEncode(this.CheckURL(P.Key, null)));
								Output.AppendLine("\"></script>");
							}
							break;
					}
				}

				if (this.syntaxHighlighting)
				{
					Output.AppendLine("<link rel=\"stylesheet\" href=\"/highlight/styles/default.css\">");
					Output.AppendLine("<script src=\"/highlight/highlight.pack.js\"></script>");
					Output.AppendLine("<script>hljs.initHighlightingOnLoad();</script>");
				}

				Output.AppendLine("</head>");
				Output.AppendLine("<body>");
			}

			bool AddSection = (!Inclusion && this.detail is null && this.elements.First != null && !(this.elements.First.Value is Sections));
			if (AddSection)
				Output.AppendLine("<section>");

			foreach (MarkdownElement E in this.elements)
				E.GenerateHTML(Output);

			if (AddSection)
				Output.AppendLine("</section>");

			if (this.footnoteOrder != null && this.footnoteOrder.Count > 0)
			{
				Output.AppendLine("<div class=\"footnotes\">");
				Output.AppendLine("<hr />");
				Output.AppendLine("<ol>");

				foreach (string Key in this.footnoteOrder)
				{
					if (this.footnoteNumbers.TryGetValue(Key, out int Nr) && this.footnotes.TryGetValue(Key, out Footnote Footnote))
					{
						Output.Append("<li id=\"fn-");
						Output.Append(Nr.ToString());
						Output.Append("\">");

						if (!footnoteBacklinksAdded)
						{
							InlineHTML Backlink = new InlineHTML(this, "<a href=\"#fnref-" + Nr.ToString() + "\" class=\"footnote-backref\">&#8617;</a>");

							if (Footnote.LastChild is Paragraph P)
								P.AddChildren(Backlink);
							else
								Footnote.AddChildren(Backlink);
						}

						Footnote.GenerateHTML(Output);

						Output.AppendLine("</li>");
					}
				}

				this.footnoteBacklinksAdded = true;

				Output.AppendLine("</ol>");
				Output.AppendLine("</div>");
			}

			if (!Inclusion)
			{
				Output.AppendLine("</body>");
				Output.Append("</html>");
			}
		}

		/// <summary>
		/// Checks the URL if it needs redirection to a proxy.
		/// </summary>
		/// <param name="Url">URL to check.</param>
		/// <param name="URL">URL of the document. If null, or empty, relative URLs can be returned. If not null or empty,
		/// all URLs returned will be absolute.</param>
		/// <returns>URL to use in clients.</returns>
		public string CheckURL(string Url, string URL)
		{
			bool IsRelative = Url.IndexOf(':') < 0;

			if (Url.StartsWith("httpx:", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(this.settings.HttpxProxy))
			{
				if (!string.IsNullOrEmpty(this.settings.LocalHttpxResourcePath) &&
					Url.StartsWith(this.settings.LocalHttpxResourcePath, StringComparison.OrdinalIgnoreCase))
				{
					Url = Url.Substring(this.settings.LocalHttpxResourcePath.Length);
					if (!Url.StartsWith("/") && this.settings.LocalHttpxResourcePath.EndsWith("/"))
						Url = "/" + Url;

					IsRelative = true;
				}
				else
				{
					Url = this.settings.HttpxProxy.Replace("%URL%", Url);
					IsRelative = this.settings.HttpxProxy.IndexOf(':') < 0;
				}
			}

			if (IsRelative && !string.IsNullOrEmpty(URL))
			{
				if (Uri.TryCreate(new Uri(URL), Url, out Uri AbsoluteUri))
					Url = AbsoluteUri.ToString();
			}

			return Url;
		}

		/// <summary>
		/// Generates Plain Text from the markdown text.
		/// </summary>
		/// <returns>Plain Text</returns>
		public string GeneratePlainText()
		{
			StringBuilder Output = new StringBuilder();
			this.GeneratePlainText(Output);
			return Output.ToString();
		}

		/// <summary>
		/// Generates Plain Text from the markdown text.
		/// </summary>
		/// <param name="Output">Plain Text will be output here.</param>
		public void GeneratePlainText(StringBuilder Output)
		{
			foreach (MarkdownElement E in this.elements)
				E.GeneratePlainText(Output);

			if (this.footnoteOrder != null && this.footnoteOrder.Count > 0)
			{
				Output.AppendLine(new string('-', 80));
				Output.AppendLine();

				foreach (string Key in this.footnoteOrder)
				{
					if (this.footnoteNumbers.TryGetValue(Key, out int Nr) && this.footnotes.TryGetValue(Key, out Footnote Footnote))
					{
						Output.Append('[');
						Output.Append(Nr.ToString());
						Output.Append("] ");

						Footnote.GeneratePlainText(Output);
					}
				}
			}
		}

		/// <summary>
		/// Generates XAML from the markdown text.
		/// </summary>
		/// <returns>XAML</returns>
		public string GenerateXAML()
		{
			return this.GenerateXAML(XML.WriterSettings(false, true), new XamlSettings());
		}

		/// <summary>
		/// Generates XAML from the markdown text.
		/// </summary>
		/// <param name="XamlSettings">XAML settings.</param>
		/// <returns>XAML</returns>
		public string GenerateXAML(XamlSettings XamlSettings)
		{
			return this.GenerateXAML(XML.WriterSettings(false, true), XamlSettings);
		}

		/// <summary>
		/// Generates XAML from the markdown text.
		/// </summary>
		/// <param name="XmlSettings">XML settings.</param>
		/// <returns>XAML</returns>
		public string GenerateXAML(XmlWriterSettings XmlSettings)
		{
			return this.GenerateXAML(XmlSettings, new XamlSettings());
		}

		/// <summary>
		/// Generates XAML from the markdown text.
		/// </summary>
		/// <param name="XmlSettings">XML settings.</param>
		/// <param name="XamlSettings">XAML settings.</param>
		/// <returns>XAML</returns>
		public string GenerateXAML(XmlWriterSettings XmlSettings, XamlSettings XamlSettings)
		{
			StringBuilder Output = new StringBuilder();
			this.GenerateXAML(Output, XmlSettings, XamlSettings);
			return Output.ToString();
		}

		/// <summary>
		/// Generates XAML from the markdown text.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		public void GenerateXAML(StringBuilder Output)
		{
			this.GenerateXAML(Output, XML.WriterSettings(false, true), new XamlSettings());
		}

		/// <summary>
		/// Generates XAML from the markdown text.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="XamlSettings">XAML settings.</param>
		public void GenerateXAML(StringBuilder Output, XamlSettings XamlSettings)
		{
			this.GenerateXAML(Output, XML.WriterSettings(false, true), XamlSettings);
		}

		/// <summary>
		/// Generates XAML from the markdown text.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="XmlSettings">XML settings.</param>
		public void GenerateXAML(StringBuilder Output, XmlWriterSettings XmlSettings)
		{
			this.GenerateXAML(Output, XmlSettings, new XamlSettings());
		}

		/// <summary>
		/// Generates XAML from the markdown text.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="XmlSettings">XML settings.</param>
		/// <param name="XamlSettings">XAML settings.</param>
		public void GenerateXAML(StringBuilder Output, XmlWriterSettings XmlSettings, XamlSettings XamlSettings)
		{
			using (XmlWriter w = XmlWriter.Create(Output, XmlSettings))
			{
				this.GenerateXAML(w, XamlSettings, false);
			}
		}

		/// <summary>
		/// Generates XAML from the markdown text.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="Settings">XAML settings.</param>
		public void GenerateXAML(XmlWriter Output, XamlSettings Settings)
		{
			this.GenerateXAML(Output, Settings, false);
		}

		/// <summary>
		/// Generates XAML from the markdown text.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="Settings">XAML settings.</param>
		/// <param name="Inclusion">If the HTML is to be included in another document (true), or if it is a standalone document (false).</param>
		internal void GenerateXAML(XmlWriter Output, XamlSettings Settings, bool Inclusion)
		{
			if (!Inclusion)
			{
				Output.WriteStartElement("StackPanel", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
				Output.WriteAttributeString("xmlns", "x", null, "http://schemas.microsoft.com/winfx/2006/xaml");
			}

			foreach (MarkdownElement E in this.elements)
				E.GenerateXAML(Output, Settings, TextAlignment.Left);

			if (this.footnoteOrder != null && this.footnoteOrder.Count > 0)
			{
				Footnote Footnote;
				string FootnoteMargin = "0," + Settings.ParagraphMarginTop.ToString() + "," +
					Settings.FootnoteSeparator.ToString() + "," + Settings.ParagraphMarginBottom.ToString();
				string Scale = CommonTypes.Encode(Settings.SuperscriptScale);
				string Offset = Settings.SuperscriptOffset.ToString();
				int Nr;
				int Row = 0;

				Output.WriteElementString("Separator", string.Empty);

				Output.WriteStartElement("Grid");
				Output.WriteStartElement("Grid.ColumnDefinitions");

				Output.WriteStartElement("ColumnDefinition");
				Output.WriteAttributeString("Width", "Auto");
				Output.WriteEndElement();

				Output.WriteStartElement("ColumnDefinition");
				Output.WriteAttributeString("Width", "*");
				Output.WriteEndElement();

				Output.WriteEndElement();
				Output.WriteStartElement("Grid.RowDefinitions");

				foreach (string Key in this.footnoteOrder)
				{
					if (this.footnoteNumbers.TryGetValue(Key, out Nr) && this.footnotes.TryGetValue(Key, out Footnote))
					{
						Output.WriteStartElement("RowDefinition");
						Output.WriteAttributeString("Height", "Auto");
						Output.WriteEndElement();
					}
				}

				Output.WriteEndElement();

				foreach (string Key in this.footnoteOrder)
				{
					if (this.footnoteNumbers.TryGetValue(Key, out Nr) && this.footnotes.TryGetValue(Key, out Footnote))
					{
						Output.WriteStartElement("TextBlock");
						Output.WriteAttributeString("Text", Nr.ToString());
						Output.WriteAttributeString("Margin", FootnoteMargin);
						Output.WriteAttributeString("Grid.Column", "0");
						Output.WriteAttributeString("Grid.Row", Row.ToString());

						Output.WriteStartElement("TextBlock.LayoutTransform");
						Output.WriteStartElement("TransformGroup");

						Output.WriteStartElement("ScaleTransform");
						Output.WriteAttributeString("ScaleX", Scale);
						Output.WriteAttributeString("ScaleY", Scale);
						Output.WriteEndElement();

						Output.WriteStartElement("TranslateTransform");
						Output.WriteAttributeString("Y", Offset);
						Output.WriteEndElement();

						Output.WriteEndElement();
						Output.WriteEndElement();
						Output.WriteEndElement();

						if (Footnote.InlineSpanElement && !Footnote.OutsideParagraph)
						{
							Output.WriteStartElement("TextBlock");
							Output.WriteAttributeString("TextWrapping", "Wrap");
						}
						else
							Output.WriteStartElement("StackPanel");

						Output.WriteAttributeString("Grid.Column", "1");
						Output.WriteAttributeString("Grid.Row", Row.ToString());

						Footnote.GenerateXAML(Output, Settings, TextAlignment.Left);
						Output.WriteEndElement();

						Row++;
					}
				}

				Output.WriteEndElement();
			}

			if (!Inclusion)
			{
				Output.WriteEndElement();
				Output.Flush();
			}
		}

		/// <summary>
		/// Exports the parsed document to XML.
		/// </summary>
		/// <returns>XML String.</returns>
		public string ExportXml()
		{
			StringBuilder Xml = new StringBuilder();
			this.ExportXml(Xml);
			return Xml.ToString();
		}

		/// <summary>
		/// Exports the parsed document to XML.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public void ExportXml(StringBuilder Xml)
		{
			this.ExportXml(Xml, XML.WriterSettings(true, true));
		}

		/// <summary>
		/// Exports the parsed document to XML.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		/// <param name="Settings">XML Settings.</param>
		public void ExportXml(StringBuilder Xml, XmlWriterSettings Settings)
		{
			using (XmlWriter w = XmlWriter.Create(Xml, Settings))
			{
				this.ExportXml(w);
				w.Flush();
			}
		}

		/// <summary>
		/// Exports the parsed document to XML.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public void ExportXml(XmlWriter Xml)
		{
			Xml.WriteStartElement("parsedMakdown", "http://waher.se/Schema/Markdown.xsd");
			Xml.WriteAttributeString("isDynamic", CommonTypes.Encode(this.isDynamic));

			if (this.metaData != null)
			{
				Xml.WriteStartElement("metaData");

				foreach (KeyValuePair<string, KeyValuePair<string, bool>[]> P in this.metaData)
				{
					Xml.WriteStartElement("tag", P.Key);

					foreach (KeyValuePair<string, bool> P2 in P.Value)
					{
						Xml.WriteStartElement("value", P2.Key);
						Xml.WriteAttributeString("lineBreak", CommonTypes.Encode(P2.Value));
						Xml.WriteEndElement();
					}

					Xml.WriteEndElement();
				}

				Xml.WriteEndElement();
			}

			Xml.WriteStartElement("elements");

			foreach (MarkdownElement E in this.elements)
				E.Export(Xml);

			Xml.WriteEndElement();

			if (this.references != null)
			{
				Xml.WriteStartElement("references");

				foreach (KeyValuePair<string, Multimedia> P in this.references)
				{
					Xml.WriteStartElement("reference");
					Xml.WriteAttributeString("key", P.Key);

					P.Value.Export(Xml);

					Xml.WriteEndElement();
				}

				Xml.WriteEndElement();
			}

			if (this.footnoteOrder != null)
			{
				Xml.WriteStartElement("footnotes");

				foreach (string s in this.footnoteOrder)
				{
					if (this.footnotes.TryGetValue(s, out Footnote F))
						F.Export(Xml);
				}

				Xml.WriteEndElement();
			}

			Xml.WriteEndElement();
		}

		internal Multimedia GetReference(string Label)
		{
			if (this.references.TryGetValue(Label.ToLower(), out Multimedia Result))
				return Result;
			else
				return null;
		}

		private static readonly char[] whiteSpace = new char[]
		{
			(char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8, (char)9,(char)10,
			(char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17, (char)18, (char)19,(char)20,
			(char)21, (char)22, (char)23, (char)24, (char)25, (char)26, (char)27, (char)28, (char)29,(char)30,
			(char)31, (char)32
		};

		/// <summary>
		/// Headers in document.
		/// </summary>
		public Header[] Headers
		{
			get { return this.headers.ToArray(); }
		}

		/// <summary>
		/// Tries to get a meta-data value given its key.
		/// </summary>
		/// <param name="Key">Meta-data value.</param>
		/// <param name="Value">(Value,linebreak)-pairs corresponding to the key, if found, null otherwise.</param>
		/// <returns>If the meta-data key was found.</returns>
		public bool TryGetMetaData(string Key, out KeyValuePair<string, bool>[] Value)
		{
			return this.metaData.TryGetValue(Key.ToUpper(), out Value);
		}

		/// <summary>
		/// Gets the meta-data values given a meta-data key. If meta-data is not found, an empty array is returned.
		/// </summary>
		/// <param name="Key">Meta-data key.</param>
		/// <returns>Values for the given key, or an empty array if the key was not found.</returns>
		public string[] GetMetaData(string Key)
		{
			if (!this.metaData.TryGetValue(Key.ToUpper(), out KeyValuePair<string, bool>[] Value))
				return new string[0];

			int i, c = Value.Length;
			string[] Result = new string[c];

			for (i = 0; i < c; i++)
				Result[i] = Value[i].Key;

			return Result;
		}

		/// <summary>
		/// Meta-data keys availale in document.
		/// </summary>
		public string[] MetaDataKeys
		{
			get
			{
				string[] Keys = new string[this.metaData.Count];
				this.metaData.Keys.CopyTo(Keys, 0);
				return Keys;
			}
		}

		/// <summary>
		/// Author(s) of document.
		/// </summary>
		public string[] Author
		{
			get
			{
				return this.GetMetaData("Author");
			}
		}

		/// <summary>
		/// Link to copyright statement.
		/// </summary>
		public string[] Copyright
		{
			get
			{
				return this.GetMetaData("Copyright");
			}
		}

		/// <summary>
		/// Link to previous document, in a paginated set of documents.
		/// </summary>
		public string[] Previous
		{
			get
			{
				return this.Merge(this.GetMetaData("Previous"), this.GetMetaData("Prev"));
			}
		}

		private string[] Merge(string[] L1, string[] L2)
		{
			int c1, c2;

			if ((c1 = L1.Length) == 0)
				return L2;
			else if ((c2 = L2.Length) == 0)
				return L1;

			string[] L = new string[c1 + c2];

			Array.Copy(L1, 0, L, 0, c1);
			Array.Copy(L2, 0, L, c1, c2);

			return L;
		}

		/// <summary>
		/// Link to next document, in a paginated set of documents.
		/// </summary>
		public string[] Next
		{
			get
			{
				return this.GetMetaData("Next");
			}
		}

		/// <summary>
		/// Link(s) to Cascading Style Sheet(s) that should be used for visual formatting of the generated HTML page.
		/// </summary>
		public string[] CSS
		{
			get
			{
				return this.GetMetaData("CSS");
			}
		}

		/// <summary>
		/// Link(s) to JavaScript files(s) that should be includedin the generated HTML page.
		/// </summary>
		public string[] JavaScript
		{
			get
			{
				return this.GetMetaData("JAVASCRIPT");
			}
		}

		/// <summary>
		/// Links to server-side script files that should be included before processing the page.
		/// </summary>
		public string[] Script
		{
			get
			{
				return this.GetMetaData("SCRIPT");
			}
		}

		/// <summary>
		/// Name of a query parameter recognized by the page.
		/// </summary>
		public string[] Parameters
		{
			get
			{
				return this.GetMetaData("PARAMETER");
			}
		}

		/// <summary>
		/// (Publication) date of document.
		/// </summary>
		public string[] Date
		{
			get
			{
				return this.GetMetaData("Date");
			}
		}

		/// <summary>
		/// Description of document.
		/// </summary>
		public string[] Description
		{
			get
			{
				return this.GetMetaData("Description");
			}
		}

		/// <summary>
		/// Link to image for page.
		/// </summary>
		public string[] Image
		{
			get
			{
				return this.GetMetaData("Image");
			}
		}

		/// <summary>
		/// Keywords.
		/// </summary>
		public string[] Keywords
		{
			get
			{
				return this.GetMetaData("Keywords");
			}
		}

		/// <summary>
		/// Subtitle of document.
		/// </summary>
		public string[] Subtitle
		{
			get
			{
				return this.GetMetaData("Subtitle");
			}
		}

		/// <summary>
		/// Title of document.
		/// </summary>
		public string[] Title
		{
			get
			{
				return this.GetMetaData("Title");
			}
		}

		/// <summary>
		/// Link to web page
		/// </summary>
		public string[] Web
		{
			get
			{
				return this.GetMetaData("Web");
			}
		}

		/// <summary>
		/// Tells the browser to refresh the page after a given number of seconds.
		/// </summary>
		public string[] Refresh
		{
			get
			{
				return this.GetMetaData("Refresh");
			}
		}

		/// <summary>
		/// Name of the variable that will hold a reference to the IUser interface for the currently logged in user.
		/// </summary>
		public string[] UserVariable
		{
			get
			{
				return this.GetMetaData("UserVariable");
			}
		}

		/// <summary>
		/// Link to a login page. This page will be shown if the user variable does not contain a user.
		/// </summary>
		public string[] Login
		{
			get
			{
				return this.GetMetaData("Login");
			}
		}

		/// <summary>
		/// Requered user privileges to display page.
		/// </summary>
		public string[] Privileges
		{
			get
			{
				return this.GetMetaData("Privileges");
			}
		}

		/// <summary>
		/// Tries to get the number of a footnote, given its key.
		/// </summary>
		/// <param name="Key">Footnote key.</param>
		/// <param name="Number">Footnote number.</param>
		/// <returns>If a footnote with the given key was found.</returns>
		public bool TryGetFootnoteNumber(string Key, out int Number)
		{
			if (this.footnoteNumbers is null)
			{
				Number = 0;
				return false;
			}
			else
				return this.footnoteNumbers.TryGetValue(Key, out Number);
		}

		/// <summary>
		/// Tries to get a footnote, given its key.
		/// </summary>
		/// <param name="Key">Footnote key.</param>
		/// <param name="Footnote">Footnote.</param>
		/// <returns>If a footnote with the given key was found.</returns>
		public bool TryGetFootnote(string Key, out Footnote Footnote)
		{
			if (this.footnotes is null)
			{
				Footnote = null;
				return false;
			}
			else
				return this.footnotes.TryGetValue(Key, out Footnote);
		}

		/// <summary>
		/// Gets the keys of the footnotes in the order that they are referenced in the document. Footnotes that are not actually
		/// used in the document are omitted.
		/// </summary>
		public string[] Footnotes
		{
			get
			{
				if (this.footnoteOrder is null)
					return new string[0];
				else
					return this.footnoteOrder.ToArray();
			}
		}

		/// <summary>
		/// Source for emojis in the document.
		/// </summary>
		public IEmojiSource EmojiSource
		{
			get { return this.emojiSource; }
		}

		/// <summary>
		/// Encodes all special characters in a string so that it can be included in a markdown document without affecting the markdown.
		/// </summary>
		/// <param name="s">String to encode.</param>
		/// <returns>Encoded string.</returns>
		public static string Encode(string s)
		{
			return Functions.MarkdownEncode.EscapeText(s);
		}

		/// <summary>
		/// If syntax highlighting is used in the document.
		/// </summary>
		public bool SyntaxHighlighting
		{
			get { return this.syntaxHighlighting; }
		}

		/// <summary>
		/// Filename of Markdown document. Markdown inclusion will be made relative to this filename.
		/// </summary>
		public string FileName
		{
			get { return this.fileName; }
			set { this.fileName = value; }
		}

		/// <summary>
		/// Local resource name of Markdown document, if referenced through a web server. Master documents use this resource name to match
		/// detail content with menu links.
		/// </summary>
		public string ResourceName
		{
			get { return this.resourceName; }
			set { this.resourceName = value; }
		}

		/// <summary>
		/// Absolute URL of Markdown document, if referenced through a web server.
		/// </summary>
		public string URL
		{
			get { return this.url; }
			set { this.url = value; }
		}

		/// <summary>
		/// Master document responsible for the current document.
		/// </summary>
		public MarkdownDocument Master
		{
			get { return this.master; }
			set { this.master = value; }
		}

		/// <summary>
		/// Detail document of a master document.
		/// </summary>
		public MarkdownDocument Detail
		{
			get
			{
				if (!(this.detail is null))
					return this.detail;

				if (this.master is null)
					return null;

				MarkdownDocument Doc = this.master.Detail;
				if (Doc != this)
					return Doc;

				return null;
			}
			set { this.detail = value; }
		}

		/// <summary>
		/// Markdown settings.
		/// </summary>
		public MarkdownSettings Settings
		{
			get { return this.settings; }
		}

		/// <summary>
		/// If the document contains a Table of Contents.
		/// </summary>
		public bool IncludesTableOfContents
		{
			get { return this.includesTableOfContents; }
		}

		/// <summary>
		/// If the contents of the document is dynamic (i.e. includes script), or not (i.e. is static).
		/// </summary>
		public bool IsDynamic
		{
			get { return this.isDynamic; }
		}

		/// <summary>
		/// Loops through all elements in the document.
		/// </summary>
		/// <param name="Callback">Method called for each one of the elements.</param>
		/// <param name="State">State object passed on to the callback method.</param>
		/// <returns>If the operation was completed.</returns>
		public bool ForEach(MarkdownElementHandler Callback, object State)
		{
			if (this.elements != null)
			{
				foreach (MarkdownElement E in this.elements)
				{
					if (!E.ForEach(Callback, State))
						return false;
				}
			}

			if (this.references != null)
			{
				foreach (MarkdownElement E in this.references.Values)
				{
					if (!E.ForEach(Callback, State))
						return false;
				}
			}

			if (this.footnotes != null)
			{
				foreach (MarkdownElement E in this.footnotes.Values)
				{
					if (!E.ForEach(Callback, State))
						return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Finds all links in the document.
		/// </summary>
		/// <returns>Array of links found in the document.</returns>
		public string[] FindLinks()
		{
			return this.FindLinks(true, true, true);
		}

		/// <summary>
		/// Finds all links in the document.
		/// </summary>
		/// <param name="IncludeAutomaticLinks">If automatic links are to be included. (Default=true)</param>
		/// <param name="IncludeLinks">If normal links are to be included. (Default=true)</param>
		/// <param name="IncludeMultimedia">If Multimedia links are to be included. (Default=true)</param>
		/// <returns>Array of links found in the document.</returns>
		public string[] FindLinks(bool IncludeAutomaticLinks, bool IncludeLinks, bool IncludeMultimedia)
		{
			Dictionary<string, bool> Links = new Dictionary<string, bool>();

			this.ForEach((E, Obj) =>
			{
				if (E is AutomaticLinkUrl AutomaticLinkUrl)
				{
					if (IncludeAutomaticLinks)
						Links[AutomaticLinkUrl.URL] = true;
				}
				else if (E is Link Link)
				{
					if (IncludeLinks)
						Links[Link.Url] = true;
				}
				else if (E is Multimedia Multimedia)
				{
					if (IncludeMultimedia)
					{
						foreach (MultimediaItem Item in Multimedia.Items)
							Links[Item.Url] = true;
					}
				}

				return true;
			}, null);

			string[] Result = new string[Links.Count];
			Links.Keys.CopyTo(Result, 0);
			return Result;
		}

		/// <summary>
		/// Gets an enumerator of root markdown elements in the document.
		/// </summary>
		public IEnumerator<MarkdownElement> GetEnumerator()
		{
			return this.elements.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.elements.GetEnumerator();
		}

		private class ReversePosition : IComparer<int>
		{
			public int Compare(int x, int y)
			{
				return y - x;
			}
		}

		/// <summary>
		/// Calculates the difference of two Markdown documents.
		/// </summary>
		/// <param name="Old">Old version of the document.</param>
		/// <param name="New">New version of the document.</param>
		/// <param name="KeepUnchanged">If unchanged parts of the document should be kept.</param>
		/// <returns>Difference document</returns>
		public static MarkdownDocument Compare(MarkdownDocument Old, MarkdownDocument New, bool KeepUnchanged)
		{
			return New.Compare(Old, KeepUnchanged);
		}

		/// <summary>
		/// Calculates the difference of two Markdown documents.
		/// </summary>
		/// <param name="Old">Old version of the document.</param>
		/// <param name="New">New version of the document.</param>
		/// <param name="Settings">Markdown settings.</param>
		/// <param name="KeepUnchanged">If unchanged parts of the document should be kept.</param>
		/// <param name="TransparentExceptionTypes">If an exception is thrown when processing script in markdown, and the exception is of
		/// any of these types, the exception will be rethrown, instead of shown as an error in the generated output.</param>
		/// <returns>Difference document</returns>
		public static string Compare(string Old, string New, MarkdownSettings Settings, bool KeepUnchanged,
			params Type[] TransparentExceptionTypes)
		{
			MarkdownDocument OldDoc = new MarkdownDocument(Old, Settings, TransparentExceptionTypes);
			MarkdownDocument NewDoc = new MarkdownDocument(New, Settings, TransparentExceptionTypes);
			MarkdownDocument DiffDoc = Compare(OldDoc, NewDoc, KeepUnchanged);

			return DiffDoc.MarkdownText;
		}

		/// <summary>
		/// Calculates the difference of the current Markdown document, and a previous version of the Markdown document.
		/// </summary>
		/// <param name="Previous">Previous version</param>
		/// <param name="KeepUnchanged">If unchanged parts of the document should be kept.</param>
		/// <returns>Difference document</returns>
		public MarkdownDocument Compare(MarkdownDocument Previous, bool KeepUnchanged)
		{
			// TODO: Meta-data

			MarkdownDocument Result = new MarkdownDocument(string.Empty, this.settings, this.transparentExceptionTypes);
			IEnumerable<MarkdownElement> Edit = Compare(Previous.elements, this.elements, KeepUnchanged, Result);

			foreach (MarkdownElement E in Edit)
				Result.elements.AddLast(E);

			// TODO: Footnotes

			Result.markdownText = null; // Triggers export, if needed.
			return Result;
		}

		private static IEnumerable<MarkdownElement> Atomize(IEnumerable<MarkdownElement> Elements, out bool Reassemble)
		{
			if (ContainsEditableText(Elements))
			{
				Reassemble = true;
				return Atomize(Elements);
			}
			else
			{
				Reassemble = false;
				return Elements;
			}
		}

		private static IEnumerable<MarkdownElement> Atomize(IEnumerable<MarkdownElement> Elements)
		{
			LinkedList<MarkdownElement> Result = new LinkedList<MarkdownElement>();

			foreach (MarkdownElement E in Elements)
			{
				if (E is IEditableText EditableText)
				{
					foreach (MarkdownElement E2 in EditableText.Atomize())
						Result.AddLast(E2);
				}
				else
					Result.AddLast(E);
			}

			return Result;
		}

		private static bool ContainsEditableText(IEnumerable<MarkdownElement> Elements)
		{
			foreach (MarkdownElement E in Elements)
			{
				if (E is IEditableText)
					return true;
			}

			return false;
		}

		private static MarkdownElement[] ToArray(IEnumerable<MarkdownElement> Elements)
		{
			if (Elements is MarkdownElement[] Array)
				return Array;

			if (Elements is ICollection<MarkdownElement> Collection)
			{
				Array = new MarkdownElement[Collection.Count];
				Collection.CopyTo(Array, 0);
				return Array;
			}

			int c = 0;

			foreach (MarkdownElement E in Elements)
				c++;

			Array = new MarkdownElement[c];

			c = 0;

			foreach (MarkdownElement E in Elements)
				Array[c++] = E;

			return Array;
		}

		private static IEnumerable<MarkdownElement> Compare(IEnumerable<MarkdownElement> Elements1,
			IEnumerable<MarkdownElement> Elements2, bool KeepUnchanged, MarkdownDocument Document)
		{
			LinkedList<MarkdownElement> Result = new LinkedList<MarkdownElement>();
			MarkdownElement[] S1 = ToArray(Atomize(Elements1, out bool Reassemble1));
			MarkdownElement[] S2 = ToArray(Atomize(Elements2, out bool Reassemble2));
			EditScript<MarkdownElement> Script = Difference.Analyze<MarkdownElement>(S1, S2);
			Step<MarkdownElement> Step, Step2;
			int i, c = Script.Steps.Length;

			if (Reassemble1 || Reassemble2)
			{
				List<MarkdownElement> Reassembled = new List<MarkdownElement>();
				StringBuilder sb = new StringBuilder();

				for (i = 0; i < c; i++)
				{
					Step = Script.Steps[i];

					switch (Step.Operation)
					{
						case EditOperation.Keep:
						case EditOperation.Delete:
							if (!Reassemble1)
								continue;
							break;

						case EditOperation.Insert:
							if (!Reassemble2)
								continue;
							break;

						default:
							continue;
					}

					Type LastAtomType = null;
					Atom LastAtom = null;
					Type AtomType;

					foreach (MarkdownElement E in Step.Symbols)
					{
						if (E is Atom Atom)
						{
							AtomType = Atom.GetType();
							if (AtomType != LastAtomType)
							{
								if (!(LastAtom is null))
								{
									Reassembled.Add(LastAtom.Source.Assemble(Document, sb.ToString()));
									sb.Clear();
								}

								LastAtom = Atom;
								LastAtomType = AtomType;
							}

							sb.Append(Atom.Charater);
						}
						else
						{
							if (!(LastAtom is null))
							{
								Reassembled.Add(LastAtom.Source.Assemble(Document, sb.ToString()));
								sb.Clear();
								LastAtom = null;
								LastAtomType = null;
							}

							Reassembled.Add(E);
						}
					}

					if (!(LastAtom is null))
					{
						Reassembled.Add(LastAtom.Source.Assemble(Document, sb.ToString()));
						sb.Clear();
					}

					Step.Symbols = Reassembled.ToArray();
					Reassembled.Clear();
				}
			}

			for (i = 0; i < c; i++)
			{
				Step = Script.Steps[i];

				if (Step.Operation == EditOperation.Keep)
				{
					if (!KeepUnchanged)
						continue;

					foreach (MarkdownElement E in Step.Symbols)
						Result.AddLast(E);
				}
				else
				{
					if (i + 1 < c &&
						(Step2 = Script.Steps[i + 1]).Operation != EditOperation.Keep &&
						Step2.Operation != Step.Operation &&
						SameBlockTypes(Step.Symbols, Step2.Symbols))
					{
						MarkdownElement E1, E2;
						int j, d = Step.Symbols.Length;

						for (j = 0; j < d; j++)
						{
							if (Step.Operation == EditOperation.Insert)
							{
								E2 = Step.Symbols[j];
								E1 = Step2.Symbols[j];
							}
							else
							{
								E1 = Step.Symbols[j];
								E2 = Step2.Symbols[j];
							}

							if (E1 is MarkdownElementChildren Children1 &&
								E2 is MarkdownElementChildren Children2)
							{
								IEnumerable<MarkdownElement> Diff = Compare(Children1.Children, Children2.Children, 
									KeepUnchanged || d > 1, Document);

								Result.AddLast(Children1.Create(Diff, Document));
							}
							else if (E1 is MarkdownElementSingleChild Child1 &&
								E2 is MarkdownElementSingleChild Child2 &&
								Child1.Child.SameMetaData(Child2.Child) &&
								Child1.Child is MarkdownElementChildren GrandChildren1 &&
								Child2.Child is MarkdownElementChildren GrandChildren2)
							{
								IEnumerable<MarkdownElement> Diff = Compare(GrandChildren1.Children, GrandChildren2.Children,
									KeepUnchanged || d > 1, Document);

								Result.AddLast(Child1.Create(GrandChildren1.Create(Diff, Document), Document));
							}
							else
							{
								Result.AddLast(GetElement(Step.Operation, Document, E1));
								Result.AddLast(GetElement(Step2.Operation, Document, E2));
							}
						}

						i++;
					}
					else
						Result.AddLast(GetElement(Step.Operation, Document, Step.Symbols));
				}
			}

			return Result;
		}

		private static MarkdownElement GetElement(EditOperation Operation, MarkdownDocument Document, params MarkdownElement[] Symbols)
		{
			if (Symbols[0].IsBlockElement)
			{
				switch (Operation)
				{
					case EditOperation.Insert:
						return new InsertBlocks(Document, Symbols);

					case EditOperation.Delete:
						return new DeleteBlocks(Document, Symbols);
				}
			}
			else
			{
				switch (Operation)
				{
					case EditOperation.Insert:
						return new Insert(Document, Symbols);

					case EditOperation.Delete:
						return new Delete(Document, Symbols);
				}
			}

			return new InvisibleBreak(Document, string.Empty);
		}

		private static bool SameBlockTypes(MarkdownElement[] E1, MarkdownElement[] E2)
		{
			int i, c = E1.Length;
			if (E2.Length != c)
				return false;

			MarkdownElement e;

			for (i = 0; i < c; i++)
			{
				if (!(e = E1[i]).IsBlockElement)
					return false;

				if (!e.SameMetaData(E2[i]))
					return false;
			}

			return true;
		}

		// TODO: Footnotes in included markdown files.
	}
}
