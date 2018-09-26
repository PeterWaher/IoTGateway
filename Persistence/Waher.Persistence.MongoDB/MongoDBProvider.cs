using System;
using System.Collections.Generic;
using System.Xml;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using Waher.Persistence.Filters;
using Waher.Persistence.Serialization;
using Waher.Persistence.MongoDB.Serialization;
using Waher.Runtime.Cache;
using System.Collections;

namespace Waher.Persistence.MongoDB
{
	/// <summary>
	/// MongoDB database provider.
	/// </summary>
	public class MongoDBProvider : IDatabaseProvider
	{
		private readonly Dictionary<string, IMongoCollection<BsonDocument>> collections = new Dictionary<string, IMongoCollection<BsonDocument>>();
		private readonly Dictionary<string, ObjectSerializer> serializers = new Dictionary<string, ObjectSerializer>();
		private MongoClient client;
		private IMongoDatabase database;
		private string defaultCollectionName;
		private string lastCollectionName = null;
		private string lastSerializerName = null;
		private ObjectSerializer lastSerializer = null;
		private IMongoCollection<BsonDocument> lastCollection = null;
		private IMongoCollection<BsonDocument> defaultCollection;

		/// <summary>
		/// MongoDB database provider, for a local MongoDB database.
		/// </summary>
		/// <param name="DatabaseName">Name of database.</param>
		/// <param name="DefaultCollectionName">Default Collection Name.</param>
		public MongoDBProvider(string DatabaseName, string DefaultCollectionName)
		{
			MongoClientSettings Settings = new MongoClientSettings();
			this.Init(Settings, DatabaseName, DefaultCollectionName);
		}

		/// <summary>
		/// MongoDB database provider.
		/// </summary>
		/// <param name="HostName">Host name of MongoDB server.</param>
		/// <param name="DatabaseName">Name of database.</param>
		/// <param name="DefaultCollectionName">Name of default collection.</param>
		public MongoDBProvider(string HostName, string DatabaseName, string DefaultCollectionName)
		{
			MongoClientSettings Settings = new MongoClientSettings()
			{
				Server = new MongoServerAddress(HostName)
			};

			this.Init(Settings, DatabaseName, DefaultCollectionName);
		}

		/// <summary>
		/// MongoDB database provider.
		/// </summary>
		/// <param name="HostName">Host name of MongoDB server.</param>
		/// <param name="Port">Port number used to connect to MongoDB server.</param>
		/// <param name="DatabaseName">Name of database.</param>
		/// <param name="DefaultCollectionName">Name of default collection.</param>
		public MongoDBProvider(string HostName, int Port, string DatabaseName, string DefaultCollectionName)
		{
			MongoClientSettings Settings = new MongoClientSettings()
			{
				Server = new MongoServerAddress(HostName, Port)
			};

			this.Init(Settings, DatabaseName, DefaultCollectionName);
		}

		/// <summary>
		/// MongoDB database provider.
		/// </summary>
		/// <param name="Settings">Connection settings.</param>
		/// <param name="DatabaseName">Name of database.</param>
		/// <param name="DefaultCollectionName">Name of default collection.</param>
		public MongoDBProvider(MongoClientSettings Settings, string DatabaseName, string DefaultCollectionName)
		{
			this.Init(Settings, DatabaseName, DefaultCollectionName);
		}

		private void Init(MongoClientSettings Settings, string DatabaseName, string DefaultCollectionName)
		{
			this.client = new MongoClient(Settings);
			this.database = this.client.GetDatabase(DatabaseName);

			this.defaultCollectionName = DefaultCollectionName;
			this.defaultCollection = this.GetCollection(this.defaultCollectionName);
		}

		/// <summary>
		/// Gets a collection.
		/// </summary>
		/// <param name="CollectionName">Name of collection.</param>
		/// <returns></returns>
		public IMongoCollection<BsonDocument> GetCollection(string CollectionName)
		{
			IMongoCollection<BsonDocument> Result;

			lock (this.collections)
			{
				if (CollectionName == this.lastCollectionName)
					Result = this.lastCollection;
				else
				{
					if (!this.collections.TryGetValue(CollectionName, out Result))
					{
						Result = this.database.GetCollection<BsonDocument>(CollectionName);
						this.collections[CollectionName] = Result;
					}

					this.lastCollection = Result;
					this.lastCollectionName = CollectionName;
				}
			}

			return Result;
		}

		private ObjectSerializer GetObjectSerializer(object Object)
		{
			return this.GetObjectSerializer(Object.GetType());
		}

		/// <summary>
		/// Underlying MongoDB client.
		/// </summary>
		public MongoClient Client
		{
			get { return this.client; }
		}

		/// <summary>
		/// Default collection name.
		/// </summary>
		public string DefaultCollectionName
		{
			get { return this.defaultCollectionName; }
		}

		/// <summary>
		/// Default collection.
		/// </summary>
		public IMongoCollection<BsonDocument> DefaultCollection
		{
			get { return this.defaultCollection; }
		}

		/// <summary>
		/// Returns a serializer for a given type.
		/// </summary>
		/// <param name="Type">Type of objects to serialize.</param>
		/// <returns>Object serializer.</returns>
		public ObjectSerializer GetObjectSerializer(Type Type)
		{
			string TypeFullName = Type.FullName;
			ObjectSerializer Result;

			lock (this.collections)
			{
				if (TypeFullName == this.lastSerializerName)
					Result = this.lastSerializer;
				else
				{
					if (!this.serializers.TryGetValue(TypeFullName, out Result))
					{
						Result = new ObjectSerializer(Type, this);
						this.serializers[TypeFullName] = Result;
					}

					this.lastSerializer = Result;
					this.lastSerializerName = TypeFullName;
				}
			}

			return Result;
		}

		/// <summary>
		/// Inserts an object into the database.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public async Task Insert(object Object)
		{
			ObjectSerializer Serializer = this.GetObjectSerializer(Object);
			string CollectionName = Serializer.CollectionName;
			IMongoCollection<BsonDocument> Collection;

			if (string.IsNullOrEmpty(CollectionName))
				Collection = this.defaultCollection;
			else
				Collection = this.GetCollection(CollectionName);

			if (Serializer.HasObjectIdField)
			{
				if (Serializer.HasObjectId(Object))
					throw new Exception("Object already has an Object ID. If updating an object, use the Update method.");
				else
					await Serializer.GetObjectId(Object, true);
			}
			else
			{
				BsonDocument Doc = Object.ToBsonDocument(Object.GetType(), Serializer);
				await Collection.InsertOneAsync(Doc);
			}
		}

		/// <summary>
		/// Inserts a collection of objects into the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public Task Insert(params object[] Objects)
		{
			return this.Insert((IEnumerable<object>)Objects);
		}

		/// <summary>
		/// Inserts a collection of objects into the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public async Task Insert(IEnumerable<object> Objects)
		{
			Dictionary<string, KeyValuePair<IMongoCollection<BsonDocument>, LinkedList<BsonDocument>>> DocumentsPerCollection =
				new Dictionary<string, KeyValuePair<IMongoCollection<BsonDocument>, LinkedList<BsonDocument>>>();
			Type Type;
			Type LastType = null;
			ObjectSerializer Serializer = null;
			string CollectionName;
			string LastCollectionName = null;
			IMongoCollection<BsonDocument> Collection;
			IMongoCollection<BsonDocument> LastCollection = null;
			LinkedList<BsonDocument> Documents = null;
			BsonDocument Document;

			foreach (object Object in Objects)
			{
				Type = Object.GetType();

				if (Type != LastType)
				{
					Serializer = this.GetObjectSerializer(Type);
					CollectionName = Serializer.CollectionName;
					LastType = Type;

					if (CollectionName == LastCollectionName)
						Collection = LastCollection;
					else
					{
						LastCollectionName = CollectionName;

						if (string.IsNullOrEmpty(CollectionName))
							CollectionName = this.defaultCollectionName;

						if (DocumentsPerCollection.TryGetValue(CollectionName, out KeyValuePair<IMongoCollection<BsonDocument>, LinkedList<BsonDocument>> P))
							Collection = P.Key;
						else
						{
							Collection = this.GetCollection(CollectionName);
							P = new KeyValuePair<IMongoCollection<BsonDocument>, LinkedList<BsonDocument>>(Collection, new LinkedList<BsonDocument>());
							DocumentsPerCollection[CollectionName] = P;
						}

						Documents = P.Value;
						LastCollection = Collection;
					}
				}

				Document = Object.ToBsonDocument(Type, Serializer);
				Documents.AddLast(Document);
			}

			foreach (KeyValuePair<IMongoCollection<BsonDocument>, LinkedList<BsonDocument>> P2 in DocumentsPerCollection.Values)
				await P2.Key.InsertManyAsync(P2.Value);
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
		public Task<IEnumerable<T>> Find<T>(int Offset, int MaxCount, params string[] SortOrder)
		{
			return this.Find<T>(Offset, MaxCount, (Filter)null, SortOrder);
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
		public Task<IEnumerable<T>> Find<T>(int Offset, int MaxCount, Filter Filter, params string[] SortOrder)
		{
			ObjectSerializer Serializer = this.GetObjectSerializer(typeof(T));
			string CollectionName = Serializer.CollectionName;
			IMongoCollection<BsonDocument> Collection;
			FilterDefinition<BsonDocument> BsonFilter;

			if (string.IsNullOrEmpty(CollectionName))
				Collection = this.defaultCollection;
			else
				Collection = this.GetCollection(CollectionName);

			if (Filter == null)
				BsonFilter = new BsonDocument();
			else
				BsonFilter = this.Convert(Filter, Serializer);

			return this.Find<T>(Serializer, Collection, Offset, MaxCount, BsonFilter, SortOrder);
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="BsonFilter">Search filter.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public Task<IEnumerable<T>> Find<T>(int Offset, int MaxCount, FilterDefinition<BsonDocument> BsonFilter,
			params string[] SortOrder)
		{
			ObjectSerializer Serializer = this.GetObjectSerializer(typeof(T));
			string CollectionName = Serializer.CollectionName;
			IMongoCollection<BsonDocument> Collection;

			if (string.IsNullOrEmpty(CollectionName))
				Collection = this.defaultCollection;
			else
				Collection = this.GetCollection(CollectionName);

			return this.Find<T>(Serializer, Collection, Offset, MaxCount, BsonFilter, SortOrder);
		}

		private async Task<IEnumerable<T>> Find<T>(ObjectSerializer Serializer, IMongoCollection<BsonDocument> Collection,
			int Offset, int MaxCount, FilterDefinition<BsonDocument> BsonFilter, params string[] SortOrder)
		{
			IFindFluent<BsonDocument, BsonDocument> ResultSet = Collection.Find<BsonDocument>(BsonFilter);

			if (SortOrder.Length > 0)
			{
				SortDefinition<BsonDocument> SortDefinition = null;

				foreach (string SortBy in SortOrder)
				{
					if (SortDefinition == null)
					{
						if (SortBy.StartsWith("-"))
							SortDefinition = Builders<BsonDocument>.Sort.Descending(Serializer.ToShortName(SortBy.Substring(1)));
						else
							SortDefinition = Builders<BsonDocument>.Sort.Ascending(Serializer.ToShortName(SortBy));
					}
					else
					{
						if (SortBy.StartsWith("-"))
							SortDefinition = SortDefinition.Descending(Serializer.ToShortName(SortBy.Substring(1)));
						else
							SortDefinition = SortDefinition.Ascending(Serializer.ToShortName(SortBy));
					}
				}

				ResultSet = ResultSet.Sort(SortDefinition);
			}

			if (Offset > 0)
				ResultSet = ResultSet.Skip(Offset);

			if (MaxCount < int.MaxValue)
				ResultSet = ResultSet.Limit(MaxCount);

			IAsyncCursor<BsonDocument> Cursor = await ResultSet.ToCursorAsync();
			LinkedList<T> Result = new LinkedList<T>();
			BsonDeserializationArgs Args = new BsonDeserializationArgs()
			{
				NominalType = typeof(T)
			};

			while (await Cursor.MoveNextAsync())
			{
				foreach (BsonDocument Document in Cursor.Current)
				{
					BsonDocumentReader Reader = new BsonDocumentReader(Document);
					BsonDeserializationContext Context = BsonDeserializationContext.CreateRoot(Reader);

					T Obj = (T)Serializer.Deserialize(Context, Args);
					Result.AddLast(Obj);
				}
			}

			return Result;
		}

		private FilterDefinition<BsonDocument> Convert(Filter Filter, ObjectSerializer Serializer)
		{
			if (Filter is FilterChildren FilterChildren)
			{
				Filter[] ChildFilters = FilterChildren.ChildFilters;
				int i, c = ChildFilters.Length;
				FilterDefinition<BsonDocument>[] Children = new FilterDefinition<BsonDocument>[c];

				for (i = 0; i < c; i++)
					Children[i] = this.Convert(ChildFilters[i], Serializer);

				if (Filter is FilterAnd)
					return Builders<BsonDocument>.Filter.And(Children);
				else if (Filter is FilterOr)
					return Builders<BsonDocument>.Filter.Or(Children);
				else
					throw this.UnknownFilterType(Filter);
			}
			else if (Filter is FilterChild FilterChild)
			{
				FilterDefinition<BsonDocument> Child = this.Convert(FilterChild.ChildFilter, Serializer);

				if (Filter is FilterNot)
					return Builders<BsonDocument>.Filter.Not(Child);
				else
					throw this.UnknownFilterType(Filter);
			}
			else if (Filter is FilterFieldValue FilterFieldValue)
			{
				object Value = FilterFieldValue.Value;
				string FieldName = Serializer.ToShortName(FilterFieldValue.FieldName, ref Value);
				bool IsDefaultValue = Serializer.IsDefaultValue(FilterFieldValue.FieldName, Value);

				if (Filter is FilterFieldEqualTo)
				{
					if (IsDefaultValue)
						return Builders<BsonDocument>.Filter.Eq<string>(FieldName, null);
					else if (Value is string)
						return Builders<BsonDocument>.Filter.Eq<string>(FieldName, (string)Value);
					else if (Value is int)
						return Builders<BsonDocument>.Filter.Eq<int>(FieldName, (int)Value);
					else if (Value is long)
						return Builders<BsonDocument>.Filter.Eq<long>(FieldName, (long)Value);
					else if (Value is double)
						return Builders<BsonDocument>.Filter.Eq<double>(FieldName, (double)Value);
					else if (Value is bool)
						return Builders<BsonDocument>.Filter.Eq<bool>(FieldName, (bool)Value);
					else if (Value is DateTime)
						return Builders<BsonDocument>.Filter.Eq<DateTime>(FieldName, (DateTime)Value);
					else if (Value is ObjectId)
						return Builders<BsonDocument>.Filter.Eq<ObjectId>(FieldName, (ObjectId)Value);
					else
						throw this.UnhandledFilterValueDataType(Serializer.ValueType.FullName, FieldName, Value);
				}
				else if (Filter is FilterFieldNotEqualTo)
				{
					if (IsDefaultValue)
						return Builders<BsonDocument>.Filter.Ne<string>(FieldName, null);
					else if (Value is string)
						return Builders<BsonDocument>.Filter.Ne<string>(FieldName, (string)Value);
					else if (Value is int)
						return Builders<BsonDocument>.Filter.Ne<int>(FieldName, (int)Value);
					else if (Value is long)
						return Builders<BsonDocument>.Filter.Ne<long>(FieldName, (long)Value);
					else if (Value is double)
						return Builders<BsonDocument>.Filter.Ne<double>(FieldName, (double)Value);
					else if (Value is bool)
						return Builders<BsonDocument>.Filter.Ne<bool>(FieldName, (bool)Value);
					else if (Value is DateTime)
						return Builders<BsonDocument>.Filter.Ne<DateTime>(FieldName, (DateTime)Value);
					else if (Value is ObjectId)
						return Builders<BsonDocument>.Filter.Ne<ObjectId>(FieldName, (ObjectId)Value);
					else
						throw this.UnhandledFilterValueDataType(Serializer.ValueType.FullName, FieldName, Value);
				}
				else if (Filter is FilterFieldGreaterThan)
				{
					if (Value is string)
						return Builders<BsonDocument>.Filter.Gt<string>(FieldName, (string)Value);
					else if (Value is int)
						return Builders<BsonDocument>.Filter.Gt<int>(FieldName, (int)Value);
					else if (Value is long)
						return Builders<BsonDocument>.Filter.Gt<long>(FieldName, (long)Value);
					else if (Value is double)
						return Builders<BsonDocument>.Filter.Gt<double>(FieldName, (double)Value);
					else if (Value is bool)
						return Builders<BsonDocument>.Filter.Gt<bool>(FieldName, (bool)Value);
					else if (Value is DateTime)
						return Builders<BsonDocument>.Filter.Gt<DateTime>(FieldName, (DateTime)Value);
					else if (Value is ObjectId)
						return Builders<BsonDocument>.Filter.Gt<ObjectId>(FieldName, (ObjectId)Value);
					else
						throw this.UnhandledFilterValueDataType(Serializer.ValueType.FullName, FieldName, Value);
				}
				else if (Filter is FilterFieldGreaterOrEqualTo)
				{
					if (IsDefaultValue)
					{
						return this.Convert(new FilterOr(new FilterFieldGreaterThan(FieldName, Value),
							new FilterFieldEqualTo(FieldName, Value)), Serializer);
					}
					else if (Value is string)
						return Builders<BsonDocument>.Filter.Gte<string>(FieldName, (string)Value);
					else if (Value is int)
						return Builders<BsonDocument>.Filter.Gte<int>(FieldName, (int)Value);
					else if (Value is long)
						return Builders<BsonDocument>.Filter.Gte<long>(FieldName, (long)Value);
					else if (Value is double)
						return Builders<BsonDocument>.Filter.Gte<double>(FieldName, (double)Value);
					else if (Value is bool)
						return Builders<BsonDocument>.Filter.Gte<bool>(FieldName, (bool)Value);
					else if (Value is DateTime)
						return Builders<BsonDocument>.Filter.Gte<DateTime>(FieldName, (DateTime)Value);
					else if (Value is ObjectId)
						return Builders<BsonDocument>.Filter.Gte<ObjectId>(FieldName, (ObjectId)Value);
					else
						throw this.UnhandledFilterValueDataType(Serializer.ValueType.FullName, FieldName, Value);
				}
				else if (Filter is FilterFieldLesserThan)
				{
					if (Value is string)
						return Builders<BsonDocument>.Filter.Lt<string>(FieldName, (string)Value);
					else if (Value is int)
						return Builders<BsonDocument>.Filter.Lt<int>(FieldName, (int)Value);
					else if (Value is long)
						return Builders<BsonDocument>.Filter.Lt<long>(FieldName, (long)Value);
					else if (Value is double)
						return Builders<BsonDocument>.Filter.Lt<double>(FieldName, (double)Value);
					else if (Value is bool)
						return Builders<BsonDocument>.Filter.Lt<bool>(FieldName, (bool)Value);
					else if (Value is DateTime)
						return Builders<BsonDocument>.Filter.Lt<DateTime>(FieldName, (DateTime)Value);
					else if (Value is ObjectId)
						return Builders<BsonDocument>.Filter.Lt<ObjectId>(FieldName, (ObjectId)Value);
					else
						throw this.UnhandledFilterValueDataType(Serializer.ValueType.FullName, FieldName, Value);
				}
				else if (Filter is FilterFieldLesserOrEqualTo)
				{
					if (IsDefaultValue)
					{
						return this.Convert(new FilterOr(new FilterFieldLesserThan(FieldName, Value),
							new FilterFieldEqualTo(FieldName, Value)), Serializer);
					}
					else if (Value is string)
						return Builders<BsonDocument>.Filter.Lte<string>(FieldName, (string)Value);
					else if (Value is int)
						return Builders<BsonDocument>.Filter.Lte<int>(FieldName, (int)Value);
					else if (Value is long)
						return Builders<BsonDocument>.Filter.Lte<long>(FieldName, (long)Value);
					else if (Value is double)
						return Builders<BsonDocument>.Filter.Lte<double>(FieldName, (double)Value);
					else if (Value is bool)
						return Builders<BsonDocument>.Filter.Lte<bool>(FieldName, (bool)Value);
					else if (Value is DateTime)
						return Builders<BsonDocument>.Filter.Lte<DateTime>(FieldName, (DateTime)Value);
					else if (Value is ObjectId)
						return Builders<BsonDocument>.Filter.Lte<ObjectId>(FieldName, (ObjectId)Value);
					else
						throw this.UnhandledFilterValueDataType(Serializer.ValueType.FullName, FieldName, Value);
				}
				else
					throw this.UnknownFilterType(Filter);
			}
			else
			{
				if (Filter is FilterFieldLikeRegEx FilterFieldLikeRegEx)
				{
					return Builders<BsonDocument>.Filter.Regex(Serializer.ToShortName(FilterFieldLikeRegEx.FieldName),
						FilterFieldLikeRegEx.RegularExpression);
				}
				else
					throw this.UnknownFilterType(Filter);
			}
		}

		private Exception UnknownFilterType(Filter Filter)
		{
			return new NotSupportedException("Filters of type " + Filter.GetType().FullName + " not supported.");
		}

		private Exception UnhandledFilterValueDataType(string TypeName, string FieldName, object Value)
		{
			if (Value == null)
			{
				return new NotSupportedException("Null filter values for field " + TypeName + "." + FieldName +
					  " not supported.");
			}
			else
			{
				return new NotSupportedException("Filter values of type " + Value.GetType().FullName +
					" for field " + TypeName + "." + FieldName + " not supported.");
			}
		}

		/// <summary>
		/// Loads an object given its Object ID <paramref name="ObjectId"/> and its base type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Base type.</typeparam>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>Loaded object.</returns>
		public Task<T> LoadObject<T>(object ObjectId)
		{
			ObjectId OID;

			if (ObjectId is ObjectId)
				OID = (ObjectId)ObjectId;
			else if (ObjectId is string)
				OID = new ObjectId((string)ObjectId);
			else if (ObjectId is byte[])
				OID = new ObjectId((byte[])ObjectId);
			else
				throw new NotSupportedException("Unsupported type for Object ID: " + ObjectId.GetType().FullName);

			return this.LoadObject<T>(OID);
		}

		/// <summary>
		/// Loads an object of a given type and Object ID.
		/// </summary>
		/// <typeparam name="T">Type of object to load.</typeparam>
		/// <param name="ObjectId">Object ID of object to load.</param>
		/// <returns>Loaded object.</returns>
		public async Task<T> LoadObject<T>(ObjectId ObjectId)
		{
			string Key = typeof(T).FullName + " " + ObjectId.ToString();

			if (this.loadCache.TryGetValue(Key, out object Obj) && Obj is T)
				return (T)Obj;

			ObjectSerializer S = this.GetObjectSerializer(typeof(T));
			IEnumerable<T> ReferencedObjects = await this.Find<T>(0, 2, new FilterFieldEqualTo(S.ObjectIdMemberName, ObjectId));
			T First = default(T);

			foreach (T Item in ReferencedObjects)
			{
				if (First == null)
					First = Item;
				else
					throw new Exception("Multiple objects of type T found with object ID " + ObjectId.ToString());
			}

			if (First == null)
				throw new Exception("Referenced object of type T not found: " + ObjectId.ToString());

			this.loadCache.Add(Key, First);     // Speeds up readout if reading multiple objects referencing a few common sub-objects.

			return First;
		}

		private readonly Cache<string, object> loadCache = new Cache<string, object>(10000, new TimeSpan(0, 0, 10), new TimeSpan(0, 0, 5));  // TODO: Make parameters configurable.

		/// <summary>
		/// Updates an object in the database.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public async Task Update(object Object)
		{
			ObjectSerializer Serializer = this.GetObjectSerializer(Object);
			ObjectId ObjectId = await Serializer.GetObjectId(Object, false);
			string CollectionName = Serializer.CollectionName;
			IMongoCollection<BsonDocument> Collection;

			if (string.IsNullOrEmpty(CollectionName))
				Collection = this.defaultCollection;
			else
				Collection = this.GetCollection(CollectionName);

			BsonDocument Doc = Object.ToBsonDocument(Object.GetType(), Serializer);
			await Collection.ReplaceOneAsync(Builders<BsonDocument>.Filter.Eq<ObjectId>("_id", ObjectId), Doc);
		}

		/// <summary>
		/// Updates a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public Task Update(params object[] Objects)
		{
			return this.Update((IEnumerable<object>)Objects);
		}

		/// <summary>
		/// Updates a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public async Task Update(IEnumerable<object> Objects)
		{
			foreach (object Obj in Objects)
				await this.Update(Obj);
		}

		/// <summary>
		/// Deletes an object in the database.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public async Task Delete(object Object)
		{
			ObjectSerializer Serializer = this.GetObjectSerializer(Object);
			ObjectId ObjectId = await Serializer.GetObjectId(Object, false);
			string CollectionName = Serializer.CollectionName;
			IMongoCollection<BsonDocument> Collection;

			if (string.IsNullOrEmpty(CollectionName))
				Collection = this.defaultCollection;
			else
				Collection = this.GetCollection(CollectionName);

			await Collection.DeleteOneAsync(Builders<BsonDocument>.Filter.Eq<ObjectId>("_id", ObjectId));
		}

		/// <summary>
		/// Deletes a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public Task Delete(params object[] Objects)
		{
			return this.Delete((IEnumerable<object>)Objects);
		}

		/// <summary>
		/// Deletes a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public async Task Delete(IEnumerable<object> Objects)
		{
			foreach (object Obj in Objects)
				await this.Delete(Obj);
		}

		/// <summary>
		/// Performs an export of the entire database.
		/// </summary>
		/// <param name="Output">Database will be output to this interface.</param>
		/// <returns>Task object for synchronization purposes.</returns>
		public Task Export(IDatabaseExport Output)
		{
			throw new NotImplementedException("MongoDB provider does not support the Export method.");	// TODO
		}

		/// <summary>
		/// Clears a collection of all objects.
		/// </summary>
		/// <param name="CollectionName">Name of collection to clear.</param>
		/// <returns>Task object for synchronization purposes.</returns>
		public Task Clear(string CollectionName)
		{
			throw new NotImplementedException("MongoDB provider does not support the Clear method.");  // TODO
		}

		/// <summary>
		/// Analyzes the database and exports findings to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="XsltPath">Optional XSLT to use to view the output.</param>
		/// <param name="ProgramDataFolder">Program data folder. Can be removed from filenames used, when referencing them in the report.</param>
		/// <param name="ExportData">If data in database is to be exported in output.</param>
		public Task Analyze(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData)
		{
			throw new NotImplementedException("MongoDB provider does not support the Analyze method.");  // TODO
		}

		/// <summary>
		/// Analyzes the database and repairs it if necessary. Results are exported to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="XsltPath">Optional XSLT to use to view the output.</param>
		/// <param name="ProgramDataFolder">Program data folder. Can be removed from filenames used, when referencing them in the report.</param>
		/// <param name="ExportData">If data in database is to be exported in output.</param>
		public Task Repair(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData)
		{
			throw new NotImplementedException("MongoDB provider does not support the Repair method.");  // TODO
		}

		/// <summary>
		/// Analyzes the database and exports findings to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="XsltPath">Optional XSLT to use to view the output.</param>
		/// <param name="ProgramDataFolder">Program data folder. Can be removed from filenames used, when referencing them in the report.</param>
		/// <param name="ExportData">If data in database is to be exported in output.</param>
		/// <param name="Repair">If files should be repaired if corruptions are detected.</param>
		public Task Analyze(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData, bool Repair)
		{
			throw new NotImplementedException("MongoDB provider does not support the Analyze method.");  // TODO
		}

		/// <summary>
		/// Adds an index to a collection, if one does not already exist.
		/// </summary>
		/// <param name="CollectionName">Name of collection.</param>
		/// <param name="FieldNames">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		public Task AddIndex(string CollectionName, string[] FieldNames)
		{
			throw new NotImplementedException("MongoDB provider does not support the AddIndex method.");  // TODO
		}

		/// <summary>
		/// Starts bulk-proccessing of data. Must be followed by a call to <see cref="EndBulk"/>.
		/// </summary>
		public Task StartBulk()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Ends bulk-processing of data. Must be called once for every call to <see cref="StartBulk"/>.
		/// </summary>
		public Task EndBulk()
		{
			return Task.CompletedTask;
		}

		// TODO:
		//	* Created field
		//	* Updated field
		//	* RegEx fields
		//	* Javascript fields
		//	* Binary fields (BLOBS)
		//	* Image fields
		//	* Collection indices.
		//	* Dictionary<string,T> fields.
		//	* SortedDictionary<string,T> fields.
		//	* Aggregates
	}
	}
