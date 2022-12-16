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
		public override ISet AssociatedSet => this.AssociatedEuclidianDomain;

		/// <summary>
		/// Associated Semi-Group.
		/// </summary>
		public override ISemiGroup AssociatedSemiGroup => this.AssociatedEuclidianDomain;

		/// <summary>
		/// Associated Group.
		/// </summary>
		public override IGroup AssociatedGroup => this.AssociatedEuclidianDomain;

		/// <summary>
		/// Associated Abelian Group.
		/// </summary>
		public override IAbelianGroup AssociatedAbelianGroup => this.AssociatedEuclidianDomain;

		/// <summary>
		/// Associated Ring.
		/// </summary>
		public override IRing AssociatedRing => this.AssociatedEuclidianDomain;

		/// <summary>
		/// Associated Commutative Ring.
		/// </summary>
		public override ICommutativeRing AssociatedCommutativeRing => this.AssociatedEuclidianDomain;

		/// <summary>
		/// Associated Commutative Ring With Identity.
		/// </summary>
		public override ICommutativeRingWithIdentity AssociatedCommutativeRingWithIdentity => this.AssociatedEuclidianDomain;

		/// <summary>
		/// Associated Integral Domain.
		/// </summary>
		public override IIntegralDomain AssociatedIntegralDomain => this.AssociatedEuclidianDomain;

		/// <summary>
		/// Associated Euclidian Domain.
		/// </summary>
		public abstract IEuclidianDomain AssociatedEuclidianDomain
		{
			get;
		}
	}
}
