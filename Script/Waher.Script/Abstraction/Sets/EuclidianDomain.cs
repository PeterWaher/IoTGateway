using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Base class for all types of euclidian domains.
	/// </summary>
	public abstract class EuclidianDomain : IntegralDomain
	{
		/// <summary>
		/// Base class for all types of euclidian domains.
		/// </summary>
		public EuclidianDomain()
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
			EuclidianDomainElement L = Left as EuclidianDomainElement;
			EuclidianDomainElement R = Right as EuclidianDomainElement;

			if (L == null || R == null)
				return base.Divide(Left, Right);
			else
			{
				EuclidianDomainElement Remainder;
				EuclidianDomainElement Result = this.Divide(L, R, out Remainder);
				if (Result == null || !Remainder.Equals(Remainder.AssociatedAbelianGroup.Zero))
					return null;
				else
					return Result;
			}
		}

		/// <summary>
		/// Divides the right ring element from the left one: Left/Right
		/// </summary>
		/// <param name="Left">Left element.</param>
		/// <param name="Right">Right element.</param>
		/// <param name="Remainder">Remainder.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public abstract EuclidianDomainElement Divide(EuclidianDomainElement Left, EuclidianDomainElement Right, out EuclidianDomainElement Remainder);

	}
}
