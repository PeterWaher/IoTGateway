using System;
using System.Collections.Generic;
using System.Reflection;

namespace Waher.Networking.Cluster.Serialization.Properties
{
	/// <summary>
	/// Array property
	/// </summary>
	public class ArrayProperty : Property
	{
		private readonly Type arrayType;
		private readonly Type elementType;

		/// <summary>
		/// Array property
		/// </summary>
		public ArrayProperty(Type ArrayType, Type ElementType)
			: base()
		{
			this.arrayType = ArrayType;
			this.elementType = ElementType;
		}

		/// <summary>
		/// Property Type
		/// </summary>
		public override Type PropertyType => this.arrayType;

		/// <summary>
		/// Serializes the property value of an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Value">Value to serialize</param>
		public override void Serialize(Serializer Output, object Value)
		{
			Array A = (Array)Value;
			Type LastType = null;
			ObjectInfo LastInfo = null;

			if (A is null)
				Output.WriteVarUInt64(0);
			else
			{
				Output.WriteVarUInt64((ulong)A.Length);

				foreach (object Element in A)
				{
					if (Element is null)
						Output.WriteString(null);
					else
					{
						Type T = Element.GetType();

						if (LastType is null || T != LastType)
						{
							LastType = T;
							LastInfo = ClusterEndpoint.GetObjectInfo(T);

							Output.WriteString(T.FullName);
						}
						else
							Output.WriteString(string.Empty);

						LastInfo.Serialize(Output, Element);
					}
				}
			}
		}
	}
}
