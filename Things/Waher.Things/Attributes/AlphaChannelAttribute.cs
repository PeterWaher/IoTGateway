using System;

namespace Waher.Things.Attributes
{
	/// <summary>
	/// Includes the alpha channel of a color property.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class AlphaChannelAttribute : Attribute
	{
		/// <summary>
		/// Includes the alpha channel of a color property.
		/// </summary>
		public AlphaChannelAttribute()
		{
		}
	}
}
