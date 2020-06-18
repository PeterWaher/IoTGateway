using System;

namespace Waher.Things.Attributes
{
	/// <summary>
	/// Only edits the date of the underlying DateTime property.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
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
