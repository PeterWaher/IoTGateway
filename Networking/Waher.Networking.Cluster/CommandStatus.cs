using System;
using System.Collections.Generic;
using System.Net;

namespace Waher.Networking.Cluster
{
	/// <summary>
	/// Keeps track of an outgoing command.
	/// </summary>
	internal class CommandStatus<ResponseType> : CommandStatusBase
	{
		public Dictionary<IPEndPoint, KeyValuePair<ResponseType, Exception>> Responses = new Dictionary<IPEndPoint, KeyValuePair<ResponseType, Exception>>();
		public ClusterResponseEventHandler<ResponseType> Callback;

		/// <summary>
		/// If all responses have been returned.
		/// </summary>
		/// <param name="Statuses">Valid statuses.</param>
		/// <returns>If all responses have been returned.</returns>
		public override bool IsComplete(EndpointStatus[] Statuses)
		{
			lock (this.Responses)
			{
				foreach (EndpointStatus Status in Statuses)
				{
					if (!Responses.ContainsKey(Status.Endpoint))
						return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Compiles available responses.
		/// </summary>
		/// <returns>Set of responses</returns>
		public EndpointResponse<ResponseType>[] GetResponses(EndpointStatus[] Statuses)
		{
			EndpointResponse<ResponseType>[] Result;
			int i, c;

			lock (this.Responses)
			{
				foreach (EndpointStatus Status in Statuses)
				{
					if (!Responses.ContainsKey(Status.Endpoint))
						Responses[Status.Endpoint] = new KeyValuePair<ResponseType, Exception>(default(ResponseType), new TimeoutException("No response returned."));
				}

				Result = new EndpointResponse<ResponseType>[c = this.Responses.Count];

				i = 0;
				foreach (KeyValuePair<IPEndPoint, KeyValuePair<ResponseType, Exception>> P in this.Responses)
					Result[i++] = new EndpointResponse<ResponseType>(P.Key, P.Value.Key, P.Value.Value);
			}

			return Result;
		}

		/// <summary>
		/// Adds a response from an endpoint.
		/// </summary>
		/// <param name="From">Endpoint providing response</param>
		/// <param name="Response">Response object</param>
		public override void AddResponse(IPEndPoint From, object Response)
		{
			lock (this.Responses)
			{
				if (Response is ResponseType Response2)
					this.Responses[From] = new KeyValuePair<ResponseType, Exception>(Response2, null);
				else
					this.Responses[From] = new KeyValuePair<ResponseType, Exception>(default(ResponseType), new Exception("Unexpected response returned."));
			}
		}

		/// <summary>
		/// Adds an error from an endpoint.
		/// </summary>
		/// <param name="From">Endpoint providing response</param>
		/// <param name="Error">Exception object describing the error.</param>
		public override void AddError(IPEndPoint From, Exception Error)
		{
			lock (this.Responses)
			{
				this.Responses[From] = new KeyValuePair<ResponseType, Exception>(default(ResponseType), Error);
			}
		}

		/// <summary>
		/// Raises the response event.
		/// </summary>
		/// <param name="CurrentStatus">Current status of endpoints in cluster.</param>
		public override void RaiseResponseEvent(EndpointStatus[] CurrentStatus)
		{
			this.Callback?.Invoke(this, new ClusterResponseEventArgs<ResponseType>(
				this.Command, this.GetResponses(CurrentStatus), this.State));
		}

	}
}
