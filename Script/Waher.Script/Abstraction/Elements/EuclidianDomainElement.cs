using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Base class for all types of Euclidian domain elements.
	/// </summary>
	public abstract class EuclidianDomainElement : IntegralDomainElement 
	{
		/// <summary>
		/// Base class for all types of Euclidian domain elements.
		/// </summary>
		public EuclidianDomainElement()
		{
		}

		/// <summary>
		/// Associated Set.
		/// </summary>
		public override Set AssociatedSet
		{
			get { return this.AssociatedEuclidianDomain; }
		}

		/// <summary>
		/// Associated Semi-Group.
		/// </summary>
		public override SemiGroup AssociatedSemiGroup
		{
			get { return this.AssociatedEuclidianDomain; }
		}

		/// <summary>
		/// Associated Group.
		/// </summary>
		public override Group AssociatedGroup
		{
			get { return this.AssociatedEuclidianDomain; }
		}

		/// <summary>
		/// Associated Abelian Group.
		/// </summary>
		public override AbelianGroup AssociatedAbelianGroup
		{
			get { return this.AssociatedEuclidianDomain; }
		}

		/// <summary>
		/// Associated Ring.
		/// </summary>
		public override Ring AssociatedRing
		{
			get { return this.AssociatedEuclidianDomain; }
		}

		/// <summary>
		/// Associated Commutative Ring.
		/// </summary>
		public override CommutativeRing AssociatedCommutativeRing
		{
			get { return this.AssociatedEuclidianDomain; }
		}

		/// <summary>
		/// Associated Commutative Ring With Identity.
		/// </summary>
		public override CommutativeRingWithIdentity AssociatedCommutativeRingWithIdentity
		{
			get { return this.AssociatedEuclidianDomain; }
		}

		/// <summary>
		/// Associated Integral Domain.
		/// </summary>
		public override IntegralDomain AssociatedIntegralDomain
		{
			get { return this.AssociatedEuclidianDomain; }
		}

		/// <summary>
		/// Associated Euclidian Domain.
		/// </summary>
		public abstract EuclidianDomain AssociatedEuclidianDomain
		{
			get;
		}
	}
}
