using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml.Attributes;
using Waher.Networking.HTTP.Interfaces;
using Waher.Security.LoginMonitor;

namespace Waher.Security.WAF.Model.Actions
{
	/// <summary>
	/// Abstract base class for audit actions.
	/// </summary>
	public abstract class WafAuditAction : WafActionWithTags
	{
		/// <summary>
		/// Message attribute
		/// </summary>
		protected readonly StringAttribute message;

		/// <summary>
		/// User Name attribute
		/// </summary>
		protected readonly StringAttribute userName;

		/// <summary>
		/// Protocol attribute
		/// </summary>
		protected readonly StringAttribute protocol;

		/// <summary>
		/// Abstract base class for audit actions.
		/// </summary>
		public WafAuditAction()
			: base()
		{
		}

		/// <summary>
		/// Abstract base class for audit actions.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public WafAuditAction(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
			this.message = new StringAttribute(Xml, "message");
			this.protocol = new StringAttribute(Xml, "protocol");
			this.userName = new StringAttribute(Xml, "userName");
		}

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Result to return, if any.</returns>
		public override async Task<WafResult?> Review(ProcessingState State)
		{
			string Message = await this.message.EvaluateAsync(State.Variables, string.Empty);
			string Protocol = await this.protocol.EvaluateAsync(State.Variables, "HTTP");
			string UserName = await this.userName.EvaluateAsync(State.Variables, string.Empty);
			KeyValuePair<string, object>[] Tags = await this.EvaluateTags(State);

			LoginAuditor.Fail(Message, UserName, State.Request.RemoteEndPoint, Protocol, Tags);

			return null;
		}
	}
}
