using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Waher.Persistence.Files.Serialization.Model
{
	/// <summary>
	/// Field member.
	/// </summary>
    public class FieldMember : Member
    {
		private FieldInfo fi;

		/// <summary>
		/// Field member.
		/// </summary>
		/// <param name="FI">Field information.</param>
		/// <param name="FieldCode">Field Code.</param>
		public FieldMember(FieldInfo FI, ulong FieldCode)
			: base(FI.Name, FieldCode, FI.FieldType)
		{
			this.fi = FI;
		}

		/// <summary>
		/// Gets the member value.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <returns>Member value.</returns>
		public override object Get(object Object)
		{
			return this.fi.GetValue(Object);
		}

		/// <summary>
		/// Sets the member value.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Value">Member value.</param>
		public override void Set(object Object, object Value)
		{
			this.fi.SetValue(Object, Value);
		}
	}
}
