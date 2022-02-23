namespace Waher.Content.Html.Css
{
	/// <summary>
	/// Encapsulates a CSS Document
	/// </summary>
	public class CssDocument
	{
		private readonly string css;

		/// <summary>
		/// Encapsulates a CSS Document
		/// </summary>
		/// <param name="Css">CSS Text</param>
		public CssDocument(string Css)
		{
			this.css = Css;
		}

		/// <summary>
		/// CSS Text
		/// </summary>
		public string Css => this.css;
	}
}
