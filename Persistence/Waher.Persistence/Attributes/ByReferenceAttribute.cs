using System;

namespace Waher.Persistence.Attributes
{
	/// <summary>
	/// The contents of this field is persisted by reference only. The default behaviour is to persist object fields as embedded
	/// sub-objects. By adding the <see cref="ByReferenceAttribute"/> to a field or property declaration, you tell the persistence
	/// layer that persistence of the value is done separately, and that only a reference to the corresponding object is to be
	/// persisted.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class ByReferenceAttribute : Attribute
	{
		/// <summary>
		/// The contents of this field is persisted by reference only. The default behaviour is to persist object fields as embedded
		/// sub-objects. By adding the <see cref="ByReferenceAttribute"/> to a field or property declaration, you tell the persistence
		/// layer that persistence of the value is done separately, and that only a reference to the corresponding object is to be
		/// persisted.
		/// </summary>
		public ByReferenceAttribute()
			: base()
		{
		}
	}
}
