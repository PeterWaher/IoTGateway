using System;
using System.Collections.Generic;
using System.Text;
using Waher.Events;

namespace Waher.Runtime.Inventory
{
	/// <summary>
	/// Defines a class or struct as singleton. This means that when instantiated, using <see cref="Types.Instantiate(Type, object[])"/>,
	/// the same instance will be returned. Singleton instances will be disposed by the <see cref="Types"/> class, when the
	/// application ends, if they implement <see cref="IDisposable"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
	public class SingletonAttribute : Attribute
	{
		private static readonly Dictionary<SingletonKey, SingletonRecord> instances = new Dictionary<SingletonKey, SingletonRecord>();

		/// <summary>
		/// Defines a class or struct as singleton. This means that when instantiated, using <see cref="Types.Instantiate(Type, object[])"/>,
		/// the same instance will be returned. Singleton instances will be disposed by the <see cref="Types"/> class, when the
		/// application ends.
		/// </summary>
		public SingletonAttribute()
		{
		}

		internal static void Clear()
		{
			SingletonRecord[] Objects;

			lock (instances)
			{
				Objects = new SingletonRecord[instances.Count];
				instances.Values.CopyTo(Objects, 0);
				instances.Clear();
			}

			foreach (SingletonRecord Rec in Objects)
			{
				if (Rec.Instantiated && Rec.Instance is IDisposable Disposable)
				{
					try
					{
						Disposable.Dispose();
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}
		}

		/// <summary>
		/// Returns an instance of the type <paramref name="Type"/>.
		/// </summary>
		/// <param name="ReturnNullIfFail">If null should be returned instead for throwing exceptions.</param>
		/// <param name="Type">Type of objects to return.</param>
		/// <param name="Arguments">Constructor arguments.</param>
		/// <returns>Instance of <paramref name="Type"/>.</returns>
		public object Instantiate(bool ReturnNullIfFail, Type Type, params object[] Arguments)
		{
			SingletonKey Key = new SingletonKey(Type, Arguments);

			lock (instances)
			{
				if (instances.TryGetValue(Key, out SingletonRecord Rec))
					return Rec.Instance;
			}

			object Object = Types.Create(ReturnNullIfFail, Type, Arguments);
			if (Object is null)
				return null;

			lock (instances)
			{
				if (instances.TryGetValue(Key, out SingletonRecord Rec))
				{
					if (Object is IDisposable Disposable)
						Disposable.Dispose();

					return Rec.Instance;
				}

				instances[Key] = new SingletonRecord(Key, true, Object);
			}

			return Object;
		}

		/// <summary>
		/// Registers a singleton instance of a type.
		/// </summary>
		/// <param name="Object">Singleton object instance.</param>
		/// <param name="Arguments">Any constructor arguments associated with the object instance.</param>
		public static void Register(object Object, params object[] Arguments)
		{
			SingletonKey Key = new SingletonKey(Object.GetType(), Arguments);

			lock (instances)
			{
				if (instances.ContainsKey(Key))
					throw new InvalidOperationException("Singleton already registered.");

				instances[Key] = new SingletonRecord(Key, false, Object);
			}
		}

		/// <summary>
		/// Unregisters a singleton instance of a type.
		/// </summary>
		/// <param name="Object">Singleton object instance.</param>
		/// <param name="Arguments">Any constructor arguments associated with the object instance.</param>
		/// <returns>If the instance was found and removed.</returns>
		public static bool Unregister(object Object, params object[] Arguments)
		{
			SingletonKey Key = new SingletonKey(Object.GetType(), Arguments);

			lock (instances)
			{
				return instances.Remove(Key);
			}
		}

		/// <summary>
		/// Checks if a singleton type (with optional associated arguments) is registered.
		/// </summary>
		/// <param name="Type">Singleton type</param>
		/// <param name="Arguments">Any constructor arguments associated with the type.</param>
		/// <returns>If such a singleton type is registered.</returns>
		public static bool IsRegistered(Type Type, params object[] Arguments)
		{
			SingletonKey Key = new SingletonKey(Type, Arguments);

			lock (instances)
			{
				return instances.ContainsKey(Key);
			}
		}

		/// <summary>
		/// Gets available singleton instances.
		/// </summary>
		/// <returns>Singleton instances.</returns>
		public static SingletonRecord[] GetInstances()
		{
			lock (instances)
			{
				SingletonRecord[] Result = new SingletonRecord[instances.Count];
				instances.Values.CopyTo(Result, 0);
				return Result;
			}
		}
	}
}
