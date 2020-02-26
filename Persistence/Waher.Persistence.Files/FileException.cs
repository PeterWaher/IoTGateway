using System;
using System.Collections.Generic;
using System.IO;
using Waher.Events;

namespace Waher.Persistence.Files
{
	/// <summary>
	/// Exception related to a file.
	/// </summary>
	public class FileException : IOException, IObject, ITags
	{
		private readonly string fileName;
		private readonly string collection;

		/// <summary>
		/// Exception related to a file.
		/// </summary>
		/// <param name="Message">Exception message</param>
		/// <param name="FileName">File Name</param>
		/// <param name="Collection">Corresponding collection.</param>
		public FileException(string Message, string FileName, string Collection)
			: base(Message)
		{
			this.fileName = FileName;
			this.collection = Collection;
		}

		/// <summary>
		/// File name.
		/// </summary>
		public string Object => this.fileName;

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
