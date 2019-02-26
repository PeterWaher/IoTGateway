using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Waher.Networking.Cluster.Serialization.Properties
{
	/// <summary>
	/// Abstract base class for properties
	/// </summary>
	public abstract class Property : IProperty
	{
		/// <summary>
		/// Property Type
		/// </summary>
		public abstract Type PropertyType
		{
			get;
		}

		/// <summary>
		/// Serializes the property value of an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Value">Value to serialize</param>
		public abstract void Serialize(Serializer Output, object Value);
	}
}
