using System;

namespace Waher.Content
{
	/// <summary>
	/// Contains information about a response to a content request.
	/// </summary>
	public class ContentResponse : IDisposable
	{
		/// <summary>
		/// Contains information about a success response to a content request.
		/// </summary>
		/// <param name="ContentType">Internet Content-Type of encoded object.</param>
		/// <param name="Decoded">Decoded object.</param>
		/// <param name="Encoded">Encoded object.</param>
		public ContentResponse(string ContentType, object Decoded, byte[] Encoded)
		{
			this.ContentType = ContentType;
			this.Decoded = Decoded;
			this.Encoded = Encoded;
			this.HasError = false;
			this.Error = null;
		}

		/// <summary>
		/// Contains information about an error response to a content request.
		/// </summary>
		/// <param name="Error">Encoded error.</param>
		public ContentResponse(Exception Error)
		{
			this.ContentType = null;
			this.Decoded = null;
			this.Encoded = null;
			this.HasError = true;
			this.Error = Error;
		}

		/// <summary>
		/// Internet Content-Type of encoded object.
		/// </summary>
		public string ContentType { get; }

		/// <summary>
		/// Decoded object.
		/// </summary>
		public object Decoded { get; }

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
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (this.Decoded is IDisposable Disposable)
				Disposable.Dispose();
		}

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
