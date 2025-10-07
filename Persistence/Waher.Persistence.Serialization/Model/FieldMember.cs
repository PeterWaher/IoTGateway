using System.Reflection;

namespace Waher.Persistence.Serialization.Model
{
	/// <summary>
	/// Field member.
	/// </summary>
    public class FieldMember : Member
    {
		private readonly FieldInfo fi;

		/// <summary>
		/// Field member.
		/// </summary>
		/// <param name="FI">Field information.</param>
		/// <param name="FieldCode">Field Code.</param>
		/// <param name="Encrypted">If the member is/should be encrypted.</param>
		/// <param name="DecryptedMinLength">Minimum length of the property, in bytes, before 
		/// encryption. If the clear text property is shorter than this, random bytes will be 
		/// appended to pad the property to this length, before encryption.</param>
		public FieldMember(FieldInfo FI, ulong FieldCode, bool Encrypted, int DecryptedMinLength)
			: base(FI.Name, FieldCode, FI.FieldType, Encrypted, DecryptedMinLength)
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
