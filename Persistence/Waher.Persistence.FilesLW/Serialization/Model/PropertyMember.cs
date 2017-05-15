using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Waher.Persistence.Files.Serialization.Model
{
	/// <summary>
	/// Property member.
	/// </summary>
    public class PropertyMember : Member
    {
		private PropertyInfo pi;

		/// <summary>
		/// Property member.
		/// </summary>
		/// <param name="PI">Property information.</param>
		/// <param name="FieldCode">Field Code.</param>
		public PropertyMember(PropertyInfo PI, ulong FieldCode)
			: base(PI.Name, FieldCode, PI.PropertyType)
		{
			this.pi = PI;
		}

		/// <summary>
		/// Gets the member value.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <returns>Member value.</returns>
		public override object Get(object Object)
		{
			return this.pi.GetValue(Object);
		}

		/// <summary>
		/// Sets the member value.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Value">Member value.</param>
		public override void Set(object Object, object Value)
		{
			this.pi.SetValue(Object, Value);
		}
	}
}
