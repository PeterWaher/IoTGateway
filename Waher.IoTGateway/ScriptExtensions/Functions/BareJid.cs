using System;
using Waher.Networking.XMPP;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.IoTGateway.ScriptExtensions.Functions
{
    /// <summary>
    /// Returns the Bare JID of a JID.
    /// </summary>
    public class BareJid : FunctionOneScalarVariable
	{
		/// <summary>
		/// Returns the Bare JID of a JID.
		/// </summary>
		/// <param name="Jid">JID.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public BareJid(ScriptNode Jid, int Start, int Length, Expression Expression)
			: base(Jid, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "BareJID";

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
			return new StringValue(XmppClient.GetBareJID(Argument));
		}
	}
}
