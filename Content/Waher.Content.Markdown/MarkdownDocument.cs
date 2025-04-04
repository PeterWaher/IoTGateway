using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Emoji;
using Waher.Content.Json;
using Waher.Content.Markdown.Functions;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Model.Atoms;
using Waher.Content.Markdown.Model.BlockElements;
using Waher.Content.Markdown.Model.SpanElements;
using Waher.Content.Markdown.Rendering;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;
using Waher.Runtime.Text;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Graphs;
using Waher.Script.Model;
using Waher.Script.Operators.Matrices;

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
	/// Delegate used for callback methods performing asynchronous Markdown processing
	/// </summary>
	/// <param name="State">State object.</param>
	public delegate Task AsyncMarkdownProcessing(object State);

	/// <summary>
	/// Contains a markdown document. This markdown document class supports original markdown, as well as several markdown extensions.
	/// See the markdown reference documentation provided with the library for more information.
	/// </summary>
	public class MarkdownDocument : IFileNameResource, IEnumerable<MarkdownElement>, IJsonEncodingHint
	{
		/// <summary>
		/// Variable name used for storing Markdown settings.
		/// </summary>
		public const string MarkdownSettingsVariableName = " MarkdownSettings ";

		internal static readonly Regex endOfHeader = new Regex(@"\n\s*\n", RegexOptions.Multiline | RegexOptions.Compiled);
		internal static readonly Regex scriptHeader = new Regex(@"^(?'Tag'(([Ss][Cc][Rr][Ii][Pp][Tt])|([Ii][Nn][Ii][Tt]))):\s*(?'ScriptFile'[^\r\n]*)", RegexOptions.Multiline | RegexOptions.Compiled);

		private readonly ChunkedList<KeyValuePair<AsyncMarkdownProcessing, object>> asyncTasks = new ChunkedList<KeyValuePair<AsyncMarkdownProcessing, object>>();
		private readonly Dictionary<string, Multimedia> references = new Dictionary<string, Multimedia>();
		private readonly Dictionary<string, KeyValuePair<string, bool>[]> metaData = new Dictionary<string, KeyValuePair<string, bool>[]>();
		private Dictionary<string, int> footnoteNumberByKey = null;
		private Dictionary<string, Footnote> footnotes = null;
		private SortedDictionary<int, char> toInsert = null;
		private readonly Type[] transparentExceptionTypes;
		private ChunkedList<string> footnoteOrder = null;
		private ChunkedList<MarkdownElement> elements;
		private readonly ChunkedList<Header> headers = new ChunkedList<Header>();
		private readonly IEmojiSource emojiSource;
		private string markdownText;
		private string fileName = string.Empty;
		private string resourceName = string.Empty;
		private string url = string.Empty;
		private MarkdownDocument master = null;
		private MarkdownDocument detail = null;
		private readonly MarkdownSettings settings;
		private int lastFootnote = 0;
		private bool syntaxHighlighting = false;
		private bool includesTableOfContents = false;
		private bool isDynamic = false;
		private bool? allowScriptTag = null;
		private object tag = null;

		/// <summary>
		/// Contains a markdown document. This markdown document class supports original markdown, as well as several markdown extensions.
		/// </summary>
		/// <param name="MarkdownText">Markdown text.</param>
		/// <param name="TransparentExceptionTypes">If an exception is thrown when processing script in markdown, and the exception is of
		/// any of these types, the exception will be rethrown, instead of shown as an error in the generated output.</param>
		public static Task<MarkdownDocument> CreateAsync(string MarkdownText, params Type[] TransparentExceptionTypes)
		{
			return CreateAsync(MarkdownText, new MarkdownSettings(), string.Empty, string.Empty, string.Empty, TransparentExceptionTypes);
		}

		/// <summary>
		/// Contains a markdown document. This markdown document class supports original markdown, as well as several markdown extensions.
		/// </summary>
		/// <param name="MarkdownText">Markdown text.</param>
		/// <param name="Settings">Parser settings.</param>
		/// <param name="TransparentExceptionTypes">If an exception is thrown when processing script in markdown, and the exception is of
		/// any of these types, the exception will be rethrown, instead of shown as an error in the generated output.</param>
		public static Task<MarkdownDocument> CreateAsync(string MarkdownText, MarkdownSettings Settings, params Type[] TransparentExceptionTypes)
		{
			return CreateAsync(MarkdownText, Settings, string.Empty, string.Empty, string.Empty, TransparentExceptionTypes);
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
		public static async Task<MarkdownDocument> CreateAsync(string MarkdownText, MarkdownSettings Settings, string FileName, string ResourceName, string URL,
			params Type[] TransparentExceptionTypes)
		{
			bool IsDynamic = false;

			if (!(Settings.Variables is null))
			{
				KeyValuePair<string, bool> P = await Preprocess(MarkdownText, Settings, FileName, TransparentExceptionTypes);
				MarkdownText = P.Key;
				IsDynamic = P.Value;
			}

			MarkdownDocument Result = new MarkdownDocument(MarkdownText, IsDynamic, Settings, FileName, ResourceName, URL, TransparentExceptionTypes);

			ICodecProgress Progress = Settings?.Progress;
			ChunkedList<Block> Blocks = ParseTextToBlocks(Result.markdownText);
			ChunkedList<KeyValuePair<string, bool>> Values = new ChunkedList<KeyValuePair<string, bool>>();
			Block Block;
			KeyValuePair<string, bool>[] Prev;
			bool HasProgress = !(Progress is null);
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
							if (HasProgress)
								await CheckEarlyHints(Result.settings.Progress, Key, Values);

							if (Result.metaData.TryGetValue(Key, out Prev))
								Values.AddRangeFirst(Prev);
							else if (Key == "LOGIN")
								Result.isDynamic = true;

							Result.metaData[Key] = Values.ToArray();
						}

						Values.Clear();
						Key = s2;
						Values.Add(new KeyValuePair<string, bool>(s.Substring(j + 1).Trim(), s.EndsWith("  ")));
					}
				}

				if (!string.IsNullOrEmpty(Key))
				{
					if (HasProgress)
						await CheckEarlyHints(Result.settings.Progress, Key, Values);

					if (Result.metaData.TryGetValue(Key, out Prev))
						Values.AddRangeFirst(Prev);
					else if (Key == "LOGIN")
						Result.isDynamic = true;

					Result.metaData[Key] = Values.ToArray();
					Start++;
				}
			}

			if (HasProgress)
				await Progress.HeaderProcessed();

			Result.elements = await Result.ParseBlocks(Blocks, Start, End);

			if (HasProgress)
				await Progress.BodyProcessed();

			if (!(Result.toInsert is null))
			{
				StringBuilder sb = new StringBuilder();
				int Last = 0;

				foreach (KeyValuePair<int, char> P in Result.toInsert)
				{
					if (P.Key > Last)
						sb.Append(Result.markdownText.Substring(Last, P.Key - Last));

					sb.Append(P.Value);
					Last = P.Key;
				}

				Result.markdownText = sb.ToString();
			}

			return Result;
		}

		private MarkdownDocument(string MarkdownText, bool IsDynamic, MarkdownSettings Settings, string FileName, string ResourceName, string URL,
			params Type[] TransparentExceptionTypes)
		{
			this.markdownText = MarkdownText?.Replace("\r\n", "\n").Replace('\r', '\n') ?? string.Empty;
			this.isDynamic = IsDynamic;
			this.emojiSource = Settings.EmojiSource;
			this.settings = Settings;
			this.fileName = FileName;
			this.resourceName = ResourceName;
			this.url = URL;
			this.transparentExceptionTypes = TransparentExceptionTypes;
		}

		private static async Task CheckEarlyHints(ICodecProgress Progress, string Key,
			IEnumerable<KeyValuePair<string, bool>> Values)
		{
			switch (Key)
			{
				case "JAVASCRIPT":
					foreach (KeyValuePair<string, bool> P in Values)
					{
						await Progress.EarlyHint(P.Key, "preload",
							new KeyValuePair<string, string>("as", "script"));
					}
					break;

				case "CSS":
					foreach (KeyValuePair<string, bool> P in Values)
					{
						await Progress.EarlyHint(P.Key, "preload",
							new KeyValuePair<string, string>("as", "style"));
					}
					break;
			}
		}

		/// <summary>
		/// Markdown text. This text might differ slightly from the original text passed to the document.
		/// </summary>
		[Obsolete("Use GenerateMarkdown() instead.")]
		public string MarkdownText
		{
			get
			{
				return this.GenerateMarkdown(false).Result;
			}
		}

		/// <summary>
		/// If an exception is thrown when processing script in markdown, and the exception is of
		/// any of these types, the exception will be rethrown, instead of shown as an error in the generated output.
		/// </summary>
		public Type[] TransparentExceptionTypes => this.transparentExceptionTypes;

		/// <summary>
		/// Gets the end position of the header, if one is found, null otherwise.
		/// </summary>
		/// <param name="Markdown">Markdown</param>
		/// <returns>Position of end of header, if found.</returns>
		public static int? HeaderEndPosition(string Markdown)
		{
			Match M = endOfHeader.Match(Markdown);
			if (!M.Success)
				return null;

			string Header = Markdown.Substring(0, M.Index);
			string[] Rows = Header.Split(CommonTypes.CRLF);
			string s;

			foreach (string Row in Rows)
			{
				s = Row.Trim();
				if (string.IsNullOrEmpty(s))
					continue;

				if (s.IndexOf(':') < 0)
					return null;
			}

			return M.Index;
		}

		/// <summary>
		/// Preprocesses markdown text.
		/// </summary>
		/// <param name="Markdown">Markdown text</param>
		/// <param name="Settings">Markdown settings.</param>
		/// <param name="TransparentExceptionTypes">If an exception is thrown when processing script in markdown, and the exception is of
		/// any of these types, the exception will be rethrown, instead of shown as an error in the generated output.</param>
		/// <returns>Preprocessed markdown.</returns>
		public static async Task<string> Preprocess(string Markdown, MarkdownSettings Settings, params Type[] TransparentExceptionTypes)
		{
			KeyValuePair<string, bool> P = await Preprocess(Markdown, Settings, string.Empty, false, TransparentExceptionTypes);
			return P.Key;
		}

		/// <summary>
		/// Preprocesses markdown text.
		/// </summary>
		/// <param name="Markdown">Markdown text</param>
		/// <param name="Settings">Markdown settings.</param>
		/// <param name="FileName">Filename of markdown.</param>
		/// <param name="TransparentExceptionTypes">If an exception is thrown when processing script in markdown, and the exception is of
		/// any of these types, the exception will be rethrown, instead of shown as an error in the generated output.</param>
		/// <returns>Preprocessed markdown, and if the markdown contains script, making the markdown dynamic.</returns>
		public static Task<KeyValuePair<string, bool>> Preprocess(string Markdown, MarkdownSettings Settings, string FileName, params Type[] TransparentExceptionTypes)
		{
			return Preprocess(Markdown, Settings, FileName, false, TransparentExceptionTypes);
		}

		/// <summary>
		/// Preprocesses markdown text.
		/// </summary>
		/// <param name="Markdown">Markdown text</param>
		/// <param name="Settings">Markdown settings.</param>
		/// <param name="FileName">Filename of markdown.</param>
		/// <param name="FromScript">If call is made from script. If true, method will assumed the variables collection is properly
		/// locked from the caller of the original script.</param>
		/// <param name="TransparentExceptionTypes">If an exception is thrown when processing script in markdown, and the exception is of
		/// any of these types, the exception will be rethrown, instead of shown as an error in the generated output.</param>
		/// <returns>Preprocessed markdown, and if the markdown contains script, making the markdown dynamic.</returns>
		public static async Task<KeyValuePair<string, bool>> Preprocess(string Markdown, MarkdownSettings Settings, string FileName, bool FromScript, params Type[] TransparentExceptionTypes)
		{
			if (Settings.Variables is null)
				Settings.Variables = new Variables();

			Variables Variables = Settings.Variables;
			Expression Exp;
			string Script, s2;
			int i, j;
			bool IsDynamic = false;

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
							string Tag = M2.Groups["Tag"].Value.ToUpper();
							string FileName2 = M2.Groups["ScriptFile"].Value;

							FileName2 = Settings.GetFileName(FileName, FileName2);

							if (Tag == "INIT" && !await InitScriptFile.NeedsExecution(FileName2))
								continue;

							try
							{
								Script = await Files.ReadAllTextAsync(FileName2);

								if (!IsDynamic)
								{
									IsDynamic = true;
									Variables.Add(MarkdownSettingsVariableName, Settings);
								}

								Exp = new Expression(Script, FileName2);

								if (!(Settings.AuthorizeExpression is null))
								{
									ScriptNode Prohibited = await Settings.AuthorizeExpression(Exp);
									if (!(Prohibited is null))
										throw new UnauthorizedAccessException("Expression not permitted: " + Prohibited.SubExpression);
								}

								await Exp.EvaluateAsync(Variables);
							}
							catch (Exception ex)
							{
								Log.Exception(ex, FileName2);
							}
						}
					}
				}
			}

			i = Markdown.IndexOf("{{");
			if (i < 0)
				return new KeyValuePair<string, bool>(Markdown, IsDynamic);

			StringBuilder Transformed = new StringBuilder();
			int From = 0;
			bool UsesImplicitPrint = false;
			bool HasImplicitPrint = false;
			object Result;

			while (i >= 0)
			{
				j = Markdown.IndexOf("}}", i + 2);
				if (j < 0)
				{
					if (From == 0)
						return new KeyValuePair<string, bool>(Markdown, IsDynamic);
					else
						break;
				}

				if (i > From)
					Transformed.Append(Markdown.Substring(From, i - From));

				From = j + 2;
				Script = Markdown.Substring(i + 2, j - i - 2);

				try
				{
					Exp = new Expression(Script, FileName);

					if (!(Settings.AuthorizeExpression is null))
					{
						ScriptNode Prohibited = await Settings.AuthorizeExpression(Exp);
						if (!(Prohibited is null))
							throw new UnauthorizedAccessException("Expression not permitted: " + Prohibited.SubExpression);
					}

					if (!IsDynamic)
					{
						IsDynamic = true;
						Variables.Add(MarkdownSettingsVariableName, Settings);
					}

					HasImplicitPrint = Exp.ContainsImplicitPrint;
					if (!HasImplicitPrint && UsesImplicitPrint && Exp.ReferencesImplicitPrint(Variables))
						HasImplicitPrint = true;

					if (HasImplicitPrint)
					{
						UsesImplicitPrint = true;

						if (!FromScript)
							await Variables.LockAsync();

						ValuePrinter PrinterBak = Variables.Printer;
						TextWriter Bak = Variables.ConsoleOut;
						StringBuilder sb = new StringBuilder();

						Variables.ConsoleOut = new StringWriter(sb);
						Variables.Printer = PrintMarkdown;
						try
						{
							await Exp.EvaluateAsync(Variables);
						}
						finally
						{
							Variables.ConsoleOut?.Flush();
							Variables.ConsoleOut = Bak;
							Variables.Printer = PrinterBak;

							if (!FromScript)
								Variables.Release();
						}

						Result = sb.ToString();
					}
					else
						Result = await Exp.EvaluateAsync(Variables);
				}
				catch (Exception ex)
				{
					ex = Log.UnnestException(ex);

					Transformed.AppendLine("<font class=\"error\">");

					if (ex is AggregateException ex2)
					{
						foreach (Exception ex3 in ex2.InnerExceptions)
						{
							CheckException(ex3, TransparentExceptionTypes);

							Log.Exception(ex3, FileName);

							Transformed.Append("<p>");
							Transformed.Append(XML.HtmlValueEncode(ex3.Message));
							Transformed.AppendLine("</p>");
						}
					}
					else
					{
						CheckException(ex, TransparentExceptionTypes);

						Log.Exception(ex, FileName);

						Transformed.AppendLine(XML.HtmlValueEncode(ex.Message));
					}

					Transformed.AppendLine("</font>");

					Result = null;
				}

				if (!(Result is null))
				{
					if (!(Result is string s3))
						s3 = await PrintMarkdown(Result, Variables);

					Transformed.Append(s3);
				}

				i = Markdown.IndexOf("{{", From);
			}

			if (From < Markdown.Length)
				Transformed.Append(Markdown.Substring(From));

			return new KeyValuePair<string, bool>(Transformed.ToString(), IsDynamic);
		}

		private static async Task<string> PrintMarkdown(object Value, Variables Variables)
		{
			if (Expression.IsNullOrVoid(Value))
				return string.Empty;

			if (Value.GetType().GetTypeInfo().IsValueType || Value is string)
				return Value.ToString();

			if (Value is XmlDocument ||
				Value is IToMatrix ||
				Value is Graph ||
				Value is PixelInformation ||
				Value is SKImage ||
				Value is MarkdownDocument ||
				Value is MarkdownContent ||
				Value is Exception ||
				Value is IMatrix ||
				Value is Array)
			{
				using (MarkdownRenderer Renderer = new MarkdownRenderer())
				{
					await Renderer.RenderObject(Value, false, Variables);
					return Renderer.ToString();
				}
			}
			else
				return Value.ToString();
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

		private Task<ChunkedList<MarkdownElement>> ParseBlocks(ChunkedList<Block> Blocks)
		{
			return this.ParseBlocks(Blocks, 0, Blocks.Count - 1);
		}

		private async Task<ChunkedList<MarkdownElement>> ParseBlocks(ChunkedList<Block> Blocks, int StartBlock, int EndBlock)
		{
			ChunkedList<MarkdownElement> Elements = new ChunkedList<MarkdownElement>();
			ChunkedList<MarkdownElement> Content;
			ChunkedList<Block> AlignedBlocks;
			Block NextBlock;
			Block Block;
			string[] Rows;
			string s, s2;
			string InitialSectionSeparator = null;
			int BlockIndex;
			int i, j, c, d;
			int Index;
			int SectionNr = 0;
			int InitialNrColumns = 1;
			bool LastHtmlIndent = false;
			bool HasSections = false;

			for (BlockIndex = StartBlock; BlockIndex <= EndBlock; BlockIndex++)
			{
				Block = Blocks[BlockIndex];

				if (Block.Indent > 0)
				{
					if (LastHtmlIndent || Block.Rows[Block.Start].StartsWith("<"))    // HTML allowed to indent.
					{
						LastHtmlIndent = true;
						Block.Indent = 0;
						Content = await this.ParseBlock(Block);
						Elements.AddRange(Content);
						continue;
					}

					c = Block.Indent;
					i = BlockIndex + 1;
					while (i <= EndBlock && (j = Blocks[i].Indent) > 0)
					{
						i++;
						if (j < c)
							c = j;
					}

					if (i == BlockIndex + 1)
						Elements.Add(new CodeBlock(this, Block.Rows, Block.Start, Block.End, c - 1));
					else
					{
						ChunkedList<string> CodeBlock = new ChunkedList<string>();

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

						Elements.Add(new CodeBlock(this, CodeBlock.ToArray(), 0, CodeBlock.Count - 1, c - 1));
						BlockIndex--;
					}
					continue;
				}
				else
				{
					LastHtmlIndent = false;

					if (Block.IsPrefixedBy("```", false))
					{
						s = Block.Rows[Block.Start];
						i = 0;
						foreach (char ch in s)
						{
							if (ch == '`')
								i++;
							else
								break;
						}

						s = s.Substring(0, i);

						i = BlockIndex;
						while (i <= EndBlock &&
							(!(Block = Blocks[i]).Rows[Block.End].StartsWith(s) ||
							(i == BlockIndex && Block.Start == Block.End)))
						{
							i++;
						}

						ChunkedList<string> Code = new ChunkedList<string>();
						bool Complete = true;

						if (i > EndBlock)
						{
							i = EndBlock;
							Complete = false;
						}

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

							if (j == i && Complete)
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

						CodeBlock CodeBlock;

						if (s.StartsWith("base64", StringComparison.CurrentCultureIgnoreCase))
						{
							try
							{
								byte[] Bin = Convert.FromBase64String(Code.Concatenate());
								s2 = Encoding.UTF8.GetString(Bin);

								Rows = s2.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

								CodeBlock = new CodeBlock(this, Rows, 0, Rows.Length - 1, 0, s.Substring(6));
							}
							catch (Exception)
							{
								CodeBlock = new CodeBlock(this, Code.ToArray(), 0, Code.Count - 1, 0, s);
							}
						}
						else
							CodeBlock = new CodeBlock(this, Code.ToArray(), 0, Code.Count - 1, 0, s);

						Elements.Add(CodeBlock);

						if (!this.syntaxHighlighting && !string.IsNullOrEmpty(CodeBlock.Language))
						{
							ICodeContentHtmlRenderer HtmlRenderer = CodeBlock.CodeContentHandler<ICodeContentHtmlRenderer>();
							if (HtmlRenderer is null)
								this.syntaxHighlighting = true;
						}

						BlockIndex = i;
						continue;
					}
				}

				if (Block.IsPrefixedBy(">", false))
				{
					if (Block.IsSuffixedBy("<<") && Block.IsPrefixedBy(">>", false))
					{
						AlignedBlocks = Block.RemovePrefixAndSuffix(">>", 2, "<<");

						while (BlockIndex < EndBlock &&
							(NextBlock = Blocks[BlockIndex + 1]).IsPrefixedBy(">>", false) &&
							NextBlock.IsSuffixedBy("<<"))
						{
							BlockIndex++;
							AlignedBlocks.AddRange(NextBlock.RemovePrefixAndSuffix(">>", 2, "<<"));
						}

						Content = await this.ParseBlocks(AlignedBlocks);

						if (Elements.HasLastItem && Elements.LastItem is CenterAligned CenterAligned)
							CenterAligned.AddChildren(Content);
						else
							Elements.Add(new CenterAligned(this, Content));
					}
					else if (Block.IsSuffixedBy(">>"))
					{
						AlignedBlocks = Block.RemoveSuffix(">>");

						while (BlockIndex < EndBlock &&
							(NextBlock = Blocks[BlockIndex + 1]).IsSuffixedBy(">>"))
						{
							BlockIndex++;
							AlignedBlocks.AddRange(NextBlock.RemoveSuffix(">>"));
						}

						Content = await this.ParseBlocks(AlignedBlocks);

						if (Elements.HasLastItem && Elements.LastItem is RightAligned RightAligned)
							RightAligned.AddChildren(Content);
						else
							Elements.Add(new RightAligned(this, Content));
					}
					else
					{
						Content = await this.ParseBlocks(Block.RemovePrefix(">", 2));

						if (Elements.HasLastItem && Elements.LastItem is BlockQuote BlockQuote)
							BlockQuote.AddChildren(Content);
						else
							Elements.Add(new BlockQuote(this, Content));
					}

					continue;
				}
				else if (Block.IsPrefixedBy("<<", false))
				{
					if (Block.IsSuffixedBy(">>"))
					{
						AlignedBlocks = Block.RemovePrefixAndSuffix("<<", 2, ">>");

						while (BlockIndex < EndBlock &&
							(NextBlock = Blocks[BlockIndex + 1]).IsPrefixedBy("<<", false) &&
							NextBlock.IsSuffixedBy(">>"))
						{
							BlockIndex++;
							AlignedBlocks.AddRange(NextBlock.RemovePrefixAndSuffix("<<", 2, ">>"));
						}

						Content = await this.ParseBlocks(AlignedBlocks);

						if (Elements.HasLastItem && Elements.LastItem is MarginAligned MarginAligned)
							MarginAligned.AddChildren(Content);
						else
							Elements.Add(new MarginAligned(this, Content));
					}
					else
					{
						AlignedBlocks = Block.RemovePrefix("<<", 2);

						while (BlockIndex < EndBlock &&
							(NextBlock = Blocks[BlockIndex + 1]).IsPrefixedBy("<<", false))
						{
							BlockIndex++;
							AlignedBlocks.AddRange(NextBlock.RemovePrefix("<<", 2));
						}

						Content = await this.ParseBlocks(AlignedBlocks);

						if (Elements.HasLastItem && Elements.LastItem is LeftAligned LeftAligned)
							LeftAligned.AddChildren(Content);
						else
							Elements.Add(new LeftAligned(this, Content));
					}

					continue;
				}
				else if (Block.IsSuffixedBy(">>"))
				{
					Content = await this.ParseBlocks(Block.RemoveSuffix(">>"));

					if (Elements.HasLastItem && Elements.LastItem is RightAligned RightAligned)
						RightAligned.AddChildren(Content);
					else
						Elements.Add(new RightAligned(this, Content));

					continue;
				}
				else if (Block.IsPrefixedBy("+>", false))
				{
					Content = await this.ParseBlocks(Block.RemovePrefix("+>", 3));

					if (Elements.HasLastItem && Elements.LastItem is InsertBlocks InsertBlocks)
						InsertBlocks.AddChildren(Content);
					else
						Elements.Add(new InsertBlocks(this, Content));

					continue;
				}
				else if (Block.IsPrefixedBy("->", false))
				{
					Content = await this.ParseBlocks(Block.RemovePrefix("->", 3));

					if (Elements.HasLastItem && Elements.LastItem is DeleteBlocks DeleteBlocks)
						DeleteBlocks.AddChildren(Content);
					else
						Elements.Add(new DeleteBlocks(this, Content));

					continue;
				}
				else if (Block.IsPrefixedBy("//", false))
				{
					string[] Comment = new string[Block.End - Block.Start + 1];

					for (i = Block.Start; i <= Block.End; i++)
						Comment[i] = Block.Rows[i].Substring(2);

					Elements.Add(new CommentBlock(this, Comment));
					continue;
				}
				else if (Block.End == Block.Start && (IsUnderline(Block.Rows[0], '-', true, true) || IsUnderline(Block.Rows[0], '*', true, true)))
				{
					Elements.Add(new HorizontalRule(this, Block.Rows[0]));
					continue;
				}
				else if (Block.End == Block.Start && IsUnderline(Block.Rows[0], '=', true, false))
				{
					int NrColumns = Block.Rows[0].Split(whiteSpace, StringSplitOptions.RemoveEmptyEntries).Length;
					HasSections = true;

					if (!Elements.HasFirstItem)
					{
						InitialNrColumns = NrColumns;
						InitialSectionSeparator = Block.Rows[0];
					}
					else
						Elements.Add(new SectionSeparator(this, ++SectionNr, NrColumns, Block.Rows[0]));

					continue;
				}
				else if (Block.End == Block.Start && IsUnderline(Block.Rows[0], '~', false, false))
				{
					Elements.Add(new InvisibleBreak(this, Block.Rows[0]));
					continue;
				}
				else if (Block.IsPrefixedBy(s2 = "*", true) ||
					Block.IsPrefixedBy(s2 = "+", true) ||
					Block.IsPrefixedBy(s2 = "-", true))
				{
					ChunkedList<Block> Segments = null;
					i = 0;
					c = Block.End;

					for (d = Block.Start + 1; d <= c; d++)
					{
						s = Block.Rows[d];
						if (IsPrefixedBy(s, s2, true))
						{
							if (Segments is null)
								Segments = new ChunkedList<Block>();

							Segments.Add(new Block(Block.Rows, Block.Positions, 0, i, d - 1));
							i = d;
						}
					}

					Segments?.Add(new Block(Block.Rows, Block.Positions, 0, i, c));

					ChunkedList<MarkdownElement> Items;
					UnnumberedItem LastItem;

					if (Segments is null)
					{
						ChunkedList<Block> SubBlocks = Block.RemovePrefix(s2, 4);

						while (BlockIndex < EndBlock && (Block = Blocks[BlockIndex + 1]).Indent > 1)
						{
							BlockIndex++;
							Block.Indent--;
							SubBlocks.Add(Block);
						}

						Items = await this.ParseBlocks(SubBlocks);
						LastItem = new UnnumberedItem(this, s2, new NestedBlock(this, Items));

						if (Elements.HasLastItem && Elements.LastItem is BulletList BulletList)
							BulletList.AddChild(LastItem);
						else
							Elements.Add(new BulletList(this, LastItem));
					}
					else
					{
						Items = await this.ParseUnnumberedItems(Segments, s2);

						if (Elements.HasLastItem && Elements.LastItem is BulletList BulletList)
							BulletList.AddChildren(Items);
						else
							Elements.Add(new BulletList(this, Items));

						if (Items.HasLastItem)
							LastItem = Items.LastItem as UnnumberedItem;
						else
							LastItem = null;
					}

					if (!(LastItem is null))
					{
						i = BlockIndex;
						while (BlockIndex < EndBlock && (Block = Blocks[BlockIndex + 1]).Indent > 0)
						{
							BlockIndex++;
							Block.Indent--;
						}

						if (BlockIndex > i)
						{
							Items = await this.ParseBlocks(Blocks, i + 1, BlockIndex);

							if (LastItem.Child is NestedBlock LastItemChildren)
							{
								if (LastItemChildren.IsBlockElement)
									LastItemChildren.AddChildren(Items);
								else
								{
									Items.AddFirstItem(new Paragraph(this, LastItemChildren.Children, true));
									LastItem.Child = new NestedBlock(this, Items);
								}
							}
							else
							{
								if (LastItem.Child.IsBlockElement)
									Items.AddFirstItem(LastItem.Child);
								else
									Items.AddFirstItem(new Paragraph(this, new ChunkedList<MarkdownElement>(LastItem.Child), true));

								LastItem.Child = new NestedBlock(this, Items);
							}
						}
					}

					continue;
				}
				else if (Block.IsPrefixedBy("#.", true))
				{
					ChunkedList<Tuple<int, bool, Block>> Segments = null;
					i = 0;
					c = Block.End;
					int Index2 = 1;
					bool Explicit = false;

					for (d = Block.Start + 1; d <= c; d++)
					{
						s = Block.Rows[d];
						if (IsPrefixedByNumber(s, out j))
						{
							if (Segments is null)
								Segments = new ChunkedList<Tuple<int, bool, Block>>();

							Segments.Add(new Tuple<int, bool, Block>(Index2, Explicit, new Block(Block.Rows, Block.Positions, 0, i, d - 1)));
							i = d;
							Index2 = j;
							Explicit = true;
						}
						else if (IsPrefixedBy(s, "#.", true))
						{
							if (Segments is null)
								Segments = new ChunkedList<Tuple<int, bool, Block>>();

							Segments.Add(new Tuple<int, bool, Block>(Index2, Explicit, new Block(Block.Rows, Block.Positions, 0, i, d - 1)));
							i = d;
							Index2++;
							Explicit = false;
						}
					}

					Segments?.Add(new Tuple<int, bool, Block>(Index2, Explicit, new Block(Block.Rows, Block.Positions, 0, i, c)));

					ChunkedList<MarkdownElement> Items;
					NumberedItem LastItem;

					if (Segments is null)
					{
						ChunkedList<Block> SubBlocks = Block.RemovePrefix("#.", 4);

						while (BlockIndex < EndBlock && (Block = Blocks[BlockIndex + 1]).Indent > 1)
						{
							BlockIndex++;
							Block.Indent--;
							SubBlocks.Add(Block);
						}

						Items = await this.ParseBlocks(SubBlocks);
						LastItem = new NumberedItem(this, Index2, Explicit, new NestedBlock(this, Items));

						if (Elements.HasLastItem && Elements.LastItem is NumberedList NumberedList)
							NumberedList.AddChild(LastItem);
						else
							Elements.Add(new NumberedList(this, LastItem));
					}
					else
					{
						Items = await this.ParseNumberedItems(Segments);

						if (Elements.HasLastItem && Elements.LastItem is NumberedList NumberedList)
							NumberedList.AddChildren(Items);
						else
							Elements.Add(new NumberedList(this, Items));

						if (Items.HasLastItem)
							LastItem = Items.LastItem as NumberedItem;
						else
							LastItem = null;
					}

					if (!(LastItem is null))
					{
						i = BlockIndex;
						while (BlockIndex < EndBlock && (Block = Blocks[BlockIndex + 1]).Indent > 0)
						{
							BlockIndex++;
							Block.Indent--;
						}

						if (BlockIndex > i)
						{
							Items = await this.ParseBlocks(Blocks, i + 1, BlockIndex);

							if (LastItem.Child is NestedBlock LastItemChildren)
							{
								if (LastItemChildren.IsBlockElement)
									LastItemChildren.AddChildren(Items);
								else
								{
									Items.AddFirstItem(new Paragraph(this, LastItemChildren.Children, true));
									LastItem.Child = new NestedBlock(this, Items);
								}
							}
							else
							{
								if (LastItem.Child.IsBlockElement)
									Items.AddFirstItem(LastItem.Child);
								else
									Items.AddFirstItem(new Paragraph(this, new ChunkedList<MarkdownElement>(LastItem.Child), true));

								LastItem.Child = new NestedBlock(this, Items);
							}
						}
					}

					continue;
				}
				else if (Block.IsPrefixedBy(s2 = "[ ]", true) ||
					Block.IsPrefixedBy(s2 = "[x]", true) ||
					Block.IsPrefixedBy(s2 = "[X]", true))
				{
					ChunkedList<Tuple<Block, string, int>> Segments = null;
					int CheckPosition = Block.Positions[0] + 1;
					string s3;
					i = 0;
					c = Block.End;

					for (d = Block.Start + 1; d <= c; d++)
					{
						s = Block.Rows[d];
						if (IsPrefixedBy(s, s3 = "[ ]", true) ||
							IsPrefixedBy(s, s3 = "[x]", true) ||
							IsPrefixedBy(s, s3 = "[X]", true))
						{
							if (Segments is null)
								Segments = new ChunkedList<Tuple<Block, string, int>>();

							Segments.Add(new Tuple<Block, string, int>(new Block(Block.Rows, Block.Positions, 0, i, d - 1), s2, CheckPosition));
							s2 = s3;
							i = d;
							CheckPosition = Block.Positions[d] + 1;
						}
					}

					Segments?.Add(new Tuple<Block, string, int>(new Block(Block.Rows, Block.Positions, 0, i, c), s2, CheckPosition));

					ChunkedList<MarkdownElement> Items;
					TaskItem LastItem;

					if (Segments is null)
					{
						ChunkedList<Block> SubBlocks = Block.RemovePrefix(s2, 4);

						while (BlockIndex < EndBlock && (Block = Blocks[BlockIndex + 1]).Indent > 1)
						{
							BlockIndex++;
							Block.Indent--;
							SubBlocks.Add(Block);
						}

						Items = await this.ParseBlocks(SubBlocks);
						LastItem = new TaskItem(this, s2 != "[ ]", CheckPosition, new NestedBlock(this, Items));

						if (Elements.HasLastItem && Elements.LastItem is TaskList TaskList)
							TaskList.AddChild(LastItem);
						else
							Elements.Add(new TaskList(this, LastItem));
					}
					else
					{
						Items = await this.ParseTaskItems(Segments);

						if (Elements.HasLastItem && Elements.LastItem is TaskList TaskList)
							TaskList.AddChildren(Items);
						else
							Elements.Add(new TaskList(this, Items));

						if (Items.HasLastItem)
							LastItem = Items.LastItem as TaskItem;
						else
							LastItem = null;
					}

					if (!(LastItem is null))
					{
						i = BlockIndex;
						while (BlockIndex < EndBlock && (Block = Blocks[BlockIndex + 1]).Indent > 0)
						{
							BlockIndex++;
							Block.Indent--;
						}

						if (BlockIndex > i)
						{
							Items = await this.ParseBlocks(Blocks, i + 1, BlockIndex);

							if (LastItem.Child is NestedBlock LastItemChildren)
							{
								if (LastItemChildren.IsBlockElement)
									LastItemChildren.AddChildren(Items);
								else
								{
									Items.AddFirstItem(new Paragraph(this, LastItemChildren.Children, true));
									LastItem.Child = new NestedBlock(this, Items);
								}
							}
							else
							{
								if (LastItem.Child.IsBlockElement)
									Items.AddFirstItem(LastItem.Child);
								else
									Items.AddFirstItem(new Paragraph(this, new ChunkedList<MarkdownElement>(LastItem.Child), true));

								LastItem.Child = new NestedBlock(this, Items);
							}
						}
					}

					continue;
				}
				else if (Block.IsPrefixedByNumber(out Index))
				{
					ChunkedList<Tuple<int, bool, Block>> Segments = null;
					i = 0;
					c = Block.End;
					bool Explicit = true;

					for (d = Block.Start + 1; d <= c; d++)
					{
						s = Block.Rows[d];
						if (IsPrefixedByNumber(s, out j))
						{
							if (Segments is null)
								Segments = new ChunkedList<Tuple<int, bool, Block>>();

							Segments.Add(new Tuple<int, bool, Block>(Index, Explicit, new Block(Block.Rows, Block.Positions, 0, i, d - 1)));
							i = d;
							Index = j;
							Explicit = true;
						}
						else if (IsPrefixedBy(s, "#.", true))
						{
							if (Segments is null)
								Segments = new ChunkedList<Tuple<int, bool, Block>>();

							Segments.Add(new Tuple<int, bool, Block>(Index, Explicit, new Block(Block.Rows, Block.Positions, 0, i, d - 1)));
							i = d;
							Index++;
							Explicit = false;
						}
					}

					Segments?.Add(new Tuple<int, bool, Block>(Index, Explicit, new Block(Block.Rows, Block.Positions, 0, i, c)));

					ChunkedList<MarkdownElement> Items;
					NumberedItem LastItem;

					if (Segments is null)
					{
						s = Index.ToString();
						ChunkedList<Block> SubBlocks = Block.RemovePrefix(s + ".", Math.Max(4, s.Length + 2));

						while (BlockIndex < EndBlock && (Block = Blocks[BlockIndex + 1]).Indent > 1)
						{
							BlockIndex++;
							Block.Indent--;
							SubBlocks.Add(Block);
						}

						Items = await this.ParseBlocks(SubBlocks);
						LastItem = new NumberedItem(this, Index, Explicit, new NestedBlock(this, Items));

						if (Elements.HasLastItem && Elements.LastItem is NumberedList NumberedList)
							NumberedList.AddChild(LastItem);
						else
							Elements.Add(new NumberedList(this, LastItem));
					}
					else
					{
						Items = await this.ParseNumberedItems(Segments);

						if (Elements.HasLastItem && Elements.LastItem is NumberedList NumberedList)
							NumberedList.AddChildren(Items);
						else
							Elements.Add(new NumberedList(this, Items));

						if (Items.HasLastItem)
							LastItem = Items.LastItem as NumberedItem;
						else
							LastItem = null;
					}

					if (!(LastItem is null))
					{
						i = BlockIndex;
						while (BlockIndex < EndBlock && (Block = Blocks[BlockIndex + 1]).Indent > 0)
						{
							BlockIndex++;
							Block.Indent--;
						}

						if (BlockIndex > i)
						{
							Items = await this.ParseBlocks(Blocks, i + 1, BlockIndex);

							if (LastItem.Child is NestedBlock LastItemChildren)
							{
								if (LastItemChildren.IsBlockElement)
									LastItemChildren.AddChildren(Items);
								else
								{
									Items.AddFirstItem(new Paragraph(this, LastItemChildren.Children, true));
									LastItem.Child = new NestedBlock(this, Items);
								}
							}
							else
							{
								if (LastItem.Child.IsBlockElement)
									Items.AddFirstItem(LastItem.Child);
								else
									Items.AddFirstItem(new Paragraph(this, new ChunkedList<MarkdownElement>(LastItem.Child), true));

								LastItem.Child = new NestedBlock(this, Items);
							}
						}
					}

					continue;
				}
				else if (Block.IsTable(out TableInformation TableInformation))
				{
					MarkdownElement[][] Headers = new MarkdownElement[TableInformation.NrHeaderRows][];
					MarkdownElement[][] DataRows = new MarkdownElement[TableInformation.NrDataRows][];
					TextAlignment?[][] HeaderCellAlignments = new TextAlignment?[TableInformation.NrHeaderRows][];
					TextAlignment?[][] DataCellAlignments = new TextAlignment?[TableInformation.NrDataRows][];
					ChunkedList<MarkdownElement> CellElements;
					string[] Row;
					int[] Positions;

					c = TableInformation.Columns;

					for (j = 0; j < TableInformation.NrHeaderRows; j++)
					{
						Row = TableInformation.Headers[j];
						Positions = TableInformation.HeaderPositions[j];

						Headers[j] = new MarkdownElement[c];
						HeaderCellAlignments[j] = new TextAlignment?[c];

						for (i = 0; i < c; i++)
						{
							s = Row[i];
							if (s is null)
							{
								Headers[j][i] = null;
								HeaderCellAlignments[j][i] = null;
							}
							else
							{
								CellElements = await this.ParseCell(Row[i], Positions[i], out HeaderCellAlignments[j][i]);

								if (CellElements.Count == 1)
								{
									if (CellElements.FirstItem is FootnoteReference FRef)
									{
										FRef.AutoExpand = true;

										if (this.footnotes.TryGetValue(FRef.Key, out Footnote Note))
											Note.TableCellContents = true;

										if (this.footnoteNumberByKey.TryGetValue(FRef.Key, out int Nr) &&
											Nr == this.lastFootnote)
										{
											this.footnoteNumberByKey.Remove(FRef.Key);
											this.lastFootnote--;
										}
									}

									Headers[j][i] = CellElements.FirstItem;
								}
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
						DataCellAlignments[j] = new TextAlignment?[c];

						for (i = 0; i < c; i++)
						{
							s = Row[i];
							if (s is null)
							{
								DataRows[j][i] = null;
								DataCellAlignments[j][i] = null;
							}
							else
							{
								CellElements = await this.ParseCell(Row[i], Positions[i], out DataCellAlignments[j][i]);

								if (CellElements.Count == 1)
								{
									if (CellElements.FirstItem is FootnoteReference FRef)
									{
										FRef.AutoExpand = true;

										if (this.footnotes.TryGetValue(FRef.Key, out Footnote Note))
											Note.TableCellContents = true;

										if (this.footnoteNumberByKey.TryGetValue(FRef.Key, out int Nr) &&
											Nr == this.lastFootnote)
										{
											this.footnoteNumberByKey.Remove(FRef.Key);
											this.lastFootnote--;
										}
									}

									DataRows[j][i] = CellElements.FirstItem;
								}
								else
									DataRows[j][i] = new NestedBlock(this, CellElements);
							}
						}
					}

					Elements.Add(new Table(this, c, Headers, DataRows, TableInformation.Alignments, TableInformation.AlignmentDefinitions,
						HeaderCellAlignments, DataCellAlignments, TableInformation.Caption, TableInformation.Id));

					continue;
				}
				else if (Block.IsPrefixedBy(":", true) && Elements.HasLastItem)
				{
					ChunkedList<MarkdownElement> Description = await this.ParseBlocks(Block.RemovePrefix(":", 4));
					DefinitionDescriptions DefinitionDescriptions;

					i = BlockIndex;
					while (BlockIndex < EndBlock && (Block = Blocks[BlockIndex + 1]).Indent > 0)
					{
						BlockIndex++;
						Block.Indent--;
					}

					if (BlockIndex > i)
						Description.AddRange(await this.ParseBlocks(Blocks, i + 1, BlockIndex));

					if (!Description.HasFirstItem)
						continue;

					if (Description.Count == 1)
						DefinitionDescriptions = new DefinitionDescriptions(this, Description);
					else
						DefinitionDescriptions = new DefinitionDescriptions(this, new NestedBlock(this, Description));

					if (Elements.HasLastItem && Elements.LastItem is DefinitionDescriptions DefinitionDescriptions2)
						DefinitionDescriptions2.AddChildren(DefinitionDescriptions.Children);
					else if (Elements.HasLastItem && Elements.LastItem is DefinitionTerms DefinitionTerms)
						Elements.LastItem = new DefinitionList(this, DefinitionTerms, DefinitionDescriptions);
					else if (Elements.HasLastItem && Elements.LastItem is DefinitionList DefinitionList)
						DefinitionList.AddChild(DefinitionDescriptions);
					else
						Elements.Add(new DefinitionList(this, DefinitionDescriptions));

					continue;
				}
				else if (BlockIndex < EndBlock && Blocks[BlockIndex + 1].IsPrefixedBy(":", true))
				{
					ChunkedList<MarkdownElement> Terms = new ChunkedList<MarkdownElement>();
					ChunkedList<MarkdownElement> Term;

					Rows = Block.Rows;
					c = Block.End;
					for (i = Block.Start; i <= c; i++)
					{
						Term = await this.ParseBlock(Rows, Block.Positions, i, i);
						if (!Term.HasFirstItem)
							continue;

						if (Term.Count == 1)
							Terms.Add(Term.FirstItem);
						else
							Terms.Add(new NestedBlock(this, Term));
					}

					if (Elements.HasLastItem && Elements.LastItem is DefinitionList DefinitionList)
						DefinitionList.AddChild(new DefinitionTerms(this, Terms));
					else
						Elements.Add(new DefinitionTerms(this, Terms));

					continue;
				}
				else if (Block.IsFootnote(out s, out int WhiteSparePrefix))
				{
					Footnote Footnote = new Footnote(this, s,
						await this.ParseBlocks(Block.RemovePrefix(string.Empty, WhiteSparePrefix)));

					i = BlockIndex;
					while (BlockIndex < EndBlock && (Block = Blocks[BlockIndex + 1]).Indent > 0)
					{
						BlockIndex++;
						Block.Indent--;
					}

					if (BlockIndex > i)
						Footnote.AddChildren(await this.ParseBlocks(Blocks, i + 1, BlockIndex));

					if (this.footnoteNumberByKey is null)
					{
						this.footnoteNumberByKey = new Dictionary<string, int>();
						this.footnoteOrder = new ChunkedList<string>();
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

					if (IsUnderline(s, '=', false, false))
					{
						Header Header = new Header(this, 1, false, s, this.PrepareHeader(await this.ParseBlock(Rows, Block.Positions, 0, c - 1)));
						Elements.Add(Header);
						this.headers.Add(Header);
						continue;
					}
					else if (IsUnderline(s, '-', false, false))
					{
						Header Header = new Header(this, 2, false, s, this.PrepareHeader(await this.ParseBlock(Rows, Block.Positions, 0, c - 1)));
						Elements.Add(Header);
						this.headers.Add(Header);
						continue;
					}
				}

				s = Rows[Block.Start];
				if (IsPrefixedBy(s, '#', out d, true) && d < s.Length)
				{
					string Prefix = s.Substring(0, d);
					Rows[Block.Start] = s.Substring(d).Trim();

					s = Rows[c];
					i = s.Length - 1;
					while (i >= 0 && s[i] == '#')
						i--;

					if (++i < s.Length)
						Rows[c] = s.Substring(0, i).TrimEnd();

					Header Header = new Header(this, d, true, Prefix, this.PrepareHeader(await this.ParseBlock(Rows, Block.Positions, Block.Start, c)));
					Elements.Add(Header);
					this.headers.Add(Header);
					continue;
				}

				KeyValuePair<ChunkedList<MarkdownElement>, int> P = await this.ParseBlock(Block, Blocks, BlockIndex, EndBlock);
				Content = P.Key;
				BlockIndex = P.Value;

				if (Content.HasFirstItem)
				{
					if (Content.HasFirstItem &&
						Content.HasLastItem &&
						Content.FirstItem is InlineHTML &&
						Content.LastItem is InlineHTML &&
						this.settings.AllowHtml)
					{
						Elements.Add(new HtmlBlock(this, Content));
					}
					else if (Content.Count == 1 && Content.FirstItem.OutsideParagraph)
					{
						if (Content.HasFirstItem &&
							Content.HasLastItem &&
							Content.FirstItem is MarkdownElementChildren MarkdownElementChildren &&
							MarkdownElementChildren.JoinOverParagraphs &&
							Elements.LastItem is MarkdownElementChildren MarkdownElementChildrenLast)
						{
							MarkdownElementChildrenLast.AddChildren(MarkdownElementChildren.Children);
						}
						else
							Elements.Add(Content.FirstItem);
					}
					else
						Elements.Add(new Paragraph(this, Content));
				}
			}

			if (HasSections)
			{
				ChunkedList<MarkdownElement> Sections = new ChunkedList<MarkdownElement>();
				Sections.Add(new Sections(this, InitialNrColumns, InitialSectionSeparator, Elements));
				return Sections;
			}
			else
				return Elements;
		}

		private async Task<ChunkedList<MarkdownElement>> ParseUnnumberedItems(
			ChunkedList<Block> Segments, string Prefix)
		{
			ChunkedList<MarkdownElement> Items = new ChunkedList<MarkdownElement>();
			ChunkNode<Block> Loop = Segments.FirstChunk;
			ChunkNode<Block> Loop2;

			while (!(Loop is null))
			{
				for (int i = Loop.Start, c = Loop.Pos; i < c; i++)
				{
					Loop2 = Loop[i].RemovePrefix(Prefix, 4).FirstChunk;

					while (!(Loop2 is null))
					{
						for (int j = Loop2.Start, d = Loop2.Pos; j < d; j++)
						{
							Items.Add(new UnnumberedItem(this, Prefix, new NestedBlock(this,
								await this.ParseBlock(Loop2[j]))));
						}

						Loop2 = Loop2.Next;
					}
				}

				Loop = Loop.Next;
			}

			return Items;
		}

		private async Task<ChunkedList<MarkdownElement>> ParseNumberedItems(
			ChunkedList<Tuple<int, bool, Block>> Segments)
		{
			ChunkedList<MarkdownElement> Items = new ChunkedList<MarkdownElement>();
			ChunkNode<Tuple<int, bool, Block>> Loop = Segments.FirstChunk;
			ChunkNode<Block> Loop2;
			string s;

			while (!(Loop is null))
			{
				for (int i = Loop.Start, c = Loop.Pos; i < c; i++)
				{
					Tuple<int, bool, Block> Segment = Loop[i];

					s = Segment.Item2 ? Segment.Item1.ToString() + "." : "#.";
					Loop2 = Segment.Item3.RemovePrefix(s, Math.Max(4, s.Length + 2)).FirstChunk;

					while (!(Loop2 is null))
					{
						for (int j = Loop2.Start, d = Loop2.Pos; j < d; j++)
						{
							Items.Add(new NumberedItem(this, Segment.Item1, Segment.Item2,
								new NestedBlock(this, await this.ParseBlock(Loop2[j]))));
						}

						Loop2 = Loop2.Next;
					}
				}

				Loop = Loop.Next;
			}

			return Items;
		}

		private async Task<ChunkedList<MarkdownElement>> ParseTaskItems(ChunkedList<Tuple<Block, string, int>> Segments)
		{
			ChunkedList<MarkdownElement> Items = new ChunkedList<MarkdownElement>();
			ChunkNode<Tuple<Block, string, int>> Loop = Segments.FirstChunk;
			ChunkNode<Block> Loop2;

			while (!(Loop is null))
			{
				for (int i = Loop.Start, c = Loop.Pos; i < c; i++)
				{
					Tuple<Block, string, int> Segment = Loop[i];

					Loop2 = Segment.Item1.RemovePrefix(Segment.Item2, 4).FirstChunk;

					while (!(Loop2 is null))
					{
						for (int j = Loop2.Start, d = Loop2.Pos; j < d; j++)
						{
							Items.Add(new TaskItem(this, Segment.Item2 != "[ ]", Segment.Item3,
								new NestedBlock(this, await this.ParseBlock(Loop2[j]))));
						}

						Loop2 = Loop2.Next;
					}
				}

				Loop = Loop.Next;
			}

			return Items;
		}

		private ChunkedList<MarkdownElement> PrepareHeader(ChunkedList<MarkdownElement> Content)
		{
			if (Content?.FirstItem is NumberedList NumberedList &&
				Content.Count == 1 &&
				NumberedList.HasOneChild &&
				NumberedList.FirstChild is NumberedItem Item &&
				Item.NumberExplicit)
			{
				ChunkedList<MarkdownElement> NewContent = new ChunkedList<MarkdownElement>
				{
					new InlineText(this, Item.Number.ToString() + ". ")
				};

				if (Item.Child is NestedBlock B)
					NewContent.AddRange(B.Children);
				else
					NewContent.Add(Item.Child);

				return NewContent;
			}
			else
				return Content;
		}

		private Task<ChunkedList<MarkdownElement>> ParseCell(string Cell, int Position, out TextAlignment? Alignment)
		{
			if (Cell.StartsWith("<<"))
			{
				Position += 2;

				if (Cell.EndsWith(">>"))
				{
					Alignment = TextAlignment.Center;
					Cell = Cell.Substring(2, Cell.Length - 4);
				}
				else
				{
					Alignment = TextAlignment.Left;
					Cell = Cell.Substring(2);
				}
			}
			else if (Cell.StartsWith(">>") && Cell.EndsWith("<<"))
			{
				Alignment = TextAlignment.Center;
				Cell = Cell.Substring(2, Cell.Length - 4);
				Position += 2;
			}
			else if (Cell.EndsWith(">>"))
			{
				Alignment = TextAlignment.Right;
				Cell = Cell.Substring(0, Cell.Length - 2);
			}
			else
				Alignment = null;

			return this.ParseBlock(new string[] { Cell }, new int[] { Position });
		}

		private async Task<ChunkedList<MarkdownElement>> ParseBlock(string[] Rows, int[] Positions)
		{
			return (await this.ParseBlock(Rows, Positions, 0, Rows.Length - 1, null, 0, 0)).Key;
		}

		private async Task<ChunkedList<MarkdownElement>> ParseBlock(Block Block)
		{
			return (await this.ParseBlock(Block.Rows, Block.Positions, Block.Start, Block.End, null, 0, 0)).Key;
		}

		private Task<KeyValuePair<ChunkedList<MarkdownElement>, int>> ParseBlock(Block Block, ChunkedList<Block> Blocks, int BlockIndex, int EndBlock)
		{
			return this.ParseBlock(Block.Rows, Block.Positions, Block.Start, Block.End, Blocks, BlockIndex, EndBlock);
		}

		private async Task<ChunkedList<MarkdownElement>> ParseBlock(string[] Rows, int[] Positions, int StartRow, int EndRow)
		{
			return (await this.ParseBlock(Rows, Positions, StartRow, EndRow, null, 0, 0)).Key;
		}

		private async Task<KeyValuePair<ChunkedList<MarkdownElement>, int>> ParseBlock(string[] Rows, int[] Positions, int StartRow, int EndRow, ChunkedList<Block> Blocks,
			int BlockIndex, int EndBlock)
		{
			ChunkedList<MarkdownElement> Elements = new ChunkedList<MarkdownElement>();
			bool PreserveCrLf = Rows[StartRow].StartsWith("<") && Rows[EndRow].EndsWith(">");
			BlockParseState State = new BlockParseState(Rows, Positions, StartRow, EndRow, PreserveCrLf, Blocks, BlockIndex, EndBlock);

			await this.ParseBlock(State, (char)0, 1, Elements, true);

			return new KeyValuePair<ChunkedList<MarkdownElement>, int>(Elements, State.BlockIndex);
		}

		private async Task<bool> ParseBlock(BlockParseState State, char TerminationCharacter, int TerminationCharacterCount,
			ChunkedList<MarkdownElement> Elements, bool AcceptIncomplete)
		{
			ChunkedList<MarkdownElement> ChildElements;
			StringBuilder Text = new StringBuilder();
			string Url, Title;
			char ch, ch2, ch3;
			char PrevChar = ' ';
			int? Width;
			int? Height;
			bool FirstCharOnLine;

			while ((ch = State.NextChar()) != (char)0)
			{
				if (ch == TerminationCharacter)
				{
					if (TerminationCharacterCount == 1 ||
						State.CheckRestOfTermination(TerminationCharacter, TerminationCharacterCount - 1))
					{
						break;
					}
				}

				switch (ch)
				{
					case '\n':
						this.AppendAnyText(Elements, Text);
						Elements.Add(new LineBreak(this));
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
								ChunkedList<string> Rows = new ChunkedList<string>();
								ChunkedList<int> Positions = new ChunkedList<int>()
								{
									State.CurrentPosition
								};

								Rows.Add(State.RestOfRow());

								while (!State.EOF)
								{
									if ((ch2 = State.PeekNextCharSameRow()) == '*' || ch2 == '+' || ch2 == '-')
									{
										Item = new UnnumberedItem(this, new string(ch, 1), new NestedBlock(this, await this.ParseBlock(Rows.ToArray(), Positions.ToArray())));

										if (Elements.HasLastItem && Elements.LastItem is BulletList BulletList)
											BulletList.AddChild(Item);
										else
											Elements.Add(new BulletList(this, Item));

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
									Item = new UnnumberedItem(this, new string(ch, 1), new NestedBlock(this, await this.ParseBlock(Rows.ToArray(), Positions.ToArray())));

									if (Elements.HasLastItem && Elements.LastItem is BulletList BulletList)
										BulletList.AddChild(Item);
									else
										Elements.Add(new BulletList(this, Item));
								}
							}
							else
								Text.Append('*');

							break;
						}

						this.AppendAnyText(Elements, Text);
						ChildElements = new ChunkedList<MarkdownElement>();
						ch2 = State.PeekNextCharSameRow();
						if (ch2 == '*')
						{
							State.NextCharSameRow();

							await this.ParseBlock(State, '*', 2, ChildElements, true);
							Elements.Add(new Strong(this, ChildElements));
						}
						else
						{
							if (this.emojiSource is null)
								ch2 = (char)0;

							switch (ch2)
							{
								case ')':
									State.NextCharSameRow();
									Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_wink));
									break;

								case '-':
									State.BackupState();
									State.NextCharSameRow();

									if (State.PeekNextCharSameRow() == ')')
									{
										State.DiscardBackup();
										State.NextCharSameRow();
										Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_wink));
									}
									else
									{
										State.RestoreState();

										if (await this.ParseBlock(State, '*', 1, ChildElements, TerminationCharacter != '*'))
											Elements.Add(new Emphasize(this, ChildElements));
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
												Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_ok_woman));
												break;
											}
										}
									}

									State.RestoreState();
									if (await this.ParseBlock(State, '*', 1, ChildElements, TerminationCharacter != '*'))
										Elements.Add(new Emphasize(this, ChildElements));
									else
										this.FixSyntaxError(Elements, "*", ChildElements);

									break;

								default:
									if (await this.ParseBlock(State, '*', 1, ChildElements, TerminationCharacter != '*'))
										Elements.Add(new Emphasize(this, ChildElements));
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
						ChildElements = new ChunkedList<MarkdownElement>();
						ch2 = State.PeekNextCharSameRow();
						if (ch2 == '_')
						{
							State.NextCharSameRow();

							await this.ParseBlock(State, '_', 2, ChildElements, true);
							Elements.Add(new Insert(this, ChildElements));
						}
						else
						{
							if (await this.ParseBlock(State, '_', 1, ChildElements, TerminationCharacter != '_'))
								Elements.Add(new Underline(this, ChildElements));
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
						ChildElements = new ChunkedList<MarkdownElement>();
						ch2 = State.PeekNextCharSameRow();
						if (ch2 == '~')
						{
							State.NextCharSameRow();

							await this.ParseBlock(State, '~', 2, ChildElements, true);
							Elements.Add(new Delete(this, ChildElements));
						}
						else
						{
							if (await this.ParseBlock(State, '~', 1, ChildElements, TerminationCharacter != '~'))
								Elements.Add(new StrikeThrough(this, ChildElements));
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

						Elements.Add(new InlineCode(this, Text.ToString()));
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
								Elements.Add(new HtmlEntity(this, "LeftDoubleBracket"));
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
										Elements.Add(new DetailsReference(this, Url));
									else
										Elements.Add(new MetaReference(this, Url));
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

									if (this.footnoteNumberByKey is null)
									{
										this.footnoteNumberByKey = new Dictionary<string, int>();
										this.footnoteOrder = new ChunkedList<string>();
										this.footnotes = new Dictionary<string, Footnote>();
									}

									try
									{
										Title = Url.ToLower();
										Elements.Add(new FootnoteReference(this, XmlConvert.VerifyNCName(Title)));
										if (!this.footnoteNumberByKey.ContainsKey(Title))
										{
											this.footnoteNumberByKey[Title] = ++this.lastFootnote;
											this.footnoteOrder.Add(Title);
										}
									}
									catch
									{
										Title = Guid.NewGuid().ToString();

										Elements.Add(new FootnoteReference(this, Title));
										this.footnoteNumberByKey[Title] = ++this.lastFootnote;
										this.footnoteOrder.Add(Title);
										this.footnotes[Title] = new Footnote(this, Title, new Paragraph(this, await this.ParseBlock(new string[] { Url }, new int[] { State.CurrentPosition - 1 - Url.Length })));
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
							ChunkedList<string> Rows = new ChunkedList<string>()
							{
								State.RestOfRow()
							};
							ChunkedList<int> Positions = new ChunkedList<int>()
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
										new NestedBlock(this, await this.ParseBlock(Rows.ToArray(), Positions.ToArray())));

									if (Elements.HasLastItem && Elements.LastItem is TaskList TaskList)
										TaskList.AddChild(Item);
									else
										Elements.Add(new TaskList(this, Item));

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
									new NestedBlock(this, await this.ParseBlock(Rows.ToArray(), Positions.ToArray())));

								if (Elements.HasLastItem && Elements.LastItem is TaskList TaskList)
									TaskList.AddChild(Item);
								else
									Elements.Add(new TaskList(this, Item));
							}

							break;
						}

						ChildElements = new ChunkedList<MarkdownElement>();
						this.AppendAnyText(Elements, Text);

						if (await this.ParseBlock(State, ']', 1, ChildElements, false))
						{
							ch2 = State.PeekNextNonWhitespaceChar(false);
							if (ch2 == '(')
							{
								State.NextNonWhitespaceChar();
								Title = string.Empty;

								while ((ch2 = State.PeekNextCharSameRow()) != 0 && ch2 > ' ' && ch2 != ')' && ch2 != 160)
								{
									State.NextChar();
									Text.Append(ch2);
								}

								Url = Text.ToString();
								if (Url.StartsWith("abbr:", StringComparison.CurrentCultureIgnoreCase))
								{
									if (ch2 == ')')
										State.NextChar();
									else if (ch2 != 0)
									{
										while ((ch2 = State.NextCharSameRow()) != 0 && ch2 != ')')
											Text.Append(ch2);
									}

									Url = Text.ToString();
									Text.Clear();

									Elements.Add(new Abbreviation(this, ChildElements, Url.Substring(5).Trim()));
								}
								else
								{
									Text.Clear();

									if (Url.StartsWith("<") && Url.EndsWith(">"))
										Url = Url.Substring(1, Url.Length - 2);

									if (ch2 <= ' ' || ch2 == 160)
									{
										ch2 = State.PeekNextNonWhitespaceChar(true);

										if (ch2 == '"' || ch2 == '\'')
										{
											State.NextNonWhitespaceChar();
											while ((ch3 = State.NextCharSameRow()) != 0 && ch3 != ch2)
												Text.Append(ch3);

											Title = Text.ToString();
											Text.Clear();

											ch2 = State.PeekNextNonWhitespaceChar(true);
										}
										else
											Title = string.Empty;
									}

									if (ch == '!' && ch2 != ')')
									{
										ParseWidthHeight(State, out Width, out Height);
										ch2 = State.PeekNextCharSameRow();
									}
									else
										Width = Height = null;

									while (ch2 != 0 && ch2 != ')')
									{
										State.NextCharSameRow();
										ch2 = State.PeekNextCharSameRow();
									}

									if (ch2 == ')')
										State.NextChar();

									if (ch == '!')
									{
										ChunkedList<MultimediaItem> Items = new ChunkedList<MultimediaItem>()
										{
											new MultimediaItem(this, Url, Title, Width, Height)
										};

										if (!this.includesTableOfContents && string.Compare(Url, "ToC", true) == 0)
											this.includesTableOfContents = true;

										State.BackupState();
										ch2 = State.PeekNextNonWhitespaceChar(false);

										while (ch2 == '(')
										{
											State.NextNonWhitespaceChar();
											Title = string.Empty;

											while ((ch2 = State.PeekNextCharSameRow()) != 0 && ch2 > ' ' && ch2 != ')' && ch2 != 160)
											{
												State.NextChar();
												Text.Append(ch2);
											}

											Url = Text.ToString();

											Text.Clear();

											if (Url.StartsWith("<") && Url.EndsWith(">"))
												Url = Url.Substring(1, Url.Length - 2);

											if (ch2 <= ' ' || ch2 == 160)
											{
												ch2 = State.PeekNextNonWhitespaceChar(true);

												if (ch2 == '"' || ch2 == '\'')
												{
													State.NextNonWhitespaceChar();
													while ((ch3 = State.NextCharSameRow()) != 0 && ch3 != ch2)
														Text.Append(ch3);

													Title = Text.ToString();
													Text.Clear();

													ch2 = State.PeekNextNonWhitespaceChar(true);
												}
												else
													Title = string.Empty;
											}

											if (ch2 != ')')
											{
												ParseWidthHeight(State, out Width, out Height);

												ch2 = State.PeekNextCharSameRow();

												while (ch2 != 0 && ch2 != ')')
												{
													State.NextCharSameRow();
													ch2 = State.PeekNextCharSameRow();
												}
											}

											Items.Add(new MultimediaItem(this, Url, Title, Width, Height));

											if (ch2 == ')')
											{
												State.NextChar();
												ch2 = State.PeekNextNonWhitespaceChar(true);
											}

											State.DiscardBackup();
											State.BackupState();
										}

										State.RestoreState();

										Multimedia Multimedia = new Multimedia(this, ChildElements,
											!Elements.HasFirstItem && State.PeekNextChar() == 0,
											Items.ToArray());

										Elements.Add(Multimedia);

										if (!(this.settings?.Progress is null))
										{
											IMultimediaHtmlRenderer Renderer = Multimedia.GetMultimediaHandler<IMultimediaHtmlRenderer>(Multimedia.Items);
											if (!(Renderer is null))
												await Renderer.Preload(this.settings.Progress, Multimedia.Items);
										}
									}
									else
										Elements.Add(new Link(this, ChildElements, Url, Title));
								}
							}
							else if (ch2 == ':' && FirstCharOnLine)
							{
								State.NextNonWhitespaceChar();
								ch2 = State.NextChar();
								while ((ch2 != 0 && ch2 <= ' ') || ch2 == 160)
									ch2 = State.NextChar();

								if (ch2 > ' ' && ch2 != 160)
								{
									ChunkedList<MultimediaItem> Items = new ChunkedList<MultimediaItem>();

									Text.Append(ch2);

									while (ch2 > ' ' && ch2 != 160 && ch2 != '[')
									{
										ch2 = State.NextNonWhitespaceChar();
										while (ch2 != 0 && ch2 > ' ' && ch2 != 160)
										{
											Text.Append(ch2);
											ch2 = State.NextCharSameRow();
										}

										Url = Text.ToString();
										Text.Clear();

										if (Url.StartsWith("<") && Url.EndsWith(">"))
											Url = Url.Substring(1, Url.Length - 2);

										ch2 = State.PeekNextNonWhitespaceChar(true);

										if (ch2 == '"' || ch2 == '\'' || ch2 == '(')
										{
											State.NextNonWhitespaceChar();
											if (ch2 == '(')
												ch2 = ')';

											while ((ch3 = State.NextCharSameRow()) != 0 && ch3 != ch2)
												Text.Append(ch3);

											Title = Text.ToString();
											Text.Clear();
										}
										else
											Title = string.Empty;

										ParseWidthHeight(State, out Width, out Height);

										Items.Add(new MultimediaItem(this, Url, Title, Width, Height));
										if (!this.includesTableOfContents && string.Compare(Url, "ToC", true) == 0)
											this.includesTableOfContents = true;

										ch2 = State.PeekNextNonWhitespaceChar(true);
									}

									using (TextRenderer Renderer = new TextRenderer(Text))
									{
										await Renderer.Render(ChildElements);
									}

									Multimedia Multimedia = new Multimedia(this, null,
										!Elements.HasFirstItem && State.PeekNextChar() == 0, Items.ToArray());

									this.references[Text.ToString().ToLower()] = Multimedia;

									if (!(this.settings?.Progress is null))
									{
										IMultimediaHtmlRenderer Renderer = Multimedia.GetMultimediaHandler<IMultimediaHtmlRenderer>(Multimedia.Items);
										if (!(Renderer is null))
											await Renderer.Preload(this.settings.Progress, Multimedia.Items);
									}

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
									using (TextRenderer Renderer = new TextRenderer(Text))
									{
										await Renderer.Render(ChildElements);
									}

									Title = Text.ToString();
									Text.Clear();
								}

								if (ch == '!')
								{
									Elements.Add(new MultimediaReference(this, ChildElements, Title,
										!Elements.HasFirstItem && State.PeekNextChar() == 0));
								}
								else
									Elements.Add(new LinkReference(this, ChildElements, Title));
							}
							else if (ch != '!')
								Elements.Add(new SubScript(this, ChildElements));
							else
							{
								this.FixSyntaxError(Elements, "![", ChildElements);

								if (ch2 == (char)0)
									Elements.Add(new InlineText(this, "]"));
								else
									Elements.Add(new InlineText(this, "]" + ch2));
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
							Elements.Add(new HtmlEntity(this, "RightDoubleBracket"));
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
								Elements.Add(new HtmlEntity(this, "Ll"));
							}
							else
								Elements.Add(new HtmlEntity(this, "laquo"));
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
									Elements.Add(new HtmlEntity(this, "harr"));
								}
								else
									Elements.Add(new HtmlEntity(this, "larr"));
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
									Elements.Add(new HtmlEntity(this, "hArr"));
								}
								else
									Elements.Add(new HtmlEntity(this, "lArr"));
							}
							else
								Elements.Add(new HtmlEntity(this, "leq"));
							break;
						}
						else if (ch2 == '>')
						{
							State.NextCharSameRow();
							this.AppendAnyText(Elements, Text);
							Elements.Add(new HtmlEntity(this, "ne"));
							break;
						}
						else if (ch2 == '3' && !(this.emojiSource is null))
						{
							State.NextCharSameRow();
							this.AppendAnyText(Elements, Text);
							Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_heart));
							break;
						}
						else if (ch2 == '/')
						{
							State.NextCharSameRow();
							if (!(this.emojiSource is null) && State.PeekNextCharSameRow() == '3')
							{
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_broken_heart));
								break;
							}
						}

						if ((!char.IsLetter(ch2) && ch2 != '/') || !this.settings.AllowHtml)
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

						if (Url.StartsWith("</"))
						{
							if (Url.StartsWith("</script", StringComparison.CurrentCultureIgnoreCase))
								Elements.Add(new InlineCode(this, Url));
							else
								Elements.Add(new InlineHTML(this, Url));
						}
						else if (Url.StartsWith("<script", StringComparison.CurrentCultureIgnoreCase))
						{
							if (this.AllowScriptTag && this.settings.AllowScriptTag)
							{
								Text.Append(State.UntilToken("</SCRIPT>"));
								Text.Append("</");
								Text.Append(Url.Substring(1, 6));
								Text.Append('>');

								Elements.Add(new InlineHTML(this, Text.ToString()));
							}
							else
								Elements.Add(new InlineCode(this, Url));
						}
						else if (Url.StartsWith("<textarea", StringComparison.CurrentCultureIgnoreCase))
						{
							Elements.Add(new InlineHTML(this, Url));

							string s = State.UntilToken("</TEXTAREA>");

							if (!string.IsNullOrEmpty(s))
								Elements.Add(new InlineText(this, s));

							Elements.Add(new InlineHTML(this, "</" + Url.Substring(1, 8) + ">"));
						}
						else
						{
							int i = Url.IndexOf(' ');

							if ((i < 0 && Url.IndexOf(':') >= 0) || (i > 0 && Url.LastIndexOf(':', i) >= 0))
								Elements.Add(new AutomaticLinkUrl(this, Url.Substring(1, Url.Length - 2)));
							else if ((i < 0 && Url.IndexOf('@') >= 0) || (i > 0 && Url.LastIndexOf('@', i) >= 0))
								Elements.Add(new AutomaticLinkMail(this, Url.Substring(1, Url.Length - 2)));
							else
							{
								Elements.Add(new InlineHTML(this, Url));

								if (Url.StartsWith("<textarea", StringComparison.CurrentCultureIgnoreCase))
								{
									string s = State.UntilToken("</TEXTAREA>");

									if (!string.IsNullOrEmpty(s))
										Elements.Add(new InlineText(this, s));

									Elements.Add(new InlineHTML(this, "</" + Url.Substring(1, 8) + ">"));
								}
							}
						}

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
									Elements.Add(new HtmlEntity(this, "Gg"));
								}
								else
									Elements.Add(new HtmlEntity(this, "raquo"));
								break;

							case '=':
								this.AppendAnyText(Elements, Text);
								State.NextCharSameRow();

								if (!(this.emojiSource is null) && State.PeekNextCharSameRow() == ')')
								{
									State.NextCharSameRow();
									Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_laughing));
								}
								else
									Elements.Add(new HtmlEntity(this, "geq"));
								break;

							case ':':
								if (!(this.emojiSource is null))
								{
									State.NextCharSameRow();
									switch (State.PeekNextCharSameRow())
									{
										case ')':
											State.NextCharSameRow();
											this.AppendAnyText(Elements, Text);
											Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_laughing));
											break;

										case '(':
											State.NextCharSameRow();
											this.AppendAnyText(Elements, Text);
											Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_angry));
											break;

										case '[':
											State.NextCharSameRow();
											this.AppendAnyText(Elements, Text);
											Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_disappointed));
											break;

										case 'O':
											State.NextCharSameRow();
											this.AppendAnyText(Elements, Text);
											Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_open_mouth));
											break;

										case 'P':
										case 'p':
										case 'b':
										case 'Þ':
										case 'þ':
											State.NextCharSameRow();
											this.AppendAnyText(Elements, Text);
											Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_stuck_out_tongue_winking_eye));
											break;

										case '/':
										case '\\':
										case 'L':
											State.NextCharSameRow();
											this.AppendAnyText(Elements, Text);
											Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_confused));
											break;

										case 'X':
										case 'x':
										case '#':
											State.NextCharSameRow();
											this.AppendAnyText(Elements, Text);
											Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_open_mouth));
											break;

										case '-':
											State.NextCharSameRow();
											switch (State.PeekNextCharSameRow())
											{
												case ')':
													State.NextCharSameRow();
													this.AppendAnyText(Elements, Text);
													Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_laughing));
													break;

												case '(':
													State.NextCharSameRow();
													this.AppendAnyText(Elements, Text);
													Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_angry));
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
								if (!(this.emojiSource is null))
								{
									State.NextCharSameRow();
									if (State.PeekNextCharSameRow() == ')')
									{
										State.NextCharSameRow();
										this.AppendAnyText(Elements, Text);
										Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_laughing));
									}
									else
										Text.Append(">;");
								}
								else
									Text.Append('>');
								break;

							case '.':
								if (!(this.emojiSource is null))
								{
									State.NextCharSameRow();
									if (State.PeekNextCharSameRow() == '<')
									{
										State.NextCharSameRow();
										this.AppendAnyText(Elements, Text);
										Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_persevere));
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
						if (!this.settings.AllowInlineScript || this.settings.Variables is null)
						{
							int Pos = State.CurrentPosition - 1;
							if (Pos < this.markdownText.Length && this.markdownText[Pos] == '{')
							{
								if (this.toInsert is null)
									this.toInsert = new SortedDictionary<int, char>();

								this.toInsert[Pos] = '\\';
								if (Pos > 0 && ((ch2 = this.markdownText[Pos - 1]) == ':' || ch2 == '*' || ch2 == '='))
									this.toInsert[Pos - 1] = '\\';  // To avoid creating a smiley.
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
							State.DiscardBackup();
							Expression Exp = new Expression(Text.ToString(), this.fileName);
							Elements.Add(new InlineScript(this, Exp, this.settings.Variables,
								!Elements.HasFirstItem && State.PeekNextChar() == 0, StartPosition, EndPosition));
							Text.Clear();
							this.isDynamic = true;
						}
						catch (Exception ex)
						{
							ex = Log.UnnestException(ex);

							Elements.Add(new HtmlBlock(this, new ChunkedList<MarkdownElement>(
								new InlineHTML(this, "<font class=\"error\">"))));

							if (ex is AggregateException ex2)
							{
								foreach (Exception ex3 in ex2.InnerExceptions)
								{
									this.CheckException(ex3);

									Log.Exception(ex3, this.fileName);

									Elements.Add(new Paragraph(this, new ChunkedList<MarkdownElement>(
										new InlineText(this, ex3.Message))));
								}
							}
							else
							{
								this.CheckException(ex);

								Log.Exception(ex, this.fileName);

								Elements.Add(new Paragraph(this, new ChunkedList<MarkdownElement>(
									new InlineText(this, ex.Message))));
							}

							Elements.Add(new HtmlBlock(this, new ChunkedList<MarkdownElement>(
								new InlineHTML(this, "</font>"))));

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
								Elements.Add(new HtmlEntity(this, "rarr"));
							}
							else if (ch3 == '-')
							{
								State.NextCharSameRow();
								Elements.Add(new HtmlEntity(this, "mdash"));
							}
							else
								Elements.Add(new HtmlEntity(this, "ndash"));
						}
						else if (ch2 == '+')
						{
							State.NextCharSameRow();
							this.AppendAnyText(Elements, Text);
							Elements.Add(new HtmlEntity(this, "MinusPlus"));
						}
						else if (ch2 == '_')
						{
							if (!(this.emojiSource is null))
							{
								State.BackupState();
								while ((ch2 = State.NextCharSameRow()) == '_')
									;

								if (ch2 == '-')
								{
									State.DiscardBackup();
									State.NextCharSameRow();
									this.AppendAnyText(Elements, Text);
									Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_expressionless));
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
								ChunkedList<string> Rows = new ChunkedList<string>();
								ChunkedList<int> Positions = new ChunkedList<int>()
								{
									State.CurrentPosition
								};

								Rows.Add(State.RestOfRow());

								while (!State.EOF)
								{
									if ((ch2 = State.PeekNextCharSameRow()) == '*' || ch2 == '+' || ch2 == '-')
									{
										Item = new UnnumberedItem(this, new string(ch, 1), new NestedBlock(this, await this.ParseBlock(Rows.ToArray(), Positions.ToArray())));

										if (Elements.HasLastItem && Elements.LastItem is BulletList BulletList)
											BulletList.AddChild(Item);
										else
											Elements.Add(new BulletList(this, Item));

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
									Item = new UnnumberedItem(this, new string(ch, 1), new NestedBlock(this, await this.ParseBlock(Rows.ToArray(), Positions.ToArray())));

									if (Elements.HasLastItem && Elements.LastItem is BulletList BulletList)
										BulletList.AddChild(Item);
									else
										Elements.Add(new BulletList(this, Item));
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
							Elements.Add(new HtmlEntity(this, "PlusMinus"));
						}
						else if ((ch2 <= ' ' && ch2 > 0) || ch2 == 160)
						{
							if (State.IsFirstCharOnLine)
							{
								this.AppendAnyText(Elements, Text);

								while (((ch2 = State.PeekNextCharSameRow()) <= ' ' && ch2 > 0) || ch2 == 160)
									State.NextCharSameRow();

								UnnumberedItem Item;
								ChunkedList<string> Rows = new ChunkedList<string>();
								ChunkedList<int> Positions = new ChunkedList<int>()
								{
									State.CurrentPosition
								};

								Rows.Add(State.RestOfRow());

								while (!State.EOF)
								{
									if ((ch2 = State.PeekNextCharSameRow()) == '*' || ch2 == '+' || ch2 == '-')
									{
										Item = new UnnumberedItem(this, new string(ch, 1), new NestedBlock(this, await this.ParseBlock(Rows.ToArray(), Positions.ToArray())));

										if (Elements.HasLastItem && Elements.LastItem is BulletList BulletList)
											BulletList.AddChild(Item);
										else
											Elements.Add(new BulletList(this, Item));

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
									Item = new UnnumberedItem(this, new string(ch, 1), new NestedBlock(this, await this.ParseBlock(Rows.ToArray(), Positions.ToArray())));

									if (Elements.HasLastItem && Elements.LastItem is BulletList BulletList)
										BulletList.AddChild(Item);
									else
										Elements.Add(new BulletList(this, Item));
								}
							}
							else
								Text.Append('+');
						}
						else
							Text.Append('+');
						break;

					case '#':
						ch2 = State.PeekNextCharSameRow();
						if (char.IsLetterOrDigit(ch2))
						{
							this.AppendAnyText(Elements, Text);

							Text.Append(ch2);
							State.NextCharSameRow();

							while (char.IsLetterOrDigit(ch2 = State.PeekNextCharSameRow()))
							{
								Text.Append(ch2);
								State.NextCharSameRow();
							}

							Elements.Add(new HashTag(this, Text.ToString()));
							Text.Clear();
						}
						else if (State.IsFirstCharOnLine && ch2 == '.')
						{
							State.NextCharSameRow();

							if (((ch2 = State.PeekNextCharSameRow()) <= ' ' && ch2 > 0) || ch2 == 160)
							{
								this.AppendAnyText(Elements, Text);

								while (((ch2 = State.PeekNextCharSameRow()) <= ' ' && ch2 > 0) || ch2 == 160)
									State.NextCharSameRow();

								NumberedItem Item;
								ChunkedList<string> Rows = new ChunkedList<string>();
								ChunkedList<int> Positions = new ChunkedList<int>()
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
											Item = new NumberedItem(this, 1, false, new NestedBlock(this, await this.ParseBlock(Rows.ToArray(), Positions.ToArray())));

											if (Elements.HasLastItem && Elements.LastItem is NumberedList NumberedList)
												NumberedList.AddChild(Item);
											else
												Elements.Add(new NumberedList(this, Item));

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
									Item = new NumberedItem(this, 1, false, new NestedBlock(this, await this.ParseBlock(Rows.ToArray(), Positions.ToArray())));

									if (Elements.HasLastItem && Elements.LastItem is NumberedList NumberedList)
										NumberedList.AddChild(Item);
									else
									{
										if (!Item.NumberExplicit &&
											Elements.HasLastItem &&
											Elements.LastItem is NumberedItem PrevItem)
										{
											Item.Number = PrevItem.Number + 1;
										}

										Elements.Add(new NumberedList(this, Item));
									}
								}
							}
							else
								Text.Append("#.");
						}
						else if (!(this.emojiSource is null))
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
											Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_dizzy_face));
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
									Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_dizzy_face));
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
						if (!(this.emojiSource is null) && (ch == '8' || ch == '0') && (char.IsPunctuation(PrevChar) || char.IsWhiteSpace(PrevChar)))
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
												Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_innocent));
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
														Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_innocent));
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
														Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_innocent));
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
												Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_sunglasses));
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
										Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_sunglasses));
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
									ChunkedList<string> Rows = new ChunkedList<string>();
									ChunkedList<int> Positions = new ChunkedList<int>()
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
												Item = new NumberedItem(this, Index, true, new NestedBlock(this, await this.ParseBlock(Rows.ToArray(), Positions.ToArray())));

												if (Elements.HasLastItem && Elements.LastItem is NumberedList NumberedList)
													NumberedList.AddChild(Item);
												else
													Elements.Add(new NumberedList(this, Item));

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
										Item = new NumberedItem(this, Index, true, new NestedBlock(this, await this.ParseBlock(Rows.ToArray(), Positions.ToArray())));

										if (Elements.HasLastItem && Elements.LastItem is NumberedList NumberedList)
											NumberedList.AddChild(Item);
										else
											Elements.Add(new NumberedList(this, Item));
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

								ch3 = State.PeekNextCharSameRow();
								if (ch3 == '>')
								{
									State.NextCharSameRow();
									Elements.Add(new HtmlEntity(this, "rArr"));
								}
								else
									Elements.Add(new HtmlEntity(this, "equiv"));
								break;

							case 'D':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_smiley));
								break;

							case ')':
							case ']':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_smile));
								break;

							case '*':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_kissing_heart));
								break;

							case '(':
							case '[':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_disappointed));
								break;

							case '$':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_flushed));
								break;

							case '/':
							case '\\':
							case 'L':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_confused));
								break;

							case 'P':
							case 'p':
							case 'b':
							case 'Þ':
							case 'þ':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_stuck_out_tongue));
								break;

							case 'X':
							case 'x':
							case '#':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_no_mouth));
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

							Elements.Add(new HtmlEntity(this, Url.Substring(1, Url.Length - 2)));
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

									Elements.Add(new HtmlEntityUnicode(this, Code));
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

									Elements.Add(new HtmlEntityUnicode(this, Code));
								}
							}
						}
						else
							Text.Append(ch);

						break;

					case '"':
						this.AppendAnyText(Elements, Text);
						if (IsLeftQuote(PrevChar, State.PeekNextCharSameRow()))
							Elements.Add(new HtmlEntity(this, "ldquo"));
						else
							Elements.Add(new HtmlEntity(this, "rdquo"));
						break;

					case '\'':
						this.AppendAnyText(Elements, Text);

						if (!(this.emojiSource is null))
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
										Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_sweat_smile));
										break;

									case '(':
										State.NextCharSameRow();
										Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_sweat));
										break;

									case '-':
										State.NextCharSameRow();
										switch (State.PeekNextCharSameRow())
										{
											case ')':
											case 'D':
												State.NextCharSameRow();
												Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_sweat_smile));
												break;

											case '(':
												State.NextCharSameRow();
												Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_sweat));
												break;

											default:
												if (IsLeftQuote(PrevChar, State.PeekNextCharSameRow()))
													Elements.Add(new HtmlEntity(this, "lsquo"));
												else
													Elements.Add(new HtmlEntity(this, "rsquo"));

												Text.Append(":-");
												break;
										}
										break;

									default:
										if (IsLeftQuote(PrevChar, State.PeekNextCharSameRow()))
											Elements.Add(new HtmlEntity(this, "lsquo"));
										else
											Elements.Add(new HtmlEntity(this, "rsquo"));

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
										Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_sweat_smile));
										break;

									case '(':
										State.NextCharSameRow();
										Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_sweat));
										break;

									default:
										if (IsLeftQuote(PrevChar, State.PeekNextCharSameRow()))
											Elements.Add(new HtmlEntity(this, "lsquo"));
										else
											Elements.Add(new HtmlEntity(this, "rsquo"));

										Text.Append('=');
										break;
								}
								break;

							default:
								if (IsLeftQuote(PrevChar, State.PeekNextCharSameRow()))
									Elements.Add(new HtmlEntity(this, "lsquo"));
								else
									Elements.Add(new HtmlEntity(this, "rsquo"));
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

								Elements.Add(new HtmlEntity(this, "hellip"));
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

								Elements.Add(new HtmlEntity(this, Url));
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
									Elements.Add(new HtmlEntity(this, "pertenk"));
								}
								else
									Elements.Add(new HtmlEntity(this, "permil"));
								break;

							case '-':
								if (!(this.emojiSource is null))
								{
									State.BackupState();
									State.NextCharSameRow();
									switch (State.PeekNextCharSameRow())
									{
										case ')':
											State.DiscardBackup();
											State.NextCharSameRow();
											this.AppendAnyText(Elements, Text);
											Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_dizzy_face));
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
								if (!(this.emojiSource is null))
								{
									State.NextCharSameRow();
									this.AppendAnyText(Elements, Text);
									Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_dizzy_face));
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
								Elements.Add(new HtmlEntity(this, "ordf"));
								break;

							case 'o':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.Add(new HtmlEntity(this, "ordm"));
								break;

							case '0':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.Add(new HtmlEntity(this, "deg"));
								break;

							case '1':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.Add(new HtmlEntityUnicode(this, 185));
								break;

							case '2':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.Add(new HtmlEntityUnicode(this, 178));
								break;

							case '3':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);
								Elements.Add(new HtmlEntityUnicode(this, 179));
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
								Elements.Add(new SuperScript(this, new string(ch2, 1)));
								break;

							case 'T':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);

								if (State.PeekNextCharSameRow() == 'M')
								{
									State.NextCharSameRow();
									Elements.Add(new HtmlEntity(this, "trade"));
								}
								else
									Elements.Add(new SuperScript(this, "T"));
								break;

							case 's':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);

								if (State.PeekNextCharSameRow() == 't')
								{
									State.NextCharSameRow();
									Elements.Add(new SuperScript(this, "st"));
								}
								else
									Elements.Add(new SuperScript(this, "s"));
								break;

							case 'n':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);

								if (State.PeekNextCharSameRow() == 'd')
								{
									State.NextCharSameRow();
									Elements.Add(new SuperScript(this, "nd"));
								}
								else
									Elements.Add(new SuperScript(this, "n"));
								break;

							case 'r':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);

								if (State.PeekNextCharSameRow() == 'd')
								{
									State.NextCharSameRow();
									Elements.Add(new SuperScript(this, "rd"));
								}
								else
									Elements.Add(new SuperScript(this, "r"));
								break;

							case 't':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);

								if (State.PeekNextCharSameRow() == 'h')
								{
									State.NextCharSameRow();
									Elements.Add(new SuperScript(this, "th"));
								}
								else
									Elements.Add(new SuperScript(this, "t"));
								break;

							case '(':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);

								ChildElements = new ChunkedList<MarkdownElement>();

								await this.ParseBlock(State, ')', 1, ChildElements, true);
								Elements.Add(new SuperScript(this, ChildElements));
								break;

							case '[':
								State.NextCharSameRow();
								this.AppendAnyText(Elements, Text);

								ChildElements = new ChunkedList<MarkdownElement>();

								await this.ParseBlock(State, ']', 1, ChildElements, true);
								Elements.Add(new SuperScript(this, ChildElements));
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
								ChunkedList<MarkdownElement> TotItem = null;
								ChunkedList<MarkdownElement> Item;
								DefinitionList DefinitionList = new DefinitionList(this);
								int i;

								for (i = State.Start; i < State.Current; i++)
								{
									Item = await this.ParseBlock(State.Rows, State.Positions, i, i);
									if (!Item.HasFirstItem)
										continue;

									if (TotItem is null)
									{
										if (Item.Count == 1)
											TotItem = Item;
										else
											TotItem.Add(Item.FirstItem);
									}
									else
									{
										if (TotItem is null)
											TotItem = new ChunkedList<MarkdownElement>();

										TotItem.Add(new NestedBlock(this, Item));
									}
								}

								if (TotItem is null)
									Text.Append(ch);
								else
								{
									DefinitionList.AddChild(new DefinitionTerms(this, TotItem));

									Text.Clear();
									Elements.Clear();
									Elements.Add(DefinitionList);

									while (((ch2 = State.PeekNextCharSameRow()) <= ' ' && ch2 > 0) || ch2 == 160)
										State.NextCharSameRow();

									ChunkedList<string> Rows = new ChunkedList<string>();
									ChunkedList<int> Positions = new ChunkedList<int>()
									{
										State.CurrentPosition
									};

									Rows.Add(State.RestOfRow());

									while (!State.EOF)
									{
										if (State.PeekNextCharSameRow() == ':')
										{
											DefinitionList.AddChild(new DefinitionDescriptions(this, new NestedBlock(this, await this.ParseBlock(Rows.ToArray(), Positions.ToArray()))));

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
										DefinitionList.AddChild(new DefinitionDescriptions(this, new NestedBlock(this, await this.ParseBlock(Rows.ToArray(), Positions.ToArray()))));
								}
							}
							else
								Text.Append(ch);
						}
						else if (!(this.emojiSource is null))
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
											Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_smiley));
											break;

										case 'L':
											State.NextCharSameRow();
											if (LeftLevel > 1)
												Text.Append(new string(':', LeftLevel - 1));

											this.AppendAnyText(Elements, Text);
											Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_confused));
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
											Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_stuck_out_tongue));
											break;

										case 'O':
										case 'o':
											State.NextCharSameRow();
											if (LeftLevel > 1)
												Text.Append(new string(':', LeftLevel - 1));

											this.AppendAnyText(Elements, Text);
											Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_open_mouth));
											break;

										case 'X':
										case 'x':
											State.NextCharSameRow();
											if (LeftLevel > 1)
												Text.Append(new string(':', LeftLevel - 1));

											this.AppendAnyText(Elements, Text);
											Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_no_mouth));
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
											Elements.Add(new InlineText(this, new string(':', LeftLevel - RightLevel)));

										Elements.Add(new EmojiReference(this, Emoji, RightLevel));
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
												Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_joy));
												break;

											case '(':
												State.NextCharSameRow();
												this.AppendAnyText(Elements, Text);
												Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_cry));
												break;

											case '-':
												State.NextCharSameRow();
												if ((ch3 = State.PeekNextCharSameRow()) == ')')
												{
													State.NextCharSameRow();
													this.AppendAnyText(Elements, Text);
													Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_joy));
												}
												else if (ch3 == '(')
												{
													State.NextCharSameRow();
													this.AppendAnyText(Elements, Text);
													Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_cry));
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
												Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_smile));
												break;

											case '(':
											case '[':
												State.NextCharSameRow();
												this.AppendAnyText(Elements, Text);
												Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_disappointed));
												break;

											case 'D':
												State.NextCharSameRow();
												this.AppendAnyText(Elements, Text);
												Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_smiley));
												break;

											case '*':
												State.NextCharSameRow();
												this.AppendAnyText(Elements, Text);
												Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_kissing_heart));
												break;

											case '/':
											case '.':
											case '\\':
											case 'L':
												State.NextCharSameRow();
												this.AppendAnyText(Elements, Text);
												Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_confused));
												break;

											case 'P':
											case 'p':
											case 'b':
											case 'Þ':
											case 'þ':
												State.NextCharSameRow();
												this.AppendAnyText(Elements, Text);
												Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_stuck_out_tongue));
												break;

											case 'O':
											case 'o':
												State.NextCharSameRow();
												this.AppendAnyText(Elements, Text);
												Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_open_mouth));
												break;

											case 'X':
											case 'x':
											case '#':
												State.NextCharSameRow();
												this.AppendAnyText(Elements, Text);
												Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_no_mouth));
												break;

											case '-':
												State.NextCharSameRow();
												if ((ch3 = State.PeekNextCharSameRow()) == ')')
												{
													State.NextCharSameRow();
													this.AppendAnyText(Elements, Text);
													Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_joy));
												}
												else if (ch3 == '(')
												{
													State.NextCharSameRow();
													this.AppendAnyText(Elements, Text);
													Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_cry));
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
													Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji__1));
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
										Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_smile));
										break;

									case '(':
									case '[':
										State.NextCharSameRow();
										this.AppendAnyText(Elements, Text);
										Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_disappointed));
										break;

									case '*':
										State.NextCharSameRow();
										this.AppendAnyText(Elements, Text);
										Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_kissing_heart));
										break;

									case '/':
									case '\\':
										State.NextCharSameRow();
										this.AppendAnyText(Elements, Text);
										Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_confused));
										break;

									case '#':
										State.NextCharSameRow();
										this.AppendAnyText(Elements, Text);
										Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_no_mouth));
										break;

									case '@':
										State.NextCharSameRow();
										this.AppendAnyText(Elements, Text);
										Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_angry));
										break;

									case '$':
										State.NextCharSameRow();
										this.AppendAnyText(Elements, Text);
										Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_flushed));
										break;

									case '^':
										State.NextCharSameRow();
										if (State.PeekNextCharSameRow() == '*')
										{
											State.NextCharSameRow();
											this.AppendAnyText(Elements, Text);
											Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_kissing_heart));
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
						if (!(this.emojiSource is null))
						{
							switch (State.PeekNextCharSameRow())
							{
								case ')':
								case ']':
								case 'D':
									State.NextCharSameRow();
									this.AppendAnyText(Elements, Text);
									Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_wink));
									break;

								case '(':
								case '[':
									State.NextCharSameRow();
									this.AppendAnyText(Elements, Text);
									Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_cry));
									break;

								case '-':
									State.NextCharSameRow();
									switch (State.PeekNextCharSameRow())
									{
										case ')':
										case ']':
											State.NextCharSameRow();
											this.AppendAnyText(Elements, Text);
											Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_wink));
											break;

										case '(':
										case '[':
											State.NextCharSameRow();
											this.AppendAnyText(Elements, Text);
											Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_cry));
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
										Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_wink));
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
						if (!(this.emojiSource is null) && (char.IsPunctuation(PrevChar) || char.IsWhiteSpace(PrevChar)))
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
											Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_stuck_out_tongue_winking_eye));
											break;

										case ')':
											State.DiscardBackup();
											State.NextCharSameRow();
											this.AppendAnyText(Elements, Text);
											Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_dizzy_face));
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
									Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_dizzy_face));
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
						if (!(this.emojiSource is null) && (char.IsPunctuation(PrevChar) || char.IsWhiteSpace(PrevChar)))
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
											Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_sunglasses));
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
									Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_sunglasses));
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
						if (!(this.emojiSource is null) && (char.IsPunctuation(PrevChar) || char.IsWhiteSpace(PrevChar)) && State.PeekNextCharSameRow() == ':')
						{
							State.NextCharSameRow();
							this.AppendAnyText(Elements, Text);
							Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_stuck_out_tongue));
						}
						else
							Text.Append(ch);
						break;

					case 'O':
						if (!(this.emojiSource is null) && (char.IsPunctuation(PrevChar) || char.IsWhiteSpace(PrevChar)))
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
											Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_innocent));
											break;

										case '-':
											if ((ch3 = State.NextCharSameRow()) == ')' || ch3 == '3')
											{
												State.DiscardBackup();
												this.AppendAnyText(Elements, Text);
												Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_innocent));
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
											Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_innocent));
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
										Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_innocent));
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
										Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_open_mouth));
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

								if (FirstCharOnLine && State.PeekNextNonWhitespaceCharSameRow(false) == 0)
								{
									IMultimediaContent Handler = Multimedia.GetMultimediaHandler<IMultimediaHtmlRenderer>(Url);
									if (!(Handler is null) && Handler.EmbedInlineLink(Url))
									{
										ChildElements = new ChunkedList<MarkdownElement>
										{
											new InlineText(this, Url)
										};

										Multimedia Multimedia = new Multimedia(this, ChildElements, true,
											new MultimediaItem(this, Url, string.Empty, null, null));

										Elements.Add(Multimedia);

										if (!(this.settings?.Progress is null))
										{
											IMultimediaHtmlRenderer Renderer = Multimedia.GetMultimediaHandler<IMultimediaHtmlRenderer>(Multimedia.Items);
											if (!(Renderer is null))
												await Renderer.Preload(this.settings.Progress, Multimedia.Items);
										}

										break;
									}
								}

								Elements.Add(new AutomaticLinkUrl(this, Url));
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
								if (!(this.emojiSource is null))
								{
									State.BackupState();
									State.NextCharSameRow();
									if (State.PeekNextCharSameRow() == '/')
									{
										State.DiscardBackup();
										State.NextCharSameRow();
										this.AppendAnyText(Elements, Text);
										Elements.Add(new EmojiReference(this, EmojiUtilities.Emoji_ok_woman));
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

			return (ch == TerminationCharacter) || AcceptIncomplete;
		}

		private static bool IsLeftQuote(char PrevChar, char NextChar)
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

		private static void ParseWidthHeight(BlockParseState State, out int? Width, out int? Height)
		{
			Width = null;
			Height = null;

			char ch = State.PeekNextNonWhitespaceCharSameRow(true);
			if (ch >= '0' && ch <= '9')
			{
				StringBuilder Text = new StringBuilder();

				Text.Append(ch);
				State.NextNonWhitespaceCharSameRow();

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

					ch = State.PeekNextNonWhitespaceCharSameRow(true);
					if (ch >= '0' && ch <= '9')
					{
						Text.Append(ch);
						State.NextNonWhitespaceCharSameRow();

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

		private void AppendAnyText(ChunkedList<MarkdownElement> Elements, StringBuilder Text)
		{
			if (Text.Length > 0)
			{
				string s = Text.ToString();
				Text.Clear();

				if (Elements.HasFirstItem || !string.IsNullOrEmpty(s.Trim()))
					Elements.Add(new InlineText(this, s));
			}
		}

		private void FixSyntaxError(ChunkedList<MarkdownElement> Elements, string Prefix, ChunkedList<MarkdownElement> ChildElements)
		{
			Elements.Add(new InlineText(this, Prefix));
			Elements.AddRange(ChildElements);
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

		private static bool IsPrefixedBy(string s, char ch, out int Count, bool MustHaveWhiteSpaceAfter)
		{
			int c = s.Length;

			Count = 0;
			while (Count < c && s[Count] == ch)
				Count++;

			if (Count == 0)
				return false;

			if (MustHaveWhiteSpaceAfter)
			{
				if (s.Length == Count)
					return false;

				return (ch = s[Count]) <= ' ' || ch == 160;
			}
			else
				return true;
		}

		internal static bool IsSuffixedBy(string s, string Suffix)
		{
			return s.EndsWith(Suffix);
		}

		/*private static bool IsSuffixedBy(string s, char ch, out int Count)
		{
			int c = s.Length;

			Count = 0;
			while (Count < c && s[c - Count - 1] == ch)
				Count++;

			if (Count == 0)
				return false;

			return true;
		}*/

		private static bool IsUnderline(string s, char ch, bool AllowSpaces, bool OnlyOneSpace)
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

		private static ChunkedList<Block> ParseTextToBlocks(string MarkdownText)
		{
			ChunkedList<Block> Blocks = new ChunkedList<Block>();
			ChunkedList<string> Rows = new ChunkedList<string>();
			ChunkedList<int> Positions = new ChunkedList<int>();
			int FirstLineIndent = 0;
			int LineIndent = 0;
			int RowStart = 0;
			int RowEnd = 0;
			int Pos, Len;
			char ch;
			bool InBlock = false;
			bool InRow = false;
			bool NonWhitespaceInRow = false;
			bool StartsWithHashSigns = false;
			bool IsHeader = false;
			bool HasRows = false;

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
							if (HasRows &&
								LineIndent < FirstLineIndent &&
								IsListPrefix(MarkdownText, RowStart))
							{
								Blocks.Add(new Block(Rows.ToArray(), Positions.ToArray(), FirstLineIndent / 4));
								Rows.Clear();
								Positions.Clear();
								FirstLineIndent = LineIndent;
							}

							Positions.Add(RowStart);
							Rows.Add(MarkdownText.Substring(RowStart, RowEnd - RowStart + 1));
							InRow = false;
							HasRows = true;

							if (IsHeader && Rows.Count == 1)
							{
								Blocks.Add(new Block(Rows.ToArray(), Positions.ToArray(), FirstLineIndent / 4));
								Rows.Clear();
								Positions.Clear();
								InBlock = false;
								HasRows = false;
								FirstLineIndent = 0;
							}
						}
						else
						{
							Blocks.Add(new Block(Rows.ToArray(), Positions.ToArray(), FirstLineIndent / 4));
							Rows.Clear();
							Positions.Clear();
							InBlock = false;
							InRow = false;
							HasRows = false;
							FirstLineIndent = 0;
						}
					}
					else
						FirstLineIndent = 0;

					LineIndent = 0;
					NonWhitespaceInRow = false;
					StartsWithHashSigns = false;
					IsHeader = false;
				}
				else if (ch <= ' ' || ch == 160)
				{
					if (InBlock)
					{
						if (InRow)
						{
							RowEnd = Pos;

							if (StartsWithHashSigns)
								IsHeader = true;
						}
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

					StartsWithHashSigns = false;
				}
				else
				{
					if (!InRow)
					{
						InRow = true;
						InBlock = true;
						RowStart = Pos;

						if (ch == '#')
							StartsWithHashSigns = true;
					}
					else if (ch != '#')
						StartsWithHashSigns = false;

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
					//HasRows = true;
				}

				Blocks.Add(new Block(Rows.ToArray(), Positions.ToArray(), FirstLineIndent / 4));
			}

			return Blocks;
		}

		private static bool IsListPrefix(string MarkdownText, int Pos)
		{
			int c = MarkdownText.Length;
			char ch = MarkdownText[Pos++];
			bool ExpectPeriod;

			if (ch == '*' || ch == '+' || ch == '-')
				ExpectPeriod = false;
			else if (ch == '#')
				ExpectPeriod = true;
			else if (ch == '[')
			{
				ExpectPeriod = false;
				if (Pos >= c)
					return false;

				ch = MarkdownText[Pos++];
				if (ch != ' ' && ch != 'x' && ch != 'X')
					return false;

				if (Pos >= c)
					return false;

				ch = MarkdownText[Pos++];
				if (ch != ']')
					return false;
			}
			else if (ch >= '0' && ch <= '9')
			{
				ExpectPeriod = true;

				while (Pos < c && (ch = MarkdownText[Pos]) >= '0' && ch <= '9')
					Pos++;
			}
			else
				return false;

			if (ExpectPeriod)
			{
				if (Pos >= c)
					return false;

				ch = MarkdownText[Pos++];
				if (ch != '.')
					return false;
			}

			if (Pos >= c)
				return false;

			ch = MarkdownText[Pos++];

			return ch <= ' ' || ch == 160;
		}

		/// <summary>
		/// Renders the document using provided output format.
		/// </summary>
		/// <param name="Output">Output</param>
		public async Task RenderDocument(IRenderer Output)
		{
			if (this.metaData.TryGetValue("MASTER", out KeyValuePair<string, bool>[] Master) && Master.Length == 1)
			{
				await this.LoadMasterIfNotLoaded(Master[0].Key);
				this.master.ClearFootnoteReferences();
				await Output.RenderDocument(this.master, false);
			}
			else
			{
				this.ClearFootnoteReferences();
				await Output.RenderDocument(this, false);
			}

			this.ProcessAsyncTasks();
		}

		/// <summary>
		/// Clears any footnote references.
		/// </summary>
		private void ClearFootnoteReferences()
		{
			if (!(this.footnotes is null))
			{
				foreach (Footnote Footnote in this.footnotes.Values)
					Footnote.Referenced = false;
			}
		}

		/// <summary>
		/// Order of footnotes.
		/// </summary>
		public IEnumerable<string> FootnoteOrder => this.footnoteOrder;

		private async Task LoadMasterIfNotLoaded(string MasterMetaValue)
		{
			if (this.master is null)
			{
				string FileName;

				if (!string.IsNullOrEmpty(this.fileName))
					FileName = this.settings.GetFileName(this.fileName, MasterMetaValue);
				else if (!string.IsNullOrEmpty(this.resourceName))
				{
					FileName = Path.Combine(this.resourceName, MasterMetaValue);
					if (!(this.settings.ResourceMap is null) && this.settings.ResourceMap.TryGetFileName(FileName, false, out string s))
						FileName = s;
				}
				else
					FileName = MasterMetaValue;

				string MarkdownText = await Files.ReadAllTextAsync(FileName);
				this.settings?.Progress?.DependencyTimestamp(File.GetLastWriteTimeUtc(FileName));

				this.master = await CreateAsync(MarkdownText, this.settings);
				this.master.fileName = FileName;
				this.master.syntaxHighlighting |= this.syntaxHighlighting;

				if (this.master.metaData.ContainsKey("MASTER"))
				{
					throw new GenericException("Master documents are not allowed to be embedded in other master documents.",
						EventType.Error, FileName, this.fileName);
				}

				CopyMetaDataTags(this, this.master, true);

				this.master.detail = this;
			}
		}

		internal static void CopyMetaDataTags(MarkdownDocument Details, MarkdownDocument Master, bool UpdateMasterPaths)
		{
			if (UpdateMasterPaths && !string.IsNullOrEmpty(Details.fileName) && !string.IsNullOrEmpty(Master.fileName))
			{
				string DetailsFileName = Path.GetFullPath(Details.fileName);
				string MasterFileName = Path.GetFullPath(Master.fileName);
				string DetailsFolder = Path.GetDirectoryName(DetailsFileName);
				string MasterFolder = Path.GetDirectoryName(MasterFileName);

				if (DetailsFolder != MasterFolder)
				{
					string Prefix = null;

					foreach (KeyValuePair<string, KeyValuePair<string, bool>[]> Meta in Master.metaData)
					{
						switch (Meta.Key)
						{
							case "ALTERNATE":
							case "COPYRIGHT":
							case "CSS":
							case "ICON":
							case "HELP":
							case "IMAGE":
							case "INIT":
							case "JAVASCRIPT":
							case "LOGIN":
							case "NEXT":
							case "PREV":
							case "PREVIOUS":
							case "SCRIPT":
							case "WEB":
								int i, j, k, l, c, d;

								for (k = 0, l = Meta.Value.Length; k < l; k++)
								{
									string s = Meta.Value[k].Key;
									if (string.IsNullOrEmpty(s))
										continue;

									if (s[0] == Path.DirectorySeparatorChar || s[0] == '/')
										continue;

									if (Prefix is null)
									{
										string[] DetailsParts = DetailsFolder.Split(Path.DirectorySeparatorChar);
										string[] MasterParts = MasterFolder.Split(Path.DirectorySeparatorChar);

										i = 0;
										c = DetailsParts.Length;
										d = MasterParts.Length;

										while (i < c && i < d && string.Compare(DetailsParts[i], MasterParts[i], true) == 0)
											i++;

										StringBuilder sb = new StringBuilder();

										j = c - i;

										while (j-- > 0)
											sb.Append("../");

										while (i < d)
										{
											sb.Append(MasterParts[i++]);
											sb.Append('/');
										}

										Prefix = sb.ToString();
									}

									Meta.Value[k] = new KeyValuePair<string, bool>(Prefix + s, Meta.Value[k].Value);
								}
								break;
						}
					}
				}
			}

			foreach (KeyValuePair<string, KeyValuePair<string, bool>[]> Meta in Details.metaData)
			{
				if (Master.metaData.TryGetValue(Meta.Key, out KeyValuePair<string, bool>[] Meta0))
					Master.metaData[Meta.Key] = Meta0.Join(Meta.Value);
				else
					Master.metaData[Meta.Key] = Meta.Value;
			}
		}

		/// <summary>
		/// If referenced footnotes need to be rendered.
		/// </summary>
		internal bool NeedsToDisplayFootnotes
		{
			get
			{
				if (this.footnotes is null)
					return false;

				foreach (Footnote Footnote in this.footnotes.Values)
				{
					if (Footnote.Referenced)
						return true;
				}

				return false;
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
		/// Generates Markdown from the markdown text.
		/// </summary>
		/// <returns>Markdown</returns>
		public Task<string> GenerateMarkdown()
		{
			return this.GenerateMarkdown(true);
		}

		/// <summary>
		/// Generates Markdown from the markdown text.
		/// </summary>
		/// <param name="PortableSyntax">If a portable syntax of markdown is to be generated (true), or if generated
		/// Markdown is to be processed on the same machine (false).</param>
		/// <returns>Markdown</returns>
		public async Task<string> GenerateMarkdown(bool PortableSyntax)
		{
			StringBuilder Output = new StringBuilder();
			await this.GenerateMarkdown(Output, PortableSyntax);
			return Output.ToString();
		}

		/// <summary>
		/// Generates Markdown from the markdown text.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public Task GenerateMarkdown(StringBuilder Output)
		{
			return this.GenerateMarkdown(Output, true);
		}

		/// <summary>
		/// Generates Markdown from the markdown text.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		/// <param name="PortableSyntax">If a portable syntax of markdown is to be generated (true), or if generated
		/// Markdown is to be processed on the same machine (false).</param>
		public async Task GenerateMarkdown(StringBuilder Output, bool PortableSyntax)
		{
			using (MarkdownRenderer Renderer = new MarkdownRenderer(Output)
			{
				PortableSyntax = PortableSyntax
			})
			{
				await this.RenderDocument(Renderer);
			}
		}

		/// <summary>
		/// Generates HTML from the markdown text.
		/// </summary>
		/// <returns>HTML</returns>
		public async Task<string> GenerateHTML()
		{
			StringBuilder Output = new StringBuilder();
			await this.GenerateHTML(Output);
			return Output.ToString();
		}

		/// <summary>
		/// Generates HTML from the markdown text.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public Task GenerateHTML(StringBuilder Output)
		{
			return this.GenerateHTML(Output, new HtmlSettings());
		}

		/// <summary>
		/// Generates HTML from the markdown text.
		/// </summary>
		/// <param name="HtmlSettings">HTML-specific settings.</param>
		/// <returns>HTML</returns>
		public async Task<string> GenerateHTML(HtmlSettings HtmlSettings)
		{
			StringBuilder Output = new StringBuilder();
			await this.GenerateHTML(Output, HtmlSettings);
			return Output.ToString();
		}

		/// <summary>
		/// Generates HTML from the markdown text.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		/// <param name="HtmlSettings">HTML-specific settings.</param>
		public async Task GenerateHTML(StringBuilder Output, HtmlSettings HtmlSettings)
		{
			using (HtmlRenderer Renderer = new HtmlRenderer(Output, HtmlSettings))
			{
				await this.RenderDocument(Renderer);
			}
		}

		/// <summary>
		/// Generates Plain Text from the markdown text.
		/// </summary>
		/// <returns>Plain Text</returns>
		public async Task<string> GeneratePlainText()
		{
			StringBuilder Output = new StringBuilder();
			await this.GeneratePlainText(Output);
			return Output.ToString();
		}

		/// <summary>
		/// Generates Plain Text from the markdown text.
		/// </summary>
		/// <param name="Output">Plain Text will be output here.</param>
		public async Task GeneratePlainText(StringBuilder Output)
		{
			using (TextRenderer Renderer = new TextRenderer(Output))
			{
				await this.RenderDocument(Renderer);
			}
		}

		/// <summary>
		/// Gets the multimedia information referenced by a label.
		/// </summary>
		/// <param name="Label">Label</param>
		/// <returns>Multimedia information if found, null otherwise.</returns>
		public Multimedia GetReference(string Label)
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
		public Header[] Headers => this.headers.ToArray();

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
		/// Adds meta-data to the document.
		/// </summary>
		/// <param name="Key">Key name</param>
		/// <param name="Value">Meta-data value.</param>
		public void AddMetaData(string Key, string Value)
		{
			if (this.metaData.TryGetValue(Key, out KeyValuePair<string, bool>[] Records))
			{
				ChunkedList<KeyValuePair<string, bool>> Values = new ChunkedList<KeyValuePair<string, bool>>();
				Values.AddRange(Records);
				Values.Add(new KeyValuePair<string, bool>(Value.Trim(), Value.EndsWith("  ")));
			}
			else
				this.metaData[Key] = new KeyValuePair<string, bool>[] { new KeyValuePair<string, bool>(Value.Trim(), Value.EndsWith("  ")) };
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
		/// Meta-data
		/// </summary>
		public IEnumerable<KeyValuePair<string, KeyValuePair<string, bool>[]>> MetaData => this.metaData;

		/// <summary>
		/// Multimedia references
		/// </summary>
		public IEnumerable<KeyValuePair<string, Multimedia>> References => this.references;

		/// <summary>
		/// Author(s) of document.
		/// </summary>
		public string[] Author => this.GetMetaData("Author");

		/// <summary>
		/// Link to copyright statement.
		/// </summary>
		public string[] Copyright => this.GetMetaData("Copyright");

		/// <summary>
		/// Link to previous document, in a paginated set of documents.
		/// </summary>
		public string[] Previous => this.Merge(this.GetMetaData("Previous"), this.GetMetaData("Prev"));

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
		public string[] Next => this.GetMetaData("Next");

		/// <summary>
		/// Link(s) to Cascading Style Sheet(s) that should be used for visual formatting of the generated HTML page.
		/// </summary>
		public string[] CSS => this.GetMetaData("CSS");

		/// <summary>
		/// Link(s) to JavaScript files(s) that should be includedin the generated HTML page.
		/// </summary>
		public string[] JavaScript => this.GetMetaData("JAVASCRIPT");

		/// <summary>
		/// Links to server-side script files that should be included before processing the page.
		/// </summary>
		public string[] Script => this.GetMetaData("SCRIPT");

		/// <summary>
		/// Links to server-side script files that should be executed before before processing the page.
		/// Initialization script are only executed once. To execute init script again, a new version
		/// (timestamp) of the file must be present.
		/// </summary>
		public string[] InitializationScript => this.GetMetaData("INIT");

		/// <summary>
		/// Name of a query parameter recognized by the page.
		/// </summary>
		public string[] Parameters => this.GetMetaData("PARAMETER");

		/// <summary>
		/// (Publication) date of document.
		/// </summary>
		public string[] Date => this.GetMetaData("Date");

		/// <summary>
		/// Description of document.
		/// </summary>
		public string[] Description => this.GetMetaData("Description");

		/// <summary>
		/// Link to image for page.
		/// </summary>
		public string[] Image => this.GetMetaData("Image");

		/// <summary>
		/// Keywords.
		/// </summary>
		public string[] Keywords => this.GetMetaData("Keywords");

		/// <summary>
		/// Subtitle of document.
		/// </summary>
		public string[] Subtitle => this.GetMetaData("Subtitle");

		/// <summary>
		/// Title of document.
		/// </summary>
		public string[] Title => this.GetMetaData("Title");

		/// <summary>
		/// Link to web page
		/// </summary>
		public string[] Web => this.GetMetaData("Web");

		/// <summary>
		/// Tells the browser to refresh the page after a given number of seconds.
		/// </summary>
		public string[] Refresh => this.GetMetaData("Refresh");

		/// <summary>
		/// Name of the variable that will hold a reference to the IUser interface for the currently logged in user.
		/// </summary>
		public string[] UserVariable => this.GetMetaData("UserVariable");

		/// <summary>
		/// Link to a login page. This page will be shown if the user variable does not contain a user.
		/// </summary>
		public string[] Login => this.GetMetaData("Login");

		/// <summary>
		/// Requered user privileges to display page.
		/// </summary>
		public string[] Privileges => this.GetMetaData("Privileges");

		/// <summary>
		/// Tries to get the number of a footnote, given its key.
		/// </summary>
		/// <param name="Key">Footnote key.</param>
		/// <param name="Number">Footnote number.</param>
		/// <returns>If a footnote with the given key was found.</returns>
		public bool TryGetFootnoteNumber(string Key, out int Number)
		{
			if (this.footnoteNumberByKey is null)
			{
				Number = 0;
				return false;
			}
			else
				return this.footnoteNumberByKey.TryGetValue(Key, out Number);
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
		public IEmojiSource EmojiSource => this.emojiSource;

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
		public bool SyntaxHighlighting => this.syntaxHighlighting;

		/// <summary>
		/// Filename of Markdown document. Markdown inclusion will be made relative to this filename.
		/// </summary>
		public string FileName
		{
			get => this.fileName;
			set => this.fileName = value;
		}

		/// <summary>
		/// Local resource name of Markdown document, if referenced through a web server. Master documents use this resource name to match
		/// detail content with menu links.
		/// </summary>
		public string ResourceName
		{
			get => this.resourceName;
			set => this.resourceName = value;
		}

		/// <summary>
		/// Absolute URL of Markdown document, if referenced through a web server.
		/// </summary>
		public string URL
		{
			get => this.url;
			set => this.url = value;
		}

		/// <summary>
		/// Master document responsible for the current document.
		/// </summary>
		public MarkdownDocument Master
		{
			get => this.master;
			set => this.master = value;
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
			set => this.detail = value;
		}

		/// <summary>
		/// Markdown settings.
		/// </summary>
		public MarkdownSettings Settings => this.settings;

		/// <summary>
		/// If the document contains a Table of Contents.
		/// </summary>
		public bool IncludesTableOfContents => this.includesTableOfContents;

		/// <summary>
		/// If the contents of the document is dynamic (i.e. includes script), or not (i.e. is static).
		/// </summary>
		public bool IsDynamic => this.isDynamic || (this.master?.isDynamic ?? false);

		/// <summary>
		/// Property can be used to tag document with client-specific information.
		/// </summary>
		public object Tag
		{
			get => this.tag;
			set => this.tag = value;
		}

		/// <summary>
		/// If client-side script tags are allowed in the document.
		/// </summary>
		public bool AllowScriptTag
		{
			get
			{
				if (!this.allowScriptTag.HasValue)
				{
					this.allowScriptTag = this.metaData.TryGetValue("ALLOWSCRIPTTAG", out KeyValuePair<string, bool>[] Value) &&
						Value.Length > 0 &&
						CommonTypes.TryParse(Value[0].Key, out bool b) &&
						b;
				}

				return this.allowScriptTag.Value;
			}
		}

		/// <summary>
		/// Loops through all elements in the document.
		/// </summary>
		/// <param name="Callback">Method called for each one of the elements.</param>
		/// <param name="State">State object passed on to the callback method.</param>
		/// <returns>If the operation was completed.</returns>
		public bool ForEach(MarkdownElementHandler Callback, object State)
		{
			ChunkNode<MarkdownElement> Loop = this.elements?.FirstChunk;
			int i, c;

			while (!(Loop is null))
			{
				for (i = Loop.Start, c = Loop.Pos; i < c; i++)
				{
					if (!Loop[i].ForEach(Callback, State))
						return false;
				}

				Loop = Loop.Next;
			}

			if (!(this.references is null))
			{
				foreach (Multimedia E in this.references.Values)
				{
					if (!E.ForEach(Callback, State))
						return false;
				}
			}

			if (!(this.footnotes is null))
			{
				foreach (Footnote E in this.footnotes.Values)
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
		/// Finds hashtags in the document.
		/// </summary>
		/// <returns>Array of hashtags found in the document.</returns>
		public string[] FindHashTags()
		{
			SortedDictionary<string, bool> Tags = new SortedDictionary<string, bool>();

			this.ForEach((E, Obj) =>
			{
				if (E is HashTag Tag)
					Tags[Tag.Tag] = true;

				return true;
			}, null);

			string[] Result = new string[Tags.Count];
			Tags.Keys.CopyTo(Result, 0);
			return Result;
		}

		/// <summary>
		/// Markdown elements making up the document.
		/// </summary>
		public ChunkedList<MarkdownElement> Elements => this.elements;

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

		/// <summary>
		/// Calculates the difference of two Markdown documents.
		/// </summary>
		/// <param name="Old">Old version of the document.</param>
		/// <param name="New">New version of the document.</param>
		/// <param name="KeepUnchanged">If unchanged parts of the document should be kept.</param>
		/// <returns>Difference document</returns>
		public static Task<MarkdownDocument> Compare(MarkdownDocument Old, MarkdownDocument New, bool KeepUnchanged)
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
		public static async Task<string> Compare(string Old, string New, MarkdownSettings Settings, bool KeepUnchanged,
			params Type[] TransparentExceptionTypes)
		{
			MarkdownDocument OldDoc = await CreateAsync(Old, Settings, TransparentExceptionTypes);
			MarkdownDocument NewDoc = await CreateAsync(New, Settings, TransparentExceptionTypes);
			MarkdownDocument DiffDoc = await Compare(OldDoc, NewDoc, KeepUnchanged);

			return await DiffDoc.GenerateMarkdown(false);
		}

		/// <summary>
		/// Calculates the difference of the current Markdown document, and a previous version of the Markdown document.
		/// </summary>
		/// <param name="Previous">Previous version</param>
		/// <param name="KeepUnchanged">If unchanged parts of the document should be kept.</param>
		/// <returns>Difference document</returns>
		public async Task<MarkdownDocument> Compare(MarkdownDocument Previous, bool KeepUnchanged)
		{
			// TODO: Meta-data

			MarkdownDocument Result = await CreateAsync(string.Empty, this.settings, this.transparentExceptionTypes);

			Result.elements.AddRange(Compare(Previous.elements, this.elements, KeepUnchanged, Result));

			// TODO: Footnotes

			Result.markdownText = null; // Triggers export, if needed.
			return Result;
		}

		private static ChunkedList<MarkdownElement> Atomize(ChunkedList<MarkdownElement> Elements, out bool Reassemble)
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

		private static ChunkedList<MarkdownElement> Atomize(ChunkedList<MarkdownElement> Elements)
		{
			ChunkedList<MarkdownElement> Result = new ChunkedList<MarkdownElement>();
			ChunkNode<MarkdownElement> Loop = Elements.FirstChunk;
			MarkdownElement E;
			int i, c;

			while (!(Loop is null))
			{
				for (i = Loop.Start, c = Loop.Pos; i < c; i++)
				{
					E = Loop[i];

					if (E is IEditableText EditableText)
						Result.AddRange(EditableText.Atomize());
					else
						Result.Add(E);
				}

				Loop = Loop.Next;
			}

			return Result;
		}

		private static bool ContainsEditableText(ChunkedList<MarkdownElement> Elements)
		{
			ChunkNode<MarkdownElement> Loop = Elements.FirstChunk;
			int i, c;

			while (!(Loop is null))
			{
				for (i = Loop.Start, c = Loop.Pos; i < c; i++)
				{
					if (Loop[i] is IEditableText)
						return true;
				}

				Loop = Loop.Next;
			}

			return false;
		}

		private static ChunkedList<MarkdownElement> Compare(ChunkedList<MarkdownElement> Elements1,
			ChunkedList<MarkdownElement> Elements2, bool KeepUnchanged, MarkdownDocument Document)
		{
			ChunkedList<MarkdownElement> Result = new ChunkedList<MarkdownElement>();
			MarkdownElement[] S1 = Atomize(Elements1, out bool Reassemble1).ToArray();
			MarkdownElement[] S2 = Atomize(Elements2, out bool Reassemble2).ToArray();
			EditScript<MarkdownElement> Script = Difference.Analyze(S1, S2);
			Step<MarkdownElement> Step, Step2;
			int i, c = Script.Steps.Length;

			if (Reassemble1 || Reassemble2)
			{
				ChunkedList<MarkdownElement> Reassembled = new ChunkedList<MarkdownElement>();
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
					MarkdownElement E;
					int j, d;

					for (j = 0, d = Step.Symbols.Length; j < d; j++)
					{
						E = Step.Symbols[j];

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

					Result.AddRange(Step.Symbols);
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
								ChunkedList<MarkdownElement> Diff = Compare(Children1.Children, Children2.Children,
									KeepUnchanged || d > 1, Document);

								Result.Add(Children1.Create(Diff, Document));
							}
							else if (E1 is MarkdownElementSingleChild Child1 &&
								E2 is MarkdownElementSingleChild Child2 &&
								Child1.Child.SameMetaData(Child2.Child) &&
								Child1.Child is MarkdownElementChildren GrandChildren1 &&
								Child2.Child is MarkdownElementChildren GrandChildren2)
							{
								ChunkedList<MarkdownElement> Diff = Compare(GrandChildren1.Children, GrandChildren2.Children,
									KeepUnchanged || d > 1, Document);

								Result.Add(Child1.Create(GrandChildren1.Create(Diff, Document), Document));
							}
							else
							{
								Result.Add(GetElement(Step.Operation, Document, E1));
								Result.Add(GetElement(Step2.Operation, Document, E2));
							}
						}

						i++;
					}
					else
						Result.Add(GetElement(Step.Operation, Document, Step.Symbols));
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
						return new InsertBlocks(Document, new ChunkedList<MarkdownElement>(Symbols));

					case EditOperation.Delete:
						return new DeleteBlocks(Document, new ChunkedList<MarkdownElement>(Symbols));
				}
			}
			else
			{
				switch (Operation)
				{
					case EditOperation.Insert:
						return new Insert(Document, new ChunkedList<MarkdownElement>(Symbols));

					case EditOperation.Delete:
						return new Delete(Document, new ChunkedList<MarkdownElement>(Symbols));
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

		/// <summary>
		/// Queues an asynchronous task to be executed. Asynchronous tasks will be executed after the main document
		/// has been generated.
		/// </summary>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object.</param>
		public void QueueAsyncTask(AsyncMarkdownProcessing Callback, object State)
		{
			lock (this.asyncTasks)
			{
				this.asyncTasks.Add(new KeyValuePair<AsyncMarkdownProcessing, object>(Callback, State));
			}
		}

		/// <summary>
		/// Enumerable set of asynchronous tasks that have been registered.
		/// </summary>
		public IEnumerable<KeyValuePair<AsyncMarkdownProcessing, object>> AsyncTasks
		{
			get
			{
				lock (this.asyncTasks)
				{
					return this.asyncTasks.ToArray();
				}
			}
		}

		/// <summary>
		/// Processes any registered asynchronous tasks. This method is normally only called from renderers of documents.
		/// </summary>
		public void ProcessAsyncTasks()
		{
			KeyValuePair<AsyncMarkdownProcessing, object>[] Tasks;

			lock (this.asyncTasks)
			{
				if (this.asyncTasks.Count == 0)
					return;

				Tasks = this.asyncTasks.ToArray();
				this.asyncTasks.Clear();
			}

			Task.Run(async () =>
			{
				foreach (KeyValuePair<AsyncMarkdownProcessing, object> P in Tasks)
				{
					try
					{
						await P.Key(P.Value);
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}
				}
			});
		}

		/// <summary>
		/// Transforms XML to an object that is easier to visualize.
		/// </summary>
		/// <param name="Xml">XML Document.</param>
		/// <param name="Variables">Current variables.</param>
		/// <returns>Transformed object (possibly the same if no XML Visualizer found).</returns>
		public static async Task<object> TransformXml(XmlDocument Xml, Variables Variables)
		{
			try
			{
				IXmlVisualizer Visualizer = CodeBlock.GetXmlVisualizerHandler(Xml);
				if (Visualizer is null)
					return Xml;

				return (await Visualizer.TransformXml(Xml, Variables)) ?? Xml;
			}
			catch (Exception ex)
			{
				return ex;
			}
		}

		/// <summary>
		/// Returns some basic statistics about the contents of the Markdown object.
		/// </summary>
		/// <returns>Markdown statistics.</returns>
		public MarkdownStatistics GetStatistics()
		{
			MarkdownStatistics Result = new MarkdownStatistics();

			this.ForEach((Element, _) =>
			{
				Result.NrElements++;
				Element.IncrementStatistics(Result);
				return true;
			}, null);

			Result.MailHyperlinks = Result.IntMailHyperlinks?.ToArray();
			Result.UrlHyperlinks = Result.IntUrlHyperlinks?.ToArray();

			this.GenerateStatDictionary(Result.IntMultimediaPerContentCategory,
				out Dictionary<string, string[]> AsArrays, out Dictionary<string, int> AsCounts);

			Result.MultimediaPerContentCategory = AsArrays;
			Result.NrMultimediaPerContentCategory = AsCounts;

			this.GenerateStatDictionary(Result.IntMultimediaPerContentType, out AsArrays, out AsCounts);
			Result.MultimediaPerContentType = AsArrays;
			Result.NrMultimediaPerContentType = AsCounts;

			this.GenerateStatDictionary(Result.IntMultimediaPerExtension, out AsArrays, out AsCounts);
			Result.MultimediaPerExtension = AsArrays;
			Result.NrMultimediaPerExtension = AsCounts;

			return Result;
		}

		private void GenerateStatDictionary(Dictionary<string, ChunkedList<string>> Temp, out Dictionary<string, string[]> AsArrays, out Dictionary<string, int> AsCounts)
		{
			if (Temp is null)
			{
				AsArrays = null;
				AsCounts = null;
				return;
			}

			AsArrays = new Dictionary<string, string[]>();
			AsCounts = new Dictionary<string, int>();

			foreach (KeyValuePair<string, ChunkedList<string>> P in Temp)
			{
				AsArrays[P.Key] = P.Value.ToArray();
				AsCounts[P.Key] = P.Value.Count;
			}
		}

		/// <summary>
		/// Appends a set of rows into a single string with newlines between rows.
		/// </summary>
		/// <param name="Rows">Rows</param>
		/// <returns>Appended rows.</returns>
		public static string AppendRows(string[] Rows)
		{
			return AppendRows(Rows, false);
		}

		/// <summary>
		/// Appends a set of rows into a single string with newlines between rows.
		/// </summary>
		/// <param name="Rows">Rows</param>
		/// <param name="SingleRow">If rows should be concatenated into a single row.</param>
		/// <returns>Appended rows.</returns>
		public static string AppendRows(string[] Rows, bool SingleRow)
		{
			if (SingleRow && Rows.Length == 1)
				return Rows[0].Trim();

			StringBuilder sb = new StringBuilder();

			foreach (string Row in Rows)
			{
				if (SingleRow)
					sb.Append(Row.Trim());
				else
					sb.AppendLine(Row);
			}

			return sb.ToString();
		}

		/// <summary>
		/// To what extent the object supports JSON encoding.
		/// </summary>
		public Grade CanEncodeJson => Grade.NotAtAll;   // Document reference from child nodes create a stack overflow.

		// TODO: Footnotes in included markdown files.
	}
}

