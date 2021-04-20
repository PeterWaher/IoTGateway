using System;

namespace Waher.Persistence.Attributes
{
	/// <summary>
	/// This attribute defines the field as containing the Object ID of the object. When creating a new object, the value of the 
	/// Object ID field should be equal to null.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class ObjectIdAttribute : Attribute
	{
		/// <summary>
		/// This attribute defines the field as containing the Object ID of the object. When creating a new object, the value of the 
		/// Object ID field should be equal to null.
		/// </summary>
		public ObjectIdAttribute()
			: base()
		{
		}
	}
}
