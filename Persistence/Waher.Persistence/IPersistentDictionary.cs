using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

		/// <summary>
		/// Clears the dictionary.
		/// </summary>
		Task ClearAsync();

		/// <summary>
		/// Determines whether the System.Collections.Generic.IDictionary{string,object} contains an element with the specified key.
		/// </summary>
		/// <param name="key">The key to locate in the System.Collections.Generic.IDictionary{string,object}.</param>
		/// <returns>true if the System.Collections.Generic.IDictionary{string,object} contains an element with the key; otherwise, false.</returns>
		Task<bool> ContainsKeyAsync(string key);

		/// <summary>
		/// Adds an element with the provided key and value to the System.Collections.Generic.IDictionary{string,object}.
		/// </summary>
		/// <param name="key">The object to use as the key of the element to add.</param>
		/// <param name="value">The object to use as the value of the element to add.</param>
		/// <exception cref="ArgumentNullException">key is null</exception>
		/// <exception cref="ArgumentException">An element with the same key already exists in the System.Collections.Generic.IDictionary{string,object}.</exception>
		Task AddAsync(string key, object value);

		/// <summary>
		/// Adds an element with the provided key and value to the System.Collections.Generic.IDictionary{string,object}.
		/// </summary>
		/// <param name="key">The object to use as the key of the element to add.</param>
		/// <param name="value">The object to use as the value of the element to add.</param>
		/// <param name="ReplaceIfExists">If replacement of any existing value is desired.</param>
		/// <exception cref="ArgumentNullException">key is null</exception>
		/// <exception cref="ArgumentException">An element with the same key already exists in the System.Collections.Generic.IDictionary{string,object}.</exception>
		Task AddAsync(string key, object value, bool ReplaceIfExists);

		/// <summary>
		/// Removes the element with the specified key from the System.Collections.IDictionary object.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">key is null.</exception>
		Task<bool> RemoveAsync(string key);

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key whose value to get.</param>
		/// <returns>Returns a pair of values:
		/// 
		/// First value is true if the object that implements System.Collections.Generic.IDictionary{string,object} contains an element 
		/// with the specified key; otherwise, false.
		/// When this method returns, the second value contains the value associated with the key, if the key is found; otherwise, 
		/// the default value for the type of the value parameter.</returns>
		/// <exception cref="ArgumentNullException">key is null.</exception>
		Task<KeyValuePair<bool, object>> TryGetValueAsync(string key);

	}
}
