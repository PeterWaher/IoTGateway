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
		/// Property information
		/// </summary>
		protected PropertyInfo pi;

		/// <summary>
		/// Abstract base class for properties
		/// </summary>
		/// <param name="PI">Property information</param>
		public Property(PropertyInfo PI)
		{
			this.pi = PI;
		}

		/// <summary>
		/// Property Type
		/// </summary>
		public abstract Type PropertyType
		{
			get;
		}

		/// <summary>
		/// Property name
		/// </summary>
		public string Name => this.pi.Name;

		/// <summary>
		/// Serializes the property value of an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Object">Object containing property</param>
		public abstract void Serialize(Serializer Output, object Object);

	}
}
