using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Runtime.Inventory.Test.Definitions
{
	[Singleton]
	public class Singleton : ISingleton
	{
		private readonly double x;
		private readonly string s;

		public Singleton(string String)
		{
			this.s = String;
			Random Rnd = new Random();
			this.x = Rnd.NextDouble();
		}

		public string String => this.s;
		public double Value => this.x;
	}
}
