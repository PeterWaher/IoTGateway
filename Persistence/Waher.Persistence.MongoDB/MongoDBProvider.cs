using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Waher.Events;
using Waher.Persistence.Filters;
using Waher.Persistence.MongoDB.Serialization;
using Waher.Persistence.MongoDB.Serialization.ReferenceTypes;
using Waher.Persistence.MongoDB.Serialization.ValueTypes;
using Waher.Persistence.Serialization;
using Waher.Runtime.Cache;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;
using Waher.Runtime.Profiling;

namespace Waher.Persistence.MongoDB
{
	/// <summary>
	/// MongoDB database provider.
	/// </summary>
	public class MongoDBProvider : IDatabaseProvider
	{
		private readonly Dictionary<string, IMongoCollection<BsonDocument>> collections = new Dictionary<string, IMongoCollection<BsonDocument>>();
		private readonly Dictionary<Type, IObjectSerializer> serializers = new Dictionary<Type, IObjectSerializer>();
		private readonly AutoResetEvent serializerAdded = new AutoResetEvent(false);
		private MongoClient client;
		private IMongoDatabase database;
		private string id;
		private string defaultCollectionName;
		private string lastCollectionName = null;
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
			this.id = Guid.NewGuid().ToString().Replace("-", string.Empty);
			this.client = new MongoClient(Settings);
			this.database = this.client.GetDatabase(DatabaseName);

			this.defaultCollectionName = DefaultCollectionName;
			this.defaultCollection = this.GetCollection(this.defaultCollectionName);

			ConstructorInfo DefaultConstructor;
			IObjectSerializer S;

			foreach (Type T in Waher.Runtime.Inventory.Types.GetTypesImplementingInterface(typeof(IObjectSerializer)))
			{
				try
				{
					DefaultConstructor = Types.GetDefaultConstructor(T);
					if (DefaultConstructor is null)
						continue;

					S = DefaultConstructor.Invoke(Types.NoParameters) as IObjectSerializer;
					if (S is null)
						continue;
				}
				catch (Exception)
				{
					continue;
				}

				this.serializers[S.ValueType] = S;
			}

			this.serializers[typeof(GenericObject)] = new GenericObjectSerializer(this, false);
			this.serializers[typeof(object)] = new GenericObjectSerializer(this, true);
		}

		/// <summary>
		/// An ID of the files provider. It's unique, and constant during the life-time of the MongoDBProvider class.
		/// </summary>
		public string Id => this.id;

		/// <summary>
		/// Number of bytes used by an Object ID.
		/// </summary>
		public int ObjectIdByteCount => 12;

		/// <summary>
		/// Gets a collection.
		/// </summary>
		/// <param name="CollectionName">Name of collection.</param>
		/// <returns>Collection</returns>
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

		/// <summary>
		/// Underlying MongoDB client.
		/// </summary>
		public MongoClient Client => this.client;

		/// <summary>
		/// Default collection name.
		/// </summary>
		public string DefaultCollectionName => this.defaultCollectionName;

		/// <summary>
		/// Default collection.
		/// </summary>
		public IMongoCollection<BsonDocument> DefaultCollection => this.defaultCollection;

		/// <summary>
		/// Returns a serializer for a given type.
		/// </summary>
		/// <param name="Type">Type of objects to serialize.</param>
		/// <returns>Object serializer.</returns>
		public IObjectSerializer GetObjectSerializer(Type Type)
		{
			IObjectSerializer Result;
			TypeInfo TI = Type.GetTypeInfo();

			lock (this.collections)
			{
				if (this.serializers.TryGetValue(Type, out Result))
					return Result;

				if (TI.IsEnum)
					Result = new EnumSerializer(Type);
				else if (Type.IsArray)
				{
					Type ElementType = Type.GetElementType();
					Type T = Waher.Runtime.Inventory.Types.GetType(typeof(ByteArraySerializer).FullName.Replace("ByteArray", "Array"));
					Type SerializerType = T.MakeGenericType(new Type[] { ElementType });
					Result = (IObjectSerializer)Activator.CreateInstance(SerializerType, this);
				}
				else if (TI.IsGenericType)
				{
					Type GT = Type.GetGenericTypeDefinition();
					if (GT == typeof(Nullable<>))
					{
						Type NullableType = Type.GenericTypeArguments[0];

						if (NullableType.IsEnum)
							Result = new Serialization.NullableTypes.NullableEnumSerializer(NullableType);
						else
							Result = null;
					}
					else
						Result = null;
				}
				else
					Result = null;

				if (!(Result is null))
				{
					this.serializers[Type] = Result;
					this.serializerAdded.Set();

					return Result;
				}
			}

			try
			{
				Result = new ObjectSerializer(Type, this);

				lock (this.collections)
				{
					this.serializers[Type] = Result;
					this.serializerAdded.Set();
				}
			}
			catch (FileLoadException ex)
			{
				// Serializer in the process of being generated from another task or thread.

				while (true)
				{
					if (!this.serializerAdded.WaitOne(1000))
						ExceptionDispatchInfo.Capture(ex).Throw();

					lock (this.collections)
					{
						if (this.serializers.TryGetValue(Type, out Result))
							return Result;
					}
				}
			}

			return Result;
		}

		/// <summary>
		/// Gets the object serializer corresponding to a specific object.
		/// </summary>
		/// <param name="Object">Object to serialize</param>
		/// <returns>Object Serializer</returns>
		public ObjectSerializer GetObjectSerializerEx(object Object)
		{
			return this.GetObjectSerializerEx(Object.GetType());
		}

		/// <summary>
		/// Gets the object serializer corresponding to a specific object.
		/// </summary>
		/// <param name="Type">Type of object to serialize.</param>
		/// <returns>Object Serializer</returns>
		public ObjectSerializer GetObjectSerializerEx(Type Type)
		{
			if (!(this.GetObjectSerializer(Type) is ObjectSerializer Serializer))
				throw new Exception("Objects of type " + Type.FullName + " must be embedded.");

			return Serializer;
		}

		/// <summary>
		/// Inserts an object into the database.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public async Task Insert(object Object)
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(Object);
			string CollectionName = Serializer.CollectionName(Object);
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
					Serializer = this.GetObjectSerializerEx(Type);
					CollectionName = Serializer.CollectionName(Object);
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
		/// Inserts an object into the database, if unlocked. If locked, object will be inserted at next opportunity.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		public Task InsertLazy(object Object, ObjectCallback Callback)
			=> this.Process(Object, this.Insert(Object), Callback);

		/// <summary>
		/// Inserts an object into the database, if unlocked. If locked, object will be inserted at next opportunity.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		public Task InsertLazy(object[] Objects, ObjectsCallback Callback)
			=> this.Process(Objects, this.Insert(Objects), Callback);

		/// <summary>
		/// Inserts an object into the database, if unlocked. If locked, object will be inserted at next opportunity.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		public Task InsertLazy(IEnumerable<object> Objects, ObjectsCallback Callback)
			=> this.Process(Objects, this.Insert(Objects), Callback);

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
			where T : class
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
			where T : class
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(typeof(T));
			string CollectionName = Serializer.CollectionName(null);
			IMongoCollection<BsonDocument> Collection;
			FilterDefinition<BsonDocument> BsonFilter;

			if (string.IsNullOrEmpty(CollectionName))
				Collection = this.defaultCollection;
			else
				Collection = this.GetCollection(CollectionName);

			if (Filter is null)
				BsonFilter = new BsonDocument();
			else
				BsonFilter = this.Convert(Filter, Serializer);

			return this.Find<T>(Serializer, Collection, Offset, MaxCount, BsonFilter, null, SortOrder);
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="ContinueAfter">Continue returning results after this object.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public Task<IEnumerable<T>> Find<T>(int Offset, int MaxCount, Filter Filter,
			T ContinueAfter, params string[] SortOrder)
			where T : class
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(typeof(T));
			string CollectionName = Serializer.CollectionName(null);
			IMongoCollection<BsonDocument> Collection;
			FilterDefinition<BsonDocument> BsonFilter;

			if (string.IsNullOrEmpty(CollectionName))
				Collection = this.defaultCollection;
			else
				Collection = this.GetCollection(CollectionName);

			if (Filter is null)
				BsonFilter = new BsonDocument();
			else
				BsonFilter = this.Convert(Filter, Serializer);

			return this.Find(Serializer, Collection, Offset, MaxCount, BsonFilter, ContinueAfter, SortOrder);
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
			where T : class
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(typeof(T));
			string CollectionName = Serializer.CollectionName(null);
			IMongoCollection<BsonDocument> Collection;

			if (string.IsNullOrEmpty(CollectionName))
				Collection = this.defaultCollection;
			else
				Collection = this.GetCollection(CollectionName);

			return this.Find<T>(Serializer, Collection, Offset, MaxCount, BsonFilter, null, SortOrder);
		}

		/// <summary>
		/// Finds objects in a given collection.
		/// </summary>
		/// <param name="CollectionName">Collection Name</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public Task<IEnumerable<T>> Find<T>(string CollectionName, int Offset, int MaxCount, Filter Filter, params string[] SortOrder)
			where T : class
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(typeof(T));
			IMongoCollection<BsonDocument> Collection;
			FilterDefinition<BsonDocument> BsonFilter;

			if (string.IsNullOrEmpty(CollectionName))
				Collection = this.defaultCollection;
			else
				Collection = this.GetCollection(CollectionName);

			if (Filter is null)
				BsonFilter = new BsonDocument();
			else
				BsonFilter = this.Convert(Filter, Serializer);

			return this.Find<T>(Serializer, Collection, Offset, MaxCount, BsonFilter, null, SortOrder);
		}

		/// <summary>
		/// Finds objects in a given collection.
		/// </summary>
		/// <param name="CollectionName">Collection Name</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="ContinueAfter">Continue returning results after this object.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public Task<IEnumerable<T>> Find<T>(string CollectionName, int Offset, int MaxCount, Filter Filter,
			T ContinueAfter, params string[] SortOrder)
			where T : class
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(typeof(T));
			IMongoCollection<BsonDocument> Collection;
			FilterDefinition<BsonDocument> BsonFilter;

			if (string.IsNullOrEmpty(CollectionName))
				Collection = this.defaultCollection;
			else
				Collection = this.GetCollection(CollectionName);

			if (Filter is null)
				BsonFilter = new BsonDocument();
			else
				BsonFilter = this.Convert(Filter, Serializer);

			return this.Find(Serializer, Collection, Offset, MaxCount, BsonFilter, ContinueAfter, SortOrder);
		}

		/// <summary>
		/// Finds objects in a given collection.
		/// </summary>
		/// <param name="CollectionName">Collection Name</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public Task<IEnumerable<object>> Find(string CollectionName, int Offset, int MaxCount, params string[] SortOrder)
		{
			return this.Find(CollectionName, Offset, MaxCount, null, SortOrder);
		}

		/// <summary>
		/// Finds objects in a given collection.
		/// </summary>
		/// <param name="CollectionName">Collection Name</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public Task<IEnumerable<object>> Find(string CollectionName, int Offset, int MaxCount, Filter Filter, params string[] SortOrder)
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(typeof(object));
			IMongoCollection<BsonDocument> Collection;
			FilterDefinition<BsonDocument> BsonFilter;

			if (string.IsNullOrEmpty(CollectionName))
				Collection = this.defaultCollection;
			else
				Collection = this.GetCollection(CollectionName);

			if (Filter is null)
				BsonFilter = new BsonDocument();
			else
				BsonFilter = this.Convert(Filter, Serializer);

			return this.Find<object>(Serializer, Collection, Offset, MaxCount, BsonFilter, SortOrder);
		}

		private async Task<IEnumerable<T>> Find<T>(ObjectSerializer Serializer, IMongoCollection<BsonDocument> Collection,
			int Offset, int MaxCount, FilterDefinition<BsonDocument> BsonFilter, T ContinueAfter, params string[] SortOrder)
			where T : class
		{
			if (!(ContinueAfter is null))
				throw new NotImplementedException("Paginated searches not implemented in MongoDB provider.");

			IFindFluent<BsonDocument, BsonDocument> ResultSet = Collection.Find(BsonFilter);

			if (SortOrder.Length > 0)
			{
				SortDefinition<BsonDocument> SortDefinition = null;

				foreach (string SortBy in SortOrder)
				{
					if (SortDefinition is null)
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

					if (Serializer.Deserialize(Context, Args) is T Obj)
						Result.AddLast(Obj);
				}
			}

			return Result;
		}

		internal FilterDefinition<BsonDocument> Convert(Filter Filter, ObjectSerializer Serializer)
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
				bool HasType = Serializer.TryGetFieldType(FilterFieldValue.FieldName, null, out Type FieldType);
				bool IsDefaultValue = Serializer.IsDefaultValue(FilterFieldValue.FieldName, Value);

				if (Filter is FilterFieldEqualTo)
				{
					if (IsDefaultValue)
						return Builders<BsonDocument>.Filter.Eq<string>(FieldName, null);
					else if (Value is string s)
					{
						if (HasType && FieldType == typeof(CaseInsensitiveString))
							return Builders<BsonDocument>.Filter.Eq<string>(FieldName + "_L", s);
						else
							return Builders<BsonDocument>.Filter.Eq<string>(FieldName, s);
					}
					else if (Value is CaseInsensitiveString cis)
						return Builders<BsonDocument>.Filter.Eq<string>(FieldName + "_L", cis.LowerCase);
					else if (Value is sbyte i8)
						return Builders<BsonDocument>.Filter.Eq<int>(FieldName, i8);
					else if (Value is short i16)
						return Builders<BsonDocument>.Filter.Eq<int>(FieldName, i16);
					else if (Value is int i32)
						return Builders<BsonDocument>.Filter.Eq<int>(FieldName, i32);
					else if (Value is long i64)
						return Builders<BsonDocument>.Filter.Eq<long>(FieldName, i64);
					else if (Value is byte ui8)
						return Builders<BsonDocument>.Filter.Eq<int>(FieldName, ui8);
					else if (Value is ushort ui16)
						return Builders<BsonDocument>.Filter.Eq<int>(FieldName, ui16);
					else if (Value is uint ui32)
						return Builders<BsonDocument>.Filter.Eq<long>(FieldName, ui32);
					else if (Value is ulong ui64)
						return Builders<BsonDocument>.Filter.Eq<Decimal128>(FieldName, ui64);
					else if (Value is double d)
						return Builders<BsonDocument>.Filter.Eq<double>(FieldName, d);
					else if (Value is float f)
						return Builders<BsonDocument>.Filter.Eq<double>(FieldName, f);
					else if (Value is decimal d2)
						return Builders<BsonDocument>.Filter.Eq<Decimal128>(FieldName, d2);
					else if (Value is bool b)
						return Builders<BsonDocument>.Filter.Eq<bool>(FieldName, b);
					else if (Value is DateTime DT)
						return Builders<BsonDocument>.Filter.Eq<long>(FieldName, (long)(DT - ObjectSerializer.UnixEpoch).TotalMilliseconds);
					else if (Value is DateTimeOffset DTO)
					{
						return Builders<BsonDocument>.Filter.And(
							Builders<BsonDocument>.Filter.Eq<long>(FieldName + ".tp", (long)(DTO.DateTime - ObjectSerializer.UnixEpoch).TotalMilliseconds),
							Builders<BsonDocument>.Filter.Eq<string>(FieldName + ".tz", DTO.Offset.ToString()));
					}
					else if (Value is TimeSpan TS)
						return Builders<BsonDocument>.Filter.Eq<string>(FieldName, TS.ToString());
					else if (Value is Guid Guid)
						return Builders<BsonDocument>.Filter.Eq<string>(FieldName, Guid.ToString());
					else if (Value is ObjectId ObjectId)
						return Builders<BsonDocument>.Filter.Eq<ObjectId>(FieldName, ObjectId);
					else
						throw this.UnhandledFilterValueDataType(Serializer.ValueType.FullName, FieldName, Value);
				}
				else if (Filter is FilterFieldNotEqualTo)
				{
					if (IsDefaultValue)
						return Builders<BsonDocument>.Filter.Ne<string>(FieldName, null);
					else if (Value is string s)
					{
						if (HasType && FieldType == typeof(CaseInsensitiveString))
							return Builders<BsonDocument>.Filter.Ne<string>(FieldName + "_L", s);
						else
							return Builders<BsonDocument>.Filter.Ne<string>(FieldName, s);
					}
					else if (Value is CaseInsensitiveString cis)
						return Builders<BsonDocument>.Filter.Ne<string>(FieldName + "_L", cis.LowerCase);
					else if (Value is sbyte i8)
						return Builders<BsonDocument>.Filter.Ne<int>(FieldName, i8);
					else if (Value is short i16)
						return Builders<BsonDocument>.Filter.Ne<int>(FieldName, i16);
					else if (Value is int i32)
						return Builders<BsonDocument>.Filter.Ne<int>(FieldName, i32);
					else if (Value is long i64)
						return Builders<BsonDocument>.Filter.Ne<long>(FieldName, i64);
					else if (Value is byte ui8)
						return Builders<BsonDocument>.Filter.Ne<int>(FieldName, ui8);
					else if (Value is ushort ui16)
						return Builders<BsonDocument>.Filter.Ne<int>(FieldName, ui16);
					else if (Value is uint ui32)
						return Builders<BsonDocument>.Filter.Ne<long>(FieldName, ui32);
					else if (Value is ulong ui64)
						return Builders<BsonDocument>.Filter.Ne<Decimal128>(FieldName, ui64);
					else if (Value is double d)
						return Builders<BsonDocument>.Filter.Ne<double>(FieldName, d);
					else if (Value is float f)
						return Builders<BsonDocument>.Filter.Ne<double>(FieldName, f);
					else if (Value is decimal d2)
						return Builders<BsonDocument>.Filter.Ne<Decimal128>(FieldName, d2);
					else if (Value is bool b)
						return Builders<BsonDocument>.Filter.Ne<bool>(FieldName, b);
					else if (Value is DateTime DT)
						return Builders<BsonDocument>.Filter.Ne<long>(FieldName, (long)(DT - ObjectSerializer.UnixEpoch).TotalMilliseconds);
					else if (Value is DateTimeOffset DTO)
					{
						return Builders<BsonDocument>.Filter.Or(
							Builders<BsonDocument>.Filter.Ne<long>(FieldName + ".tp", (long)(DTO.DateTime - ObjectSerializer.UnixEpoch).TotalMilliseconds),
							Builders<BsonDocument>.Filter.Ne<string>(FieldName + ".tz", DTO.Offset.ToString()));
					}
					else if (Value is TimeSpan TS)
						return Builders<BsonDocument>.Filter.Ne<string>(FieldName, TS.ToString());
					else if (Value is Guid Guid)
						return Builders<BsonDocument>.Filter.Ne<string>(FieldName, Guid.ToString());
					else if (Value is ObjectId ObjectId)
						return Builders<BsonDocument>.Filter.Ne<ObjectId>(FieldName, ObjectId);
					else
						throw this.UnhandledFilterValueDataType(Serializer.ValueType.FullName, FieldName, Value);
				}
				else if (Filter is FilterFieldGreaterThan)
				{
					if (Value is string s)
					{
						if (HasType && FieldType == typeof(CaseInsensitiveString))
							return Builders<BsonDocument>.Filter.Gt<string>(FieldName + "_L", s);
						else
							return Builders<BsonDocument>.Filter.Gt<string>(FieldName, s);
					}
					else if (Value is CaseInsensitiveString cis)
						return Builders<BsonDocument>.Filter.Gt<string>(FieldName + "_L", cis.LowerCase);
					else if (Value is sbyte i8)
						return Builders<BsonDocument>.Filter.Gt<int>(FieldName, i8);
					else if (Value is short i16)
						return Builders<BsonDocument>.Filter.Gt<int>(FieldName, i16);
					else if (Value is int i32)
						return Builders<BsonDocument>.Filter.Gt<int>(FieldName, i32);
					else if (Value is long i64)
						return Builders<BsonDocument>.Filter.Gt<long>(FieldName, i64);
					else if (Value is byte ui8)
						return Builders<BsonDocument>.Filter.Gt<int>(FieldName, ui8);
					else if (Value is ushort ui16)
						return Builders<BsonDocument>.Filter.Gt<int>(FieldName, ui16);
					else if (Value is uint ui32)
						return Builders<BsonDocument>.Filter.Gt<long>(FieldName, ui32);
					else if (Value is ulong ui64)
						return Builders<BsonDocument>.Filter.Gt<Decimal128>(FieldName, ui64);
					else if (Value is double d)
						return Builders<BsonDocument>.Filter.Gt<double>(FieldName, d);
					else if (Value is float f)
						return Builders<BsonDocument>.Filter.Gt<double>(FieldName, f);
					else if (Value is decimal d2)
						return Builders<BsonDocument>.Filter.Gt<Decimal128>(FieldName, d2);
					else if (Value is bool b)
						return Builders<BsonDocument>.Filter.Gt<bool>(FieldName, b);
					else if (Value is DateTime DT)
						return Builders<BsonDocument>.Filter.Gt<long>(FieldName, (long)(DT - ObjectSerializer.UnixEpoch).TotalMilliseconds);
					else if (Value is TimeSpan TS)
						return Builders<BsonDocument>.Filter.Gt<string>(FieldName, TS.ToString());
					else if (Value is Guid Guid)
						return Builders<BsonDocument>.Filter.Gt<string>(FieldName, Guid.ToString());
					else if (Value is ObjectId ObjectId)
						return Builders<BsonDocument>.Filter.Gt<ObjectId>(FieldName, ObjectId);
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
					else if (Value is string s)
					{
						if (HasType && FieldType == typeof(CaseInsensitiveString))
							return Builders<BsonDocument>.Filter.Gte<string>(FieldName + "_L", s);
						else
							return Builders<BsonDocument>.Filter.Gte<string>(FieldName, s);
					}
					else if (Value is CaseInsensitiveString cis)
						return Builders<BsonDocument>.Filter.Gte<string>(FieldName + "_L", cis.LowerCase);
					else if (Value is sbyte i8)
						return Builders<BsonDocument>.Filter.Gte<int>(FieldName, i8);
					else if (Value is short i16)
						return Builders<BsonDocument>.Filter.Gte<int>(FieldName, i16);
					else if (Value is int i32)
						return Builders<BsonDocument>.Filter.Gte<int>(FieldName, i32);
					else if (Value is long i64)
						return Builders<BsonDocument>.Filter.Gte<long>(FieldName, i64);
					else if (Value is byte ui8)
						return Builders<BsonDocument>.Filter.Gte<int>(FieldName, ui8);
					else if (Value is ushort ui16)
						return Builders<BsonDocument>.Filter.Gte<int>(FieldName, ui16);
					else if (Value is uint ui32)
						return Builders<BsonDocument>.Filter.Gte<long>(FieldName, ui32);
					else if (Value is ulong ui64)
						return Builders<BsonDocument>.Filter.Gte<Decimal128>(FieldName, ui64);
					else if (Value is double d)
						return Builders<BsonDocument>.Filter.Gte<double>(FieldName, d);
					else if (Value is float f)
						return Builders<BsonDocument>.Filter.Gte<double>(FieldName, f);
					else if (Value is decimal d2)
						return Builders<BsonDocument>.Filter.Gte<Decimal128>(FieldName, d2);
					else if (Value is bool b)
						return Builders<BsonDocument>.Filter.Gte<bool>(FieldName, b);
					else if (Value is DateTime DT)
						return Builders<BsonDocument>.Filter.Gte<long>(FieldName, (long)(DT - ObjectSerializer.UnixEpoch).TotalMilliseconds);
					else if (Value is TimeSpan TS)
						return Builders<BsonDocument>.Filter.Gte<string>(FieldName, TS.ToString());
					else if (Value is Guid Guid)
						return Builders<BsonDocument>.Filter.Gte<string>(FieldName, Guid.ToString());
					else if (Value is ObjectId ObjectId)
						return Builders<BsonDocument>.Filter.Gte<ObjectId>(FieldName, ObjectId);
					else
						throw this.UnhandledFilterValueDataType(Serializer.ValueType.FullName, FieldName, Value);
				}
				else if (Filter is FilterFieldLesserThan)
				{
					if (Value is string s)
					{
						if (HasType && FieldType == typeof(CaseInsensitiveString))
							return Builders<BsonDocument>.Filter.Lt<string>(FieldName + "_L", s);
						else
							return Builders<BsonDocument>.Filter.Lt<string>(FieldName, s);
					}
					else if (Value is CaseInsensitiveString cis)
						return Builders<BsonDocument>.Filter.Lt<string>(FieldName + "_L", cis.LowerCase);
					else if (Value is sbyte i8)
						return Builders<BsonDocument>.Filter.Lt<int>(FieldName, i8);
					else if (Value is short i16)
						return Builders<BsonDocument>.Filter.Lt<int>(FieldName, i16);
					else if (Value is int i32)
						return Builders<BsonDocument>.Filter.Lt<int>(FieldName, i32);
					else if (Value is long i64)
						return Builders<BsonDocument>.Filter.Lt<long>(FieldName, i64);
					else if (Value is byte ui8)
						return Builders<BsonDocument>.Filter.Lt<int>(FieldName, ui8);
					else if (Value is ushort ui16)
						return Builders<BsonDocument>.Filter.Lt<int>(FieldName, ui16);
					else if (Value is uint ui32)
						return Builders<BsonDocument>.Filter.Lt<long>(FieldName, ui32);
					else if (Value is ulong ui64)
						return Builders<BsonDocument>.Filter.Lt<Decimal128>(FieldName, ui64);
					else if (Value is double d)
						return Builders<BsonDocument>.Filter.Lt<double>(FieldName, d);
					else if (Value is float f)
						return Builders<BsonDocument>.Filter.Lt<double>(FieldName, f);
					else if (Value is decimal d2)
						return Builders<BsonDocument>.Filter.Lt<Decimal128>(FieldName, d2);
					else if (Value is bool b)
						return Builders<BsonDocument>.Filter.Lt<bool>(FieldName, b);
					else if (Value is DateTime DT)
						return Builders<BsonDocument>.Filter.Lt<long>(FieldName, (long)(DT - ObjectSerializer.UnixEpoch).TotalMilliseconds);
					else if (Value is TimeSpan TS)
						return Builders<BsonDocument>.Filter.Lt<string>(FieldName, TS.ToString());
					else if (Value is Guid Guid)
						return Builders<BsonDocument>.Filter.Lt<string>(FieldName, Guid.ToString());
					else if (Value is ObjectId ObjectId)
						return Builders<BsonDocument>.Filter.Lt<ObjectId>(FieldName, ObjectId);
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
					else if (Value is string s)
					{
						if (HasType && FieldType == typeof(CaseInsensitiveString))
							return Builders<BsonDocument>.Filter.Lte<string>(FieldName + "_L", s);
						else
							return Builders<BsonDocument>.Filter.Lte<string>(FieldName, s);
					}
					else if (Value is CaseInsensitiveString cis)
						return Builders<BsonDocument>.Filter.Lte<string>(FieldName + "_L", cis.LowerCase);
					else if (Value is sbyte i8)
						return Builders<BsonDocument>.Filter.Lte<int>(FieldName, i8);
					else if (Value is short i16)
						return Builders<BsonDocument>.Filter.Lte<int>(FieldName, i16);
					else if (Value is int i32)
						return Builders<BsonDocument>.Filter.Lte<int>(FieldName, i32);
					else if (Value is long i64)
						return Builders<BsonDocument>.Filter.Lte<long>(FieldName, i64);
					else if (Value is byte ui8)
						return Builders<BsonDocument>.Filter.Lte<int>(FieldName, ui8);
					else if (Value is ushort ui16)
						return Builders<BsonDocument>.Filter.Lte<int>(FieldName, ui16);
					else if (Value is uint ui32)
						return Builders<BsonDocument>.Filter.Lte<long>(FieldName, ui32);
					else if (Value is ulong ui64)
						return Builders<BsonDocument>.Filter.Lte<Decimal128>(FieldName, ui64);
					else if (Value is double d)
						return Builders<BsonDocument>.Filter.Lte<double>(FieldName, d);
					else if (Value is float f)
						return Builders<BsonDocument>.Filter.Lte<double>(FieldName, f);
					else if (Value is decimal d2)
						return Builders<BsonDocument>.Filter.Lte<Decimal128>(FieldName, d2);
					else if (Value is bool b)
						return Builders<BsonDocument>.Filter.Lte<bool>(FieldName, b);
					else if (Value is DateTime DT)
						return Builders<BsonDocument>.Filter.Lte<long>(FieldName, (long)(DT - ObjectSerializer.UnixEpoch).TotalMilliseconds);
					else if (Value is TimeSpan TS)
						return Builders<BsonDocument>.Filter.Lte<string>(FieldName, TS.ToString());
					else if (Value is Guid Guid)
						return Builders<BsonDocument>.Filter.Lte<string>(FieldName, Guid.ToString());
					else if (Value is ObjectId ObjectId)
						return Builders<BsonDocument>.Filter.Lte<ObjectId>(FieldName, ObjectId);
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
			if (Value is null)
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
		/// Finds the first page of objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="PageSize">Number of items on a page.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>First page of objects.</returns>
		public async Task<IPage<T>> FindFirst<T>(int PageSize, params string[] SortOrder)
			where T : class
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(typeof(T));
			IEnumerable<T> Items = await this.Find<T>(0, PageSize, SortOrder);
			return new Page<T>(PageSize, null, null, SortOrder, Items, Serializer, this);
		}

		/// <summary>
		/// Finds the first page of objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="PageSize">Number of items on a page.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>First page of objects.</returns>
		public async Task<IPage<T>> FindFirst<T>(int PageSize, Filter Filter, params string[] SortOrder)
			where T : class
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(typeof(T));
			IEnumerable<T> Items = await this.Find<T>(0, PageSize, Filter, SortOrder);
			return new Page<T>(PageSize, null, Filter, SortOrder, Items, Serializer, this);
		}

		/// <summary>
		/// Finds the first page of objects in a given collection.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="PageSize">Number of items on a page.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>First page of objects.</returns>
		public async Task<IPage<object>> FindFirst(string Collection, int PageSize, params string[] SortOrder)
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(typeof(object));
			IEnumerable<object> Items = await this.Find(Collection, 0, PageSize, SortOrder);
			return new Page<object>(PageSize, Collection, null, SortOrder, Items, Serializer, this);
		}

		/// <summary>
		/// Finds the first page of objects in a given collection.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="PageSize">Number of items on a page.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>First page of objects.</returns>
		public async Task<IPage<object>> FindFirst(string Collection, int PageSize, Filter Filter, params string[] SortOrder)
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(typeof(object));
			IEnumerable<object> Items = await this.Find(Collection, 0, PageSize, Filter, SortOrder);
			return new Page<object>(PageSize, Collection, Filter, SortOrder, Items, Serializer, this);
		}

		/// <summary>
		/// Finds the first page of objects in a given collection.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="PageSize">Number of items on a page.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>First page of objects.</returns>
		public async Task<IPage<T>> FindFirst<T>(string Collection, int PageSize, Filter Filter, params string[] SortOrder)
			where T : class
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(typeof(T));
			IEnumerable<T> Items = await this.Find<T>(Collection, 0, PageSize, Filter, SortOrder);
			return new Page<T>(PageSize, Collection, Filter, SortOrder, Items, Serializer, this);
		}

		/// <summary>
		/// Finds the next page of objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Page">Page reference.</param>
		/// <returns>Next page, directly following <paramref name="Page"/>.</returns>
		public Task<IPage<T>> FindNext<T>(IPage<T> Page)
			where T : class
		{
			if (Page is Page<T> CurrentPage)
				return CurrentPage.FindNext();
			else
				throw new IOException("Incompatible page.");
		}

		/// <summary>
		/// Finds the next page of objects in a given collection.
		/// </summary>
		/// <param name="Page">Page reference.</param>
		/// <returns>Next page, directly following <paramref name="Page"/>.</returns>
		public Task<IPage<object>> FindNext(IPage<object> Page)
		{
			if (Page is Page<object> CurrentPage)
				return CurrentPage.FindNext();
			else
				throw new IOException("Incompatible page.");
		}

		/// <summary>
		/// Tries to load an object given its Object ID <paramref name="ObjectId"/> and its base type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Base type.</typeparam>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>Loaded object, or null if not found.</returns>
		public Task<T> TryLoadObject<T>(object ObjectId)
			where T : class
		{
			ObjectId OID;

			if (ObjectId is ObjectId ObjId)
				OID = ObjId;
			else if (ObjectId is string s)
				OID = new ObjectId(s);
			else if (ObjectId is byte[] A)
				OID = new ObjectId(A);
			else if (ObjectId is Guid Guid)
				OID = GeneratedObjectSerializerBase.GuidToObjectId(Guid);
			else
				throw new NotSupportedException("Unsupported type for Object ID: " + ObjectId.GetType().FullName);

			return this.TryLoadObject<T>(OID);
		}

		/// <summary>
		/// Tries to load an object of a given type and Object ID.
		/// </summary>
		/// <typeparam name="T">Type of object to load.</typeparam>
		/// <param name="ObjectId">Object ID of object to load.</param>
		/// <returns>Loaded object, or null if not found.</returns>
		public async Task<T> TryLoadObject<T>(ObjectId ObjectId)
			where T : class
		{
			string Key = typeof(T).FullName + " " + ObjectId.ToString();

			if (this.loadCache.TryGetValue(Key, out object Obj) && Obj is T Result)
				return Result;

			ObjectSerializer S = this.GetObjectSerializerEx(typeof(T));
			IEnumerable<T> ReferencedObjects = await this.Find<T>(0, 2, new FilterFieldEqualTo(S.ObjectIdMemberName, ObjectId));
			T First = default;

			foreach (T Item in ReferencedObjects)
			{
				if (First is null)
					First = Item;
				else
					throw new Exception("Multiple objects of type T found with object ID " + ObjectId.ToString());
			}

			if (!(First is null))
				this.loadCache.Add(Key, First);     // Speeds up readout if reading multiple objects referencing a few common sub-objects.

			return First;
		}

		/// <summary>
		/// Tries to load an object given its Object ID <paramref name="ObjectId"/> and its base type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Base type.</typeparam>
		/// <param name="CollectionName">Name of collection in which the object resides.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>Loaded object, or null if not found.</returns>
		public Task<T> TryLoadObject<T>(string CollectionName, object ObjectId)
			where T : class
		{
			ObjectId OID;

			if (ObjectId is ObjectId ObjId)
				OID = ObjId;
			else if (ObjectId is string s)
				OID = new ObjectId(s);
			else if (ObjectId is byte[] A)
				OID = new ObjectId(A);
			else if (ObjectId is Guid Guid)
				OID = GeneratedObjectSerializerBase.GuidToObjectId(Guid);
			else
				throw new NotSupportedException("Unsupported type for Object ID: " + ObjectId.GetType().FullName);

			return this.TryLoadObject<T>(CollectionName, OID);
		}

		/// <summary>
		/// Tries to load an object of a given type and Object ID.
		/// </summary>
		/// <typeparam name="T">Type of object to load.</typeparam>
		/// <param name="CollectionName">Name of collection in which the object resides.</param>
		/// <param name="ObjectId">Object ID of object to load.</param>
		/// <returns>Loaded object, or null if not found.</returns>
		public async Task<T> TryLoadObject<T>(string CollectionName, ObjectId ObjectId)
			where T : class
		{
			string Key = typeof(T).FullName + " " + ObjectId.ToString();

			if (this.loadCache.TryGetValue(Key, out object Obj) && Obj is T Result)
				return Result;

			ObjectSerializer S = this.GetObjectSerializerEx(typeof(T));
			IEnumerable<T> ReferencedObjects = await this.Find<T>(CollectionName, 0, 2, new FilterFieldEqualTo(S.ObjectIdMemberName, ObjectId));
			T First = default;

			foreach (T Item in ReferencedObjects)
			{
				if (First is null)
					First = Item;
				else
					throw new Exception("Multiple objects of type T found with object ID " + ObjectId.ToString());
			}

			if (!(First is null))
				this.loadCache.Add(Key, First);     // Speeds up readout if reading multiple objects referencing a few common sub-objects.

			return First;
		}

		/// <summary>
		/// Tries to load an object given its Object ID <paramref name="ObjectId"/> and its collection name <paramref name="CollectionName"/>.
		/// </summary>
		/// <param name="CollectionName">Name of collection in which the object resides.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>Loaded object, or null if not found.</returns>
		public async Task<object> TryLoadObject(string CollectionName, object ObjectId)
		{
			ObjectId OID;

			if (ObjectId is ObjectId ObjId)
				OID = ObjId;
			else if (ObjectId is string s)
				OID = new ObjectId(s);
			else if (ObjectId is byte[] A)
				OID = new ObjectId(A);
			else if (ObjectId is Guid Guid)
				OID = GeneratedObjectSerializerBase.GuidToObjectId(Guid);
			else
				throw new NotSupportedException("Unsupported type for Object ID: " + ObjectId.GetType().FullName);

			ObjectSerializer S = this.GetObjectSerializerEx(typeof(object));
			IEnumerable<object> ReferencedObjects = await this.Find(CollectionName, 0, 2, new FilterFieldEqualTo(S.ObjectIdMemberName, OID));
			object First = null;

			foreach (object Item in ReferencedObjects)
			{
				if (First is null)
					First = Item;
				else
					throw new Exception("Multiple objects of type T found with object ID " + ObjectId.ToString());
			}

			return First;
		}

		private readonly Cache<string, object> loadCache = new Cache<string, object>(10000, new TimeSpan(0, 0, 10), new TimeSpan(0, 0, 5), true);  // TODO: Make parameters configurable.

		/// <summary>
		/// Processes objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Processor">Processor to call for every object, unless the
		/// processor returns false, in which the process is cancelled.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to process.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>If process was completed (true) or cancelled (false).</returns>
		public Task<bool> Process<T>(IProcessor<T> Processor, int Offset, int MaxCount, params string[] SortOrder)
			where T : class
		{
			return this.Process<T>(Processor, Offset, MaxCount, (Filter)null, SortOrder);
		}

		/// <summary>
		/// Processes objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Processor">Processor to call for every object, unless the
		/// processor returns false, in which the process is cancelled.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to process.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>If process was completed (true) or cancelled (false).</returns>
		public Task<bool> Process<T>(IProcessor<T> Processor, int Offset, int MaxCount, Filter Filter, params string[] SortOrder)
			where T : class
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(typeof(T));
			string CollectionName = Serializer.CollectionName(null);
			IMongoCollection<BsonDocument> Collection;
			FilterDefinition<BsonDocument> BsonFilter;

			if (string.IsNullOrEmpty(CollectionName))
				Collection = this.defaultCollection;
			else
				Collection = this.GetCollection(CollectionName);

			if (Filter is null)
				BsonFilter = new BsonDocument();
			else
				BsonFilter = this.Convert(Filter, Serializer);

			return this.Process<T>(Processor, Serializer, Collection, Offset, MaxCount, BsonFilter, null, SortOrder);
		}

		/// <summary>
		/// Processes objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Processor">Processor to call for every object, unless the
		/// processor returns false, in which the process is cancelled.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to process.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="ContinueAfter">Continue returning results after this object.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>If process was completed (true) or cancelled (false).</returns>
		public Task<bool> Process<T>(IProcessor<T> Processor, int Offset, int MaxCount, Filter Filter,
			T ContinueAfter, params string[] SortOrder)
			where T : class
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(typeof(T));
			string CollectionName = Serializer.CollectionName(null);
			IMongoCollection<BsonDocument> Collection;
			FilterDefinition<BsonDocument> BsonFilter;

			if (string.IsNullOrEmpty(CollectionName))
				Collection = this.defaultCollection;
			else
				Collection = this.GetCollection(CollectionName);

			if (Filter is null)
				BsonFilter = new BsonDocument();
			else
				BsonFilter = this.Convert(Filter, Serializer);

			return this.Process(Processor, Serializer, Collection, Offset, MaxCount, BsonFilter, ContinueAfter, SortOrder);
		}

		/// <summary>
		/// Processes objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Processor">Processor to call for every object, unless the
		/// processor returns false, in which the process is cancelled.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to process.</param>
		/// <param name="BsonFilter">Search filter.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>If process was completed (true) or cancelled (false).</returns>
		public Task<bool> Process<T>(IProcessor<T> Processor, int Offset, int MaxCount, FilterDefinition<BsonDocument> BsonFilter,
			params string[] SortOrder)
			where T : class
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(typeof(T));
			string CollectionName = Serializer.CollectionName(null);
			IMongoCollection<BsonDocument> Collection;

			if (string.IsNullOrEmpty(CollectionName))
				Collection = this.defaultCollection;
			else
				Collection = this.GetCollection(CollectionName);

			return this.Process<T>(Processor, Serializer, Collection, Offset, MaxCount, BsonFilter, null, SortOrder);
		}

		/// <summary>
		/// Processes objects in a given collection.
		/// </summary>
		/// <param name="Processor">Processor to call for every object, unless the
		/// processor returns false, in which the process is cancelled.</param>
		/// <param name="CollectionName">Collection Name</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to process.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>If process was completed (true) or cancelled (false).</returns>
		public Task<bool> Process<T>(IProcessor<T> Processor, string CollectionName, int Offset, int MaxCount, Filter Filter, params string[] SortOrder)
			where T : class
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(typeof(T));
			IMongoCollection<BsonDocument> Collection;
			FilterDefinition<BsonDocument> BsonFilter;

			if (string.IsNullOrEmpty(CollectionName))
				Collection = this.defaultCollection;
			else
				Collection = this.GetCollection(CollectionName);

			if (Filter is null)
				BsonFilter = new BsonDocument();
			else
				BsonFilter = this.Convert(Filter, Serializer);

			return this.Process<T>(Processor, Serializer, Collection, Offset, MaxCount, BsonFilter, null, SortOrder);
		}

		/// <summary>
		/// Processes objects in a given collection.
		/// </summary>
		/// <param name="Processor">Processor to call for every object, unless the
		/// processor returns false, in which the process is cancelled.</param>
		/// <param name="CollectionName">Collection Name</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to process.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="ContinueAfter">Continue returning results after this object.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>If process was completed (true) or cancelled (false).</returns>
		public Task<bool> Process<T>(IProcessor<T> Processor, string CollectionName, int Offset, int MaxCount, Filter Filter,
			T ContinueAfter, params string[] SortOrder)
			where T : class
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(typeof(T));
			IMongoCollection<BsonDocument> Collection;
			FilterDefinition<BsonDocument> BsonFilter;

			if (string.IsNullOrEmpty(CollectionName))
				Collection = this.defaultCollection;
			else
				Collection = this.GetCollection(CollectionName);

			if (Filter is null)
				BsonFilter = new BsonDocument();
			else
				BsonFilter = this.Convert(Filter, Serializer);

			return this.Process(Processor, Serializer, Collection, Offset, MaxCount, BsonFilter, ContinueAfter, SortOrder);
		}

		/// <summary>
		/// Processes objects in a given collection.
		/// </summary>
		/// <param name="Processor">Processor to call for every object, unless the
		/// processor returns false, in which the process is cancelled.</param>
		/// <param name="CollectionName">Collection Name</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to process.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>If process was completed (true) or cancelled (false).</returns>
		public Task<bool> Process(IProcessor<object> Processor, string CollectionName, int Offset, int MaxCount, params string[] SortOrder)
		{
			return this.Process(Processor, CollectionName, Offset, MaxCount, null, SortOrder);
		}

		/// <summary>
		/// Processes objects in a given collection.
		/// </summary>
		/// <param name="Processor">Processor to call for every object, unless the
		/// processor returns false, in which the process is cancelled.</param>
		/// <param name="CollectionName">Collection Name</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to process.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>If process was completed (true) or cancelled (false).</returns>
		public Task<bool> Process(IProcessor<object> Processor, string CollectionName, int Offset, int MaxCount, Filter Filter, params string[] SortOrder)
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(typeof(object));
			IMongoCollection<BsonDocument> Collection;
			FilterDefinition<BsonDocument> BsonFilter;

			if (string.IsNullOrEmpty(CollectionName))
				Collection = this.defaultCollection;
			else
				Collection = this.GetCollection(CollectionName);

			if (Filter is null)
				BsonFilter = new BsonDocument();
			else
				BsonFilter = this.Convert(Filter, Serializer);

			return this.Process<object>(Processor, Serializer, Collection, Offset, MaxCount, BsonFilter, SortOrder);
		}

		private async Task<bool> Process<T>(IProcessor<T> Processor, ObjectSerializer Serializer, IMongoCollection<BsonDocument> Collection,
			int Offset, int MaxCount, FilterDefinition<BsonDocument> BsonFilter, T ContinueAfter, params string[] SortOrder)
			where T : class
		{
			if (!(ContinueAfter is null))
				throw new NotImplementedException("Paginated searches not implemented in MongoDB provider.");

			IFindFluent<BsonDocument, BsonDocument> ResultSet = Collection.Find(BsonFilter);

			if (SortOrder.Length > 0)
			{
				SortDefinition<BsonDocument> SortDefinition = null;

				foreach (string SortBy in SortOrder)
				{
					if (SortDefinition is null)
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
			BsonDeserializationArgs Args = new BsonDeserializationArgs()
			{
				NominalType = typeof(T)
			};

			bool Asynchronous = Processor.IsAsynchronous;
			bool Continue;

			while (await Cursor.MoveNextAsync())
			{
				foreach (BsonDocument Document in Cursor.Current)
				{
					BsonDocumentReader Reader = new BsonDocumentReader(Document);
					BsonDeserializationContext Context = BsonDeserializationContext.CreateRoot(Reader);

					if (Serializer.Deserialize(Context, Args) is T Obj)
					{
						if (Asynchronous)
							Continue = await Processor.ProcessAsync(Obj);
						else
							Continue = Processor.Process(Obj);

						if (!Continue)
							return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Updates an object in the database.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public async Task Update(object Object)
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(Object);
			ObjectId ObjectId = await Serializer.GetObjectId(Object, false);
			string CollectionName = Serializer.CollectionName(Object);
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
		/// Updates an object in the database, if unlocked. If locked, object will be updated at next opportunity.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		public Task UpdateLazy(object Object, ObjectCallback Callback)
			=> this.Process(Object, this.Update(Object), Callback);

		/// <summary>
		/// Updates a collection of objects in the database, if unlocked. If locked, objects will be updated at next opportunity.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		public Task UpdateLazy(object[] Objects, ObjectsCallback Callback)
			=> this.Process(Objects, this.Update(Objects), Callback);

		/// <summary>
		/// Updates a collection of objects in the database, if unlocked. If locked, objects will be updated at next opportunity.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		public Task UpdateLazy(IEnumerable<object> Objects, ObjectsCallback Callback)
			=> this.Process(Objects, this.Update(Objects), Callback);

		/// <summary>
		/// Deletes an object in the database.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public async Task Delete(object Object)
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(Object);
			ObjectId ObjectId = await Serializer.GetObjectId(Object, false);
			string CollectionName = Serializer.CollectionName(Object);
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

		private async Task Process(object Object, Task Op, ObjectCallback Callback)
		{
			await Op;
			if (!(Callback is null))
				Callback(Object);
		}

		private async Task Process(IEnumerable<object> Objects, Task Op, ObjectsCallback Callback)
		{
			await Op;

			if (!(Callback is null))
				Callback(Objects);
		}

		/// <summary>
		/// Deletes an object in the database, if unlocked. If locked, object will be deleted at next opportunity.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		public Task DeleteLazy(object Object, ObjectCallback Callback)
			=> this.Process(Object, this.Delete(Object), Callback);

		/// <summary>
		/// Deletes a collection of objects in the database, if unlocked. If locked, objects will be deleted at next opportunity.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		public Task DeleteLazy(object[] Objects, ObjectsCallback Callback)
			=> this.Process(Objects, this.Delete(Objects), Callback);

		/// <summary>
		/// Deletes a collection of objects in the database, if unlocked. If locked, objects will be deleted at next opportunity.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		public Task DeleteLazy(IEnumerable<object> Objects, ObjectsCallback Callback)
			=> this.Process(Objects, this.Delete(Objects), Callback);

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/> and deletes them in the same atomic operation.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public async Task<IEnumerable<T>> FindDelete<T>(int Offset, int MaxCount, params string[] SortOrder)
			where T : class
		{
			IEnumerable<T> Result = await this.Find<T>(Offset, MaxCount, SortOrder);
			await this.Delete(Result);
			return Result;
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
		public async Task<IEnumerable<T>> FindDelete<T>(int Offset, int MaxCount, Filter Filter, params string[] SortOrder)
			where T : class
		{
			IEnumerable<T> Result = await this.Find<T>(Offset, MaxCount, Filter, SortOrder);
			await this.Delete(Result);
			return Result;
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
		public async Task<IEnumerable<object>> FindDelete(string Collection, int Offset, int MaxCount, params string[] SortOrder)
		{
			IEnumerable<object> Result = await this.Find(Collection, Offset, MaxCount, SortOrder);
			await this.Delete(Result);
			return Result;
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
		public async Task<IEnumerable<object>> FindDelete(string Collection, int Offset, int MaxCount, Filter Filter, params string[] SortOrder)
		{
			IEnumerable<object> Result = await this.Find(Collection, Offset, MaxCount, Filter, SortOrder);
			await this.Delete(Result);
			return Result;
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/> and deletes them in the same atomic operation.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		public async Task DeleteLazy<T>(int Offset, int MaxCount, string[] SortOrder, ObjectsCallback Callback)
			where T : class
		{
			IEnumerable<T> Objects = await this.FindDelete<T>(Offset, MaxCount, SortOrder);
			if (!(Callback is null))
				Callback(Objects);
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
		/// <param name="Callback">Method to call when operation completes.</param>
		public async Task DeleteLazy<T>(int Offset, int MaxCount, Filter Filter, string[] SortOrder, ObjectsCallback Callback)
			where T : class
		{
			IEnumerable<T> Objects = await this.FindDelete<T>(Offset, MaxCount, Filter, SortOrder);
			if (!(Callback is null))
				Callback(Objects);
		}

		/// <summary>
		/// Finds objects in a given collection and deletes them in the same atomic operation.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		public async Task DeleteLazy(string Collection, int Offset, int MaxCount, string[] SortOrder, ObjectsCallback Callback)
		{
			IEnumerable<object> Objects = await this.FindDelete(Collection, Offset, MaxCount, SortOrder);
			if (!(Callback is null))
				Callback(Objects);
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
		/// <param name="Callback">Method to call when operation completes.</param>
		public async Task DeleteLazy(string Collection, int Offset, int MaxCount, Filter Filter, string[] SortOrder, ObjectsCallback Callback)
		{
			IEnumerable<object> Objects = await this.FindDelete(Collection, Offset, MaxCount, Filter, SortOrder);
			if (!(Callback is null))
				Callback(Objects);
		}

		/// <summary>
		/// Clears a collection of all objects.
		/// </summary>
		/// <param name="CollectionName">Name of collection to clear.</param>
		/// <returns>Task object for synchronization purposes.</returns>
		public Task Clear(string CollectionName)
		{
			IMongoCollection<BsonDocument> Collection = this.GetCollection(CollectionName);
			return Collection.DeleteManyAsync(FilterDefinition<BsonDocument>.Empty);
		}

		/// <summary>
		/// Adds an index to a collection, if one does not already exist.
		/// </summary>
		/// <param name="CollectionName">Name of collection.</param>
		/// <param name="FieldNames">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		public async Task AddIndex(string CollectionName, string[] FieldNames)
		{
			IMongoCollection<BsonDocument> Collection;
			List<BsonDocument> Indices;

			if (string.IsNullOrEmpty(CollectionName))
				Collection = this.DefaultCollection;
			else
				Collection = this.GetCollection(CollectionName);

			IAsyncCursor<BsonDocument> Cursor = await Collection.Indexes.ListAsync();
			Indices = await Cursor.ToListAsync<BsonDocument>();

			await ObjectSerializer.CheckIndexExists(Collection, Indices, FieldNames, null);
		}

		/// <summary>
		/// Removes an index from a collection, if one exist.
		/// </summary>
		/// <param name="CollectionName">Name of collection.</param>
		/// <param name="FieldNames">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		public async Task RemoveIndex(string CollectionName, string[] FieldNames)
		{
			IMongoCollection<BsonDocument> Collection;
			List<BsonDocument> Indices;

			if (string.IsNullOrEmpty(CollectionName))
				Collection = this.DefaultCollection;
			else
				Collection = this.GetCollection(CollectionName);

			IAsyncCursor<BsonDocument> Cursor = await Collection.Indexes.ListAsync();
			Indices = await Cursor.ToListAsync<BsonDocument>();

			await ObjectSerializer.RemoveIndex(Collection, Indices, FieldNames);
		}

		/// <summary>
		/// Removes an index from a collection, if one exist.
		/// </summary>
		/// <param name="CollectionName">Name of collection.</param>
		/// <returns>Sort order of each index. Each string represents a field name. 
		/// By default, sort order is ascending. If descending sort order is desired, 
		/// the field name is prefixed by a hyphen (minus) sign.</returns>
		public async Task<string[][]> GetIndices(string CollectionName)
		{
			IMongoCollection<BsonDocument> Collection;

			if (string.IsNullOrEmpty(CollectionName))
				Collection = this.DefaultCollection;
			else
				Collection = this.GetCollection(CollectionName);

			IAsyncCursor<BsonDocument> Cursor = await Collection.Indexes.ListAsync();
			ChunkedList<string[]> Result = new ChunkedList<string[]>();

			while (await Cursor.MoveNextAsync())
			{
				foreach (BsonDocument Index in Cursor.Current)
				{
					ChunkedList<string> FieldNames = null;

					foreach (BsonElement E in Index.Elements)
					{
						if (E.Name != "key")
							continue;

						FieldNames = new ChunkedList<string>();

						foreach (BsonElement E2 in E.Value.AsBsonDocument.Elements)
						{
							// Value is typically 1 (ascending) or -1 (descending). Can also be "text" etc.
							if (E2.Value.IsInt32 || E2.Value.IsInt64 || E2.Value.IsDouble)
							{
								double v = E2.Value.ToDouble();
								if (v < 0)
									FieldNames.Add("-" + E2.Name);
								else
									FieldNames.Add(E2.Name);
							}
							else if (E2.Value.IsString)
							{
								// For text or hashed indexes we just report the field name without sign.
								FieldNames.Add(E2.Name);
							}
							else
							{
								// Fallback: just add field name.
								FieldNames.Add(E2.Name);
							}
						}

						break; // Done with this index.
					}

					if (FieldNames != null && FieldNames.Count > 0)
						Result.Add(FieldNames.ToArray());
				}
			}

			return Result.ToArray();
		}

		/// <summary>
		/// Analyzes the database and exports findings to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="XsltPath">Optional XSLT to use to view the output.</param>
		/// <param name="ProgramDataFolder">Program data folder. Can be removed from filenames used, when referencing them in the report.</param>
		/// <param name="ExportData">If data in database is to be exported in output.</param>
		public Task<string[]> Analyze(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData)
		{
			return this.Analyze(Output, XsltPath, ProgramDataFolder, ExportData, false);
		}

		/// <summary>
		/// Analyzes the database and exports findings to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="XsltPath">Optional XSLT to use to view the output.</param>
		/// <param name="ProgramDataFolder">Program data folder. Can be removed from filenames used, when referencing them in the report.</param>
		/// <param name="ExportData">If data in database is to be exported in output.</param>
		/// <param name="Thread">Optional Profiler thread.</param>
		public Task<string[]> Analyze(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData, ProfilerThread Thread)
		{
			return this.Analyze(Output, XsltPath, ProgramDataFolder, ExportData, false, Thread);
		}

		/// <summary>
		/// Analyzes the database and repairs it if necessary. Results are exported to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="XsltPath">Optional XSLT to use to view the output.</param>
		/// <param name="ProgramDataFolder">Program data folder. Can be removed from filenames used, when referencing them in the report.</param>
		/// <param name="ExportData">If data in database is to be exported in output.</param>
		public Task<string[]> Repair(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData)
		{
			return this.Analyze(Output, XsltPath, ProgramDataFolder, ExportData, true);
		}

		/// <summary>
		/// Analyzes the database and repairs it if necessary. Results are exported to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="XsltPath">Optional XSLT to use to view the output.</param>
		/// <param name="ProgramDataFolder">Program data folder. Can be removed from filenames used, when referencing them in the report.</param>
		/// <param name="ExportData">If data in database is to be exported in output.</param>
		/// <param name="Thread">Optional Profiler thread.</param>
		public Task<string[]> Repair(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData, ProfilerThread Thread)
		{
			return this.Analyze(Output, XsltPath, ProgramDataFolder, ExportData, true, Thread);
		}

		/// <summary>
		/// Analyzes the database and exports findings to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="XsltPath">Optional XSLT to use to view the output.</param>
		/// <param name="ProgramDataFolder">Program data folder. Can be removed from filenames used, when referencing them in the report.</param>
		/// <param name="ExportData">If data in database is to be exported in output.</param>
		/// <param name="Repair">If files should be repaired if corruptions are detected.</param>
		public Task<string[]> Analyze(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData, bool Repair)
		{
			return this.Analyze(null, XsltPath, ProgramDataFolder, ExportData, Repair, null);
		}

		/// <summary>
		/// Analyzes the database and exports findings to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="XsltPath">Optional XSLT to use to view the output.</param>
		/// <param name="ProgramDataFolder">Program data folder. Can be removed from filenames used, when referencing them in the report.</param>
		/// <param name="ExportData">If data in database is to be exported in output.</param>
		/// <param name="Repair">If files should be repaired if corruptions are detected.</param>
		/// <param name="Thread">Optional Profiler thread.</param>
		public async Task<string[]> Analyze(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData, bool Repair,
			ProfilerThread Thread)
		{
			Thread?.Start();
			Output.WriteStartDocument();

			if (!string.IsNullOrEmpty(XsltPath))
			{
				if (File.Exists(XsltPath))
				{
					try
					{
						byte[] XsltBin = File.ReadAllBytes(XsltPath);

						Output.WriteProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"data:text/xsl;base64," +
							System.Convert.ToBase64String(XsltBin) + "\"");
					}
					catch (Exception)
					{
						Output.WriteProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"" + Encode(XsltPath) + "\"");
					}
				}
				else
					Output.WriteProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"" + Encode(XsltPath) + "\"");
			}

			Output.WriteStartElement("DatabaseStatistics", "http://waher.se/Schema/Persistence/Statistics.xsd");

			foreach (string CollectionName in (await this.database.ListCollectionNamesAsync()).ToEnumerable())
			{
				Thread?.NewState(CollectionName);

				IMongoCollection<BsonDocument> Collection = this.database.GetCollection<BsonDocument>(CollectionName);

				Output.WriteStartElement("File");
				Output.WriteAttributeString("id", Collection.CollectionNamespace.FullName);
				Output.WriteAttributeString("collectionName", CollectionName);
				Output.WriteAttributeString("count", (await Collection.CountDocumentsAsync(Builders<BsonDocument>.Filter.Empty)).ToString());

				if (!(Collection.Settings.WriteEncoding is null))
					Output.WriteAttributeString("encoding", Collection.Settings.WriteEncoding.WebName);

				if (Collection.Settings.WriteConcern.WTimeout.HasValue)
					Output.WriteAttributeString("timeoutMs", ((int)Collection.Settings.WriteConcern.WTimeout.Value.TotalMilliseconds).ToString());

				foreach (BsonDocument Index in (await Collection.Indexes.ListAsync()).ToEnumerable())
				{
					List<string> FieldNames = new List<string>();

					Output.WriteStartElement("Index");

					foreach (BsonElement E in Index.Elements)
					{
						switch (E.Name)
						{
							case "key":
								foreach (BsonElement E2 in E.Value.AsBsonDocument.Elements)
								{
									if (E2.Value.AsInt32 < 0)
										FieldNames.Add("-" + E2.Name);
									else
										FieldNames.Add(E2.Name);
								}
								break;

							case "name":
								Output.WriteAttributeString("id", E.Value.AsString);
								break;
						}
					}

					foreach (string Field in FieldNames)
						Output.WriteElementString("Field", Field);

					Output.WriteEndElement();
				}

				Output.WriteEndElement();
			}

			Output.WriteEndElement();
			Output.WriteEndDocument();

			Thread?.Idle();
			Thread?.Stop();

			return Array.Empty<string>();
		}

		/// <summary>
		/// Repairs a set of collections.
		/// </summary>
		/// <param name="CollectionNames">Set of collections to repair.</param>
		/// <returns>Collections repaired.</returns>
		public Task<string[]> Repair(params string[] CollectionNames)
		{
			return Task.FromResult<string[]>(Array.Empty<string>());
		}

		/// <summary>
		/// Repairs a set of collections.
		/// </summary>
		/// <param name="Thread">Optional Profiler thread.</param>
		/// <param name="CollectionNames">Set of collections to repair.</param>
		/// <returns>Collections repaired.</returns>
		public Task<string[]> Repair(ProfilerThread Thread, params string[] CollectionNames)
		{
			return Task.FromResult<string[]>(Array.Empty<string>());
		}

		private static string Encode(string s)
		{
			return s.
				Replace("&", "&amp;").
				Replace("<", "&lt;").
				Replace(">", "&gt;").
				Replace("\"", "&quot;").
				Replace("'", "&apos;");
		}

		/// <summary>
		/// Performs an export of the database.
		/// </summary>
		/// <param name="Output">Database will be output to this interface.</param>
		/// <param name="CollectionNames">Optional array of collections to export. If null, all collections will be exported.</param>
		/// <returns>If export process was completed (true), or terminated by <paramref name="Output"/> (false).</returns>
		public Task<bool> Export(IDatabaseExport Output, string[] CollectionNames)
		{
			return this.Export(Output, CollectionNames, null);
		}

		/// <summary>
		/// Performs an export of the database.
		/// </summary>
		/// <param name="Output">Database will be output to this interface.</param>
		/// <param name="CollectionNames">Optional array of collections to export. If null, all collections will be exported.</param>
		/// <param name="Thread">Optional Profiler thread.</param>
		/// <returns>If export process was completed (true), or terminated by <paramref name="Output"/> (false).</returns>
		public async Task<bool> Export(IDatabaseExport Output, string[] CollectionNames, ProfilerThread Thread)
		{
			bool Continue;

			Thread?.Start();
			if (!await Output.StartDatabase())
				return false;
			try
			{
				IDatabaseExportFilter Filter = Output as IDatabaseExportFilter;
				ObjectSerializer Serializer = this.GetObjectSerializerEx(typeof(GenericObject));
				BsonDeserializationArgs Args = new BsonDeserializationArgs()
				{
					NominalType = typeof(GenericObject)
				};

				foreach (string CollectionName in (await this.database.ListCollectionNamesAsync()).ToEnumerable())
				{
					if (!(CollectionNames is null) && Array.IndexOf(CollectionNames, CollectionName) < 0)
						continue;

					if (!(Filter is null) && !Filter.CanExportCollection(CollectionName))
						continue;

					Thread?.NewState(CollectionName);

					IMongoCollection<BsonDocument> Collection = this.database.GetCollection<BsonDocument>(CollectionName);

					if (!await Output.StartCollection(CollectionName))
						return false;
					try
					{
						foreach (BsonDocument Index in (await Collection.Indexes.ListAsync()).ToEnumerable())
						{
							if (!await Output.StartIndex())
								return false;

							foreach (BsonElement E in Index.Elements)
							{
								if (E.Name == "key")
								{
									foreach (BsonElement E2 in E.Value.AsBsonDocument.Elements)
									{
										if (!await Output.ReportIndexField(E2.Name, E2.Value.AsInt32 > 0))
											return false;
									}

									break;
								}
							}

							if (!await Output.EndIndex())
								return false;
						}

						foreach (BsonDocument Doc in (await Collection.FindAsync<BsonDocument>(Builders<BsonDocument>.Filter.Empty)).ToEnumerable())
						{
							BsonDocumentReader Reader = new BsonDocumentReader(Doc);
							BsonDeserializationContext Context = BsonDeserializationContext.CreateRoot(Reader);

							object Object = Serializer.Deserialize(Context, Args);

							if (Object is GenericObject Obj)
							{
								if (!(Filter is null) && !Filter.CanExportObject(Obj))
									continue;

								if (await Output.StartObject(Obj.ObjectId.ToString(), Obj.TypeName) is null)
									return false;
								try
								{
									foreach (KeyValuePair<string, object> P in Obj)
									{
										if (P.Value is ObjectId ObjectId)
										{
											if (!await Output.ReportProperty(P.Key, GeneratedObjectSerializerBase.ObjectIdToGuid(ObjectId)))
												return false;
										}
										else
										{
											if (!await Output.ReportProperty(P.Key, P.Value))
												return false;
										}
									}
								}
								catch (Exception ex)
								{
									Thread?.Exception(ex);
									if (!await this.ReportException(ex, Output))
										return false;
								}
								finally
								{
									Continue = await Output.EndObject();
								}

								if (!Continue)
									return false;
							}
							else if (!(Object is null))
							{
								if (!await Output.ReportError("Unable to load object " + Doc["_id"].AsString + "."))
									return false;
							}
						}
					}
					catch (Exception ex)
					{
						Thread?.Exception(ex);
						if (!await this.ReportException(ex, Output))
							return false;
					}
					finally
					{
						Continue = await Output.EndCollection();
					}

					if (!Continue)
						return false;
				}
			}
			catch (Exception ex)
			{
				Thread?.Exception(ex);
				if (!await this.ReportException(ex, Output))
					return false;
			}
			finally
			{
				Continue = await Output.EndDatabase();
				Thread?.Idle();
				Thread?.Stop();
			}

			return Continue;
		}

		private async Task<bool> ReportException(Exception ex, IDatabaseExport Output)
		{
			ex = Log.UnnestException(ex);

			if (ex is AggregateException ex2)
			{
				foreach (Exception ex3 in ex2.InnerExceptions)
				{
					if (!await Output.ReportException(ex3))
						return false;
				}

				return true;
			}
			else
				return await Output.ReportException(ex);
		}

		/// <summary>
		/// Performs an iteration of contents of the entire database.
		/// </summary>
		/// <typeparam name="T">Type of objects to iterate.</typeparam>
		/// <param name="Recipient">Recipient of iterated objects.</param>
		/// <param name="CollectionNames">Optional array of collections to export. If null, all collections will be exported.</param>
		/// <returns>Task object for synchronization purposes.</returns>
		public Task Iterate<T>(IDatabaseIteration<T> Recipient, string[] CollectionNames)
			where T : class
		{
			return this.Iterate(Recipient, CollectionNames, null);
		}

		/// <summary>
		/// Performs an iteration of contents of the entire database.
		/// </summary>
		/// <typeparam name="T">Type of objects to iterate.</typeparam>
		/// <param name="Recipient">Recipient of iterated objects.</param>
		/// <param name="CollectionNames">Optional array of collections to export. If null, all collections will be exported.</param>
		/// <param name="Thread">Optional Profiler thread.</param>
		/// <returns>Task object for synchronization purposes.</returns>
		public async Task Iterate<T>(IDatabaseIteration<T> Recipient, string[] CollectionNames, ProfilerThread Thread)
			where T : class
		{
			Thread?.Start();
			await Recipient.StartDatabase();
			try
			{
				ObjectSerializer Serializer = this.GetObjectSerializerEx(typeof(T));
				BsonDeserializationArgs Args = new BsonDeserializationArgs()
				{
					NominalType = typeof(GenericObject)
				};

				foreach (string CollectionName in (await this.database.ListCollectionNamesAsync()).ToEnumerable())
				{
					if (!(CollectionNames is null) && Array.IndexOf(CollectionNames, CollectionName) < 0)
						continue;

					Thread?.NewState(CollectionName);

					IMongoCollection<BsonDocument> Collection = this.database.GetCollection<BsonDocument>(CollectionName);

					await Recipient.StartCollection(CollectionName);
					try
					{
						foreach (BsonDocument Doc in (await Collection.FindAsync<BsonDocument>(Builders<BsonDocument>.Filter.Empty)).ToEnumerable())
						{
							BsonDocumentReader Reader = new BsonDocumentReader(Doc);
							BsonDeserializationContext Context = BsonDeserializationContext.CreateRoot(Reader);

							object Object = Serializer.Deserialize(Context, Args);

							if (Object is T Obj)
								await Recipient.ProcessObject(Obj);
							else if (!(Object is null))
							{
								ObjectId ObjectId = await Serializer.GetObjectId(Object, false);
								if (ObjectId != ObjectId.Empty)
									await Recipient.IncompatibleObject(ObjectId);
							}
						}
					}
					catch (Exception ex)
					{
						Thread?.Exception(ex);
						this.ReportException(ex, Recipient);
					}
					finally
					{
						await Recipient.EndCollection();
					}
				}
			}
			catch (Exception ex)
			{
				Thread?.Exception(ex);
				this.ReportException(ex, Recipient);
			}
			finally
			{
				await Recipient.EndDatabase();
				Thread?.Idle();
				Thread?.Stop();
			}
		}

		private void ReportException<T>(Exception ex, IDatabaseIteration<T> Recipient)
			where T : class
		{
			ex = Events.Log.UnnestException(ex);

			if (ex is AggregateException ex2)
			{
				foreach (Exception ex3 in ex2.InnerExceptions)
					Recipient.ReportException(ex3);
			}
			else
				Recipient.ReportException(ex);
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

		/// <summary>
		/// Called when processing starts.
		/// </summary>
		public Task Start()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called when processing ends.
		/// </summary>
		public Task Stop()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Persists any pending changes.
		/// </summary>
		public Task Flush()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Gets a persistent dictionary containing objects in a collection.
		/// </summary>
		/// <param name="Collection">Collection Name</param>
		/// <returns>Persistent dictionary</returns>
		public Task<IPersistentDictionary> GetDictionary(string Collection)
		{
			return Task.FromResult<IPersistentDictionary>(new StringDictionary("DICT_" + Collection, this));  // TODO
		}

		/// <summary>
		/// Gets an array of available dictionary collections.
		/// </summary>
		/// <returns>Array of dictionary collections.</returns>
		public async Task<string[]> GetDictionaries()
		{
			List<string> Collections = new List<string>();

			foreach (string CollectionName in (await this.database.ListCollectionNamesAsync()).ToEnumerable())
			{
				if (CollectionName.StartsWith("DICT_"))
					Collections.Add(CollectionName);
			}

			return Collections.ToArray();
		}

		/// <summary>
		/// Gets an array of available collections.
		/// </summary>
		/// <returns>Array of collections.</returns>
		public async Task<string[]> GetCollections()
		{
			List<string> Collections = new List<string>();

			foreach (string CollectionName in (await this.database.ListCollectionNamesAsync()).ToEnumerable())
			{
				if (!CollectionName.StartsWith("DICT_"))
					Collections.Add(CollectionName);
			}

			return Collections.ToArray();
		}

		/// <summary>
		/// Gets the collection corresponding to a given type.
		/// </summary>
		/// <param name="Type">Type</param>
		/// <returns>Collection name.</returns>
		public Task<string> GetCollection(Type Type)
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(Type);
			return Task.FromResult<string>(Serializer.CollectionName(null));
		}

		/// <summary>
		/// Gets the collection corresponding to a given object.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>Collection name.</returns>
		public Task<string> GetCollection(Object Object)
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(Object);
			return Task.FromResult<string>(Serializer.CollectionName(Object));
		}

		/// <summary>
		/// Checks if a string is a label in a given collection.
		/// </summary>
		/// <param name="CollectionName">Name of collection.</param>
		/// <param name="Label">Label to check.</param>
		/// <returns>If <paramref name="Label"/> is a label in the collection
		/// defined by <paramref name="CollectionName"/>.</returns>
		public async Task<bool> IsLabel(string CollectionName, string Label)
		{
			IMongoCollection<BsonDocument> Collection = this.GetCollection(CollectionName);
			FilterDefinition<BsonDocument> BsonFilter = Builders<BsonDocument>.Filter.Ne<string>(Label, null);
			IFindFluent<BsonDocument, BsonDocument> ResultSet = Collection.Find<BsonDocument>(BsonFilter);

			return !(await ResultSet.SingleAsync<BsonDocument>() is null);
		}

		/// <summary>
		/// Gets an array of available labels for a collection.
		/// </summary>
		/// <returns>Array of labels.</returns>
		public Task<string[]> GetLabels(string Collection)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Tries to get the Object ID of an object, if it exists.
		/// </summary>
		/// <param name="Object">Object whose Object ID is of interest.</param>
		/// <returns>Object ID, if found, null otherwise.</returns>
		public async Task<object> TryGetObjectId(object Object)
		{
			if (Object is null)
				return null;

			IObjectSerializer Serializer = this.GetObjectSerializer(Object.GetType());
			if (Serializer is ObjectSerializer SerializerEx &&
				SerializerEx.HasObjectId(Object))
			{
				return await SerializerEx.GetObjectId(Object, false);
			}
			else
				return null;
		}

		/// <summary>
		/// Drops a collection, if it exist.
		/// </summary>
		/// <param name="CollectionName">Name of collection.</param>
		public Task DropCollection(string CollectionName)
		{
			lock (this.collections)
			{
				this.collections.Remove(CollectionName);

				if (CollectionName == this.lastCollectionName)
				{
					this.lastCollection = null;
					this.lastCollectionName = string.Empty;
				}
			}

			return this.database.DropCollectionAsync(CollectionName);
		}

		/// <summary>
		/// Creates a generalized representation of an object.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>Generalized representation.</returns>
		public Task<GenericObject> Generalize(object Object)
		{
			if (Object is null)
				return Task.FromResult<GenericObject>(null);

			ObjectSerializer Serializer = this.GetObjectSerializerEx(Object);
			string CollectionName = Serializer.CollectionName(Object);

			BsonDocument Doc = Object.ToBsonDocument(Object.GetType(), Serializer);

			ObjectSerializer Deserializer = this.GetObjectSerializerEx(typeof(GenericObject));

			BsonDocumentReader Reader = new BsonDocumentReader(Doc);
			BsonDeserializationContext Context = BsonDeserializationContext.CreateRoot(Reader);
			BsonDeserializationArgs Args = new BsonDeserializationArgs()
			{
				NominalType = typeof(GenericObject)
			};

			if (Deserializer.Deserialize(Context, Args) is GenericObject Obj)
			{
				Obj.ArchivingTime = Serializer.GetArchivingTimeDays(Object);
				return Task.FromResult(Obj);
			}
			else
				throw new InvalidOperationException("Unable to generalize object.");
		}

		/// <summary>
		/// Creates a specialized representation of a generic object.
		/// </summary>
		/// <param name="Object">Generic object.</param>
		/// <returns>Specialized representation.</returns>
		public Task<object> Specialize(GenericObject Object)
		{
			if (Object is null)
				return Task.FromResult<object>(null);

			Type T = Types.GetType(Object.TypeName);
			if (T is null)
				return Task.FromResult<object>(Object);

			ObjectSerializer Serializer = this.GetObjectSerializerEx(typeof(GenericObject));
			string CollectionName = Serializer.CollectionName(Object);

			BsonDocument Doc = Object.ToBsonDocument(Object.GetType(), Serializer);

			Serializer = this.GetObjectSerializerEx(T);

			BsonDocumentReader Reader = new BsonDocumentReader(Doc);
			BsonDeserializationContext Context = BsonDeserializationContext.CreateRoot(Reader);
			BsonDeserializationArgs Args = new BsonDeserializationArgs()
			{
				NominalType = typeof(GenericObject)
			};

			return Task.FromResult<object>(Serializer.Deserialize(Context, Args));
		}

		/// <summary>
		/// Gets an array of collections that should be excluded from backups.
		/// </summary>
		/// <returns>Array of excluded collections.</returns>
		public string[] GetExcludedCollections()
		{
			SortedDictionary<string, bool> Sorted = new SortedDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

			lock (this.collections)
			{
				foreach (IObjectSerializer Serializer in this.serializers.Values)
				{
					if (Serializer is ObjectSerializer ObjectSerializer && !ObjectSerializer.BackupCollection)
						Sorted[ObjectSerializer.CollectionNameConstant] = true;
				}
			}

			string[] Result = new string[Sorted.Count];
			Sorted.Keys.CopyTo(Result, 0);

			return Result;
		}

		// TODO:
		//	* Created field
		//	* Updated field
		//	* RegEx fields
		//	* JavaScript fields
		//	* Binary fields (BLOBS)
		//	* Image fields
		//	* Collection indices.
		//	* Dictionary<string,T> fields.
		//	* SortedDictionary<string,T> fields.
		//	* Aggregates
		//  * Case insensitive strings.
		//  * Encrypted properties
	}
}
