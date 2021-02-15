using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
		private static readonly Dictionary<string, MethodInfo> tryParseMethods = new Dictionary<string, MethodInfo>();
		private static Assembly[] assemblies = null;
		private static IModule[] modules = null;
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
				tryParseMethods.Clear();
			}

			EventHandler h = OnInvalidated;
			if (!(h is null))
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
						Array.Sort<IModule>(Modules, Order);

					foreach (IModule Module in Modules)
					{
						try
						{
							await Module.Stop();
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}
					}

					modules = new IModule[0];
				}

				lock (moduleParameters)
				{
					moduleParameters.Clear();
				}
			}

			SingletonAttribute.Clear();
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
		public static async Task<bool> StartAllModules(int Timeout, IComparer<IModule> Order)
		{
			if (modules is null || modules.Length == 0)
			{
				List<Task> Tasks = new List<Task>();
				List<IModule> Modules = new List<IModule>();
				IModule Module;
				TypeInfo TI;

				foreach (Type T in GetTypesImplementingInterface(typeof(IModule)))
				{
					TI = T.GetTypeInfo();
					if (TI.IsAbstract || TI.IsGenericTypeDefinition)
						continue;

					try
					{
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
						Tasks.Add(StartModule(Module2, 60000)); // 1 min timeout
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}

				modules = Modules.ToArray();

				if (Tasks.Count > 0)
				{
					Task TimeoutTask = Task.Delay(Timeout);
					Task Result = await Task.WhenAny(Task.WhenAll(Tasks.ToArray()), TimeoutTask);
					return Result != TimeoutTask;
				}
				else
					return true;
			}
			else
				return true;
		}

		private static async Task StartModule(IModule Module, int TimeoutMilliseconds)
		{
			Type T = Module.GetType();

			try
			{
				Log.Informational("Starting module.", T.FullName);

				Task<int> TimeoutTask = Timeout(TimeoutMilliseconds);
				Task<int> StartTask = StartModule2(Module);
				Task<int> First = await Task.WhenAny<int>(StartTask, TimeoutTask);

				if (await First != 0)
					Log.Warning("Starting module takes too long time. Startup continues in the background.", T.FullName);
				else
					Log.Informational("Module started.", T.FullName);
			}
			catch (Exception ex)
			{
				Log.Error("Unable to start module: " + ex.Message, T.FullName);
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
			return FindBest<InterfaceType, ObjectType>(Object, Types.GetTypesImplementingInterface(typeof(InterfaceType)));
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

			foreach (Type T2 in Interfaces)
			{
				try
				{
					InterfaceType Enumerator = (InterfaceType)Activator.CreateInstance(T2);
					Grade Grade = Enumerator.Supports(Object);
					if (Grade > BestGrade)
					{
						Best = Enumerator;
						BestGrade = Grade;
					}
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

			return Best;
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
			if (T is null || !T.GetTypeInfo().IsEnum)
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
						if (MI.ContainsGenericParameters && MI.IsStatic && MI.Name == "TryParse" &&
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
						if (!(ArgType[i] is null) && !Parameters[i].ParameterType.GetTypeInfo().IsAssignableFrom(ArgType[i]))
							break;
					}

					if (i < NrArgs)
						continue;

					if (NrArgs < NrParams)
					{
						Array.Resize<object>(ref Arguments, NrParams);

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

			if (TI.IsInterface || TI.IsAbstract)
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

			if (!(Arguments is null) && Arguments.Length > 0 && !(Type.GetTypeInfo().GetCustomAttribute(typeof(SingletonAttribute)) is null))
				RegisterSingleton(Result);

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

	}
}
