using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using Waher.Events;

namespace Waher.Script
{
    /// <summary>
    /// Static class that dynamically manages types and interfaces available in the runtime environment.
    /// </summary>
    public static class Types
    {
        /// <summary>
        /// Case insensitive string comparer.
        /// </summary>
        public static readonly StringComparer CaseInsensitiveComparer = StringComparer.Create(System.Globalization.CultureInfo.InvariantCulture, true);

        private static SortedDictionary<string, Type> types = new SortedDictionary<string, Type>();
        private static SortedDictionary<string, SortedDictionary<string, Type>> typesPerInterface = new SortedDictionary<string, SortedDictionary<string, Type>>();
        private static SortedDictionary<string, SortedDictionary<string, Type>> typesPerNamespace = new SortedDictionary<string, SortedDictionary<string, Type>>();
        private static SortedDictionary<string, SortedDictionary<string, bool>> namespacesPerNamespace = new SortedDictionary<string, SortedDictionary<string, bool>>();
        private static SortedDictionary<string, bool> rootNamespaces = new SortedDictionary<string, bool>();
        private static IModule[] modules = null;
        private static readonly Type[] noTypes = new Type[0];
        private static readonly object[] noParameters = new object[0];
        private static object synchObject = new object();
        private static bool memoryScanned = false;

        static Types()
        {
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        }

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
            string InterfaceName;
            string TypeName;
            string Namespace;
            string ParentNamespace;
            string LastNamespace = string.Empty;
            int i, j;

            string BinaryFolder = AppDomain.CurrentDomain.BaseDirectory;
            string[] DllFiles = Directory.GetFiles(BinaryFolder, "*.dll", SearchOption.TopDirectoryOnly);
            Dictionary<string, Assembly> LoadedAssemblies = new Dictionary<string, Assembly>(CaseInsensitiveComparer);

            foreach (Assembly Assembly in AppDomain.CurrentDomain.GetAssemblies())
                LoadedAssemblies[Assembly.Location] = Assembly;

            foreach (string DllFile in DllFiles)
            {
                if (LoadedAssemblies.ContainsKey(DllFile))
                    continue;

                Assembly A = Assembly.LoadFrom(DllFile);
                LoadedAssemblies[DllFile] = A;
            }

            foreach (Assembly Assembly in LoadedAssemblies.Values)
            {
                foreach (Type Type in Assembly.GetTypes())
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

            memoryScanned = true;

            List<IModule> Modules = new List<IModule>();
            ConstructorInfo CI;
            IModule Module;

            foreach (Type T in GetTypesImplementingInterface(typeof(IModule)))
            {
                if (T.IsAbstract)
                    continue;

                try
                {
                    CI = T.GetConstructor(noTypes);
                    if (CI == null)
                        continue;

                    Module = (IModule)CI.Invoke(noParameters);
                    Module.Start();

                    Modules.Add(Module);
                }
                catch (Exception ex)
                {
                    Log.Critical(ex);
                }
            }

            modules = Modules.ToArray();
        }

        private static void OnProcessExit(object Sender, EventArgs e)
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

    }
}
