using System.Threading.Tasks;

namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// Contains information about a pending request to send data over an HTTP/2
	/// stream to a client.
	/// </summary>
	internal class PendingRequest
	{
		/// <summary>
		/// Contains information about a pending request to send data over an HTTP/2
		/// stream to a client.
		/// </summary>
		/// <param name="Requested">Number of bytes requested.</param>
		public PendingRequest(int Requested)
		{
			this.Request = new TaskCompletionSource<int>();
			this.Requested = Requested;
		}

		/// <summary>
		/// Task completion source that can be used to await for the request to
		/// be fully or partially fulfilled.
		/// </summary>
		public TaskCompletionSource<int> Request { get; }

		/// <summary>
		/// Number of bytes requested.
		/// </summary>
		public int Requested { get; }
	}
}
