﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Waher.Events;
using Waher.Runtime.Collections;

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
			Initialize(string.Empty, null);
		}

		/// <summary>
		/// Initializes the inventory engine, registering types and interfaces available in <see cref="Types"/>.
		/// </summary>
		/// <param name="Folder">Name of folder containing assemblies to load, if they are not already loaded.</param>
		public static void Initialize(string Folder)
		{
			Initialize(Folder, null);
		}

		/// <summary>
		/// Initializes the inventory engine, registering types and interfaces available in <see cref="Types"/>.
		/// </summary>
		/// <param name="Folder">Name of folder containing assemblies to load, if they are not already loaded.</param>
		/// <param name="LoadFile">Optional callback method used to determine what files to load.</param>
		public static void Initialize(string Folder, Predicate<string> LoadFile)
		{
			if (string.IsNullOrEmpty(Folder))
			{
				Folder = Assembly.GetExecutingAssembly().Location;
				if (string.IsNullOrEmpty(Folder))
					Folder = AppDomain.CurrentDomain.BaseDirectory;

				Folder = Path.GetDirectoryName(Folder);
			}

			string[] DllFiles = Directory.GetFiles(Folder, "*.dll", SearchOption.TopDirectoryOnly);
			SortedDictionary<string, Assembly> LoadedAssembliesByName = new SortedDictionary<string, Assembly>(StringComparer.CurrentCultureIgnoreCase);
			SortedDictionary<string, Assembly> LoadedAssembliesByLocation = new SortedDictionary<string, Assembly>(StringComparer.CurrentCultureIgnoreCase);
			SortedDictionary<string, AssemblyName> ReferencedAssemblies = new SortedDictionary<string, AssemblyName>(StringComparer.CurrentCultureIgnoreCase);

			foreach (Assembly A in AppDomain.CurrentDomain.GetAssemblies())
			{
				LoadedAssembliesByName[A.FullName] = A;

				if (!A.IsDynamic)
					LoadedAssembliesByLocation[A.Location] = A;

				try
				{
					foreach (AssemblyName AN in A.GetReferencedAssemblies())
						ReferencedAssemblies[AN.FullName] = AN;
				}
				catch (Exception)
				{
					// Referenced assemblies might not be accessible.
				}
			}

			LoadReferencedAssemblies(LoadedAssembliesByName, LoadedAssembliesByLocation, ReferencedAssemblies);

			foreach (string DllFile in DllFiles)
			{
				if (LoadedAssembliesByLocation.ContainsKey(DllFile))
					continue;

				if (!(LoadFile is null))
				{
					try
					{
						if (!LoadFile(DllFile))
							continue;
					}
					catch (Exception ex)
					{
						Log.Exception(ex, DllFile);
						continue;
					}
				}

				AssemblyName AN;
				Assembly A;

				try
				{
					try
					{
						AN = AssemblyName.GetAssemblyName(DllFile);
						A = AppDomain.CurrentDomain.Load(AN);
					}
					catch (PlatformNotSupportedException)
					{
						continue;	// GetAssemblyName() not available on some platforms.
					}

					LoadedAssembliesByName[A.FullName] = A;
					LoadedAssembliesByLocation[A.Location] = A;

					try
					{
						foreach (AssemblyName ANRef in A.GetReferencedAssemblies())
							ReferencedAssemblies[ANRef.FullName] = ANRef;
					}
					catch (Exception)
					{
						// Referenced assemblies might not be accessible.
					}
				}
				catch (BadImageFormatException ex)
				{
					LogException(ex);
				}
				catch (FileNotFoundException)
				{
					Log.Error("Unable to load assembly. It must be registered in the deps.json file.", DllFile);
				}
				catch (Exception ex)
				{
					Log.Exception(ex, DllFile);
				}
			}

			LoadReferencedAssemblies(LoadedAssembliesByName, LoadedAssembliesByLocation, ReferencedAssemblies);

			Assembly[] Assemblies = new Assembly[LoadedAssembliesByName.Count];
			LoadedAssembliesByName.Values.CopyTo(Assemblies, 0);

			Types.Initialize(Assemblies);
		}

		private static void LogException(BadImageFormatException ex)
		{
			ChunkedList<KeyValuePair<string, object>> Tags = new ChunkedList<KeyValuePair<string, object>>()
			{
				new KeyValuePair<string, object>("FusionLog", ex.FusionLog)
			};

			foreach (KeyValuePair<object, object> P in ex.Data)
				Tags.Add(new KeyValuePair<string, object>(P.Key.ToString(), P.Value));

			Log.Exception(ex, ex.FileName, Tags.ToArray());
		}

		private static void LoadReferencedAssemblies(SortedDictionary<string, Assembly> LoadedAssembliesByName,
			SortedDictionary<string, Assembly> LoadedAssembliesByLocation,
			SortedDictionary<string, AssemblyName> ReferencedAssemblies)
		{
			do
			{
				AssemblyName[] References = new AssemblyName[ReferencedAssemblies.Count];
				ReferencedAssemblies.Values.CopyTo(References, 0);
				ReferencedAssemblies.Clear();

				foreach (AssemblyName AN in References)
				{
					if (LoadedAssembliesByName.ContainsKey(AN.FullName))
						continue;

					try
					{
						Assembly A = AppDomain.CurrentDomain.Load(AN);
						LoadedAssembliesByName[A.FullName] = A;
						LoadedAssembliesByLocation[A.Location] = A;

						try
						{
							foreach (AssemblyName AN2 in A.GetReferencedAssemblies())
								ReferencedAssemblies[AN2.FullName] = AN2;
						}
						catch (Exception)
						{
							// Referenced assemblies might not be accessible.
						}
					}
					catch (BadImageFormatException ex)
					{
						LogException(ex);
					}
					catch (Exception)
					{
						string s = AN.ToString();

						if (s.StartsWith("System.") ||
							s.StartsWith("Microsoft.") ||
							s.StartsWith("Windows.") ||
							s.StartsWith("SQLitePCLRaw.") ||
							s.StartsWith("SkiaSharp") ||
							s.StartsWith("Gma.QrCodeNet.Encoding") ||
							s.StartsWith("Esent.Interop"))
						{
							continue;
						}

						Log.Error("Unable to load assembly " + s + ".", AN.FullName);
					}
				}
			}
			while (ReferencedAssemblies.Count > 0);
		}
	}
}
