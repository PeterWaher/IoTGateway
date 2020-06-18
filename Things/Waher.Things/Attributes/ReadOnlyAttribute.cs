using System;

namespace Waher.Things.Attributes
{
	/// <summary>
	/// Defines a read-only parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class ReadOnlyAttribute : Attribute
	{
		/// <summary>
		/// Defines a read-only parameter.
		/// </summary>
		public ReadOnlyAttribute()
		{
		}
	}
}
