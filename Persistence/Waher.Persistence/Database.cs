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

		/// <summary>
		/// Registers a database provider for use from the static <see cref="Database"/> class, 
		/// throughout the lifetime of the application.
		/// 
		/// Note: Only one database provider can be registered.
		/// </summary>
		/// <param name="DatabaseProvider">Database provider to use.</param>
		public static void Register(IDatabaseProvider DatabaseProvider)
		{
			if (provider != null)
				throw new Exception("A database provider is already registered.");

			provider = DatabaseProvider;
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
		/// Inserts an object into the default collection of the database.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public static Task Insert(object Object)
		{
			return Provider.Insert(Object);
		}

		/// <summary>
		/// Inserts a set of objects into the default collection of the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public static Task Insert(params object[] Objects)
		{
			return Provider.Insert(Objects);
		}

		/// <summary>
		/// Inserts a set of objects into the default collection of the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public static Task Insert(IEnumerable<object> Objects)
		{
			return Provider.Insert(Objects);
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
		/// Finds the first object of a given class <typeparamref name="T"/> and deletes the rest.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		///	<exception cref="TimeoutException">Thrown if a response is not returned from the database within the given number of milliseconds.</exception>
		public async static Task<T> FindFirstDeleteRest<T>(params string[] SortOrder)
		{
			return await FirstDeleteRest<T>(await Provider.Find<T>(0, int.MaxValue, SortOrder));
		}

		private static async Task<T> FirstDeleteRest<T>(IEnumerable<T> Set)
		{
			T Result = default(T);
			bool First = true;

			foreach (T Obj in Set)
			{
				if (First)
				{
					First = false;
					Result = Obj;
				}
				else
					await Database.Delete(Obj);
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
		/// <returns>Objects found.</returns>
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
			return FirstIgnoreRest<T>(await Provider.Find<T>(0, int.MaxValue, SortOrder));
		}

		private static T FirstIgnoreRest<T>(IEnumerable<T> Set)
		{
			foreach (T Obj in Set)
				return Obj;

			return default(T);
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
			return FirstIgnoreRest<T>(await Provider.Find<T>(0, int.MaxValue, Filter, SortOrder));
		}

		/// <summary>
		/// Updates an object in the database.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public static Task Update(object Object)
		{
			return Provider.Update(Object);
		}

		/// <summary>
		/// Updates a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public static Task Update(params object[] Objects)
		{
			return Provider.Update(Objects);
		}

		/// <summary>
		/// Updates a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public static Task Update(IEnumerable<object> Objects)
		{
			return Provider.Update(Objects);
		}

		/// <summary>
		/// Deletes an object in the database.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public static Task Delete(object Object)
		{
			return Provider.Delete(Object);
		}

		/// <summary>
		/// Deletes a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public static Task Delete(params object[] Objects)
		{
			if (Objects.Length == 1)
				return Provider.Delete(Objects[0]);
			else
				return Provider.Delete(Objects);
		}

		/// <summary>
		/// Deletes a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public static Task Delete(IEnumerable<object> Objects)
		{
			return Provider.Delete(Objects);
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
		public static Task Clear(string CollectionName)
		{
			return Provider.Clear(CollectionName);
		}


		/// <summary>
		/// Analyzes the database and exports findings to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="XsltPath">Optional XSLT to use to view the output.</param>
		/// <param name="ProgramDataFolder">Program data folder. Can be removed from filenames used, when referencing them in the report.</param>
		/// <param name="ExportData">If data in database is to be exported in output.</param>
		public static Task Analyze(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData)
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
		public static Task Repair(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData)
		{
			return Provider.Repair(Output, XsltPath, ProgramDataFolder, ExportData);
		}

		/// <summary>
		/// Analyzes the database and exports findings to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="XsltPath">Optional XSLT to use to view the output.</param>
		/// <param name="ProgramDataFolder">Program data folder. Can be removed from filenames used, when referencing them in the report.</param>
		/// <param name="ExportData">If data in database is to be exported in output.</param>
		/// <param name="Repair">If files should be repaired if corruptions are detected.</param>
		public static Task Analyze(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData, bool Repair)
		{
			return Provider.Analyze(Output, XsltPath, ProgramDataFolder, ExportData, Repair);
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
			int i, c = A.Length;
			CaseInsensitiveString[] B = new CaseInsensitiveString[c];

			for (i = 0; i < c; i++)
				B[i] = A[i];

			return B;
		}

	}
}
