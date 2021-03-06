﻿using System;
using ApprovalTests;
using ApprovalTests.Reporters;
using Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSP;

namespace CSP_Unittests
{
	[TestClass]
	[UseReporter(typeof(WinMergeReporter))]
	public class VariableParserTests
	{
		[TestMethod]
		public void VerifyStringWithOneVariable()
		{
			var input = "a";
			var output = VariableParser.Parse(input);
			Approvals.Verify(output.ToPrettyString());
		}

		[TestMethod]
		public void VerifyStringWithTwoVariables()
		{
			var input = "a,b";
			var output = VariableParser.Parse(input);
			Approvals.Verify(output.ToPrettyString());
		}

		[TestMethod]
		public void VerifyStringWithUntrimmedVariables()
		{
			var input = " a,b ";
			var output = VariableParser.Parse(input);
			Approvals.Verify(output.ToPrettyString());
		}

		[TestMethod]
		public void WhenVariableHasWhiteSpace_ThenReplaceWithUnderline()
		{
			var input = " a,b c";
			var output = VariableParser.Parse(input);
			Approvals.Verify(output.ToPrettyString());
		}

		[TestMethod]
		public void WhenVariablesNotUnique_ThenThrowException()
		{
			var input = " a,a";
			var exc = "";
			try
			{
				VariableParser.Parse(input);
			}
			catch (ArgumentException ex)
			{
				exc = ex.Message;
			}
			Approvals.Verify("Exception message: " +exc);
		}

		[TestMethod]
		public void WhenVariableLengthIsNull_ThenThrowException()
		{
			var input = " a,";
			var exc = "";
			try
			{
				VariableParser.Parse(input);
			}
			catch (ArgumentException ex)
			{
				exc = ex.Message;
			}
			Approvals.Verify("Exception message: " + exc);
		}

		[TestMethod]
		public void WhenInputIsEmpty_ThenThrowException()
		{
			var input = "";
			var exc = "";
			try
			{
				VariableParser.Parse(input);
			}
			catch (ArgumentException ex)
			{
				exc = ex.Message;
			}
			Approvals.Verify("Exception message: " + exc);
		}
	}
}
