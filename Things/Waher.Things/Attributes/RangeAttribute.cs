using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Things.Attributes
{
	/// <summary>
	/// Defines a valid input range for the property.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class RangeAttribute : Attribute
	{
		private string min;
		private string max;

		/// <summary>
		/// Defines a valid input range for the property.
		/// </summary>
		/// <param name="Min">Smallest accepted value. Can be null, if no minimum exists.</param>
		/// <param name="Max">Largest accepted value. Can be null, if no maximum exists.</param>
		public RangeAttribute(object Min, object Max)
		{
			this.min = Min?.ToString();
			this.max = Max?.ToString();
		}

		/// <summary>
		/// Smallest accepted value. Can be null, if no minimum exists.
		/// </summary>
		public string Min
		{
			get { return this.min; }
		}

		/// <summary>
		/// Largest accepted value. Can be null, if no maximum exists.
		/// </summary>
		public string Max
		{
			get { return this.max; }
		}
	}
}
