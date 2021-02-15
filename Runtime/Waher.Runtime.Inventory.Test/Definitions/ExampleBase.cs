using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Runtime.Inventory.Test.Definitions
{
	[DefaultImplementation(typeof(Example))]
	public abstract class ExampleBase : IExample
	{
		public abstract double Eval(double x);
	}
}
