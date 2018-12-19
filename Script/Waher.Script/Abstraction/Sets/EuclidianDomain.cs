using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Base class for all types of euclidian domains.
	/// </summary>
	public abstract class EuclidianDomain : IntegralDomain, IEuclidianDomain
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
		public override ICommutativeRingElement Divide(ICommutativeRingElement Left, ICommutativeRingElement Right)
		{
			IEuclidianDomainElement L = Left as IEuclidianDomainElement;
			IEuclidianDomainElement R = Right as IEuclidianDomainElement;

			if (L is null || R is null)
				return base.Divide(Left, Right);
			else
			{
				IEuclidianDomainElement Remainder;
				IEuclidianDomainElement Result = this.Divide(L, R, out Remainder);
				if (Result is null || !Remainder.Equals(Remainder.AssociatedAbelianGroup.Zero))
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
		public abstract IEuclidianDomainElement Divide(IEuclidianDomainElement Left, IEuclidianDomainElement Right, out IEuclidianDomainElement Remainder);

	}
}
