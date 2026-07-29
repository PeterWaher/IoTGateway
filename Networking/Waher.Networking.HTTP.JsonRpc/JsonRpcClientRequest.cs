using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Waher.Content;

namespace Waher.Networking.HTTP.JsonRpc
{
	/// <summary>
	/// Represents a request made to a JSON-RPC client.
	/// </summary>
	public class JsonRpcClientRequest<T> : IJsonRpcClientRequest, IDisposable
	{
		private readonly JsonRpcWebService webService;
		private readonly IJsonRpcSession session;
		private readonly TaskCompletionSource<T> result;
		private readonly Func<object?, T> parseResult;
		private readonly string method;
		private readonly object? parameters;
		private bool processed = false;

		internal JsonRpcClientRequest(object? Id, string Method, object? Parameters,
			IJsonRpcSession Session, Func<object?, T> ParseResult,
			JsonRpcWebService WebService)
		{
			this.Id = Id;
			this.method = Method;
			this.parameters = Parameters;
			this.session = Session;
			this.parseResult = ParseResult;
			this.webService = WebService;
			this.result = new TaskCompletionSource<T>();
		}

		/// <summary>
		/// ID of request.
		/// </summary>
		public object? Id { get; }

		/// <summary>
		/// Called when a result is received for the request.
		/// </summary>
		/// <param name="Result">Result of the request.</param>
		public void ReportResult(object? Result)
		{
			try
			{
				T ParsedResult = this.parseResult(Result);
				this.Return(ParsedResult);
			}
			catch (Exception ex)
			{
				this.Error(ex);
			}
		}

		/// <summary>
		/// Returns a result for the request.
		/// </summary>
		/// <param name="Result">The result to return.</param>
		public void Return(T Result)
		{
			if (!this.processed)
			{
				this.processed = true;
				this.webService.RemoveClientRequest(this.Id?.ToString() ?? string.Empty);
				this.result.TrySetResult(Result);
			}
		}

		/// <summary>
		/// Returns an error for the request.
		/// </summary>
		/// <param name="Error">The error to return.</param>
		public void Error(Exception Error)
		{
			if (!this.processed)
			{
				this.processed = true;
				this.webService.RemoveClientRequest(this.Id?.ToString() ?? string.Empty);
				this.result.TrySetException(Error);
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (!this.processed)
			{
				this.processed = true;
				this.webService.RemoveClientRequest(this.Id?.ToString() ?? string.Empty);
				this.result.TrySetCanceled();
			}
		}

		/// <summary>
		/// Waits for a response to the request.
		/// </summary>
		/// <param name="Timeout">The timeout in milliseconds.</param>
		/// <returns>The result of the request.</returns>
		public async Task<T> WaitForResultAsync(int Timeout)
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

			_ = Task.Delay(Timeout).ContinueWith(_ => this.Error(new TimeoutException()));

			return await this.result.Task;
		}
	}
}
