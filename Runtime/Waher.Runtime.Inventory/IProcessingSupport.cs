using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Runtime.Inventory
{
	/// <summary>
	/// Interface for detecting interfaces supporting objects with predefined features.
	/// </summary>
	/// <typeparam name="T">Type defining features to look for.</typeparam>
	public interface IProcessingSupport<T>
	{
		/// <summary>
		/// If the interface understands objects such as <paramref name="Object"/>.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>How well objects of this type are supported.</returns>
		Grade Supports(T Object);
	}
}
