using System;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Base class for all types of module elements.
	/// </summary>
	public abstract class ModuleElement : AbelianGroupElement, IModuleElement
	{
		/// <summary>
		/// Base class for all types of module elements.
		/// </summary>
		public ModuleElement()
		{
		}

		/// <summary>
		/// Tries to multiply a scalar to the current element from the left.
		/// </summary>
		/// <param name="Scalar">Scalar to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public virtual ILeftModuleElement MultiplyScalarLeft(IRingElement Scalar)
		{
			return this.MultiplyScalar(Scalar) as ILeftModuleElement;
		}

		/// <summary>
		/// Tries to multiply a scalar to the current element from the right.
		/// </summary>
		/// <param name="Scalar">Scalar to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public virtual IRightModuleElement MultiplyScalarRight(IRingElement Scalar)
		{
			return this.MultiplyScalar(Scalar) as IRightModuleElement;
		}

		/// <summary>
		/// Tries to multiply a scalar to the current element.
		/// </summary>
		/// <param name="Scalar">Scalar to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public abstract IModuleElement MultiplyScalar(IRingElement Scalar);

		/// <summary>
		/// Associated Set.
		/// </summary>
		public override ISet AssociatedSet => this.AssociatedModule;

		/// <summary>
		/// Associated Semi-Group.
		/// </summary>
		public override ISemiGroup AssociatedSemiGroup => this.AssociatedModule;

		/// <summary>
		/// Associated Group.
		/// </summary>
		public override IGroup AssociatedGroup => this.AssociatedModule;

		/// <summary>
		/// Associated Abelian Group.
		/// </summary>
		public override IAbelianGroup AssociatedAbelianGroup => this.AssociatedModule;

		/// <summary>
		/// Associated Left-Module.
		/// </summary>
		public virtual ILeftModule AssociatedLeftModule => this.AssociatedModule;

		/// <summary>
		/// Associated Right-Module.
		/// </summary>
		public virtual IRightModule AssociatedRightModule => this.AssociatedModule;

		/// <summary>
		/// Associated Module.
		/// </summary>
		public abstract Sets.IModule AssociatedModule
		{
			get;
		}
	}
}
