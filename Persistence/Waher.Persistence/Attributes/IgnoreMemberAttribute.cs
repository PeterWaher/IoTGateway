using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Attributes
{
	/// <summary>
	/// This attribute tells the persistence layer that the member (field or property) should be ignored.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class IgnoreMemberAttribute : Attribute
	{
		/// <summary>
		/// This attribute tells the persistence layer that the member (field or property) should be ignored.
		/// </summary>
		public IgnoreMemberAttribute()
			: base()
		{
		}
	}
}
