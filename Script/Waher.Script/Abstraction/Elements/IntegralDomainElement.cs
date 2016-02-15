using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Base class for all types of integral domain elements.
	/// </summary>
	public abstract class IntegralDomainElement : CommutativeRingWithIdentityElement
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
		public override Set AssociatedSet
		{
			get { return this.AssociatedIntegralDomain; }
		}

		/// <summary>
		/// Associated Semi-Group.
		/// </summary>
		public override SemiGroup AssociatedSemiGroup
		{
			get { return this.AssociatedIntegralDomain; }
		}

		/// <summary>
		/// Associated Group.
		/// </summary>
		public override Group AssociatedGroup
		{
			get { return this.AssociatedIntegralDomain; }
		}

		/// <summary>
		/// Associated Abelian Group.
		/// </summary>
		public override AbelianGroup AssociatedAbelianGroup
		{
			get { return this.AssociatedIntegralDomain; }
		}

		/// <summary>
		/// Associated Ring.
		/// </summary>
		public override Ring AssociatedRing
		{
			get { return this.AssociatedIntegralDomain; }
		}

		/// <summary>
		/// Associated Commutative Ring.
		/// </summary>
		public override CommutativeRing AssociatedCommutativeRing
		{
			get { return this.AssociatedIntegralDomain; }
		}

		/// <summary>
		/// Associated Commutative Ring With Identity.
		/// </summary>
		public override CommutativeRingWithIdentity AssociatedCommutativeRingWithIdentity
		{
			get { return this.AssociatedIntegralDomain; }
		}

		/// <summary>
		/// Associated Integral Domain.
		/// </summary>
		public abstract IntegralDomain AssociatedIntegralDomain
		{
			get;
		}
	}
}
