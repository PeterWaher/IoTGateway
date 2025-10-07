using System.Reflection;

namespace Waher.Persistence.Serialization.Model
{
	/// <summary>
	/// Property member.
	/// </summary>
	public class PropertyMember : Member
	{
		private readonly PropertyInfo pi;

		/// <summary>
		/// Property member.
		/// </summary>
		/// <param name="PI">Property information.</param>
		/// <param name="FieldCode">Field Code.</param>
		/// <param name="Encrypted">If the member is/should be encrypted.</param>
		/// <param name="DecryptedMinLength">Minimum length of the property, in bytes, before 
		/// encryption. If the clear text property is shorter than this, random bytes will be 
		/// appended to pad the property to this length, before encryption.</param>
		public PropertyMember(PropertyInfo PI, ulong FieldCode, bool Encrypted, int DecryptedMinLength)
			: base(PI.Name, FieldCode, PI.PropertyType, Encrypted, DecryptedMinLength)
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
