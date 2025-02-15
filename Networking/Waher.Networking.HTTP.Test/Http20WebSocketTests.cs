﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP.Test
{
	[TestClass]
	public class Http20WebSocketTests : WebSocketTests
	{
		[ClassInitialize]
		public new static void ClassInitialize(TestContext _)
		{
			WebSocketTests.ClassInitialize(_);
		}

		[ClassCleanup]
		public new static async Task ClassCleanup()
		{
			await WebSocketTests.ClassCleanup();
		}

		public override Version ProtocolVersion => HttpVersion.Version20;
	}
}
