using System;
using System.Collections.Generic;
using System.Text;
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
		public override ISet AssociatedSet
		{
			get { return this.AssociatedIntegralDomain; }
		}

		/// <summary>
		/// Associated Semi-Group.
		/// </summary>
		public override ISemiGroup AssociatedSemiGroup
		{
			get { return this.AssociatedIntegralDomain; }
		}

		/// <summary>
		/// Associated Group.
		/// </summary>
		public override IGroup AssociatedGroup
		{
			get { return this.AssociatedIntegralDomain; }
		}

		/// <summary>
		/// Associated Abelian Group.
		/// </summary>
		public override IAbelianGroup AssociatedAbelianGroup
		{
			get { return this.AssociatedIntegralDomain; }
		}

		/// <summary>
		/// Associated Ring.
		/// </summary>
		public override IRing AssociatedRing
		{
			get { return this.AssociatedIntegralDomain; }
		}

		/// <summary>
		/// Associated Commutative Ring.
		/// </summary>
		public override ICommutativeRing AssociatedCommutativeRing
		{
			get { return this.AssociatedIntegralDomain; }
		}

		/// <summary>
		/// Associated Commutative Ring With Identity.
		/// </summary>
		public override ICommutativeRingWithIdentity AssociatedCommutativeRingWithIdentity
		{
			get { return this.AssociatedIntegralDomain; }
		}

		/// <summary>
		/// Associated Integral Domain.
		/// </summary>
		public abstract IIntegralDomain AssociatedIntegralDomain
		{
			get;
		}
	}
}
