namespace Waher.WebService.Tesseract
{
	/// <summary>
	/// Page Segmentation Mode
	/// </summary>
	public enum PageSegmentationMode
	{
		/// <summary>
		/// Orientation and script detection (OSD) only
		/// </summary>
		DetectOrientationAndScript = 0,

		/// <summary>
		/// Automatic page segmentation with OSD.
		/// </summary>
		AutomaticPageSegmentationWithOsd = 1,

		/// <summary>
		/// Automatic page segmentation, but no OSD, or OCR.
		/// </summary>
		AutomaticPageSegmentationNoOsdOrOsr = 2,

		/// <summary>
		/// Fully automatic page segmentation, but no OSD. (Default)
		/// </summary>
		FullyAutomaticPageSegmentationNoOsd = 3,

		/// <summary>
		/// Assume a single column of text of variable sizes.
		/// </summary>
		SingleColumnOfText = 4,

		/// <summary>
		/// Assume a single uniform block of vertically aligned text.
		/// </summary>
		SingleUniformBlockOfVerticallyAlignedText = 5,

		/// <summary>
		/// Assume a single uniform block of text.
		/// </summary>
		SingleUniformBlockOfText = 6,

		/// <summary>
		/// Treat the image as a single text line.
		/// </summary>
		SingleLineOfText = 7,

		/// <summary>
		/// Treat the image as a single word.
		/// </summary>
		SingleWord = 8,

		/// <summary>
		/// Treat the image as a single word in a circle.
		/// </summary>
		SingleWordInCircle = 9,

		/// <summary>
		/// Treat the image as a single character.
		/// </summary>
		SingleCharacter = 10,

		/// <summary>
		/// Sparse text. Find as much text as possible in no particular order.
		/// </summary>
		SparseText = 11,

		/// <summary>
		/// Sparse text with OSD.
		/// </summary>
		SparseTextWithOsd = 12,

		/// <summary>
		/// Raw line. Treat the image as a single text line,
		/// bypassing hacks that are Tesseract-specific.
		/// </summary>
		RawLine = 13
	}
}
