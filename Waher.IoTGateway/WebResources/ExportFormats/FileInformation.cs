using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.IoTGateway.WebResources.ExportFormats
{
	/// <summary>
	/// Class containing information about a file.
	/// </summary>
	public class FileInformation
	{
		private readonly string name;
		private readonly DateTime created;
		private readonly long size;
		private readonly string sizeStr;

		/// <summary>
		/// Class containing information about a file.
		/// </summary>
		/// <param name="Name">Name of file</param>
		/// <param name="Created">When file was created</param>
		/// <param name="Size">Size of file</param>
		/// <param name="SizeStr">Formatted string containing size of file using appropriate unit</param>
		public FileInformation(string Name, DateTime Created, long Size, string SizeStr)
		{
			this.name = Name;
			this.created = Created;
			this.size = Size;
			this.sizeStr = SizeStr;
		}

		/// <summary>
		/// Name of file
		/// </summary>
		public string Name
		{
			get { return this.name; }
		}

		/// <summary>
		/// When file was created
		/// </summary>
		public DateTime Created
		{
			get { return this.created; }
		}

		/// <summary>
		/// Size of file
		/// </summary>
		public long Size
		{
			get { return this.size; }
		}

		/// <summary>
		/// Formatted string containing size of file using appropriate unit
		/// </summary>
		public string SizeStr
		{
			get { return this.sizeStr; }
		}

		/// <summary>
		/// If the file represents a key file
		/// </summary>
		public bool IsKey
		{
			get { return this.name.EndsWith(".key", StringComparison.CurrentCultureIgnoreCase); }
		}
	}
}
