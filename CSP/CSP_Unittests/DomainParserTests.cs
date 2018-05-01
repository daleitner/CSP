using System;
using ApprovalTests;
using ApprovalTests.Reporters;
using Base;
using CSP;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSP_Unittests
{
	[TestClass]
	[UseReporter(typeof(WinMergeReporter))]
	public class DomainParserTests
	{
		[TestMethod]
		public void VerifyStringWithOneVariable()
		{
			var input = "a";
			var output = DomainParser.Parse(input);
			Approvals.Verify(output.ToPrettyString());
		}

		[TestMethod]
		public void VerifyStringWithTwoVariables()
		{
			var input = "a,b";
			var output = DomainParser.Parse(input);
			Approvals.Verify(output.ToPrettyString());
		}

		[TestMethod]
		public void VerifyStringWithUntrimmedVariables()
		{
			var input = " a,b ";
			var output = DomainParser.Parse(input);
			Approvals.Verify(output.ToPrettyString());
		}

		[TestMethod]
		public void WhenVariableHasWhiteSpace_ThenReplaceWithUnderline()
		{
			var input = " a,b c";
			var output = DomainParser.Parse(input);
			Approvals.Verify(output.ToPrettyString());
		}

		[TestMethod]
		public void WhenVariablesNotUnique_ThenThrowException()
		{
			var input = " a,a";
			var exc = "";
			try
			{
				DomainParser.Parse(input);
			}
			catch (ArgumentException ex)
			{
				exc = ex.Message;
			}
			Approvals.Verify("Exception message: " + exc);
		}

		[TestMethod]
		public void WhenVariableLengthIsNull_ThenThrowException()
		{
			var input = " a,";
			var exc = "";
			try
			{
				DomainParser.Parse(input);
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
				DomainParser.Parse(input);
			}
			catch (ArgumentException ex)
			{
				exc = ex.Message;
			}
			Approvals.Verify("Exception message: " + exc);
		}

		[TestMethod]
		public void VerifyNumberInterval()
		{
			var input = "[1-9]";

			var output = DomainParser.Parse(input);

			Approvals.Verify(output.ToPrettyString());
		}

		[TestMethod]
		public void VerifyLetterInterval()
		{
			var input = "[A-F]";
			var output = DomainParser.Parse(input);
			Approvals.Verify(output.ToPrettyString());
		}
	}
}
