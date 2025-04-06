using System;
using System.Collections.Generic;

namespace Waher.Script.Units
{
	/// <summary>
	/// Interface for physical compound quantities 
	/// </summary>
	public interface ICompoundQuantity : IUnitCategory
	{
		/// <summary>
		/// Compound quantities. Must only use base quantity units.
		/// </summary>
		Tuple<string, Prefix, UnitFactor[]>[] CompoundQuantities
		{
			get;
		}
	}
}
