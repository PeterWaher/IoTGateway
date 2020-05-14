using System;
using System.Collections.Generic;
using System.Reflection;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects.Matrices;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Runtime.PropertyEnumerators
{
	/// <summary>
	/// Enumerates any type of object.
	/// </summary>
	public class DefaultPropertyEnumerator : IPropertyEnumerator
	{
		/// <summary>
		/// Enumerates any type of object.
		/// </summary>
		public DefaultPropertyEnumerator()
		{
		}

		/// <summary>
		/// Enumerates the properties of an object (of a type it supports).
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>Property enumeration as a script element.</returns>
		public IElement EnumerateProperties(object Object)
		{
			List<IElement> Elements = new List<IElement>();
			Type T = Object.GetType();

			foreach (PropertyInfo PI in T.GetRuntimeProperties())
			{
				if (PI.GetIndexParameters().Length > 0)
					continue;

				Elements.Add(new StringValue(PI.Name));
				Elements.Add(Expression.Encapsulate(PI.GetValue(Object)));
			}

			ObjectMatrix M = new ObjectMatrix(Elements.Count / 2, 2, Elements)
			{
				ColumnNames = new string[] { "Name", "Value" }
			};

			return M;
		}

		/// <summary>
		/// If the interface understands objects such as <paramref name="Object"/>.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>How well objects of this type are supported.</returns>
		public Grade Supports(Type Object)
		{
			return Grade.Barely;
		}
	}
}
