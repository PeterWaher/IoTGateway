using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Waher.Events;
#if WINDOWS_UWP
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Storage;
#endif

namespace Waher.Script
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
		private static IModule[] modules = null;
		private static WaitHandle[] startWaitHandles = null;
		private static readonly Type[] noTypes = new Type[0];
		private static readonly object[] noParameters = new object[0];
		private static object synchObject = new object();
		private static bool memoryScanned = false;

#if WINDOWS_UWP
		/// <summary>
		/// Must be called in UWP application when the application is terminated. Stops all dynamic modules that have been loaded
		/// </summary>
		public static void Terminate()
		{
			OnProcessExit(null, new EventArgs());
		}
#else
		static Types()
		{
			AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
		}
#endif

		/// <summary>
		/// Gets a type, given its full name.
		/// </summary>
		/// <param name="FullName">Full name of type.</param>
		/// <returns>Type, if found, null otherwise.</returns>
		public static Type GetType(string FullName)
		{
			Type Result;

			lock (synchObject)
			{
				if (!memoryScanned)
					SearchTypesLocked();

				if (types.TryGetValue(FullName, out Result))
					return Result;
				else
					return null;
			}
		}

		/// <summary>
		/// Gets all types implementing a given interface.
		/// </summary>
		/// <param name="InterfaceFullName">Full name of interface.</param>
		/// <returns>Types implementing the interface.</returns>
		public static Type[] GetTypesImplementingInterface(string InterfaceFullName)
		{
			SortedDictionary<string, Type> Types;
			Type[] Result;

			lock (synchObject)
			{
				if (!memoryScanned)
					SearchTypesLocked();

				if (!typesPerInterface.TryGetValue(InterfaceFullName, out Types))
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
			SortedDictionary<string, Type> Types;
			Type[] Result;

			lock (synchObject)
			{
				if (!memoryScanned)
					SearchTypesLocked();

				if (!typesPerNamespace.TryGetValue(Namespace, out Types))
					return new Type[0];

				Result = new Type[Types.Count];
				Types.Values.CopyTo(Result, 0);
			}

			return Result;
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
				if (!memoryScanned)
					SearchTypesLocked();

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
			SortedDictionary<string, bool> Namespaces;
			string[] Result;

			lock (synchObject)
			{
				if (!memoryScanned)
					SearchTypesLocked();

				if (!namespacesPerNamespace.TryGetValue(Namespace, out Namespaces))
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
			SortedDictionary<string, bool> Namespaces;

			lock (synchObject)
			{
				if (!memoryScanned)
					SearchTypesLocked();

				if (!namespacesPerNamespace.TryGetValue(Namespace, out Namespaces))
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
				memoryScanned = false;

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

		private static void SearchTypesLocked()
		{
			SortedDictionary<string, Type> Types;
			SortedDictionary<string, bool> Namespaces;
			SortedDictionary<string, Type> LastTypes = null;
			Type[] AssemblyTypes;
			string InterfaceName;
			string TypeName;
			string Namespace;
			string ParentNamespace;
			string LastNamespace = string.Empty;
			int i, j;

#if WINDOWS_UWP
			ManualResetEvent Done = new ManualResetEvent(false);
			IAsyncOperation<IReadOnlyList<StorageFile>> FilesAO = Package.Current.InstalledLocation.GetFilesAsync();

			FilesAO.Completed += (info, status) => Done.Set();
			if (FilesAO.Status == AsyncStatus.Started)
				Done.WaitOne(5000);

			Done.Dispose();
			IReadOnlyList<StorageFile> Files = FilesAO.GetResults();
			List<string> DllFiles = new List<string>();
			Dictionary<AssemblyName, Assembly> LoadedAssemblies = new Dictionary<AssemblyName, Assembly>();
			string s;

			foreach (StorageFile File in Files)
			{
				s = File.FileType.ToLower();
				if (s == ".dll" || s == ".exe")
					DllFiles.Add(File.Name);

				try
				{
					AssemblyName AN = new AssemblyName(File.DisplayName);
					Assembly A = Assembly.Load(AN);
					LoadedAssemblies[AN] = A;
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

			{
				foreach (Assembly Assembly in LoadedAssemblies.Values)
				{
#else
			string BinaryFolder = AppDomain.CurrentDomain.BaseDirectory;
			string[] DllFiles = Directory.GetFiles(BinaryFolder, "*.dll", SearchOption.TopDirectoryOnly);
			Dictionary<string, Assembly> LoadedAssemblies = new Dictionary<string, Assembly>(StringComparer.CurrentCultureIgnoreCase);
			Dictionary<string, Assembly> AssembliesToLoad = new Dictionary<string, Assembly>(StringComparer.CurrentCultureIgnoreCase);

			foreach (string DllFile in DllFiles)
			{
				if (AssembliesToLoad.ContainsKey(DllFile))
					continue;

				try
				{
					Assembly A = Assembly.LoadFrom(DllFile);
					AssembliesToLoad[DllFile] = A;
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

			while (true)
			{
				foreach (Assembly Assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					if (LoadedAssemblies.ContainsKey(Assembly.Location) || Assembly.IsDynamic)
						continue;

					AssembliesToLoad[Assembly.Location] = Assembly;
				}

				foreach (Assembly Assembly in AssembliesToLoad.Values)
				{
					LoadedAssemblies[Assembly.Location] = Assembly;
#endif

					try
					{
						AssemblyTypes = Assembly.GetTypes();
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
						if (i > 0 && int.TryParse(TypeName.Substring(i + 1), out j))
							TypeName = TypeName.Substring(0, i);

						types[TypeName] = Type;

						foreach (Type Interface in Type.GetInterfaces())
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

										if (!namespacesPerNamespace.TryGetValue(ParentNamespace, out Namespaces))
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

#if !WINDOWS_UWP
				if (AssembliesToLoad.Count == 0)
					break;

				AssembliesToLoad.Clear();
#endif
			}

			memoryScanned = true;
			modules = Modules.ToArray();
		}

		private static void OnProcessExit(object Sender, EventArgs e)
		{
			if (memoryScanned)
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

		/// <summary>
		/// Loaded modules.
		/// </summary>
		public static IModule[] Modules
		{
			get
			{
				lock (synchObject)
				{
					if (!memoryScanned)
						SearchTypesLocked();

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
			ConstructorInfo CI;
			IModule Module;
			WaitHandle Handle;

			foreach (Type T in GetTypesImplementingInterface(typeof(IModule)))
			{
#if WINDOWS_UWP
				if (T.GetTypeInfo().IsAbstract)
#else
				if (T.IsAbstract)
#endif
					continue;

				try
				{
					CI = T.GetConstructor(noTypes);
					if (CI == null)
						continue;

					Module = (IModule)CI.Invoke(noParameters);
					Handle = Module.Start();
					if (Handle != null)
						Handles.Add(Handle);

					Modules.Add(Module);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

			startWaitHandles = Handles.ToArray();

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
					throw new ArgumentException("Module parameter already defined: " + Name, "Name");

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
		/// Loads any modules, if not already loaded.
		/// </summary>
		public static void LoadModules()
		{
			if (!memoryScanned)
				SearchTypesLocked();
		}

	}
}
