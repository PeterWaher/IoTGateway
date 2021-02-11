using System;

namespace Waher.Runtime.Inventory.Test.Definitions
{
	public class OneArgument
	{
		private readonly int n;

		public OneArgument(int N)
		{
			this.n = N;
		}

		public int N => this.n;
	}
}
