using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Runtime.Inventory.Test.Definitions
{
	[DefaultImplementation(typeof(Example))]
	public interface IExample
	{
		double f(double x);
	}
}
