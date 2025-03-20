namespace Waher.Content.Markdown.Latex
{
	/// <summary>
	/// Document class of LaTeX output.
	/// </summary>
	public enum LaTeXDocumentClass
	{
		/// <summary>
		/// LaTeX Article
		/// </summary>
		Article,

		/// <summary>
		/// LaTeX Report
		/// </summary>
		Report,

		/// <summary>
		/// LaTeX Book
		/// </summary>
		Book,

		/// <summary>
		/// Standalone output
		/// </summary>
		Standalone
	}

	/// <summary>
	/// LaTeX output paper format
	/// </summary>
	public enum LaTeXPaper
	{
		/// <summary>
		/// Letter paper output
		/// </summary>
		Letter,

		/// <summary>
		/// A4 paper output
		/// </summary>
		A4
	}

	/// <summary>
	/// Contains settings that the LaTeX export uses to customize LaTeX output.
	/// </summary>
	public class LaTeXSettings
	{
		/// <summary>
		/// Contains settings that the LaTeX export uses to customize LaTeX output.
		/// </summary>
		public LaTeXSettings()
		{
		}

		/// <summary>
		/// Document class of LaTeX output.
		/// </summary>
		public LaTeXDocumentClass DocumentClass { get; set; } = LaTeXDocumentClass.Article;

		/// <summary>
		/// Document output paper format.
		/// </summary>
		public LaTeXPaper PaperFormat { get; set; } = LaTeXPaper.A4;

		/// <summary>
		/// Default font size, in points (pt)
		/// </summary>
		public int DefaultFontSize { get; set; } = 10;

		/// <summary>
		/// Document language.
		/// </summary>
		public string Language { get; set; } = "english";

	}
}
