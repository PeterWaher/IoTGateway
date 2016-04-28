using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using Waher.Persistence.Filters;
using Waher.Persistence.MongoDB.Serialization;

namespace Waher.Persistence.MongoDB
{
	/// <summary>
	/// MongoDB database provider.
	/// </summary>
	public class MongoDBProvider : IDatabaseProvider
	{
		private Dictionary<string, IMongoCollection<BsonDocument>> collections = new Dictionary<string, IMongoCollection<BsonDocument>>();
		private Dictionary<string, ObjectSerializer> serializers = new Dictionary<string, ObjectSerializer>();
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
		public MongoDBProvider(string HostName, string DatabaseName, string DefaultCollectionName)
		{
			MongoClientSettings Settings = new MongoClientSettings();
			Settings.Server = new MongoServerAddress(HostName);
			this.Init(Settings, DatabaseName, DefaultCollectionName);
		}

		/// <summary>
		/// MongoDB database provider.
		/// </summary>
		/// <param name="HostName">Host name of MongoDB server.</param>
		/// <param name="Port">Port number used to connect to MongoDB server.</param>
		/// <param name="DatabaseName">Name of database.</param>
		public MongoDBProvider(string HostName, int Port, string DatabaseName, string DefaultCollectionName)
		{
			MongoClientSettings Settings = new MongoClientSettings();
			Settings.Server = new MongoServerAddress(HostName, Port);
			this.Init(Settings, DatabaseName, DefaultCollectionName);
		}

		/// <summary>
		/// MongoDB database provider.
		/// </summary>
		/// <param name="Settings">Connection settings.</param>
		/// <param name="DatabaseName">Name of database.</param>
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

		private IMongoCollection<BsonDocument> GetCollection(string CollectionName)
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

		private ObjectSerializer GetObjectSerializer(Type Type)
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
						Result = new ObjectSerializer(Type);
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
		public void Insert(object Object)
		{
			ObjectSerializer Serializer = this.GetObjectSerializer(Object);
			string CollectionName = Serializer.CollectionName;
			IMongoCollection<BsonDocument> Collection;

			if (string.IsNullOrEmpty(CollectionName))
				Collection = this.defaultCollection;
			else
				Collection = this.GetCollection(CollectionName);

			BsonDocument Doc = Object.ToBsonDocument(Object.GetType(), Serializer);
			Collection.InsertOneAsync(Doc);
		}

		/// <summary>
		/// Inserts a collection of objects into the database.
		/// </summary>
		/// <param name="Object">Objects to insert.</param>
		public void Insert(params object[] Objects)
		{
			this.Insert((IEnumerable<object>)Objects);
		}

		/// <summary>
		/// Inserts a collection of objects into the database.
		/// </summary>
		/// <param name="Object">Objects to insert.</param>
		public void Insert(IEnumerable<object> Objects)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="SortOrder">Sort order.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public async Task<IEnumerable<T>> Find<T>(params string[] SortOrder)
		{
			return await this.Find<T>(null, SortOrder);
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public async Task<IEnumerable<T>> Find<T>(Filter Filter, params string[] SortOrder)
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
				BsonFilter = this.Convert(Filter);

			IEnumerable<BsonDocument> Documents;

			if (SortOrder.Length > 0)
			{
				SortDefinition<BsonDocument> SortDefinition = null;

				foreach (string SortBy in SortOrder)
				{
					if (SortDefinition == null)
					{
						if (SortBy.StartsWith("-"))
							SortDefinition = Builders<BsonDocument>.Sort.Descending(SortBy.Substring(1));
						else
							SortDefinition = Builders<BsonDocument>.Sort.Ascending(SortBy);
					}
					else
					{
						if (SortBy.StartsWith("-"))
							SortDefinition = SortDefinition.Descending(SortBy.Substring(1));
						else
							SortDefinition = SortDefinition.Ascending(SortBy);
					}
				}

				Documents = Collection.Find<BsonDocument>(BsonFilter).Sort(SortDefinition).ToEnumerable<BsonDocument>();
			}
			else
			{
				IAsyncCursor<BsonDocument> Cursor = await Collection.FindAsync<BsonDocument>(BsonFilter);
				LinkedList<BsonDocument> Documents2 = new LinkedList<BsonDocument>();

				while (await Cursor.MoveNextAsync())
				{
					foreach (BsonDocument Object in Cursor.Current)
						Documents2.AddLast(Object);
				}

				Documents = Documents2;
			}

			LinkedList<T> Result = new LinkedList<T>();
			BsonDeserializationArgs Args = new BsonDeserializationArgs();
			Args.NominalType = typeof(T);

			foreach (BsonDocument Doc in Documents)
			{
				BsonDocumentReader Reader = new BsonDocumentReader(Doc);
				BsonDeserializationContext Context = BsonDeserializationContext.CreateRoot(Reader);

				T Obj = (T)Serializer.Deserialize(Context, Args);
				Result.AddLast(Obj);
			}

			return Result;
		}

		private FilterDefinition<BsonDocument> Convert(Filter Filter)
		{
			if (Filter is FilterChildren)
			{
				FilterChildren FilterChildren = (FilterChildren)Filter;
				Filter[] ChildFilters = FilterChildren.ChildFilters;
				int i, c = ChildFilters.Length;
				FilterDefinition<BsonDocument>[] Children = new FilterDefinition<BsonDocument>[c];

				for (i = 0; i < c; i++)
					Children[i] = this.Convert(ChildFilters[i]);

				if (Filter is FilterAnd)
					return Builders<BsonDocument>.Filter.And(Children);
				else if (Filter is FilterOr)
					return Builders<BsonDocument>.Filter.Or(Children);
				else
					throw this.UnknownFilterType(Filter);
			}
			else if (Filter is FilterChild)
			{
				FilterChild FilterChild = (FilterChild)Filter;
				FilterDefinition<BsonDocument> Child = this.Convert(FilterChild.ChildFilter);

				if (Filter is FilterNot)
					return Builders<BsonDocument>.Filter.Not(Child);
				else
					throw this.UnknownFilterType(Filter);
			}
			else if (Filter is FilterFieldValue)
			{
				FilterFieldValue FilterFieldValue = (FilterFieldValue)Filter;
				string FieldName = FilterFieldValue.FieldName;
				object Value = FilterFieldValue.Value;

				if (Filter is FilterFieldEqualTo)
				{
					if (Value is string)
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
					else
						throw this.UnhandledFilterValueDataType(Value);
				}
				else if (Filter is FilterFieldNotEqualTo)
				{
					if (Value is string)
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
					else
						throw this.UnhandledFilterValueDataType(Value);
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
					else
						throw this.UnhandledFilterValueDataType(Value);
				}
				else if (Filter is FilterFieldGreaterOrEqualTo)
				{
					if (Value is string)
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
					else
						throw this.UnhandledFilterValueDataType(Value);
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
					else
						throw this.UnhandledFilterValueDataType(Value);
				}
				else if (Filter is FilterFieldLesserOrEqualTo)
				{
					if (Value is string)
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
					else
						throw this.UnhandledFilterValueDataType(Value);
				}
				else
					throw this.UnknownFilterType(Filter);
			}
			else
			{
				if (Filter is FilterFieldLikeRegEx)
				{
					FilterFieldLikeRegEx FilterFieldLikeRegEx = (FilterFieldLikeRegEx)Filter;

					return Builders<BsonDocument>.Filter.Regex(FilterFieldLikeRegEx.FieldName, FilterFieldLikeRegEx.RegularExpression);
				}
				else
					throw this.UnknownFilterType(Filter);
			}
		}

		private Exception UnknownFilterType(Filter Filter)
		{
			return new NotSupportedException("Filters of type " + Filter.GetType().FullName + " not supported.");
		}

		private Exception UnhandledFilterValueDataType(object Value)
		{
			return new NotSupportedException("Filter values of type " + Value.GetType().FullName + " not supported.");
		}

		// TODO:
		//	* Insert Many
		//	* Find
		//	* Remove
		//	* Update
		//	* Object ID field
		//	* Created field
		//	* Updated field
		//	* Embedded objects
		//	* Object references
		//	* RegEx fields
		//	* Javascript fields
		//	* Array fields
		//	* Binary fields (BLOBS)
		//	* Image fields
		//	* Collection indices.
		//	* Dictionary<string,T> fields.
		//	* SortedDictionary<string,T> fields.
		//	* Aggregates
		//	* Type field.
		//	* Untyped Find, that takes the class name from a field in the object.
	}
}
