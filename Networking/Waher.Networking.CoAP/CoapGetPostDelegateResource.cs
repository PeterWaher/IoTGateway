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
        /// (i.e. sends the response from another thread).</param>
        /// <param name="HandlesSubPaths">If sub-paths are handled.</param>
        public CoapGetPostDelegateResource(string ResourceName, CoapMethodHandler GET, CoapMethodHandler POST, bool HandlesSubPaths)
			: base(ResourceName, GET, HandlesSubPaths)
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
