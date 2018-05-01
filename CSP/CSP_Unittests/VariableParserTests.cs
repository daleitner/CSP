using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApprovalTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSP;

namespace CSP_Unittests
{
	[TestClass]
	public class VariableParserTests
	{
		[TestMethod]
		public void VerifyStringWithOneVariable()
		{
			var input = "a";
			var output = VariableParser.Parse(input);
			Approvals.VerifyAll(output, "Variable");
		}
	}
}
