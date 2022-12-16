using System;
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
		public override ISet AssociatedSet => this.AssociatedField;

		/// <summary>
		/// Associated Semi-Group.
		/// </summary>
		public override ISemiGroup AssociatedSemiGroup => this.AssociatedField;

		/// <summary>
		/// Associated Group.
		/// </summary>
		public override IGroup AssociatedGroup => this.AssociatedField;

		/// <summary>
		/// Associated Abelian Group.
		/// </summary>
		public override IAbelianGroup AssociatedAbelianGroup => this.AssociatedField;

		/// <summary>
		/// Associated Ring.
		/// </summary>
		public override IRing AssociatedRing => this.AssociatedField;

		/// <summary>
		/// Associated Commutative Ring.
		/// </summary>
		public override ICommutativeRing AssociatedCommutativeRing => this.AssociatedField;

		/// <summary>
		/// Associated Commutative Ring With Identity.
		/// </summary>
		public override ICommutativeRingWithIdentity AssociatedCommutativeRingWithIdentity => this.AssociatedField;

		/// <summary>
		/// Associated Integral Domain.
		/// </summary>
		public override IIntegralDomain AssociatedIntegralDomain => this.AssociatedField;

		/// <summary>
		/// Associated Euclidian Domain.
		/// </summary>
		public override IEuclidianDomain AssociatedEuclidianDomain => this.AssociatedField;

		/// <summary>
		/// Associated Field.
		/// </summary>
		public abstract IField AssociatedField
		{
			get;
		}
	}
}
