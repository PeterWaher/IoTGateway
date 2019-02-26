using System;
using System.Collections.Generic;
using System.Threading;
using Waher.Networking.Cluster;

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

		public void MessageReceived()
		{
		}
	}
}
