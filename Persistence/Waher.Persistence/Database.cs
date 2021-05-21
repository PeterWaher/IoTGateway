using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Events;
using Waher.Persistence.Exceptions;
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
		private static readonly Dictionary<string, Dictionary<string, FlagSource>> toRepair = new Dictionary<string, Dictionary<string, FlagSource>>();
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
				if (!(provider is null))
					return provider;
				else
					throw new Exception("A database provider has not been registered.");
			}
		}

		internal static IDatabaseProvider Stop()
		{
			IDatabaseProvider Result = provider;
			provider = new NullDatabaseProvider();
			locked = false;
			return Result;
		}

		/// <summary>
		/// If a database provider is registered.
		/// </summary>
		public static bool HasProvider
		{
			get => !(provider is null) && (!(provider is NullDatabaseProvider) || locked);
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
			RaiseInserted(Object);
		}

		private static void RaiseInserted(object Object)
		{
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

		private static void RaiseInserted(IEnumerable<object> Objects)
		{
			foreach (object Object in Objects)
				RaiseInserted(Object);
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
			if (Objects.Length == 1)
				await Provider.Insert(Objects[0]);
			else
				await Provider.Insert(Objects);

			RaiseInserted(Objects);
		}

		/// <summary>
		/// Inserts a set of objects into the default collection of the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public async static Task Insert(IEnumerable<object> Objects)
		{
			await Provider.Insert(Objects);
			RaiseInserted(Objects);
		}

		/// <summary>
		/// Inserts an object into the database, if unlocked. If locked, object will be inserted at next opportunity.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public static async Task InsertLazy(object Object)
		{
			await Provider.InsertLazy(Object);
			RaiseInserted(Object);
		}

		/// <summary>
		/// Inserts an object into the database, if unlocked. If locked, object will be inserted at next opportunity.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public static async Task InsertLazy(params object[] Objects)
		{
			if (Objects.Length == 1)
				await Provider.InsertLazy(Objects[0]);
			else
				await Provider.InsertLazy(Objects);

			RaiseInserted(Objects);
		}

		/// <summary>
		/// Inserts an object into the database, if unlocked. If locked, object will be inserted at next opportunity.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public static async Task InsertLazy(IEnumerable<object> Objects)
		{
			await Provider.InsertLazy(Objects);
			RaiseInserted(Objects);
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public static Task<IEnumerable<T>> Find<T>(params string[] SortOrder)
			where T : class
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
			where T : class
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
			where T : class
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
			where T : class
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
			where T : class
		{
			return await FirstDeleteRest<T>(await Provider.Find<T>(0, int.MaxValue, SortOrder));
		}

		private static async Task<T> FirstDeleteRest<T>(IEnumerable<T> Set)
			where T : class
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
			where T : class
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
			where T : class
		{
			return FirstIgnoreRest<T>(await Provider.Find<T>(0, 1, SortOrder));
		}

		private static T FirstIgnoreRest<T>(IEnumerable<T> Set)
			where T : class
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
			where T : class
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
			RaiseUpdated(Object);
		}

		private static void RaiseUpdated(object Object)
		{
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

		private static void RaiseUpdated(IEnumerable<object> Objects)
		{
			foreach (object Object in Objects)
				RaiseUpdated(Object);
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
			if (Objects.Length == 1)
				await Provider.Update(Objects[0]);
			else
				await Provider.Update(Objects);

			RaiseUpdated(Objects);
		}

		/// <summary>
		/// Updates a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public async static Task Update(IEnumerable<object> Objects)
		{
			await Provider.Update(Objects);
			RaiseUpdated(Objects);
		}

		/// <summary>
		/// Updates an object in the database, if unlocked. If locked, object will be updated at next opportunity.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public async static Task UpdateLazy(object Object)
		{
			await Provider.UpdateLazy(Object);
			RaiseUpdated(Object);
		}

		/// <summary>
		/// Updates a collection of objects in the database, if unlocked. If locked, objects will be updated at next opportunity.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public async static Task UpdateLazy(params object[] Objects)
		{
			if (Objects.Length == 1)
				await Provider.UpdateLazy(Objects[0]);
			else
				await Provider.UpdateLazy(Objects);

			RaiseUpdated(Objects);
		}

		/// <summary>
		/// Updates a collection of objects in the database, if unlocked. If locked, objects will be updated at next opportunity.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public async static Task UpdateLazy(IEnumerable<object> Objects)
		{
			await Provider.UpdateLazy(Objects);
			RaiseUpdated(Objects);
		}

		/// <summary>
		/// Deletes an object in the database.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public async static Task Delete(object Object)
		{
			await Provider.Delete(Object);
			RaiseDeleted(Object);
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

			RaiseDeleted(Objects);
		}

		/// <summary>
		/// Deletes a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public async static Task Delete(IEnumerable<object> Objects)
		{
			await Provider.Delete(Objects);
			RaiseDeleted(Objects);
		}

		/// <summary>
		/// Deletes an object in the database, if unlocked. If locked, object will be deleted at next opportunity.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public async static Task DeleteLazy(object Object)
		{
			await Provider.DeleteLazy(Object);
			RaiseDeleted(Object);
		}

		/// <summary>
		/// Deletes a collection of objects in the database, if unlocked. If locked, objects will be deleted at next opportunity.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public async static Task DeleteLazy(params object[] Objects)
		{
			if (Objects.Length == 1)
				await Provider.DeleteLazy(Objects[0]);
			else
				await Provider.DeleteLazy(Objects);

			RaiseDeleted(Objects);
		}

		/// <summary>
		/// Deletes a collection of objects in the database, if unlocked. If locked, objects will be deleted at next opportunity.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public async static Task DeleteLazy(IEnumerable<object> Objects)
		{
			await Provider.DeleteLazy(Objects);
			RaiseDeleted(Objects);
		}

		private static void RaiseDeleted(object Object)
		{
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

		private static void RaiseDeleted(IEnumerable<object> Objects)
		{
			foreach (object Object in Objects)
				RaiseDeleted(Object);
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/> and deletes them in the same atomic operation.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public static Task<IEnumerable<T>> FindDelete<T>(params string[] SortOrder)
			where T : class
		{
			return FindDelete<T>(0, int.MaxValue, SortOrder);
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/> and deletes them in the same atomic operation.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public static async Task<IEnumerable<T>> FindDelete<T>(int Offset, int MaxCount, params string[] SortOrder)
			where T : class
		{
			IEnumerable<T> Result = await Provider.FindDelete<T>(Offset, MaxCount, SortOrder);

			foreach (T Object in Result)
				RaiseDeleted(Object);

			return Result;
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/> and deletes them in the same atomic operation.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public static Task<IEnumerable<T>> FindDelete<T>(Filter Filter, params string[] SortOrder)
			where T : class
		{
			return FindDelete<T>(0, int.MaxValue, Filter, SortOrder);
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/> and deletes them in the same atomic operation.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public static async Task<IEnumerable<T>> FindDelete<T>(int Offset, int MaxCount, Filter Filter, params string[] SortOrder)
			where T : class
		{
			IEnumerable<T> Result = await Provider.FindDelete<T>(Offset, MaxCount, Filter, SortOrder);

			foreach (T Object in Result)
				RaiseDeleted(Object);

			return Result;
		}

		/// <summary>
		/// Finds objects in a given collection and deletes them in the same atomic operation.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public static Task<IEnumerable<object>> FindDelete(string Collection, params string[] SortOrder)
		{
			return FindDelete(Collection, 0, int.MaxValue, SortOrder);
		}

		/// <summary>
		/// Finds objects in a given collection and deletes them in the same atomic operation.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public static async Task<IEnumerable<object>> FindDelete(string Collection, int Offset, int MaxCount, params string[] SortOrder)
		{
			IEnumerable<object> Result = await Provider.FindDelete(Collection, Offset, MaxCount, SortOrder);

			foreach (object Object in Result)
				RaiseDeleted(Object);

			return Result;
		}

		/// <summary>
		/// Finds objects in a given collection and deletes them in the same atomic operation.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public static Task<IEnumerable<object>> FindDelete(string Collection, Filter Filter, params string[] SortOrder)
		{
			return FindDelete(Collection, 0, int.MaxValue, Filter, SortOrder);
		}

		/// <summary>
		/// Finds objects in a given collection and deletes them in the same atomic operation.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public static async Task<IEnumerable<object>> FindDelete(string Collection, int Offset, int MaxCount, Filter Filter, params string[] SortOrder)
		{
			IEnumerable<object> Result = await Provider.FindDelete(Collection, Offset, MaxCount, Filter, SortOrder);

			foreach (object Object in Result)
				RaiseDeleted(Object);

			return Result;
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/> and deletes them in the same atomic operation.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public static Task DeleteLazy<T>(params string[] SortOrder)
			where T : class
		{
			return DeleteLazy<T>(0, int.MaxValue, SortOrder);
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/> and deletes them in the same atomic operation.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public static Task DeleteLazy<T>(int Offset, int MaxCount, params string[] SortOrder)
			where T : class
		{
			return Provider.DeleteLazy<T>(Offset, MaxCount, SortOrder);
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/> and deletes them in the same atomic operation.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public static Task DeleteLazy<T>(Filter Filter, params string[] SortOrder)
			where T : class
		{
			return DeleteLazy<T>(0, int.MaxValue, Filter, SortOrder);
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/> and deletes them in the same atomic operation.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public static Task DeleteLazy<T>(int Offset, int MaxCount, Filter Filter, params string[] SortOrder)
			where T : class
		{
			return Provider.DeleteLazy<T>(Offset, MaxCount, Filter, SortOrder);
		}

		/// <summary>
		/// Finds objects in a given collection and deletes them in the same atomic operation.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public static Task DeleteLazy(string Collection, params string[] SortOrder)
		{
			return DeleteLazy(Collection, 0, int.MaxValue, SortOrder);
		}

		/// <summary>
		/// Finds objects in a given collection and deletes them in the same atomic operation.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public static Task DeleteLazy(string Collection, int Offset, int MaxCount, params string[] SortOrder)
		{
			return Provider.DeleteLazy(Collection, Offset, MaxCount, SortOrder);
		}

		/// <summary>
		/// Finds objects in a given collection and deletes them in the same atomic operation.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public static Task DeleteLazy(string Collection, Filter Filter, params string[] SortOrder)
		{
			return DeleteLazy(Collection, 0, int.MaxValue, Filter, SortOrder);
		}

		/// <summary>
		/// Finds objects in a given collection and deletes them in the same atomic operation.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public static Task DeleteLazy(string Collection, int Offset, int MaxCount, Filter Filter, params string[] SortOrder)
		{
			return Provider.DeleteLazy(Collection, Offset, MaxCount, Filter, SortOrder);
		}

		/// <summary>
		/// Tries to load an object given its Object ID <paramref name="ObjectId"/> and its class type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Base type.</typeparam>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>Loaded object, or null if not found.</returns>
		public static Task<T> TryLoadObject<T>(object ObjectId)
			where T : class
		{
			return Provider.TryLoadObject<T>(ObjectId);
		}

		/// <summary>
		/// Tries to load an object given its Object ID <paramref name="ObjectId"/> and its class type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Base type.</typeparam>
		/// <param name="CollectionName">Name of collection in which the object resides.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>Loaded object, or null if not found.</returns>
		public static Task<T> TryLoadObject<T>(string CollectionName, object ObjectId)
			where T : class
		{
			return Provider.TryLoadObject<T>(CollectionName, ObjectId);
		}

		/// <summary>
		/// Tries to load an object given its Object ID <paramref name="ObjectId"/> and its collection name <paramref name="CollectionName"/>.
		/// </summary>
		/// <param name="CollectionName">Name of collection in which the object resides.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>Loaded object, or null if not found.</returns>
		public static Task<object> TryLoadObject(string CollectionName, object ObjectId)
		{
			return Provider.TryLoadObject(CollectionName, ObjectId);
		}

		/// <summary>
		/// Loads an object given its Object ID <paramref name="ObjectId"/> and its base type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Base type.</typeparam>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>Loaded object.</returns>
		public static async Task<T> LoadObject<T>(object ObjectId)
			where T : class
		{
			T Result = await Provider.TryLoadObject<T>(ObjectId);
			if (Result is null)
				throw new KeyNotFoundException("Object not found.");

			return Result;
		}

		/// <summary>
		/// Loads an object given its Object ID <paramref name="ObjectId"/> and its collection name <paramref name="CollectionName"/>.
		/// </summary>
		/// <typeparam name="T">Base type.</typeparam>
		/// <param name="CollectionName">Name of collection in which the object resides.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>Loaded object.</returns>
		public async static Task<T> LoadObject<T>(string CollectionName, object ObjectId)
			where T : class
		{
			T Result = await Provider.TryLoadObject<T>(CollectionName, ObjectId);
			if (Result is null)
				throw new KeyNotFoundException("Object not found.");

			return Result;
		}

		/// <summary>
		/// Loads an object given its Object ID <paramref name="ObjectId"/> and its collection name <paramref name="CollectionName"/>.
		/// </summary>
		/// <param name="CollectionName">Name of collection in which the object resides.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>Loaded object.</returns>
		public static async Task<object> LoadObject(string CollectionName, object ObjectId)
		{
			object Result = await Provider.TryLoadObject(CollectionName, ObjectId);
			if (Result is null)
				throw new KeyNotFoundException("Object not found.");

			return Result;
		}

		/// <summary>
		/// Performs an export of the entire database.
		/// </summary>
		/// <param name="Output">Database will be output to this interface.</param>
		/// <returns>Task object for synchronization purposes.</returns>
		public static Task Export(IDatabaseExport Output)
		{
			return Export(Output, null);
		}

		/// <summary>
		/// Performs an export of the entire database.
		/// </summary>
		/// <param name="Output">Database will be output to this interface.</param>
		/// <param name="CollectionNames">Optional array of collections to export. If null (default), all collections will be exported.</param>
		/// <returns>Task object for synchronization purposes.</returns>
		public static Task Export(IDatabaseExport Output, string[] CollectionNames)
		{
			return Provider.Export(Output, CollectionNames);
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
			KeyValuePair<string, Dictionary<string, FlagSource>>[] Flagged = GetFlaggedCollections();

			string[] Result = await Provider.Repair(Output, XsltPath, ProgramDataFolder, ExportData);

			if (Result.Length > 0)
				RaiseRepaired(Result, Flagged);

			return Result;
		}

		private static void RaiseRepaired(string[] Collections, KeyValuePair<string, Dictionary<string, FlagSource>>[] Flagged)
		{
			FlagSource[] FlaggedCollection;
			CollectionRepairedEventHandler h = CollectionRepaired;

			if (!(h is null))
			{
				foreach (string Collection in Collections)
				{
					FlaggedCollection = null;

					if (!(Flagged is null))
					{
						foreach (KeyValuePair<string, Dictionary<string, FlagSource>> Rec in Flagged)
						{
							if (Rec.Key == Collection)
							{
								FlaggedCollection = new FlagSource[Rec.Value.Count];
								Rec.Value.Values.CopyTo(FlaggedCollection, 0);
								break;
							}
						}
					}

					try
					{
						h(Provider, new CollectionRepairedEventArgs(Collection, FlaggedCollection));
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
		public static event CollectionRepairedEventHandler CollectionRepaired = null;

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
			KeyValuePair<string, Dictionary<string, FlagSource>>[] Flagged = GetFlaggedCollections();

			string[] Result = await Provider.Analyze(Output, XsltPath, ProgramDataFolder, ExportData, Repair);

			if (Repair && Result.Length > 0)
				RaiseRepaired(Result, Flagged);

			return Result;
		}

		private static KeyValuePair<string, Dictionary<string, FlagSource>>[] GetFlaggedCollections()
		{
			KeyValuePair<string, Dictionary<string, FlagSource>>[] Flagged;
			int i;

			lock (toRepair)
			{
				Flagged = new KeyValuePair<string, Dictionary<string, FlagSource>>[toRepair.Count];
				i = 0;

				foreach (KeyValuePair<string, Dictionary<string, FlagSource>> P in toRepair)
					Flagged[i++] = P;

				toRepair.Clear();
			}

			return Flagged;
		}

		/// <summary>
		/// Gets the names of the collections that have been flagged as possibly corrupt.
		/// </summary>
		/// <returns></returns>
		public static string[] GetFlaggedCollectionNames()
		{
			string[] Result;

			lock (toRepair)
			{
				Result = new string[toRepair.Count];
				toRepair.Keys.CopyTo(Result, 0);
			}

			return Result;
		}

		/// <summary>
		/// Is called when reparation of a collection is begin.
		/// </summary>
		/// <param name="Collection">Name of collection.</param>
		public static void BeginRepair(string Collection)
		{
			lock (toRepair)
			{
				toRepair[Collection] = null;
			}
		}

		/// <summary>
		/// Is called when reparation of a collection is ended.
		/// </summary>
		/// <param name="Collection">Name of collection.</param>
		public static void EndRepair(string Collection)
		{
			lock (toRepair)
			{
				toRepair.Remove(Collection);
			}
		}

		/// <summary>
		/// Flags a collection for repairing.
		/// </summary>
		/// <param name="Collection">Collection</param>
		/// <param name="Reason">Reason for flagging collection.</param>
		public static Exception FlagForRepair(string Collection, string Reason)
		{
			InconsistencyException Result = new InconsistencyException(Collection, Reason);
			string StackTrace = Log.CleanStackTrace(Environment.StackTrace);
			string Key = Reason + " | " + StackTrace;

			lock (toRepair)
			{
				if (toRepair.TryGetValue(Collection, out Dictionary<string, FlagSource> PerStackTrace))
				{
					if (!(PerStackTrace is null))
					{
						if (PerStackTrace.TryGetValue(Key, out FlagSource FlagSource))
							FlagSource.Count++;
						else
							PerStackTrace[Key] = new FlagSource(Reason, StackTrace, 1);
					}
				}
				else
					toRepair[Collection] = new Dictionary<string, FlagSource>() { { Key, new FlagSource(Reason, StackTrace, 1) } };
			}

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
		public static Task<IPersistentDictionary> GetDictionary(string Collection)
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
		/// <returns>Object ID, if found, null otherwise.</returns>
		public static Task<object> TryGetObjectId(object Object)
		{
			return Provider.TryGetObjectId(Object);
		}

		/// <summary>
		/// Drops a collection, if it exist.
		/// </summary>
		/// <param name="CollectionName">Name of collection.</param>
		public static Task DropCollection(string CollectionName)
		{
			return Provider.DropCollection(CollectionName);
		}

		/// <summary>
		/// Converts a wildcard string to a regular expression string.
		/// </summary>
		/// <param name="s">String</param>
		/// <param name="Wildcard">Wildcard</param>
		/// <returns>Regular expression</returns>
		public static string WildcardToRegex(string s, string Wildcard)
		{
			string[] Parts = s.Split(new string[] { Wildcard }, StringSplitOptions.None);
			StringBuilder RegEx = new StringBuilder();
			bool First = true;
			int i, j, c;

			foreach (string Part in Parts)
			{
				if (First)
					First = false;
				else
					RegEx.Append(".*");

				i = 0;
				c = Part.Length;
				while (i < c)
				{
					j = Part.IndexOfAny(regexSpecialCharaters, i);
					if (j < i)
					{
						RegEx.Append(Part.Substring(i));
						i = c;
					}
					else
					{
						if (j > i)
							RegEx.Append(Part.Substring(i, j - i));

						RegEx.Append('\\');
						RegEx.Append(Part[j]);

						i = j + 1;
					}
				}
			}

			return RegEx.ToString();
		}

		private static readonly char[] regexSpecialCharaters = new char[] { '\\', '^', '$', '{', '}', '[', ']', '(', ')', '.', '*', '+', '?', '|', '<', '>', '-', '&' };

		/// <summary>
		/// Creates a generalized representation of an object.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>Generalized representation.</returns>
		public static Task<GenericObject> Generalize(object Object)
		{
			return Provider.Generalize(Object);
		}

	}
}
