using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Waher.Networking.Cluster.Serialization.Properties
{
	/// <summary>
	/// Abstract base class for properties
	/// </summary>
	public interface IProperty
	{
		/// <summary>
		/// Property Type
		/// </summary>
		Type PropertyType
		{
			get;
		}

		/// <summary>
		/// Property name
		/// </summary>
		string Name
		{
			get;
		}

		/// <summary>
		/// Serializes the property value of an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Object">Object containing property</param>
		void Serialize(Serializer Output, object Object);
	}
}
