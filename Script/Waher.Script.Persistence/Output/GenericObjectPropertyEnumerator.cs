using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence.Serialization;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Functions.Runtime.PropertyEnumerators;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;

namespace Waher.Script.Persistence.Output
{
	/// <summary>
	/// Enumerates properties in a <see cref="GenericObject"/> object.
	/// </summary>
	public class GenericObjectPropertyEnumerator : IPropertyEnumerator
	{
		/// <summary>
		/// Enumerates properties in a <see cref="GenericObject"/> object.
		/// </summary>
		public GenericObjectPropertyEnumerator()
		{
		}

		/// <summary>
		/// Enumerates the properties of an object (of a type it supports).
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>Property enumeration as a script element.</returns>
		public Task<IElement> EnumerateProperties(object Object)
		{
			if (Object is GenericObject Obj)
			{
				ChunkedList<IElement> Elements = new ChunkedList<IElement>();

				if (!string.IsNullOrEmpty(Obj.CollectionName) && !Obj.TryGetFieldValue("CollectionName", out object _))
				{
					Elements.Add(new StringValue("CollectionName"));
					Elements.Add(Expression.Encapsulate(Obj.CollectionName));
				}

				if (!string.IsNullOrEmpty(Obj.TypeName) && !Obj.TryGetFieldValue("TypeName", out object _))
				{
					Elements.Add(new StringValue("TypeName"));
					Elements.Add(Expression.Encapsulate(Obj.TypeName));
				}

				if (Obj.ObjectId != Guid.Empty && !Obj.TryGetFieldValue("ObjectId", out object _))
				{
					Elements.Add(new StringValue("ObjectId"));
					Elements.Add(Expression.Encapsulate(Obj.ObjectId));
				}

				foreach (KeyValuePair<string, object> P in Obj.Properties)
				{
					Elements.Add(new StringValue(P.Key));
					Elements.Add(Expression.Encapsulate(P.Value));
				}

				ObjectMatrix M = new ObjectMatrix(Elements.Count / 2, 2, Elements)
				{
					ColumnNames = new string[] { "Name", "Value" }
				};

				return Task.FromResult<IElement>(M);
			}
			else
				return Task.FromResult<IElement>(ObjectValue.Null);
		}

		/// <summary>
		/// If the interface understands objects such as <paramref name="Object"/>.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>How well objects of this type are supported.</returns>
		public Grade Supports(Type Object)
		{
			if (Object == typeof(GenericObject))
				return Grade.Ok;
			else
				return Grade.NotAtAll;
		}
	}
}
