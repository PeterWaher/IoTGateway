using System;

namespace Waher.Persistence.Attributes
{
	/// <summary>
	/// This attribute informs the persistence layer that the property must be encrypted before persisting it.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class EncryptedAttribute : Attribute
	{
		private readonly int minLength;

		/// <summary>
		/// This attribute informs the persistence layer that the property must be encrypted before persisting it.
		/// </summary>
		public EncryptedAttribute()
			: this(0)
		{
		}

		/// <summary>
		/// This attribute informs the persistence layer that the property must be encrypted before persisting it.
		/// </summary>
		/// <param name="MinLength">Minimum length of the property, in bytes, before 
		/// encryption. If the clear text property is shorter than this, random bytes 
		/// will be appended to pad the property to this length, before encryption.</param>
		public EncryptedAttribute(int MinLength)
			: base()
		{
			this.minLength = MinLength;
		}

		/// <summary>
		/// Minimum length of the property, in bytes, before encryption. If the clear text 
		/// property is shorter than this, random bytes will be appended to pad the property 
		/// to this length, before encryption.
		/// </summary>
		public int MinLength => this.minLength;
	}
}
