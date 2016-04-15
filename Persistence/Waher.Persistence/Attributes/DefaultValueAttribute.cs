using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Attributes
{
	/// <summary>
	/// This attribute informs the persistence layer about the default value of a member (field or property).
	/// Default values are not persisted, which allows for more efficient storage.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class DefaultValueAttribute : Attribute
	{
		private object value;

		/// <summary>
		/// This attribute informs the persistence layer about the default value of a member (field or property).
		/// Default values are not persisted, which allows for more efficient storage.
		/// </summary>
		/// <param name="Value">Default value.</param>
		public DefaultValueAttribute(object Value)
			: base()
		{
			this.value = Value;
		}

		/// <summary>
		/// Default value of member.
		/// </summary>
		public object Value
		{
			get { return this.value; }
		}
	}
}
