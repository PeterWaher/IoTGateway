using System;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP.JsonRpc
{
	/// <summary>
	/// Interface for JSON-RPC client request objects.
	/// </summary>
	public interface IJsonRpcClientRequest : IDisposable
	{
		/// <summary>
		/// Message to user.
		/// </summary>
		string Message { get; }

		/// <summary>
		/// ID of request.
		/// </summary>
		object? Id { get; }

		/// <summary>
		/// Property that can be used to store user-defined data associated with 
		/// the request.
		/// </summary>
		object? Tag { get; set; }

		/// <summary>
		/// Called when a result is received for the request.
		/// </summary>
		/// <param name="Result">Result of the request.</param>
		Task ReportResult(object? Result);

		/// <summary>
		/// Called when an error is received for the request.
		/// </summary>
		/// <param name="ErrorCode">Error Code</param>
		/// <param name="ErrorMessage">Error Message</param>
		Task ReportError(int? ErrorCode, string ErrorMessage);

		/// <summary>
		/// Called when the input dialog has been cancelled.
		/// </summary>
		Task Cancel();
	}
}
