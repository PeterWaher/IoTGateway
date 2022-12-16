using System;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Base class for all types of integral domain elements.
	/// </summary>
	public abstract class IntegralDomainElement : CommutativeRingWithIdentityElement, IIntegralDomainElement
	{
		/// <summary>
		/// Base class for all types of integral domain elements.
		/// </summary>
		public IntegralDomainElement()
		{
		}

		/// <summary>
		/// Associated Set.
		/// </summary>
		public override ISet AssociatedSet => this.AssociatedIntegralDomain;

		/// <summary>
		/// Associated Semi-Group.
		/// </summary>
		public override ISemiGroup AssociatedSemiGroup => this.AssociatedIntegralDomain;

		/// <summary>
		/// Associated Group.
		/// </summary>
		public override IGroup AssociatedGroup => this.AssociatedIntegralDomain;

		/// <summary>
		/// Associated Abelian Group.
		/// </summary>
		public override IAbelianGroup AssociatedAbelianGroup => this.AssociatedIntegralDomain;

		/// <summary>
		/// Associated Ring.
		/// </summary>
		public override IRing AssociatedRing => this.AssociatedIntegralDomain;

		/// <summary>
		/// Associated Commutative Ring.
		/// </summary>
		public override ICommutativeRing AssociatedCommutativeRing => this.AssociatedIntegralDomain;

		/// <summary>
		/// Associated Commutative Ring With Identity.
		/// </summary>
		public override ICommutativeRingWithIdentity AssociatedCommutativeRingWithIdentity => this.AssociatedIntegralDomain;

		/// <summary>
		/// Associated Integral Domain.
		/// </summary>
		public abstract IIntegralDomain AssociatedIntegralDomain
		{
			get;
		}
	}
}
