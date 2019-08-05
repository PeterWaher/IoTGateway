using System;
using System.Diagnostics;
using System.Reflection;

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
			CallFromSource(Assemblies);
		}

		/// <summary>
		/// Makes sure the call is made from one of the listed classes.
		/// </summary>
		/// <param name="Classes">Original call must be made from one of these classes.</param>
		public static void CallFromClass(params Type[] Classes)
		{
			CallFromSource(Classes);
		}

		/// <summary>
		/// Makes sure the call is made from one of the listed sources.
		/// </summary>
		/// <param name="Sources">Original call must be made from one of these sources.</param>
		public static void CallFromSource(params object[] Sources)
		{
			StackTrace Trace = new StackTrace(2, false);
			int i, c = Trace.FrameCount;

			for (i = 0; i < c; i++)
			{
				StackFrame Frame = Trace.GetFrame(i);
				MethodBase Method = Frame.GetMethod();
				Type Type = Method.DeclaringType;
				Assembly Assembly = Type.Assembly;

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
				}
			}

			throw new UnauthorizedAccessException("Unauthorized access.");
		}
	}
}
