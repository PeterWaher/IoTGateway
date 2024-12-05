﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;

namespace Waher.Networking.HTTP.Test
{
	[TestClass]
	public class Http10ServerTests : HttpServerTests
	{
		[ClassInitialize]
		public new static void ClassInitialize(TestContext _)
		{
			HttpServerTests.ClassInitialize(_);
		}

		[ClassCleanup]
		public new static void ClassCleanup()
		{
			HttpServerTests.ClassCleanup();

		}

		public override Version ProtocolVersion => HttpVersion.Version10;
	}
}