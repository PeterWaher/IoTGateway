using System.Threading.Tasks;
using Waher.Content;
using Waher.Runtime.Settings;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Security;
using Waher.Things;

namespace Waher.Script.Persistence.Functions
{
	/// <summary>
	/// Gets a runtime setting with name `Name`. If `Host` is provided, the function first tries to get the corresponding runtime 
	/// host setting, and if one is not found, the corresponding runtime setting. If one is not found, the `DefaultValue` value is 
	/// returned.
	/// </summary>
	public class GetSetting : FunctionMultiVariate
	{
		/// <summary>
		/// Gets a runtime setting with name `Name`. If `Host` is provided, the function first tries to get the corresponding runtime 
		/// host setting, and if one is not found, the corresponding runtime setting. If one is not found, the `DefaultValue` value is 
		/// returned.
		/// </summary>
		/// <param name="Name">Name of runtime setting parameter.</param>
		/// <param name="DefaultValue">Default value, if setting not found.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public GetSetting(ScriptNode Name, ScriptNode DefaultValue, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Name, DefaultValue }, argumentTypes2Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Gets a runtime setting with name `Name`. If `Host` is provided, the function first tries to get the corresponding runtime 
		/// host setting, and if one is not found, the corresponding runtime setting. If one is not found, the `DefaultValue` value is 
		/// returned.
		/// </summary>
		/// <param name="Host">Name of host.</param>
		/// <param name="Name">Name of runtime setting parameter.</param>
		/// <param name="DefaultValue">Default value, if setting not found.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public GetSetting(ScriptNode Host, ScriptNode Name, ScriptNode DefaultValue, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Host, Name, DefaultValue }, argumentTypes3Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(GetSetting);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Name", "DefaultValue" };

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
			int i = 0;
			int c = Arguments.Length;
			string Host = c < 3 ? null : GetHost(Arguments[i++].AssociatedObjectValue, this);
			string Name = Arguments[i++].AssociatedObjectValue?.ToString();
			object DefaultValue = Arguments[i].AssociatedObjectValue;
			object Result;

			if (string.IsNullOrEmpty(Host))
				Result = await RuntimeSettings.GetAsync(Name, DefaultValue);
			else
				Result = await HostSettings.GetAsync(Host, Name, DefaultValue);

			return Expression.Encapsulate(Result);
		}

		/// <summary>
		/// Gets the host name from a value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <param name="Node">Script node executing code.</param>
		/// <returns>Host name.</returns>
		public static string GetHost(object Value, ScriptNode Node)
		{
			if (Value is IHostReference Ref)
				return Ref.Host;
			else if (Value is string s)
				return s;
			else if (Value is null)
				return null;
			else
				throw new ScriptRuntimeException("Expected a host reference, or null.", Node);
		}

		/// <summary>
		/// Gets the host name from a value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <param name="Node">Script node executing code.</param>
		/// <returns>Host name.</returns>
		public static async Task<string> GetUser(object Value, ScriptNode Node)
		{
			if (Value is IUser Ref)
				return Ref.UserName;
			else if (Value is IRequestOrigin Ref2)
			{
				RequestOrigin Origin = await Ref2.GetOrigin();
				return Origin.From;
			}
			else if (Value is string s)
				return s;
			else if (Value is null)
				return null;
			else
				throw new ScriptRuntimeException("Expected a user reference, or null.", Node);
		}
	}
}
