using System;
using Waher.Events;

namespace Waher.Persistence.Exceptions
{
	/// <summary>
	/// An attempt to insert a key was done, but the key was already there.
	/// </summary>
	public class KeyAlreadyExistsException : CollectionException
	{
		/// <summary>
		/// An attempt to insert a key was done, but the key was already there.
		/// </summary>
		/// <param name="Message">Exception message</param>
		/// <param name="Collection">Corresponding collection.</param>
		public KeyAlreadyExistsException(string Message, string Collection)
			: base(Message, Collection)
		{
		}
	}
}
