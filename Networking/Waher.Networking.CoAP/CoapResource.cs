using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// Base class for CoAP resources.
	/// </summary>
    public abstract class CoapResource
    {
		private string path;

		/// <summary>
		/// Base class for CoAP resources.
		/// </summary>
		/// <param name="Path">Path of resource.</param>
		public CoapResource(string Path)
		{
			this.path = Path;
		}

		/// <summary>
		/// Path of resource.
		/// </summary>
		public string Path
		{
			get { return this.path; }
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public abstract bool HandlesSubPaths
		{
			get;
		}
	}
}
