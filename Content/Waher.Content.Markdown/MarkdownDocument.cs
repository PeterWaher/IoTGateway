using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content.Emoji;
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
	/// Typographic enhancements inspired by the Smarty Pants addition for markdown is also supported:
	/// http://daringfireball.net/projects/smartypants/
	/// 
	/// There are however some differences, and some definitions where the implementation in <see cref="Waher.Content.Markdown"/> differ:
	/// 
	/// - Markdown syntax within block-level HTML constructs is allowed.
	/// - Numbered lists retain the number used in the text.
	/// - Lazy numbering supported by prefixing items using "#. " instead of using actual numbers.
	/// - _underline_ underlines text.
	/// - __inserted__ displays inserted text.
	/// - ~strike through~ strikes through text.
	/// - ~~deleted~~ displays deleted text.
	/// - `` is solely used to display code. Curly quotes are inserted using normal ".
	/// - Headers receive automatic id's (camel casing).
	/// - Emojis are supported using the shortname syntax `:shortname:`.
	/// - Smileys are supported, and converted to emojis. Inspired from: http://git.emojione.com/demos/ascii-smileys.html
	/// 
	/// - Any multimedia, not just images, can be inserted using the ! syntax, including audio and video. The architecture is pluggable and allows for 
	///   customization of inclusion of content, including web content such as YouTube videos, etc.
	///   
	///   Linking to a local markdown file will include the file into the context of the document. This allows for markdown templates to be used, and 
	///   for more complex constructs, such as richer tables, to be built.
	///   
	///   Multimedia can have additional width and height information. Multimedia handler is selected based on URL or file extension. If no particular 
	///   multimedia handler is found, the source is considered to be an image.
	///   
	///   Examples:
	///   
	///	    ![some text](/some/url "some title" WIDTH HEIGHT) where WIDTH and HEIGHT are positive integers.
	///     ![Your browser does not support the audio tag](/local/music.mp3)            (is rendered using the &lt;audio&gt; tag)
	///     ![Your browser does not support the video tag](/local/video.mp4 320 200)    (is rendered using the &lt;video&gt; tag)
	///     ![Your browser does not support the iframe tag](https://www.youtube.com/watch?v=whBPLc8m4SU 800 600)
	///     ![Table of Contents](ToC)		(ToC is case insensitive)
	///
	///   Width and Height can also be defined in referenced content. Example: ![some text][someref]
	///   [someref]: some/url "some title" WIDTH HEIGHT
	///
	///   Multiresolution or Multiformatted multimedia can be included by including a sequence of sources. If inline mode is used, each source is written
	///   between a set of parenthesis. The sources are then optionally separated by whitespace (inluding on a new row).
	///   
	/// - Typographical additions include:
	///     (c)				©		&copy;
	///     (C)				©		&COPY;
	///     (r)				®		&reg;
	///     (R)				®		&REG;
	///     (p)				℗		&copysr;
	///     (P)				℗		&copysr;
	///     (s)				Ⓢ		&oS;
	///     (S)				Ⓢ		&circledS;
	///     &lt;&lt;		«		&laquo;
	///     &gt;&gt;		»		&raquo;
	///     &lt;&lt;&lt;	⋘		&Ll;
	///     &gt;&gt;&gt;	⋙		&Gg;
	///     &lt;--			←		&larr;
	///     --&gt;			→		&rarr;
	///     &lt;--&gt;		↔		&harr;
	///     &lt;==			⇐		&lArr;
	///     ==&gt;			⇒		&rArr;
	///     &lt;==&gt;		⇔		&hArr;
	///     [[				⟦		&LeftDoubleBracket;
	///     ]]				⟧		&RightDoubleBracket;
	///     +-				±		&PlusMinus;
	///     -+				∓		&MinusPlus;
	///     &lt;&gt;		≠		&ne;
	///     &lt;=			≤		&leq;
	///     &gt;=			≥		&geq;
	///     ==				≡		&equiv;
	///     ^a				ª		&ordf;
	///     ^o				º		&ordm;
	///     ^0				°		&deg;
	///     ^1				¹		&#185;
	///     ^2				²		&#178;
	///     ^3				³		&#179;
	///     ^TM				™		&trade;
	///     %0				‰		&permil;
	///     %00				‱		&pertenk;
	///     
	/// Selected features from MultiMarkdown (https://rawgit.com/fletcher/human-markdown-reference/master/index.html) and
	/// Markdown Extra (https://michelf.ca/projects/php-markdown/extra/) have also been included:
	/// 
	/// - Images placed in a paragraph by itself is wrapped in a &lt;figure&gt; tag.
	/// - Tables.
	/// - Definition lists.
	/// - Metadata
	/// - Footnotes.
	/// - Fenced code blocks, with syntax highlighting.
	/// 
	/// Meta-data tags that are recognized by the parser are, as follows. Other meta-data tags are simply copied into the meta-data section of the 
	/// generated HTML document. Keys are case insensitive.
	/// 
	/// - Title			Title of document.
	/// - Subtitle		Subtitle of document.
	/// - Description	Description of document.
	/// - Author		Author(s) of document.
	/// - Date			(Publication) date of document.
	/// - Copyright		Link to copyright statement.
	/// - Previous		Link to previous document, in a paginated set of documents.
	/// - Prev			Synonymous with Previous.
	/// - Next			Link to next document, in a paginated set of documents.
	/// - Alternate		Link to alternate page.
	/// - Help			Link to help page.
	/// - Icon			Link to icon for page.
	/// - CSS			Link(s) to Cascading Style Sheet(s) that should be used for visial formatting of the generated HTML page.
	/// - Keywords		Keywords.
	/// - Image			Link to image for page.
	/// - Web			Link to web page
	/// </summary>
	public class MarkdownDocument
	{
		private Dictionary<string, Multimedia> references = new Dictionary<string, Multimedia>();
		private Dictionary<string, KeyValuePair<string, bool>[]> metaData = new Dictionary<string, KeyValuePair<string, bool>[]>();
		private Dictionary<string, int> footnoteNumbers = null;
		private Dictionary<string, Footnote> footnotes = null;
		private List<string> footnoteOrder = null;
		private LinkedList<MarkdownElement> elements;
		private List<Header> headers = new List<Header>();
		private IEmojiSource emojiSource;
		private string markdownText;
		private int lastFootnote = 0;
		private bool footnoteBacklinksAdded = false;
		private bool syntaxHighlighting = false;

		/// <summary>
		/// Contains a markdown document. This markdown document class supports original markdown, as well as several markdown extensions, as
		/// defined in the following links.
		/// 
		/// Original Markdown was invented by John Gruber at Daring Fireball.
		/// http://daringfireball.net/projects/markdown/basics
		/// http://daringfireball.net/projects/markdown/syntax
		/// 
		/// Typographic enhancements inspired by the Smarty Pants addition for markdown is also supported:
		/// http://daringfireball.net/projects/smartypants/
		/// 
		/// There are however some differences, and some definitions where the implementation in <see cref="Waher.Content.Markdown"/> differ:
		/// 
		/// - Markdown syntax within block-level HTML constructs is allowed.
		/// - Numbered lists retain the number used in the text.
		/// - Lazy numbering supported by prefixing items using "#. " instead of using actual numbers.
		/// - _underline_ underlines text.
		/// - __inserted__ displays inserted text.
		/// - ~strike through~ strikes through text.
		/// - ~~deleted~~ displays deleted text.
		/// - `` is solely used to display code. Curly quotes are inserted using normal ".
		/// - Headers receive automatic id's (camel casing).
		/// - Emojis are supported using the shortname syntax `:shortname:`.
		/// - Smileys are supported, and converted to emojis. Inspired from: http://git.emojione.com/demos/ascii-smileys.html
		/// 
		/// - Any multimedia, not just images, can be inserted using the ! syntax, including audio and video. The architecture is pluggable and allows for 
		///   customization of inclusion of content, including web content such as YouTube videos, etc.
		///   
		///   Linking to a local markdown file will include the file into the context of the document. This allows for markdown templates to be used, and 
		///   for more complex constructs, such as richer tables, to be built.
		///   
		///   Multimedia can have additional width and height information. Multimedia handler is selected based on URL or file extension. If no particular 
		///   multimedia handler is found, the source is considered to be an image.
		///   
		///   Examples:
		///   
		///	    ![some text](/some/url "some title" WIDTH HEIGHT) where WIDTH and HEIGHT are positive integers.
		///     ![Your browser does not support the audio tag](/local/music.mp3)            (is rendered using the &lt;audio&gt; tag)
		///     ![Your browser does not support the video tag](/local/video.mp4 320 200)    (is rendered using the &lt;video&gt; tag)
		///     ![Your browser does not support the iframe tag](https://www.youtube.com/watch?v=whBPLc8m4SU 800 600)
		///     ![Table of Contents](ToC)		(ToC is case insensitive)
		///
		///   Width and Height can also be defined in referenced content. Example: ![some text][someref]
		///   [someref]: some/url "some title" WIDTH HEIGHT
		///
		///   Multiresolution or Multiformatted multimedia can be included by including a sequence of sources. If inline mode is used, each source is written
		///   between a set of parenthesis. The sources are then optionally separated by whitespace (inluding on a new row).
		///   
		/// - Typographical additions include:
		///     (c)				©		&copy;
		///     (C)				©		&COPY;
		///     (r)				®		&reg;
		///     (R)				®		&REG;
		///     (p)				℗		&copysr;
		///     (P)				℗		&copysr;
		///     (s)				Ⓢ		&oS;
		///     (S)				Ⓢ		&circledS;
		///     &lt;&lt;		«		&laquo;
		///     &gt;&gt;		»		&raquo;
		///     &lt;&lt;&lt;	⋘		&Ll;
		///     &gt;&gt;&gt;	⋙		&Gg;
		///     &lt;--			←		&larr;
		///     --&gt;			→		&rarr;
		///     &lt;--&gt;		↔		&harr;
		///     &lt;==			⇐		&lArr;
		///     ==&gt;			⇒		&rArr;
		///     &lt;==&gt;		⇔		&hArr;
		///     [[				⟦		&LeftDoubleBracket;
		///     ]]				⟧		&RightDoubleBracket;
		///     +-				±		&PlusMinus;
		///     -+				∓		&MinusPlus;
		///     &lt;&gt;		≠		&ne;
		///     &lt;=			≤		&leq;
		///     &gt;=			≥		&geq;
		///     ==				≡		&equiv;
		///     ^a				ª		&ordf;
		///     ^o				º		&ordm;
		///     ^0				°		&deg;
		///     ^1				¹		&#185;
		///     ^2				²		&#178;
		///     ^3				³		&#179;
		///     ^TM				™		&trade;
		///     %0				‰		&permil;
		///     %00				‱		&pertenk;
		///     
		/// Selected features from MultiMarkdown (https://rawgit.com/fletcher/human-markdown-reference/master/index.html) and
		/// Markdown Extra (https://michelf.ca/projects/php-markdown/extra/) have also been included:
		/// 
		/// - Images placed in a paragraph by itself is wrapped in a &lt;figure&gt; tag.
		/// - Tables.
		/// - Definition lists.
		/// - Metadata
		/// - Footnotes.
		/// - Fenced code blocks, with syntax highlighting.
		/// 
		/// Meta-data tags that are recognized by the parser are, as follows. Other meta-data tags are simply copied into the meta-data section of the 
		/// generated HTML document. Keys are case insensitive.
		/// 
		/// - Title			Title of document.
		/// - Subtitle		Subtitle of document.
		/// - Description	Description of document.
		/// - Author		Author(s) of document.
		/// - Date			(Publication) date of document.
		/// - Copyright		Link to copyright statement.
		/// - Previous		Link to previous document, in a paginated set of documents.
		/// - Prev			Synonymous with Previous.
		/// - Next			Link to next document, in a paginated set of documents.
		/// - Alternate		Link to alternate page.
		/// - Help			Link to help page.
		/// - Icon			Link to icon for page.
		/// - CSS			Link(s) to Cascading Style Sheet(s) that should be used for visial formatting of the generated HTML page.
		/// - Keywords		Keywords.
		/// - Image			Link to image for page.
		/// - Web			Link to web page
		/// </summary>
		/// <param name="MarkdownText">Markdown text.</param>
		public MarkdownDocument(string MarkdownText)
			: this(MarkdownText, new MarkdownSettings())
		{
		}

		/// <summary>
		/// Contains a markdown document. This markdown document class supports original markdown, as well as several markdown extensions, as
		/// defined in the following links.
		/// 
		/// Original Markdown was invented by John Gruber at Daring Fireball.
		/// http://daringfireball.net/projects/markdown/basics
		/// http://daringfireball.net/projects/markdown/syntax
		/// 
		/// Typographic enhancements inspired by the Smarty Pants addition for markdown is also supported:
		/// http://daringfireball.net/projects/smartypants/
		/// 
		/// There are however some differences, and some definitions where the implementation in <see cref="Waher.Content.Markdown"/> differ:
		/// 
		/// - Markdown syntax within block-level HTML constructs is allowed.
		/// - Numbered lists retain the number used in the text.
		/// - Lazy numbering supported by prefixing items using "#. " instead of using actual numbers.
		/// - _underline_ underlines text.
		/// - __inserted__ displays inserted text.
		/// - ~strike through~ strikes through text.
		/// - ~~deleted~~ displays deleted text.
		/// - `` is solely used to display code. Curly quotes are inserted using normal ".
		/// - Headers receive automatic id's (camel casing).
		/// - Emojis are supported using the shortname syntax `:shortname:`.
		/// - Smileys are supported, and converted to emojis. Inspired from: http://git.emojione.com/demos/ascii-smileys.html
		/// 
		/// - Any multimedia, not just images, can be inserted using the ! syntax, including audio and video. The architecture is pluggable and allows for 
		///   customization of inclusion of content, including web content such as YouTube videos, etc.
		///   
		///   Linking to a local markdown file will include the file into the context of the document. This allows for markdown templates to be used, and 
		///   for more complex constructs, such as richer tables, to be built.
		///   
		///   Multimedia can have additional width and height information. Multimedia handler is selected based on URL or file extension. If no particular 
		///   multimedia handler is found, the source is considered to be an image.
		///   
		///   Examples:
		///   
		///	    ![some text](/some/url "some title" WIDTH HEIGHT) where WIDTH and HEIGHT are positive integers.
		///     ![Your browser does not support the audio tag](/local/music.mp3)            (is rendered using the &lt;audio&gt; tag)
		///     ![Your browser does not support the video tag](/local/video.mp4 320 200)    (is rendered using the &lt;video&gt; tag)
		///     ![Your browser does not support the iframe tag](https://www.youtube.com/watch?v=whBPLc8m4SU 800 600)
		///     ![Table of Contents](ToC)		(ToC is case insensitive)
		///
		///   Width and Height can also be defined in referenced content. Example: ![some text][someref]
		///   [someref]: some/url "some title" WIDTH HEIGHT
		///
		///   Multiresolution or Multiformatted multimedia can be included by including a sequence of sources. If inline mode is used, each source is written
		///   between a set of parenthesis. The sources are then optionally separated by whitespace (inluding on a new row).
		///   
		/// - Typographical additions include:
		///     (c)				©		&copy;
		///     (C)				©		&COPY;
		///     (r)				®		&reg;
		///     (R)				®		&REG;
		///     (p)				℗		&copysr;
		///     (P)				℗		&copysr;
		///     (s)				Ⓢ		&oS;
		///     (S)				Ⓢ		&circledS;
		///     &lt;&lt;		«		&laquo;
		///     &gt;&gt;		»		&raquo;
		///     &lt;&lt;&lt;	⋘		&Ll;
		///     &gt;&gt;&gt;	⋙		&Gg;
		///     &lt;--			←		&larr;
		///     --&gt;			→		&rarr;
		///     &lt;--&gt;		↔		&harr;
		///     &lt;==			⇐		&lArr;
		///     ==&gt;			⇒		&rArr;
		///     &lt;==&gt;		⇔		&hArr;
		///     [[				⟦		&LeftDoubleBracket;
		///     ]]				⟧		&RightDoubleBracket;
		///     +-				±		&PlusMinus;
		///     -+				∓		&MinusPlus;
		///     &lt;&gt;		≠		&ne;
		///     &lt;=			≤		&leq;
		///     &gt;=			≥		&geq;
		///     ==				≡		&equiv;
		///     ^a				ª		&ordf;
		///     ^o				º		&ordm;
		///     ^0				°		&deg;
		///     ^1				¹		&#185;
		///     ^2				²		&#178;
		///     ^3				³		&#179;
		///     ^TM				™		&trade;
		///     %0				‰		&permil;
		///     %00				‱		&pertenk;
		///     
		/// Selected features from MultiMarkdown (https://rawgit.com/fletcher/human-markdown-reference/master/index.html) and
		/// Markdown Extra (https://michelf.ca/projects/php-markdown/extra/) have also been included:
		/// 
		/// - Images placed in a paragraph by itself is wrapped in a &lt;figure&gt; tag.
		/// - Tables.
		/// - Definition lists.
		/// - Metadata
		/// - Footnotes.
		/// - Fenced code blocks, with syntax highlighting.
		/// 
		/// Meta-data tags that are recognized by the parser are, as follows. Other meta-data tags are simply copied into the meta-data section of the 
		/// generated HTML document. Keys are case insensitive.
		/// 
		/// - Title			Title of document.
		/// - Subtitle		Subtitle of document.
		/// - Description	Description of document.
		/// - Author		Author(s) of document.
		/// - Date			(Publication) date of document.
		/// - Copyright		Link to copyright statement.
		/// - Previous		Link to previous document, in a paginated set of documents.
		/// - Prev			Synonymous with Previous.
		/// - Next			Link to next document, in a paginated set of documents.
		/// - Alternate		Link to alternate page.
		/// - Help			Link to help page.
		/// - Icon			Link to icon for page.
		/// - CSS			Link(s) to Cascading Style Sheet(s) that should be used for visial formatting of the generated HTML page.
		/// - Keywords		Keywords.
		/// - Image			Link to image for page.
		/// - Web			Link to web page
		/// </summary>
		/// <param name="MarkdownText">Markdown text.</param>
		/// <param name="Settings">Parser settings.</param>
		public MarkdownDocument(string MarkdownText, MarkdownSettings Settings)
		{
			this.markdownText = MarkdownText;
			this.emojiSource = Settings.EmojiSource;

			List<Block> Blocks = this.ParseTextToBlocks(MarkdownText);
			List<KeyValuePair<string, bool>> Values = new List<KeyValuePair<string, bool>>();
			Block Block;
			KeyValuePair<string, bool>[] Prev;
			string s, s2;
			string Key = null;
			int Start = 0;
			int End = Blocks.Count - 1;
			int i, j;

			if (Settings.ParseMetaData)
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

							if (s2 == null)
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

					this.metaData[Key] = Values.ToArray();
					Start++;
				}
			}

			this.elements = this.ParseBlocks(Blocks, Start, End);
		}

		private LinkedList<MarkdownElement> ParseBlocks(List<Block> Blocks)
		{
			return this.ParseBlocks(Blocks, 0, Blocks.Count - 1);
		}

		private LinkedList<MarkdownElement> ParseBlocks(List<Block> Blocks, int StartBlock, int EndBlock)
		{
			LinkedList<MarkdownElement> Elements = new LinkedList<MarkdownElement>();
			LinkedList<MarkdownElement> Content;
			TableInformation TableInformation;
			Block Block;
			string[] Rows;
			string s, s2;
			int BlockIndex;
			int i, j, c, d;
			int Index;

			for (BlockIndex = StartBlock; BlockIndex <= EndBlock; BlockIndex++)
			{
				Block = Blocks[BlockIndex];

				if (Block.Indent > 0)
				{
					Elements.AddLast(new CodeBlock(this, Block.Rows, Block.Start, Block.End, Block.Indent - 1));
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

					if (Elements.Last != null && Elements.Last.Value is BlockQuote)
						((BlockQuote)Elements.Last.Value).AddChildren(Content);
					else
						Elements.AddLast(new BlockQuote(this, Content));

					continue;
				}
				else if (Block.End == Block.Start && (this.IsUnderline(Block.Rows[0], '-', true) || this.IsUnderline(Block.Rows[0], '*', true)))
				{
					Elements.AddLast(new HorizontalRule(this));
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
						if (IsPrefixedBy(s, "*", true) || IsPrefixedBy(s, "+", true) || IsPrefixedBy(s, "-", true))
						{
							if (Segments == null)
								Segments = new LinkedList<Block>();

							Segments.AddLast(new Block(Block.Rows, 0, i, d - 1));
							i = d;
						}
					}

					if (Segments != null)
						Segments.AddLast(new Block(Block.Rows, 0, i, c));

					if (Segments == null)
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

						if (Elements.Last != null && Elements.Last.Value is BulletList)
							((BulletList)Elements.Last.Value).AddChildren(new UnnumberedItem(this, s2 + " ", new NestedBlock(this, Items)));
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
									this.ParseBlock(SegmentItem.Rows, SegmentItem.Start, SegmentItem.End))));
							}
						}

						if (Elements.Last != null && Elements.Last.Value is BulletList)
							((BulletList)Elements.Last.Value).AddChildren(Items);
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
							if (Segments == null)
								Segments = new LinkedList<Block>();

							Segments.AddLast(new Block(Block.Rows, 0, i, d - 1));
							i = d;
						}
					}

					if (Segments != null)
						Segments.AddLast(new Block(Block.Rows, 0, i, c));

					if (Segments == null)
					{
						LinkedList<MarkdownElement> Items = this.ParseBlocks(Block.RemovePrefix("#.", 5));

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

						if (Elements.Last != null && Elements.Last.Value is NumberedList)
							((NumberedList)Elements.Last.Value).AddChildren(new UnnumberedItem(this, "#. ", new NestedBlock(this, Items)));
						else
							Elements.AddLast(new NumberedList(this, new UnnumberedItem(this, "#. ", new NestedBlock(this, Items))));

						continue;
					}
					else
					{
						LinkedList<MarkdownElement> Items = new LinkedList<MarkdownElement>();

						foreach (Block Segment in Segments)
						{
							foreach (Block SegmentItem in Segment.RemovePrefix("#.", 5))
							{
								Items.AddLast(new UnnumberedItem(this, "#. ", new NestedBlock(this,
									this.ParseBlock(SegmentItem.Rows, SegmentItem.Start, SegmentItem.End))));
							}
						}

						if (Elements.Last != null && Elements.Last.Value is NumberedList)
							((NumberedList)Elements.Last.Value).AddChildren(Items);
						else
							Elements.AddLast(new NumberedList(this, Items));

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
							if (Segments == null)
								Segments = new LinkedList<KeyValuePair<int, Block>>();

							Segments.AddLast(new KeyValuePair<int, Block>(Index, new Block(Block.Rows, 0, i, d - 1)));
							i = d;
							Index = j;
						}
					}

					if (Segments != null)
						Segments.AddLast(new KeyValuePair<int, Block>(Index, new Block(Block.Rows, 0, i, c)));

					if (Segments == null)
					{
						s = Index.ToString();
						LinkedList<MarkdownElement> Items = this.ParseBlocks(Block.RemovePrefix(s + ".", s.Length + 4));

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

						if (Elements.Last != null && Elements.Last.Value is NumberedList)
							((NumberedList)Elements.Last.Value).AddChildren(new NumberedItem(this, Index, new NestedBlock(this, Items)));
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
							foreach (Block SegmentItem in Segment.Value.RemovePrefix(s + ".", s.Length + 4))
							{
								Items.AddLast(new NumberedItem(this, Segment.Key, new NestedBlock(this,
									this.ParseBlock(SegmentItem.Rows, SegmentItem.Start, SegmentItem.End))));
							}
						}

						if (Elements.Last != null && Elements.Last.Value is NumberedList)
							((NumberedList)Elements.Last.Value).AddChildren(Items);
						else
							Elements.AddLast(new NumberedList(this, Items));

						continue;
					}
				}
				else if (Block.IsTable(out TableInformation))
				{
					MarkdownElement[][] Headers = new MarkdownElement[TableInformation.NrHeaderRows][];
					MarkdownElement[][] DataRows = new MarkdownElement[TableInformation.NrDataRows][];
					LinkedList<MarkdownElement> CellElements;
					string[] Row;

					c = TableInformation.Columns;

					for (j = 0; j < TableInformation.NrHeaderRows; j++)
					{
						Row = TableInformation.Headers[j];
						Headers[j] = new MarkdownElement[c];

						for (i = 0; i < c; i++)
						{
							s = Row[i];
							if (s == null)
								Headers[j][i] = null;
							else
							{
								CellElements = this.ParseBlock(new string[] { Row[i] });

								if (CellElements.First != null && CellElements.First.Next == null)
									Headers[j][i] = CellElements.First.Value;
								else
									Headers[j][i] = new NestedBlock(this, CellElements);
							}
						}
					}

					for (j = 0; j < TableInformation.NrDataRows; j++)
					{
						Row = TableInformation.Rows[j];
						DataRows[j] = new MarkdownElement[c];

						for (i = 0; i < c; i++)
						{
							s = Row[i];
							if (s == null)
								DataRows[j][i] = null;
							else
							{
								CellElements = this.ParseBlock(new string[] { Row[i] });

								if (CellElements.First != null && CellElements.First.Next == null)
									DataRows[j][i] = CellElements.First.Value;
								else
									DataRows[j][i] = new NestedBlock(this, CellElements);
							}
						}
					}

					Elements.AddLast(new Table(this, c, Headers, DataRows, TableInformation.Alignments, TableInformation.Caption, TableInformation.Id));
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

					if (Description.First == null)
						continue;

					if (Description.First.Next == null)
						DefinitionDescriptions = new DefinitionDescriptions(this, Description);
					else
						DefinitionDescriptions = new DefinitionDescriptions(this, new NestedBlock(this, Description));

					if (Elements.Last.Value is DefinitionDescriptions)
						((DefinitionDescriptions)Elements.Last.Value).AddChildren(DefinitionDescriptions.Children);
					else if (Elements.Last.Value is DefinitionTerms)
						Elements.Last.Value = new DefinitionList(this, Elements.Last.Value, DefinitionDescriptions);
					else if (Elements.Last.Value is DefinitionList)
						((DefinitionList)Elements.Last.Value).AddChildren(DefinitionDescriptions);
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
						Term = this.ParseBlock(Rows, i, i);
						if (Term.First == null)
							continue;

						if (Term.First.Next == null)
							Terms.AddLast(Term.First.Value);
						else
							Terms.AddLast(new NestedBlock(this, Term));
					}

					if (Elements.Last != null && Elements.Last.Value is DefinitionList)
						((DefinitionList)Elements.Last.Value).AddChildren(new DefinitionTerms(this, Terms));
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

					if (this.footnoteNumbers == null)
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

					if (this.IsUnderline(s, '=', false))
					{
						Header Header = new Header(this, 1, this.ParseBlock(Rows, 0, c - 1));
						Elements.AddLast(Header);
						this.headers.Add(Header);
						continue;
					}
					else if (this.IsUnderline(s, '-', false))
					{
						Header Header = new Header(this, 2, this.ParseBlock(Rows, 0, c - 1));
						Elements.AddLast(Header);
						this.headers.Add(Header);
						continue;
					}
				}

				s = Rows[Block.Start];
				if (this.IsPrefixedBy(s, '#', out d) && d < s.Length)
				{
					Rows[Block.Start] = Rows[Block.Start].Substring(d + 1).Trim();

					s = Rows[c];
					i = s.Length - 1;
					while (i >= 0 && s[i] == '#')
						i--;

					if (++i < s.Length)
						Rows[c] = s.Substring(0, i).TrimEnd();

					Header Header = new Header(this, d, this.ParseBlock(Rows, Block.Start, c));
					Elements.AddLast(Header);
					this.headers.Add(Header);
					continue;
				}

				Content = this.ParseBlock(Rows, Block.Start, c);
				if (Content.First != null)
				{
					if (Content.First.Value is InlineHTML && Content.Last.Value is InlineHTML)
						Elements.AddLast(new HtmlBlock(this, Content));
					else if (Content.First.Next == null && Content.First.Value.OutsideParagraph)
					{
						if (Content.First.Value is MarkdownElementChildren &&
							((MarkdownElementChildren)Content.First.Value).JoinOverParagraphs &&
							Elements.Last != null && Elements.Last.Value.GetType() == Content.First.Value.GetType())
						{
							((MarkdownElementChildren)Elements.Last.Value).AddChildren(((MarkdownElementChildren)Content.First.Value).Children);
						}
						else
							Elements.AddLast(Content.First.Value);
					}
					else
						Elements.AddLast(new Paragraph(this, Content));
				}
			}

			return Elements;
		}

		private LinkedList<MarkdownElement> ParseBlock(string[] Rows)
		{
			return this.ParseBlock(Rows, 0, Rows.Length - 1);
		}

		private LinkedList<MarkdownElement> ParseBlock(string[] Rows, int StartRow, int EndRow)
		{
			LinkedList<MarkdownElement> Elements = new LinkedList<MarkdownElement>();
			bool PreserveCrLf = Rows[StartRow].StartsWith("<") && Rows[EndRow].EndsWith(">");
			BlockParseState State = new BlockParseState(Rows, StartRow, EndRow, PreserveCrLf);

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
						if ((ch2 = State.PeekNextCharSameRow()) <= ' ' && ch2 > 0)
						{
							if (State.IsFirstCharOnLine)
							{
								this.AppendAnyText(Elements, Text);

								while ((ch2 = State.PeekNextCharSameRow()) <= ' ' && ch2 > 0)
									State.NextCharSameRow();

								UnnumberedItem Item;
								List<string> Rows = new List<string>();
								Rows.Add(State.RestOfRow());

								while (!State.EOF)
								{
									if ((ch2 = State.PeekNextCharSameRow()) == '*' || ch2 == '+' || ch2 == '-')
									{
										Item = new UnnumberedItem(this, ch + " ", new NestedBlock(this, this.ParseBlock(Rows.ToArray())));

										if (Elements.Last != null && Elements.Last.Value is BulletList)
											((BulletList)Elements.Last.Value).AddChildren(Item);
										else
											Elements.AddLast(new BulletList(this, Item));

										State.NextCharSameRow();
										State.SkipWhitespaceSameRow(3);

										Rows.Clear();
										Rows.Add(State.RestOfRow());
									}
									else
									{
										State.SkipWhitespaceSameRow(4);
										Rows.Add(State.RestOfRow());
									}
								}

								if (Rows.Count > 0)
								{
									Item = new UnnumberedItem(this, ch + " ", new NestedBlock(this, this.ParseBlock(Rows.ToArray())));

									if (Elements.Last != null && Elements.Last.Value is BulletList)
										((BulletList)Elements.Last.Value).AddChildren(Item);
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
							if (this.emojiSource == null)
								ch2 = (char)0;

							switch (ch2)
							{
								case '*':
									State.NextCharSameRow();

									if (this.ParseBlock(State, '*', 2, ChildElements))
										Elements.AddLast(new Strong(this, ChildElements));
									else
										this.FixSyntaxError(Elements, "**", ChildElements);
									break;

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
									Elements.AddLast(new MetaReference(this, Text.ToString()));
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

									if (this.footnoteNumbers == null)
									{
										this.footnoteNumbers = new Dictionary<string, int>();
										this.footnoteOrder = new List<string>();
										this.footnotes = new Dictionary<string, Footnote>();
									}

									try
									{
										Title = Url.ToLower();
										Elements.AddLast(new FootnoteReference(this, System.Xml.XmlConvert.VerifyNCName(Title)));
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
										this.footnotes[Title] = new Footnote(this, Title, new Paragraph(this, this.ParseBlock(new string[] { Url })));
									}
								}
								else
									Text.Insert(0, "[^");

								break;
							}
						}

						ChildElements = new LinkedList<MarkdownElement>();
						this.AppendAnyText(Elements, Text);

						if (this.ParseBlock(State, ']', 1, ChildElements))
						{
							ch2 = State.NextNonWhitespaceChar();
							if (ch2 == '(')
							{
								Title = string.Empty;

								while ((ch2 = State.NextCharSameRow()) != 0 && ch2 > ' ' && ch2 != ')')
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
								{
									List<MultimediaItem> Items = new List<MultimediaItem>();

									Items.Add(new MultimediaItem(Url, Title, Width, Height));

									State.BackupState();
									ch2 = State.NextNonWhitespaceChar();
									while (ch2 == '(')
									{
										Title = string.Empty;

										while ((ch2 = State.NextCharSameRow()) != 0 && ch2 > ' ' && ch2 != ')')
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

										this.ParseWidthHeight(State, out Width, out Height);

										while (ch2 != 0 && ch2 != ')')
											ch2 = State.NextCharSameRow();

										Items.Add(new MultimediaItem(Url, Title, Width, Height));

										State.DiscardBackup();
										State.BackupState();
										ch2 = State.NextNonWhitespaceChar();
									}

									State.RestoreState();
									Elements.AddLast(new Multimedia(this, ChildElements, Elements.First == null && State.PeekNextChar() == 0,
										Items.ToArray()));
								}
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

									this.references[Text.ToString().ToLower()] = new Multimedia(this, null,
										Elements.First == null && State.PeekNextChar() == 0,
										new MultimediaItem[] { new MultimediaItem(Url, Title, Width, Height) });
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
								{
									Elements.AddLast(new MultimediaReference(this, ChildElements, Title,
										Elements.First == null && State.PeekNextChar() == 0));
								}
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
						else if (ch2 <= ' ' && ch2 > 0)
						{
							if (State.IsFirstCharOnLine)
							{
								this.AppendAnyText(Elements, Text);

								while ((ch2 = State.PeekNextCharSameRow()) <= ' ' && ch2 > 0)
									State.NextCharSameRow();

								UnnumberedItem Item;
								List<string> Rows = new List<string>();
								Rows.Add(State.RestOfRow());

								while (!State.EOF)
								{
									if ((ch2 = State.PeekNextCharSameRow()) == '*' || ch2 == '+' || ch2 == '-')
									{
										Item = new UnnumberedItem(this, ch + " ", new NestedBlock(this, this.ParseBlock(Rows.ToArray())));

										if (Elements.Last != null && Elements.Last.Value is BulletList)
											((BulletList)Elements.Last.Value).AddChildren(Item);
										else
											Elements.AddLast(new BulletList(this, Item));

										State.NextCharSameRow();
										State.SkipWhitespaceSameRow(3);

										Rows.Clear();
										Rows.Add(State.RestOfRow());
									}
									else
									{
										State.SkipWhitespaceSameRow(4);
										Rows.Add(State.RestOfRow());
									}
								}

								if (Rows.Count > 0)
								{
									Item = new UnnumberedItem(this, ch + " ", new NestedBlock(this, this.ParseBlock(Rows.ToArray())));

									if (Elements.Last != null && Elements.Last.Value is BulletList)
										((BulletList)Elements.Last.Value).AddChildren(Item);
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
						else if (ch2 <= ' ' && ch2 > 0)
						{
							if (State.IsFirstCharOnLine)
							{
								this.AppendAnyText(Elements, Text);

								while ((ch2 = State.PeekNextCharSameRow()) <= ' ' && ch2 > 0)
									State.NextCharSameRow();

								UnnumberedItem Item;
								List<string> Rows = new List<string>();
								Rows.Add(State.RestOfRow());

								while (!State.EOF)
								{
									if ((ch2 = State.PeekNextCharSameRow()) == '*' || ch2 == '+' || ch2 == '-')
									{
										Item = new UnnumberedItem(this, ch + " ", new NestedBlock(this, this.ParseBlock(Rows.ToArray())));

										if (Elements.Last != null && Elements.Last.Value is BulletList)
											((BulletList)Elements.Last.Value).AddChildren(Item);
										else
											Elements.AddLast(new BulletList(this, Item));

										State.NextCharSameRow();
										State.SkipWhitespaceSameRow(3);

										Rows.Clear();
										Rows.Add(State.RestOfRow());
									}
									else
									{
										State.SkipWhitespaceSameRow(4);
										Rows.Add(State.RestOfRow());
									}
								}

								if (Rows.Count > 0)
								{
									Item = new UnnumberedItem(this, ch + " ", new NestedBlock(this, this.ParseBlock(Rows.ToArray())));

									if (Elements.Last != null && Elements.Last.Value is BulletList)
										((BulletList)Elements.Last.Value).AddChildren(Item);
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

							if ((ch2 = State.PeekNextCharSameRow()) <= ' ' && ch2 > 0)
							{
								this.AppendAnyText(Elements, Text);

								while ((ch2 = State.PeekNextCharSameRow()) <= ' ' && ch2 > 0)
									State.NextCharSameRow();

								UnnumberedItem Item;
								List<string> Rows = new List<string>();
								Rows.Add(State.RestOfRow());

								while (!State.EOF)
								{
									if ((ch2 = State.PeekNextCharSameRow()) == '#')
									{
										State.NextCharSameRow();
										if ((ch3 = State.PeekNextCharSameRow()) == '.')
										{
											Item = new UnnumberedItem(this, "#. ", new NestedBlock(this, this.ParseBlock(Rows.ToArray())));

											if (Elements.Last != null && Elements.Last.Value is NumberedList)
												((NumberedList)Elements.Last.Value).AddChildren(Item);
											else
												Elements.AddLast(new NumberedList(this, Item));

											State.NextCharSameRow();
											State.SkipWhitespaceSameRow(3);

											Rows.Clear();
											Rows.Add(State.RestOfRow());
										}
										else
										{
											State.SkipWhitespaceSameRow(4);
											Rows.Add("#" + State.RestOfRow());
										}
									}
									else
										Rows.Add(State.RestOfRow());
								}

								if (Rows.Count > 0)
								{
									Item = new UnnumberedItem(this, "#. ", new NestedBlock(this, this.ParseBlock(Rows.ToArray())));

									if (Elements.Last != null && Elements.Last.Value is NumberedList)
										((NumberedList)Elements.Last.Value).AddChildren(Item);
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
								int Index, Index2;

								State.NextCharSameRow();
								if ((ch2 = State.PeekNextCharSameRow()) <= ' ' && ch2 > 0 && int.TryParse(sb.ToString(), out Index))
								{
									this.AppendAnyText(Elements, Text);

									while ((ch2 = State.PeekNextCharSameRow()) <= ' ' && ch2 > 0)
										State.NextCharSameRow();

									NumberedItem Item;
									List<string> Rows = new List<string>();
									Rows.Add(State.RestOfRow());

									while (!State.EOF)
									{
										if ((ch2 = State.PeekNextCharSameRow()) >= '0' && ch2 <= '9')
										{
											sb.Clear();
											while ((ch2 = State.NextCharSameRow()) >= '0' && ch2 <= '9')
												sb.Append(ch2);

											if (ch2 == '.' && int.TryParse(sb.ToString(), out Index2))
											{
												Item = new NumberedItem(this, Index, new NestedBlock(this, this.ParseBlock(Rows.ToArray())));

												if (Elements.Last != null && Elements.Last.Value is NumberedList)
													((NumberedList)Elements.Last.Value).AddChildren(Item);
												else
													Elements.AddLast(new NumberedList(this, Item));

												State.NextCharSameRow();
												State.SkipWhitespaceSameRow(3);

												Rows.Clear();
												Rows.Add(State.RestOfRow());
												Index = Index2;
											}
											else
											{
												State.SkipWhitespaceSameRow(4);
												Rows.Add(sb.ToString() + ch2 + State.RestOfRow());
											}
										}
										else
											Rows.Add(State.RestOfRow());
									}

									if (Rows.Count > 0)
									{
										Item = new NumberedItem(this, Index, new NestedBlock(this, this.ParseBlock(Rows.ToArray())));

										if (Elements.Last != null && Elements.Last.Value is NumberedList)
											((NumberedList)Elements.Last.Value).AddChildren(Item);
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
						if (this.emojiSource == null && ch2 != '=')
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
						if (PrevChar <= ' ' || char.IsPunctuation(PrevChar) || char.IsSeparator(PrevChar))
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
												if (PrevChar <= ' ' || char.IsPunctuation(PrevChar) || char.IsSeparator(PrevChar))
													Elements.AddLast(new HtmlEntity(this, "lsquo"));
												else
													Elements.AddLast(new HtmlEntity(this, "rsquo"));

												Text.Append(":-");
												break;
										}
										break;

									default:
										if (PrevChar <= ' ' || char.IsPunctuation(PrevChar) || char.IsSeparator(PrevChar))
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
										if (PrevChar <= ' ' || char.IsPunctuation(PrevChar) || char.IsSeparator(PrevChar))
											Elements.AddLast(new HtmlEntity(this, "lsquo"));
										else
											Elements.AddLast(new HtmlEntity(this, "rsquo"));

										Text.Append('=');
										break;
								}
								break;

							default:
								if (PrevChar <= ' ' || char.IsPunctuation(PrevChar) || char.IsSeparator(PrevChar))
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
						if (ch2 == 'a' || ch2 == 'o' || (ch2 >= '0' && ch2 <= '3') || ch2 == 'T')
						{
							State.NextCharSameRow();
							this.AppendAnyText(Elements, Text);

							switch (ch2)
							{
								case 'a':
									Elements.AddLast(new HtmlEntity(this, "ordf"));
									break;

								case 'o':
									Elements.AddLast(new HtmlEntity(this, "ordm"));
									break;

								case '0':
									Elements.AddLast(new HtmlEntity(this, "deg"));
									break;

								case '1':
									Elements.AddLast(new HtmlEntityUnicode(this, 185));
									break;

								case '2':
									Elements.AddLast(new HtmlEntityUnicode(this, 178));
									break;

								case '3':
									Elements.AddLast(new HtmlEntityUnicode(this, 179));
									break;

								case 'T':
									ch3 = State.PeekNextCharSameRow();
									if (ch3 == 'M')
									{
										State.NextCharSameRow();
										Elements.AddLast(new HtmlEntity(this, "trade"));
									}
									else
										Text.Append("^T");
									break;
							}
						}
						else
							Text.Append('^');
						break;

					case ':':
						if ((ch2 = State.PeekNextCharSameRow()) <= ' ')
						{
							if (State.IsFirstCharOnLine && ch2 > 0)
							{
								LinkedList<MarkdownElement> TotItem = null;
								LinkedList<MarkdownElement> Item;
								DefinitionList DefinitionList = new DefinitionList(this);
								int i;

								for (i = State.Start; i < State.Current; i++)
								{
									Item = this.ParseBlock(State.Rows, i, i);
									if (Item.First == null)
										continue;

									if (TotItem == null)
									{
										if (Item.First.Next == null)
											TotItem = Item;
										else
											TotItem.AddLast(Item.First.Value);
									}
									else
									{
										if (TotItem == null)
											TotItem = new LinkedList<MarkdownElement>();

										TotItem.AddLast(new NestedBlock(this, Item));
									}
								}

								DefinitionList.AddChildren(new DefinitionTerms(this, TotItem));

								Text.Clear();
								Elements.Clear();
								Elements.AddLast(DefinitionList);

								while ((ch2 = State.PeekNextCharSameRow()) <= ' ' && ch2 > 0)
									State.NextCharSameRow();

								List<string> Rows = new List<string>();
								Rows.Add(State.RestOfRow());

								while (!State.EOF)
								{
									if ((ch2 = State.PeekNextCharSameRow()) == ':')
									{
										DefinitionList.AddChildren(new DefinitionDescriptions(this, new NestedBlock(this, this.ParseBlock(Rows.ToArray()))));

										State.NextCharSameRow();
										State.SkipWhitespaceSameRow(3);

										Rows.Clear();
										Rows.Add(State.RestOfRow());
									}
									else
									{
										State.SkipWhitespaceSameRow(4);
										Rows.Add(State.RestOfRow());
									}
								}

								if (Rows.Count > 0)
									DefinitionList.AddChildren(new DefinitionDescriptions(this, new NestedBlock(this, this.ParseBlock(Rows.ToArray()))));
							}
							else
								Text.Append(ch);
						}
						else if (this.emojiSource != null)
						{
							if (char.IsLetter(ch2))
							{
								this.AppendAnyText(Elements, Text);
								State.NextCharSameRow();

								ch3 = State.PeekNextCharSameRow();
								if (char.IsLetter(ch3) || ch3 == '_')
									Text.Append(ch2);
								else
								{
									switch (ch2)
									{
										case 'D':
											State.NextCharSameRow();
											this.AppendAnyText(Elements, Text);
											Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_smiley));
											break;

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
											State.NextCharSameRow();
											this.AppendAnyText(Elements, Text);
											Elements.AddLast(new EmojiReference(this, EmojiUtilities.Emoji_no_mouth));
											break;

										default:
											ch2 = (char)0;
											break;
									}

									if (ch2 != 0)
										break;
								}

								while (char.IsLetter(ch2 = State.PeekNextCharSameRow()) || ch2 == '_')
								{
									State.NextCharSameRow();
									Text.Append(ch2);
								}

								if (ch2 == ':')
								{
									Title = Text.ToString().ToLower();
									EmojiInfo Emoji;

									if (EmojiUtilities.TryGetEmoji(Title, out Emoji))
									{
										State.NextCharSameRow();
										Elements.AddLast(new EmojiReference(this, Emoji));
										Text.Clear();
									}
									else
										Text.Insert(0, ':');
								}
								else
									Text.Insert(0, ':');
							}
							else
							{
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
							case '=':
							case ':':
							case '|':
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

				PrevChar = ch;
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

		internal static bool IsPrefixedByNumber(string s, out int Numeral)
		{
			int i, c = s.Length;

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
			if (i < c && s[i] > ' ')
				return false;

			return true;
		}

		internal static bool IsPrefixedBy(string s, string Prefix, bool MustHaveWhiteSpaceAfter)
		{
			int i;

			if (!s.StartsWith(Prefix))
				return false;

			if (MustHaveWhiteSpaceAfter)
			{
				if (s.Length == (i = Prefix.Length))
					return false;

				return s[i] <= ' ';
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

		private bool IsUnderline(string s, char ch, bool AllowSpaces)
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
				else if (ch2 == ' ')
				{
					if (!AllowSpaces || LastSpace)
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
			int FirstLineIndent = 0;
			int LineIndent = 0;
			int RowStart = 0;
			int RowEnd = 0;
			int Pos, Len;
			char ch;
			bool InBlock = false;
			bool InRow = false;
			bool NonWhitespaceInRow = false;

			MarkdownText = MarkdownText.Replace("\r\n", "\n").Replace('\r', '\n');
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
							Rows.Add(MarkdownText.Substring(RowStart, RowEnd - RowStart + 1));
							InRow = false;
						}
						else
						{
							Blocks.Add(new Block(Rows.ToArray(), FirstLineIndent / 4));
							Rows.Clear();
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
				else if (ch <= ' ')
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
							else if (ch == ' ')
								LineIndent++;
						}
					}
					else if (ch == '\t')
						FirstLineIndent += 4;
					else if (ch == ' ')
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
					Rows.Add(MarkdownText.Substring(RowStart, RowEnd - RowStart + 1));

				Blocks.Add(new Block(Rows.ToArray(), FirstLineIndent / 4));
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
			KeyValuePair<string, bool>[] Values;
			StringBuilder sb = null;
			string Description = string.Empty;
			string Title = string.Empty;
			string s2;
			bool First;

			Output.AppendLine("<!DOCTYPE html>");
			Output.AppendLine("<html>");
			Output.AppendLine("<head>");

			if (this.metaData.TryGetValue("TITLE", out Values))
			{
				foreach (KeyValuePair<string, bool> P in Values)
				{
					if (sb == null)
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

				Title = HtmlAttributeEncode(sb.ToString());
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
					if (sb == null)
						sb = new StringBuilder();
					else
						sb.Append(" ");

					sb.Append(P.Key);
				}

				if (sb != null)
				{
					Description = HtmlAttributeEncode(sb.ToString());

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
				if (sb == null || string.IsNullOrEmpty(s2 = sb.ToString()))
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
				if (sb == null || string.IsNullOrEmpty(s2 = sb.ToString()))
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
				Output.Append(HtmlAttributeEncode(sb.ToString()));
				Output.AppendLine("\"/>");
			}

			foreach (KeyValuePair<string, KeyValuePair<string, bool>[]> MetaData in this.metaData)
			{
				switch (MetaData.Key)
				{
					case "COPYRIGHT":
					case "PREVIOUS":
					case "PREV":
					case "NEXT":
					case "ALTERNATE":
					case "HELP":
					case "ICON":
					case "CSS":
					case "TITLE":
					case "SUBTITLE":
					case "DATE":
					case "DESCRIPTION":
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

							Output.Append(HtmlAttributeEncode(P.Key));
						}

						Output.AppendLine("\"/>");
						break;

					case "AUTHOR":
						foreach (KeyValuePair<string, bool> P in MetaData.Value)
						{
							Output.Append("<meta name=\"author\" content=\"");
							Output.Append(HtmlAttributeEncode(P.Key));
							Output.AppendLine("\"/>");
						}
						break;

					case "IMAGE":
						foreach (KeyValuePair<string, bool> P in MetaData.Value)
						{
							s2 = HtmlAttributeEncode(P.Key);

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
							Output.Append(HtmlAttributeEncode(P.Key));
							Output.AppendLine("\"/>");
						}
						break;

					default:
						foreach (KeyValuePair<string, bool> P in MetaData.Value)
						{
							Output.Append("<meta name=\"");
							Output.Append(HtmlAttributeEncode(MetaData.Key));
							Output.Append("\" content=\"");
							Output.Append(HtmlAttributeEncode(P.Key));
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
							Output.Append(HtmlAttributeEncode(P.Key));
							Output.AppendLine("\"/>");
						}
						break;

					case "PREVIOUS":
					case "PREV":
						foreach (KeyValuePair<string, bool> P in MetaData.Value)
						{
							Output.Append("<link rel=\"prev\" href=\"");
							Output.Append(HtmlAttributeEncode(P.Key));
							Output.AppendLine("\"/>");
						}
						break;

					case "NEXT":
						foreach (KeyValuePair<string, bool> P in MetaData.Value)
						{
							Output.Append("<link rel=\"next\" href=\"");
							Output.Append(HtmlAttributeEncode(P.Key));
							Output.AppendLine("\"/>");
						}
						break;

					case "ALTERNATE":
						foreach (KeyValuePair<string, bool> P in MetaData.Value)
						{
							Output.Append("<link rel=\"alternate\" href=\"");
							Output.Append(HtmlAttributeEncode(P.Key));
							Output.AppendLine("\"/>");
						}
						break;

					case "HELP":
						foreach (KeyValuePair<string, bool> P in MetaData.Value)
						{
							Output.Append("<link rel=\"help\" href=\"");
							Output.Append(HtmlAttributeEncode(P.Key));
							Output.AppendLine("\"/>");
						}
						break;

					case "ICON":
						foreach (KeyValuePair<string, bool> P in MetaData.Value)
						{
							Output.Append("<link rel=\"shortcut icon\" href=\"");
							Output.Append(HtmlAttributeEncode(P.Key));
							Output.AppendLine("\"/>");
						}
						break;

					case "CSS":
						foreach (KeyValuePair<string, bool> P in MetaData.Value)
						{
							Output.Append("<link rel=\"stylesheet\" href=\"");
							Output.Append(HtmlAttributeEncode(P.Key));
							Output.AppendLine("\"/>");
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

			foreach (MarkdownElement E in this.elements)
				E.GenerateHTML(Output);

			if (this.footnoteOrder != null && this.footnoteOrder.Count > 0)
			{
				Footnote Footnote;
				int Nr;

				Output.AppendLine("<div class=\"footnotes\">");
				Output.AppendLine("<hr />");
				Output.AppendLine("<ol>");

				foreach (string Key in this.footnoteOrder)
				{
					if (this.footnoteNumbers.TryGetValue(Key, out Nr) && this.footnotes.TryGetValue(Key, out Footnote))
					{
						Output.Append("<li id=\"fn-");
						Output.Append(Nr.ToString());
						Output.Append("\">");

						if (!footnoteBacklinksAdded)
						{
							Paragraph P = Footnote.LastChild as Paragraph;
							InlineHTML Backlink = new InlineHTML(this, "<a href=\"#fnref-" + Nr.ToString() + "\" class=\"footnote-backref\">&#8617;</a>");

							if (P != null)
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

			Output.AppendLine("</body>");
			Output.Append("</html>");
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
				Footnote Footnote;
				int Nr;

				Output.AppendLine(new string('-', 80));
				Output.AppendLine();

				foreach (string Key in this.footnoteOrder)
				{
					if (this.footnoteNumbers.TryGetValue(Key, out Nr) && this.footnotes.TryGetValue(Key, out Footnote))
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
				this.GenerateXAML(w, XamlSettings);
			}
		}

		/// <summary>
		/// Generates XAML from the markdown text.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="Settings">XAML settings.</param>
		public void GenerateXAML(XmlWriter Output, XamlSettings Settings)
		{
			Output.WriteStartElement("StackPanel", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
			Output.WriteAttributeString("xmlns", "x", null, "http://schemas.microsoft.com/winfx/2006/xaml");

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

			Output.WriteEndElement();
			Output.Flush();
		}

		internal Multimedia GetReference(string Label)
		{
			Multimedia Result;

			if (this.references.TryGetValue(Label.ToLower(), out Result))
				return Result;
			else
				return null;
		}

		// Different from XML.Encode, in that it does not encode the aposotrophe.
		internal static string HtmlAttributeEncode(string s)
		{
			if (s.IndexOfAny(specialAttributeCharacters) < 0)
				return s;

			return s.
				Replace("&", "&amp;").
				Replace("<", "&lt;").
				Replace(">", "&gt;").
				Replace("\"", "&quot;");
		}

		// Different from XML.Encode, in that it does not encode the aposotrophe or the quote.
		internal static string HtmlValueEncode(string s)
		{
			if (s.IndexOfAny(specialValueCharacters) < 0)
				return s;

			return s.
				Replace("&", "&amp;").
				Replace("<", "&lt;").
				Replace(">", "&gt;");
		}

		private static readonly char[] specialAttributeCharacters = new char[] { '<', '>', '&', '"' };
		private static readonly char[] specialValueCharacters = new char[] { '<', '>', '&' };

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
			KeyValuePair<string, bool>[] Value;

			if (!this.metaData.TryGetValue(Key.ToUpper(), out Value))
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
		/// Address of author.
		/// </summary>
		public string[] Address
		{
			get
			{
				return this.GetMetaData("Address");
			}
		}

		/// <summary>
		/// Author
		/// </summary>
		public string[] Author
		{
			get
			{
				return this.GetMetaData("Author");
			}
		}

		/// <summary>
		/// Affiliation.
		/// </summary>
		public string[] Affiliation
		{
			get
			{
				return this.GetMetaData("Affiliation");
			}
		}

		/// <summary>
		/// URL to Copyright page.
		/// </summary>
		public string[] Copyright
		{
			get
			{
				return this.GetMetaData("Copyright");
			}
		}

		/// <summary>
		/// URL to Previous page.
		/// </summary>
		public string[] Previous
		{
			get
			{
				return this.GetMetaData("Previous");
			}
		}

		/// <summary>
		/// URL to Next page.
		/// </summary>
		public string[] Next
		{
			get
			{
				return this.GetMetaData("Next");
			}
		}

		/// <summary>
		/// CSS.
		/// </summary>
		public string[] CSS
		{
			get
			{
				return this.GetMetaData("CSS");
			}
		}

		/// <summary>
		/// Date.
		/// </summary>
		public string[] Date
		{
			get
			{
				return this.GetMetaData("Date");
			}
		}

		/// <summary>
		/// Description.
		/// </summary>
		public string[] Description
		{
			get
			{
				return this.GetMetaData("Description");
			}
		}

		/// <summary>
		/// Email.
		/// </summary>
		public string[] Email
		{
			get
			{
				return this.GetMetaData("Email");
			}
		}

		/// <summary>
		/// Image.
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
		/// Language.
		/// </summary>
		public string[] Language
		{
			get
			{
				return this.GetMetaData("Language");
			}
		}

		/// <summary>
		/// Phone.
		/// </summary>
		public string[] Phone
		{
			get
			{
				return this.GetMetaData("Phone");
			}
		}

		/// <summary>
		/// Revision.
		/// </summary>
		public string[] Revision
		{
			get
			{
				return this.GetMetaData("Revision");
			}
		}

		/// <summary>
		/// Subtitle.
		/// </summary>
		public string[] Subtitle
		{
			get
			{
				return this.GetMetaData("Subtitle");
			}
		}

		/// <summary>
		/// Title.
		/// </summary>
		public string[] Title
		{
			get
			{
				return this.GetMetaData("Title");
			}
		}

		/// <summary>
		/// Web.
		/// </summary>
		public string[] Web
		{
			get
			{
				return this.GetMetaData("Web");
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
			if (this.footnoteNumbers == null)
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
			if (this.footnotes == null)
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
				if (this.footnoteOrder == null)
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
			int i = s.IndexOfAny(specialCharacters);

			while (i >= 0)
			{
				s = s.Insert(i, new string('\\', 1));
				i = s.IndexOfAny(specialCharacters, i + 2);
			}

			return s;
		}

		private static readonly char[] specialCharacters = new char[]
		{
			'*',
			'_',
			'~',
			'\\',
			'`',
			'{',
			'}',
			'[',
			']',
			'(',
			')',
			'<',
			'>',
			'#',
			'+',
			'-',
			'.',
			'!',
			'\'',
			'"',
			'^',
			'%',
			'=',
			':',
			'|'
		};

		/// <summary>
		/// If syntax highlighting is used in the document.
		/// </summary>
		public bool SyntaxHighlighting
		{
			get { return this.syntaxHighlighting; }
		}

		// TODO: Include local markdown file if used with ![] construct.
		// TODO: Include web page in iframe if used with ![] construct and URI is absolute.
		// TODO: Multi-resolution/-format images/video/audio

	}
}
