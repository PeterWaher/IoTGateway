﻿//#define INFO_IN_SNIFFERS

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;
using Waher.Runtime.Profiling;

namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// Represents a node in a HTTP/2 priority tree
	/// </summary>
	public class PriorityNodeRfc7540 : IPriorityNode
	{
		private LinkedList<PendingRequest> pendingRequests = null;
		private LinkedList<PriorityNodeRfc7540> childNodes = null;
		private PriorityNodeRfc7540 dependentOn;
		private ProfilerThread dataThread;
		private ProfilerThread windowThread;
		private readonly Profiler profiler;
		private readonly IObservableLayer observable;
		private readonly PriorityNodeRfc7540 root;
		private readonly FlowControlRfc7540 flowControl;
		private readonly int outputMaxFrameSize;
		private readonly bool hasProfiler;
		private readonly bool isStream;
		private double resourceFraction = 1;
		private int totalChildWeights = 0;
		private int outputWindowSize0;
		private int outputWindowSize;
		private int outputWindowSizeFraction;
		private byte weight;
		private bool customWindowSize = false;
		private bool disposed = false;

		/// <summary>
		/// Represents a node in a HTTP/2 priority tree
		/// </summary>
		/// <param name="DependentNode">Node dependency.</param>
		/// <param name="Root">Root node.</param>
		/// <param name="Stream">Corresponding HTTP/2 stream.</param>
		/// <param name="Weight">Weight assigned to the node.</param>
		/// <param name="FlowControl">Flow control object.</param>
		/// <param name="Profiler">Profiler, if available.</param>
		/// <param name="Observable">Observable object.</param>
		public PriorityNodeRfc7540(PriorityNodeRfc7540 DependentNode, PriorityNodeRfc7540 Root, Http2Stream Stream, byte Weight,
			FlowControlRfc7540 FlowControl, Profiler Profiler, IObservableLayer Observable)
		{
			this.isStream = !(Stream is null);

			if (this.isStream)
				this.outputWindowSize0 = FlowControl.RemoteSettings.InitialStreamWindowSize;
			else
				this.outputWindowSize0 = ConnectionSettings.DefaultHttp2InitialConnectionWindowSize;

			this.dependentOn = DependentNode;
			this.root = Root;
			this.Stream = Stream;
			this.weight = Weight;
			this.outputWindowSize = this.outputWindowSizeFraction = this.outputWindowSize0;
			this.outputMaxFrameSize = FlowControl.RemoteSettings.MaxFrameSize;
			this.profiler = Profiler;
			this.observable = Observable;
			this.hasProfiler = !(this.profiler is null);
			this.flowControl = FlowControl;
		}

		/// <summary>
		/// Parent node.
		/// </summary>
		public PriorityNodeRfc7540 DependentOn
		{
			get => this.dependentOn;
			internal set => this.dependentOn = value;
		}

		/// <summary>
		/// Parent node in the priority tree.
		/// </summary>
		public PriorityNodeRfc7540 Parent => this.DependentOn ?? this.root;

		/// <summary>
		/// Corresponding HTTP/2 stream.
		/// </summary>
		public Http2Stream Stream { get; }

		/// <summary>
		/// Window Profiler thread, if any.
		/// </summary>
		public ProfilerThread WindowThread => this.windowThread;

		/// <summary>
		/// Window Data thread, if any.
		/// </summary>
		public ProfilerThread DataThread => this.dataThread;

		/// <summary>
		/// HTTP/2 client connection object.
		/// </summary>
		internal HttpClientConnection Connection => this.flowControl.Connection;

		internal LinkedList<PriorityNodeRfc7540> MoveChildrenFrom()
		{
			LinkedList<PriorityNodeRfc7540> Result = this.childNodes;
			this.childNodes = null;
			this.totalChildWeights = 0;
			return Result;
		}

		internal void MoveChildrenTo(LinkedList<PriorityNodeRfc7540> Children)
		{
			if (!(Children is null))
			{
				if (this.childNodes is null)
					this.childNodes = new LinkedList<PriorityNodeRfc7540>();

				foreach (PriorityNodeRfc7540 Child in Children)
				{
					this.childNodes.AddLast(Child);
					this.totalChildWeights += Child.Weight;
				}

				this.RecalculateChildFractions();
			}
		}

		/// <summary>
		/// Currently available resources
		/// </summary>
		public int AvailableResources
		{
			get
			{
				if (this.customWindowSize)
					return this.outputWindowSize;
				else
					return this.outputWindowSize + this.outputWindowSizeFraction - this.outputWindowSize0;
			}
		}

		/// <summary>
		/// Weight assigned to the node.
		/// </summary>
		public byte Weight
		{
			get => this.weight;
			internal set
			{
				if (this.weight != value)
				{
					if (!(this.dependentOn is null))
						this.dependentOn.totalChildWeights += value - this.weight;

					this.weight = value;

					this.dependentOn?.RecalculateChildFractions();
				}
			}
		}

		/// <summary>
		/// Sum of weights of children.
		/// </summary>
		public int TotalChildWeights => this.totalChildWeights;

		/// <summary>
		/// If the node has child nodes.
		/// </summary>
		public bool HasChildren => !(this.childNodes is null);

		/// <summary>
		/// First child node, if any, or null if none.
		/// </summary>
		public LinkedListNode<PriorityNodeRfc7540> FirstChild => this.childNodes?.First;

		/// <summary>
		/// Adds a child node to the node.
		/// </summary>
		/// <param name="Child">Child node.</param>
		public void AddChildDependency(PriorityNodeRfc7540 Child)
		{
			PriorityNodeRfc7540 OrgChildParent = Child.Parent;
			OrgChildParent?.RemoveChildDependency(Child);

			PriorityNodeRfc7540 Loop = this.Parent;
			while (!(Loop is null) && Loop != Child)
				Loop = Loop.Parent;

			if (!(Loop is null))
			{
				this.Parent.RemoveChildDependency(this);
				OrgChildParent.AddChildDependency(this);
			}

			if (this.childNodes is null)
				this.childNodes = new LinkedList<PriorityNodeRfc7540>();

			this.childNodes.AddLast(Child);
			Child.dependentOn = this;

			this.totalChildWeights += Child.Weight;

			this.RecalculateChildFractions();
		}

		/// <summary>
		/// Removes a child node from the node.
		/// </summary>
		/// <param name="Child">Child node.</param>
		/// <returns>If the child node was not found.</returns>
		public bool RemoveChildDependency(PriorityNodeRfc7540 Child)
		{
			Child.DependentOn = null;

			if (this.childNodes is null)
				return false;

			if (!this.childNodes.Remove(Child))
				return false;

			this.totalChildWeights -= Child.Weight;

			if (this.childNodes?.First is null)
				this.childNodes = null;
			else
				this.RecalculateChildFractions();

			return true;
		}

		private void RecalculateChildFractions()
		{
			if (!(this.childNodes is null) && this.totalChildWeights > 0)
			{
				LinkedListNode<PriorityNodeRfc7540> Loop = this.childNodes.First;
				PriorityNodeRfc7540 Child;
				int MaxStreamWindow = this.flowControl.RemoteSettings.InitialStreamWindowSize;
				int Size;

				while (!(Loop is null))
				{
					Child = Loop.Value;

					Child.resourceFraction = this.resourceFraction * Child.weight / this.totalChildWeights;

					if (!Child.customWindowSize)
					{
						Size = (int)Math.Floor(this.outputWindowSize0 * Child.resourceFraction);
						if (Child.isStream && Size > MaxStreamWindow)
							Size = MaxStreamWindow;

						Child.outputWindowSizeFraction = Size;
					}

#if INFO_IN_SNIFFERS
					StringBuilder sb = new StringBuilder();
					sb.Append("Stream ");
					sb.Append((Child.Stream?.StreamId ?? 0).ToString());
					sb.Append(" State: Available Resources = ");
					sb.Append(Child.AvailableResources.ToString());

					if (!Child.customWindowSize)
					{
						sb.Append(", New Max Size Fraction = ");
						sb.Append(Child.outputWindowSizeFraction.ToString());
					}

					this.observable?.Information(sb.ToString());
#endif
					Loop = Loop.Next;
				}
			}
		}

		private void RecalculateChildWindows(int ConnectionWindowSize, int StreamWindowSize, bool Trigger)
		{
			if (!(this.childNodes is null) && this.totalChildWeights > 0)
			{
				LinkedListNode<PriorityNodeRfc7540> Loop = this.childNodes.First;
				PriorityNodeRfc7540 Child;

				while (!(Loop is null))
				{
					Child = Loop.Value;

					Child.SetNewWindowSize(ConnectionWindowSize, Math.Min(StreamWindowSize,
						(int)(ConnectionWindowSize * this.resourceFraction * Child.weight / this.totalChildWeights)), Trigger);
					Loop = Loop.Next;
				}
			}
		}

		/// <summary>
		/// Requests resources from the available pool of resources in the tree.
		/// </summary>
		/// <param name="RequestedResources">Requested amount of resources.</param>
		/// <param name="CancellationToken">Optional Cancellation token</param>
		/// <returns>Number of resources granted.</returns>
		public Task<int> RequestAvailableResources(int RequestedResources,
			CancellationToken? CancellationToken)
		{
			int AvailableResources = this.AvailableResources;
			int RootAvailableResources = this.root.AvailableResources;
			int Available = Math.Min(RequestedResources,
				Math.Min(RootAvailableResources, AvailableResources));

#if INFO_IN_SNIFFERS
			StringBuilder sb = new StringBuilder();

			sb.Append("Stream ");
			sb.Append((this.Stream?.StreamId ?? 0).ToString());
			sb.Append(" Requests resources: Requested = ");
			sb.Append(RequestedResources.ToString());
			sb.Append(", Available = ");
			sb.Append(AvailableResources.ToString());
			sb.Append(" (at root = ");
			sb.Append(RootAvailableResources.ToString());
			sb.Append("), Granted = ");
			sb.Append(Available.ToString());

			this.observable?.Information(sb.ToString());
#endif
			if (Available <= 0)
			{
				PendingRequest Request = new PendingRequest(RequestedResources);

				if (this.pendingRequests is null)
					this.pendingRequests = new LinkedList<PendingRequest>();

				this.pendingRequests.AddLast(Request);

				if (CancellationToken.HasValue && CancellationToken.Value.CanBeCanceled)
					CancellationToken.Value.Register(() => Request.Request.TrySetException(new OperationCanceledException()));

				return Request.Request.Task;
			}
			else
			{
				if (Available > this.outputMaxFrameSize)
					Available = this.outputMaxFrameSize;

				if (this.hasProfiler)
				{
					if (this.windowThread is null)
						this.CheckProfilerThreads();

					this.windowThread.NewSample(this.AvailableResources);
					this.root.windowThread.NewSample(this.root.outputWindowSize);

					this.outputWindowSize -= Available;
					this.root.outputWindowSize -= Available;

					this.windowThread.NewSample(this.AvailableResources);
					this.root.windowThread.NewSample(this.root.outputWindowSize);

					this.dataThread.NewSample(0);
					this.dataThread.NewSample(Available);
				}
				else
				{
					this.outputWindowSize -= Available;
					this.root.outputWindowSize -= Available;
				}

				return Task.FromResult(Available);
			}
		}

		/// <summary>
		/// Releases stream resources back to the stream, as a result of a client sending a
		/// WINDOW_UPDATE frame with Stream ID > 0.
		/// </summary>
		/// <param name="Resources">Amount of resources released back</param>
		/// <returns>Size of current window. Negative = error</returns>
		public int ReleaseStreamResources(int Resources)
		{
			int NewSize = this.outputWindowSize + Resources;
			if (NewSize < 0 || NewSize > int.MaxValue - 1)
				return -2;

			if (this.hasProfiler)
			{
				if (this.windowThread is null)
					this.CheckProfilerThreads();

				this.windowThread.NewSample(this.AvailableResources);
				this.windowThread.Event("StreamWindowUpdate");

				this.outputWindowSize = NewSize;

				this.windowThread.NewSample(this.AvailableResources);
			}
			else
				this.outputWindowSize = NewSize;

#if INFO_IN_SNIFFERS
			StringBuilder sb = new StringBuilder();

			sb.Append("Stream ");
			sb.Append((this.Stream?.StreamId ?? 0).ToString());
			sb.Append(" State: New Size = ");
			sb.Append(NewSize.ToString());
#endif

			if (NewSize > this.outputWindowSize0)
			{
				this.outputWindowSize0 = NewSize;
				this.customWindowSize = true;
				
				// Note: Client allowed to increase a particular stream's window size beyond
				//       the initial stream size defined for the connection.

#if INFO_IN_SNIFFERS
				sb.Append(", New Max Size = ");
				sb.Append(this.outputWindowSize0.ToString());
#endif
			}

			int RootAvailableResources = this.root.AvailableResources;
			Resources = Math.Min(RootAvailableResources, this.AvailableResources);

#if INFO_IN_SNIFFERS
			sb.Append(", Available Resources = ");
			sb.Append(Resources.ToString());
			sb.Append(" (at root = ");
			sb.Append(RootAvailableResources.ToString());
			sb.Append(')');

			this.observable?.Information(sb.ToString());
#endif
			this.TriggerPending(ref Resources);

			return this.outputWindowSize;
		}

		private void TriggerPending(ref int Resources)
		{
			LinkedList<KeyValuePair<int, PendingRequest>> ToRelease = null;
			LinkedListNode<PendingRequest> Loop;
			int i;

			while (Resources > 0 && !((Loop = this.pendingRequests?.First) is null))
			{
				this.pendingRequests.RemoveFirst();

				i = Loop.Value.Requested;

				if (Resources < i)
					i = Resources;

				if (this.outputMaxFrameSize < i)
					i = this.outputMaxFrameSize;

				if (this.hasProfiler)
				{
					if (this.windowThread is null)
						this.CheckProfilerThreads();

					this.windowThread.NewSample(this.AvailableResources);
					this.root.windowThread.NewSample(this.root.outputWindowSize);

					this.outputWindowSize -= i;
					this.root.outputWindowSize -= i;

					this.windowThread.NewSample(this.AvailableResources);
					this.root.windowThread.NewSample(this.root.outputWindowSize);
				}
				else
				{
					this.outputWindowSize -= i;
					this.root.outputWindowSize -= i;
				}

				if (ToRelease is null)
					ToRelease = new LinkedList<KeyValuePair<int, PendingRequest>>();

				ToRelease.AddLast(new KeyValuePair<int, PendingRequest>(i, Loop.Value));

				Resources -= i;
			}

			if (!(ToRelease is null))
			{
				foreach (KeyValuePair<int, PendingRequest> P in ToRelease)
					P.Value.Request.TrySetResult(P.Key);
			}
		}

		/// <summary>
		/// Releases connection resources back, as a result of a client sending a
		/// WINDOW_UPDATE frame with Stream ID = 0.
		/// </summary>
		/// <param name="Resources">Amount of resources released back</param>
		/// <returns>Size of current window. Negative = error</returns>
		public int ReleaseConnectionResources(int Resources)
		{
			int NewSize = this.outputWindowSize + Resources;
			if (NewSize < 0 || NewSize > int.MaxValue - 1)
				return -1;

			if (this.hasProfiler)
			{
				if (this.windowThread is null)
					this.CheckProfilerThreads();

				this.windowThread.NewSample(this.AvailableResources);
				this.windowThread.Event("ConnectionWindowUpdate");

				this.outputWindowSize = NewSize;

				this.windowThread.NewSample(this.AvailableResources);
			}
			else
				this.outputWindowSize = NewSize;

#if INFO_IN_SNIFFERS
			StringBuilder sb = new StringBuilder();

			sb.Append("Stream ");
			sb.Append((this.Stream?.StreamId ?? 0).ToString());
			sb.Append(" State: New Size = ");
			sb.Append(NewSize.ToString());
#endif

			if (NewSize > this.outputWindowSize0)
			{
				this.outputWindowSize0 = NewSize;
				this.customWindowSize = true;

				this.RecalculateChildWindows(this.outputWindowSize0, this.outputWindowSizeFraction, true);

#if INFO_IN_SNIFFERS
				sb.Append(", New Max Size = ");
				sb.Append(this.outputWindowSize0.ToString());
#endif
			}

#if INFO_IN_SNIFFERS
			sb.Append(", Available Resources = ");
			sb.Append(this.AvailableResources.ToString());

			if (!(this.root is null))
			{
				sb.Append(" (at root = ");
				sb.Append(this.root.AvailableResources.ToString());
				sb.Append(')');
			}

			this.observable?.Information(sb.ToString());
#endif
			Resources = NewSize;
			this.TriggerPendingIfAvailbleDown(ref Resources);

			return NewSize;
		}

		/// <summary>
		/// Current output window size.
		/// </summary>
		public int OutputWindowSize => this.outputWindowSize0;

		/// <summary>
		/// Sets a new window size.
		/// </summary>
		/// <param name="ConnectionWindowSize">Connection Window size</param>
		/// <param name="StreamWindowSize">Stream Window size</param>
		/// <param name="Trigger">If pending streams should be triggered.</param>
		public void SetNewWindowSize(int ConnectionWindowSize, int StreamWindowSize, bool Trigger)
		{
			int WindowSize = this.isStream ? StreamWindowSize : ConnectionWindowSize;
			int Diff = WindowSize - this.outputWindowSize0;
			this.outputWindowSize0 = WindowSize;

			int Size = (int)Math.Floor(this.outputWindowSize0 * this.resourceFraction);
			if (this.isStream)
				Size = Math.Min(this.flowControl.RemoteSettings.InitialStreamWindowSize, Size);

			this.outputWindowSizeFraction = Size;

			if (this.hasProfiler)
			{
				this.windowThread?.NewSample(this.AvailableResources);

				this.outputWindowSize += Diff;

				this.windowThread?.Event("Settings");
				this.windowThread?.NewSample(this.AvailableResources);
			}
			else
				this.outputWindowSize += Diff;

			this.RecalculateChildWindows(ConnectionWindowSize, StreamWindowSize, false);

			if (Trigger)
			{
				if (!(this.root is null))
					WindowSize = Math.Min(this.root.AvailableResources, WindowSize);

				if (WindowSize > 0)
					this.TriggerPendingIfAvailbleDown(ref WindowSize);
			}
		}

		private void TriggerPendingIfAvailbleDown(ref int Resources)
		{
			this.TriggerPending(ref Resources);
			if (Resources <= 0 || this.totalChildWeights <= 0)
				return;

			LinkedListNode<PriorityNodeRfc7540> ChildLoop = this.childNodes?.First;
			PriorityNodeRfc7540 Child;
			int Resources0 = Resources;
			int Part;
			int Delta;

			while (Resources > 0 && !(ChildLoop is null))
			{
				Child = ChildLoop.Value;
				Part = (Resources0 * Child.weight + this.totalChildWeights - 1) / this.totalChildWeights;

				if (Part > 0)
				{
					Part = Delta = Math.Min(Child.AvailableResources, Math.Min(Part, Resources));
					if (Part > 0)
					{
						Child.TriggerPendingIfAvailbleDown(ref Part);
						Delta -= Part;
						Resources -= Delta;
					}
				}

				ChildLoop = ChildLoop.Next;
			}
		}

		/// <summary>
		/// Disposes the node and its children, and releases any pending tasks.
		/// </summary>
		public void Dispose()
		{
			if (!this.disposed)
			{
				this.disposed = true;

				LinkedListNode<PendingRequest> RequestLoop = this.pendingRequests?.First;

				while (!(RequestLoop is null))
				{
					RequestLoop.Value.Request.TrySetResult(-1);
					RequestLoop = RequestLoop.Next;
				}

				this.pendingRequests = null;

				LinkedListNode<PriorityNodeRfc7540> ChildLoop = this.childNodes?.First;

				while (!(ChildLoop is null))
				{
					ChildLoop.Value.Dispose();
					ChildLoop = ChildLoop.Next;
				}

				this.childNodes = null;

				if (this.isStream)
					this.Stream.State = StreamState.Closed;

				if (this.hasProfiler)
				{
					this.windowThread?.NewSample(this.outputWindowSize);
					this.windowThread?.Idle();
					this.windowThread?.Stop();

					this.dataThread?.NewSample(0);
					this.dataThread?.Idle();
					this.dataThread?.Stop();
				}
			}
		}

		/// <summary>
		/// Checks the node has the corresponding profiler threads created.
		/// </summary>
		public void CheckProfilerThreads()
		{
			if (this.hasProfiler)
			{
				if (this.dataThread is null && this.isStream)
				{
					this.dataThread = HttpClientConnection.CreateProfilerDataThread(this.profiler, this.Stream.StreamId);
					this.dataThread.NewSample(0);
				}

				if (this.windowThread is null)
				{
					this.windowThread = HttpClientConnection.CreateProfilerWindowThread(this.profiler, this.Stream?.StreamId ?? 0);
					this.windowThread.NewSample(this.AvailableResources);
				}
			}
		}

		/// <summary>
		/// Exports the priorty node to PlantUML format.
		/// </summary>
		/// <param name="Output">UML diagram will be exported here.</param>
		public void ExportPlantUml(StringBuilder Output)
		{
			if (this.isStream)
			{
				Output.Append("object \"Stream ");
				Output.Append(this.Stream.StreamId.ToString());
				Output.Append("\" as S");
				Output.Append(this.Stream.StreamId.ToString());
				Output.AppendLine(" {");
				Output.Append("resource = \"");
				Output.Append(GetResourceFromLabel(this.Stream.StreamThread?.Label));
				Output.AppendLine("\"");
				Output.Append("pendingRequests = ");
				Output.AppendLine(this.pendingRequests?.Count.ToString() ?? "0");
			}
			else
				Output.AppendLine("object \"Root\" as S {");

			Output.Append("childNodes = ");
			Output.AppendLine(this.childNodes?.Count.ToString() ?? "0");
			Output.Append("maxFrameSize = ");
			Output.AppendLine(this.outputMaxFrameSize.ToString());
			Output.Append("resourceFraction = ");
			Output.AppendLine(CommonTypes.Encode(this.resourceFraction));
			Output.Append("totalChildWeights = ");
			Output.AppendLine(this.totalChildWeights.ToString());
			Output.Append("windowSize0 = ");
			Output.AppendLine(this.outputWindowSize0.ToString());
			Output.Append("windowSize = ");
			Output.AppendLine(this.outputWindowSize.ToString());
			Output.Append("windowSizeFraction = ");
			Output.AppendLine(this.outputWindowSizeFraction.ToString());
			Output.Append("weight = ");
			Output.AppendLine(this.weight.ToString());
			Output.Append("AvailableResources = ");
			Output.AppendLine(this.AvailableResources.ToString());
			Output.AppendLine("}");
			Output.AppendLine();

			Output.Append('S');
			Output.Append((this.dependentOn ?? this.root)?.Stream?.StreamId.ToString());
			Output.Append(" *-- S");
			Output.AppendLine(this.Stream?.StreamId.ToString());
			Output.AppendLine();
		}

		internal static string GetResourceFromLabel(string Label)
		{
			if (string.IsNullOrEmpty(Label))
				return string.Empty;

			int i = Label.IndexOf('(');
			if (i >= 0)
				Label = Label.Substring(i + 1).TrimStart();
			else
				return Label;

			i = Label.LastIndexOf(')');
			if (i > 0)
				Label = Label.Substring(0, i).TrimEnd();

			return Label;
		}

		internal void ExportStates(StringBuilder sb, int Indent, Dictionary<int, bool> Exported)
		{
			int StreamId = this.Stream?.StreamId ?? 0;
			if (Exported.ContainsKey(StreamId))
				return;

			Exported[StreamId] = true;

			int AvailableResources = this.AvailableResources;

			for (int i = 1; i < Indent; i++)
				sb.Append("| ");

			if (Indent > 0)
				sb.Append("+-");

			sb.Append(StreamId.ToString());
			sb.Append(": Available = ");
			sb.Append(AvailableResources.ToString());

			if (!(this.root is null))
			{
				int RootAvailableResources = this.root.AvailableResources;

				sb.Append(" (at root = ");
				sb.Append(RootAvailableResources.ToString());
				sb.Append(')');
			}

			sb.AppendLine();

			if (!(this.childNodes is null))
			{
				Indent++;

				foreach (PriorityNodeRfc7540 ChildNode in this.childNodes)
					ChildNode.ExportStates(sb, Indent, Exported);
			}
		}
	}
}
