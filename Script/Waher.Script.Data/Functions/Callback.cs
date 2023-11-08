using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Data.Functions;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.IoTGateway.ScriptExtensions.Functions
{
	/// <summary>
	/// Generates a callback function based on script.
	/// </summary>
	public class Callback : FunctionTwoScalarVariables
	{
		/// <summary>
		/// Generates a callback function based on script.
		/// </summary>
		/// <param name="DelegateType">Delegate type.</param>
		/// <param name="Lambda">Lambda expression that will be used in call-backs.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Callback(ScriptNode DelegateType, ScriptNode Lambda, int Start, int Length, Expression Expression)
			: base(DelegateType, Lambda, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Callback);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "DelegateType", "Lambda" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument1, IElement Argument2, Variables Variables)
		{
			if (!(Argument1.AssociatedObjectValue is Type Type) || !delegateTypeInfo.IsAssignableFrom(Type.GetTypeInfo()))
				throw new ScriptRuntimeException("Expected a delegate type in the first argument.", this);

			if (!(Argument2.AssociatedObjectValue is ILambdaExpression Lambda))
				throw new ScriptRuntimeException("Expected a lambda expression in the second argument.", this);

			Type ScriptProxyType;

			lock (scriptProxyTypes)
			{
				if (!scriptProxyTypes.TryGetValue(Type, out ScriptProxyType))
				{
					Type ReturnType = null;
					ParameterInfo[] Parameters = null;

					foreach (MethodInfo MI in Type.GetRuntimeMethods())
					{
						if (MI.Name == "Invoke")
						{
							ReturnType = MI.ReturnType;
							Parameters = MI.GetParameters();
							break;
						}
					}

					if (ReturnType is null || Parameters is null)
						throw new ScriptRuntimeException("Delegate type lacks an Invoke method.", this);

					bool IsAsync = taskTypeInfo.IsAssignableFrom(ReturnType.GetTypeInfo());
					string TypeName = Type.Name.Replace("`", "_GT_");
					StringBuilder CSharp = new StringBuilder();

					CSharp.AppendLine("using System;");
					CSharp.AppendLine("using Waher.Script;");
					CSharp.AppendLine("using Waher.Script.Data.Functions;");
					CSharp.AppendLine("using Waher.Script.Model;");
					CSharp.AppendLine();
					CSharp.Append("namespace ");
					CSharp.Append(Type.Namespace);
					CSharp.AppendLine(".ScriptCallbacks");
					CSharp.AppendLine("{");
					CSharp.Append("\tpublic class ScriptProxy");
					CSharp.Append(TypeName);
					CSharp.Append(" : ScriptProxy<");
					CSharp.Append(Type.FullName);
					CSharp.AppendLine(">");
					CSharp.AppendLine("\t{");
					CSharp.Append("\t\tpublic ScriptProxy");
					CSharp.Append(TypeName);
					CSharp.AppendLine("(ILambdaExpression Lambda, Variables Variables)");
					CSharp.AppendLine("\t\t\t: base(Lambda, Variables)");
					CSharp.AppendLine("\t\t{");
					CSharp.AppendLine("\t\t}");
					CSharp.AppendLine();
					CSharp.Append("\t\tpublic override ");
					CSharp.Append(Type.FullName);
					CSharp.AppendLine(" GetCallbackFunction()");
					CSharp.AppendLine("\t\t{");
					CSharp.AppendLine("\t\t\treturn this.CallLambda;");
					CSharp.AppendLine("\t\t}");
					CSharp.AppendLine();
					CSharp.Append("\t\tprivate ");

					if (ReturnType == typeof(void))
						CSharp.Append("void");
					else
					{
						if (IsAsync)
							CSharp.Append("async Task<");

						AppendType(ReturnType, CSharp);

						if (IsAsync)
							CSharp.Append('>');
					}

					CSharp.Append(" CallLambda(");

					bool First = true;

					foreach (ParameterInfo Parameter in Parameters)
					{
						if (First)
							First = false;
						else
							CSharp.Append(", ");

						AppendType(Parameter.ParameterType, CSharp);
						CSharp.Append(' ');
						CSharp.Append(Parameter.Name);
					}

					CSharp.AppendLine(")");
					CSharp.AppendLine("\t\t{");

					if (IsAsync)
						CSharp.Append("\t\t\tIElement Result = await this.Lambda.EvaluateAsync(");
					else
						CSharp.Append("\t\t\tIElement Result = this.Lambda.Evaluate(");

					if (Parameters.Length == 0)
						CSharp.Append("new IElement[0]");
					else
					{
						CSharp.AppendLine("new IElement[]");
						CSharp.AppendLine("\t\t\t{");

						First = true;

						foreach (ParameterInfo Parameter in Parameters)
						{
							if (First)
								First = false;
							else
								CSharp.AppendLine(",");

							CSharp.Append("\t\t\t\tExpression.Encapsulate(");
							CSharp.Append(Parameter.Name);
							CSharp.Append(')');
						}

						CSharp.AppendLine();
						CSharp.Append("\t\t\t}");
					}

					CSharp.AppendLine(", this.Variables);");
					CSharp.AppendLine();
					if (ReturnType == typeof(IElement))
						CSharp.AppendLine("\t\t\treturn Result;");
					else
					{
						CSharp.Append("\t\t\treturn (");
						AppendType(ReturnType, CSharp);
						CSharp.AppendLine(")Result.AssociatedObjectValue;");
					}

					CSharp.AppendLine("\t\t}");
					CSharp.AppendLine("\t}");
					CSharp.AppendLine("}");

					string CSharpCode = CSharp.ToString();

					Dictionary<string, bool> Dependencies = new Dictionary<string, bool>()
					{
						{ GetLocation(typeof(object)), true },
						{ Path.Combine(Path.GetDirectoryName(GetLocation(typeof(object))), "System.Runtime.dll"), true },
						{ Path.Combine(Path.GetDirectoryName(GetLocation(typeof(Encoding))), "System.Text.Encoding.dll"), true },
						{ Path.Combine(Path.GetDirectoryName(GetLocation(typeof(MemoryStream))), "System.IO.dll"), true },
						{ Path.Combine(Path.GetDirectoryName(GetLocation(typeof(MemoryStream))), "System.Runtime.Extensions.dll"), true },
						{ Path.Combine(Path.GetDirectoryName(GetLocation(typeof(Task))), "System.Threading.Tasks.dll"), true },
						{ Path.Combine(Path.GetDirectoryName(GetLocation(typeof(Dictionary<string, object>))), "System.Collections.dll"), true },
						{ GetLocation(typeof(Types)), true },
						{ GetLocation(typeof(Expression)), true },
						{ GetLocation(typeof(Callback)), true }
					};

					Dependencies[GetLocation(Type)] = true;

					TypeInfo LoopInfo;
					Type Loop = Type;
					PropertyInfo PI;
					FieldInfo FI;
					string s = Path.Combine(Path.GetDirectoryName(GetLocation(typeof(object))), "netstandard.dll");

					if (File.Exists(s))
						Dependencies[s] = true;

					while (!(Loop is null))
					{
						LoopInfo = Loop.GetTypeInfo();
						Dependencies[GetLocation(Loop)] = true;

						foreach (Type Interface in LoopInfo.ImplementedInterfaces)
						{
							s = GetLocation(Interface);
							Dependencies[s] = true;
						}

						foreach (MemberInfo MI2 in LoopInfo.DeclaredMembers)
						{
							FI = MI2 as FieldInfo;
							if (!(FI is null) && !((s = GetLocation(FI.FieldType)).EndsWith("mscorlib.dll") || s.EndsWith("System.Runtime.dll") || s.EndsWith("System.Private.CoreLib.dll")))
								Dependencies[s] = true;
							PI = MI2 as PropertyInfo;
							if (!(PI is null) && !((s = GetLocation(PI.PropertyType)).EndsWith("mscorlib.dll") || s.EndsWith("System.Runtime.dll") || s.EndsWith("System.Private.CoreLib.dll")))
								Dependencies[s] = true;
						}
						Loop = LoopInfo.BaseType;
						if (Loop == typeof(object))
							break;
					}

					List<Microsoft.CodeAnalysis.MetadataReference> References = new List<Microsoft.CodeAnalysis.MetadataReference>();

					foreach (string Location in Dependencies.Keys)
					{
						if (!string.IsNullOrEmpty(Location))
							References.Add(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(Location));
					}

					StringBuilder sb = new StringBuilder();

					sb.Append("WSDA.");
					AppendType(Type, sb);

					Microsoft.CodeAnalysis.CSharp.CSharpCompilation Compilation =
						Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create(sb.ToString(),
						new Microsoft.CodeAnalysis.SyntaxTree[] { Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(CSharpCode) },
						References, new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(
							Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary));

					MemoryStream Output = new MemoryStream();
					MemoryStream PdbOutput = new MemoryStream();

					EmitResult CompilerResults = Compilation.Emit(Output, pdbStream: PdbOutput);

					if (!CompilerResults.Success)
					{
						sb.Clear();

						sb.Append("Unable to create a script proxy for callback methods of type ");
						AppendType(Type, sb);
						sb.AppendLine(". When generating proxy class, the following compiler errors were reported:");

						foreach (Microsoft.CodeAnalysis.Diagnostic Error in CompilerResults.Diagnostics)
						{
							sb.AppendLine();
							sb.Append(Error.Location.ToString());
							sb.Append(": ");
							sb.Append(Error.GetMessage());
						}

						sb.AppendLine();
						sb.AppendLine();
						sb.AppendLine("Code generated:");
						sb.AppendLine();
						sb.AppendLine(CSharpCode);

						throw new ScriptRuntimeException(sb.ToString(), this);
					}

					Output.Position = 0;
					PdbOutput.Position = 0;
					Assembly A;

					A = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(Output, PdbOutput);

					sb.Clear();
					sb.Append(Type.Namespace);
					sb.Append(".ScriptCallbacks.ScriptProxy");
					sb.Append(TypeName);

					ScriptProxyType = A.GetType(sb.ToString());
					scriptProxyTypes[Type] = ScriptProxyType;
				}
			}

			IScriptProxy Proxy = (IScriptProxy)Activator.CreateInstance(ScriptProxyType, Lambda, Variables);

			return new ObjectValue(Proxy.GetCallbackFunctionUntyped());
		}

		private static readonly TypeInfo delegateTypeInfo = typeof(Delegate).GetTypeInfo();
		private static readonly TypeInfo taskTypeInfo = typeof(Task).GetTypeInfo();
		private static readonly Dictionary<Type, Type> scriptProxyTypes = new Dictionary<Type, Type>();

		private static string GetLocation(Type T)
		{
			TypeInfo TI = T.GetTypeInfo();
			string s = TI.Assembly.Location;

			if (!string.IsNullOrEmpty(s))
				return s;

			return Path.Combine(Path.GetDirectoryName(GetLocation(typeof(Expression))), TI.Module.ScopeName);
		}

		private static void AppendType(Type T, StringBuilder sb)
		{
			if (T.IsConstructedGenericType)
			{
				Type T2 = T.GetGenericTypeDefinition();
				string s = T2.FullName;
				int i = s.IndexOf('`');

				if (i > 0)
					s = s.Substring(0, i);

				sb.Append(s);
				sb.Append('<');

				bool First = true;

				foreach (Type Arg in T.GenericTypeArguments)
				{
					if (First)
						First = false;
					else
						sb.Append(',');

					AppendType(Arg, sb);
				}

				sb.Append('>');
			}
			else if (T.HasElementType)
			{
				if (T.IsArray)
				{
					AppendType(T.GetElementType(), sb);
					sb.Append("[]");
				}
				else if (T.IsPointer)
				{
					AppendType(T.GetElementType(), sb);
					sb.Append('*');
				}
				else
					sb.Append(T.FullName);
			}
			else
				sb.Append(T.FullName);
		}

	}
}
