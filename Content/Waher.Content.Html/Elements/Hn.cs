using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.Elements
{
	/// <summary>
	/// H&lt;n&gt; element
	/// </summary>
    public class Hn : HtmlElement
    {
		private int level;

		/// <summary>
		/// H&lt;n&gt; element
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		/// <param name="Level">Header level.</param>
		public Hn(HtmlDocument Document, HtmlElement Parent, int StartPosition, int Level)
			: base(Document, Parent, StartPosition, "H" + Level.ToString())
		{
			this.level = Level;
		}

		/// <summary>
		/// Header level.
		/// </summary>
		public int Level
		{
			get { return this.level; }
		}
    }
}
