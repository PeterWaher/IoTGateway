using System;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Functions.Runtime.PropertyEnumerators
{
	/// <summary>
	/// Interface for property enumerators.
	/// </summary>
	public interface IPropertyEnumerator : IProcessingSupport<Type>
	{
		/// <summary>
		/// Enumerates the properties of an object (of a type it supports).
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>Property enumeration as a script element.</returns>
		Task<IElement> EnumerateProperties(object Object);
	}
}
