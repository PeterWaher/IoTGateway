using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Attributes
{
	/// <summary>
	/// This attribute defines the field as containing the Object ID of the object.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class ObjectIdAttribute : Attribute
	{
		/// <summary>
		/// This attribute defines the field as containing the Object ID of the object.
		/// </summary>
		public ObjectIdAttribute()
			: base()
		{
		}
	}
}
