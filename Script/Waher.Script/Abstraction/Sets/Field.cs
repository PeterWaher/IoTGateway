using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Base class for all types of fields.
	/// </summary>
	public abstract class Field : EuclidianDomain 
	{
		/// <summary>
		/// Base class for all types of fields.
		/// </summary>
		public Field()
			: base()
		{
		}

		/// <summary>
		/// Divides the right ring element from the left one: Left/Right
		/// </summary>
		/// <param name="Left">Left element.</param>
		/// <param name="Right">Right element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override CommutativeRingElement Divide(CommutativeRingElement Left, CommutativeRingElement Right)
		{
			return this.Multiply(Left, Right.Invert()) as CommutativeRingElement;
		}

		/// <summary>
		/// Divides the right ring element from the left one: Left/Right
		/// </summary>
		/// <param name="Left">Left element.</param>
		/// <param name="Right">Right element.</param>
		/// <param name="Remainder">Remainder.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override EuclidianDomainElement Divide(EuclidianDomainElement Left, EuclidianDomainElement Right, out EuclidianDomainElement Remainder)
		{
			EuclidianDomainElement Result = this.Divide(Left, Right) as EuclidianDomainElement;
			if (Result == null)
				Remainder = null;
			else
				Remainder = Result.AssociatedAbelianGroup.Zero as EuclidianDomainElement;

			return Result;
		}

	}
}
