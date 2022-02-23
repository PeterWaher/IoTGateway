namespace Waher.Content.Html.Javascript
{
	/// <summary>
	/// Encapsulates a Javascript Document
	/// </summary>
	public class JavascriptDocument
	{
		private readonly string javascript;

		/// <summary>
		/// Encapsulates a Javascript Document
		/// </summary>
		/// <param name="Javascript">Javascript Text</param>
		public JavascriptDocument(string Javascript)
		{
			this.javascript = Javascript;
		}

		/// <summary>
		/// Javascript Text
		/// </summary>
		public string Javascript => this.javascript;
	}
}
