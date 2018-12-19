using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Base class for all types of fields.
	/// </summary>
	public abstract class Field : EuclidianDomain, IField
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
		public override ICommutativeRingElement Divide(ICommutativeRingElement Left, ICommutativeRingElement Right)
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
		public override IEuclidianDomainElement Divide(IEuclidianDomainElement Left, IEuclidianDomainElement Right, out IEuclidianDomainElement Remainder)
		{
			EuclidianDomainElement Result = this.Divide(Left, Right) as EuclidianDomainElement;
			if (Result is null)
				Remainder = null;
			else
				Remainder = Result.AssociatedAbelianGroup.Zero as EuclidianDomainElement;

			return Result;
		}

	}
}
