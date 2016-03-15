using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Base class for all types of left module elements.
	/// </summary>
	public abstract class LeftModuleElement : AbelianGroupElement, ILeftModuleElement
	{
		/// <summary>
		/// Base class for all types of left module elements.
		/// </summary>
		public LeftModuleElement()
		{
		}

		/// <summary>
		/// Tries to multiply a scalar to the current element from the left.
		/// </summary>
		/// <param name="Scalar">Scalar to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public abstract ILeftModuleElement MultiplyScalarLeft(IRingElement Scalar);

		/// <summary>
		/// Associated Set.
		/// </summary>
		public override ISet AssociatedSet
		{
			get { return this.AssociatedLeftModule; }
		}

		/// <summary>
		/// Associated Semi-Group.
		/// </summary>
		public override ISemiGroup AssociatedSemiGroup
		{
			get { return this.AssociatedLeftModule; }
		}

		/// <summary>
		/// Associated Group.
		/// </summary>
		public override IGroup AssociatedGroup
		{
			get { return this.AssociatedLeftModule; }
		}

		/// <summary>
		/// Associated Abelian Group.
		/// </summary>
		public override IAbelianGroup AssociatedAbelianGroup
		{
			get { return this.AssociatedLeftModule; }
		}

		/// <summary>
		/// Associated Left-Module.
		/// </summary>
		public abstract ILeftModule AssociatedLeftModule
		{
			get;
		}
	}
}
