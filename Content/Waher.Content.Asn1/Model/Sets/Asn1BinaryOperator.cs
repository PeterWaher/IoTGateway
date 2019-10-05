using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Sets
{
	/// <summary>
	/// Abstract base class of binary set operators
	/// </summary>
	public abstract class Asn1BinaryOperator : Asn1Set
	{
		private readonly Asn1Set left;
		private readonly Asn1Set right;

		/// <summary>
		/// Abstract base class of binary set operators
		/// </summary>
		/// <param name="Left">Left set</param>
		/// <param name="Right">Right set</param>
		public Asn1BinaryOperator(Asn1Set Left, Asn1Set Right)
		{
			this.left = Left;
			this.right = Right;
		}

		/// <summary>
		/// Left set
		/// </summary>
		public Asn1Set Left => this.left;

		/// <summary>
		/// Right set
		/// </summary>
		public Asn1Set Right => this.right;
	}
}
