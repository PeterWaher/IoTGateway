using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

	}
}
