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

		/// <summary>
		/// If the resource is observable.
		/// </summary>
		public virtual bool Observable
		{
			get { return false; }
		}

		/// <summary>
		/// Optional title of resource.
		/// </summary>
		public virtual string Title
		{
			get { return null; }
		}

		/// <summary>
		/// Optional resource type.
		/// </summary>
		public virtual string[] ResourceTypes
		{
			get { return null; }
		}

		/// <summary>
		/// Optional interface descriptions.
		/// </summary>
		public virtual string[] InterfaceDescriptions
		{
			get { return null; }
		}

		/// <summary>
		/// Optional array of supported content formats.
		/// </summary>
		public virtual int[] ContentFormats
		{
			get { return null; }
		}

		/// <summary>
		/// Optional maximum size estimate.
		/// </summary>
		public virtual int? MaximumSizeEstimate
		{
			get { return null; }
		}
	}
}
