using System;

namespace Waher.Runtime.Inventory.Test.Definitions
{
	public class TwoArguments : OneArgument
	{
		private readonly string s;

		public TwoArguments(int N, string s)
			: base(N)
		{
			this.s = s;
		}

		public string S => this.s;
	}
}
