using System;

namespace Waher.Things.Attributes
{
	/// <summary>
	/// Defines a masked parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class MaskedAttribute : Attribute
	{
		/// <summary>
		/// Defines a masked parameter.
		/// </summary>
		public MaskedAttribute()
		{
		}
	}
}
