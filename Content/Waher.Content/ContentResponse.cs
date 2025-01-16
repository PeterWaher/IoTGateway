using System;
using System.Threading.Tasks;
using Waher.Events;

namespace Waher.Content
{
	/// <summary>
	/// Contains information about a response to a content request.
	/// </summary>
	public class ContentResponse : IDisposableAsync
	{
		private readonly object decoded;
		private readonly bool hasError;

		/// <summary>
		/// Contains information about a success response to a content request.
		/// </summary>
		/// <param name="ContentType">Internet Content-Type of encoded object.</param>
		/// <param name="Decoded">Decoded object.</param>
		/// <param name="Encoded">Encoded object.</param>
		public ContentResponse(string ContentType, object Decoded, byte[] Encoded)
		{
			this.decoded = Decoded;
			this.hasError = false;
			
			this.ContentType = ContentType;
			this.Encoded = Encoded;
			this.Error = null;
		}

		/// <summary>
		/// Contains information about an error response to a content request.
		/// </summary>
		/// <param name="Error">Encoded error.</param>
		public ContentResponse(Exception Error)
		{
			this.decoded = null;
			this.hasError = true;

			this.ContentType = null;
			this.Encoded = null;
			this.Error = Error;
		}

		/// <summary>
		/// Internet Content-Type of encoded object.
		/// </summary>
		public string ContentType { get; }

		/// <summary>
		/// Decoded object.
		/// </summary>
		public object Decoded 
		{
			get
			{
				if (this.hasError)
					throw this.Error;
				else
					return this.decoded;
			}
		}

		/// <summary>
		/// Encoded object.
		/// </summary>
		public byte[] Encoded { get; }

		/// <summary>
		/// If an error occurred.
		/// </summary>
		public bool HasError => this.hasError;

		/// <summary>
		/// Error response.
		/// </summary>
		public Exception Error { get; }

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		[Obsolete("Use DisposeAsync() instead.")]
		public void Dispose()
		{
			this.DisposeAsync().Wait();
		}

		/// <summary>
		/// <see cref="IDisposableAsync.DisposeAsync"/>
		/// </summary>
		public async Task DisposeAsync()
		{ 
			if (this.Decoded is IDisposableAsync DisposableAsync)
				await DisposableAsync.DisposeAsync();
			else if (this.Decoded is IDisposable Disposable)
				Disposable.Dispose();
		}

		/// <summary>
		/// Asserts response is OK.
		/// </summary>
		public void AssertOk()
		{
			if (this.hasError)
				throw this.Error;
		}
	}
}
