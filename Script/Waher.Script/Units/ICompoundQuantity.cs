using System;
using System.Collections.Generic;

namespace Waher.Script.Units
{
	/// <summary>
	/// Interface for physical compound quantities 
	/// </summary>
	public interface ICompoundQuantity
	{
		/// <summary>
		/// Name of compound quantity.
		/// </summary>
		string Name
		{
			get;
		}

		/// <summary>
		/// Compound quantities. Must only use base quantity units.
		/// </summary>
		KeyValuePair<string, KeyValuePair<AtomicUnit, int>[]>[] CompoundQuantities
		{
			get;
		}
	}
}
