using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Runtime.Inventory.Test.Definitions
{
	[DefaultImplementation(typeof(Singleton))]
	public interface ISingleton
	{
		string String { get; }
		double Value { get; }
	}
}
