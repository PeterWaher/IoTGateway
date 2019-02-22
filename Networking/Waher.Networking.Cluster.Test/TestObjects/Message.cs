using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.Cluster;

namespace Waher.Networking.Cluster.Test.TestObjects
{
	public class Message : IClusterMessage
	{
		private string s1;
		private string s2;
		private string s3;

		public Message()
		{
		}

		public string S1
		{
			get => this.s1;
			set => this.s1 = value;
		}

		public string S2
		{
			get => this.s2;
			set => this.s2 = value;
		}

		public string S3
		{
			get => this.s3;
			set => this.s3 = value;
		}
	}
}
