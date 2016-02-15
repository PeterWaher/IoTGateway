using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Base class for all types of commutative ring with identity elements.
	/// </summary>
	public abstract class CommutativeRingWithIdentityElement : CommutativeRingElement 
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
		public override Set AssociatedSet
		{
			get { return this.AssociatedCommutativeRingWithIdentity; }
		}

		/// <summary>
		/// Associated Semi-Group.
		/// </summary>
		public override SemiGroup AssociatedSemiGroup
		{
			get { return this.AssociatedCommutativeRingWithIdentity; }
		}

		/// <summary>
		/// Associated Group.
		/// </summary>
		public override Group AssociatedGroup
		{
			get { return this.AssociatedCommutativeRingWithIdentity; }
		}

		/// <summary>
		/// Associated Abelian Group.
		/// </summary>
		public override AbelianGroup AssociatedAbelianGroup
		{
			get { return this.AssociatedCommutativeRingWithIdentity; }
		}

		/// <summary>
		/// Associated Ring.
		/// </summary>
		public override Ring AssociatedRing
		{
			get { return this.AssociatedCommutativeRingWithIdentity; }
		}

		/// <summary>
		/// Associated Commutative Ring.
		/// </summary>
		public override CommutativeRing AssociatedCommutativeRing
		{
			get { return this.AssociatedCommutativeRingWithIdentity; }
		}

		/// <summary>
		/// Associated Commutative Ring With Identity.
		/// </summary>
		public abstract CommutativeRingWithIdentity AssociatedCommutativeRingWithIdentity
		{
			get;
		}
	}
}
