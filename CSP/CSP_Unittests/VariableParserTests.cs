using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApprovalTests;
using ApprovalTests.Reporters;
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
			Approvals.VerifyAll(output, "Variable");
		}

		[TestMethod]
		public void VerifyStringWithTwoVariables()
		{
			var input = "a,b";
			var output = VariableParser.Parse(input);
			Approvals.VerifyAll(output, "Variable");
		}

		[TestMethod]
		public void VerifyStringWithUntrimmedVariables()
		{
			var input = " a,b ";
			var output = VariableParser.Parse(input);
			Approvals.VerifyAll(output, "Variable");
		}

		[TestMethod]
		public void WhenVariableHasWhiteSpace_ThenReplaceWithUnderline()
		{
			var input = " a,b c";
			var output = VariableParser.Parse(input);
			Approvals.VerifyAll(output, "Variable");
		}

		[TestMethod]
		public void WhenVariablesNotUnique_ThenThrowException()
		{
			var input = " a,a";
			var exc = "";
			try
			{
				var output = VariableParser.Parse(input);
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
				var output = VariableParser.Parse(input);
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
				var output = VariableParser.Parse(input);
			}
			catch (ArgumentException ex)
			{
				exc = ex.Message;
			}
			Approvals.Verify("Exception message: " + exc);
		}
	}
}
