using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Persistence.Serialization.ReferenceTypes;
using Waher.Persistence.Serialization.ValueTypes;
using Waher.Script.Abstraction.Elements;

namespace Waher.Persistence.Serialization
{
	/// <summary>
	/// Maintains a set of serializers.
	/// </summary>
	public class SerializerCollection : IDisposable
	{
		private Dictionary<Type, IObjectSerializer> serializers;
		private AutoResetEvent serializerAdded = new AutoResetEvent(false);
		private readonly ISerializerContext context;
		private readonly object synchObj = new object();
#if COMPILED
		private readonly bool compiled;
#endif

#if COMPILED
		/// <summary>
		/// Maintains a set of serializers.
		/// </summary>
		/// <param name="Context">Serialization context.</param>
		/// <param name="Compiled">If object serializers should be compiled or not.</param>
		public SerializerCollection(ISerializerContext Context, bool Compiled)
#else
		/// <summary>
		/// Maintains a set of serializers.
		/// </summary>
		/// <param name="Context">Serialization context.</param>
		public SerializerCollection(ISerializerContext Context)
#endif
		{
			this.context = Context;
			this.serializers = new Dictionary<Type, IObjectSerializer>();
#if COMPILED
			this.compiled = Compiled;
#endif

			ConstructorInfo DefaultConstructor;
			IObjectSerializer S;

			foreach (Type T in Types.GetTypesImplementingInterface(typeof(IObjectSerializer)))
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

			this.serializers[typeof(GenericObject)] = new GenericObjectSerializer(this.context, false);
			this.serializers[typeof(Dictionary<string, object>)] = new StringDictionaryObjectSerializer(this.context);
			this.serializers[typeof(Dictionary<string, IElement>)] = new ObjectExNihiloObjectSerializer(this.context);
			this.serializers[typeof(KeyValuePair<string, object>[])] = new TagsObjectSerializer(this.context);
			this.serializers[typeof(KeyValuePair<string, IElement>[])] = new TagElementsObjectSerializer(this.context);
			this.serializers[typeof(object)] = new GenericObjectSerializer(this.context, true);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (!(this.serializers is null))
			{
				lock (this.synchObj)
				{
					foreach (IObjectSerializer Serializer in this.serializers.Values)
					{
						if (Serializer is IDisposable d)
						{
							try
							{
								d.Dispose();
							}
							catch (Exception)
							{
								// Ignore
							}
						}
					}

					this.serializers.Clear();
					this.serializers = null;
				}
			}

			this.serializerAdded?.Dispose();
			this.serializerAdded = null;
		}

		/// <summary>
		/// Gets the object serializer corresponding to a specific type, if one exists.
		/// </summary>
		/// <param name="Type">Type of object to serialize.</param>
		/// <returns>Object Serializer if exists, or null if not.</returns>
		public Task<IObjectSerializer> GetObjectSerializerNoCreate(Type Type)
		{
			lock (this.synchObj)
			{
				if (this.serializers.TryGetValue(Type, out IObjectSerializer Result))
					return Task.FromResult<IObjectSerializer>(Result);
			}

			return null;
		}

		/// <summary>
		/// Gets the object serializer corresponding to a specific type.
		/// </summary>
		/// <param name="Type">Type of object to serialize.</param>
		/// <returns>Object Serializer</returns>
		public async Task<IObjectSerializer> GetObjectSerializer(Type Type)
		{
			IObjectSerializer Result;
			TypeInfo TI = Type.GetTypeInfo();
			DateTime Start = DateTime.Now;

			do
			{
				lock (this.synchObj)
				{
					if (this.serializers.TryGetValue(Type, out Result))
					{
						if (Result is ObjectSerializer ObjectSerializer)
						{
							if (ObjectSerializer.Prepared)
								return Result;
							else
								Result = null;  // Wait for preparation process of serializer to complete (or fail).
						}
						else
							return Result;
					}
					else
					{
						if (TI.IsEnum)
							Result = new EnumSerializer(Type);
						else if (Type.IsArray)
						{
							Type ElementType = Type.GetElementType();
							Type T = Types.GetType(typeof(ByteArraySerializer).FullName.Replace("ByteArray", "Array"));
							Type SerializerType = T.MakeGenericType(new Type[] { ElementType });
							Result = (IObjectSerializer)Activator.CreateInstance(SerializerType, this.context);
						}
						else if (TI.IsGenericType)
						{
							Type GT = Type.GetGenericTypeDefinition();
							if (GT == typeof(Nullable<>))
							{
								Type NullableType = Type.GenericTypeArguments[0];

								if (NullableType.GetTypeInfo().IsEnum)
									Result = new NullableTypes.NullableEnumSerializer(NullableType);
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

						Result = new ObjectSerializer();
						this.serializers[Type] = Result;
					}
				}

				if (Result is null)
				{
					if (DateTime.Now.Subtract(Start).TotalMinutes >= 1)
						throw new Exception("Unable to create an object serializer for type: " + Type.FullName + ". Timeout.");

					await Task.Delay(100);  // Await for compilation of previous attempt completes.
				}
			}
			while (Result is null);

			try
			{
#if COMPILED
				await ((ObjectSerializer)Result).Prepare(Type, this.context, this.compiled);
#else
				await ((ObjectSerializer)Result).Prepare(Type, this.context);
#endif
				await Result.Init();

				this.serializerAdded.Set();
			}
			catch (FileLoadException ex)
			{
				// Serializer in the process of being generated from another task or thread.

				while (true)
				{
					if (!this.serializerAdded.WaitOne(1000))
						ExceptionDispatchInfo.Capture(ex).Throw();

					lock (this.synchObj)
					{
						if (this.serializers.TryGetValue(Type, out Result))
							return Result;
					}
				}
			}
			catch (Exception ex)
			{
				lock (this.synchObj)
				{
					if ((this.serializers?.TryGetValue(Type, out IObjectSerializer Result2) ?? false) &&
						Result2 == Result)
					{
						this.serializers.Remove(Type);
					}

					ExceptionDispatchInfo.Capture(ex).Throw();
				}
			}

			return Result;
		}

		/// <summary>
		/// Gets an array of collections that should be excluded from backups.
		/// </summary>
		/// <returns>Array of excluded collections.</returns>
		public string[] GetExcludedCollections()
		{
			SortedDictionary<string, bool> Sorted = new SortedDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

			lock (this.synchObj)
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

	}
}
