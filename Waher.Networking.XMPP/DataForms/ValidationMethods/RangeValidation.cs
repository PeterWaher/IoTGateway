using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.DataForms.ValidationMethods
{
	/// <summary>
	/// Performs range validation.
	/// 
	/// Defined in:
	/// http://xmpp.org/extensions/xep-0122.html#usercases-validation.range
	/// </summary>
	public class RangeValidation : ValidationMethod
	{
		private string min;
		private string max;

		/// <summary>
		/// Performs range validation.
		/// 
		/// Defined in:
		/// http://xmpp.org/extensions/xep-0122.html#usercases-validation.range
		/// </summary>
		/// <param name="Min">Minimum value (string representation).</param>
		/// <param name="Max">Maximum value (string representation).</param>
		public RangeValidation(string Min, string Max)
			: base()
		{
			this.min = Min;
			this.max = Max;
		}

		/// <summary>
		/// Minimum value (string representation).
		/// </summary>
		public string Min { get { return this.min; } }

		/// <summary>
		/// Maximum value (string representation).
		/// </summary>
		public string Max { get { return this.max; } }
	}
}
