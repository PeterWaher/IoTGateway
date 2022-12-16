using System;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Base class for all types of commutative ring elements.
	/// </summary>
	public abstract class CommutativeRingElement : RingElement, ICommutativeRingElement
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
		public override IRingElement MultiplyLeft(IRingElement Element)
		{
			if (Element is ICommutativeRingElement E)
				return this.Multiply(E);
			else
				return null;
		}

		/// <summary>
		/// Tries to multiply an element to the current element, from the right.
		/// </summary>
		/// <param name="Element">Element to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override IRingElement MultiplyRight(IRingElement Element)
		{
			if (Element is ICommutativeRingElement E)
				return this.Multiply(E);
			else
				return null;
		}

		/// <summary>
		/// Tries to multiply an element to the current element.
		/// </summary>
		/// <param name="Element">Element to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public abstract ICommutativeRingElement Multiply(ICommutativeRingElement Element);

		/// <summary>
		/// Associated Set.
		/// </summary>
		public override ISet AssociatedSet => this.AssociatedCommutativeRing;

		/// <summary>
		/// Associated Semi-Group.
		/// </summary>
		public override ISemiGroup AssociatedSemiGroup => this.AssociatedCommutativeRing;

		/// <summary>
		/// Associated Group.
		/// </summary>
		public override IGroup AssociatedGroup => this.AssociatedCommutativeRing;

		/// <summary>
		/// Associated Abelian Group.
		/// </summary>
		public override IAbelianGroup AssociatedAbelianGroup => this.AssociatedCommutativeRing;

		/// <summary>
		/// Associated Ring.
		/// </summary>
		public override IRing AssociatedRing => this.AssociatedCommutativeRing;

		/// <summary>
		/// Associated Commutative Ring.
		/// </summary>
		public abstract ICommutativeRing AssociatedCommutativeRing
		{
			get;
		}
	}
}
