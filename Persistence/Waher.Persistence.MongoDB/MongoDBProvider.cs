using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
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
		/// Inserts an object into the default collection of the database.
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
			Collection.InsertOne(Doc);
		}

	}
}
