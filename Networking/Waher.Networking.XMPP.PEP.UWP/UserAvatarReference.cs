using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Security;

namespace Waher.Networking.XMPP.PEP
{
	/// <summary>
	/// User Avatar Reference
	/// </summary>
	public class UserAvatarReference
	{
		/// <summary>
		/// User Avatar Reference
		/// </summary>
		public UserAvatarReference()
		{
		}

		/// <summary>
		/// Item ID of avatar
		/// </summary>
		public string Id
		{
			get;
			set;
		}

		/// <summary>
		/// Internet Content Type
		/// </summary>
		public string Type
		{
			get;
			set;
		}

		/// <summary>
		/// Optional URL for out-of-band retrieval of Avatar
		/// </summary>
		public Uri URL
		{
			get;
			set;
		}

		/// <summary>
		/// Number of bytes
		/// </summary>
		public int Bytes
		{
			get;
			set;
		}

		/// <summary>
		/// Width, in pixels
		/// </summary>
		public int Width
		{
			get;
			set;
		}

		/// <summary>
		/// Height, in pixels
		/// </summary>
		public int Height
		{
			get;
			set;
		}

	}
}
