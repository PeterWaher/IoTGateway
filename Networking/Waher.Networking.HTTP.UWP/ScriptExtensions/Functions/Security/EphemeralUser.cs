using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Security;

namespace Waher.Networking.HTTP.ScriptExtensions.Functions.Security
{
    /// <summary>
    /// Creates an ephemeral user that is not persisted.
    /// </summary>
    public class EphemeralUser : FunctionMultiVariate
    {
        /// <summary>
        /// Creates an ephemeral user that is not persisted.
        /// </summary>
        /// <param name="UserName">User Name of user object.</param>
        /// <param name="Jid">JID of user.</param>
        /// <param name="Privileges">A vector of privileges</param>
        /// <param name="Properties">Additional properties of user object.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        /// <param name="Expression">Expression.</param>
        public EphemeralUser(ScriptNode UserName, ScriptNode Jid, ScriptNode Privileges, ScriptNode Properties,
            int Start, int Length, Expression Expression)
            : base(new ScriptNode[] { UserName, Jid, Privileges, Properties },
                  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Vector, ArgumentType.Normal },
                  Start, Length, Expression)
        {
        }

        /// <summary>
        /// Creates an ephemeral user that is not persisted.
        /// </summary>
        /// <param name="UserName">User Name of user object.</param>
        /// <param name="Jid">JID of user.</param>
        /// <param name="Privileges">A vector of privileges</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        /// <param name="Expression">Expression.</param>
        public EphemeralUser(ScriptNode UserName, ScriptNode Jid, ScriptNode Privileges, int Start, int Length, Expression Expression)
            : base(new ScriptNode[] { UserName, Jid, Privileges },
                  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Vector },
                  Start, Length, Expression)
        {
        }

        /// <summary>
        /// Creates an ephemeral user that is not persisted.
        /// </summary>
        /// <param name="UserName">User Name of user object.</param>
        /// <param name="Jid">JID of user.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        /// <param name="Expression">Expression.</param>
        public EphemeralUser(ScriptNode UserName, ScriptNode Jid, int Start, int Length, Expression Expression)
            : base(new ScriptNode[] { UserName, Jid }, argumentTypes2Scalar, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Creates an ephemeral user that is not persisted.
        /// </summary>
        /// <param name="UserName">User Name of user object.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        /// <param name="Expression">Expression.</param>
        public EphemeralUser(ScriptNode UserName, int Start, int Length, Expression Expression)
            : base(new ScriptNode[] { UserName }, argumentTypes1Scalar, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(EphemeralUser);

        /// <summary>
        /// Default Argument names
        /// </summary>
        public override string[] DefaultArgumentNames => new string[] { "UserName", "JID", "Privileges", "Properties" };

        /// <summary>
        /// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
        /// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
        /// </summary>
        public override bool IsAsynchronous => false;


        /// <summary>
        /// Evaluates the function.
        /// </summary>
        /// <param name="Arguments">Function arguments.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement Evaluate(IElement[] Arguments, Variables Variables)
        {
            int i = 0;
            int c = Arguments.Length;

            string UserName = Arguments[i++].AssociatedObjectValue?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(UserName))
                throw new ScriptRuntimeException("Missing user name.", this);

            string Jid;

            if (i < c)
                Jid = Arguments[i++].AssociatedObjectValue?.ToString() ?? string.Empty;
            else
                Jid = string.Empty;

            LinkedList<string> Privileges = new LinkedList<string>();

            if (i < c)
            {
                if (!(Arguments[i++] is IVector V))
                    throw new ScriptRuntimeException("Expected an array of privileges as third argument.", this);

                foreach (IElement E in V.VectorElements)
                    Privileges.AddLast(E.AssociatedObjectValue?.ToString() ?? string.Empty);
            }

            IDictionary<string, object> Properties;

            if (i < c)
            {
                object Obj = Arguments[i++];

                if (Obj is Dictionary<string, IElement> Object)
                {
                    Properties = new Dictionary<string, object>();

                    foreach (KeyValuePair<string, IElement> P in Object)
                        Properties[P.Key] = P.Value.AssociatedObjectValue;
                }
                else if (Obj is IDictionary<string, object> Object2)
                    Properties = Object2;
                else
                    throw new ScriptRuntimeException("Expected an object ex-nihilo as fourth argument.", this);
            }
            else
                Properties = null;

            return new ObjectValue(new EphemeralUserObject(UserName, Jid, Privileges, Properties));
        }

        private class EphemeralUserObject : IUser
        {
            private readonly KeyValuePair<Regex, bool>[] privileges;
            private readonly IDictionary<string, object> properties;

            public EphemeralUserObject(string UserName, string Jid, IEnumerable<string> Privileges, IDictionary<string, object> Properties)
            {
                this.UserName = UserName;
                this.Jid = Jid ?? string.Empty;
                this.properties = Properties;

                if (Privileges is null)
                    this.privileges = Array.Empty<KeyValuePair<Regex, bool>>();
                else
                {
                    List<KeyValuePair<Regex, bool>> ParsedPrivileges = new List<KeyValuePair<Regex, bool>>();

                    foreach (string Privilege in Privileges)
                    {
                        if (string.IsNullOrEmpty(Privilege))
                            continue;

                        bool Include;
                        Regex Parsed;

                        switch (Privilege[0])
                        {
                            case '+':
                                Include = true;
                                Parsed = new Regex(Privilege.Substring(1), RegexOptions.Singleline);
                                break;

                            case '-':
                                Include = false;
                                Parsed = new Regex(Privilege.Substring(1), RegexOptions.Singleline);
                                break;

                            default:
                                Include = true;
                                Parsed = new Regex(Privilege, RegexOptions.Singleline);
                                break;
                        }

                        ParsedPrivileges.Add(new KeyValuePair<Regex, bool>(Parsed, Include));
                    }

                    this.privileges = ParsedPrivileges.ToArray();
                }
            }

            public string PasswordHash => string.Empty;
            public string PasswordHashType => string.Empty;
            public string UserName { get; }
            public string Jid { get; }

            public bool HasPrivilege(string Privilege)
            {
                int c = Privilege.Length;

                foreach (KeyValuePair<Regex, bool> P in this.privileges)
                {
                    Match M = P.Key.Match(Privilege);
                    if (M.Success && M.Index == 0 && M.Length == c)
                        return P.Value;
                }

                return false;
            }
        }

    }
}
