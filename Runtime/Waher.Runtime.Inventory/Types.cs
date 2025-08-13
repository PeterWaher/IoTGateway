﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.Collections;

namespace Waher.Runtime.Inventory
{
	/// <summary>
	/// Static class that dynamically manages types and interfaces available in the runtime environment.
	/// </summary>
	public static class Types
	{
		private static readonly SortedDictionary<string, Type> types = new SortedDictionary<string, Type>();
		private static readonly SortedDictionary<string, SortedDictionary<string, Type>> typesPerInterface = new SortedDictionary<string, SortedDictionary<string, Type>>();
		private static readonly SortedDictionary<string, SortedDictionary<string, Type>> typesPerNamespace = new SortedDictionary<string, SortedDictionary<string, Type>>();
		private static readonly SortedDictionary<string, SortedDictionary<string, bool>> namespacesPerNamespace = new SortedDictionary<string, SortedDictionary<string, bool>>();
		private static readonly SortedDictionary<string, bool> rootNamespaces = new SortedDictionary<string, bool>();
		private static readonly SortedDictionary<string, object> qualifiedNames = new SortedDictionary<string, object>();
		private static readonly Dictionary<string, object> moduleParameters = new Dictionary<string, object>();
		private static readonly Dictionary<string, MethodInfo> tryParseMethods = new Dictionary<string, MethodInfo>();
		private static readonly Dictionary<Type, ConstructorInfo> defaultConstructors = new Dictionary<Type, ConstructorInfo>();
		private static Assembly[] assemblies = null;
		private static IModule[] modules = null;
		private static readonly Type[] noTypes = Array.Empty<Type>();
		private static readonly object[] noParameters = Array.Empty<object>();
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
			}

			if (FullName.EndsWith("[]"))
			{
				Type ElementType = GetType(FullName.Substring(0, FullName.Length - 2));
				if (ElementType is null)
					return null;

				return ElementType.MakeArrayType();
			}

			return null;
		}

		/// <summary>
		/// Checks if <paramref name="FullName"/> references a type in the inventory.
		/// </summary>
		/// <param name="FullName">Full name</param>
		/// <returns>If <paramref name="FullName"/> references a type.</returns>
		public static bool IsType(string FullName)
		{
			return !(GetType(FullName) is null);
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
					return Array.Empty<Type>();

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
					return Array.Empty<Type>();

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
					return T.Assembly;
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
					return Array.Empty<string>();

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
		/// Gets an array (possibly null) of qualified names relating to an unqualified name.
		/// </summary>
		/// <param name="UnqualifiedName">Unqualified name.</param>
		/// <param name="QualifiedNames">Array of qualified names (null if none)</param>
		/// <returns>If the unqualified name was recognized.</returns>
		public static bool TryGetQualifiedNames(string UnqualifiedName, out string[] QualifiedNames)
		{
			lock (synchObject)
			{
				if (!qualifiedNames.TryGetValue(UnqualifiedName, out object Obj))
				{
					QualifiedNames = null;
					return false;
				}

				if (Obj is string[] A)
				{
					QualifiedNames = A;
					return true;
				}

				if (Obj is string s)
				{
					QualifiedNames = new string[] { s };
					qualifiedNames[UnqualifiedName] = QualifiedNames;
					return true;
				}

				if (Obj is SortedDictionary<string, bool> Sorted)
				{
					QualifiedNames = new string[Sorted.Count];
					Sorted.Keys.CopyTo(QualifiedNames, 0);
					qualifiedNames[UnqualifiedName] = QualifiedNames;
					return true;
				}

				QualifiedNames = new string[] { Obj.ToString() };
				qualifiedNames[UnqualifiedName] = QualifiedNames;
				return true;
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
				qualifiedNames.Clear();
				tryParseMethods.Clear();
			}

			EventHandler h = OnInvalidated;
			if (!(h is null))
			{
				try
				{
					h(typeof(Types), EventArgs.Empty);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}
		}

		/// <summary>
		/// Event raised when the type cache has been invalidated. Can be used by code that themselves cache results and need to be updated
		/// after new types are available.
		/// </summary>
		public static event EventHandler OnInvalidated = null;

		private static Task OnProcessExit(object Sender, EventArgs e)
		{
			return StopAllModules();
		}

		/// <summary>
		/// Stops all modules.
		/// </summary>
		public static Task StopAllModules()
		{
			return StopAllModules(null);
		}

		/// <summary>
		/// Stops all modules.
		/// </summary>
		/// <param name="Order">Order in which modules should be stopped.
		/// Default order is the reverse starting order, if no other order is provided.</param>
		public static async Task StopAllModules(IComparer<IModule> Order)
		{
			if (isInitialized)
			{
				IModule[] Modules = (IModule[])Types.Modules?.Clone();

				if (!(Modules is null))
				{
					if (Order is null)
						Array.Reverse(Modules);
					else
						Array.Sort(Modules, Order);

					foreach (IModule Module in Modules)
					{
						if (Module is IGracefulModule GracefulModule)
						{
							try
							{
								await GracefulModule.PrepareStop();
							}
							catch (Exception ex)
							{
								Log.Exception(ex);
							}
						}
					}

					foreach (IModule Module in Modules)
					{
						try
						{
							await Module.Stop();
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
						}
					}

					modules = Array.Empty<IModule>();
				}

				lock (moduleParameters)
				{
					moduleParameters.Clear();
				}

				await SingletonAttribute.Clear();
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
		/// Gets an array of loaded modules.
		/// </summary>
		/// <returns>Array of loaded modules.</returns>
		public static IModule[] GetLoadedModules()
		{
			return GetLoadedModules(null);
		}

		/// <summary>
		/// Gets an array of loaded modules.
		/// </summary>
		/// <param name="Order">Optional sort order of modules.</param>
		/// <returns>Array of loaded modules.</returns>
		public static IModule[] GetLoadedModules(IComparer<IModule> Order)
		{
			ChunkedList<IModule> Modules = new ChunkedList<IModule>();
			IModule Module;
			TypeInfo TI;

			foreach (Type T in GetTypesImplementingInterface(typeof(IModule)))
			{
				TI = T.GetTypeInfo();
				if (TI.IsAbstract || TI.IsInterface || TI.IsGenericTypeDefinition)
					continue;

				try
				{
					Module = (IModule)Instantiate(T);
					Modules.Add(Module);
				}
				catch (Exception ex)
				{
					Log.Error("Unable to start module: " + ex.Message, T.FullName);
				}
			}

			if (Order is null)
				Order = new DependencyOrder();

			Modules.Sort(Order);

			return Modules.ToArray();
		}

		/// <summary>
		/// Starts all loaded modules.
		/// </summary>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <returns>If all modules have been successfully started (true), or if at least one has not been
		/// started within the time period defined by <paramref name="Timeout"/>.</returns>
		public static Task<bool> StartAllModules(int Timeout)
		{
			return StartAllModules(Timeout, null);
		}

		/// <summary>
		/// Starts all loaded modules.
		/// </summary>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <param name="Order">Order in which modules should be started.</param>
		/// <returns>If all modules have been successfully started (true), or if at least one has not been
		/// started within the time period defined by <paramref name="Timeout"/>.</returns>
		public static Task<bool> StartAllModules(int Timeout, IComparer<IModule> Order)
		{
			return StartAllModules(Timeout, Order, null, null);
		}

		/// <summary>
		/// Starts all loaded modules.
		/// </summary>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <param name="Order">Order in which modules should be started.</param>
		/// <param name="LoadedModules">Optional list that will be filled with all
		/// modules that were successfully loaded.</param>
		/// <param name="FailedModules">Optional list that will be filled with all
		/// modules that failed to load.</param>
		/// <returns>If all modules have been successfully started (true), or if at least one has not been
		/// started within the time period defined by <paramref name="Timeout"/>.</returns>
		public static async Task<bool> StartAllModules(int Timeout, IComparer<IModule> Order,
			ChunkedList<IModule> LoadedModules, ChunkedList<IModule> FailedModules)
		{
			if (modules is null || modules.Length == 0)
			{
				IModule[] Modules = GetLoadedModules(Order);
				bool Ok = true;

				foreach (IModule Module2 in Modules)
				{
					try
					{
						if (await StartModule(Module2, Timeout))       // 1 min timeout
							LoadedModules?.Add(Module2);
						else
						{
							Ok = false;
							FailedModules?.Add(Module2);
						}
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
						Ok = false;
						FailedModules?.Add(Module2);
					}
				}

				modules = Modules;

				return Ok;
			}
			else
				return true;
		}

		private static async Task<bool> StartModule(IModule Module, int TimeoutMilliseconds)
		{
			Type T = Module.GetType();

			try
			{
				Log.Informational("Starting module.", T.FullName);

				Task<int> TimeoutTask = Timeout(TimeoutMilliseconds);
				Task<int> StartTask = StartModule2(Module);
				Task<int> First = await Task.WhenAny<int>(StartTask, TimeoutTask);

				if (await First != 0)
				{
					Log.Warning("Starting module takes too long time. Startup continues in the background.", T.FullName);
					return false;
				}
				else
				{
					Log.Informational("Module started.", T.FullName);
					return true;
				}
			}
			catch (Exception ex)
			{
				Log.Error("Unable to start module: " + ex.Message, T.FullName);
				return false;
			}
		}

		private static async Task<int> StartModule2(IModule Module)
		{
			await Module.Start();
			return 0;
		}

		private static async Task<int> Timeout(int Milliseconds)
		{
			await Task.Delay(Milliseconds);
			return 1;
		}

		/// <summary>
		/// Contains an empty array of types.
		/// </summary>
		public static Type[] NoTypes => noTypes;

		/// <summary>
		/// Contains an empty array of parameter values.
		/// </summary>
		public static object[] NoParameters => noParameters;

		/// <summary>
		/// Sets a module parameter. This parameter value will be accessible to modules when they are loaded.
		/// </summary>
		/// <param name="Name">Parameter name.</param>
		/// <param name="Value">Parameter value.</param>
		/// <exception cref="ArgumentException">If a module parameter with the same name is already defined.</exception>
		/// <remarks>A module parameter that is disposed will still be available, but it can be reset by
		/// another module parameter instance using the same name. The method to check if a parameter is
		/// disposed, is if the object instance has a public property named "Disposeed" returning a bool
		/// value indicating if the object has been disposed or not.</remarks>
		public static void SetModuleParameter(string Name, object Value)
		{
			lock (moduleParameters)
			{
				if (moduleParameters.TryGetValue(Name, out object Value2) && !Value.Equals(Value2))
				{
					Type T = Value2.GetType();
					PropertyInfo PI = T.GetRuntimeProperty("Disposed");

					if (PI is null || PI.PropertyType != typeof(bool) || !(bool)PI.GetValue(Value2))
						throw new ArgumentException("Module parameter already defined: " + Name, nameof(Name));
				}

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

		/// <summary>
		/// Tries to get a typed module parameter value.
		/// </summary>
		/// <param name="Name">Name of module parameter.</param>
		/// <param name="Value">Value of module parameter.</param>
		/// <returns>If a module parameter with the same name was found.</returns>
		public static bool TryGetModuleParameter<T>(string Name, out T Value)
		{
			lock (moduleParameters)
			{
				if (moduleParameters.TryGetValue(Name, out object Obj) && Obj is T Typed)
				{
					Value = Typed;
					return true;
				}
				else
				{
					Value = default;
					return false;
				}
			}
		}

		/// <summary>
		/// Tries to get a typed module parameter value.
		/// </summary>
		/// <param name="Name">Name of module parameter.</param>
		/// <returns>Typed value of module parameter, if found, null otherwise.</returns>
		public static T TryGetModuleParameter<T>(string Name)
			where T : class
		{
			lock (moduleParameters)
			{
				if (moduleParameters.TryGetValue(Name, out object Value) && Value is T TypedValue)
					return TypedValue;
				else
					return null;
			}
		}

		/// <summary>
		/// If the inventory has been initialized.
		/// </summary>
		public static bool IsInitialized => isInitialized;

		/// <summary>
		/// Initializes the inventory engine, registering types and interfaces available in <paramref name="Assemblies"/>.
		/// </summary>
		public static void Initialize(params Assembly[] Assemblies)
		{
			SortedDictionary<string, Type> Types;
			SortedDictionary<string, Type> LastTypes = null;
			Dictionary<string, Type> TypeNameAliases = null;
			Dictionary<string, Assembly> NamespaceAliases = null;
			IEnumerable<Type> AssemblyTypes;
			Assembly A;
			string InterfaceName;
			string TypeName;
			string Namespace;
			string ParentNamespace;
			string LastNamespace = string.Empty;
			int i;

			lock (synchObject)
			{
				if (isInitialized)
				{
					ChunkedList<Assembly> NewAssemblies = new ChunkedList<Assembly>();
					ChunkedList<Assembly> AllAssemblies = new ChunkedList<Assembly>();

					AllAssemblies.AddRange(assemblies);

					foreach (Assembly Assembly in Assemblies)
					{
						if (!AllAssemblies.Contains(Assembly))
						{
							NewAssemblies.Add(Assembly);
							AllAssemblies.Add(Assembly);
						}
					}

					assemblies = AllAssemblies.ToArray();
					Assemblies = NewAssemblies.ToArray();
				}
				else
				{
					CheckIncluded(ref Assemblies, typeof(Types).Assembly);
					CheckIncluded(ref Assemblies, typeof(int).Assembly);

					if (Array.IndexOf(Assemblies, A = typeof(Types).Assembly) < 0)
					{
						int c = Assemblies.Length;
						Array.Resize(ref Assemblies, c + 1);
						Assemblies[c] = A;
					}

					assemblies = Assemblies;
				}

				foreach (Assembly Assembly in Assemblies)
				{
					foreach (NamespaceAliasAttribute Alias in Assembly.GetCustomAttributes<NamespaceAliasAttribute>())
					{
						if (NamespaceAliases is null)
							NamespaceAliases = new Dictionary<string, Assembly>();

						if (NamespaceAliases.ContainsKey(Alias.Namespace))
							Log.Error("Namespace alias already registered.", Alias.Namespace, Assembly.FullName);
						else
							NamespaceAliases[Alias.Namespace] = Assembly;
					}

					try
					{
						AssemblyTypes = Assembly.ExportedTypes;
					}
					catch (ReflectionTypeLoadException ex)
					{
						foreach (Exception ex2 in ex.LoaderExceptions)
							Log.Exception(ex2);

						continue;
					}
					catch (Exception ex)
					{
						Log.Exception(ex, Assembly.FullName);
						continue;
					}

					foreach (Type Type in AssemblyTypes)
					{
						TypeName = Type.FullName;
						i = TypeName.LastIndexOf('`');
						if (i > 0 && int.TryParse(TypeName.Substring(i + 1), out int j))
							TypeName = TypeName.Substring(0, i);

						types[TypeName] = Type;

						i = TypeName.LastIndexOf('.');
						if (i >= 0)
							RegisterQualifiedName(TypeName.Substring(i + 1), TypeName);

						try
						{
							TypeInfo TI = Type.GetTypeInfo();

							foreach (Type Interface in TI.ImplementedInterfaces)
							{
								InterfaceName = Interface.FullName;
								if (InterfaceName is null)
									continue;   // Generic interface.

								if (!typesPerInterface.TryGetValue(InterfaceName, out Types))
								{
									Types = new SortedDictionary<string, Type>();
									typesPerInterface[InterfaceName] = Types;
								}

								Types[TypeName] = Type;
							}

							foreach (TypeAliasAttribute Alias in TI.GetCustomAttributes<TypeAliasAttribute>(false))
							{
								if (TypeNameAliases is null)
									TypeNameAliases = new Dictionary<string, Type>();

								if (TypeNameAliases.ContainsKey(Alias.TypeName))
									Log.Error("Type alias already registered.", Alias.TypeName, Type.FullName);
								else
									TypeNameAliases[Alias.TypeName] = Type;
							}
						}
						catch (Exception)
						{
							// Implemented interfaces might not be accessible.
						}

						Namespace = Type.Namespace;
						if (!(Namespace is null))
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
									while (i >= 0)
									{
										RegisterQualifiedName(Namespace.Substring(i + 1), Namespace);
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
									{
										rootNamespaces[Namespace] = true;
										RegisterQualifiedName(Namespace, Namespace);
									}
								}

								LastNamespace = Namespace;
								LastTypes = Types;
							}

							Types[TypeName] = Type;
						}
					}
				}

				if (!(TypeNameAliases is null))
				{
					foreach (KeyValuePair<string, Type> P in TypeNameAliases)
					{
						if (types.TryGetValue(P.Key, out Type T))
							Log.Error("Type alias conflicts with registered type.", P.Key, T.FullName);
						else
							types[P.Key] = P.Value;
					}
				}

				if (!(NamespaceAliases is null))
				{
					foreach (KeyValuePair<string, Assembly> P in NamespaceAliases)
					{
						ChunkedList<KeyValuePair<string, string>> CheckNamespaces = new ChunkedList<KeyValuePair<string, string>>
						{
							new KeyValuePair<string, string>(P.Key, P.Value.GetName().Name)
						};

						while (CheckNamespaces.HasFirstItem)
						{
							KeyValuePair<string, string> P2 = CheckNamespaces.RemoveFirst();
							string MapToNamespace = P2.Value;
							string Alias = P2.Key;

							if (typesPerNamespace.TryGetValue(MapToNamespace, out Types))
							{
								foreach (Type T in Types.Values)
								{
									string TypeAlias = T.FullName.Replace(MapToNamespace, Alias);

									if (types.TryGetValue(TypeAlias, out Type T2))
										Log.Error("Type alias conflicts with registered type.", TypeAlias, T2.FullName);
									else
										types[TypeAlias] = T;
								}
							}

							if (namespacesPerNamespace.TryGetValue(MapToNamespace, out SortedDictionary<string, bool> Namespaces))
							{
								foreach (string Subnamespace in Namespaces.Keys)
								{
									i = Subnamespace.LastIndexOf('.');
									if (i >= 0)
										CheckNamespaces.Add(new KeyValuePair<string, string>(Alias + Subnamespace.Substring(i), Subnamespace));
								}
							}
						}
					}
				}

				isInitialized = true;
			}
		}

		private static void RegisterQualifiedName(string UnqualifiedName, string QualifiedName)
		{
			if (qualifiedNames.TryGetValue(UnqualifiedName, out object Obj))
			{
				if (!(Obj is SortedDictionary<string, bool> Qualified))
				{
					if (Obj is string s)
					{
						Qualified = new SortedDictionary<string, bool>()
						{
							{ s, true }
						};
					}
					else if (Obj is string[] A)
					{
						Qualified = new SortedDictionary<string, bool>();

						foreach (string s2 in A)
							Qualified[s2] = true;
					}
					else
					{
						Qualified = new SortedDictionary<string, bool>()
						{
							{ Obj.ToString(), true }
						};
					}

					qualifiedNames[UnqualifiedName] = Qualified;
				}

				Qualified[QualifiedName] = true;
			}
			else
				qualifiedNames[UnqualifiedName] = QualifiedName;
		}

		private static void CheckIncluded(ref Assembly[] Assemblies, Assembly A)
		{
			if (Array.IndexOf(Assemblies, A) < 0)
			{
				int c = Assemblies.Length;
				Array.Resize(ref Assemblies, c + 1);
				Assemblies[c] = A;
			}
		}

		/// <summary>
		/// Assemblies in the inventory.
		/// </summary>
		public static Assembly[] Assemblies => assemblies;

		/// <summary>
		/// Creates an object of a given type, given its full name.
		/// </summary>
		/// <param name="TypeName">Full type name.</param>
		/// <param name="Parameters">Parameters to pass on to the constructor.</param>
		/// <returns>Created object.</returns>
		/// <exception cref="ArgumentException">If no type with the given name exists.</exception>
		public static object CreateObject(string TypeName, params object[] Parameters)
		{
			Type T = GetType(TypeName)
				?? throw new ArgumentException("Type not loaded: " + TypeName, nameof(TypeName));

			return Activator.CreateInstance(T, Parameters);
		}

		/// <summary>
		/// Gets a property value (or field value) from an object.
		/// </summary>
		/// <param name="Object">Object instance.</param>
		/// <param name="PropertyName">Name of property (or field).</param>
		/// <returns>Property (or field) value.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="Object"/> is null.</exception>
		/// <exception cref="ArgumentException">If there is no property or field with the given name.</exception>
		public static object GetProperty(object Object, string PropertyName)
		{
			if (Object is null)
				throw new ArgumentNullException(nameof(Object));

			Type T = Object.GetType();
			PropertyInfo PI = T.GetRuntimeProperty(PropertyName);
			if (!(PI is null))
			{
				if (PI.CanRead && PI.GetMethod.IsPublic)
					return PI.GetValue(Object);
				else
					throw new ArgumentException("Property not readable or accessible.", nameof(PropertyName));
			}

			FieldInfo FI = T.GetRuntimeField(PropertyName);
			if (!(FI is null))
			{
				if (FI.IsPublic)
					return FI.GetValue(Object);
				else
					throw new ArgumentException("Field not accessible.", nameof(PropertyName));
			}

			throw new ArgumentException("Property (or field) not found.", nameof(PropertyName));
		}

		/// <summary>
		/// Sets a property value (or field value) in an object.
		/// </summary>
		/// <param name="Object">Object instance.</param>
		/// <param name="PropertyName">Name of property (or field).</param>
		/// <param name="Value">Value to set.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="Object"/> is null.</exception>
		/// <exception cref="ArgumentException">If there is no property or field with the given name.</exception>
		public static void SetProperty(object Object, string PropertyName, object Value)
		{
			if (Object is null)
				throw new ArgumentNullException(nameof(Object));

			Type T = Object.GetType();
			PropertyInfo PI = T.GetRuntimeProperty(PropertyName);
			if (!(PI is null))
			{
				if (PI.CanWrite && PI.SetMethod.IsPublic)
					PI.SetValue(Object, Value);
				else
					throw new ArgumentException("Property not writable or accessible.", nameof(PropertyName));
			}
			else
			{
				FieldInfo FI = T.GetRuntimeField(PropertyName);
				if (!(FI is null))
				{
					if (FI.IsPublic)
						FI.SetValue(Object, Value);
					else
						throw new ArgumentException("Field not accessible.", nameof(PropertyName));
				}
				else
					throw new ArgumentException("Property (or field) not found.", nameof(PropertyName));
			}
		}

		/// <summary>
		/// Calls a method on an object.
		/// </summary>
		/// <param name="Object">Object instance.</param>
		/// <param name="MethodName">Name of method.</param>
		/// <param name="Arguments">Arguments to pass on to method.</param>
		/// <returns>Result</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="Object"/> is null.</exception>
		/// <exception cref="ArgumentException">If there is no method with the given name and argument types.</exception>
		public static object Call(object Object, string MethodName, params object[] Arguments)
		{
			if (Object is null)
				throw new ArgumentNullException(nameof(Object));

			Type T = Object.GetType();

			return Call(Object, T, MethodName, Arguments);
		}

		private static object Call(object Object, Type T, string MethodName, params object[] Arguments)
		{
			int i, c = Arguments.Length;
			Type[] ArgumentTypes = new Type[c];
			bool HasNull = false;
			object Arg;

			for (i = 0; i < c; i++)
			{
				Arg = Arguments[i];
				if (Arg is null)
				{
					HasNull = true;
					break;
				}
				else
					ArgumentTypes[i] = Arg.GetType();
			}

			if (HasNull)
			{
				foreach (MethodInfo MI in T.GetRuntimeMethods())
				{
					if (!MI.IsPublic)
						continue;

					ParameterInfo[] P = MI.GetParameters();
					if (P.Length != c)
						continue;

					bool Found = true;

					for (i = 0; i < c; i++)
					{
						Arg = Arguments[i];

						if (!(Arg is null) && !P[i].ParameterType.IsAssignableFrom(Arg.GetType().GetTypeInfo()))
						{
							Found = false;
							break;
						}
					}

					if (Found)
						return MI.Invoke(Object, Arguments);
				}
			}
			else
			{
				MethodInfo MI = T.GetRuntimeMethod(MethodName, ArgumentTypes);
				if (!(MI is null))
				{
					if (MI.IsPublic)
						return MI.Invoke(Object, Arguments);
					else
						throw new ArgumentException("Method not accessible.", nameof(MethodName));
				}
			}

			throw new ArgumentException("Method with corresponding arguments not found.", nameof(MethodName));
		}

		/// <summary>
		/// Calls a static method on a class.
		/// </summary>
		/// <param name="TypeName">Name of class (or type).</param>
		/// <param name="MethodName">Name of method.</param>
		/// <param name="Arguments">Arguments to pass on to method.</param>
		/// <returns>Result</returns>
		/// <exception cref="ArgumentException">If there is no method with the given name and argument types.</exception>
		public static object CallStatic(string TypeName, string MethodName, params object[] Arguments)
		{
			Type T = GetType(TypeName);
			return Call(null, T, MethodName, Arguments);
		}

		/// <summary>
		/// Finds the best interface for a certain task.
		/// </summary>
		/// <typeparam name="InterfaceType">Check interfaces of this type.</typeparam>
		/// <typeparam name="ObjectType">Return interfaces supporting processing of this type 
		/// (i.e. implementing <see cref="IProcessingSupport{ObjectType}"/>).</typeparam>
		/// <param name="Object">Object with features to process.</param>
		/// <returns>Best interface, if found, null otherwise.</returns>
		public static InterfaceType FindBest<InterfaceType, ObjectType>(ObjectType Object)
			where InterfaceType : IProcessingSupport<ObjectType>
		{
			return FindBest<InterfaceType, ObjectType>(Object, GetTypesImplementingInterface(typeof(InterfaceType)));
		}

		/// <summary>
		/// Finds the best interface for a certain task.
		/// </summary>
		/// <typeparam name="InterfaceType">Check interfaces of this type.</typeparam>
		/// <typeparam name="ObjectType">Return interfaces supporting processing of this type 
		/// (i.e. implementing <see cref="IProcessingSupport{ObjectType}"/>).</typeparam>
		/// <param name="Object">Object with features to process.</param>
		/// <param name="Interfaces">Array of types (of <typeparamref name="InterfaceType"/>) to search.</param>
		/// <returns>Best interface, if found, null otherwise.</returns>
		public static InterfaceType FindBest<InterfaceType, ObjectType>(ObjectType Object, Type[] Interfaces)
			where InterfaceType : IProcessingSupport<ObjectType>
		{
			InterfaceType Best = default;
			Grade BestGrade = Grade.NotAtAll;
			TypeInfo TI;

			foreach (Type T2 in Interfaces)
			{
				TI = T2.GetTypeInfo();
				if (TI.IsAbstract || TI.IsInterface || TI.IsGenericTypeDefinition)
					continue;

				try
				{
					InterfaceType Candidate = (InterfaceType)Instantiate(T2);
					Grade Grade = Candidate.Supports(Object);
					if (Grade > BestGrade)
					{
						if (Grade == Grade.Perfect)
							return Candidate;

						Best = Candidate;
						BestGrade = Grade;
					}
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}

			return Best;
		}

		/// <summary>
		/// Finds interfaces that support a a certain task, ordered by reverse order of support.
		/// </summary>
		/// <typeparam name="InterfaceType">Check interfaces of this type.</typeparam>
		/// <typeparam name="ObjectType">Return interfaces supporting processing of this type 
		/// (i.e. implementing <see cref="IProcessingSupport{ObjectType}"/>).</typeparam>
		/// <param name="Object">Object with features to process.</param>
		/// <returns>Best interface, if found, null otherwise.</returns>
		public static InterfaceType[] FindSupport<InterfaceType, ObjectType>(ObjectType Object)
			where InterfaceType : IProcessingSupport<ObjectType>
		{
			return FindSupport<InterfaceType, ObjectType>(Object, Grade.Barely, GetTypesImplementingInterface(typeof(InterfaceType)));
		}

		/// <summary>
		/// Finds the best interface for a certain task.
		/// </summary>
		/// <typeparam name="InterfaceType">Check interfaces of this type.</typeparam>
		/// <typeparam name="ObjectType">Return interfaces supporting processing of this type 
		/// (i.e. implementing <see cref="IProcessingSupport{ObjectType}"/>).</typeparam>
		/// <param name="Object">Object with features to process.</param>
		/// <param name="MinSupport">Minimum support required.</param>
		/// <returns>Best interface, if found, null otherwise.</returns>
		public static InterfaceType[] FindSupport<InterfaceType, ObjectType>(ObjectType Object, Grade MinSupport)
			where InterfaceType : IProcessingSupport<ObjectType>
		{
			return FindSupport<InterfaceType, ObjectType>(Object, MinSupport, GetTypesImplementingInterface(typeof(InterfaceType)));
		}

		/// <summary>
		/// Finds interfaces that support a a certain task, ordered by reverse order of support.
		/// </summary>
		/// <typeparam name="InterfaceType">Check interfaces of this type.</typeparam>
		/// <typeparam name="ObjectType">Return interfaces supporting processing of this type 
		/// (i.e. implementing <see cref="IProcessingSupport{ObjectType}"/>).</typeparam>
		/// <param name="Object">Object with features to process.</param>
		/// <param name="MinSupport">Minimum support required.</param>
		/// <param name="Interfaces">Array of types (of <typeparamref name="InterfaceType"/>) to search.</param>
		/// <returns>Best interface, if found, null otherwise.</returns>
		public static InterfaceType[] FindSupport<InterfaceType, ObjectType>(ObjectType Object, Grade MinSupport, Type[] Interfaces)
			where InterfaceType : IProcessingSupport<ObjectType>
		{
			ChunkedList<KeyValuePair<InterfaceType, Grade>> Found = new ChunkedList<KeyValuePair<InterfaceType, Grade>>();
			TypeInfo TI;

			foreach (Type T2 in Interfaces)
			{
				TI = T2.GetTypeInfo();
				if (TI.IsAbstract || TI.IsInterface || TI.IsGenericTypeDefinition)
					continue;

				try
				{
					InterfaceType Candidate = (InterfaceType)Instantiate(T2);
					Grade Support = Candidate.Supports(Object);

					if (Support >= MinSupport)
						Found.Add(new KeyValuePair<InterfaceType, Grade>(Candidate, Support));
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}

			Found.Sort((x, y) =>
			{
				int j = x.Value.CompareTo(y.Value);
				if (j != 0)
					return j;

				return x.Key.GetType().FullName.CompareTo(y.Key.GetType().FullName);
			});

			ChunkedList<InterfaceType> Result = new ChunkedList<InterfaceType>();
			ChunkNode<KeyValuePair<InterfaceType, Grade>> Loop = Found.FirstChunk;
			int i, c;

			while (!(Loop is null))
			{
				for (i = Loop.Start, c = Loop.Pos; i < c; i++)
					Result.Add(Loop[i].Key);

				Loop = Loop.Next;
			}

			return Result.ToArray();
		}

		/// <summary>
		/// Tries to parse an enumeration value in string form, given the full name of the enumeration type.
		/// </summary>
		/// <param name="TypeName">Full name of the enumeration type.</param>
		/// <param name="StringValue">String representation of enumeration value.</param>
		/// <param name="Value">Value, if parsed.</param>
		/// <returns>If the string value could be parsed to an enumeration value of the given type.</returns>
		public static bool TryParseEnum(string TypeName, string StringValue, out Enum Value)
		{
			Type T = GetType(TypeName);
			if (T is null || !T.IsEnum)
			{
				Value = null;
				return false;
			}

			MethodInfo TryParse = null;

			lock (synchObject)
			{
				if (!tryParseMethods.TryGetValue(TypeName, out TryParse))
				{
					foreach (MethodInfo MI in typeof(Enum).GetRuntimeMethods())
					{
						if (MI.ContainsGenericParameters && MI.IsStatic && MI.IsPublic && MI.Name == "TryParse" &&
							MI.ReturnType == typeof(bool) && MI.GetParameters().Length == 2)
						{
							TryParse = MI;
							break;
						}
					}

					TryParse = TryParse?.MakeGenericMethod(T);
					tryParseMethods[TypeName] = TryParse;
				}
			}

			if (TryParse is null)
			{
				Value = null;
				return false;
			}

			object[] P = new object[] { StringValue, null };
			bool Result = (bool)TryParse.Invoke(null, P);

			Value = (Enum)P[1];
			return Result;
		}

		/// <summary>
		/// Returns an instance of the type <paramref name="Type"/>. Creates an instance of a type.
		/// If the constructor requires arguments, these are instantiated as necessary, if not provided
		/// in <paramref name="Arguments"/>.
		/// </summary>
		/// <param name="ReturnNullIfFail">If null should be returned instead for throwing exceptions.</param>
		/// <param name="Type">Type of objects to return.</param>
		/// <param name="Arguments">Constructor arguments.</param>
		/// <returns>Instance of <paramref name="Type"/>.</returns>
		public static object Create(bool ReturnNullIfFail, Type Type, params object[] Arguments)
		{
			TypeInfo TI = Type.GetTypeInfo();
			if (TI.IsPrimitive)
				return Activator.CreateInstance(Type);

			ParameterInfo[] Parameters;
			int i, NrParams, NrArgs = Arguments.Length;
			TypeInfo[] ArgType = NrArgs == 0 ? null : new TypeInfo[NrArgs];
			int Pass;

			for (i = 0; i < NrArgs; i++)
				ArgType[i] = Arguments[i]?.GetType().GetTypeInfo();

			// Pass 0: Use first public constructor whose parameters match arguments exactly
			// Pass 1: Use first constructor whose parameters match provided arguments exactly, but that allows instantiation of arguments not provided.

			for (Pass = 0; Pass < 2; Pass++)
			{
				foreach (ConstructorInfo CI in TI.DeclaredConstructors)
				{
					if (Pass == 0 && !CI.IsPublic)
						continue;

					if (CI.IsStatic)
						continue;

					if (CI.ContainsGenericParameters)
						continue;

					Parameters = CI.GetParameters();
					NrParams = Parameters.Length;
					if ((Pass == 0 && NrParams != NrArgs) || (Pass == 1 && NrParams < NrArgs))
						continue;

					for (i = 0; i < NrArgs; i++)
					{
						if (!(ArgType[i] is null) && !Parameters[i].ParameterType.IsAssignableFrom(ArgType[i]))
							break;
					}

					if (i < NrArgs)
						continue;

					if (NrArgs < NrParams)
					{
						Array.Resize(ref Arguments, NrParams);

						for (; i < NrParams; i++)
							Arguments[i] = Instantiate(ReturnNullIfFail, Parameters[i].ParameterType);
					}

					return CI.Invoke(Arguments);
				}
			}

			if (ReturnNullIfFail)
				return null;
			else
			{
				StringBuilder Msg = new StringBuilder();

				Msg.Append("Unable to instantiate an object of type ");
				Msg.Append(Type.FullName);
				Msg.Append(". ");

				if (NrArgs == 0)
					Msg.Append("No arguments provided.");
				else
				{
					Msg.Append("Argument types provided: ");

					for (i = 0; i < NrArgs; i++)
					{
						object Obj = Arguments[i];

						if (i > 0)
							Msg.Append(", ");

						if (Obj is null)
							Msg.Append("null");
						else
							Msg.Append(Obj.GetType().FullName);
					}
				}

				throw new MissingMethodException(Msg.ToString());
			}
		}

		/// <summary>
		/// Returns an instance of the type <typeparamref name="T"/>. If one needs to be created, it is.
		/// If the constructor requires arguments, these are instantiated as necessary, if not provided
		/// in <paramref name="Arguments"/>. Attributes <see cref="SingletonAttribute"/> and
		/// <see cref="DefaultImplementationAttribute"/> can be used to control instantiation of
		/// singleton classes, interfaces or abstract classes.
		/// </summary>
		/// <typeparam name="T">Type of objects to return.</typeparam>
		/// <param name="ReturnNullIfFail">If null should be returned instead for throwing exceptions.</param>
		/// <param name="Arguments">Constructor arguments.</param>
		/// <returns>Instance of <typeparamref name="T"/>.</returns>
		public static T Instantiate<T>(bool ReturnNullIfFail, params object[] Arguments)
		{
			return (T)Instantiate(ReturnNullIfFail, typeof(T), Arguments);
		}

		/// <summary>
		/// Returns an instance of the type <paramref name="Type"/>. If one needs to be created, it is.
		/// If the constructor requires arguments, these are instantiated as necessary, if not provided
		/// in <paramref name="Arguments"/>. Attributes <see cref="SingletonAttribute"/> and
		/// <see cref="DefaultImplementationAttribute"/> can be used to control instantiation of
		/// singleton classes, interfaces or abstract classes.
		/// </summary>
		/// <param name="Type">Type of objects to return.</param>
		/// <param name="Arguments">Constructor arguments.</param>
		/// <returns>Instance of <paramref name="Type"/>.</returns>
		public static object Instantiate(Type Type, params object[] Arguments)
		{
			return Instantiate(false, Type, Arguments);
		}

		/// <summary>
		/// Returns an instance of the type <paramref name="Type"/>. If one needs to be created, it is.
		/// If the constructor requires arguments, these are instantiated as necessary, if not provided
		/// in <paramref name="Arguments"/>. Attributes <see cref="SingletonAttribute"/> and
		/// <see cref="DefaultImplementationAttribute"/> can be used to control instantiation of
		/// singleton classes, interfaces or abstract classes.
		/// </summary>
		/// <param name="ReturnNullIfFail">If null should be returned instead for throwing exceptions.</param>
		/// <param name="Type">Type of objects to return.</param>
		/// <param name="Arguments">Constructor arguments.</param>
		/// <returns>Instance of <paramref name="Type"/>.</returns>
		public static object Instantiate(bool ReturnNullIfFail, Type Type, params object[] Arguments)
		{
			if (Type is null)
			{
				if (ReturnNullIfFail)
					return null;
				else
					throw new ArgumentException("Type cannot be null.", nameof(Type));
			}

			TypeInfo TI = Type.GetTypeInfo();

			if (TI.IsInterface || TI.IsAbstract || TI.IsGenericTypeDefinition)
			{
				if (DefaultImplementationAttribute.TryGetDefaultImplementation(Type, out Type DefaultImplementation))
				{
					Type = DefaultImplementation;
					TI = Type.GetTypeInfo();
				}
				else if (ReturnNullIfFail)
					return null;
				else
					throw new ArgumentException("Interface " + Type.FullName + " lacks a default implementation.", nameof(Type));
			}

			SingletonAttribute Singleton = TI.GetCustomAttribute<SingletonAttribute>(true);

			if (Singleton is null)
				return Create(ReturnNullIfFail, Type, Arguments);
			else
				return Singleton.Instantiate(ReturnNullIfFail, Type, Arguments);
		}

		/// <summary>
		/// Returns an instance of the type defined by the full name <paramref name="TypeName"/>. If one needs to be created, it is.
		/// If the constructor requires arguments, these are instantiated as necessary, if not provided
		/// in <paramref name="Arguments"/>. Attributes <see cref="SingletonAttribute"/> and
		/// <see cref="DefaultImplementationAttribute"/> can be used to control instantiation of
		/// singleton classes, interfaces or abstract classes.
		/// </summary>
		/// <param name="TypeName">Full name of type of objects to return.</param>
		/// <param name="Arguments">Constructor arguments.</param>
		/// <returns>Instance of object.</returns>
		public static object Instantiate(string TypeName, params object[] Arguments)
		{
			return Instantiate(false, GetType(TypeName), Arguments);
		}

		/// <summary>
		/// Returns an instance of the type defined by the full name <paramref name="TypeName"/>. If one needs to be created, it is.
		/// If the constructor requires arguments, these are instantiated as necessary, if not provided
		/// in <paramref name="Arguments"/>. Attributes <see cref="SingletonAttribute"/> and
		/// <see cref="DefaultImplementationAttribute"/> can be used to control instantiation of
		/// singleton classes, interfaces or abstract classes.
		/// </summary>
		/// <param name="ReturnNullIfFail">If null should be returned instead for throwing exceptions.</param>
		/// <param name="TypeName">Full name of type of objects to return.</param>
		/// <param name="Arguments">Constructor arguments.</param>
		/// <returns>Instance of object.</returns>
		public static object Instantiate(bool ReturnNullIfFail, string TypeName, params object[] Arguments)
		{
			return Instantiate(ReturnNullIfFail, GetType(TypeName), Arguments);
		}

		/// <summary>
		/// Returns an instance of the type <typeparamref name="T"/>. If one needs to be created, it is.
		/// If the constructor requires arguments, these are instantiated as necessary, if not provided
		/// in <paramref name="Arguments"/>. Attributes <see cref="SingletonAttribute"/> and
		/// <see cref="DefaultImplementationAttribute"/> can be used to control instantiation of
		/// singleton classes, interfaces or abstract classes.
		/// 
		/// If providing arguments when creating a singleton instance, the result will be registered as 
		/// the default singleton instance, for cases when the type is instantiated without arguments.
		/// </summary>
		/// <typeparam name="T">Type of objects to return.</typeparam>
		/// <param name="ReturnNullIfFail">If null should be returned instead for throwing exceptions.</param>
		/// <param name="Arguments">Constructor arguments.</param>
		/// <returns>Instance of <typeparamref name="T"/>.</returns>
		public static T InstantiateDefault<T>(bool ReturnNullIfFail, params object[] Arguments)
		{
			return (T)InstantiateDefault(ReturnNullIfFail, typeof(T), Arguments);
		}

		/// <summary>
		/// Returns an instance of the type <paramref name="Type"/>. If one needs to be created, it is.
		/// If the constructor requires arguments, these are instantiated as necessary, if not provided
		/// in <paramref name="Arguments"/>. Attributes <see cref="SingletonAttribute"/> and
		/// <see cref="DefaultImplementationAttribute"/> can be used to control instantiation of
		/// singleton classes, interfaces or abstract classes.
		/// 
		/// If providing arguments when creating a singleton instance, the result will be registered as 
		/// the default singleton instance, for cases when the type is instantiated without arguments.
		/// </summary>
		/// <param name="Type">Type of objects to return.</param>
		/// <param name="Arguments">Constructor arguments.</param>
		/// <returns>Instance of <paramref name="Type"/>.</returns>
		public static object InstantiateDefault(Type Type, params object[] Arguments)
		{
			return InstantiateDefault(false, Type, Arguments);
		}

		/// <summary>
		/// Returns an instance of the type <paramref name="Type"/>. If one needs to be created, it is.
		/// If the constructor requires arguments, these are instantiated as necessary, if not provided
		/// in <paramref name="Arguments"/>. Attributes <see cref="SingletonAttribute"/> and
		/// <see cref="DefaultImplementationAttribute"/> can be used to control instantiation of
		/// singleton classes, interfaces or abstract classes.
		/// 
		/// If providing arguments when creating a singleton instance, the result will be registered as 
		/// the default singleton instance, for cases when the type is instantiated without arguments.
		/// </summary>
		/// <param name="ReturnNullIfFail">If null should be returned instead for throwing exceptions.</param>
		/// <param name="Type">Type of objects to return.</param>
		/// <param name="Arguments">Constructor arguments.</param>
		/// <returns>Instance of <paramref name="Type"/>.</returns>
		public static object InstantiateDefault(bool ReturnNullIfFail, Type Type, params object[] Arguments)
		{
			object Result = Instantiate(ReturnNullIfFail, Type, Arguments);
			if (Result is null)
				return null;

			if (Arguments is null || Arguments.Length == 0)
				return Result;

			if (!(Type.GetCustomAttribute<SingletonAttribute>() is null))
				RegisterSingleton(Result);
			else
			{
				Type Type2 = Result.GetType();
				if (Type2 != Type && !(Type2.GetCustomAttribute<SingletonAttribute>() is null))
					RegisterSingleton(Result);
			}

			return Result;
		}

		/// <summary>
		/// Returns an instance of the type defined by the full name <paramref name="TypeName"/>. If one needs to be created, it is.
		/// If the constructor requires arguments, these are instantiated as necessary, if not provided
		/// in <paramref name="Arguments"/>. Attributes <see cref="SingletonAttribute"/> and
		/// <see cref="DefaultImplementationAttribute"/> can be used to control instantiation of
		/// singleton classes, interfaces or abstract classes.
		/// 
		/// If providing arguments when creating a singleton instance, the result will be registered as 
		/// the default singleton instance, for cases when the type is instantiated without arguments.
		/// </summary>
		/// <param name="TypeName">Full name of type of objects to return.</param>
		/// <param name="Arguments">Constructor arguments.</param>
		/// <returns>Instance of object.</returns>
		public static object InstantiateDefault(string TypeName, params object[] Arguments)
		{
			return InstantiateDefault(false, GetType(TypeName), Arguments);
		}

		/// <summary>
		/// Returns an instance of the type defined by the full name <paramref name="TypeName"/>. If one needs to be created, it is.
		/// If the constructor requires arguments, these are instantiated as necessary, if not provided
		/// in <paramref name="Arguments"/>. Attributes <see cref="SingletonAttribute"/> and
		/// <see cref="DefaultImplementationAttribute"/> can be used to control instantiation of
		/// singleton classes, interfaces or abstract classes.
		/// 
		/// If providing arguments when creating a singleton instance, the result will be registered as 
		/// the default singleton instance, for cases when the type is instantiated without arguments.
		/// </summary>
		/// <param name="ReturnNullIfFail">If null should be returned instead for throwing exceptions.</param>
		/// <param name="TypeName">Full name of type of objects to return.</param>
		/// <param name="Arguments">Constructor arguments.</param>
		/// <returns>Instance of object.</returns>
		public static object InstantiateDefault(bool ReturnNullIfFail, string TypeName, params object[] Arguments)
		{
			return InstantiateDefault(ReturnNullIfFail, GetType(TypeName), Arguments);
		}

		/// <summary>
		/// Registers a singleton instance of a type.
		/// </summary>
		/// <param name="Object">Singleton object instance.</param>
		/// <param name="Arguments">Any constructor arguments associated with the object instance.</param>
		public static void RegisterSingleton(object Object, params object[] Arguments)
		{
			SingletonAttribute.Register(Object, Arguments);
		}

		/// <summary>
		/// Unregisters a singleton instance of a type.
		/// </summary>
		/// <param name="Object">Singleton object instance.</param>
		/// <param name="Arguments">Any constructor arguments associated with the object instance.</param>
		/// <returns>If the instance was found and removed.</returns>
		public static bool UnregisterSingleton(object Object, params object[] Arguments)
		{
			return SingletonAttribute.Unregister(Object, Arguments);
		}

		/// <summary>
		/// Replaces a singleton instance of a type.
		/// </summary>
		/// <param name="Object">Singleton object instance.</param>
		/// <param name="Arguments">Any constructor arguments associated with the object instance.</param>
		/// <returns>If the instance was found and removed.</returns>
		public static void ReplaceSingleton(object Object, params object[] Arguments)
		{
			SingletonAttribute.Replace(Object, Arguments);
		}

		/// <summary>
		/// Checks if a singleton type (with optional associated arguments) is registered.
		/// </summary>
		/// <param name="Type">Singleton type</param>
		/// <param name="Arguments">Any constructor arguments associated with the type.</param>
		/// <returns>If such a singleton type is registered.</returns>
		public static bool IsSingletonRegistered(Type Type, params object[] Arguments)
		{
			return SingletonAttribute.IsRegistered(Type, Arguments);
		}

		/// <summary>
		/// Gets available singleton instances.
		/// </summary>
		/// <returns>Singleton instances.</returns>
		public static SingletonRecord[] GetSingletonInstances()
		{
			return SingletonAttribute.GetInstances();
		}

		/// <summary>
		/// Registers a default implementation for an interface. Such a registration takes presedence of any default implementations
		/// provided by the <see cref="DefaultImplementationAttribute"/> associated with the interface definition.
		/// </summary>
		/// <param name="From">Type of interface.</param>
		/// <param name="To">Default implementation.</param>
		public static void RegisterDefaultImplementation(Type From, Type To)
		{
			DefaultImplementationAttribute.RegisterDefaultImplementation(From, To);
		}

		/// <summary>
		/// Tries to get the default implementation for an interface.
		/// </summary>
		/// <param name="Type">Type of interface.</param>
		/// <param name="DefaultImplementation">Default implementation to use for interface.</param>
		/// <returns>If a default implementation was found.</returns>
		public static bool TryGetDefaultImplementation(Type Type, out Type DefaultImplementation)
		{
			return DefaultImplementationAttribute.TryGetDefaultImplementation(Type, out DefaultImplementation);
		}

		/// <summary>
		/// Checks if an interface has a default implementation registered.
		/// </summary>
		/// <param name="Type">Type of interface.</param>
		/// <returns>If a default implementation was registered.</returns>
		public static bool IsDefaultImplementationRegistered(Type Type)
		{
			return TryGetDefaultImplementation(Type, out _);
		}

		/// <summary>
		/// Unregisters a default implementation for an interface.
		/// </summary>
		/// <param name="From">Type of interface.</param>
		/// <param name="To">Default implementation.</param>
		public static void UnregisterDefaultImplementation(Type From, Type To)
		{
			DefaultImplementationAttribute.UnregisterDefaultImplementation(From, To);
		}

		/// <summary>
		/// Gets the default constructor of a type, if one exists.
		/// </summary>
		/// <param name="Type">Type to instantiate.</param>
		/// <returns>Default constructor, if one exists, null otherwise.</returns>
		public static ConstructorInfo GetDefaultConstructor(Type Type)
		{
			ConstructorInfo Result;

			lock (defaultConstructors)
			{
				if (defaultConstructors.TryGetValue(Type, out Result))
					return Result;
			}

			TypeInfo TI = Type.GetTypeInfo();
			if (TI.IsAbstract || TI.IsInterface || TI.IsGenericTypeDefinition)
				Result = null;
			else
			{
				Result = null;

				foreach (ConstructorInfo CI in TI.DeclaredConstructors)
				{
					if (!CI.IsPublic)
						continue;

					ParameterInfo[] Parameters = CI.GetParameters();
					if (Parameters.Length == 0)
					{
						Result = CI;
						break;
					}
				}
			}

			lock (defaultConstructors)
			{
				defaultConstructors[Type] = Result;
			}

			return Result;
		}

		/// <summary>
		/// Gets the assembly corresponding to a given resource name.
		/// </summary>
		/// <param name="ResourceName">Resource name.</param>
		/// <returns>Assembly.</returns>
		/// <exception cref="ArgumentException">If no assembly could be found.</exception>
		public static Assembly GetAssemblyForResource(string ResourceName)
		{
			string[] Parts = ResourceName.Split('.');
			string ParentNamespace;
			int i, c;
			Assembly A;

			if (!IsRootNamespace(Parts[0]))
				A = null;
			else
			{
				ParentNamespace = Parts[0];
				i = 1;
				c = Parts.Length;

				while (i < c)
				{
					if (!IsSubNamespace(ParentNamespace, Parts[i]))
						break;

					ParentNamespace += "." + Parts[i];
					i++;
				}

				A = GetFirstAssemblyReferenceInNamespace(ParentNamespace);
			}

			if (A is null)
				throw new ArgumentException("Assembly not found for resource " + ResourceName + ".", nameof(ResourceName));

			return A;
		}

	}
}
