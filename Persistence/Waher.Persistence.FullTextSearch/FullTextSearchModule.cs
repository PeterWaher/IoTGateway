using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence.Attributes;
using Waher.Persistence.Filters;
using Waher.Persistence.FullTextSearch.KeywordEnumerators;
using Waher.Persistence.FullTextSearch.Orders;
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
		private static Dictionary<string, IPersistentDictionary> indices;
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
			collectionInformation = await Database.GetDictionary("FullTextSearchCollections");
			collections = new Dictionary<string, CollectionInformation>();
			indices = new Dictionary<string, IPersistentDictionary>();
			types = new Dictionary<Type, TypeInformation>();

			synchObj = new SemaphoreSlim(1);

			Database.ObjectInserted += this.Database_ObjectInserted;
			Database.ObjectUpdated += this.Database_ObjectUpdated;
			Database.ObjectDeleted += this.Database_ObjectDeleted;
		}

		/// <summary>
		/// Stops the module.
		/// </summary>
		public async Task Stop()
		{
			Database.ObjectInserted -= this.Database_ObjectInserted;
			Database.ObjectUpdated -= this.Database_ObjectUpdated;
			Database.ObjectDeleted -= this.Database_ObjectDeleted;

			// TODO: Wait for current objects to be finished.

			await synchObj.WaitAsync();
			try
			{
				foreach (IPersistentDictionary Index in indices.Values)
					Index.Dispose();

				indices.Clear();
				indices = null;

				collectionInformation.Dispose();
				collectionInformation = null;

				collections.Clear();
				collections = null;

				types.Clear();
				types = null;
			}
			finally
			{
				synchObj.Release();
				synchObj.Dispose();
				synchObj = null;
			}
		}

		private async void Database_ObjectInserted(object Sender, ObjectEventArgs e)
		{
			try
			{
				Tuple<CollectionInformation, TypeInformation, GenericObject> P = await Prepare(e.Object);
				if (P is null)
					return;

				object ObjectId = await Database.TryGetObjectId(e.Object);
				if (ObjectId is null)
					return;

				CollectionInformation CollectionInfo = P.Item1;
				TypeInformation TypeInfo = P.Item2;
				GenericObject GenObj = P.Item3;
				Dictionary<string, string> IndexableProperties;

				if (GenObj is null)
					IndexableProperties = TypeInfo.GetIndexableProperties(e.Object, CollectionInfo.PropertyNames);
				else
					IndexableProperties = GetIndexableProperties(GenObj, CollectionInfo.PropertyNames);

				if (IndexableProperties.Count == 0)
					return;

				TokenCount[] Tokens = Tokenize(IndexableProperties.Values);
				if (Tokens is null)
					return;

				ObjectReference Ref;

				await synchObj.WaitAsync();
				try
				{
					ulong Index = await GetNextIndexNrLocked(CollectionInfo.IndexCollectionName);

					Ref = new ObjectReference()
					{
						IndexCollection = CollectionInfo.IndexCollectionName,
						Collection = CollectionInfo.CollectionName,
						ObjectInstanceId = ObjectId,
						Index = Index,
						Tokens = Tokens
					};

					await AddTokensToIndexLocked(Ref);
					await Database.Insert(Ref);
				}
				finally
				{
					synchObj.Release();
				}

				await Search.RaiseObjectAddedToIndex(this, new ObjectReferenceEventArgs(Ref));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private static async Task<IPersistentDictionary> GetIndexLocked(string IndexCollection)
		{
			if (indices.TryGetValue(IndexCollection, out IPersistentDictionary Result))
				return Result;

			Result = await Database.GetDictionary(IndexCollection);
			indices[IndexCollection] = Result;

			return Result;
		}

		private static async Task AddTokensToIndexLocked(ObjectReference Ref)
		{
			DateTime TP = DateTime.UtcNow;
			IPersistentDictionary Index = await GetIndexLocked(Ref.IndexCollection);

			foreach (TokenCount Token in Ref.Tokens)
			{
				KeyValuePair<bool, object> P = await Index.TryGetValueAsync(Token.Token);
				int c;

				if (!P.Key || !(P.Value is TokenReferences References))
				{
					References = new TokenReferences()
					{
						LastBlock = 0,
						ObjectReferences = new ulong[] { Ref.Index },
						Counts = new uint[] { Token.Count },
						Timestamps = new DateTime[] { TP }
					};

					await Index.AddAsync(Token.Token, References, true);
				}
				else if ((c = References.ObjectReferences.Length) < TokenReferences.MaxReferences)
				{
					ulong[] NewReferences = new ulong[c + 1];
					uint[] NewCounts = new uint[c + 1];
					DateTime[] NewTimestamps = new DateTime[c + 1];

					Array.Copy(References.ObjectReferences, 0, NewReferences, 0, c);
					Array.Copy(References.Counts, 0, NewCounts, 0, c);
					Array.Copy(References.Timestamps, 0, NewTimestamps, 0, c);

					NewReferences[c] = Ref.Index;
					NewCounts[c] = Token.Count;
					NewTimestamps[c] = TP;

					References.ObjectReferences = NewReferences;
					References.Counts = NewCounts;
					References.Timestamps = NewTimestamps;

					await Index.AddAsync(Token.Token, References, true);
				}
				else
				{
					References.LastBlock++;

					TokenReferences NewBlock = new TokenReferences()
					{
						LastBlock = 0,
						Counts = References.Counts,
						ObjectReferences = References.ObjectReferences,
						Timestamps = References.Timestamps
					};

					await Index.AddAsync(Token + " " + References.LastBlock.ToString(), NewBlock, true);

					References.ObjectReferences = new ulong[] { Ref.Index };
					References.Counts = new uint[] { Token.Count };
					References.Timestamps = new DateTime[] { TP };

					await Index.AddAsync(Token.Token, References, true);
				}

				Token.Block = References.LastBlock + 1;
			}
		}

		private static async Task<ulong> GetNextIndexNrLocked(string IndexedCollection)
		{
			string Key = " C(" + IndexedCollection + ")";
			KeyValuePair<bool, object> P = await collectionInformation.TryGetValueAsync(Key);

			if (!P.Key || !(P.Value is ulong Nr))
				Nr = 0;

			Nr++;

			await collectionInformation.AddAsync(Key, Nr, true);

			return Nr;
		}

		/// <summary>
		/// Tokenizes a set of strings.
		/// </summary>
		/// <param name="Text">Enumerable set of strings to tokenize.</param>
		/// <returns>Tokens found, with associated counts.</returns>
		internal static TokenCount[] Tokenize(IEnumerable<string> Text)
		{
			Dictionary<string, uint> Result = new Dictionary<string, uint>();
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

							if (!Result.TryGetValue(Token, out uint Nr))
								Result[Token] = 1;
							else if (Nr < uint.MaxValue)
								Result[Token] = Nr + 1;
						}
					}
				}

				if (!First)
				{
					Token = sb.ToString();
					sb.Clear();
					First = true;

					if (!Result.TryGetValue(Token, out uint Nr))
						Result[Token] = 1;
					else if (Nr < uint.MaxValue)
						Result[Token] = Nr + 1;
				}
			}

			int c = Result.Count;
			if (c == 0)
				return null;

			int i = 0;
			TokenCount[] Counts = new TokenCount[c];

			foreach (KeyValuePair<string, uint> P in Result)
				Counts[i++] = new TokenCount(P.Key, P.Value);

			return Counts;
		}

		/// <summary>
		/// Gets the indexable property values from an object. Property values will be returned in lower-case.
		/// </summary>
		/// <param name="Obj">Generic object.</param>
		/// <param name="PropertyNames">Indexable property names.</param>
		/// <returns>Indexable property values found.</returns>
		internal static async Task<Dictionary<string, string>> GetIndexableProperties(object Obj, params string[] PropertyNames)
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
		internal static Dictionary<string, string> GetIndexableProperties(GenericObject Obj, params string[] PropertyNames)
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

		private static Task<CollectionInformation> GetCollectionInfoLocked(string CollectionName, bool CreateIfNotExists)
		{
			return GetCollectionInfoLocked(CollectionName, CollectionName, CreateIfNotExists);
		}

		private static async Task<CollectionInformation> GetCollectionInfoLocked(
			string IndexCollectionName, string CollectionName, bool CreateIfNotExists)
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

			Result = new CollectionInformation(IndexCollectionName, CollectionName, false);
			collections[CollectionName] = Result;
			await collectionInformation.AddAsync(CollectionName, Result, true);

			return Result;
		}

		/// <summary>
		/// Defines the Full-text-search index collection name, for objects in a given collection.
		/// </summary>
		/// <param name="IndexCollection">Collection name for full-text-search index of objects in the given collection.</param>
		/// <param name="CollectionName">Collection of objects to index.</param>
		internal static async Task SetFullTextSearchIndexCollection(string IndexCollection, string CollectionName)
		{
			await synchObj.WaitAsync();
			try
			{
				CollectionInformation Info = await GetCollectionInfoLocked(IndexCollection, CollectionName, true);

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
		internal static async Task<bool> AddFullTextSearch(string CollectionName, params string[] Properties)
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
		internal static async Task<bool> RemoveFullTextSearch(string CollectionName, params string[] Properties)
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
		internal static async Task<string[]> GetFullTextSearchIndexedProperties(string CollectionName)
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
				CollectionInformation Info = await GetCollectionInfoLocked(SearchAttr?.IndexCollection ?? CollectionName, CollectionName, true);

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

		/// <summary>
		/// Performs a Full-Text-Search
		/// </summary>
		/// <param name="IndexCollection">Index collection name.</param>
		/// <param name="Offset">Index of first object matching the keywords.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Keywords">Keywords to search for.</param>
		/// <returns>Array of objects</returns>
		internal static async Task<T[]> FullTextSearch<T>(string IndexCollection,
			int Offset, int MaxCount, FullTextSearchOrder Order, params string[] Keywords)
			where T : class
		{
			if (MaxCount <= 0)
				return new T[0];

			TokenCount[] Tokens = Tokenize(Keywords);
			int NrTokens = Tokens.Length;

			if (NrTokens == 0)
				return new T[0];

			Array.Sort(Tokens, descendingLengthOrder);

			Dictionary<ulong, LinkedList<TokenReference>> ReferencesByObject;

			await synchObj.WaitAsync();
			try
			{
				IPersistentDictionary Index = await GetIndexLocked(IndexCollection);

				ReferencesByObject = new Dictionary<ulong, LinkedList<TokenReference>>();

				foreach (TokenCount Token in Tokens)
				{
					TokenReferenceEnumerator e;

					switch (Order)
					{
						case FullTextSearchOrder.Relevance:
						case FullTextSearchOrder.Newest:
						case FullTextSearchOrder.Occurrences:
						default:
							e = new TokenReferencesNewToOld(Index, Token.Token);
							break;

						case FullTextSearchOrder.Oldest:
							e = new TokenReferencesOldToNew(Index, Token.Token);
							break;
					}

					while (await e.MoveNextAsync())
					{
						TokenReference Ref = e.Current;

						if (!ReferencesByObject.TryGetValue(Ref.ObjectReference, out LinkedList<TokenReference> References))
						{
							References = new LinkedList<TokenReference>();
							ReferencesByObject[Ref.ObjectReference] = References;
						}

						References.AddLast(Ref);
					}
				}
			}
			finally
			{
				synchObj.Release();
			}

			int c = ReferencesByObject.Count;
			LinkedList<TokenReference>[] FoundReferences = new LinkedList<TokenReference>[c];
			ReferencesByObject.Values.CopyTo(FoundReferences, 0);

			switch (Order)
			{
				case FullTextSearchOrder.Relevance:
				default:
					Array.Sort(FoundReferences, relevanceOrder);
					break;

				case FullTextSearchOrder.Occurrences:
					Array.Sort(FoundReferences, occurrencesOrder);
					break;

				case FullTextSearchOrder.Newest:
					Array.Sort(FoundReferences, newestOrder);
					break;

				case FullTextSearchOrder.Oldest:
					Array.Sort(FoundReferences, oldestOrder);
					break;
			}

			List<T> Result = new List<T>();

			foreach (LinkedList<TokenReference> ObjectReference in FoundReferences)
			{
				ulong RefIndex = ObjectReference.First.Value.ObjectReference;
				ObjectReference Ref = await Database.FindFirstIgnoreRest<ObjectReference>(new FilterAnd(
					new FilterFieldEqualTo("IndexCollection", IndexCollection),
					new FilterFieldEqualTo("Index", RefIndex)));

				if (Ref is null)
					continue;

				T Object = await Database.TryLoadObject<T>(Ref.Collection, Ref.ObjectInstanceId);
				if (Object is null)
					continue;

				if (Offset > 0)
				{
					Offset--;
					continue;
				}

				Result.Add(Object);
				MaxCount--;

				if (MaxCount <= 0)
					break;
			}

			return Result.ToArray();
		}

		private static readonly DescendingLengthOrder descendingLengthOrder = new DescendingLengthOrder();
		private static readonly RelevanceOrder relevanceOrder = new RelevanceOrder();
		private static readonly OccurrencesOrder occurrencesOrder = new OccurrencesOrder();
		private static readonly NewestOrder newestOrder = new NewestOrder();
		private static readonly OldestOrder oldestOrder = new OldestOrder();

		private async void Database_ObjectDeleted(object Sender, ObjectEventArgs e)
		{
			try
			{
				Tuple<CollectionInformation, TypeInformation, GenericObject> P = await Prepare(e.Object);
				if (P is null)
					return;

				object ObjectId = await Database.TryGetObjectId(e.Object);
				if (ObjectId is null)
					return;

				ObjectReference Ref = await Database.FindFirstIgnoreRest<ObjectReference>(new FilterAnd(
					new FilterFieldEqualTo("Collection", P.Item1.CollectionName),
					new FilterFieldEqualTo("ObjectInstanceId", ObjectId)));

				if (Ref is null)
					return;

				await synchObj.WaitAsync();
				try
				{
					await RemoveTokensFromIndexLocked(Ref);
				}
				finally
				{
					synchObj.Release();
				}

				await Search.RaiseObjectRemovedFromIndex(this, new ObjectReferenceEventArgs(Ref));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private static async Task RemoveTokensFromIndexLocked(ObjectReference Ref)
		{
			IPersistentDictionary Index = await GetIndexLocked(Ref.IndexCollection);

			foreach (TokenCount Token in Ref.Tokens)
			{
				string Suffix = " " + Token.Block.ToString();
				KeyValuePair<bool, object> P = await Index.TryGetValueAsync(Token.Token + Suffix);

				if (!P.Key)
					P = await Index.TryGetValueAsync(Token.Token);

				if (!P.Key || !(P.Value is TokenReferences References))
					continue;

				int i = Array.IndexOf(References.ObjectReferences, Ref.Index);
				if (i < 0)
					continue;

				int c = References.ObjectReferences.Length;
				ulong[] NewReferences = new ulong[c - 1];
				uint[] NewCounts = new uint[c - 1];
				DateTime[] NewTimestamps = new DateTime[c - 1];

				if (i > 0)
				{
					Array.Copy(References.ObjectReferences, 0, NewReferences, 0, i);
					Array.Copy(References.Counts, 0, NewCounts, 0, i);
					Array.Copy(References.Timestamps, 0, NewTimestamps, 0, i);
				}

				if (i < c - 1)
				{
					Array.Copy(References.ObjectReferences, i + 1, NewReferences, i, c - i - 1);
					Array.Copy(References.Counts, i + 1, NewCounts, i, c - i - 1);
					Array.Copy(References.Timestamps, i + 1, NewTimestamps, i, c - i - 1);
				}

				References.ObjectReferences = NewReferences;
				References.Counts = NewCounts;
				References.Timestamps = NewTimestamps;

				await Index.AddAsync(Token.Token, References, true);
			}
		}

		private async void Database_ObjectUpdated(object Sender, ObjectEventArgs e)
		{
			try
			{
				Tuple<CollectionInformation, TypeInformation, GenericObject> P = await Prepare(e.Object);
				if (P is null)
					return;

				// TODO
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}
	}
}
