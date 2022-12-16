using System;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Base class for all types of commutative ring with identity elements.
	/// </summary>
	public abstract class CommutativeRingWithIdentityElement : CommutativeRingElement, ICommutativeRingWithIdentityElement
	{
		/// <summary>
		/// Base class for all types of commutative ring with identity elements.
		/// </summary>
		public CommutativeRingWithIdentityElement()
		{
		}

		/// <summary>
		/// Associated Set.
		/// </summary>
		public override ISet AssociatedSet => this.AssociatedCommutativeRingWithIdentity;

		/// <summary>
		/// Associated Semi-Group.
		/// </summary>
		public override ISemiGroup AssociatedSemiGroup => this.AssociatedCommutativeRingWithIdentity;

		/// <summary>
		/// Associated Group.
		/// </summary>
		public override IGroup AssociatedGroup => this.AssociatedCommutativeRingWithIdentity;

		/// <summary>
		/// Associated Abelian Group.
		/// </summary>
		public override IAbelianGroup AssociatedAbelianGroup => this.AssociatedCommutativeRingWithIdentity;

		/// <summary>
		/// Associated Ring.
		/// </summary>
		public override IRing AssociatedRing => this.AssociatedCommutativeRingWithIdentity;

		/// <summary>
		/// Associated Commutative Ring.
		/// </summary>
		public override ICommutativeRing AssociatedCommutativeRing => this.AssociatedCommutativeRingWithIdentity;

		/// <summary>
		/// Associated Commutative Ring With Identity.
		/// </summary>
		public abstract ICommutativeRingWithIdentity AssociatedCommutativeRingWithIdentity
		{
			get;
		}

		/// <summary>
		/// Returns the identity element of the commutative ring with identity.
		/// </summary>
		public abstract ICommutativeRingWithIdentityElement One
		{
			get;
		}

	}
}
