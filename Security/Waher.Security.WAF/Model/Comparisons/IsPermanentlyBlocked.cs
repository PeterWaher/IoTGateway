using System;
using System.Threading.Tasks;
using System.Xml;
using Waher.Networking.HTTP.Interfaces;

namespace Waher.Security.WAF.Model.Comparisons
{
	/// <summary>
	/// Checks if the remote endpoint of the request is permanently blocked.
	/// </summary>
	public class IsPermanentlyBlocked : WafComparison
	{
		/// <summary>
		/// Checks if the remote endpoint of the request is permanently blocked.
		/// </summary>
		public IsPermanentlyBlocked()
			: base()
		{
		}

		/// <summary>
		/// Checks if the remote endpoint of the request is permanently blocked.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public IsPermanentlyBlocked(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(IsPermanentlyBlocked);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new IsPermanentlyBlocked(Xml, Parent, Document);

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Result to return, if any.</returns>
		public override async Task<WafResult?> Review(ProcessingState State)
		{
			DateTime? TP = await State.Firewall.LoginAuditor.GetEarliestLoginOpportunity(State.Request.RemoteEndPoint, "HTTP");

			if (TP.HasValue && TP.Value == DateTime.MaxValue)
				return await this.ReviewChildren(State);
			else
				return null;
		}
	}
}
