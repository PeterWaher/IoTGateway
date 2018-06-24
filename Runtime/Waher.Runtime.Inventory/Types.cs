using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using Waher.Events;

namespace Waher.Runtime.Inventory
{
	/// <summary>
	/// Static class that dynamically manages types and interfaces available in the runtime environment.
	/// </summary>
	public static class Types
	{
		private static SortedDictionary<string, Type> types = new SortedDictionary<string, Type>();
		private static SortedDictionary<string, SortedDictionary<string, Type>> typesPerInterface = new SortedDictionary<string, SortedDictionary<string, Type>>();
		private static SortedDictionary<string, SortedDictionary<string, Type>> typesPerNamespace = new SortedDictionary<string, SortedDictionary<string, Type>>();
		private static SortedDictionary<string, SortedDictionary<string, bool>> namespacesPerNamespace = new SortedDictionary<string, SortedDictionary<string, bool>>();
		private static SortedDictionary<string, bool> rootNamespaces = new SortedDictionary<string, bool>();
		private static Assembly[] assemblies = null;
		private static IModule[] modules = null;
		private static WaitHandle[] startWaitHandles = null;
		private static readonly Type[] noTypes = new Type[0];
		private static readonly object[] noParameters = new object[0];
		private static readonly object synchObject = new object();
		private static bool isInitialized = false;

		static Types()
		{
			Log.Terminating += OnProcessExit;
		}

		/// <summary>
		/// Gets a type, given its full name.
		/// </summary>
		/// <param name="FullName">Full name of type.</param>
		/// <returns>Type, if found, null otherwise.</returns>
		public static Type GetType(string FullName)
		{
			lock (synchObject)
			{
				if (!isInitialized)
					throw NotInitializedException();

				if (types.TryGetValue(FullName, out Type Result))
					return Result;
				else
					return null;
			}
		}

		private static Exception NotInitializedException()
		{
			return new Exception("Inventory engine not initialized properly. Make sure to call Waher.Runtime.Inventory.Types.Initialize() or Waher.Runtime.Inventory.Loader.TypesLoader.Initialize() first.");
		}

		/// <summary>
		/// Gets all types implementing a given interface.
		/// </summary>
		/// <param name="InterfaceFullName">Full name of interface.</param>
		/// <returns>Types implementing the interface.</returns>
		public static Type[] GetTypesImplementingInterface(string InterfaceFullName)
		{
			Type[] Result;

			lock (synchObject)
			{
				if (!isInitialized)
					throw NotInitializedException();

				if (!typesPerInterface.TryGetValue(InterfaceFullName, out SortedDictionary<string, Type> Types))
					return new Type[0];

				Result = new Type[Types.Count];
				Types.Values.CopyTo(Result, 0);
			}

			return Result;
		}

		/// <summary>
		/// Gets all types implementing a given interface.
		/// </summary>
		/// <param name="Interface">Interface</param>
		/// <returns>Types implementing the interface.</returns>
		public static Type[] GetTypesImplementingInterface(Type Interface)
		{
			return GetTypesImplementingInterface(Interface.FullName);
		}

		/// <summary>
		/// Gets all types in a namespace. (Types in sub-namespaces are not included.)
		/// </summary>
		/// <param name="Namespace">Namespace.</param>
		/// <returns>Types in the namespace.</returns>
		public static Type[] GetTypesInNamespace(string Namespace)
		{
			Type[] Result;

			lock (synchObject)
			{
				if (!isInitialized)
					throw NotInitializedException();

				if (!typesPerNamespace.TryGetValue(Namespace, out SortedDictionary<string, Type> Types))
					return new Type[0];

				Result = new Type[Types.Count];
				Types.Values.CopyTo(Result, 0);
			}

			return Result;
		}

		/// <summary>
		/// Gets the assembly reference of the first type found in a namespace.
		/// </summary>
		/// <param name="Namespace">Namespace.</param>
		/// <returns>Assembly reference of first type found in a namespace. If no such type was found, null is returned.</returns>
		public static Assembly GetFirstAssemblyReferenceInNamespace(string Namespace)
		{
			lock (synchObject)
			{
				if (!isInitialized)
					throw NotInitializedException();

				if (!typesPerNamespace.TryGetValue(Namespace, out SortedDictionary<string, Type> Types))
					return null;

				foreach (Type T in Types.Values)
					return T.GetTypeInfo().Assembly;
			}

			return null;
		}

		/// <summary>
		/// Gets an array of root namespaces.
		/// </summary>
		/// <returns>Array of root namespaces.</returns>
		public static string[] GetRootNamespaces()
		{
			string[] Result;

			lock (synchObject)
			{
				if (!isInitialized)
					throw NotInitializedException();

				Result = new string[rootNamespaces.Count];
				rootNamespaces.Keys.CopyTo(Result, 0);
			}

			return Result;
		}

		/// <summary>
		/// Checks if a name is a root namespace.
		/// </summary>
		/// <param name="Name">Name to check.</param>
		/// <returns>If the name is a root namespace.</returns>
		public static bool IsRootNamespace(string Name)
		{
			lock (synchObject)
			{
				return rootNamespaces.ContainsKey(Name);
			}
		}

		/// <summary>
		/// Gets an array of sub-namespaces to a given namespace.
		/// </summary>
		/// <param name="Namespace">Namespace</param>
		/// <returns>Array of sub-namespaces.</returns>
		public static string[] GetSubNamespaces(string Namespace)
		{
			string[] Result;

			lock (synchObject)
			{
				if (!isInitialized)
					throw NotInitializedException();

				if (!namespacesPerNamespace.TryGetValue(Namespace, out SortedDictionary<string, bool> Namespaces))
					return new string[0];

				Result = new string[Namespaces.Count];
				Namespaces.Keys.CopyTo(Result, 0);
			}

			return Result;
		}

		/// <summary>
		/// Checks if a local name in <paramref name="LocalName"/> represents a subnamespace from the point of view of the namespace
		/// in <paramref name="Namespace"/>.
		/// </summary>
		/// <param name="Namespace">Namespace.</param>
		/// <param name="LocalName">Local name.</param>
		/// <returns>If the local name represents a subnamespace.</returns>
		public static bool IsSubNamespace(string Namespace, string LocalName)
		{
			lock (synchObject)
			{
				if (!isInitialized)
					throw NotInitializedException();

				if (!namespacesPerNamespace.TryGetValue(Namespace, out SortedDictionary<string, bool> Namespaces))
					return false;

				return Namespaces.ContainsKey(Namespace + "." + LocalName);
			}
		}

		/// <summary>
		/// Invalidates type caches. This method should be called after having loaded assemblies dynamically, to make sure any types,
		/// interfaces and namespaces in the newly loaded assemblies are included.
		/// </summary>
		public static void Invalidate()
		{
			lock (synchObject)
			{
				isInitialized = false;

				types.Clear();
				typesPerInterface.Clear();
				typesPerNamespace.Clear();
				namespacesPerNamespace.Clear();
				rootNamespaces.Clear();
			}

			EventHandler h = OnInvalidated;
			if (h != null)
			{
				try
				{
					h(typeof(Types), new EventArgs());
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Event raised when the type cache has been invalidated. Can be used by code that themselves cache results and need to be updated
		/// after new types are available.
		/// </summary>
		public static event EventHandler OnInvalidated = null;

		private static void OnProcessExit(object Sender, EventArgs e)
		{
			if (isInitialized)
			{
				IModule[] Modules = Types.Modules;

				if (Modules != null)
				{
					foreach (IModule Module in Modules)
					{
						try
						{
							Module.Stop();
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}
					}
				}
			}
		}

		/// <summary>
		/// Loaded modules.
		/// </summary>
		public static IModule[] Modules
		{
			get
			{
				lock (synchObject)
				{
					if (!isInitialized)
						throw NotInitializedException();

					return modules;
				}
			}
		}

		/// <summary>
		/// Starts all loaded modules.
		/// </summary>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <returns>If all modules have been successfully started (true), or if at least one has not been
		/// started within the time period defined by <paramref name="Timeout"/>.</returns>
		public static bool StartAllModules(int Timeout)
		{
			List<WaitHandle> Handles = new List<WaitHandle>();
			List<IModule> Modules = new List<IModule>();
			IModule Module;
			WaitHandle Handle;
			TypeInfo TI;

			foreach (Type T in GetTypesImplementingInterface(typeof(IModule)))
			{
				TI = T.GetTypeInfo();
				if (TI.IsAbstract || TI.IsGenericTypeDefinition)
					continue;

				try
				{
					Log.Informational("Starting module.", T.FullName);

					Module = (IModule)Activator.CreateInstance(T);
					Handle = Module.Start();
					if (Handle != null)
						Handles.Add(Handle);

					Modules.Add(Module);
				}
				catch (Exception ex)
				{
					Log.Error("Unable to start module: " + ex.Message, T.FullName);
				}
			}

			startWaitHandles = Handles.ToArray();
			modules = Modules.ToArray();

			if (startWaitHandles.Length == 0)
				return true;

			return WaitHandle.WaitAll(startWaitHandles, Timeout);
		}

		/// <summary>
		/// Contains an empty array of types.
		/// </summary>
		public static Type[] NoTypes
		{
			get { return noTypes; }
		}

		/// <summary>
		/// Contains an empty array of parameter values.
		/// </summary>
		public static object[] NoParameters
		{
			get { return noParameters; }
		}

		/// <summary>
		/// Sets a module parameter. This parameter value will be accessible to modules when they are loaded.
		/// </summary>
		/// <param name="Name">Parameter name.</param>
		/// <param name="Value">Parameter value.</param>
		/// <exception cref="ArgumentException">If a module parameter with the same name is already defined.</exception>
		public static void SetModuleParameter(string Name, object Value)
		{
			lock (moduleParameters)
			{
				if (moduleParameters.ContainsKey(Name))
					throw new ArgumentException("Module parameter already defined: " + Name, nameof(Name));

				moduleParameters[Name] = Value;
			}
		}

		/// <summary>
		/// Tries to get a module parameter value.
		/// </summary>
		/// <param name="Name">Name of module parameter.</param>
		/// <param name="Value">Value of module parameter.</param>
		/// <returns>If a module parameter with the same name was found.</returns>
		public static bool TryGetModuleParameter(string Name, out object Value)
		{
			lock (moduleParameters)
			{
				return moduleParameters.TryGetValue(Name, out Value);
			}
		}

		private static Dictionary<string, object> moduleParameters = new Dictionary<string, object>();

		/// <summary>
		/// If the inventory has been initialized.
		/// </summary>
		public static bool IsInitialized
		{
			get { return isInitialized; }
		}

		/// <summary>
		/// Initializes the inventory engine, registering types and interfaces available in <paramref name="Assemblies"/>.
		/// </summary>
		public static void Initialize(params Assembly[] Assemblies)
		{
			SortedDictionary<string, Type> Types;
			SortedDictionary<string, Type> LastTypes = null;
			IEnumerable<Type> AssemblyTypes;
			Assembly A;
			string InterfaceName;
			string TypeName;
			string Namespace;
			string ParentNamespace;
			string LastNamespace = string.Empty;
			int i;

			if (isInitialized)
				throw new Exception("Script engine is already initialized.");

			CheckIncluded(ref Assemblies, typeof(Types).GetTypeInfo().Assembly);
			CheckIncluded(ref Assemblies, typeof(int).GetTypeInfo().Assembly);

			if (Array.IndexOf<Assembly>(Assemblies, A = typeof(Types).GetTypeInfo().Assembly) < 0)
			{
				int c = Assemblies.Length;
				Array.Resize<Assembly>(ref Assemblies, c + 1);
				Assemblies[c] = A;
			}

			assemblies = Assemblies;

			foreach (Assembly Assembly in Assemblies)
			{
				try
				{
					AssemblyTypes = Assembly.ExportedTypes;
				}
				catch (ReflectionTypeLoadException ex)
				{
					foreach (Exception ex2 in ex.LoaderExceptions)
						Log.Critical(ex2);

					continue;
				}
				catch (Exception ex)
				{
					Log.Critical(ex, Assembly.FullName);
					continue;
				}

				foreach (Type Type in AssemblyTypes)
				{
					TypeName = Type.FullName;
					i = TypeName.LastIndexOf('`');
					if (i > 0 && int.TryParse(TypeName.Substring(i + 1), out int j))
						TypeName = TypeName.Substring(0, i);

					types[TypeName] = Type;

					try
					{
						foreach (Type Interface in Type.GetTypeInfo().ImplementedInterfaces)
						{
							InterfaceName = Interface.FullName;
							if (InterfaceName == null)
								continue;   // Generic interface.

							if (!typesPerInterface.TryGetValue(InterfaceName, out Types))
							{
								Types = new SortedDictionary<string, Type>();
								typesPerInterface[InterfaceName] = Types;
							}

							Types[TypeName] = Type;
						}
					}
					catch (Exception)
					{
						// Implemented interfaces might not be accessible.
					}

					Namespace = Type.Namespace;
					if (Namespace != null)
					{
						if (Namespace == LastNamespace)
							Types = LastTypes;
						else
						{
							if (!typesPerNamespace.TryGetValue(Namespace, out Types))
							{
								Types = new SortedDictionary<string, Type>();
								typesPerNamespace[Namespace] = Types;

								i = Namespace.LastIndexOf('.');
								while (i > 0)
								{
									ParentNamespace = Namespace.Substring(0, i);

									if (!namespacesPerNamespace.TryGetValue(ParentNamespace, out SortedDictionary<string, bool> Namespaces))
									{
										Namespaces = new SortedDictionary<string, bool>();
										namespacesPerNamespace[ParentNamespace] = Namespaces;
									}
									else
									{
										if (Namespaces.ContainsKey(Namespace))
											break;
									}

									Namespaces[Namespace] = true;
									Namespace = ParentNamespace;
									i = Namespace.LastIndexOf('.');
								}

								if (i < 0)
									rootNamespaces[Namespace] = true;
							}

							LastNamespace = Namespace;
							LastTypes = Types;
						}

						Types[TypeName] = Type;
					}
				}
			}

			isInitialized = true;
		}

		private static void CheckIncluded(ref Assembly[] Assemblies, Assembly A)
		{
			if (Array.IndexOf<Assembly>(Assemblies, A) < 0)
			{
				int c = Assemblies.Length;
				Array.Resize<Assembly>(ref Assemblies, c + 1);
				Assemblies[c] = A;
			}
		}

		/// <summary>
		/// Assemblies in the inventory.
		/// </summary>
		public static Assembly[] Assemblies => assemblies;

	}
}
