using System;
using System.Collections.Generic;
using System.IO;
using Waher.Events;
using Waher.Persistence.Exceptions;

namespace Waher.Persistence.Files
{
	/// <summary>
	/// Exception related to a file.
	/// </summary>
	public class FileException : CollectionException, IEventObject
	{
		private readonly string fileName;

		/// <summary>
		/// Exception related to a file.
		/// </summary>
		/// <param name="Message">Exception message</param>
		/// <param name="FileName">File Name</param>
		/// <param name="Collection">Corresponding collection.</param>
		public FileException(string Message, string FileName, string Collection)
			: base(Message, Collection)
		{
			this.fileName = FileName;
		}

		/// <summary>
		/// File name.
		/// </summary>
		public string Object => this.fileName;
	}
}
