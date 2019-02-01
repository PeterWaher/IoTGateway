using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html
{
	/// <summary>
	/// HTML Entity, as a unicode number string.
	/// </summary>
	public class HtmlEntityUnicode : HtmlEntity
	{
		private readonly int code;

		/// <summary>
		/// HTML Entity, as a unicode number string.
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		/// <param name="EndPosition">End position.</param>
		/// <param name="EntityName">Entity name.</param>
		/// <param name="Code">Unicode number</param>
		public HtmlEntityUnicode(HtmlDocument Document, HtmlNode Parent, int StartPosition, int EndPosition, 
			string EntityName, int Code)
			: base(Document, Parent, StartPosition, EndPosition, EntityName)
		{
			this.code = Code;
		}

		/// <summary>
		/// Unicode number
		/// </summary>
		public int Code
		{
			get { return this.code; }
		}

		/// <summary>
		/// <see cref="object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return new string((char)this.code, 1);
		}
	}
}
