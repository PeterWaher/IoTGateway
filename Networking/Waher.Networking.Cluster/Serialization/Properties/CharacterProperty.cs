using System;
using System.Collections.Generic;
using System.Reflection;

namespace Waher.Networking.Cluster.Serialization.Properties
{
	/// <summary>
	/// Character property
	/// </summary>
	public class CharacterProperty : Property
	{
		/// <summary>
		/// Character property
		/// </summary>
		/// <param name="PI">Property information</param>
		public CharacterProperty(PropertyInfo PI)
			: base(PI)
		{
		}

		/// <summary>
		/// Property Type
		/// </summary>
		public override Type PropertyType => typeof(char);

		/// <summary>
		/// Serializes the property value of an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Object">Object containing property</param>
		public override void Serialize(Serializer Output, object Object)
		{
			Output.WriteCharacter((char)this.pi.GetValue(Object));
		}
	}
}
