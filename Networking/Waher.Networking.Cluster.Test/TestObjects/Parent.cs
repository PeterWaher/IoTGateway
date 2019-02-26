using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.Cluster.Test.TestObjects
{
	public class Parent
	{
		public string S
		{
			get;
			set;
		}

		public override bool Equals(object obj)
		{
			return obj is Parent P && this.S.Equals(P.S);
		}

		public override int GetHashCode()
		{
			return this.S.GetHashCode();
		}
	}
}
