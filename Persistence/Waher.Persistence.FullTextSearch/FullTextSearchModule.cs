using System;
using System.Collections.Generic;
using System.Reflection;
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
		private static Dictionary<Type, TypeInformation> typesChecked;

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
			typesChecked = new Dictionary<Type, TypeInformation>();

			Database.ObjectInserted += this.Database_ObjectInserted;
			Database.ObjectUpdated += this.Database_ObjectUpdated;
			Database.ObjectDeleted += this.Database_ObjectDeleted;
		}

		/// <summary>
		/// Stops the module.
		/// </summary>
		public Task Stop()
		{
			Database.ObjectInserted -= this.Database_ObjectInserted;
			Database.ObjectUpdated -= this.Database_ObjectUpdated;
			Database.ObjectDeleted -= this.Database_ObjectDeleted;

			collectionInformation.Dispose();
			collectionInformation = null;

			typesChecked.Clear();
			typesChecked = null;

			return Task.CompletedTask;
		}

		private async void Database_ObjectInserted(object Sender, ObjectEventArgs e)
		{
			try
			{
				Tuple<CollectionInformation, TypeInformation, GenericObject> P = await this.Process(e.Object);
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
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Gets the indexable property values from an object. Property values will be returned in lower-case.
		/// </summary>
		/// <param name="Obj">Generic object.</param>
		/// <param name="PropertyNames">Indexable property names.</param>
		/// <returns>Indexable property values found.</returns>
		public static Dictionary<string, string> GetIndexableProperties(object Obj, params string[] PropertyNames)
		{
			Type T = Obj?.GetType() ?? typeof(object);
			TypeInfo TI = T.GetTypeInfo();
			TypeInformation TypeInfo = new TypeInformation(T, TI);
			return TypeInfo.GetIndexableProperties(Obj, PropertyNames);
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

			return Result;
		}

		private async Task<Tuple<CollectionInformation, TypeInformation, GenericObject>> Process(object Object)
		{
			CollectionInformation CollectionInfo;
			string CollectionName;

			if (Object is GenericObject GenObj)
			{
				CollectionName = GenObj.CollectionName;
				KeyValuePair<bool, object> P = await collectionInformation.TryGetValueAsync(CollectionName);

				if (!P.Key || (CollectionInfo = P.Value as CollectionInformation) is null)
				{
					CollectionInfo = new CollectionInformation(CollectionName, false);
					await collectionInformation.AddAsync(CollectionName, CollectionInfo);
					return null;
				}
				else if (CollectionInfo.IndexForFullTextSearch)
					return new Tuple<CollectionInformation, TypeInformation, GenericObject>(CollectionInfo, null, GenObj);
				else
					return null;
			}
			else
			{
				Type T = Object.GetType();
				TypeInformation TypeInfo;
				bool New;

				lock (typesChecked)
				{
					if (typesChecked.TryGetValue(T, out TypeInfo))
					{
						CollectionInfo = TypeInfo.CollectionInformation;
						if (CollectionInfo is null || !CollectionInfo.IndexForFullTextSearch)
							return null;

						CollectionName = CollectionInfo.CollectionName;
						New = false;
					}
					else
					{
						TypeInfo TI = T.GetTypeInfo();

						FullTextSearchAttribute SearchAttr = TI.GetCustomAttribute<FullTextSearchAttribute>(true);
						CollectionNameAttribute CollectionAttr = TI.GetCustomAttribute<CollectionNameAttribute>(true);

						if (CollectionAttr is null)
						{
							typesChecked[T] = new TypeInformation(T, TI)
							{
								CollectionInformation = new CollectionInformation(string.Empty, false)
							};

							return null;
						}
						else
						{
							CollectionName = CollectionAttr.Name;

							if (SearchAttr is null)
								CollectionInfo = new CollectionInformation(CollectionName, false);
							else
								CollectionInfo = new CollectionInformation(CollectionName, true, SearchAttr.PropertyNames);

							typesChecked[T] = TypeInfo = new TypeInformation(T, TI)
							{
								CollectionInformation = CollectionInfo
							};

							New = true;
						}
					}
				}

				if (New)
					await collectionInformation.AddAsync(CollectionName, CollectionInfo);

				if (!CollectionInfo.IndexForFullTextSearch)
					return null;
		
				return new Tuple<CollectionInformation, TypeInformation, GenericObject>(CollectionInfo, TypeInfo, null);
			}
		}

		private async void Database_ObjectUpdated(object Sender, ObjectEventArgs e)
		{
			try
			{
				Tuple<CollectionInformation, TypeInformation, GenericObject> P = await this.Process(e.Object);
				if (P is null)
					return;

			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private async void Database_ObjectDeleted(object Sender, ObjectEventArgs e)
		{
			try
			{
				Tuple<CollectionInformation, TypeInformation, GenericObject> P = await this.Process(e.Object);
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
