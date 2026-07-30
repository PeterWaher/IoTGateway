using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;

namespace Waher.Networking.HTTP.JsonRpc
{
	/// <summary>
	/// Represents a request made to a JSON-RPC client.
	/// </summary>
	public class JsonRpcClientRequest<T> : IJsonRpcClientRequest, IDisposableAsync
	{
		private readonly JsonRpcWebService webService;
		private readonly IJsonRpcSession session;
		private readonly TaskCompletionSource<T> result;
		private readonly Func<object?, Task<T>> parseResult;
		private readonly HttpRequest httpRequest;
		private readonly string method;
		private readonly object? parameters;
		private bool processed = false;

		internal JsonRpcClientRequest(string Message, object? Id, string Method,
			object? Parameters, IJsonRpcSession Session, Func<object?, Task<T>> ParseResult,
			JsonRpcWebService WebService, HttpRequest HttpRequest)
		{
			this.Message = Message;
			this.Id = Id;
			this.method = Method;
			this.parameters = Parameters;
			this.session = Session;
			this.parseResult = ParseResult;
			this.webService = WebService;
			this.httpRequest = HttpRequest;
			this.result = new TaskCompletionSource<T>();
		}

		/// <summary>
		/// Message to user.
		/// </summary>
		public string Message { get; }

		/// <summary>
		/// ID of request.
		/// </summary>
		public object? Id { get; }

		/// <summary>
		/// Property that can be used to store user-defined data associated with 
		/// the request.
		/// </summary>
		public object? Tag { get; set; }

		/// <summary>
		/// Called when a result is received for the request.
		/// </summary>
		/// <param name="Result">Result of the request.</param>
		public async Task ReportResult(object? Result)
		{
			try
			{
				if (!(Result is T ParsedResult))
					ParsedResult = await this.parseResult(Result);

				await this.Return(ParsedResult);
			}
			catch (Exception ex)
			{
				await this.Error(ex);
			}
		}

		/// <summary>
		/// Called when the input dialog has been cancelled.
		/// </summary>
		public async Task Cancel()
		{
			if (!this.processed)
			{
				this.processed = true;
				this.webService.RemoveClientRequest(this.Id?.ToString() ?? string.Empty);
				this.result.TrySetCanceled();

				await this.Cancelled.Raise(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Event raised when the request has been cancelled.
		/// </summary>
		public event EventHandlerAsync? Cancelled;

		/// <summary>
		/// Called when an error is received for the request.
		/// </summary>
		/// <param name="ErrorCode">Error Code</param>
		/// <param name="ErrorMessage">Error Message</param>
		public Task ReportError(int? ErrorCode, string ErrorMessage)
		{
			if (!this.processed)
			{
				this.processed = true;
				this.webService.RemoveClientRequest(this.Id?.ToString() ?? string.Empty);
				this.result.TrySetException(new Exception(ErrorMessage));

				// Note: Do not raise event. Not an error in processing; the request was not sent or received properly.
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Returns a result for the request.
		/// </summary>
		/// <param name="Result">The result to return.</param>
		public async Task Return(T Result)
		{
			if (!this.processed)
			{
				this.processed = true;
				this.webService.RemoveClientRequest(this.Id?.ToString() ?? string.Empty);
				this.result.TrySetResult(Result);

				await this.ResultReturned.Raise(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Event raised when a result has been returned for the request.
		/// </summary>
		public event EventHandlerAsync? ResultReturned;

		/// <summary>
		/// Returns an error for the request.
		/// </summary>
		/// <param name="Error">The error to return.</param>
		public async Task Error(Exception Error)
		{
			if (!this.processed)
			{
				this.processed = true;
				this.webService.RemoveClientRequest(this.Id?.ToString() ?? string.Empty);
				this.result.TrySetException(Error);

				await this.ErrorReturned.Raise(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Event raised when an error has been returned for the request.
		/// </summary>
		public event EventHandlerAsync? ErrorReturned;

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		[Obsolete("Use DisposeAsync instead.")]
		public void Dispose()
		{
			this.DisposeAsync().Wait();
		}

		/// <summary>
		/// Disposes of the object, asynchronously.
		/// </summary>
		public Task DisposeAsync()
		{
			return this.Cancel();
		}

		/// <summary>
		/// Sends the request to the MCP client.
		/// </summary>
		public async Task SendRequest()
		{
			Dictionary<string, object?> Request = new Dictionary<string, object?>()
			{
				{ "jsonrpc", "2.0" },
				{ "id", this.Id },
				{ "method", this.method },
				{ "params", this.parameters }
			};
			string Data = JSON.Encode(Request, false);

			int NrSent = await this.webService.SendEvent(
				Session =>
				{
					if (this.session.SessionId != Session?.SessionId)
						return false;

					this.session.TransmitText(Data);

					return true;
				},
				new KeyValuePair<string, object>("event", "message"),
				new KeyValuePair<string, object>("data", Data));

			if (NrSent == 0)
			{
				this.webService.RemoveClientRequest(this.Id?.ToString() ?? string.Empty);
				throw new IOException("Session no longer active.");
			}
		}

		/// <summary>
		/// Waits for a response to the request.
		/// </summary>
		/// <param name="Timeout">The timeout in milliseconds.</param>
		/// <returns>The result of the request.</returns>
		public async Task<T> WaitForResultAsync(int Timeout)
		{
			TaskCompletionSource<bool> Completed = new TaskCompletionSource<bool>();

			async void KeepAlive(TaskCompletionSource<bool> Completed, int Timeout)
			{
				DateTime Start = DateTime.UtcNow;
				DateTime Until = Start.AddMilliseconds(Timeout);
				DateTime TP;

				try
				{
					while (!Completed.Task.IsCompleted && (TP = DateTime.UtcNow) < Until)
					{
						this.httpRequest.Ping();
						await Task.Delay(Math.Min(1000, (int)Until.Subtract(TP).TotalMilliseconds));
					}

					if (!Completed.Task.IsCompleted)
						await this.Error(new TimeoutException());
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}

			KeepAlive(Completed, Timeout);

			try
			{
				return await this.result.Task;
			}
			finally
			{
				Completed.TrySetResult(true);
			}
		}
	}
}
