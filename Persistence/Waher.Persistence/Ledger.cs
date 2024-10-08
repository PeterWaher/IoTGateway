using System;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence.Serialization;
using Waher.Runtime.Profiling;

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
			if (!(provider is null))
			{
				if (locked)
					throw new Exception("A ledger provider is already registered.");

				provider.Unregister(externalEvents);
			}

			provider = LedgerProvider;
			locked = Lock;

			provider.Register(externalEvents);
		}

		private static readonly ExternalEvents externalEvents = new ExternalEvents();

		private class ExternalEvents : ILedgerExternalEvents
		{
			public void RaiseEntryAdded(object Object)
			{
				Ledger.RaiseEntryAdded(Object);
			}

			public void RaiseEntryDeleted(object Object)
			{
				Ledger.RaiseEntryDeleted(Object);
			}

			public void RaiseEntryUpdated(object Object)
			{
				Ledger.RaiseEntryUpdated(Object);
			}

			public void RaiseCollectionCleared(string Collection)
			{
				Ledger.RaiseCollectionCleared(Collection);
			}
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

		internal static ILedgerProvider Stop()
		{
			ILedgerProvider Result = provider;
			provider = new NullLedgerProvider();
			locked = false;
			return Result;
		}

		/// <summary>
		/// If a ledger provider is registered.
		/// </summary>
		public static bool HasProvider
		{
			get => !(provider is null) && (!(provider is NullLedgerProvider) || locked);
		}

		/// <summary>
		/// If the datbase provider has been locked for the rest of the run-time of the application.
		/// </summary>
		public static bool Locked => locked;

		/// <summary>
		/// Adds an entry to the ledger.
		/// </summary>
		/// <param name="Object">New object.</param>
		public static async Task NewEntry(object Object)
		{
			await Provider.NewEntry(Object);
			RaiseEntryAdded(Object);
		}

		private static void RaiseEntryAdded(object Object)
		{
			ObjectEventHandler h = EntryAdded;
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
		/// Event raised when an entry has been added to the ledger.
		/// </summary>
		public static event ObjectEventHandler EntryAdded = null;

		/// <summary>
		/// Updates an entry in the ledger.
		/// </summary>
		/// <param name="Object">Updated object.</param>
		public static async Task UpdatedEntry(object Object)
		{
			await Provider.UpdatedEntry(Object);
			RaiseEntryUpdated(Object);
		}

		private static void RaiseEntryUpdated(object Object)
		{
			ObjectEventHandler h = EntryUpdated;
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
		/// Event raised when an entry has been updated in the ledger.
		/// </summary>
		public static event ObjectEventHandler EntryUpdated = null;

		/// <summary>
		/// Deletes an entry in the ledger.
		/// </summary>
		/// <param name="Object">Deleted object.</param>
		public static async Task DeletedEntry(object Object)
		{
			await Provider.DeletedEntry(Object);
			RaiseEntryDeleted(Object);
		}

		private static void RaiseEntryDeleted(object Object)
		{
			ObjectEventHandler h = EntryDeleted;
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
		/// Event raised when an entry has been deleted in the ledger.
		/// </summary>
		public static event ObjectEventHandler EntryDeleted = null;

		/// <summary>
		/// Clears a collection in the ledger.
		/// </summary>
		/// <param name="Collection">Cleared collection.</param>
		public static async Task ClearedCollection(string Collection)
		{
			await Provider.ClearedCollection(Collection);
			RaiseCollectionCleared(Collection);
		}

		private static async void RaiseCollectionCleared(string Collection)
		{
			CollectionEventHandler h = CollectionCleared;
			if (!(h is null))
			{
				try
				{
					await h(Provider, new CollectionEventArgs(Collection));
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}
		}

		/// <summary>
		/// Event raised when a collection has been cleared.
		/// </summary>
		public static event CollectionEventHandler CollectionCleared = null;

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

		/// <summary>
		/// Performs an export of the entire ledger.
		/// </summary>
		/// <param name="Output">Ledger will be output to this interface.</param>
		/// <returns>If export process was completed (true), or terminated by <paramref name="Output"/> (false).</returns>
		public static Task<bool> Export(ILedgerExport Output)
		{
			return Provider.Export(Output, null);
		}

		/// <summary>
		/// Performs an export of the entire ledger.
		/// </summary>
		/// <param name="Output">Ledger will be output to this interface.</param>
		/// <param name="CollectionNames">Optional array of collections to export. If null, all collections will be exported.</param>
		/// <returns>If export process was completed (true), or terminated by <paramref name="Output"/> (false).</returns>
		public static Task<bool> Export(ILedgerExport Output, string[] CollectionNames)
		{
			return Provider.Export(Output, CollectionNames);
		}

		/// <summary>
		/// Performs an export of the entire ledger.
		/// </summary>
		/// <param name="Output">Ledger will be output to this interface.</param>
		/// <param name="CollectionNames">Optional array of collections to export. If null, all collections will be exported.</param>
		/// <param name="Thread">Optional Profiler thread.</param>
		/// <returns>If export process was completed (true), or terminated by <paramref name="Output"/> (false).</returns>
		public static Task<bool> Export(ILedgerExport Output, string[] CollectionNames, ProfilerThread Thread)
		{
			return Provider.Export(Output, CollectionNames, Thread);
		}

		private static readonly object listeningSynchObj = new object();
		private static int listeningCounter = 0;

		/// <summary>
		/// Makes the ledger listen on database events.
		/// Each call to <see cref="StartListeningToDatabaseEvents"/> must be followed by a call
		/// to <see cref="StopListeningToDatabaseEvents"/> when listening should stop.
		/// </summary>
		public static void StartListeningToDatabaseEvents()
		{
			lock (listeningSynchObj)
			{
				if (listeningCounter == 0)
				{
					Database.ObjectInserted += Database_ObjectInserted;
					Database.ObjectUpdated += Database_ObjectUpdated;
					Database.ObjectDeleted += Database_ObjectDeleted;
					Database.CollectionCleared += Database_CollectionCleared;

					listeningCounter++;
				}
			}

		}

		/// <summary>
		/// Makes the ledger listen on database events.
		/// Each call to <see cref="StartListeningToDatabaseEvents"/> must be followed by a call
		/// to <see cref="StopListeningToDatabaseEvents"/> when listening should stop.
		/// </summary>
		public static void StopListeningToDatabaseEvents()
		{
			lock (listeningSynchObj)
			{
				if (listeningCounter > 0)
				{
					listeningCounter--;

					if (listeningCounter == 0)
					{
						Database.ObjectInserted -= Database_ObjectInserted;
						Database.ObjectUpdated -= Database_ObjectUpdated;
						Database.ObjectDeleted -= Database_ObjectDeleted;
						Database.CollectionCleared -= Database_CollectionCleared;
					}
				}
			}
		}

		private static async void Database_ObjectInserted(object Sender, ObjectEventArgs e)
		{
			try
			{
				GenericObject Obj = await Database.Generalize(e.Object);
				await NewEntry(Obj);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private static async void Database_ObjectUpdated(object Sender, ObjectEventArgs e)
		{
			try
			{
				GenericObject Obj = await Database.Generalize(e.Object);
				await UpdatedEntry(Obj);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private static async void Database_ObjectDeleted(object Sender, ObjectEventArgs e)
		{
			try
			{
				GenericObject Obj = await Database.Generalize(e.Object);
				await DeletedEntry(Obj);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private static async Task Database_CollectionCleared(object Sender, CollectionEventArgs e)
		{
			try
			{
				await ClearedCollection(e.Collection);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}
	}
}
