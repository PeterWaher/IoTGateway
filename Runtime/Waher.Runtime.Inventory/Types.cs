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
		private static readonly SortedDictionary<string, Type> types = new SortedDictionary<string, Type>();
		private static readonly SortedDictionary<string, SortedDictionary<string, Type>> typesPerInterface = new SortedDictionary<string, SortedDictionary<string, Type>>();
		private static readonly SortedDictionary<string, SortedDictionary<string, Type>> typesPerNamespace = new SortedDictionary<string, SortedDictionary<string, Type>>();
		private static readonly SortedDictionary<string, SortedDictionary<string, bool>> namespacesPerNamespace = new SortedDictionary<string, SortedDictionary<string, bool>>();
		private static readonly SortedDictionary<string, bool> rootNamespaces = new SortedDictionary<string, bool>();
		private static readonly SortedDictionary<string, object> qualifiedNames = new SortedDictionary<string, object>();
		private static readonly Dictionary<string, object> moduleParameters = new Dictionary<string, object>();
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
			StopAllModules();
		}

		/// <summary>
		/// Stops all modules.
		/// </summary>
		public static void StopAllModules()
		{
			StopAllModules(null);
		}

		/// <summary>
		/// Stops all modules.
		/// </summary>
		/// <param name="Order">Order in which modules should be stopped.
		/// Default order is the reverse starting order, if no other order is provided.</param>
		public static void StopAllModules(IComparer<IModule> Order)
		{
			if (isInitialized)
			{
				IModule[] Modules = (IModule[])Types.Modules?.Clone();

				if (!(Modules is null))
				{
					if (Order is null)
						Array.Reverse(Modules);
					else
						Array.Sort<IModule>(Modules, Order);

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

					modules = new IModule[0];
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
			return StartAllModules(Timeout, null);
		}

		/// <summary>
		/// Starts all loaded modules.
		/// </summary>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <param name="Order">Order in which modules should be started.</param>
		/// <returns>If all modules have been successfully started (true), or if at least one has not been
		/// started within the time period defined by <paramref name="Timeout"/>.</returns>
		public static bool StartAllModules(int Timeout, IComparer<IModule> Order)
		{
			if (modules is null || modules.Length == 0)
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
						Modules.Add(Module);
					}
					catch (Exception ex)
					{
						Log.Error("Unable to start module: " + ex.Message, T.FullName);
					}
				}

				if (!(Order is null))
					Modules.Sort(Order);

				foreach (IModule Module2 in Modules)
				{
					try
					{
						Handle = Module2.Start();
						if (Handle != null)
							Handles.Add(Handle);
					}
					catch (Exception ex)
					{
						Log.Error("Unable to start module: " + ex.Message, Module2.GetType().FullName);
					}
				}

				startWaitHandles = Handles.ToArray();
				modules = Modules.ToArray();

				if (startWaitHandles.Length == 0)
					return true;

				return WaitHandle.WaitAll(startWaitHandles, Timeout);
			}
			else
				return true;
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
				if (moduleParameters.TryGetValue(Name, out object Value2) && !Value.Equals(Value2))
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

			lock (synchObject)
			{
				if (isInitialized)
				{
					List<Assembly> NewAssemblies = new List<Assembly>();
					List<Assembly> AllAssemblies = new List<Assembly>();

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
					CheckIncluded(ref Assemblies, typeof(Types).GetTypeInfo().Assembly);
					CheckIncluded(ref Assemblies, typeof(int).GetTypeInfo().Assembly);

					if (Array.IndexOf<Assembly>(Assemblies, A = typeof(Types).GetTypeInfo().Assembly) < 0)
					{
						int c = Assemblies.Length;
						Array.Resize<Assembly>(ref Assemblies, c + 1);
						Assemblies[c] = A;
					}

					assemblies = Assemblies;
				}

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


						i = TypeName.LastIndexOf('.');
						if (i >= 0)
							RegisterQualifiedName(TypeName.Substring(i + 1), TypeName);

						try
						{
							foreach (Type Interface in Type.GetTypeInfo().ImplementedInterfaces)
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

		/// <summary>
		/// Creates an object of a given type, given its full name.
		/// </summary>
		/// <param name="TypeName">Full type name.</param>
		/// <param name="Parameters">Parameters to pass on to the constructor.</param>
		/// <returns>Created object.</returns>
		/// <exception cref="ArgumentException">If no type with the given name exists.</exception>
		public static object CreateObject(string TypeName, params object[] Parameters)
		{
			Type T = GetType(TypeName);
			if (T is null)
				throw new ArgumentException("Type not loaded: " + TypeName, nameof(TypeName));

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
				return PI.GetValue(Object);

			FieldInfo FI = T.GetRuntimeField(PropertyName);
			if (!(FI is null))
				return FI.GetValue(Object);

			throw new ArgumentException("Property (or field) not found: " + PropertyName, nameof(PropertyName));
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
				PI.SetValue(Object, Value);
			else
			{
				FieldInfo FI = T.GetRuntimeField(PropertyName);
				if (!(FI is null))
					FI.SetValue(Object, Value);
				else
					throw new ArgumentException("Property (or field) not found: " + PropertyName, nameof(PropertyName));
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
					ParameterInfo[] P = MI.GetParameters();
					if (P.Length != c)
						continue;

					bool Found = true;

					for (i = 0; i < c; i++)
					{
						Arg = Arguments[i];

						if (!(Arg is null) && !P[i].ParameterType.GetTypeInfo().IsAssignableFrom(Arg.GetType().GetTypeInfo()))
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
					return MI.Invoke(Object, Arguments);
			}

			throw new ArgumentException("Method with corresponding arguments not found: " + MethodName, nameof(MethodName));
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

	}
}
