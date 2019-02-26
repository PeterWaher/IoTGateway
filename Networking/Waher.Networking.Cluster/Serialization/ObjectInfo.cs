using System;
using System.Collections.Generic;
using System.Reflection;
using Waher.Networking.Cluster.Serialization.Properties;

namespace Waher.Networking.Cluster.Serialization
{
	internal class ObjectInfo
	{
		public PropertyReference[] Properties;
		public string TypeName;

		public void Serialize(Serializer Output, object Object)
		{
			Output.WriteString(this.TypeName);

			if (!(this.Properties is null))
			{
				foreach (PropertyReference Property in this.Properties)
				{
					Output.WriteString(Property.Name);
					object Value = Property.Info.GetValue(Object);
					Property.Property.Serialize(Output, Value);
				}
			}
		}
	}
}
