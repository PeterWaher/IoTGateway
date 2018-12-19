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
		private Notifications notifications;
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
		/// <param name="Notifications">If the resource is observable, and how notifications are to be sent.</param>
		/// <param name="Title">Optional CoRE title. Can be null.</param>
		/// <param name="ResourceTypes">Optional set of CoRE resource types. Can be null or empty.</param>
		/// <param name="InterfaceDescriptions">Optional set of CoRE interface descriptions. Can be null or empty.</param>
		/// <param name="ContentFormats">Optional set of content format representations supported by the resource. Can be null or empty.</param>
		/// <param name="MaximumSizeEstimate">Optional maximum size estimate of resource. Can be null.</param>
		public CoapGetDelegateResource(string ResourceName, CoapMethodHandler GET,
			Notifications Notifications, string Title, string[] ResourceTypes,
			string[] InterfaceDescriptions, int[] ContentFormats, int? MaximumSizeEstimate)
			: base(ResourceName)
		{
			this.get = GET;
			this.notifications = Notifications;
			this.title = Title;
			this.resourceTypes = ResourceTypes;
			this.interfaceDescriptions = InterfaceDescriptions;
			this.contentFormats = ContentFormats;
			this.maximumSizeEstimate = MaximumSizeEstimate;
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
		/// If the resource is observable, and how notifications are to be sent.
		/// </summary>
		public override Notifications Notifications => this.notifications;

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
			if (this.get is null)
				throw new CoapException(CoapCode.MethodNotAllowed);
			else
				this.get(Request, Response);
		}

	}
}
