using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.HTTP;

namespace Waher.WebService.Script
{
	/// <summary>
	/// Internal script execution state.
	/// </summary>
	internal class State
	{
		private readonly HttpResponse response;
		private readonly string tag;
		private bool previewing;

		/// <summary>
		/// Internal script execution state.
		/// </summary>
		/// <param name="Response">HTTP Response object for request.</param>
		/// <param name="Tag">Client-side tag.</param>
		public State(HttpResponse Response, string Tag)
		{
			this.response = Response;
			this.tag = Tag;
			this.previewing = false;
		}

		/// <summary>
		/// HTTP Response object for request.
		/// </summary>
		public HttpResponse Response => this.response;

		/// <summary>
		/// Client-side tag.
		/// </summary>
		public string Tag => this.tag;

		/// <summary>
		/// If the script supports previewing results.
		/// </summary>
		public bool Previewing
		{
			get => this.previewing;
			set => this.previewing = value;
		}
	}
}
