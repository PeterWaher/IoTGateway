using System;
using System.Collections.Generic;
using System.Text;
using Waher.Runtime.Inventory;

namespace Waher.Content.Text
{
	/// <summary>
	/// Provides a JSON Encoding hint for an object that implements this interface.
	/// </summary>
	public interface IJsonEncodingHint
	{
		/// <summary>
		/// To what extent the object supports JSON encoding.
		/// </summary>
		Grade CanEncodeJson { get; }
	}
}
