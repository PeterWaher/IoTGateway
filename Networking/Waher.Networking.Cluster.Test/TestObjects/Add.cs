using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.Cluster.Test.TestObjects
{
	public class Add : IClusterCommand
	{
		public int A
		{
			get;
			set;
		}

		public int B
		{
			get;
			set;
		}

		public Task<object> Execute(ClusterEndpoint LocalEndpoint, IPEndPoint RemoteEndpoint)
		{
			return Task.FromResult<object>(this.A + this.B);
		}
	}
}
