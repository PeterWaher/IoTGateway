using System;

namespace Waher.Things.Attributes
{
	/// <summary>
	/// Defines an open parameter. Open parameters accept values outside of listed options, as long as values conform to the
	/// underlying data type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class OpenAttribute : Attribute
	{
		/// <summary>
		/// Defines an open parameter. Open parameters accept values outside of listed options, as long as values conform to the
		/// underlying data type.
		/// </summary>
		public OpenAttribute()
		{
		}
	}
}
