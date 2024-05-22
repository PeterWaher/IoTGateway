using System.Threading.Tasks;
using Waher.Runtime.Settings;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Persistence.Functions;

namespace Waher.IoTGateway.ScriptExtensions.Functions
{
    /// <summary>
    /// Sets a domain setting. If the Host (which can be a string or implement <see cref="Waher.Content.IHostReference"/>, 
    /// such as an HTTP Request object for instance) is an alternative domain, it will be treated as a host setting, otherwise 
    /// a runtime setting.
    /// </summary>
    public class SetDomainSetting : FunctionMultiVariate
    {
        /// <summary>
        /// Sets a domain setting. If the Host (which can be a string or implement <see cref="Waher.Content.IHostReference"/>, 
        /// such as an HTTP Request object for instance) is an alternative domain, it will be treated as a host setting, otherwise 
        /// a runtime setting.
        /// </summary>
        /// <param name="Host">Name of host.</param>
        /// <param name="Name">Name of runtime setting parameter.</param>
        /// <param name="Value">Default value, if setting not found.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        /// <param name="Expression">Expression containing script.</param>
        public SetDomainSetting(ScriptNode Host, ScriptNode Name, ScriptNode Value, int Start, int Length, Expression Expression)
            : base(new ScriptNode[] { Host, Name, Value }, argumentTypes3Scalar, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(SetDomainSetting);

        /// <summary>
        /// Default Argument names
        /// </summary>
        public override string[] DefaultArgumentNames => new string[] { "Host", "Name", "Value" };

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
            string Host = GetSetting.GetHost(Arguments[0].AssociatedObjectValue, this);
            string Name = Arguments[1].AssociatedObjectValue?.ToString();
            object Value = Arguments[2].AssociatedObjectValue;
            bool Result;

            if (GetDomainSetting.IsAlternativeDomain(Host))
                Result = await HostSettings.SetAsync(Host, Name, Value);
            else
                Result = await RuntimeSettings.SetAsync(Name, Value);

            return new BooleanValue(Result);
        }
    }
}