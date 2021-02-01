using System;
using Waher.Networking.XMPP;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.IoTGateway.ScriptExtensions.Functions
{
	/// <summary>
	/// Returns the Full JID of a JID. If JID is a Bare JID, the Full JID of the last online presence is returned.
	/// </summary>
	public class FullJid : FunctionOneScalarVariable
	{
		/// <summary>
		/// Returns the Full JID of a JID. If JID is a Bare JID, the Full JID of the last online presence is returned.
		/// </summary>
		/// <param name="Jid">JID.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public FullJid(ScriptNode Jid, int Start, int Length, Expression Expression)
			: base(Jid, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "FullJID";

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "JID" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument, Variables Variables)
		{
			int i = Argument.IndexOf('@');
			if (i < 0)
				return new StringValue(Argument);

			i = Argument.IndexOf('/', i + 1);
			if (i > 0)
				return new StringValue(Argument);

			RosterItem Item = Gateway.XmppClient[Argument];
			if (Item is null)
				throw new ScriptRuntimeException("Not connected with " + Argument, this);

			if (!Item.HasLastPresence || !Item.LastPresence.IsOnline)
				throw new ScriptRuntimeException(Argument + " not online.", this);

			return new StringValue(Item.LastPresenceFullJid);
		}
	}
}
