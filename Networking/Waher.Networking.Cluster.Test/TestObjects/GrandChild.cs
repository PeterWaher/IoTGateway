using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.Cluster.Test.TestObjects
{
	public class GrandChild : Child 
	{
		public bool B
		{
			get;
			set;
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj) && obj is GrandChild GC && this.B.Equals(GC.B);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode() ^ this.B.GetHashCode();
		}
	}
}
