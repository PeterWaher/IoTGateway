using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence.Attributes;
using Waher.Persistence.Filters;
using Waher.Persistence.FullTextSearch.Files;
using Waher.Persistence.FullTextSearch.Keywords;
using Waher.Persistence.FullTextSearch.Orders;
using Waher.Persistence.FullTextSearch.Tokenizers;
using Waher.Persistence.LifeCycle;
using Waher.Persistence.Serialization;
using Waher.Runtime.Cache;
using Waher.Runtime.Inventory;
using Waher.Runtime.Threading;
using Waher.Script.Model;

namespace Waher.Persistence.FullTextSearch
{
	/// <summary>
	/// Full-text search module, controlling the life-cycle of the full-text-search engine.
	/// </summary>
	[ModuleDependency(typeof(DatabaseModule))]
	public class FullTextSearchModule : IModule
	{
		private static readonly MultiReadSingleWriteObject synchObj = new MultiReadSingleWriteObject();
		private static Cache<string, QueryRecord> queryCache;
		private static Dictionary<string, bool> stopWords = new Dictionary<string, bool>();
		private static IPersistentDictionary collectionInformation;
		private static Dictionary<string, CollectionInformation> collections;
		private static Dictionary<string, IPersistentDictionary> indices;
		private static Dictionary<Type, TypeInformation> types;
		private static FullTextSearchModule instance = null;

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
			queryCache = new Cache<string, QueryRecord>(int.MaxValue, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

			instance = this;

			Database.ObjectInserted += this.Database_ObjectInserted;
			Database.ObjectUpdated += this.Database_ObjectUpdated;
			Database.ObjectDeleted += this.Database_ObjectDeleted;
			Database.CollectionCleared += this.Database_CollectionCleared;

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
			Database.CollectionCleared -= this.Database_CollectionCleared;

			Types.OnInvalidated -= this.Types_OnInvalidated;

			// TODO: Wait for current objects to be finished.

			await synchObj.BeginWrite();
			try
			{
				queryCache?.Dispose();
				queryCache = null;

				if (!(indices is null))
				{
					foreach (IPersistentDictionary Index in indices.Values)
						Index.Dispose();

					indices.Clear();
					indices = null;
				}

				collectionInformation?.Dispose();
				collectionInformation = null;

				collections?.Clear();
				collections = null;

				types?.Clear();
				types = null;

			}
			finally
			{
				await synchObj.EndWrite();
				instance = null;
			}
		}

		private void Database_ObjectInserted(object Sender, ObjectEventArgs e)
		{
			Task.Run(() => this.ObjectInserted(e));
		}

		private async Task ObjectInserted(ObjectEventArgs e)
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
				TokenCount[] Tokens;
				string IndexName;

				if (GenObj is null)
				{
					IndexName = TypeInfo.GetIndexCollection(e.Object);
					Tokens = await TypeInfo.Tokenize(e.Object, CollectionInfo.Properties);
				}
				else
				{
					IndexName = CollectionInfo.IndexCollectionName;
					Tokens = await Tokenize(GenObj, CollectionInfo.Properties);
				}

				if (Tokens.Length == 0)
					return;

				ObjectReference Ref;

				await synchObj.BeginWrite();
				try
				{
					ulong Index = await GetNextIndexNrLocked(IndexName);

					Ref = new ObjectReference()
					{
						IndexCollection = IndexName,
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
					await synchObj.EndWrite();
				}

				queryCache.Clear();

				await Search.RaiseObjectAddedToIndex(this, new ObjectReferenceEventArgs(Ref));
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private static async Task<IPersistentDictionary> GetIndexLocked(string IndexCollection, bool CreateIfNotFound)
		{
			if (indices.TryGetValue(IndexCollection, out IPersistentDictionary Result))
				return Result;

			if (CreateIfNotFound)
			{
				Result = await Database.GetDictionary(IndexCollection);
				indices[IndexCollection] = Result;
			}

			return Result;
		}

		private static async Task AddTokensToIndexLocked(ObjectReference Ref)
		{
			DateTime TP = DateTime.UtcNow;
			IPersistentDictionary Index = await GetIndexLocked(Ref.IndexCollection, true);

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
		/// Gets the database collections that get indexed into a given index colltion.
		/// </summary>
		/// <returns>Collection Names indexed in the full-text-search index.</returns>
		public static async Task<Dictionary<string, string[]>> GetCollectionNames()
		{
			Dictionary<string, List<string>> ByIndex = new Dictionary<string, List<string>>();

			await synchObj.BeginRead();
			try
			{
				foreach (object Obj in await collectionInformation.GetValuesAsync())
				{
					if (Obj is CollectionInformation Info && Info.IndexForFullTextSearch)
					{
						if (!ByIndex.TryGetValue(Info.IndexCollectionName, out List<string> Collections))
						{
							Collections = new List<string>();
							ByIndex[Info.IndexCollectionName] = Collections;
						}

						Collections.Add(Info.CollectionName);
					}
				}
			}
			finally
			{
				await synchObj.EndRead();
			}

			Dictionary<string, string[]> Result = new Dictionary<string, string[]>();

			foreach (KeyValuePair<string, List<string>> Rec in ByIndex)
				Result[Rec.Key] = Rec.Value.ToArray();

			return Result;
		}

		/// <summary>
		/// Gets the database collections that get indexed into a given index colltion.
		/// </summary>
		/// <param name="IndexCollectionName">Index Collection Name</param>
		/// <returns>Collection Names indexed in the full-text-search index
		/// defined by <paramref name="IndexCollectionName"/>.</returns>
		public static async Task<string[]> GetCollectionNames(string IndexCollectionName)
		{
			await synchObj.BeginRead();
			try
			{
				return await GetCollectionNamesLocked(IndexCollectionName);
			}
			finally
			{
				await synchObj.EndRead();
			}
		}

		/// <summary>
		/// Gets the database collections that get indexed into a given index colltion.
		/// </summary>
		/// <param name="IndexCollectionName">Index Collection Name</param>
		/// <returns>Collection Names indexed in the full-text-search index
		/// defined by <paramref name="IndexCollectionName"/>.</returns>
		private static async Task<string[]> GetCollectionNamesLocked(string IndexCollectionName)
		{
			List<string> Result = new List<string>();

			foreach (object Obj in await collectionInformation.GetValuesAsync())
			{
				if (Obj is CollectionInformation Info && Info.IndexForFullTextSearch)
				{
					if (Info.IndexCollectionName == IndexCollectionName)
						Result.Add(Info.CollectionName);
				}
			}

			return Result.ToArray();
		}

		/// <summary>
		/// Defines the Full-text-search index collection name, for objects in a given collection.
		/// </summary>
		/// <param name="IndexCollection">Collection name for full-text-search index of objects in the given collection.</param>
		/// <param name="CollectionName">Collection of objects to index.</param>
		/// <returns>If the configuration was changed.</returns>
		internal static async Task<bool> SetFullTextSearchIndexCollection(string IndexCollection, string CollectionName)
		{
			await synchObj.BeginWrite();
			try
			{
				CollectionInformation Info = await GetCollectionInfoLocked(IndexCollection, CollectionName, false);
				bool Created;

				if (Info is null)
				{
					Created = true;
					Info = await GetCollectionInfoLocked(IndexCollection, CollectionName, true);
				}
				else
					Created = false;

				if (Info.IndexCollectionName != IndexCollection)
				{
					Info.IndexCollectionName = IndexCollection;
					await collectionInformation.AddAsync(Info.CollectionName, Info, true);

					return true;
				}
				else
					return Created;
			}
			finally
			{
				await synchObj.EndWrite();
			}
		}

		/// <summary>
		/// Adds properties for full-text-search indexation.
		/// </summary>
		/// <param name="CollectionName">Collection name.</param>
		/// <param name="Properties">Properties to index.</param>
		/// <returns>If new property names were found and added.</returns>
		internal static async Task<bool> AddFullTextSearch(string CollectionName, params PropertyDefinition[] Properties)
		{
			await synchObj.BeginWrite();
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
				await synchObj.EndWrite();
			}
		}

		/// <summary>
		/// Removes properties from full-text-search indexation.
		/// </summary>
		/// <param name="CollectionName">Collection name.</param>
		/// <param name="Properties">Properties to remove from indexation.</param>
		/// <returns>If property names were found and removed.</returns>
		internal static async Task<bool> RemoveFullTextSearch(string CollectionName, params PropertyDefinition[] Properties)
		{
			await synchObj.BeginWrite();
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
				await synchObj.EndWrite();
			}
		}

		/// <summary>
		/// Gets indexed properties for full-text-search indexation.
		/// </summary>
		/// <returns>Dictionary of indexed properties, per collection.</returns>
		internal static async Task<Dictionary<string, PropertyDefinition[]>> GetFullTextSearchIndexedProperties()
		{
			Dictionary<string, PropertyDefinition[]> Result = new Dictionary<string, PropertyDefinition[]>();

			await synchObj.BeginRead();
			try
			{
				foreach (object Obj in await collectionInformation.GetValuesAsync())
				{
					if (Obj is CollectionInformation Info && Info.IndexForFullTextSearch)
						Result[Info.CollectionName] = Info.Properties;
				}
			}
			finally
			{
				await synchObj.EndRead();
			}

			return Result;
		}

		/// <summary>
		/// Gets indexed properties for full-text-search indexation.
		/// </summary>
		/// <param name="CollectionName">Collection name.</param>
		/// <returns>Array of indexed properties.</returns>
		internal static async Task<PropertyDefinition[]> GetFullTextSearchIndexedProperties(string CollectionName)
		{
			await synchObj.BeginRead();
			try
			{
				CollectionInformation Info = await GetCollectionInfoLocked(CollectionName, false);

				if (Info is null || !Info.IndexForFullTextSearch)
					return new PropertyDefinition[0];
				else
					return (PropertyDefinition[])Info.Properties.Clone();
			}
			finally
			{
				await synchObj.EndRead();
			}
		}

		private static async Task<Tuple<CollectionInformation, TypeInformation, GenericObject>> Prepare(object Object)
		{
			Object = await ScriptNode.WaitPossibleTask(Object);

			await synchObj.BeginWrite();
			try
			{
				if (Object is GenericObject GenObj)
					return await PrepareLocked(GenObj);
				else
					return await PrepareLocked(Object.GetType(), Object);
			}
			finally
			{
				await synchObj.EndWrite();
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

		private static async Task<TypeInformation> GetTypeInfoLocked(Type T, object Instance)
		{
			if (types is null)
				throw new Exception("Full text search module not started, or in the process of being stopped.");

			if (types.TryGetValue(T, out TypeInformation Result))
				return Result;

			TypeInfo TI = T.GetTypeInfo();
			IEnumerable<FullTextSearchAttribute> SearchAttrs = TI.GetCustomAttributes<FullTextSearchAttribute>(true);
			CollectionNameAttribute CollectionAttr = TI.GetCustomAttribute<CollectionNameAttribute>(true);
			ITokenizer CustomTokenizer = Types.FindBest<ITokenizer, Type>(T);

			if (CollectionAttr is null)
				Result = new TypeInformation(T, TI, null, null, CustomTokenizer, null);
			else
			{
				string CollectionName = CollectionAttr.Name;
				bool DynamicIndex = false;
				string IndexName;

				if (!(SearchAttrs is null))
				{
					foreach (FullTextSearchAttribute Attribute in SearchAttrs)
					{
						if (Attribute.DynamicIndexCollection)
						{
							DynamicIndex = true;
							break;
						}
					}
				}

				if (DynamicIndex)
					IndexName = null;
				else
				{
					IndexName = CollectionName;

					foreach (FullTextSearchAttribute Attribute in SearchAttrs)
					{
						IndexName = Attribute.GetIndexCollection(Instance);
						break;
					}
				}

				CollectionInformation Info = await GetCollectionInfoLocked(IndexName, CollectionName, true);

				Result = new TypeInformation(T, TI, CollectionName, Info, CustomTokenizer, SearchAttrs);

				if (Result.HasPropertyDefinitions && Info.AddIndexableProperties(Result.Properties))
					await collectionInformation.AddAsync(CollectionName, Info, true);
				else if (!(CustomTokenizer is null) && !Info.IndexForFullTextSearch)
				{
					Info.IndexForFullTextSearch = true;
					await collectionInformation.AddAsync(CollectionName, Info, true);
				}
			}

			types[T] = Result;

			return Result;
		}

		private static async Task<Tuple<CollectionInformation, TypeInformation, GenericObject>> PrepareLocked(Type T, object Instance)
		{
			TypeInformation TypeInfo = await GetTypeInfoLocked(T, Instance);
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

			StringBuilder sb = new StringBuilder();

			sb.Append(IndexCollection);
			sb.Append(' ');
			sb.Append(Order.ToString());

			foreach (Keyword Keyword in Keywords)
			{
				if (!Keyword.Ignore)
				{
					sb.Append(' ');
					sb.Append(Keyword.ToString());
				}
			}

			string Key = sb.ToString();
			MatchInformation[] FoundReferences;
			SearchProcess Process = null;

			if (queryCache.TryGetValue(Key, out QueryRecord QueryRecord))
			{
				FoundReferences = QueryRecord.FoundReferences;
				Process = QueryRecord.Process;
			}
			else
			{
				IPersistentDictionary Index;

				await synchObj.BeginRead();
				try
				{
					Index = await GetIndexLocked(IndexCollection, false);

					if (!(Index is null))
					{
						Process = new SearchProcess(Index, IndexCollection);

						foreach (Keyword Keyword in Keywords)
						{
							if (Keyword.Ignore)
								continue;

							if (!await Keyword.Process(Process))
								return new T[0];
						}
					}
				}
				finally
				{
					await synchObj.EndRead();
				}

				if (Index is null)
				{
					await synchObj.BeginWrite();
					try
					{
						Index = await GetIndexLocked(IndexCollection, true);

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
						await synchObj.EndWrite();
					}
				}

				int c = Process.ReferencesByObject.Count;

				FoundReferences = new MatchInformation[c];
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

				queryCache[Key] = new QueryRecord()
				{
					FoundReferences = FoundReferences,
					Process = Process
				};
			}

			List<T> Result = new List<T>();

			switch (PaginationStrategy)
			{
				case PaginationStrategy.PaginateOverObjectsNullIfIncompatible:
				default:
					foreach (MatchInformation ObjectReference in FoundReferences)
					{
						if (Offset > 0)
						{
							Offset--;
							continue;
						}

						ulong RefIndex = ObjectReference.ObjectReference;
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
					foreach (MatchInformation ObjectReference in FoundReferences)
					{
						if (Offset > 0)
						{
							Offset--;
							continue;
						}

						ulong RefIndex = ObjectReference.ObjectReference;
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
					foreach (MatchInformation ObjectReference in FoundReferences)
					{
						ulong RefIndex = ObjectReference.ObjectReference;
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

		private class QueryRecord
		{
			public MatchInformation[] FoundReferences;
			public SearchProcess Process;
		}

		private void Database_ObjectDeleted(object Sender, ObjectEventArgs e)
		{
			Task.Run(() => this.ObjectDeleted(e));
		}

		private async Task ObjectDeleted(ObjectEventArgs e)
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

				await synchObj.BeginWrite();
				try
				{
					await RemoveTokensFromIndexLocked(Ref);
				}
				finally
				{
					await synchObj.EndWrite();
				}

				queryCache.Clear();

				await Search.RaiseObjectRemovedFromIndex(this, new ObjectReferenceEventArgs(Ref));
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private static async Task RemoveTokensFromIndexLocked(ObjectReference Ref)
		{
			IPersistentDictionary Index = await GetIndexLocked(Ref.IndexCollection, true);

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

		private void Database_ObjectUpdated(object Sender, ObjectEventArgs e)
		{
			Task.Run(() => ProcessObjectUpdate(e.Object));
		}

		/// <summary>
		/// Processes an object that has been updated.
		/// </summary>
		/// <param name="Object">Updated object.</param>
		public static async Task ProcessObjectUpdate(object Object)
		{
			try
			{
				Tuple<CollectionInformation, TypeInformation, GenericObject> P = await Prepare(Object);
				if (P is null)
					return;

				object ObjectId = await Database.TryGetObjectId(Object);
				if (ObjectId is null)
					return;

				CollectionInformation CollectionInfo = P.Item1;
				TypeInformation TypeInfo = P.Item2;
				GenericObject GenObj = P.Item3;
				TokenCount[] Tokens;

				if (GenObj is null)
					Tokens = await TypeInfo.Tokenize(Object, CollectionInfo.Properties);
				else
					Tokens = await Tokenize(GenObj, CollectionInfo.Properties);

				ObjectReference Ref = await Database.FindFirstIgnoreRest<ObjectReference>(new FilterAnd(
					new FilterFieldEqualTo("Collection", P.Item1.CollectionName),
					new FilterFieldEqualTo("ObjectInstanceId", ObjectId)));

				if (AreSame(Tokens, Ref?.Tokens))
					return;

				bool Added = false;

				await synchObj.BeginWrite();
				try
				{
					if (Ref is null)
					{
						if (Tokens.Length == 0)
							return;

						string IndexName;

						if (GenObj is null)
							IndexName = TypeInfo.GetIndexCollection(Object);
						else
							IndexName = CollectionInfo.IndexCollectionName;

						ulong Index = await GetNextIndexNrLocked(IndexName);

						Ref = new ObjectReference()
						{
							IndexCollection = IndexName,
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
					await synchObj.EndWrite();
				}

				queryCache.Clear();

				if (Added)
					await Search.RaiseObjectAddedToIndex(instance, new ObjectReferenceEventArgs(Ref));
				else
					await Search.RaiseObjectUpdatedInIndex(instance, new ObjectReferenceEventArgs(Ref));
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private static bool AreSame(TokenCount[] Tokens1, TokenCount[] Tokens2)
		{
			int c = Tokens1?.Length ?? 0;
			int d = Tokens2?.Length ?? 0;

			if (c != d)
				return false;

			int i;

			for (i = 0; i < c; i++)
			{
				if (!Tokens1[i].Equals(Tokens2[i]))
					return false;
			}

			return true;
		}

		private async Task Database_CollectionCleared(object Sender, CollectionEventArgs e)
		{
			try
			{
				IEnumerable<ObjectReference> ObjectsDeleted;

				do
				{
					ObjectsDeleted = await Database.FindDelete<ObjectReference>(0, 1000,
						new FilterFieldEqualTo("Collection", e.Collection));

					foreach (ObjectReference Ref in ObjectsDeleted)
					{
						await synchObj.BeginWrite();
						try
						{
							await RemoveTokensFromIndexLocked(Ref);
						}
						finally
						{
							await synchObj.EndWrite();
						}

						queryCache.Clear();

						await Search.RaiseObjectRemovedFromIndex(this, new ObjectReferenceEventArgs(Ref));
					}
				}
				while (!IsEmpty(ObjectsDeleted));
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		/// <summary>
		/// Reindexes the full-text-search index for a database collection.
		/// </summary>
		/// <param name="IndexCollectionName">Index Collection</param>
		/// <returns>Number of objects reindexed.</returns>
		public static async Task<long> ReindexCollection(string IndexCollectionName)
		{
			IPersistentDictionary Index;
			string[] Collections;

			await synchObj.BeginWrite();
			try
			{
				Index = await GetIndexLocked(IndexCollectionName, true);
				await Index.ClearAsync();

				Collections = await GetCollectionNamesLocked(IndexCollectionName);
			}
			finally
			{
				await synchObj.EndWrite();
			}

			IEnumerable<ObjectReference> ObjectsDeleted;

			do
			{
				ObjectsDeleted = await Database.FindDelete<ObjectReference>(0, 1000,
					new FilterFieldEqualTo("IndexCollection", IndexCollectionName));

				foreach (ObjectReference Ref in ObjectsDeleted)
					await Search.RaiseObjectRemovedFromIndex(instance, new ObjectReferenceEventArgs(Ref));
			}
			while (!IsEmpty(ObjectsDeleted));

			ReindexCollectionIteration Iteration = new ReindexCollectionIteration();

			await Database.Iterate<object>(Iteration, Collections);

			return Iteration.NrObjectsProcessed;
		}

		private static bool IsEmpty(IEnumerable<ObjectReference> Objects)
		{
			foreach (ObjectReference _ in Objects)
				return false;

			return true;
		}

		private class ReindexCollectionIteration : IDatabaseIteration<object>
		{
			public Task StartDatabase() => Task.CompletedTask;
			public Task EndDatabase() => Task.CompletedTask;
			public Task EndCollection() => Task.CompletedTask;
			public Task IncompatibleObject(object ObjectId) => Task.CompletedTask;

			public long NrObjectsProcessed = 0;
			public int NrCollectionsProcessed = 0;

			public Task StartCollection(string CollectionName)
			{
				this.NrCollectionsProcessed++;
				return Task.CompletedTask;
			}

			public async Task ProcessObject(object Object)
			{
				this.NrObjectsProcessed++;
				await instance.ObjectInserted(new ObjectEventArgs(Object));
			}

			public Task ReportException(Exception Exception)
			{
				Log.Exception(Exception);
				return Task.CompletedTask;
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

			foreach (string StopWord in StopWords)
				NewList[StopWord] = true;

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
		public static async Task<TokenCount[]> Tokenize(IEnumerable<object> Objects)
		{
			TokenizationProcess Process = new TokenizationProcess();
			await Tokenize(Objects, Process);

			return Process.ToArray();
		}

		/// <summary>
		/// Tokenizes a set of objects using available tokenizers.
		/// Tokenizers are classes with a default contructor, implementing
		/// the <see cref="ITokenizer"/> interface.
		/// </summary>
		/// <param name="Objects">Objects to tokenize.</param>
		/// <param name="Process">Tokenization process.</param>
		public static async Task Tokenize(IEnumerable<object> Objects,
			TokenizationProcess Process)
		{
			foreach (object Object in Objects)
			{
				if (Object is null)
					continue;

				object Object2 = await ScriptNode.WaitPossibleTask(Object);

				Type T = Object2.GetType();
				ITokenizer Tokenizer;
				bool Found;

				lock (tokenizers)
				{
					Found = tokenizers.TryGetValue(T, out Tokenizer);
				}

				if (!Found)
				{
					Tokenizer = Types.FindBest<ITokenizer, Type>(T);

					lock (tokenizers)
					{
						tokenizers[T] = Tokenizer;
					}
				}

				if (Tokenizer is null)
				{
					Tuple<CollectionInformation, TypeInformation, GenericObject> P = await Prepare(Object2);
					if (P is null)
						continue;

					object ObjectId = await Database.TryGetObjectId(Object2);
					if (ObjectId is null)
						return;

					CollectionInformation CollectionInfo = P.Item1;
					TypeInformation TypeInfo = P.Item2;
					GenericObject GenObj = P.Item3;

					if (GenObj is null)
						await TypeInfo.Tokenize(Object2, Process, CollectionInfo.Properties);
					else
						await Tokenize(GenObj, Process, CollectionInfo.Properties);
				}
				else
					await Tokenizer.Tokenize(Object2, Process);

				Process.DocumentIndexOffset++;
			}
		}

		private static readonly Dictionary<Type, ITokenizer> tokenizers = new Dictionary<Type, ITokenizer>();

		private void Types_OnInvalidated(object sender, EventArgs e)
		{
			lock (tokenizers)
			{
				tokenizers.Clear();
			}
		}

		/// <summary>
		/// Gets the indexable property values from an object. Property values will be returned in lower-case.
		/// </summary>
		/// <param name="Obj">Generic object.</param>
		/// <param name="Properties">Indexable properties.</param>
		/// <returns>Indexable property values found.</returns>
		internal static async Task<TokenCount[]> Tokenize(GenericObject Obj, params PropertyDefinition[] Properties)
		{
			LinkedList<object> Values = await GetValues(Obj, Properties);

			if (Values.First is null)
				return null;

			return await Tokenize(Values);
		}

		/// <summary>
		/// Gets the indexable property values from an object. Property values will be returned in lower-case.
		/// </summary>
		/// <param name="Obj">Generic object.</param>
		/// <param name="Process">Tokenization process.</param>
		/// <param name="Properties">Indexable properties.</param>
		/// <returns>Indexable property values found.</returns>
		internal static async Task Tokenize(GenericObject Obj, TokenizationProcess Process, params PropertyDefinition[] Properties)
		{
			LinkedList<object> Values = await GetValues(Obj, Properties);

			if (!(Values.First is null))
				await Tokenize(Values, Process);
		}

		/// <summary>
		/// Gets object property values from a generic object.
		/// </summary>
		/// <param name="Obj">Generic object</param>
		/// <param name="Properties">Properties</param>
		/// <returns>Enumeration of property values.</returns>
		internal static async Task<LinkedList<object>> GetValues(GenericObject Obj, params PropertyDefinition[] Properties)
		{
			LinkedList<object> Values = new LinkedList<object>();
			object Value;

			if (!(Obj is null))
			{
				foreach (PropertyDefinition Property in Properties)
				{
					Value = await Property.GetValue(Obj);
					if (!(Value is null))
						Values.AddLast(Value);
				}
			}

			return Values;
		}

		/// <summary>
		/// Indexes or reindexes files in a folder.
		/// </summary>
		/// <param name="IndexCollection">Name of index collection.</param>
		/// <param name="Folder">Folder name.</param>
		/// <param name="Recursive">If processing of files in subfolders should be performed.</param>
		/// <param name="ExcludeSubfolders">Any subfolders to exclude (in recursive mode).</param>
		/// <returns>Statistics about indexation process.</returns>
		internal static async Task<FolderIndexationStatistics> IndexFolder(string IndexCollection, string Folder, bool Recursive,
			params string[] ExcludeSubfolders)
		{
			if (string.IsNullOrEmpty(IndexCollection))
				throw new ArgumentException("Empty index.", nameof(IndexCollection));

			if (string.IsNullOrEmpty(Folder))
				throw new ArgumentException("Empty folder.", nameof(Folder));

			Folder = Path.GetFullPath(Folder);
			if (!Directory.Exists(Folder))
				throw new ArgumentException("Folder does not exist.", nameof(Folder));

			if (Folder[Folder.Length - 1] != Path.DirectorySeparatorChar)
				Folder += Path.DirectorySeparatorChar;

			string[] FileNames = Directory.GetFiles(Folder, "*.*", Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
			Dictionary<CaseInsensitiveString, FileReference> References = new Dictionary<CaseInsensitiveString, FileReference>();
			FolderIndexationStatistics Result = new FolderIndexationStatistics();
			int i, c, d;

			if (!(ExcludeSubfolders is null))
			{
				ExcludeSubfolders = (string[])ExcludeSubfolders.Clone();
				c = ExcludeSubfolders.Length;

				for (i = 0; i < c; i++)
				{
					ExcludeSubfolders[i] = Path.GetFullPath(ExcludeSubfolders[i]);
					d = ExcludeSubfolders[i].Length;

					if (d == 0 || ExcludeSubfolders[i][d - 1] != Path.DirectorySeparatorChar)
						ExcludeSubfolders[i] += Path.DirectorySeparatorChar;
				}
			}

			IEnumerable<FileReference> ReferencesInDB = await Database.Find<FileReference>(
				new FilterAnd(
					new FilterFieldEqualTo("IndexCollection", IndexCollection),
					new FilterFieldLikeRegEx("FileName", Database.WildcardToRegex(Folder + "*", "*"))));

			foreach (FileReference Reference in ReferencesInDB)
				References[Reference.FileName] = Reference;

			foreach (string FileName in FileNames)
			{
				if (!(ExcludeSubfolders is null))
				{
					bool Exclude = false;

					foreach (string s in ExcludeSubfolders)
					{
						if (FileName.StartsWith(s))
						{
							Exclude = true;
							break;
						}
					}

					if (Exclude)
						continue;
				}

				if (!FileReferenceTokenizer.HasTokenizer(FileName))
					continue;

				Result.NrFiles++;

				DateTime TP = File.GetLastWriteTimeUtc(FileName);

				if (References.TryGetValue(FileName, out FileReference Ref))
				{
					References.Remove(FileName);

					if (Ref.Timestamp == TP)
						continue;

					Ref.Timestamp = TP;
					await Database.Update(Ref); // Will trigger retokenization of file.

					Result.NrUpdated++;
					Result.TotalChanges++;
				}
				else
				{
					Ref = new FileReference()
					{
						FileName = FileName,
						IndexCollection = IndexCollection,
						Timestamp = TP
					};

					await Database.Insert(Ref); // Will trigger tokenization of file.

					Result.NrAdded++;
					Result.TotalChanges++;
				}
			}

			foreach (FileReference Reference in References.Values)
			{
				await Database.Delete(Reference);   // Will trigger removal of tokens.
				Result.NrDeleted++;
				Result.TotalChanges++;
			}

			return Result;
		}

		/// <summary>
		/// Indexes or reindexes a file.
		/// </summary>
		/// <param name="IndexCollection">Name of index collection.</param>
		/// <param name="FileName">File name.</param>
		/// <returns>If index was updated.</returns>
		internal static async Task<bool> IndexFile(string IndexCollection, string FileName)
		{
			FileName = Path.GetFullPath(FileName);

			FileReference ReferenceInDB = await Database.FindFirstIgnoreRest<FileReference>(
				new FilterAnd(
					new FilterFieldEqualTo("IndexCollection", IndexCollection),
					new FilterFieldEqualTo("FileName", FileName)));

			if (!FileReferenceTokenizer.HasTokenizer(FileName))
				return false;

			if (File.Exists(FileName))
			{
				DateTime TP = File.GetLastWriteTimeUtc(FileName);

				if (ReferenceInDB is null)
				{
					ReferenceInDB = new FileReference()
					{
						FileName = FileName,
						IndexCollection = IndexCollection,
						Timestamp = TP
					};

					await Database.Insert(ReferenceInDB); // Will trigger tokenization of file.

					return true;
				}
				else
				{
					if (ReferenceInDB.Timestamp == TP)
						return false;

					ReferenceInDB.Timestamp = TP;
					await Database.Update(ReferenceInDB); // Will trigger retokenization of file.

					return true;
				}
			}
			else if (ReferenceInDB is null)
				return false;
			else
			{
				await Database.Delete(ReferenceInDB);   // Will trigger removal of tokens.
				return true;
			}
		}

	}
}
