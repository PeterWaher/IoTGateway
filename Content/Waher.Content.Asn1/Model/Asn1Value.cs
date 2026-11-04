using System;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Abstract base class for values.
	/// </summary>
	public abstract class Asn1Value : Asn1Node
	{
		private bool powerOfTwo = false;

		/// <summary>
		/// Abstract base class for values.
		/// </summary>
		public Asn1Value()
			: base()
		{
		}

		/// <summary>
		/// If the value represents 2^original value.
		/// </summary>
		public bool PowerOfTwo
		{
			get => this.powerOfTwo;
			protected set => this.powerOfTwo = value;
		}

		/// <summary>
		/// Makes the value 2^value, for use with <see cref="System.FlagsAttribute"/>
		/// enumerations.
		/// </summary>
		public virtual void TwosPower()
		{
			throw new NotImplementedException("Invalid power of two.");
		}

		/// <summary>
		/// Corresponding C# type.
		/// </summary>
		public virtual string CSharpType
		{
			get
			{
				throw new NotImplementedException("Support for value type " +
					this.GetType().FullName + " not implemented.");
			}
		}
	}
}
