using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Runtime.Inventory
{
	/// <summary>
	/// Grade enumeration
	/// </summary>
	public enum Grade
	{
		/// <summary>
		/// Match is perfect.
		/// </summary>
		Perfect = 4,

		/// <summary>
		/// Match is excellent.
		/// </summary>
		Excellent = 3,

		/// <summary>
		/// Match is ok.
		/// </summary>
		Ok = 2,

		/// <summary>
		/// Match is limited
		/// </summary>
		Barely = 1,

		/// <summary>
		/// No match.
		/// </summary>
		NotAtAll = 0
	}
}
