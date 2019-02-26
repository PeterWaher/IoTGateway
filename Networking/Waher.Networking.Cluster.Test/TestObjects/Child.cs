using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.Cluster.Test.TestObjects
{
	public class Child : Parent
	{
		public int I
		{
			get;
			set;
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj) && obj is Child C && this.I.Equals(C.I);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode() ^ this.I.GetHashCode();
		}
	}
}
