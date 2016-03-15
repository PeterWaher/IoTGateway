using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Base class for all types of right module elements.
	/// </summary>
	public abstract class RightModuleElement : AbelianGroupElement, IRightModuleElement
	{
		/// <summary>
		/// Base class for all types of right module elements.
		/// </summary>
		public RightModuleElement()
		{
		}

		/// <summary>
		/// Tries to multiply a scalar to the current element from the right.
		/// </summary>
		/// <param name="Scalar">Scalar to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public abstract IRightModuleElement MultiplyScalarRight(IRingElement Scalar);

		/// <summary>
		/// Associated Set.
		/// </summary>
		public override ISet AssociatedSet
		{
			get { return this.AssociatedRightModule; }
		}

		/// <summary>
		/// Associated Semi-Group.
		/// </summary>
		public override ISemiGroup AssociatedSemiGroup
		{
			get { return this.AssociatedRightModule; }
		}

		/// <summary>
		/// Associated Group.
		/// </summary>
		public override IGroup AssociatedGroup
		{
			get { return this.AssociatedRightModule; }
		}

		/// <summary>
		/// Associated Abelian Group.
		/// </summary>
		public override IAbelianGroup AssociatedAbelianGroup
		{
			get { return this.AssociatedRightModule; }
		}

		/// <summary>
		/// Associated Right-Module.
		/// </summary>
		public abstract IRightModule AssociatedRightModule
		{
			get;
		}
	}
}
