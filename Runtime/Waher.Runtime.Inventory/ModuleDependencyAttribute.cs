using System;

namespace Waher.Runtime.Inventory
{
	/// <summary>
	/// Defines a module dependency for a module class. Modules are started after a dependency, and is stopped before a dependency.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public class ModuleDependencyAttribute : Attribute
	{
		private string moduleTypeName;
		private Type moduleType;

		/// <summary>
		/// Defines a module dependency for a module class. Modules are started after a dependency, and is stopped before a dependency.
		/// </summary>
		/// <param name="ModuleTypeName">Type name of module dependency.</param>
		public ModuleDependencyAttribute(string ModuleTypeName)
		{
			this.moduleTypeName = ModuleTypeName;
		}

		/// <summary>
		/// Defines a module dependency for a module class. Modules are started after a dependency, and is stopped before a dependency.
		/// </summary>
		/// <param name="ModuleType">Type of module dependency.</param>
		public ModuleDependencyAttribute(Type ModuleType)
			: this(ModuleType.FullName)
		{
			this.moduleType = ModuleType;
		}

		/// <summary>
		/// Defines a module dependency for a module class. Modules are started after a dependency, and is stopped before a dependency.
		/// </summary>
		/// <param name="Module">Module dependency.</param>
		public ModuleDependencyAttribute(IModule Module)
			: this(Module.GetType())
		{
		}

		/// <summary>
		/// Module Type Name
		/// </summary>
		public string ModeTypeName => this.ModeTypeName;

		/// <summary>
		/// Module Type
		/// </summary>
		public Type ModuleType
		{
			get
			{
				if (this.moduleType is null)
					this.moduleType = Type.GetType(this.moduleTypeName);

				return this.moduleType;
			}
		}

		/// <summary>
		/// Checks if there is a dependency on a given module.
		/// </summary>
		/// <param name="Module">Module</param>
		/// <returns>If a dependency exists.</returns>
		public bool DependsOn(IModule Module)
		{
			return this.DependsOn(Module.GetType());
		}

		/// <summary>
		/// Checks if there is a dependency on a given module.
		/// </summary>
		/// <param name="ModuleType">Module Type</param>
		/// <returns>If a dependency exists.</returns>
		public bool DependsOn(Type ModuleType)
		{
			if (this.moduleType is null)
				return this.moduleTypeName == ModuleType.FullName;
			else
				return this.moduleType == ModuleType;
		}
	}
}
