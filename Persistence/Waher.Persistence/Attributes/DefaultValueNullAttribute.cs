using System;

namespace Waher.Persistence.Attributes
{
	/// <summary>
	/// This attribute informs the persistence layer about the default value (null) of a member 
	/// (field or property). Default values are not persisted, which allows for more efficient storage.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class DefaultValueNullAttribute : DefaultValueAttribute
	{
		/// <summary>
		/// This attribute informs the persistence layer about the default value (null) of a member 
		/// (field or property). Default values are not persisted, which allows for more efficient storage.
		/// </summary>
		public DefaultValueNullAttribute()
			: base(null)
		{
		}
	}
}
