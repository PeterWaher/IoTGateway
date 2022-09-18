using System.Collections.Generic;

namespace Waher.Content.Markdown
{
	/// <summary>
	/// Contains some basic statistical information about a Markdown document.
	/// </summary>
	public class MarkdownStatistics
	{
		/// <summary>
		/// Number of elements in Markdown document (total).
		/// </summary>
		public int NrElements { get; set; }

		/// <summary>
		/// Number of abbreviations.
		/// </summary>
		public int NrAbbreviations { get; set; }

		/// <summary>
		/// Number of mail hyperlinks.
		/// </summary>
		public int NrMailHyperLinks { get; set; }

		/// <summary>
		/// Number of URL hyperlinks.
		/// </summary>
		public int NrUrlHyperLinks { get; set; }

		/// <summary>
		/// Number of hyperlinks (total).
		/// </summary>
		public int NrHyperLinks { get; set; }

		/// <summary>
		/// Internal (during computation of statistics) Mail hyperlinks.
		/// </summary>
		internal List<string> IntMailHyperlinks { get; set; }

		/// <summary>
		/// Internal (during computation of statistics) URL hyperlinks.
		/// </summary>
		internal List<string> IntUrlHyperlinks { get; set; }

		/// <summary>
		/// Mail hyperlinks.
		/// </summary>
		public string[] MailHyperlinks { get; set; }

		/// <summary>
		/// URL hyperlinks.
		/// </summary>
		public string[] UrlHyperlinks { get; set; }

		/// <summary>
		/// Number of deletes (span+block).
		/// </summary>
		public int NrDelete { get; set; }

		/// <summary>
		/// Number of details references.
		/// </summary>
		public int NrDetailsReference { get; set; }

		/// <summary>
		/// Number of emoji references.
		/// </summary>
		public int NrEmojiReference { get; set; }

		/// <summary>
		/// Number of emphasizes.
		/// </summary>
		public int NrEmphasize { get; set; }

		/// <summary>
		/// Number of footnote references
		/// </summary>
		public int NrFootnoteReference { get; set; }

		/// <summary>
		/// Number of hash-tags.
		/// </summary>
		public int NrHashTags { get; set; }

		/// <summary>
		/// Number of HTML entities.
		/// </summary>
		public int NrHtmlEntities { get; set; }

		/// <summary>
		/// Number of unicode HTML entities.
		/// </summary>
		public int NrHtmlUnicodeEntities { get; set; }

		/// <summary>
		/// Number of HTML entities (total).
		/// </summary>
		public int NrHtmlEntitiesTotal { get; set; }

		/// <summary>
		/// Number of code inlines.
		/// </summary>
		public int NrInlineCode { get; set; }

		/// <summary>
		/// Number of HTML inlines.
		/// </summary>
		public int NrInlineHtml { get; set; }

		/// <summary>
		/// Number of script inlines.
		/// </summary>
		public int NrInlineScript { get; set; }

		/// <summary>
		/// Number of text inlines.
		/// </summary>
		public int NrInlineText { get; set; }

		/// <summary>
		/// Number of insert (span+block).
		/// </summary>
		public int NrInsert { get; set; }

		/// <summary>
		/// Number of line breaks.
		/// </summary>
		public int NrLineBreak { get; set; }

		/// <summary>
		/// Number of meta references.
		/// </summary>
		public int NrMetaReference { get; set; }

		/// <summary>
		/// Number of multimedia (total).
		/// </summary>
		public int NrMultimedia { get; set; }

		/// <summary>
		/// Number of multi-formatted multimedia.
		/// </summary>
		public int NrMultiformatMultimedia { get; set; }

		/// <summary>
		/// Internal (during computation of statistics) Multimedia links, per file extension.
		/// </summary>
		internal Dictionary<string, List<string>> IntMultimediaPerExtension { get; set; }

		/// <summary>
		/// Internal (during computation of statistics) Multimedia links, per Content-Type
		/// </summary>
		internal Dictionary<string, List<string>> IntMultimediaPerContentType { get; set; }

		/// <summary>
		/// Internal (during computation of statistics) Multimedia links, per Content Category (top part of Content-Type)
		/// </summary>
		internal Dictionary<string, List<string>> IntMultimediaPerContentCategory { get; set; }

		/// <summary>
		/// Multimedia links, per file extension.
		/// </summary>
		public Dictionary<string, string[]> MultimediaPerExtension { get; set; }

		/// <summary>
		/// Multimedia links, per Content-Type
		/// </summary>
		public Dictionary<string, string[]> MultimediaPerContentType { get; set; }

		/// <summary>
		/// Multimedia links, per Content Category (top part of Content-Type)
		/// </summary>
		public Dictionary<string, string[]> MultimediaPerContentCategory { get; set; }

		/// <summary>
		/// Number of multimedia, per file extension.
		/// </summary>
		public Dictionary<string, int> NrMultimediaPerExtension { get; set; }

		/// <summary>
		/// Number of multimedia, per Content-Type
		/// </summary>
		public Dictionary<string, int> NrMultimediaPerContentType { get; set; }

		/// <summary>
		/// Number of multimedia, per Content Category (top part of Content-Type)
		/// </summary>
		public Dictionary<string, int> NrMultimediaPerContentCategory { get; set; }

		/// <summary>
		/// Number of strike throughs.
		/// </summary>
		public int NrStrikeThrough { get; set; }

		/// <summary>
		/// Number of strong.
		/// </summary>
		public int NrStrong { get; set; }

		/// <summary>
		/// Number of subscripts.
		/// </summary>
		public int NrSubscript { get; set; }

		/// <summary>
		/// Number of superscripts.
		/// </summary>
		public int NrSuperScript { get; set; }

		/// <summary>
		/// Number of underlines.
		/// </summary>
		public int NrUnderline { get; set; }

		/// <summary>
		/// Number of block quotes.
		/// </summary>
		public int NrBlockQuotes { get; set; }

		/// <summary>
		/// Number of bullet lists.
		/// </summary>
		public int NrBulletLists { get; set; }

		/// <summary>
		/// Number of code blocks.
		/// </summary>
		public int NrCodeBlocks { get; set; }

		/// <summary>
		/// Number of comments.
		/// </summary>
		public int NrComments { get; set; }

		/// <summary>
		/// Number of definition descriptions.
		/// </summary>
		public int NrDefinitionDescriptions { get; set; }

		/// <summary>
		/// Number of definition lists.
		/// </summary>
		public int NrDefinitionLists { get; set; }

		/// <summary>
		/// Number of definition terms.
		/// </summary>
		public int NrDefinitionTerms { get; set; }

		/// <summary>
		/// Number of footnotes.
		/// </summary>
		public int NrFootnotes { get; set; }

		/// <summary>
		/// Number of headers.
		/// </summary>
		public int NrHeaders { get; set; }

		/// <summary>
		/// Number of horizontal rules.
		/// </summary>
		public int NrHorizontalRules { get; set; }

		/// <summary>
		/// Number of HTML blocks.
		/// </summary>
		public int NrHtmlBlocks { get; set; }

		/// <summary>
		/// Number of invisible breaks.
		/// </summary>
		public int NrInvisibleBreaks { get; set; }

		/// <summary>
		/// Number of numbered items.
		/// </summary>
		public int NrNumberedItems { get; set; }

		/// <summary>
		/// Number of numbered lists.
		/// </summary>
		public int NrNumberedLists { get; set; }

		/// <summary>
		/// Number of paragraphs.
		/// </summary>
		public int NrParagraph { get; set; }

		/// <summary>
		/// Number of sections.
		/// </summary>
		public int NrSections { get; set; }

		/// <summary>
		/// Number of section separators.
		/// </summary>
		public int NrSectionSeparators { get; set; }

		/// <summary>
		/// Number of tables.
		/// </summary>
		public int NrTables { get; set; }

		/// <summary>
		/// Number of task items.
		/// </summary>
		public int NrTaskItems { get; set; }

		/// <summary>
		/// Number of task lists.
		/// </summary>
		public int NrTaskLists { get; set; }

		/// <summary>
		/// Number of unnumbered items.
		/// </summary>
		public int NrUnnumberedItems { get; set; }

		/// <summary>
		/// Number of list items (total).
		/// </summary>
		public int NrListItems { get; set; }

		/// <summary>
		/// Number of lists (total).
		/// </summary>
		public int NrLists { get; set; }

		/// <summary>
		/// Number of nested blocks.
		/// </summary>
		public int NrNestedBlocks { get; set; }
	}
}
