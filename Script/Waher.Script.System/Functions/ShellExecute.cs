using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.System.Functions
{
	/// <summary>
	/// ShellExecute(FileName,Arguments,WorkFolder)
	/// </summary>
	public class ShellExecute : FunctionMultiVariate
    {
		/// <summary>
		/// ShellExecute(FileName,Arguments,WorkFolder)
		/// </summary>
		/// <param name="FileName">File name of executable file.</param>
		/// <param name="Arguments">Command-line arguments.</param>
		/// <param name="WorkFolder">Working folder.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ShellExecute(ScriptNode FileName, ScriptNode Arguments, ScriptNode WorkFolder, 
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { FileName, Arguments, WorkFolder },
				  argumentTypes3Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// ShellExecute(FileName,Arguments,WorkFolder,TimeoutMs)
		/// </summary>
		/// <param name="FileName">File name of executable file.</param>
		/// <param name="Arguments">Command-line arguments.</param>
		/// <param name="WorkFolder">Working folder.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (0=infinite or no timeout).</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ShellExecute(ScriptNode FileName, ScriptNode Arguments, ScriptNode WorkFolder,
			ScriptNode TimeoutMs, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { FileName, Arguments, WorkFolder, TimeoutMs }, 
				  argumentTypes4Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(ShellExecute);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] 
		{
			"FileName", "Arguments", "WorkFolder", "TimeoutMs" 
		};

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
			if (!(Arguments[0].AssociatedObjectValue is string FileName) ||
				!(Arguments[1].AssociatedObjectValue is string Arg) ||
				!(Arguments[2].AssociatedObjectValue is string WorkFolder))
			{
				throw new ScriptRuntimeException("Expected string arguments.", this);
			}

			int TimeoutMs;

			if (Arguments.Length > 3)
			{
				TimeoutMs = (int)Expression.ToDouble(Arguments[3].AssociatedObjectValue);
				if (TimeoutMs < 0)
					throw new ScriptRuntimeException("Timeout must be non-negative.", this);
			}
			else
				TimeoutMs = 1000 * 60 * 5;

			ProcessStartInfo ProcessInformation = new ProcessStartInfo()
			{
				FileName = FileName,
				Arguments = Arg,
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				WorkingDirectory = WorkFolder,
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				ErrorDialog = false
			};

			Process P = new Process();
			TaskCompletionSource<IElement> ResultSource = new TaskCompletionSource<IElement>();

			P.ErrorDataReceived += (Sender, e) =>
			{
				ResultSource.TrySetException(new ScriptRuntimeException(e.Data, this));
			};

			P.Exited += async (Sender, e) =>
			{
				try
				{
					if (P.ExitCode != 0)
					{
						string ErrorText = await P.StandardError.ReadToEndAsync();
						ResultSource.TrySetException(new ScriptRuntimeException(ErrorText, this));
					}
					else
					{
						string s = await P.StandardOutput.ReadToEndAsync();
						ResultSource.TrySetResult(new StringValue(s));
					}
				}
				catch (Exception ex)
				{
					ResultSource.TrySetException(ex);
				}
			};

			if (TimeoutMs > 0)
				_ = Task.Delay(TimeoutMs).ContinueWith(Prev => ResultSource.TrySetException(new TimeoutException("Process did not exit within the provided time.")));

			using (CancellationTokenRegistration Registration = Variables.CancellationToken.Register(
				() => ResultSource.TrySetException(new OperationCanceledException("Evaluation cancelled."))))
			{
				P.StartInfo = ProcessInformation;
				P.EnableRaisingEvents = true;
				P.Start();

				return await ResultSource.Task;
			}
		}
	}
}
