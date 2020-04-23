using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Delegate for file not found event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void FileNotFoundEventHandler(object Sender, FileNotFoundEventArgs e);

	/// <summary>
	/// Event arguments for file not found events.
	/// </summary>
	public class FileNotFoundEventArgs : EventArgs
	{
		private NotFoundException exception;
		private readonly HttpRequest request;
		private readonly HttpResponse response;
		private readonly string fullPath;

		/// <summary>
		/// Event arguments for file not found events.
		/// </summary>
		/// <param name="Exception">Exception that will be returned to client.</param>
		/// <param name="FullPath">Full path to requested file that was not found.</param>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		public FileNotFoundEventArgs(NotFoundException Exception, string FullPath, HttpRequest Request, HttpResponse Response)
		{
			this.exception = Exception;
			this.fullPath = FullPath;
			this.request = Request;
			this.response = Response;
		}

		/// <summary>
		/// Exception that will be returned to client. Change, if a custom exception is to be returned. Set to null, if the
		/// handler manages sending the response asynchronously, and does custom logging.
		/// </summary>
		public NotFoundException Exception
		{
			get => this.exception;
			set => this.exception = value;
		}

		/// <summary>
		/// Current request object.
		/// </summary>
		public HttpRequest Request => this.request;

		/// <summary>
		/// Current response object.
		/// </summary>
		public HttpResponse Response => this.response;

		/// <summary>
		/// Full path to requested file that was not found.
		/// </summary>
		public string FullPath => this.fullPath;
	}
}
