using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// CoAP resource defined by a GET delegate method.
	/// </summary>
	public class CoapGetDelegateResource : CoapResource, ICoapGetMethod
	{
		private CoapMethodHandler get;
		private bool handlesSubPaths;

		/// <summary>
		/// CoAP resource defined by a GET delegate method.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="GET">GET Method.</param>
		/// (i.e. sends the response from another thread).</param>
		/// <param name="HandlesSubPaths">If sub-paths are handled.</param>
		public CoapGetDelegateResource(string ResourceName, CoapMethodHandler GET, bool HandlesSubPaths)
			: base(ResourceName)
		{
			this.get = GET;
			this.handlesSubPaths = HandlesSubPaths;
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths
		{
			get { return this.handlesSubPaths; }
		}

		/// <summary>
		/// If the GET method is allowed.
		/// </summary>
		public bool AllowsGET
		{
			get
			{
				return this.get != null;
			}
		}

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		public void GET(CoapMessage Request, CoapResponse Response)
		{
			if (this.get == null)
				throw new CoapException(CoapCode.MethodNotAllowed);
			else
				this.get(Request, Response);
		}

	}
}
