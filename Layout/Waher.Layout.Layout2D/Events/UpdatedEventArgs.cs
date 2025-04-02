using System;
using Waher.Layout.Layout2D.Model;

namespace Waher.Layout.Layout2D.Events
{
	/// <summary>
	/// Event raised when the layout model has been updated internally.
	/// </summary>
	public class UpdatedEventArgs : EventArgs
	{
		private readonly Layout2DDocument doc;
		private readonly ILayoutElement element;

		/// <summary>
		/// Event raised when the layout model has been updated internally.
		/// </summary>
		/// <param name="Document">Document that has been updated.</param>
		/// <param name="Element">Layout element that has been updated.</param>
		public UpdatedEventArgs(Layout2DDocument Document, ILayoutElement Element)
			: base()
		{
			this.doc = Document;
			this.element = Element;
		}

		/// <summary>
		/// Document that has been updated.
		/// </summary>
		public Layout2DDocument Document => this.doc;

		/// <summary>
		/// Layout element that has been updated.
		/// </summary>
		public ILayoutElement Element => this.element;
	}
}
