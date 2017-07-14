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
		private bool observable;
		private string title;
		private string[] resourceTypes;
		private string[] interfaceDescriptions;
		private int[] contentFormats;
		private int? maximumSizeEstimate;

		/// <summary>
		/// CoAP resource defined by a GET delegate method.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="GET">GET Method.</param>
		/// <param name="HandlesSubPaths">If sub-paths are handled.</param>
		/// <param name="Observable">If the resource is observable.</param>
		/// <param name="Title">Optional CoRE title. Can be null.</param>
		/// <param name="ResourceTypes">Optional set of CoRE resource types. Can be null or empty.</param>
		/// <param name="InterfaceDescriptions">Optional set of CoRE interface descriptions. Can be null or empty.</param>
		/// <param name="ContentFormats">Optional set of content format representations supported by the resource. Can be null or empty.</param>
		/// <param name="MaximumSizeEstimate">Optional maximum size estimate of resource. Can be null.</param>
		public CoapGetDelegateResource(string ResourceName, CoapMethodHandler GET,
			bool HandlesSubPaths, bool Observable, string Title, string[] ResourceTypes,
			string[] InterfaceDescriptions, int[] ContentFormats, int? MaximumSizeEstimate)
			: base(ResourceName)
		{
			this.get = GET;
			this.handlesSubPaths = HandlesSubPaths;
			this.observable = Observable;
			this.title = Title;
			this.resourceTypes = ResourceTypes;
			this.interfaceDescriptions = InterfaceDescriptions;
			this.contentFormats = ContentFormats;
			this.maximumSizeEstimate = MaximumSizeEstimate;
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths
		{
			get { return this.handlesSubPaths; }
		}

		/// <summary>
		/// Optional title of resource.
		/// </summary>
		public override string Title => this.title;

		/// <summary>
		/// Optional resource type.
		/// </summary>
		public override string[] ResourceTypes => this.resourceTypes;
		/// <summary>
		/// Optional interface descriptions.
		/// </summary>
		public override string[] InterfaceDescriptions => this.interfaceDescriptions;

		/// <summary>
		/// If the resource is observable.
		/// </summary>
		public override bool Observable => this.observable;

		/// <summary>
		/// Optional maximum size estimate.
		/// </summary>
		public override int? MaximumSizeEstimate => this.maximumSizeEstimate;

		/// <summary>
		/// Optional array of supported content formats.
		/// </summary>
		public override int[] ContentFormats => this.contentFormats;

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
