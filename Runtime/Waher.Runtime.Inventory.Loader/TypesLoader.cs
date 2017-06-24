using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Waher.Events;
using Waher.Runtime.Inventory;

namespace Waher.Runtime.Inventory.Loader
{
	/// <summary>
	/// Static class, loading and initializing assemblies dynamically.
	/// </summary>
	public static class TypesLoader
	{
		/// <summary>
		/// Initializes the inventory engine, registering types and interfaces available in <see cref="Types"/>.
		/// </summary>
		public static void Initialize()
		{
			Initialize(string.Empty);
		}

		/// <summary>
		/// Initializes the inventory engine, registering types and interfaces available in <see cref="Types"/>.
		/// </summary>
		/// <param name="Folder">Name of folder containing assemblies to load, if they are not already loaded.</param>
		public static void Initialize(string Folder)
		{
			if (string.IsNullOrEmpty(Folder))
				Folder = Path.GetDirectoryName(typeof(TypesLoader).GetTypeInfo().Assembly.Location);

			string[] DllFiles = Directory.GetFiles(Folder, "*.dll", SearchOption.TopDirectoryOnly);
			Dictionary<string, Assembly> LoadedAssemblies = new Dictionary<string, Assembly>(StringComparer.CurrentCultureIgnoreCase);
			Dictionary<string, AssemblyName> ReferencedAssemblies = new Dictionary<string, AssemblyName>(StringComparer.CurrentCultureIgnoreCase);

			foreach (string DllFile in DllFiles)
			{
				try
				{
					Assembly A = AssemblyLoadContext.Default.LoadFromAssemblyPath(DllFile);
					LoadedAssemblies[A.GetName().FullName] = A;

					foreach (AssemblyName AN in A.GetReferencedAssemblies())
						ReferencedAssemblies[AN.FullName] = AN;
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

			do
			{
				AssemblyName[] References = new AssemblyName[ReferencedAssemblies.Count];
				ReferencedAssemblies.Values.CopyTo(References, 0);
				ReferencedAssemblies.Clear();

				foreach (AssemblyName AN in References)
				{
					if (LoadedAssemblies.ContainsKey(AN.FullName))
						continue;

					Assembly A = AssemblyLoadContext.Default.LoadFromAssemblyName(AN);
					LoadedAssemblies[A.GetName().FullName] = A;

					foreach (AssemblyName AN2 in A.GetReferencedAssemblies())
						ReferencedAssemblies[AN2.FullName] = AN2;
				}
			}
			while (ReferencedAssemblies.Count > 0);

			Assembly[] Assemblies = new Assembly[LoadedAssemblies.Count];
			LoadedAssemblies.Values.CopyTo(Assemblies, 0);

			Types.Initialize(Assemblies);
		}
	}
}
