using System.Collections.Generic;
using Waher.Script.Objects;

namespace Waher.Script.Units
{
	/// <summary>
	/// Interface for derived quantities 
	/// </summary>
	public interface IDerivedQuantity : IUnitCategory
	{
		/// <summary>
		/// Derived Units supported.
		/// </summary>
		KeyValuePair<string, PhysicalQuantity>[] DerivedUnits
		{
			get;
		}
	}
}
