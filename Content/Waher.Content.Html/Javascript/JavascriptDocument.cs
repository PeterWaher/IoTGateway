namespace Waher.Content.Html.JavaScript
{
	/// <summary>
	/// Encapsulates a JavaScript Document
	/// </summary>
	public class JavaScriptDocument
	{
		private readonly string javaScript;

		/// <summary>
		/// Encapsulates a JavaScript Document
		/// </summary>
		/// <param name="JavaScript">JavaScript Text</param>
		public JavaScriptDocument(string JavaScript)
		{
			this.javaScript = JavaScript;
		}

		/// <summary>
		/// JavaScript Text
		/// </summary>
		public string JavaScript => this.javaScript;
	}
}
