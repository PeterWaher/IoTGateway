using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence.Attributes;
using Waher.Persistence.LifeCycle;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;

namespace Waher.Persistence.FullTextSearch
{
	/// <summary>
	/// Full-text search module, controlling the life-cycle of the full-text-search engine.
	/// </summary>
	[ModuleDependency(typeof(DatabaseModule))]
	public class FullTextSearchModule : IModule
	{
		private static IPersistentDictionary collectionInformation;
		private static Dictionary<string, CollectionInformation> collections;
		private static Dictionary<Type, TypeInformation> types;
		private static SemaphoreSlim synchObj;

		/// <summary>
		/// Full-text search module, controlling the life-cycle of the full-text-search engine.
		/// </summary>
		public FullTextSearchModule()
		{
		}

		/// <summary>
		/// Starts the module.
		/// </summary>
		public async Task Start()
		{
			collectionInformation = await Database.GetDictionary("FullTextSearchSettings");
			collections = new Dictionary<string, CollectionInformation>();
			types = new Dictionary<Type, TypeInformation>();

			synchObj = new SemaphoreSlim(1);

			Database.ObjectInserted += Database_ObjectInserted;
			Database.ObjectUpdated += Database_ObjectUpdated;
			Database.ObjectDeleted += Database_ObjectDeleted;
		}

		/// <summary>
		/// Stops the module.
		/// </summary>
		public Task Stop()
		{
			Database.ObjectInserted -= Database_ObjectInserted;
			Database.ObjectUpdated -= Database_ObjectUpdated;
			Database.ObjectDeleted -= Database_ObjectDeleted;

			// TODO: Wait for current objects to be finished.

			synchObj.Dispose();
			synchObj = null;

			collectionInformation.Dispose();
			collectionInformation = null;

			collections.Clear();
			collections = null;

			types.Clear();
			types = null;

			return Task.CompletedTask;
		}

		private static async void Database_ObjectInserted(object Sender, ObjectEventArgs e)
		{
			try
			{
				Tuple<CollectionInformation, TypeInformation, GenericObject> P = await Prepare(e.Object);
				if (P is null)
					return;

				CollectionInformation CollectionInfo = P.Item1;
				TypeInformation TypeInfo = P.Item2;
				GenericObject GenObj = P.Item3;
				Dictionary<string, string> IndexableProperties;

				if (GenObj is null)
					IndexableProperties = TypeInfo.GetIndexableProperties(e.Object, CollectionInfo.PropertyNames);
				else
					IndexableProperties = GetIndexableProperties(GenObj, CollectionInfo.PropertyNames);

				Dictionary<string, int> Tokens = Tokenize(IndexableProperties.Values);
				if (Tokens.Count == 0)
					return;


			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Tokenizes a set of strings.
		/// </summary>
		/// <param name="Text">Enumerable set of strings to tokenize.</param>
		/// <returns>Tokens found, with associated counts.</returns>
		public static Dictionary<string, int> Tokenize(IEnumerable<string> Text)
		{
			Dictionary<string, int> Result = new Dictionary<string, int>();
			UnicodeCategory Category;
			StringBuilder sb = new StringBuilder();
			string Token;
			bool First = true;

			foreach (string s in Text)
			{
				if (string.IsNullOrEmpty(s))
					continue;

				foreach (char ch in s.ToLower().Normalize(NormalizationForm.FormD))
				{
					Category = CharUnicodeInfo.GetUnicodeCategory(ch);
					if (Category == UnicodeCategory.NonSpacingMark)
						continue;

					if (char.IsLetterOrDigit(ch))
					{
						sb.Append(ch);
						First = false;
					}
					else
					{
						if (!First)
						{
							Token = sb.ToString();
							sb.Clear();
							First = true;

							if (!Result.TryGetValue(Token, out int Nr))
								Nr = 0;

							Result[Token] = Nr + 1;
						}
					}
				}

				if (!First)
				{
					Token = sb.ToString();
					sb.Clear();
					First = true;

					if (!Result.TryGetValue(Token, out int Nr))
						Nr = 0;

					Result[Token] = Nr + 1;
				}
			}

			return Result;
		}

		/// <summary>
		/// Gets the indexable property values from an object. Property values will be returned in lower-case.
		/// </summary>
		/// <param name="Obj">Generic object.</param>
		/// <param name="PropertyNames">Indexable property names.</param>
		/// <returns>Indexable property values found.</returns>
		public static async Task<Dictionary<string, string>> GetIndexableProperties(object Obj, params string[] PropertyNames)
		{
			if (Obj is null)
				return new Dictionary<string, string>();
			else if (Obj is GenericObject GenObj)
				return GetIndexableProperties(GenObj, PropertyNames);
			else
			{
				await synchObj.WaitAsync();
				try
				{
					TypeInformation TypeInfo = await GetTypeInfoLocked(Obj.GetType());
					return TypeInfo.GetIndexableProperties(Obj, PropertyNames);
				}
				finally
				{
					synchObj.Release();
				}
			}
		}

		/// <summary>
		/// Gets the indexable property values from an object. Property values will be returned in lower-case.
		/// </summary>
		/// <param name="Obj">Generic object.</param>
		/// <param name="PropertyNames">Indexable property names.</param>
		/// <returns>Indexable property values found.</returns>
		public static Dictionary<string, string> GetIndexableProperties(GenericObject Obj, params string[] PropertyNames)
		{
			Dictionary<string, string> Result = new Dictionary<string, string>();

			if (!(Obj is null))
			{
				foreach (string PropertyName in PropertyNames)
				{
					if (Obj.TryGetFieldValue(PropertyName, out object Value))
					{
						if (Value is string s)
							Result[PropertyName] = s.ToLower();
						else if (Value is CaseInsensitiveString cis)
							Result[PropertyName] = cis.LowerCase;
					}
				}
			}

			return Result;
		}

		private static async Task<CollectionInformation> GetCollectionInfoLocked(string CollectionName, bool CreateIfNotExists)
		{
			if (collections.TryGetValue(CollectionName, out CollectionInformation Result))
				return Result;

			KeyValuePair<bool, object> P = await collectionInformation.TryGetValueAsync(CollectionName);
			if (P.Key && P.Value is CollectionInformation Result2)
			{
				collections[CollectionName] = Result2;
				return Result2;
			}

			if (!CreateIfNotExists)
				return null;

			Result = new CollectionInformation(CollectionName, CollectionName, false);
			collections[CollectionName] = Result;
			await collectionInformation.AddAsync(CollectionName, Result, true);

			return Result;
		}

		/// <summary>
		/// Defines the Full-text-search index collection name, for objects in a given collection.
		/// </summary>
		/// <param name="IndexCollection">Collection name for full-text-search index of objects in the given collection.</param>
		/// <param name="CollectionName">Collection of objects to index.</param>
		public static async Task SetFullTextSearchIndexCollection(string IndexCollection, string CollectionName)
		{
			await synchObj.WaitAsync();
			try
			{
				CollectionInformation Info = await GetCollectionInfoLocked(CollectionName, true);

				if (Info.IndexCollectionName != IndexCollection)
				{
					Info.IndexCollectionName = IndexCollection;
					await collectionInformation.AddAsync(Info.CollectionName, Info, true);
				}
			}
			finally
			{
				synchObj.Release();
			}
		}

		/// <summary>
		/// Adds properties for full-text-search indexation.
		/// </summary>
		/// <param name="CollectionName">Collection name.</param>
		/// <param name="Properties">Properties to index.</param>
		/// <returns>If new property names were found and added.</returns>
		public static async Task<bool> AddFullTextSearch(string CollectionName, params string[] Properties)
		{
			await synchObj.WaitAsync();
			try
			{
				CollectionInformation Info = await GetCollectionInfoLocked(CollectionName, true);

				if (Info.AddIndexableProperties(Properties))
				{
					await collectionInformation.AddAsync(Info.CollectionName, Info, true);
					return true;
				}
				else
					return false;
			}
			finally
			{
				synchObj.Release();
			}
		}

		/// <summary>
		/// Removes properties from full-text-search indexation.
		/// </summary>
		/// <param name="CollectionName">Collection name.</param>
		/// <param name="Properties">Properties to remove from indexation.</param>
		/// <returns>If property names were found and removed.</returns>
		public static async Task<bool> RemoveFullTextSearch(string CollectionName, params string[] Properties)
		{
			await synchObj.WaitAsync();
			try
			{
				CollectionInformation Info = await GetCollectionInfoLocked(CollectionName, true);

				if (Info.RemoveIndexableProperties(Properties))
				{
					await collectionInformation.AddAsync(Info.CollectionName, Info, true);
					return true;
				}
				else
					return false;
			}
			finally
			{
				synchObj.Release();
			}
		}

		/// <summary>
		/// Gets indexed properties for full-text-search indexation.
		/// </summary>
		/// <param name="CollectionName">Collection name.</param>
		/// <returns>Array of indexed properties.</returns>
		public static async Task<string[]> GetFullTextSearchIndexedProperties(string CollectionName)
		{
			await synchObj.WaitAsync();
			try
			{
				CollectionInformation Info = await GetCollectionInfoLocked(CollectionName, false);

				if (Info is null || !Info.IndexForFullTextSearch)
					return new string[0];
				else
					return (string[])Info.PropertyNames.Clone();
			}
			finally
			{
				synchObj.Release();
			}
		}

		private static async Task<Tuple<CollectionInformation, TypeInformation, GenericObject>> Prepare(object Object)
		{
			await synchObj.WaitAsync();
			try
			{
				if (Object is GenericObject GenObj)
					return await PrepareLocked(GenObj);
				else
					return await PrepareLocked(Object.GetType());
			}
			finally
			{
				synchObj.Release();
			}
		}

		private static async Task<Tuple<CollectionInformation, TypeInformation, GenericObject>> PrepareLocked(GenericObject GenObj)
		{
			CollectionInformation CollectionInfo = await GetCollectionInfoLocked(GenObj.CollectionName, true);

			if (CollectionInfo.IndexForFullTextSearch)
				return new Tuple<CollectionInformation, TypeInformation, GenericObject>(CollectionInfo, null, GenObj);
			else
				return null;
		}

		private static async Task<TypeInformation> GetTypeInfoLocked(Type T)
		{
			if (types.TryGetValue(T, out TypeInformation Result))
				return Result;

			TypeInfo TI = T.GetTypeInfo();
			FullTextSearchAttribute SearchAttr = TI.GetCustomAttribute<FullTextSearchAttribute>(true);
			CollectionNameAttribute CollectionAttr = TI.GetCustomAttribute<CollectionNameAttribute>(true);

			if (CollectionAttr is null)
				Result = new TypeInformation(T, TI, null, null);
			else
			{
				string CollectionName = CollectionAttr.Name;
				CollectionInformation Info = await GetCollectionInfoLocked(CollectionName, true);

				Result = new TypeInformation(T, TI, CollectionName, Info);

				if (!(SearchAttr is null) && Info.AddIndexableProperties(SearchAttr.PropertyNames))
					await collectionInformation.AddAsync(CollectionName, Info, true);
			}

			types[T] = Result;

			return Result;
		}

		private static async Task<Tuple<CollectionInformation, TypeInformation, GenericObject>> PrepareLocked(Type T)
		{
			TypeInformation TypeInfo = await GetTypeInfoLocked(T);
			if (!TypeInfo.HasCollection)
				return null;

			if (!TypeInfo.CollectionInformation?.IndexForFullTextSearch ?? false)
				return null;

			return new Tuple<CollectionInformation, TypeInformation, GenericObject>(TypeInfo.CollectionInformation, TypeInfo, null);
		}

		private static async void Database_ObjectUpdated(object Sender, ObjectEventArgs e)
		{
			try
			{
				Tuple<CollectionInformation, TypeInformation, GenericObject> P = await Prepare(e.Object);
				if (P is null)
					return;

			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private static async void Database_ObjectDeleted(object Sender, ObjectEventArgs e)
		{
			try
			{
				Tuple<CollectionInformation, TypeInformation, GenericObject> P = await Prepare(e.Object);
				if (P is null)
					return;

			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}
	}
}
