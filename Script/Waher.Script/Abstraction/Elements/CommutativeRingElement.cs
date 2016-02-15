using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Base class for all types of commutative ring elements.
	/// </summary>
	public abstract class CommutativeRingElement : RingElement 
	{
		/// <summary>
		/// Base class for all types of commutative ring elements.
		/// </summary>
		public CommutativeRingElement()
		{
		}

		/// <summary>
		/// Tries to multiply an element to the current element, from the left.
		/// </summary>
		/// <param name="Element">Element to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override RingElement MultiplyLeft(RingElement Element)
		{
			CommutativeRingElement E = Element as CommutativeRingElement;
			if (E == null)
				return null;
			else
				return this.Multiply(E);
		}

		/// <summary>
		/// Tries to multiply an element to the current element, from the right.
		/// </summary>
		/// <param name="Element">Element to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override RingElement MultiplyRight(RingElement Element)
		{
			CommutativeRingElement E = Element as CommutativeRingElement;
			if (E == null)
				return null;
			else
				return this.Multiply(E);
		}

		/// <summary>
		/// Tries to multiply an element to the current element.
		/// </summary>
		/// <param name="Element">Element to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public abstract RingElement Multiply(CommutativeRingElement Element);

		/// <summary>
		/// Associated Set.
		/// </summary>
		public override Set AssociatedSet
		{
			get { return this.AssociatedCommutativeRing; }
		}

		/// <summary>
		/// Associated Semi-Group.
		/// </summary>
		public override SemiGroup AssociatedSemiGroup
		{
			get { return this.AssociatedCommutativeRing; }
		}

		/// <summary>
		/// Associated Group.
		/// </summary>
		public override Group AssociatedGroup
		{
			get { return this.AssociatedCommutativeRing; }
		}

		/// <summary>
		/// Associated Abelian Group.
		/// </summary>
		public override AbelianGroup AssociatedAbelianGroup
		{
			get { return this.AssociatedCommutativeRing; }
		}

		/// <summary>
		/// Associated Ring.
		/// </summary>
		public override Ring AssociatedRing
		{
			get { return this.AssociatedCommutativeRing; }
		}

		/// <summary>
		/// Associated Commutative Ring.
		/// </summary>
		public abstract CommutativeRing AssociatedCommutativeRing
		{
			get;
		}
	}
}
