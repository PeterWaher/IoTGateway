using System;
using System.Diagnostics;
using System.Reflection;

namespace Waher.Security.CallStack
{
	/// <summary>
	/// Contains information about a specific frame in a call stack.
	/// </summary>
	public class FrameInformation
	{
		private readonly bool valid;
		private readonly bool last;
		private readonly MethodBase method;
		private readonly Type type;
		private readonly string typeName;
		private readonly Assembly assembly;
		private readonly string assemblyName;
		private readonly StackFrame frame;

		/// <summary>
		/// Contains information about a specific frame in a call stack.
		/// </summary>
		/// <param name="Frame">Stack frame.</param>
		public FrameInformation(StackFrame Frame)
		{
			this.frame = Frame;
			this.method = Frame.GetMethod();
			if (this.method is null)
				this.last = true;
			else
			{
				this.last = false;

				this.type = this.method.DeclaringType;
				if (this.type is null)
					this.valid = false;
				else
				{
					this.valid = true;

					this.typeName = this.type.Name;
					this.assembly = this.type.Assembly;
					this.assemblyName = this.assembly.GetName().Name;
				}
			}
		}

		/// <summary>
		/// Stack frame represented.
		/// </summary>
		public StackFrame Frame => this.frame;

		/// <summary>
		/// If the frame is valid.
		/// </summary>
		public bool Valid => this.valid;

		/// <summary>
		/// If this is the last frame in the stack (no method information available).
		/// </summary>
		public bool Last => this.last;

		/// <summary>
		/// Method represented by the frame.
		/// </summary>
		public MethodBase Method => this.method;

		/// <summary>
		/// Call made from this type.
		/// </summary>
		public Type Type => this.type;

		/// <summary>
		/// Name of <see cref="Type"/>
		/// </summary>
		public string TypeName => this.typeName;

		/// <summary>
		/// Assembly represented by the frame.
		/// </summary>
		public Assembly Assembly => this.assembly;

		/// <summary>
		/// Name of <see cref="Assembly"/>
		/// </summary>
		public string AssemblyName => this.assemblyName;

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.frame.ToString();
		}
	}
}
