using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Attributes
{
	/// <summary>
	/// This attribute informs the persistence layer about the default value (<see cref="TimeSpan.MaxValue"/>) of a member 
	/// (field or property). Default values are not persisted, which allows for more efficient storage.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class DefaultValueTimeSpanMaxValueAttribute : DefaultValueAttribute
	{
		/// <summary>
		/// This attribute informs the persistence layer about the default value (<see cref="TimeSpan.MaxValue"/>) of a member 
		/// (field or property). Default values are not persisted, which allows for more efficient storage.
		/// </summary>
		public DefaultValueTimeSpanMaxValueAttribute()
			: base(TimeSpan.MaxValue)
		{
		}
	}
}
