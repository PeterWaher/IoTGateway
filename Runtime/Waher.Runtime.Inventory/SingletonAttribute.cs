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
		private static readonly Dictionary<SingletonKey, KeyValuePair<bool, object>> instances = new Dictionary<SingletonKey, KeyValuePair<bool, object>>();

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
			KeyValuePair<bool, object>[] Objects;

			lock (instances)
			{
				Objects = new KeyValuePair<bool, object>[instances.Count];
				instances.Values.CopyTo(Objects, 0);
				instances.Clear();
			}

			foreach (KeyValuePair<bool, object> P in Objects)
			{
				if (P.Key && P.Value is IDisposable Disposable)
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
				if (instances.TryGetValue(Key, out KeyValuePair<bool, object> P))
					return P.Value;
			}

			object Object = Types.Create(ReturnNullIfFail, Type, Arguments);
			if (Object is null)
				return null;

			lock (instances)
			{
				if (instances.TryGetValue(Key, out KeyValuePair<bool, object> P))
				{
					if (Object is IDisposable Disposable)
						Disposable.Dispose();

					return P.Value;
				}

				instances[Key] = new KeyValuePair<bool, object>(true, Object);
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

				instances[Key] = new KeyValuePair<bool, object>(false, Object);
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

			public override string ToString()
			{
				StringBuilder sb = new StringBuilder();
				int i, c;

				sb.Append(this.type.FullName);
				sb.Append('(');

				for (i = 0, c = this.arguments?.Length ?? 0; i < c; i++)
				{
					if (i > 0)
						sb.Append(", ");

					if (this.arguments[i] is null)
						sb.Append("null");
					else
						sb.Append(this.arguments[i].GetType().FullName);
				}

				sb.Append(')');

				return sb.ToString();
			}
		}
	}
}
