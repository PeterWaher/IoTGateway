﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;

namespace Waher.Networking.HTTP.Test
{
	[TestClass]
	public class Http11WebSocketTests : WebSocketTests
	{
		[ClassInitialize]
		public new static void ClassInitialize(TestContext _)
		{
			WebSocketTests.ClassInitialize(_);
		}

		[ClassCleanup]
		public new static void ClassCleanup()
		{
			WebSocketTests.ClassCleanup();

		}

		public override Version ProtocolVersion => HttpVersion.Version11;
	}
}