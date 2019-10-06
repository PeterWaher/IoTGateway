using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Restrictions
{
	/// <summary>
	/// Abstract base class of binary restriction operators
	/// </summary>
	public abstract class Asn1BinaryRestrictionOperator : Asn1Restriction
	{
		private readonly Asn1Restriction left;
		private readonly Asn1Restriction right;

		/// <summary>
		/// Abstract base class of binary restriction operators
		/// </summary>
		/// <param name="Left">Left set</param>
		/// <param name="Right">Right set</param>
		public Asn1BinaryRestrictionOperator(Asn1Restriction Left, Asn1Restriction Right)
		{
			this.left = Left;
			this.right = Right;
		}

		/// <summary>
		/// Left restriction
		/// </summary>
		public Asn1Restriction Left => this.left;

		/// <summary>
		/// Right restriction
		/// </summary>
		public Asn1Restriction Right => this.right;
	}
}
