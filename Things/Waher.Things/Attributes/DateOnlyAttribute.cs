using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Things.Attributes
{
	/// <summary>
	/// Only edits the date of the underlying DateTime property.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class DateOnlyAttribute : Attribute
	{
		/// <summary>
		/// Only edits the date of the underlying DateTime property.
		/// </summary>
		public DateOnlyAttribute()
		{
		}
	}
}
