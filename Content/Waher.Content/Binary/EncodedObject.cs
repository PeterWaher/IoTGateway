namespace Waher.Content.Binary
{
	/// <summary>
	/// Represents an encoded object.
	/// </summary>
	public class EncodedObject
	{
		private readonly byte[] data;
		private readonly string contentType;

		/// <summary>
		/// Represents an encoded object.
		/// </summary>
		/// <param name="Data">Encoded object.</param>
		/// <param name="ContentType">Internet Content-Type.</param>
		public EncodedObject(byte[] Data, string ContentType)
		{
			this.data = Data;
			this.contentType = ContentType;
		}

		/// <summary>
		/// Encoded object.
		/// </summary>
		public byte[] Data => this.data;

		/// <summary>
		/// Internet Content-Type.
		/// </summary>
		public string ContentType => this.contentType;
	}
}
