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
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Value(Asn1Document Document)
			: base(Document)
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

	}
}
