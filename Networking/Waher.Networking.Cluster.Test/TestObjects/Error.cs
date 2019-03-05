using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.Cluster.Test.TestObjects
{
	public class Error : IClusterCommand
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
			throw new ArgumentException((this.A + this.B).ToString());
		}
	}
}
