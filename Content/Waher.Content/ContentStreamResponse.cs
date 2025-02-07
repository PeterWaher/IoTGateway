using System;
using Waher.Runtime.Temporary;

namespace Waher.Content
{
	/// <summary>
	/// Contains information about a stream response to a content request.
	/// </summary>
	public class ContentStreamResponse : IDisposable
	{
		/// <summary>
		/// Contains information about a success stream response to a content request.
		/// </summary>
		/// <param name="ContentType">Internet Content-Type of encoded object.</param>
		/// <param name="Encoded">Encoded object.</param>
		public ContentStreamResponse(string ContentType, TemporaryStream Encoded)
		{
			this.ContentType = ContentType;
			this.Encoded = Encoded;
			this.HasError = false;
			this.Error = null;
		}

		/// <summary>
		/// Contains information about an error stream response to a content request.
		/// </summary>
		/// <param name="Error">Encoded error.</param>
		public ContentStreamResponse(Exception Error)
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
		public TemporaryStream Encoded { get; private set; }

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
			this.Encoded?.Dispose();
			this.Encoded = null;
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
