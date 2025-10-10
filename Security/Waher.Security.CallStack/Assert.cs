﻿using System;
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
			StackFrame Frame;
			MethodBase Method;
			Type Type;
			Assembly Assembly;
			string TypeName;
			string AssemblyName;
			int Skip = 1;
			bool WaherPersistence = false;
			bool AsynchTask = false;
			bool Other = false;

			while (true)
			{
				Frame = new StackFrame(Skip);
				Method = Frame.GetMethod();
				if (Method is null)
					break;

				Type = Method.DeclaringType;
				if (!(Type is null) && Type != typeof(Assert))
					break;

				Skip++;
			}

			int Caller = Skip++;

			while (true)
			{
				Frame = new StackFrame(Skip++);
				Method = Frame.GetMethod();
				if (Method is null)
					break;

				Type = Method.DeclaringType;
				if (Type is null)
					continue;

				TypeName = Type.FullName;
				Assembly = Type.Assembly;
				AssemblyName = Assembly.GetName().Name;

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
						if (IsMatch(Regex, TypeName + "." + Method.Name) ||
							IsMatch(Regex, TypeName) ||
							IsMatch(Regex, AssemblyName))
						{
							return;
						}
					}
					else if (Source is string s)
					{
						if (TypeName + "." + Method.Name == s ||
							TypeName == s ||
							AssemblyName == s)
						{
							return;
						}
					}
				}

				if (!Other || !AsynchTask || !WaherPersistence)
				{
					if (string.IsNullOrEmpty(Assembly.Location))
					{
						if (AssemblyName.StartsWith("WPSA."))
							WaherPersistence = true;
						else
							Other = true;
					}
					else
					{
						if (Type == typeof(System.Threading.Tasks.Task))
							AsynchTask = true;
						else if (TypeName.StartsWith(AssemblyName) &&
							AssemblyName + "." == Path.ChangeExtension(Path.GetFileName(Assembly.Location), string.Empty))
						{
							if (AssemblyName.StartsWith("Waher.Persistence."))
								WaherPersistence = true;
							else if (!AssemblyName.StartsWith("Waher.") && !AssemblyName.StartsWith("System."))
								Other = true;
						}
						else if (!Path.GetFileName(Assembly.Location).StartsWith("System."))
							Other = true;
					}
				}
			}

			if (AsynchTask && WaherPersistence && !Other)
				return; // In asynch call - stack trace not showing asynchronous call stack. If loading from database, i.e. populating object asynchronously, (possibly, check is vulnerable), give check a pass. Access will be restricted at a later stage, when accessing properties synchronously.

			Frame = new StackFrame(Skip = Caller);
			Method = Frame.GetMethod();
			Type = Method.DeclaringType;
			Assembly = Type.Assembly;

			string ObjectId = Type.FullName + "." + Method.Name;
			StackTrace Trace = new StackTrace(Skip, false);
			UnauthorizedAccessEventArgs e = new UnauthorizedAccessEventArgs(Method, Type, Assembly, Trace);
			List<KeyValuePair<string, object>> Tags = new List<KeyValuePair<string, object>>()
			{
				new KeyValuePair<string, object>("Method", Method.Name),
				new KeyValuePair<string, object>("Type", Type.FullName),
				new KeyValuePair<string, object>("Assembly", Assembly.FullName)
			};

			Skip = 0;
			while (true)
			{
				Frame = new StackFrame(Skip);
				Method = Frame.GetMethod();
				if (Method is null)
					break;

				Type = Method.DeclaringType;
				if (Type is null)
					Tags.Add(new KeyValuePair<string, object>("Pos" + Skip.ToString(), Frame.ToString()));
				else
				{
					TypeName = Type.FullName;
					Assembly = Type.Assembly;
					AssemblyName = Assembly.GetName().Name;

					Tags.Add(new KeyValuePair<string, object>("Pos" + Skip.ToString(), Assembly.GetName().Name + ", " + TypeName + ", " + Method.Name));
				}

				Skip++;
			}

			Log.Warning("Unauthorized access detected and prevented.", ObjectId, string.Empty, "UnauthorizedAccess", EventLevel.Major, 
				string.Empty, Assembly.FullName, Trace.ToString(), Tags.ToArray());

			UnauthorizedAccess?.Raise(null, e);

			throw new UnauthorizedCallstackException("Unauthorized access.");
		}

		private static bool IsMatch(Regex Regex, string s)
		{
			Match M = Regex.Match(s);
			return M.Success && M.Index == 0 && M.Length == s.Length;
		}

		/// <summary>
		/// Event raised when an unauthorized access has been detected.
		/// </summary>
		public static event EventHandler<UnauthorizedAccessEventArgs> UnauthorizedAccess = null;

	}
}
