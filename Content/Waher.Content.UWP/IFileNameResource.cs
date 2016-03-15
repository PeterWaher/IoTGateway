using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Waher.Script;

namespace Waher.Content
{
	/// <summary>
	/// Basic interface for resources having a FileName property.
	/// </summary>
	public interface IFileNameResource
	{
		/// <summary>
		/// Filename of resource.
		/// </summary>
		string FileName
		{
			get;
			set;
		}
	}
}
