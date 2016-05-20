using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Provisioning.SearchOperators
{
	/// <summary>
	/// Filters things with a named string-valued tag outside a given range.
	/// </summary>
	public class StringTagNotInRange : StringTagRange 
	{
		/// <summary>
		/// Filters things with a named string-valued tag outside a given range.
		/// </summary>
		/// <param name="Name">Tag name.</param>
		/// <param name="Min">Minimum value.</param>
		/// <param name="MinIncluded">If the minimum value is included in the range.</param>
		/// <param name="Max">Maximum value.</param>
		/// <param name="MaxIncluded">If the maximum value is included in the range.</param>
		public StringTagNotInRange(string Name, string Min, bool MinIncluded, string Max, bool MaxIncluded)
			: base(Name, Min, MinIncluded, Max, MaxIncluded)
		{
		}

		internal override string TagName
		{
			get { return "strNRange"; }
		}
	}
}
