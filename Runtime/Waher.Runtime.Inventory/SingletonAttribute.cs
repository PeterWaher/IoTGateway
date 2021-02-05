using System;
using System.Collections.Generic;
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
		private static readonly Dictionary<SingletonKey, object> instances = new Dictionary<SingletonKey, object>();

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
			object[] Objects;

			lock (instances)
			{
				Objects = new object[instances.Count];
				instances.Values.CopyTo(Objects, 0);
			}

			foreach (object Object in Objects)
			{
				if (Object is IDisposable Disposable)
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
		/// <param name="Type">Type of objects to return.</param>
		/// <param name="Arguments">Constructor arguments.</param>
		/// <returns>Instance of <paramref name="Type"/>.</returns>
		public object Instantiate(Type Type, params object[] Arguments)
		{
			SingletonKey Key = new SingletonKey(Type, Arguments);

			lock (instances)
			{
				if (instances.TryGetValue(Key, out object Object))
					return Object;

				Object = Activator.CreateInstance(Type, Arguments);
				instances[Key] = Object;

				return Object;
			}
		}

		/// <summary>
		/// Registers a singleton instance of a type.
		/// </summary>
		/// <param name="Object">Singleton objcet instance.</param>
		/// <param name="Arguments">Any constructor arguments associated with the object instance.</param>
		public static void Register(object Object, params object[] Arguments)
		{
			SingletonKey Key = new SingletonKey(Object.GetType(), Arguments);

			lock (instances)
			{
				if (instances.ContainsKey(Key))
					throw new InvalidOperationException("Singleton already registered.");

				instances[Key] = Object;
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

		private class SingletonKey
		{
			public readonly Type type;
			public readonly object[] arguments;

			public SingletonKey(Type Type, object[] Arguments)
			{
				this.type = Type;
				this.arguments = Arguments;
			}

			public override bool Equals(object obj)
			{
				int i, c;

				if (!(obj is SingletonKey Key) ||
					this.type != Key.type ||
					(this.arguments is null) ^ (Key.arguments is null) ||
					(c = this.arguments?.Length ?? 0) != (Key.arguments?.Length ?? 0))
				{
					return false;
				}

				for (i = 0; i < c; i++)
				{
					if (!this.arguments[i].Equals(Key.arguments[i]))
						return false;
				}

				return true;
			}

			public override int GetHashCode()
			{
				int Result = this.type.GetHashCode();

				if (!(this.arguments is null))
				{
					foreach (object Obj in this.arguments)
						Result ^= Result << 5 ^ (Obj?.GetHashCode() ?? 0);
				}

				return Result;
			}
		}
	}
}
