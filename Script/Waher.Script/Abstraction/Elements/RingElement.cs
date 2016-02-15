using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Base class for all types of ring elements.
	/// </summary>
	public abstract class RingElement : AbelianGroupElement
	{
		/// <summary>
		/// Base class for all types of ring elements.
		/// </summary>
		public RingElement()
		{
		}

		/// <summary>
		/// Tries to multiply an element to the current element, from the left.
		/// </summary>
		/// <param name="Element">Element to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public abstract RingElement MultiplyLeft(RingElement Element);

		/// <summary>
		/// Tries to multiply an element to the current element, from the right.
		/// </summary>
		/// <param name="Element">Element to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public abstract RingElement MultiplyRight(RingElement Element);

		/// <summary>
		/// Inverts the element, if possible.
		/// </summary>
		/// <returns>Inverted element, or null if not possible.</returns>
		public abstract RingElement Invert();

		/// <summary>
		/// Associated Set.
		/// </summary>
		public override Set AssociatedSet
		{
			get { return this.AssociatedRing; }
		}

		/// <summary>
		/// Associated Semi-Group.
		/// </summary>
		public override SemiGroup AssociatedSemiGroup
		{
			get { return this.AssociatedRing; }
		}

		/// <summary>
		/// Associated Group.
		/// </summary>
		public override Group AssociatedGroup
		{
			get { return this.AssociatedRing; }
		}

		/// <summary>
		/// Associated Abelian Group.
		/// </summary>
		public override AbelianGroup AssociatedAbelianGroup
		{
			get { return this.AssociatedRing; }
		}

		/// <summary>
		/// Associated Ring.
		/// </summary>
		public abstract Ring AssociatedRing
		{
			get;
		}
	}
}
