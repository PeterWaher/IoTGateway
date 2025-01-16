using System;

namespace Waher.Content
{
	/// <summary>
	/// Contains information about a binary response to a content request.
	/// </summary>
	public class ContentBinaryResponse
	{
		/// <summary>
		/// Contains information about a success binary response to a content request.
		/// </summary>
		/// <param name="ContentType">Internet Content-Type of encoded object.</param>
		/// <param name="Encoded">Encoded object.</param>
		public ContentBinaryResponse(string ContentType, byte[] Encoded)
		{
			this.ContentType = ContentType;
			this.Encoded = Encoded;
			this.HasError = false;
			this.Error = null;
		}

		/// <summary>
		/// Contains information about an error binary response to a content request.
		/// </summary>
		/// <param name="Error">Encoded error.</param>
		public ContentBinaryResponse(Exception Error)
		{
			this.ContentType = null;
			this.Encoded = null;
			this.HasError = true;
			this.Error = Error;
		}

		/// <summary>
		/// Internet Content-Type of encoded object.
		/// </summary>
		public string ContentType { get; }

		/// <summary>
		/// Encoded object.
		/// </summary>
		public byte[] Encoded { get; }

		/// <summary>
		/// If an error occurred.
		/// </summary>
		public bool HasError { get; }

		/// <summary>
		/// Error response.
		/// </summary>
		public Exception Error { get; }

		/// <summary>
		/// Asserts response is OK.
		/// </summary>
		public void AssertOk()
		{
			if (this.HasError)
				throw this.Error;
		}
	}
}
