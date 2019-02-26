using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.Cluster;

namespace Waher.Networking.Cluster.Test.TestObjects
{
	public class FloatingPoints : IClusterMessage
	{
		public FloatingPoints()
		{
		}

		public float S
		{
			get;
			set;
		}

		public double D
		{
			get;
			set;
		}

		public decimal Dec
		{
			get;
			set;
		}
	}
}
