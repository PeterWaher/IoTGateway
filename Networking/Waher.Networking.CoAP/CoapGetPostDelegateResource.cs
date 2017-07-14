using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// CoAP resource defined by GET and POST delegate methods.
	/// </summary>
	public class CoapGetPostDelegateResource : CoapGetDelegateResource, ICoapPostMethod
	{
		private CoapMethodHandler post;

		/// <summary>
		/// CoAP resource defined by GET and POST delegate methods.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="GET">GET Method.</param>
		/// <param name="POST">POST Method.</param>
		/// <param name="HandlesSubPaths">If sub-paths are handled.</param>
		/// <param name="Observable">If the resource is observable.</param>
		/// <param name="Title">Optional CoRE title. Can be null.</param>
		/// <param name="ResourceTypes">Optional set of CoRE resource types. Can be null or empty.</param>
		/// <param name="InterfaceDescriptions">Optional set of CoRE interface descriptions. Can be null or empty.</param>
		/// <param name="ContentFormats">Optional set of content format representations supported by the resource. Can be null or empty.</param>
		/// <param name="MaximumSizeEstimate">Optional maximum size estimate of resource. Can be null.</param>
		public CoapGetPostDelegateResource(string ResourceName, CoapMethodHandler GET, 
			CoapMethodHandler POST, bool HandlesSubPaths, bool Observable, string Title, string[] ResourceTypes,
			string[] InterfaceDescriptions, int[] ContentFormats, int? MaximumSizeEstimate)
			: base(ResourceName, GET, HandlesSubPaths, Observable, Title, ResourceTypes, InterfaceDescriptions,
				  ContentFormats, MaximumSizeEstimate)
		{
			this.post = POST;
		}

		/// <summary>
		/// If the POST method is allowed.
		/// </summary>
		public bool AllowsPOST
		{
			get
			{
				return this.post != null;
			}
		}

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		public void POST(CoapMessage Request, CoapResponse Response)
		{
			if (this.post == null)
				throw new CoapException(CoapCode.MethodNotAllowed);
			else
				this.post(Request, Response);
		}
	}
}
