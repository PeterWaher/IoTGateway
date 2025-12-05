using SkiaSharp;
using System;

namespace Waher.Layout.Layout2D.Events
{
	/// <summary>
	/// Event raised when the layout model has been updated internally.
	/// </summary>
	public class InteralImageEventArgs : EventArgs
	{
		private readonly Layout2DDocument doc;
		private readonly string contentId;
		private SKImage image = null;

		/// <summary>
		/// Event raised when the layout model has been updated internally.
		/// </summary>
		/// <param name="Document">Document that has been updated.</param>
		/// <param name="ContentId">Content ID to retrieve.</param>
		public InteralImageEventArgs(Layout2DDocument Document, string ContentId)
			: base()
		{
			this.doc = Document;
			this.contentId = ContentId;
		}

		/// <summary>
		/// Document that has been updated.
		/// </summary>
		public Layout2DDocument Document => this.doc;

		/// <summary>
		/// Content ID to retrieve.
		/// </summary>
		public string ContentId => this.contentId;

		/// <summary>
		/// Image to return.
		/// </summary>
		public SKImage Image => this.image;

		/// <summary>
		/// If an image has been set.
		/// </summary>
		public bool HasImage => !(this.image is null);

		/// <summary>
		/// Sets the image to return for the event.
		/// </summary>
		/// <param name="Image">Image to return.</param>
		public void SetImage(SKImage Image)
		{
			this.image?.Dispose();
			this.image = Image;
		}
	}
}
