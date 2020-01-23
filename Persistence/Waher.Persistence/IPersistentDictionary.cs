using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Persistence
{
	/// <summary>
	/// Persistent dictionary that can contain more entries than possible in the internal memory.
	/// </summary>
	public interface IPersistentDictionary : IDisposable, IDictionary<string, object>
	{
		/// <summary>
		/// Deletes the dictionary and disposes the object.
		/// </summary>
		void DeleteAndDispose();
	}
}
