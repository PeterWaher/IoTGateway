using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Persistence.Filters;
using Waher.Persistence.Serialization;

namespace Waher.Persistence
{
	/// <summary>
	/// Static interface for database persistence. In order to work, a database provider has to be assigned to it. This is
	/// ideally done as one of the first steps in the startup of an application.
	/// </summary>
	public static class Database
	{
		private static IDatabaseProvider provider = null;
		private static bool locked = false;

		/// <summary>
		/// Registers a database provider for use from the static <see cref="Database"/> class, 
		/// throughout the lifetime of the application.
		/// 
		/// Note: Only one database provider can be registered.
		/// </summary>
		/// <param name="DatabaseProvider">Database provider to use.</param>
		public static void Register(IDatabaseProvider DatabaseProvider)
		{
			Register(DatabaseProvider, true);
		}

		/// <summary>
		/// Registers a database provider for use from the static <see cref="Database"/> class, 
		/// throughout the lifetime of the application.
		/// 
		/// Note: Only one database provider can be registered.
		/// </summary>
		/// <param name="DatabaseProvider">Database provider to use.</param>
		/// <param name="Lock">If the database provider should be locked for the rest of the running time of the application.</param>
		public static void Register(IDatabaseProvider DatabaseProvider, bool Lock)
		{
			if (provider != null && locked)
				throw new Exception("A database provider is already registered.");

			provider = DatabaseProvider;
			locked = Lock;
		}

		/// <summary>
		/// Registered database provider.
		/// </summary>
		public static IDatabaseProvider Provider
		{
			get
			{
				if (provider != null)
					return provider;
				else
					throw new Exception("A database provider has not been registered.");
			}
		}

		/// <summary>
		/// If a database provider is registered.
		/// </summary>
		public static bool HasProvider
		{
			get => !(provider is null);
		}

		/// <summary>
		/// If the datbase provider has been locked for the rest of the run-time of the application.
		/// </summary>
		public static bool Locked
		{
			get { return locked; }
		}

		/// <summary>
		/// Inserts an object into the default collection of the database.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public async static Task Insert(object Object)
		{
			await Provider.Insert(Object);

			ObjectEventHandler h = ObjectInserted;
			if (!(h is null))
			{
				try
				{
					h(Provider, new ObjectEventArgs(Object));
				}
				catch (Exception)
				{
					// Ignore
				}
			}
		}

		/// <summary>
		/// Event raised when an object has been inserted.
		/// </summary>
		public static event ObjectEventHandler ObjectInserted = null;

		/// <summary>
		/// Inserts a set of objects into the default collection of the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public async static Task Insert(params object[] Objects)
		{
			await Provider.Insert(Objects);

			ObjectEventHandler h = ObjectInserted;
			if (!(h is null))
			{
				foreach (object Object in Objects)
				{
					try
					{
						h(Provider, new ObjectEventArgs(Object));
					}
					catch (Exception)
					{
						// Ignore
					}
				}
			}
		}

		/// <summary>
		/// Inserts a set of objects into the default collection of the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public async static Task Insert(IEnumerable<object> Objects)
		{
			await Provider.Insert(Objects);

			ObjectEventHandler h = ObjectInserted;
			if (!(h is null))
			{
				foreach (object Object in Objects)
				{
					try
					{
						h(Provider, new ObjectEventArgs(Object));
					}
					catch (Exception)
					{
						// Ignore
					}
				}
			}
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public static Task<IEnumerable<T>> Find<T>(params string[] SortOrder)
		{
			return Provider.Find<T>(0, int.MaxValue, SortOrder);
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public static Task<IEnumerable<T>> Find<T>(Filter Filter, params string[] SortOrder)
		{
			return Provider.Find<T>(0, int.MaxValue, Filter, SortOrder);
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public static Task<IEnumerable<T>> Find<T>(int Offset, int MaxCount, params string[] SortOrder)
		{
			return Provider.Find<T>(Offset, MaxCount, SortOrder);
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public static Task<IEnumerable<T>> Find<T>(int Offset, int MaxCount, Filter Filter, params string[] SortOrder)
		{
			return Provider.Find<T>(Offset, MaxCount, Filter, SortOrder);
		}

		/// <summary>
		/// Finds objects in a given collection.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public static Task<IEnumerable<object>> Find(string Collection, params string[] SortOrder)
		{
			return Provider.Find(Collection, 0, int.MaxValue, SortOrder);
		}

		/// <summary>
		/// Finds objects in a given collection.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public static Task<IEnumerable<object>> Find(string Collection, Filter Filter, params string[] SortOrder)
		{
			return Provider.Find(Collection, 0, int.MaxValue, Filter, SortOrder);
		}

		/// <summary>
		/// Finds objects in a given collection.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public static Task<IEnumerable<object>> Find(string Collection, int Offset, int MaxCount, params string[] SortOrder)
		{
			return Provider.Find(Collection, Offset, MaxCount, SortOrder);
		}

		/// <summary>
		/// Finds objects in a given collection.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public static Task<IEnumerable<object>> Find(string Collection, int Offset, int MaxCount, Filter Filter, params string[] SortOrder)
		{
			return Provider.Find(Collection, Offset, MaxCount, Filter, SortOrder);
		}

		/// <summary>
		/// Finds the first object of a given class <typeparamref name="T"/> and deletes the rest.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>First Object, if found, null otherwise.</returns>
		///	<exception cref="TimeoutException">Thrown if a response is not returned from the database within the given number of milliseconds.</exception>
		public async static Task<T> FindFirstDeleteRest<T>(params string[] SortOrder)
		{
			return await FirstDeleteRest<T>(await Provider.Find<T>(0, int.MaxValue, SortOrder));
		}

		private static async Task<T> FirstDeleteRest<T>(IEnumerable<T> Set)
		{
			T Result = default;
			bool First = true;

			foreach (T Obj in Set)
			{
				if (First)
				{
					First = false;
					Result = Obj;
				}
				else
				{
					try
					{
						await Database.Delete(Obj);
					}
					catch (KeyNotFoundException)
					{
						// TODO: Inconsistency should flag collection for repairing
					}
				}
			}

			return Result;
		}

		/// <summary>
		/// Finds the first object of a given class <typeparamref name="T"/> and deletes the rest.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>First Object, if found, null otherwise.</returns>
		///	<exception cref="TimeoutException">Thrown if a response is not returned from the database within the given number of milliseconds.</exception>
		public static async Task<T> FindFirstDeleteRest<T>(Filter Filter, params string[] SortOrder)
		{
			return await FirstDeleteRest<T>(await Provider.Find<T>(0, int.MaxValue, Filter, SortOrder));
		}

		/// <summary>
		/// Finds the first object of a given class <typeparamref name="T"/> and ignores the rest.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		///	<exception cref="TimeoutException">Thrown if a response is not returned from the database within the given number of milliseconds.</exception>
		public async static Task<T> FindFirstIgnoreRest<T>(params string[] SortOrder)
		{
			return FirstIgnoreRest<T>(await Provider.Find<T>(0, 1, SortOrder));
		}

		private static T FirstIgnoreRest<T>(IEnumerable<T> Set)
		{
			foreach (T Obj in Set)
				return Obj;

			return default;
		}

		/// <summary>
		/// Finds the first object of a given class <typeparamref name="T"/> and ignores the rest.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		///	<exception cref="TimeoutException">Thrown if a response is not returned from the database within the given number of milliseconds.</exception>
		public static async Task<T> FindFirstIgnoreRest<T>(Filter Filter, params string[] SortOrder)
		{
			return FirstIgnoreRest<T>(await Provider.Find<T>(0, 1, Filter, SortOrder));
		}

		/// <summary>
		/// Updates an object in the database.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public async static Task Update(object Object)
		{
			await Provider.Update(Object);

			ObjectEventHandler h = ObjectUpdated;
			if (!(h is null))
			{
				try
				{
					h(Provider, new ObjectEventArgs(Object));
				}
				catch (Exception)
				{
					// Ignore
				}
			}
		}

		/// <summary>
		/// Event raised when an object has been updated.
		/// </summary>
		public static event ObjectEventHandler ObjectUpdated = null;

		/// <summary>
		/// Updates a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public async static Task Update(params object[] Objects)
		{
			await Provider.Update(Objects);

			ObjectEventHandler h = ObjectUpdated;
			if (!(h is null))
			{
				foreach (object Object in Objects)
				{
					try
					{
						h(Provider, new ObjectEventArgs(Object));
					}
					catch (Exception)
					{
						// Ignore
					}
				}
			}
		}

		/// <summary>
		/// Updates a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public async static Task Update(IEnumerable<object> Objects)
		{
			await Provider.Update(Objects);

			ObjectEventHandler h = ObjectUpdated;
			if (!(h is null))
			{
				foreach (object Object in Objects)
				{
					try
					{
						h(Provider, new ObjectEventArgs(Object));
					}
					catch (Exception)
					{
						// Ignore
					}
				}
			}
		}

		/// <summary>
		/// Deletes an object in the database.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public async static Task Delete(object Object)
		{
			await Provider.Delete(Object);

			ObjectEventHandler h = ObjectDeleted;
			if (!(h is null))
			{
				try
				{
					h(Provider, new ObjectEventArgs(Object));
				}
				catch (Exception)
				{
					// Ignore
				}
			}
		}

		/// <summary>
		/// Event raised when an object has been deleted.
		/// </summary>
		public static event ObjectEventHandler ObjectDeleted = null;

		/// <summary>
		/// Deletes a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public async static Task Delete(params object[] Objects)
		{
			if (Objects.Length == 1)
				await Provider.Delete(Objects[0]);
			else
				await Provider.Delete(Objects);

			ObjectEventHandler h = ObjectDeleted;
			if (!(h is null))
			{
				foreach (object Object in Objects)
				{
					try
					{
						h(Provider, new ObjectEventArgs(Object));
					}
					catch (Exception)
					{
						// Ignore
					}
				}
			}
		}

		/// <summary>
		/// Deletes a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public async static Task Delete(IEnumerable<object> Objects)
		{
			await Provider.Delete(Objects);

			ObjectEventHandler h = ObjectDeleted;
			if (!(h is null))
			{
				foreach (object Object in Objects)
				{
					try
					{
						h(Provider, new ObjectEventArgs(Object));
					}
					catch (Exception)
					{
						// Ignore
					}
				}
			}
		}

		/// <summary>
		/// Loads an object given its Object ID <paramref name="ObjectId"/> and its base type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Base type.</typeparam>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>Loaded object.</returns>
		public static Task<T> LoadObject<T>(object ObjectId)
		{
			return Provider.LoadObject<T>(ObjectId);
		}

		/// <summary>
		/// Loads an object given its Object ID <paramref name="ObjectId"/> and its collection name <paramref name="CollectionName"/>.
		/// </summary>
		/// <typeparam name="T">Base type.</typeparam>
		/// <param name="CollectionName">Name of collection in which the object resides.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>Loaded object.</returns>
		public static Task<T> LoadObject<T>(string CollectionName, object ObjectId)
		{
			return Provider.LoadObject<T>(CollectionName, ObjectId);
		}

		/// <summary>
		/// Loads an object given its Object ID <paramref name="ObjectId"/> and its collection name <paramref name="CollectionName"/>.
		/// </summary>
		/// <param name="CollectionName">Name of collection in which the object resides.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>Loaded object.</returns>
		public static Task<object> LoadObject(string CollectionName, object ObjectId)
		{
			return Provider.LoadObject(CollectionName, ObjectId);
		}

		/// <summary>
		/// Performs an export of the entire database.
		/// </summary>
		/// <param name="Output">Database will be output to this interface.</param>
		/// <returns>Task object for synchronization purposes.</returns>
		public static Task Export(IDatabaseExport Output)
		{
			return Provider.Export(Output);
		}

		/// <summary>
		/// Clears a collection of all objects.
		/// </summary>
		/// <param name="CollectionName">Name of collection to clear.</param>
		/// <returns>Task object for synchronization purposes.</returns>
		public async static Task Clear(string CollectionName)
		{
			await Provider.Clear(CollectionName);

			CollectionEventHandler h = CollectionCleared;
			if (!(h is null))
			{
				try
				{
					h(Provider, new CollectionEventArgs(CollectionName));
				}
				catch (Exception)
				{
					// Ignore
				}
			}
		}

		/// <summary>
		/// Event raised when a collection has been cleared.
		/// </summary>
		public static event CollectionEventHandler CollectionCleared = null;

		/// <summary>
		/// Analyzes the database and exports findings to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="XsltPath">Optional XSLT to use to view the output.</param>
		/// <param name="ProgramDataFolder">Program data folder. Can be removed from filenames used, when referencing them in the report.</param>
		/// <param name="ExportData">If data in database is to be exported in output.</param>
		/// <returns>Collections with errors found.</returns>
		public static Task<string[]> Analyze(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData)
		{
			return Provider.Analyze(Output, XsltPath, ProgramDataFolder, ExportData);
		}

		/// <summary>
		/// Analyzes the database and repairs it if necessary. Results are exported to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="XsltPath">Optional XSLT to use to view the output.</param>
		/// <param name="ProgramDataFolder">Program data folder. Can be removed from filenames used, when referencing them in the report.</param>
		/// <param name="ExportData">If data in database is to be exported in output.</param>
		/// <returns>Collections with errors found and repaired.</returns>
		public async static Task<string[]> Repair(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData)
		{
			string[] Result = await Provider.Repair(Output, XsltPath, ProgramDataFolder, ExportData);

			if (Result.Length > 0)
				RaiseRepaired(Result);

			return Result;
		}

		private static void RaiseRepaired(string[] Collections)
		{
			CollectionEventHandler h = CollectionRepaired;
			if (!(h is null))
			{
				foreach (string Collection in Collections)
				{
					try
					{
						h(Provider, new CollectionEventArgs(Collection));
					}
					catch (Exception)
					{
						// Ignore
					}
				}
			}
		}

		/// <summary>
		/// Event raised when a collection has been repaired.
		/// </summary>
		public static event CollectionEventHandler CollectionRepaired = null;

		/// <summary>
		/// Analyzes the database and exports findings to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="XsltPath">Optional XSLT to use to view the output.</param>
		/// <param name="ProgramDataFolder">Program data folder. Can be removed from filenames used, when referencing them in the report.</param>
		/// <param name="ExportData">If data in database is to be exported in output.</param>
		/// <param name="Repair">If files should be repaired if corruptions are detected.</param>
		/// <returns>Collections with errors found, and repaired if <paramref name="Repair"/>=true.</returns>
		public async static Task<string[]> Analyze(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData, bool Repair)
		{
			string[] Result = await Provider.Analyze(Output, XsltPath, ProgramDataFolder, ExportData, Repair);

			if (Repair && Result.Length > 0)
				RaiseRepaired(Result);

			return Result;
		}

		/// <summary>
		/// Adds an index to a collection, if one does not already exist.
		/// </summary>
		/// <param name="CollectionName">Name of collection.</param>
		/// <param name="FieldNames">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		public static Task AddIndex(string CollectionName, string[] FieldNames)
		{
			return Provider.AddIndex(CollectionName, FieldNames);
		}

		/// <summary>
		/// Removes an index from a collection, if one exist.
		/// </summary>
		/// <param name="CollectionName">Name of collection.</param>
		/// <param name="FieldNames">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		public static Task RemoveIndex(string CollectionName, string[] FieldNames)
		{
			return Provider.RemoveIndex(CollectionName, FieldNames);
		}

		/// <summary>
		/// Starts bulk-proccessing of data. Must be followed by a call to <see cref="EndBulk"/>.
		/// </summary>
		public static Task StartBulk()
		{
			return Provider.StartBulk();
		}

		/// <summary>
		/// Ends bulk-processing of data. Must be called once for every call to <see cref="StartBulk"/>.
		/// </summary>
		public static Task EndBulk()
		{
			return Provider.EndBulk();
		}

		/// <summary>
		/// Converts a case insensitive string array to a normal string array.
		/// </summary>
		/// <param name="A">Case insensitive string array-</param>
		/// <returns>Normal string array.</returns>
		public static string[] ToStringArray(this CaseInsensitiveString[] A)
		{
			if (A is null)
				return null;

			int i, c = A.Length;
			string[] B = new string[c];

			for (i = 0; i < c; i++)
				B[i] = A[i].Value;

			return B;
		}

		/// <summary>
		/// Converts a case insensitive string array to a normal string array.
		/// </summary>
		/// <param name="A">Case insensitive string array-</param>
		/// <returns>Normal string array.</returns>
		public static CaseInsensitiveString[] ToCaseInsensitiveStringArray(this string[] A)
		{
			if (A is null)
				return null;

			int i, c = A.Length;
			CaseInsensitiveString[] B = new CaseInsensitiveString[c];

			for (i = 0; i < c; i++)
				B[i] = A[i];

			return B;
		}

		/// <summary>
		/// Gets a persistent dictionary containing objects in a collection.
		/// </summary>
		/// <param name="Collection">Collection Name</param>
		/// <returns>Persistent dictionary</returns>
		public static IPersistentDictionary GetDictionary(string Collection)
		{
			return Provider.GetDictionary(Collection);
		}

		/// <summary>
		/// Gets an array of available collections.
		/// </summary>
		/// <returns>Array of collections.</returns>
		public static Task<string[]> GetCollections()
		{
			return Provider.GetCollections();
		}

		/// <summary>
		/// Gets the collection corresponding to a given type.
		/// </summary>
		/// <param name="Type">Type</param>
		/// <returns>Collection name.</returns>
		public static Task<string> GetCollection(Type Type)
		{
			return Provider.GetCollection(Type);
		}

		/// <summary>
		/// Gets the collection corresponding to a given object.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>Collection name.</returns>
		public static Task<string> GetCollection(Object Object)
		{
			return Provider.GetCollection(Object);
		}

		/// <summary>
		/// Checks if a string is a label in a given collection.
		/// </summary>
		/// <param name="Collection">Name of collection.</param>
		/// <param name="Label">Label to check.</param>
		/// <returns>If <paramref name="Label"/> is a label in the collection
		/// defined by <paramref name="Collection"/>.</returns>
		public static Task<bool> IsLabel(string Collection, string Label)
		{
			return Provider.IsLabel(Collection, Label);
		}

		/// <summary>
		/// Gets an array of available labels for a collection.
		/// </summary>
		/// <returns>Array of labels.</returns>
		public static Task<string[]> GetLabels(string Collection)
		{
			return Provider.GetLabels(Collection);
		}

		/// <summary>
		/// Tries to get the Object ID of an object, if it exists.
		/// </summary>
		/// <param name="Object">Object whose Object ID is of interest.</param>
		/// <param name="ObjectId">Resulting Object ID will be placed in this argument.</param>
		/// <returns>If an Object ID was found.</returns>
		public static bool TryGetObjectId(object Object, out object ObjectId)
		{
			return Provider.TryGetObjectId(Object, out ObjectId);
		}

	}
}
