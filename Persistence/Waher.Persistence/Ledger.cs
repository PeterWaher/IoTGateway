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
	/// Static interface for ledger persistence. In order to work, a ledger provider has to be assigned to it. This is
	/// ideally done as one of the first steps in the startup of an application.
	/// </summary>
	public static class Ledger
	{
		private static ILedgerProvider provider = null;
		private static bool locked = false;

		/// <summary>
		/// Registers a ledger provider for use from the static <see cref="Ledger"/> class, 
		/// throughout the lifetime of the application.
		/// 
		/// Note: Only one ledger provider can be registered.
		/// </summary>
		/// <param name="LedgerProvider">Ledger provider to use.</param>
		public static void Register(ILedgerProvider LedgerProvider)
		{
			Register(LedgerProvider, true);
		}

		/// <summary>
		/// Registers a ledger provider for use from the static <see cref="Ledger"/> class, 
		/// throughout the lifetime of the application.
		/// 
		/// Note: Only one ledger provider can be registered.
		/// </summary>
		/// <param name="LedgerProvider">Ledger provider to use.</param>
		/// <param name="Lock">If the ledger provider should be locked for the rest of the running time of the application.</param>
		public static void Register(ILedgerProvider LedgerProvider, bool Lock)
		{
			if (provider != null && locked)
				throw new Exception("A ledger provider is already registered.");

			provider = LedgerProvider;
			locked = Lock;
		}

		/// <summary>
		/// Registered ledger provider.
		/// </summary>
		public static ILedgerProvider Provider
		{
			get
			{
				if (!(provider is null))
					return provider;
				else
					throw new Exception("A ledger provider has not been registered.");
			}
		}

		/// <summary>
		/// If a ledger provider is registered.
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
		/// Adds an entry to the ledger.
		/// </summary>
		/// <param name="Object">New object.</param>
		public static Task NewEntry(object Object)
		{
			return Provider.NewEntry(Object);
		}

		/// <summary>
		/// Updates an entry in the ledger.
		/// </summary>
		/// <param name="Object">Updated object.</param>
		public static Task UpdatedEntry(object Object)
		{
			return Provider.UpdatedEntry(Object);
		}

		/// <summary>
		/// Deletes an entry in the ledger.
		/// </summary>
		/// <param name="Object">Deleted object.</param>
		public static Task DeletedEntry(object Object)
		{
			return Provider.DeletedEntry(Object);
		}

		/// <summary>
		/// Gets an eumerator for objects of type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Type of object entries to enumerate.</typeparam>
		/// <returns>Enumerator object.</returns>
		public static Task<ILedgerEnumerator<T>> GetEnumerator<T>()
		{
			return Provider.GetEnumerator<T>();
		}

		/// <summary>
		/// Gets an eumerator for objects in a collection.
		/// </summary>
		/// <param name="CollectionName">Collection to enumerate.</param>
		/// <returns>Enumerator object.</returns>
		public static Task<ILedgerEnumerator<object>> GetEnumerator(string CollectionName)
		{
			return Provider.GetEnumerator(CollectionName);
		}

		/// <summary>
		/// Gets an array of available collections.
		/// </summary>
		/// <returns>Array of collections.</returns>
		public static Task<string[]> GetCollections()
		{
			return Provider.GetCollections();
		}

	}
}
