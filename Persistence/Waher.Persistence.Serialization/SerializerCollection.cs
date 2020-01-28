using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using Waher.Runtime.Inventory;
using Waher.Persistence.Serialization.ReferenceTypes;
using Waher.Persistence.Serialization.ValueTypes;

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
#if NETSTANDARD1_5
		private readonly bool compiled;
#endif

#if NETSTANDARD1_5
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
#if NETSTANDARD1_5
			this.compiled = Compiled;
#endif

			ConstructorInfo DefaultConstructor;
			IObjectSerializer S;
			TypeInfo TI;

			foreach (Type T in Types.GetTypesImplementingInterface(typeof(IObjectSerializer)))
			{
				TI = T.GetTypeInfo();
				if (TI.IsAbstract || TI.IsGenericTypeDefinition)
					continue;

				DefaultConstructor = null;

				try
				{
					foreach (ConstructorInfo CI in TI.DeclaredConstructors)
					{
						if (CI.IsPublic && CI.GetParameters().Length == 0)
						{
							DefaultConstructor = CI;
							break;
						}
					}

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
			this.serializers[typeof(object)] = new GenericObjectSerializer(this.context, true);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (!(this.serializers is null))
			{
				IDisposable d;

				foreach (IObjectSerializer Serializer in this.serializers.Values)
				{
					d = Serializer as IDisposable;
					if (!(d is null))
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

			this.serializerAdded?.Dispose();
			this.serializerAdded = null;
		}

		/// <summary>
		/// Gets the object serializer corresponding to a specific type.
		/// </summary>
		/// <param name="Type">Type of object to serialize.</param>
		/// <returns>Object Serializer</returns>
		public IObjectSerializer GetObjectSerializer(Type Type)
		{
			IObjectSerializer Result;
			TypeInfo TI = Type.GetTypeInfo();

			lock (this.synchObj)
			{
				if (this.serializers.TryGetValue(Type, out Result))
					return Result;

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
#if NETSTANDARD1_5
				Result = new ObjectSerializer(Type, this.context, this.compiled);
#else
				Result = new ObjectSerializer(Type, this.context);
#endif
				lock (this.synchObj)
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

					lock (this.synchObj)
					{
						if (this.serializers.TryGetValue(Type, out Result))
							return Result;
					}
				}
			}

			return Result;
		}

	}
}
