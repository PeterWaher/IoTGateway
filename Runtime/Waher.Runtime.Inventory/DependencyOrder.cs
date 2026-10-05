using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Waher.Events;
using Waher.Runtime.Collections;

namespace Waher.Runtime.Inventory
{
	/// <summary>
	/// Orders modules in dependency order.
	/// </summary>
	public class DependencyOrder : IModuleOrder
	{
		private readonly Dictionary<string, ModuleRec> modulesPerTypeName = new Dictionary<string, ModuleRec>();
		private IEnumerable<IModule> modules;

		private class ModuleRec
		{
			public Type Type;
			public IModule Module;
			public HashSet<string> DependenciesLeft;
			public int NrDependenciesLeft;
			public int DependencyOrder;
		}

		/// <summary>
		/// Sets the modules to order.
		/// </summary>
		/// <param name="Modules">Modules available.</param>
		public void SetModules(IEnumerable<IModule> Modules)
		{
			this.modules = Modules;
			this.modulesPerTypeName.Clear();

			ChunkedList<ModuleRec> DependendenciesLeft = new ChunkedList<ModuleRec>();
			ChunkedList<ModuleRec> Unresolved = null;
			int i, NrDependenciesLeft;
			bool DependencyResolved;

			foreach (IModule Module in Modules)
			{
				Type T = Module.GetType();
				IEnumerable<ModuleDependencyAttribute> Dependencies = T.GetCustomAttributes<ModuleDependencyAttribute>();
				HashSet<string> DependencyNames = new HashSet<string>();

				foreach (ModuleDependencyAttribute Dependency in Dependencies)
					DependencyNames.Add(Dependency.ModuleTypeName);

				ModuleRec Rec = new ModuleRec()
				{
					Type = T,
					Module = Module,
					DependenciesLeft = DependencyNames,
					NrDependenciesLeft = DependencyNames.Count,
					DependencyOrder = 0
				};

				if (Rec.NrDependenciesLeft == 0)
					this.modulesPerTypeName[T.FullName] = Rec;
				else
					DependendenciesLeft.Add(Rec);
			}

			NrDependenciesLeft = DependendenciesLeft.Count;

			while (NrDependenciesLeft > 0)
			{
				DependencyResolved = false;

				for (i = 0; i < NrDependenciesLeft; i++)
				{
					ModuleRec Rec = DependendenciesLeft[i];

					Rec.DependencyOrder++;

					string[] Dependencies = new string[Rec.NrDependenciesLeft];
					Rec.DependenciesLeft.CopyTo(Dependencies);

					foreach (string Dependency in Dependencies)
					{
						if (this.modulesPerTypeName.ContainsKey(Dependency))
						{
							Rec.DependenciesLeft.Remove(Dependency);
							Rec.NrDependenciesLeft--;

							if (Rec.NrDependenciesLeft == 0)
							{
								this.modulesPerTypeName[Rec.Type.FullName] = Rec;
								DependendenciesLeft.RemoveAt(i);
								NrDependenciesLeft--;
								i--;
								DependencyResolved = true;
								break;
							}
						}
					}
				}

				if (!DependencyResolved)
				{
					ModuleRec Rec = DependendenciesLeft.FirstItem;
					
					DependendenciesLeft.RemoveFirst();
					NrDependenciesLeft--;

					if (Unresolved is null)
						Unresolved = new ChunkedList<ModuleRec>();

					this.modulesPerTypeName[Rec.Type.FullName] = Rec;
					Unresolved.Add(Rec);
				}
			}

			if (!(Unresolved is null))
			{
				StringBuilder sb = new StringBuilder();

				sb.AppendLine("Unable to resolve dependencies for the following modules:");

				foreach (ModuleRec Rec in Unresolved)
				{
					sb.AppendLine();
					sb.Append(Rec.Type.FullName);
				}

				Log.Error(sb.ToString());
			}
		}

		/// <summary>
		/// Compares two modules.
		/// </summary>
		/// <param name="x">Module 1</param>
		/// <param name="y">Module 2</param>
		/// <returns>Signed comparison result.</returns>
		public int Compare(IModule x, IModule y)
		{
			if (!this.modulesPerTypeName.TryGetValue(x.GetType().FullName, out ModuleRec RecX))
				return 1;

			if (!this.modulesPerTypeName.TryGetValue(y.GetType().FullName, out ModuleRec RecY))
				return -1;

			int i = RecX.DependencyOrder - RecY.DependencyOrder;
			if (i != 0)
				return i;

			return RecX.Type.FullName.CompareTo(RecY.Type.FullName);
		}
	}
}
