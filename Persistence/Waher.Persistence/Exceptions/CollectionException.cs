using System;
using System.Collections.Generic;
using System.IO;
using Waher.Events;

namespace Waher.Persistence.Exceptions
{
	/// <summary>
	/// Exception related to a collection.
	/// </summary>
	public class CollectionException : DatabaseException, IEventTags
	{
		private readonly string collection;

		/// <summary>
		/// Exception related to a collection.
		/// </summary>
		/// <param name="Message">Exception message</param>
		/// <param name="Collection">Corresponding collection.</param>
		public CollectionException(string Message, string Collection)
			: base(Message)
		{
			this.collection = Collection;
		}

		/// <summary>
		/// Collection.
		/// </summary>
		public string Collection => this.collection;

		/// <summary>
		/// Tags related to the object.
		/// </summary>
		public KeyValuePair<string, object>[] Tags
		{
			get
			{
				if (string.IsNullOrEmpty(this.collection))
					return new KeyValuePair<string, object>[0];
				else
				{
					return new KeyValuePair<string, object>[]
					{
						new KeyValuePair<string, object>("Collection", this.collection)
					};
				}
			}
		}
	}
}
