using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence.Attributes;
using Waher.Persistence.Filters;
using Waher.Persistence.FullTextSearch.Keywords;
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
		private static Dictionary<string, bool> stopWords = new Dictionary<string, bool>();
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

			Types.OnInvalidated += this.Types_OnInvalidated;
		}

		/// <summary>
		/// Stops the module.
		/// </summary>
		public async Task Stop()
		{
			Database.ObjectInserted -= this.Database_ObjectInserted;
			Database.ObjectUpdated -= this.Database_ObjectUpdated;
			Database.ObjectDeleted -= this.Database_ObjectDeleted;

			Types.OnInvalidated -= this.Types_OnInvalidated;

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
				Dictionary<string, object> IndexableProperties;

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
						Tokens = Tokens,
						Indexed = DateTime.UtcNow
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
						Counts = new uint[] { (uint)Token.DocIndex.Length },
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
					NewCounts[c] = (uint)Token.DocIndex.Length;
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

					await Index.AddAsync(Token.Token + " " + References.LastBlock.ToString(), NewBlock, true);

					References.ObjectReferences = new ulong[] { Ref.Index };
					References.Counts = new uint[] { (uint)Token.DocIndex.Length };
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
		/// Gets the indexable property values from an object. Property values will be returned in lower-case.
		/// </summary>
		/// <param name="Obj">Generic object.</param>
		/// <param name="PropertyNames">Indexable property names.</param>
		/// <returns>Indexable property values found.</returns>
		internal static async Task<Dictionary<string, object>> GetIndexableProperties(object Obj, params string[] PropertyNames)
		{
			if (Obj is null)
				return new Dictionary<string, object>();
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
		internal static Dictionary<string, object> GetIndexableProperties(GenericObject Obj, params string[] PropertyNames)
		{
			Dictionary<string, object> Result = new Dictionary<string, object>();

			if (!(Obj is null))
			{
				foreach (string PropertyName in PropertyNames)
				{
					if (Obj.TryGetFieldValue(PropertyName, out object Value))
						Result[PropertyName] = Value;
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
		/// Parses a search string into keyworkds.
		/// </summary>
		/// <param name="Search">Search string.</param>
		/// <param name="TreatKeywordsAsPrefixes">If keywords should be treated as
		/// prefixes. Example: "test" would match "test", "tests" and "testing" if
		/// treated as a prefix, but also "tester", "testosterone", etc.</param>
		/// <returns>Keywords</returns>
		internal static Keyword[] ParseKeywords(string Search, bool TreatKeywordsAsPrefixes)
		{
			return ParseKeywords(Search, TreatKeywordsAsPrefixes, true);
		}

		/// <summary>
		/// Parses a search string into keyworkds.
		/// </summary>
		/// <param name="Search">Search string.</param>
		/// <param name="TreatKeywordsAsPrefixes">If keywords should be treated as
		/// prefixes. Example: "test" would match "test", "tests" and "testing" if
		/// treated as a prefix, but also "tester", "testosterone", etc.</param>
		/// <param name="ParseQuotes">If quotes are to be processed.</param>
		/// <returns>Keywords</returns>
		private static Keyword[] ParseKeywords(string Search, bool TreatKeywordsAsPrefixes,
			bool ParseQuotes)
		{
			List<Keyword> Result = new List<Keyword>();
			StringBuilder sb = new StringBuilder();
			bool First = true;
			bool Required = false;
			bool Prohibited = false;
			string Wildcard = null;
			int Type = 0;
			Keyword Keyword;
			string Token;

			foreach (char ch in Search.ToLower().Normalize(NormalizationForm.FormD))
			{
				UnicodeCategory Category = CharUnicodeInfo.GetUnicodeCategory(ch);
				if (Category == UnicodeCategory.NonSpacingMark)
					continue;

				if (char.IsLetterOrDigit(ch))
				{
					sb.Append(ch);
					First = false;
				}
				else if (Type == 2)
				{
					if (ch == '/')
					{
						Token = sb.ToString();
						sb.Clear();
						First = true;
						Type = 0;

						Add(new RegexKeyword(Token), Result, ref Required, ref Prohibited);
					}
					else
					{
						sb.Append(ch);
						First = false;
					}
				}
				else if (Type == 3)
				{
					if (ch == '"')
					{
						Token = sb.ToString();
						sb.Clear();
						First = true;
						Type = 0;

						Add(new SequenceOfKeywords(ParseKeywords(Token, false)),
							Result, ref Required, ref Prohibited);
					}
					else
						sb.Append(ch);
				}
				else if (Type == 4)
				{
					if (ch == '\'')
					{
						Token = sb.ToString();
						sb.Clear();
						First = true;
						Type = 0;

						Add(new SequenceOfKeywords(ParseKeywords(Token, false)),
							Result, ref Required, ref Prohibited);
					}
					else
						sb.Append(ch);
				}
				else if (Type == 0 && (ch == '*' || ch == '%' || ch == '¤' || ch == '#'))
				{
					sb.Append(ch);
					Type = 1;
					Wildcard = new string(ch, 1);
				}
				else
				{
					if (!First)
					{
						Token = sb.ToString();
						sb.Clear();
						First = true;

						if (Type == 1)
						{
							Keyword = new WildcardKeyword(Token, Wildcard);
							Wildcard = null;
						}
						else if (TreatKeywordsAsPrefixes)
							Keyword = new WildcardKeyword(Token);
						else
							Keyword = new PlainKeyword(Token);

						Add(Keyword, Result, ref Required, ref Prohibited);
						Type = 0;
					}

					if (ch == '+')
					{
						Required = true;
						Prohibited = false;
					}
					else if (ch == '-')
					{
						Prohibited = true;
						Required = false;
					}
					else if (ch == '/')
						Type = 2;
					else if (ch == '"' && ParseQuotes)
						Type = 3;
					else if (ch == '\'' && ParseQuotes)
						Type = 4;
				}
			}

			if (!First)
			{
				Token = sb.ToString();
				sb.Clear();

				switch (Type)
				{
					case 0:
					default:
						if (TreatKeywordsAsPrefixes)
							Keyword = new WildcardKeyword(Token);
						else
							Keyword = new PlainKeyword(Token);
						break;

					case 1:
						Keyword = new WildcardKeyword(Token, Wildcard);
						break;

					case 2:
						Keyword = new RegexKeyword(Token);
						break;
				}

				Add(Keyword, Result, ref Required, ref Prohibited);
			}

			return Result.ToArray();
		}

		private static void Add(Keyword Keyword, List<Keyword> Result, ref bool Required, ref bool Prohibited)
		{
			if (Required)
			{
				Keyword = new RequiredKeyword(Keyword);
				Required = false;
			}

			if (Prohibited)
			{
				Keyword = new ProhibitedKeyword(Keyword);
				Prohibited = false;
			}

			Result.Add(Keyword);
		}

		/// <summary>
		/// Performs a Full-Text-Search
		/// </summary>
		/// <param name="IndexCollection">Index collection name.</param>
		/// <param name="Offset">Index of first object matching the keywords.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Order">The order of objects to return.</param>
		/// <param name="PaginationStrategy">How to handle pagination.</param>
		/// <param name="Keywords">Keywords to search for.</param>
		/// <returns>Array of objects. Depending on choice of
		/// <paramref name="PaginationStrategy"/>, null items may be returned
		/// if underlying object is not compatible with <typeparamref name="T"/>.</returns>
		internal static async Task<T[]> FullTextSearch<T>(string IndexCollection,
			int Offset, int MaxCount, FullTextSearchOrder Order,
			PaginationStrategy PaginationStrategy, params Keyword[] Keywords)
			where T : class
		{
			if (MaxCount <= 0 || Keywords is null)
				return new T[0];

			int NrKeywords = Keywords.Length;
			if (NrKeywords == 0)
				return new T[0];

			Keywords = (Keyword[])Keywords.Clone();
			Array.Sort(Keywords, orderOfProcessing);

			SearchProcess Process;

			await synchObj.WaitAsync();
			try
			{
				IPersistentDictionary Index = await GetIndexLocked(IndexCollection);

				Process = new SearchProcess(Index, IndexCollection);

				foreach (Keyword Keyword in Keywords)
				{
					if (Keyword.Ignore)
						continue;

					if (!await Keyword.Process(Process))
						return new T[0];
				}
			}
			finally
			{
				synchObj.Release();
			}

			int c = Process.ReferencesByObject.Count;
			LinkedList<TokenReference>[] FoundReferences = new LinkedList<TokenReference>[c];
			Process.ReferencesByObject.Values.CopyTo(FoundReferences, 0);

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

			switch (PaginationStrategy)
			{
				case PaginationStrategy.PaginateOverObjectsNullIfIncompatible:
				default:
					foreach (LinkedList<TokenReference> ObjectReference in FoundReferences)
					{
						ulong RefIndex = ObjectReference.First.Value.ObjectReference;
						if (Offset > 0)
						{
							Offset--;
							continue;
						}

						ObjectReference Ref = await Process.TryGetObjectReference(RefIndex, true);
						if (Ref is null)
							Result.Add(null);
						else
						{
							T Object = await Database.TryLoadObject<T>(Ref.Collection, Ref.ObjectInstanceId);
							if (Object is null)
								Result.Add(null);
							else
								Result.Add(Object);
						}

						MaxCount--;

						if (MaxCount <= 0)
							break;
					}
					break;

				case PaginationStrategy.PaginateOverObjectsOnlyCompatible:
					foreach (LinkedList<TokenReference> ObjectReference in FoundReferences)
					{
						ulong RefIndex = ObjectReference.First.Value.ObjectReference;
						if (Offset > 0)
						{
							Offset--;
							continue;
						}

						ObjectReference Ref = await Process.TryGetObjectReference(RefIndex, true);
						if (Ref is null)
							continue;

						T Object = await Database.TryLoadObject<T>(Ref.Collection, Ref.ObjectInstanceId);
						if (Object is null)
							continue;

						Result.Add(Object);
						MaxCount--;

						if (MaxCount <= 0)
							break;
					}
					break;

				case PaginationStrategy.PaginationOverCompatibleOnly:
					foreach (LinkedList<TokenReference> ObjectReference in FoundReferences)
					{
						ulong RefIndex = ObjectReference.First.Value.ObjectReference;
						ObjectReference Ref = await Process.TryGetObjectReference(RefIndex, true);

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
					break;
			}

			return Result.ToArray();
		}

		private static readonly OrderOfProcessing orderOfProcessing = new OrderOfProcessing();
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

				object ObjectId = await Database.TryGetObjectId(e.Object);
				if (ObjectId is null)
					return;

				CollectionInformation CollectionInfo = P.Item1;
				TypeInformation TypeInfo = P.Item2;
				GenericObject GenObj = P.Item3;
				Dictionary<string, object> IndexableProperties;

				if (GenObj is null)
					IndexableProperties = TypeInfo.GetIndexableProperties(e.Object, CollectionInfo.PropertyNames);
				else
					IndexableProperties = GetIndexableProperties(GenObj, CollectionInfo.PropertyNames);

				if (IndexableProperties.Count == 0)
					return;

				TokenCount[] Tokens = Tokenize(IndexableProperties.Values);
				if (Tokens is null)
					return;

				ObjectReference Ref = await Database.FindFirstIgnoreRest<ObjectReference>(new FilterAnd(
					new FilterFieldEqualTo("Collection", P.Item1.CollectionName),
					new FilterFieldEqualTo("ObjectInstanceId", ObjectId)));

				bool Added = false;

				await synchObj.WaitAsync();
				try
				{
					if (Ref is null)
					{
						ulong Index = await GetNextIndexNrLocked(CollectionInfo.IndexCollectionName);

						Ref = new ObjectReference()
						{
							IndexCollection = CollectionInfo.IndexCollectionName,
							Collection = CollectionInfo.CollectionName,
							ObjectInstanceId = ObjectId,
							Index = Index,
							Tokens = Tokens,
							Indexed = DateTime.UtcNow
						};

						await AddTokensToIndexLocked(Ref);
						await Database.Insert(Ref);

						Added = true;
					}
					else
					{
						await RemoveTokensFromIndexLocked(Ref);

						Ref.Tokens = Tokens;
						await AddTokensToIndexLocked(Ref);

						await Database.Update(Ref);
					}
				}
				finally
				{
					synchObj.Release();
				}

				if (Added)
					await Search.RaiseObjectAddedToIndex(this, new ObjectReferenceEventArgs(Ref));
				else
					await Search.RaiseObjectUpdatedInIndex(this, new ObjectReferenceEventArgs(Ref));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Registers stop-words with the search-engine.
		/// Stop-words are ignored in searches.
		/// </summary>
		/// <param name="StopWords">Stop words.</param>
		internal static void RegisterStopWords(params string[] StopWords)
		{
			Dictionary<string, bool> NewList = new Dictionary<string, bool>();

			foreach (KeyValuePair<string, bool> P in stopWords)
				NewList[P.Key] = P.Value;

			stopWords = NewList;
		}

		/// <summary>
		/// Checks if a word is a stop word.
		/// </summary>
		/// <param name="StopWord">Word to check.</param>
		/// <returns>If word is a stop word.</returns>
		internal static bool IsStopWord(string StopWord)
		{
			return stopWords.TryGetValue(StopWord, out bool b) && b;
		}

		/// <summary>
		/// Tokenizes a set of objects using available tokenizers.
		/// Tokenizers are classes with a default contructor, implementing
		/// the <see cref="ITokenizer"/> interface.
		/// </summary>
		/// <param name="Objects">Objects to tokenize.</param>
		/// <returns>Tokens</returns>
		public static TokenCount[] Tokenize(IEnumerable<object> Objects)
		{
			Dictionary<string, List<uint>> TokenCounts = new Dictionary<string, List<uint>>();

			foreach (object Object in Objects)
			{
				if (Object is null)
					continue;

				Type T = Object.GetType();
				ITokenizer Tokenizer;

				lock (tokenizers)
				{
					if (tokenizers.TryGetValue(T, out Tokenizer) && Tokenizer is null)
						continue;
				}

				if (Tokenizer is null)
				{
					Tokenizer = Types.FindBest<ITokenizer, Type>(T);

					lock (tokenizers)
					{
						tokenizers[T] = Tokenizer;
					}

					if (Tokenizer is null)
						continue;
				}

				Tokenizer.Tokenize(Object, TokenCounts);
			}

			int c = TokenCounts.Count;
			if (c == 0)
				return null;

			int i = 0;
			TokenCount[] Counts = new TokenCount[c];

			foreach (KeyValuePair<string, List<uint>> P in TokenCounts)
				Counts[i++] = new TokenCount(P.Key, P.Value.ToArray());

			return Counts;
		}

		private static readonly Dictionary<Type, ITokenizer> tokenizers = new Dictionary<Type, ITokenizer>();

		private void Types_OnInvalidated(object sender, EventArgs e)
		{
			lock (tokenizers)
			{
				tokenizers.Clear();
			}
		}

	}
}
