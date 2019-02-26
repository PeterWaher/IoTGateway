using System;
using System.Collections.Generic;
using System.Reflection;
using Waher.Networking.Cluster.Serialization.Properties;

namespace Waher.Networking.Cluster.Serialization
{
	internal class PropertyReference
	{
		public string Name;
		public PropertyInfo Info;
		public IProperty Property;
	}
}
