using System;
using System.Net;
using System.Threading.Tasks;

namespace Waher.Networking.Cluster.Test.TestObjects
{
	public class Message : IClusterMessage
	{
		public Message()
		{
		}

		public string Text
		{
			get;
			set;
		}

		public DateTime Timestamp
		{
			get;
			set;
		}

		public Task<bool> MessageReceived(ClusterEndpoint Endpoint, IPEndPoint RemoteEndpoint)
		{
			return Task.FromResult<bool>(true);
		}
	}
}
