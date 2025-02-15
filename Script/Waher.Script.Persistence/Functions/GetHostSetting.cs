using System.Threading.Tasks;
using Waher.Runtime.Settings;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Persistence.Functions
{
	/// <summary>
	/// Gets a runtime hgost setting with name `Name`. `Host` defines the host. 
	/// If a host setting is not found, the `DefaultValue` value is returned.
	/// </summary>
	public class GetHostSetting : FunctionMultiVariate
	{
		/// <summary>
		/// Gets a runtime hgost setting with name `Name`. `Host` defines the host. 
		/// If a host setting is not found, the `DefaultValue` value is returned.
		/// </summary>
		/// <param name="Host">Name of host.</param>
		/// <param name="Name">Name of runtime setting parameter.</param>
		/// <param name="DefaultValue">Default value, if setting not found.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public GetHostSetting(ScriptNode Host, ScriptNode Name, ScriptNode DefaultValue, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Host, Name, DefaultValue }, argumentTypes3Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(GetHostSetting);

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
			string Host = GetSetting.GetHost(Arguments[0].AssociatedObjectValue, this);

			if (string.IsNullOrEmpty(Host))
				throw new ScriptRuntimeException("Host not defined.", this);

			string Name = Arguments[1].AssociatedObjectValue?.ToString();
			object DefaultValue = Arguments[2].AssociatedObjectValue;
			object Result;
			Result = await HostSettings.GetAsync(Host, Name, DefaultValue);

			return Expression.Encapsulate(Result);
		}
	}
}
