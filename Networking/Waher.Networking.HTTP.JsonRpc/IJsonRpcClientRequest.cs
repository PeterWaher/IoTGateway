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
		/// ID of request.
		/// </summary>
		object? Id { get; }

		/// <summary>
		/// Called when a result is received for the request.
		/// </summary>
		/// <param name="Result">Result of the request.</param>
		Task ReportResult(object? Result);
	}
}
