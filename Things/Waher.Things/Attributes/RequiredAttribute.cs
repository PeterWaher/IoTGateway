using System;

namespace Waher.Things.Attributes
{
	/// <summary>
	/// Defines a required parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class RequiredAttribute : Attribute
	{
		/// <summary>
		/// Defines a required parameter.
		/// </summary>
		public RequiredAttribute()
		{
		}
	}
}
