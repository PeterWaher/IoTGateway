using System;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Base class for all types of Euclidian domain elements.
	/// </summary>
	public abstract class EuclidianDomainElement : IntegralDomainElement, IEuclidianDomainElement
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
		public override ISet AssociatedSet
		{
			get { return this.AssociatedEuclidianDomain; }
		}

		/// <summary>
		/// Associated Semi-Group.
		/// </summary>
		public override ISemiGroup AssociatedSemiGroup
		{
			get { return this.AssociatedEuclidianDomain; }
		}

		/// <summary>
		/// Associated Group.
		/// </summary>
		public override IGroup AssociatedGroup
		{
			get { return this.AssociatedEuclidianDomain; }
		}

		/// <summary>
		/// Associated Abelian Group.
		/// </summary>
		public override IAbelianGroup AssociatedAbelianGroup
		{
			get { return this.AssociatedEuclidianDomain; }
		}

		/// <summary>
		/// Associated Ring.
		/// </summary>
		public override IRing AssociatedRing
		{
			get { return this.AssociatedEuclidianDomain; }
		}

		/// <summary>
		/// Associated Commutative Ring.
		/// </summary>
		public override ICommutativeRing AssociatedCommutativeRing
		{
			get { return this.AssociatedEuclidianDomain; }
		}

		/// <summary>
		/// Associated Commutative Ring With Identity.
		/// </summary>
		public override ICommutativeRingWithIdentity AssociatedCommutativeRingWithIdentity
		{
			get { return this.AssociatedEuclidianDomain; }
		}

		/// <summary>
		/// Associated Integral Domain.
		/// </summary>
		public override IIntegralDomain AssociatedIntegralDomain
		{
			get { return this.AssociatedEuclidianDomain; }
		}

		/// <summary>
		/// Associated Euclidian Domain.
		/// </summary>
		public abstract IEuclidianDomain AssociatedEuclidianDomain
		{
			get;
		}
	}
}
