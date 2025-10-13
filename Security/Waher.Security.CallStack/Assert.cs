using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Waher.Events;

namespace Waher.Security.CallStack
{
	/// <summary>
	/// Static class containing methods that can be used to make sure calls are made from appropriate locations.
	/// </summary>
	public static class Assert
	{
		/// <summary>
		/// Makes sure the call is made from one of the listed assemblies.
		/// </summary>
		/// <param name="Assemblies">Original call must be made from one of these assemblies.</param>
		[Obsolete("Use CallFromSource(ICallStackCheck[]) instead.")]
		public static void CallFromAssembly(params Assembly[] Assemblies)
		{
			int i, c = Assemblies.Length;
			ICallStackCheck[] Sources = new ICallStackCheck[c];

			for (i = 0; i < c; i++)
				Sources[i] = new ApproveAssembly(Assemblies[i]);

			CallFromSource(Sources);
		}

		/// <summary>
		/// Makes sure the call is made from one of the listed classes.
		/// </summary>
		/// <param name="Classes">Original call must be made from one of these classes.</param>
		[Obsolete("Use CallFromSource(ICallStackCheck[]) instead.")]
		public static void CallFromClass(params Type[] Classes)
		{
			int i, c = Classes.Length;
			ICallStackCheck[] Sources = new ICallStackCheck[c];

			for (i = 0; i < c; i++)
				Sources[i] = new ApproveType(Classes[i]);

			CallFromSource(Sources);
		}

		/// <summary>
		/// Makes sure the call is made from one of the listed sources.
		/// </summary>
		/// <param name="Sources">Original call must be made from one of these sources. Source strings are checked against
		/// Assemblies, classes and method names.</param>
		[Obsolete("Use CallFromSource(ICallStackCheck[]) instead.")]
		public static void CallFromSource(params string[] Sources)
		{
			int i, c = Sources.Length;
			ICallStackCheck[] Sources2 = new ICallStackCheck[c];

			for (i = 0; i < c; i++)
				Sources2[i] = new ApproveString(Sources[i]);

			CallFromSource(Sources2);
		}

		/// <summary>
		/// Makes sure the call is made from one of the listed sources.
		/// </summary>
		/// <param name="Sources">Original call must be made from one of these sources. Source strings are checked against
		/// Assemblies, classes and method names.</param>
		[Obsolete("Use CallFromSource(ICallStackCheck[]) instead.")]
		public static void CallFromSource(params Regex[] Sources)
		{
			int i, c = Sources.Length;
			ICallStackCheck[] Sources2 = new ICallStackCheck[c];

			for (i = 0; i < c; i++)
				Sources2[i] = new ApproveRegex(Sources[i]);

			CallFromSource(Sources2);
		}

		/// <summary>
		/// Makes sure the call is made from one of the listed sources.
		/// </summary>
		/// <param name="Sources">Original call must be made from one of these sources. Can be a mix of
		/// <see cref="Assembly"/>, <see cref="Type"/>, <see cref="string"/> and <see cref="Regex"/> objects.</param>
		[Obsolete("Use CallFromSource(ICallStackCheck[]) instead.")]
		public static void CallFromSource(params object[] Sources)
		{
			CallFromSource(Convert(Sources));
		}

		/// <summary>
		/// Converts an array of objects into an array of <see cref="ICallStackCheck"/> 
		/// objects, assuming each listed source is approved.
		/// </summary>
		/// <param name="Sources">Sources</param>
		/// <returns>Array of corresponding Callstack checks.</returns>
		public static ICallStackCheck[] Convert(params object[] Sources)
		{
			int i, c = Sources.Length;
			ICallStackCheck[] Sources2 = new ICallStackCheck[c];

			for (i = 0; i < c; i++)
			{
				object Source = Sources[i];

				if (Source is ICallStackCheck Check)
					Sources2[i] = Check;
				else if (Source is Assembly A)
					Sources2[i] = new ApproveAssembly(A);
				else if (Source is Type T)
					Sources2[i] = new ApproveType(T);
				else if (Source is Regex Regex)
					Sources2[i] = new ApproveRegex(Regex);
				else if (Source is string s)
					Sources2[i] = new ApproveString(s);
				else
					throw new ArgumentException("Invalid source type: " + Source.GetType().FullName, nameof(Sources));
			}

			return Sources2;
		}

		/// <summary>
		/// Makes sure the call is made from one of the approved sources and not from one
		/// of the prohibited sources.
		/// </summary>
		/// <param name="Sources">The stack trace from the original call is checked for approved
		/// or prohibited sources to assert the code can proceed.</param>
		public static void CallFromSource(params ICallStackCheck[] Sources)
		{
			CallFromSource(new ICallStackCheck[][] { Sources });
		}

		/// <summary>
		/// Makes sure the call is made from one of the approved sources and not from one
		/// of the prohibited sources.
		/// </summary>
		/// <param name="SourcesByPriority">The stack trace from the original call is checked 
		/// for approved or prohibited sources to assert the code can proceed. The Sources 
		/// lists are evaluated in order, and the first list resulting in a conclusion is 
		/// used.</param>
		public static void CallFromSource(params ICallStackCheck[][] SourcesByPriority)
		{
			FrameInformation FrameInfo;
			int Skip = 1;
			bool WaherPersistence = false;
			bool AsynchTask = false;
			bool Other = false;

			while (true)
			{
				FrameInfo = new FrameInformation(new StackFrame(Skip));
				if (FrameInfo.Last)
					break;

				if (FrameInfo.Valid && FrameInfo.Type != typeof(Assert))
					break;

				Skip++;
			}

			int Caller = Skip;
			bool Prohibited = false;
			bool? Status;

			foreach (ICallStackCheck[] Sources in SourcesByPriority)
			{
				Skip = Caller + 1;

				while (!Prohibited)
				{
					FrameInfo = new FrameInformation(new StackFrame(Skip++));
					if (FrameInfo.Last)
						break;

					if (!FrameInfo.Valid)
						continue;

					foreach (ICallStackCheck Source in Sources)
					{
						Status = Source.Check(FrameInfo);
						if (Status.HasValue)
						{
							if (Status.Value)
								return;
							else
							{
								Prohibited = true;
								break;
							}
						}
					}

					if (Prohibited)
						break;

					if (!Other || !AsynchTask || !WaherPersistence)
					{
						if (string.IsNullOrEmpty(FrameInfo.Assembly.Location))
						{
							if (FrameInfo.AssemblyName.StartsWith("WPSA."))
								WaherPersistence = true;
							else
								Other = true;
						}
						else
						{
							if (FrameInfo.Type == typeof(System.Threading.Tasks.Task))
								AsynchTask = true;
							else if (FrameInfo.TypeName.StartsWith(FrameInfo.AssemblyName) &&
								FrameInfo.AssemblyName + "." == Path.ChangeExtension(Path.GetFileName(FrameInfo.Assembly.Location), string.Empty))
							{
								if (FrameInfo.AssemblyName.StartsWith("Waher.Persistence."))
									WaherPersistence = true;
								else if (!FrameInfo.AssemblyName.StartsWith("Waher.") && !FrameInfo.AssemblyName.StartsWith("System."))
									Other = true;
							}
							else if (!Path.GetFileName(FrameInfo.Assembly.Location).StartsWith("System."))
								Other = true;
						}
					}
				}

				if (Prohibited)
					break;
			}

			if (!Prohibited && AsynchTask && WaherPersistence && !Other)
				return; // In asynch call - stack trace not showing asynchronous call stack. If loading from database, i.e. populating object asynchronously, (possibly, check is vulnerable), give check a pass. Access will be restricted at a later stage, when accessing properties synchronously.

			FrameInfo = new FrameInformation(new StackFrame(Skip = Caller));

			string ObjectId = FrameInfo.Type.FullName + "." + FrameInfo.Method.Name;
			StackTrace Trace = new StackTrace(Skip, false);
			UnauthorizedAccessEventArgs e = new UnauthorizedAccessEventArgs(FrameInfo, Trace);
			List<KeyValuePair<string, object>> Tags = new List<KeyValuePair<string, object>>()
			{
				new KeyValuePair<string, object>("Method", FrameInfo.Method.Name),
				new KeyValuePair<string, object>("Type", FrameInfo.Type.FullName),
				new KeyValuePair<string, object>("Assembly", FrameInfo.Assembly.FullName)
			};

			Skip = 0;
			while (true)
			{
				FrameInfo = new FrameInformation(new StackFrame(Skip));
				if (FrameInfo.Last)
					break;

				if (FrameInfo.Valid)
				{
					Tags.Add(new KeyValuePair<string, object>("Pos" + Skip.ToString(),
						FrameInfo.Assembly.GetName().Name + ", " + FrameInfo.TypeName + ", " +
						FrameInfo.Method.Name));
				}
				else
					Tags.Add(new KeyValuePair<string, object>("Pos" + Skip.ToString(), FrameInfo.ToString()));

				Skip++;
			}

			Log.Warning("Unauthorized access detected and prevented.", ObjectId, string.Empty, "UnauthorizedAccess", EventLevel.Major,
				string.Empty, e.Assembly.FullName, Trace.ToString(), Tags.ToArray());

			UnauthorizedAccess?.Raise(null, e);

			throw new UnauthorizedCallstackException("Unauthorized access.");
		}

		/// <summary>
		/// Event raised when an unauthorized access has been detected.
		/// </summary>
		public static event EventHandler<UnauthorizedAccessEventArgs> UnauthorizedAccess = null;

	}
}
