﻿using System;
using Waher.Persistence.Attributes;
using Waher.Things.DisplayableParameters;

namespace Waher.Things.SourceEvents
{
	/// <summary>
	/// Abstract base class for all node events with parameters.
	/// </summary>
	public abstract class NodeParametersEvent : NodeStatusEvent 
    {
		private Parameter[] parameters = null;
		private bool hasChildren = false;
		private bool childrenOrdered = false;
		private bool isReadable = false;
		private bool isControllable = false;
		private bool hasCommands = false;
		private string parentId = string.Empty;
		private string parentPartition = string.Empty;
		private DateTime updated = DateTime.MinValue;

		/// <summary>
		/// Abstract base class for all node events with parameters.
		/// </summary>
		public NodeParametersEvent()
			: base()
		{
		}

		/// <summary>
		/// Displayable parameters.
		/// </summary>
		[DefaultValueNull]
		[ShortName("dp")]
		public Parameter[] Parameters
		{
			get => this.parameters;
			set => this.parameters = value;
		}

		/// <summary>
		/// When node was last updated, at time of event.
		/// </summary>
		[DefaultValueDateTimeMinValue]
		public DateTime Updated
		{
			get => this.updated;
			set => this.updated = value;
		}

		/// <summary>
		/// Parent identity of node, at time of event.
		/// </summary>
		[DefaultValueStringEmpty]
		public string ParentId
		{
			get => this.parentId;
			set => this.parentId = value;
		}

		/// <summary>
		/// Partition of parent of node, at time of event.
		/// </summary>
		[DefaultValueStringEmpty]
		public string ParentPartition
		{
			get => this.parentPartition;
			set => this.parentPartition = value;
		}

		/// <summary>
		/// If node had children, at time of event.
		/// </summary>
		[DefaultValue(false)]
		public bool HasChildren
		{
			get => this.hasChildren;
			set => this.hasChildren = value;
		}

		/// <summary>
		/// If node was readable, at time of event.
		/// </summary>
		[DefaultValue(false)]
		public bool IsReadable
		{
			get => this.isReadable;
			set => this.isReadable = value;
		}

		/// <summary>
		/// If node was controllable, at time of event.
		/// </summary>
		[DefaultValue(false)]
		public bool IsControllable
		{
			get => this.isControllable;
			set => this.isControllable = value;
		}

		/// <summary>
		/// If node had commands, at time of event.
		/// </summary>
		[DefaultValue(false)]
		public bool HasCommands
		{
			get => this.hasCommands;
			set => this.hasCommands = value;
		}

		/// <summary>
		/// If node children was ordered, at time of event.
		/// </summary>
		[DefaultValue(false)]
		public bool ChildrenOrdered
		{
			get => this.childrenOrdered;
			set => this.childrenOrdered = value;
		}
	}
}
