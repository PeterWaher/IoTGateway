namespace Waher.IoTGateway.Encoding
{
	/// <summary>
	/// A Binary PDF Document
	/// </summary>
	public class BinaryPdfDocument
	{
		/// <summary>
		/// A Binary PDF Document
		/// </summary>
		/// <param name="Document">Document</param>
		public BinaryPdfDocument(byte[] Document)
		{
			this.Document = Document;
		}

		/// <summary>
		/// Binary representation of PDF document.
		/// </summary>
		public byte[] Document { get; }
	}
}
