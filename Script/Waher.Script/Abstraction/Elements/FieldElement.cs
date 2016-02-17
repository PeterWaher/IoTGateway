using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Base class for all types of field elements.
	/// </summary>
	public abstract class FieldElement : EuclidianDomainElement, IFieldElement 
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
		public override ISet AssociatedSet
		{
			get { return this.AssociatedField; }
		}

		/// <summary>
		/// Associated Semi-Group.
		/// </summary>
		public override ISemiGroup AssociatedSemiGroup
		{
			get { return this.AssociatedField; }
		}

		/// <summary>
		/// Associated Group.
		/// </summary>
		public override IGroup AssociatedGroup
		{
			get { return this.AssociatedField; }
		}

		/// <summary>
		/// Associated Abelian Group.
		/// </summary>
		public override IAbelianGroup AssociatedAbelianGroup
		{
			get { return this.AssociatedField; }
		}

		/// <summary>
		/// Associated Ring.
		/// </summary>
		public override IRing AssociatedRing
		{
			get { return this.AssociatedField; }
		}

		/// <summary>
		/// Associated Commutative Ring.
		/// </summary>
		public override ICommutativeRing AssociatedCommutativeRing
		{
			get { return this.AssociatedField; }
		}

		/// <summary>
		/// Associated Commutative Ring With Identity.
		/// </summary>
		public override ICommutativeRingWithIdentity AssociatedCommutativeRingWithIdentity
		{
			get { return this.AssociatedField; }
		}

		/// <summary>
		/// Associated Integral Domain.
		/// </summary>
		public override IIntegralDomain AssociatedIntegralDomain
		{
			get { return this.AssociatedField; }
		}

		/// <summary>
		/// Associated Euclidian Domain.
		/// </summary>
		public override IEuclidianDomain AssociatedEuclidianDomain
		{
			get { return this.AssociatedField; }
		}

		/// <summary>
		/// Associated Field.
		/// </summary>
		public abstract IField AssociatedField
		{
			get;
		}
	}
}
