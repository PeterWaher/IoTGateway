using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Filters;

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
		public static void Insert(object Object)
		{
			Provider.Insert(Object);
		}

		/// <summary>
		/// Inserts a set of objects into the default collection of the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public static void Insert(params object[] Objects)
		{
			Provider.Insert(Objects);
		}

		/// <summary>
		/// Inserts a set of objects into the default collection of the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public static void Insert(IEnumerable<object> Objects)
		{
			Provider.Insert(Objects);
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
			return Provider.Find<T>(SortOrder);
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
			return Provider.Find<T>(Filter, SortOrder);
		}

		/// <summary>
		/// Finds the first object of a given class <typeparamref name="T"/> and deletes the rest.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		///	<exception cref="TimeoutException">Thrown if a response is not returned from the database within the given number of milliseconds.</exception>
		public static T FindFirstDeleteRest<T>(int Timeout, params string[] SortOrder)
		{
			return FirstDeleteRest<T>(Timeout, Provider.Find<T>(SortOrder));
		}

		private static T FirstDeleteRest<T>(int Timeout, Task<IEnumerable<T>> Set)
		{
			if (!Set.Wait(Timeout))
				throw new TimeoutException();

			T Result = default(T);
			bool First = true;

			foreach (T Obj in Set.Result)
			{
				if (First)
				{
					First = false;
					Result = Obj;
				}
				else
					Database.Delete(Obj);
			}

			return Result;
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		///	<exception cref="TimeoutException">Thrown if a response is not returned from the database within the given number of milliseconds.</exception>
		public static T FindFirstDeleteRest<T>(int Timeout, Filter Filter, params string[] SortOrder)
		{
			return FirstDeleteRest<T>(Timeout, Provider.Find<T>(Filter, SortOrder));
		}

		/// <summary>
		/// Updates an object in the database.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public static void Update(object Object)
		{
			Provider.Update(Object);
		}

		/// <summary>
		/// Updates a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public static void Update(params object[] Objects)
		{
			Provider.Update(Objects);
		}

		/// <summary>
		/// Updates a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public static void Update(IEnumerable<object> Objects)
		{
			Provider.Update(Objects);
		}

		/// <summary>
		/// Deletes an object in the database.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public static void Delete(object Object)
		{
			Provider.Delete(Object);
		}

		/// <summary>
		/// Deletes a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public static void Delete(params object[] Objects)
		{
			Provider.Delete(Objects);
		}

		/// <summary>
		/// Deletes a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public static void Delete(IEnumerable<object> Objects)
		{
			Provider.Delete(Objects);
		}

	}
}
