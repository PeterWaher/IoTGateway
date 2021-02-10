using System;
using System.Collections.Generic;
using System.Reflection;

namespace Waher.Runtime.Inventory
{
	/// <summary>
	/// Defines a default implementation for an interface. If a request to instantiate an interface is made,
	/// the default implementation will be instantiated.
	/// </summary>
	[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
	public class DefaultImplementationAttribute : Attribute
	{
		private static readonly Dictionary<Type, Type> defaultImplementationOverrides = new Dictionary<Type, Type>();
		private readonly Type type;

		/// <summary>
		/// Defines a default implementation for an interface. If a request to instantiate an interface is made,
		/// the default implementation will be instantiated.
		/// </summary>
		/// <param name="Type">Type to instantiate.</param>
		public DefaultImplementationAttribute(Type Type)
		{
			if (Type is null)
				throw new ArgumentException("Type cannot be null.", nameof(Type));

			this.type = Type;
		}

		/// <summary>
		/// Type to instantiate.
		/// </summary>
		public Type Type => this.type;

		/// <summary>
		/// Registers a default implementation for an interface.
		/// </summary>
		/// <param name="From">Type of interface.</param>
		/// <param name="To">Default implementation.</param>
		public static void RegisterDefaultImplementation(Type From, Type To)
		{
			lock (defaultImplementationOverrides)
			{
				if (defaultImplementationOverrides.ContainsKey(From))
					throw new InvalidOperationException("Default implemnentation already registered.");

				defaultImplementationOverrides[From] = To;
			}
		}

		/// <summary>
		/// Unregisters a default implementation for an interface.
		/// </summary>
		/// <param name="From">Type of interface.</param>
		/// <param name="To">Default implementation.</param>
		public static bool UnregisterDefaultImplementation(Type From, Type To)
		{
			lock (defaultImplementationOverrides)
			{
				if (defaultImplementationOverrides.TryGetValue(From, out Type To2) && To == To2)
					return defaultImplementationOverrides.Remove(From);
				else
					return false;
			}
		}

		/// <summary>
		/// Tries to get the default implementation for an interface.
		/// </summary>
		/// <param name="Type">Type of interface.</param>
		/// <param name="DefaultImplementation">Default implementation to use for interface.</param>
		/// <returns>If a default implementation was found.</returns>
		public static bool TryGetDefaultImplementation(Type Type, out Type DefaultImplementation)
		{
			lock (defaultImplementationOverrides)
			{
				if (defaultImplementationOverrides.TryGetValue(Type, out DefaultImplementation))
					return true;
			}

			TypeInfo TI = Type.GetTypeInfo();
			DefaultImplementationAttribute Attr = TI.GetCustomAttribute<DefaultImplementationAttribute>(true);

			if (!(Attr is null))
			{
				DefaultImplementation = Attr.Type;
				return true;
			}
			else
				return false;
		}
	}
}
