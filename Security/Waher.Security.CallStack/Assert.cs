using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		public static void CallFromAssembly(params Assembly[] Assemblies)
		{
			AssertSource(Assemblies);
		}

		/// <summary>
		/// Makes sure the call is made from one of the listed classes.
		/// </summary>
		/// <param name="Classes">Original call must be made from one of these classes.</param>
		public static void CallFromClass(params Type[] Classes)
		{
			AssertSource(Classes);
		}

		/// <summary>
		/// Makes sure the call is made from one of the listed sources.
		/// </summary>
		/// <param name="Sources">Original call must be made from one of these sources. Source strings are checked against
		/// Assemblies, classes and method names.</param>
		public static void CallFromSource(params string[] Sources)
		{
			AssertSource(Sources);
		}

		/// <summary>
		/// Makes sure the call is made from one of the listed sources.
		/// </summary>
		/// <param name="Sources">Original call must be made from one of these sources. Source strings are checked against
		/// Assemblies, classes and method names.</param>
		public static void CallFromSource(params Regex[] Sources)
		{
			AssertSource(Sources);
		}

		/// <summary>
		/// Makes sure the call is made from one of the listed sources.
		/// </summary>
		/// <param name="Sources">Original call must be made from one of these sources. Can be a mix of
		/// <see cref="Assembly"/>, <see cref="Type"/>, <see cref="string"/> and <see cref="Regex"/> objects.</param>
		public static void CallFromSource(params object[] Sources)
		{
			AssertSource(Sources);
		}

		/// <summary>
		/// Makes sure the call is made from one of the listed sources.
		/// </summary>
		/// <param name="Sources">Original call must be made from one of these sources. Can be a mix of
		/// <see cref="Assembly"/>, <see cref="Type"/>, <see cref="string"/> and <see cref="Regex"/> objects.</param>
		private static void AssertSource(params object[] Sources)
		{
			StackTrace Trace = new StackTrace(2, false);
			int i, c = Trace.FrameCount;
			StackFrame Frame;
			MethodBase Method;
			Type Type;
			Assembly Assembly;

			for (i = 1; i < c; i++)
			{
				Frame = Trace.GetFrame(i);
				Method = Frame.GetMethod();
				Type = Method.DeclaringType;
				Assembly = Type.Assembly;

				foreach (object Source in Sources)
				{
					if (Source is Assembly A)
					{
						if (A == Assembly)
							return;
					}
					else if (Source is Type T)
					{
						if (T == Type)
							return;
					}
					else if (Source is Regex Regex)
					{
						if (IsMatch(Regex, Type.FullName + "." + Method.Name) ||
							IsMatch(Regex, Type.FullName) ||
							IsMatch(Regex, Assembly.GetName().Name))
						{
							return;
						}
					}
					else if (Source is string s)
					{
						if (Type.FullName + "." + Method.Name == s ||
							Type.FullName == s ||
							Assembly.GetName().Name == s)
						{
							return;
						}
					}
				}
			}

			Frame = Trace.GetFrame(0);
			Method = Frame.GetMethod();
			Type = Method.DeclaringType;
			Assembly = Type.Assembly;

			Log.Warning("Unauthorized access detected and prevented.", Type.FullName + "." + Method.Name, string.Empty,
				"UnauthorizedAccess", EventLevel.Major, string.Empty, Assembly.FullName, Trace.ToString(),
				new KeyValuePair<string, object>[]
				{
					new KeyValuePair<string, object>("Method", Method.Name),
					new KeyValuePair<string, object>("Type", Type.FullName),
					new KeyValuePair<string, object>("Assembly", Assembly.FullName)
				});

			UnauthorizedAccessEventHandler h = UnauthorizedAccess;
			if (!(h is null))
			{
				try
				{
					h(null, new UnauthorizedAccessEventArgs(Method, Type, Assembly, Trace));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

			throw new UnauthorizedAccessException("Unauthorized access.");
		}

		private static bool IsMatch(Regex Regex, string s)
		{
			Match M = Regex.Match(s);
			return M.Success && M.Index == 0 && M.Length == s.Length;
		}

		/// <summary>
		/// Event raised when an unauthorized access has been detected.
		/// </summary>
		public static event UnauthorizedAccessEventHandler UnauthorizedAccess = null;

	}
}
