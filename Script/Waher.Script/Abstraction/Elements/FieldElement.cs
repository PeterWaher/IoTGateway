using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Base class for all types of field elements.
	/// </summary>
	public abstract class FieldElement : EuclidianDomainElement 
	{
		/// <summary>
		/// Base class for all types of field elements.
		/// </summary>
		public FieldElement()
		{
		}

		/// <summary>
		/// Associated Set.
		/// </summary>
		public override Set AssociatedSet
		{
			get { return this.AssociatedField; }
		}

		/// <summary>
		/// Associated Semi-Group.
		/// </summary>
		public override SemiGroup AssociatedSemiGroup
		{
			get { return this.AssociatedField; }
		}

		/// <summary>
		/// Associated Group.
		/// </summary>
		public override Group AssociatedGroup
		{
			get { return this.AssociatedField; }
		}

		/// <summary>
		/// Associated Abelian Group.
		/// </summary>
		public override AbelianGroup AssociatedAbelianGroup
		{
			get { return this.AssociatedField; }
		}

		/// <summary>
		/// Associated Ring.
		/// </summary>
		public override Ring AssociatedRing
		{
			get { return this.AssociatedField; }
		}

		/// <summary>
		/// Associated Commutative Ring.
		/// </summary>
		public override CommutativeRing AssociatedCommutativeRing
		{
			get { return this.AssociatedField; }
		}

		/// <summary>
		/// Associated Commutative Ring With Identity.
		/// </summary>
		public override CommutativeRingWithIdentity AssociatedCommutativeRingWithIdentity
		{
			get { return this.AssociatedField; }
		}

		/// <summary>
		/// Associated Integral Domain.
		/// </summary>
		public override IntegralDomain AssociatedIntegralDomain
		{
			get { return this.AssociatedField; }
		}

		/// <summary>
		/// Associated Euclidian Domain.
		/// </summary>
		public override EuclidianDomain AssociatedEuclidianDomain
		{
			get { return this.AssociatedField; }
		}

		/// <summary>
		/// Associated Field.
		/// </summary>
		public abstract Field AssociatedField
		{
			get;
		}
	}
}
