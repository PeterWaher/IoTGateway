using System.Threading.Tasks;
using Waher.Runtime.Settings;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Persistence.Functions;

namespace Waher.IoTGateway.ScriptExtensions.Functions
{
    /// <summary>
    /// Gets a domain setting. If the Host (which can be a string or implement <see cref="Waher.Content.IHostReference"/>, 
    /// such as an HTTP Request object for instance) is an alternative domain, it will be treated as a host setting, otherwise 
    /// a runtime setting.
    /// </summary>
    public class GetDomainSetting : FunctionMultiVariate
    {
        /// <summary>
        /// Gets a domain setting. If the Host (which can be a string or implement <see cref="Waher.Content.IHostReference"/>, 
        /// such as an HTTP Request object for instance) is an alternative domain, it will be treated as a host setting, otherwise 
        /// a runtime setting.
        /// returned.
        /// </summary>
        /// <param name="Host">Name of host.</param>
        /// <param name="Name">Name of runtime setting parameter.</param>
        /// <param name="DefaultValue">Default value, if setting not found.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        /// <param name="Expression">Expression containing script.</param>
        public GetDomainSetting(ScriptNode Host, ScriptNode Name, ScriptNode DefaultValue, int Start, int Length, Expression Expression)
            : base(new ScriptNode[] { Host, Name, DefaultValue }, argumentTypes3Scalar, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(GetDomainSetting);

        /// <summary>
        /// Default Argument names
        /// </summary>
        public override string[] DefaultArgumentNames => new string[] { "Host", "Name", "DefaultValue" };

        /// <summary>
        /// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
        /// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
        /// </summary>
        public override bool IsAsynchronous => true;

        /// <summary>
        /// Evaluates the function.
        /// </summary>
        /// <param name="Arguments">Function arguments.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement Evaluate(IElement[] Arguments, Variables Variables)
        {
            return this.EvaluateAsync(Arguments, Variables).Result;
        }

        /// <summary>
        /// Evaluates the function.
        /// </summary>
        /// <param name="Arguments">Function arguments.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override async Task<IElement> EvaluateAsync(IElement[] Arguments, Variables Variables)
        {
            string Host = IsAlternativeDomain(GetSetting.GetHost(Arguments[0].AssociatedObjectValue, this));
            string Name = Arguments[1].AssociatedObjectValue?.ToString();
            object DefaultValue = Arguments[2].AssociatedObjectValue;
            object Result;

            if (!string.IsNullOrEmpty(Host))
                Result = await HostSettings.GetAsync(Host, Name, DefaultValue);
            else
                Result = await RuntimeSettings.GetAsync(Name, DefaultValue);

            return Expression.Encapsulate(Result);
        }

        internal static string IsAlternativeDomain(string Host)
        {
            if (string.IsNullOrEmpty(Host))
                return null;

            int i = Host.LastIndexOf(':');
            if (i > 0)
                Host = Host.Substring(0, i);

            if (Gateway.IsDomain(Host, true) && !Gateway.IsDomain(Host, false))
                return Host;
            else
                return null;
        }
    }
}
