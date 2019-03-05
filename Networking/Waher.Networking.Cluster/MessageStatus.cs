using System;
using System.Collections.Generic;
using System.Net;

namespace Waher.Networking.Cluster
{
	/// <summary>
	/// Keeps track of an outgoing message.
	/// </summary>
	internal class MessageStatus
	{
		public Guid Id;
		public Dictionary<IPEndPoint, bool?> Acknowledged = new Dictionary<IPEndPoint, bool?>();
		public IClusterMessage Message;
		public byte[] MessageBinary;
		public DateTime Timeout;
		public ClusterMessageAckEventHandler Callback;
		public object State;

		/// <summary>
		/// If all responses have been returned.
		/// </summary>
		/// <param name="Statuses">Valid statuses.</param>
		/// <returns>If all responses have been returned.</returns>
		public bool IsComplete(EndpointStatus[] Statuses)
		{
			lock (this.Acknowledged)
			{
				foreach (EndpointStatus Status in Statuses)
				{
					if (!Acknowledged.ContainsKey(Status.Endpoint))
						return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Compiles available responses.
		/// </summary>
		/// <returns>Set of responses</returns>
		public EndpointAcknowledgement[] GetResponses(EndpointStatus[] Statuses)
		{
			EndpointAcknowledgement[] Result;
			int i, c;

			lock (this.Acknowledged)
			{
				foreach (EndpointStatus Status in Statuses)
				{
					if (!Acknowledged.ContainsKey(Status.Endpoint))
						Acknowledged[Status.Endpoint] = null;
				}

				Result = new EndpointAcknowledgement[c = this.Acknowledged.Count];

				i = 0;
				foreach (KeyValuePair<IPEndPoint, bool?> P in this.Acknowledged)
					Result[i++] = new EndpointAcknowledgement(P.Key, P.Value);
			}

			return Result;
		}
	}
}
