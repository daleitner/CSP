using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApprovalTests;
using ApprovalTests.Reporters;
using Base;
using CSP.Calculation;
using CSP.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSP_Unittests
{
	[TestClass]
	[UseReporter(typeof(WinMergeReporter))]
	public class ConstraintManagerTests
	{
		#region not match
		[TestMethod]
		public void WhenEqualsConstraintExists_givenPairwiseDisjunctive_thenConstraintNotMatch()
		{
			var variables = new List<Variable>()
			{
				new Variable("A", null),
				new Variable("B", null)
			};
			var domains = new List<Domain>
			{
				new Domain("red", 1),
				new Domain("blue", 2)
			};
			var constraints = new List<Constraint>
			{
				new Constraint(1, variables[0], CompareEnum.Equals, variables[1])
			};
			Approvals.Verify(ConstraintManager.GetNotMatchedConstraints(constraints, true).ToPrettyString());
		}
		#endregion

		#region redundance
		[TestMethod]
		public void WhenNotEqualsConstraintExists_givenPairwiseDisjunctive_thenConstraintIsRedundand()
		{
			var variables = new List<Variable>()
			{
				new Variable("A", null),
				new Variable("B", null)
			};
			var domains = new List<Domain>
			{
				new Domain("red", 1),
				new Domain("blue", 2)
			};
			var constraints = new List<Constraint>
			{
				new Constraint(1, variables[0], CompareEnum.NotEquals, variables[1])
			};
			Approvals.Verify(ConstraintManager.GetRedundandConstraints(constraints, true).ToPrettyString());
		}

		[TestMethod]
		public void WhenConstraintIsSame_ThenConstraintIsRedundand()
		{
			var variables = new List<Variable>()
			{
				new Variable("A", null),
				new Variable("B", null)
			};
			var domains = new List<Domain>
			{
				new Domain("red", 1),
				new Domain("blue", 2)
			};
			var constraints = new List<Constraint>
			{
				new Constraint(1, variables[0], CompareEnum.Greater, variables[1]),
				new Constraint(2, variables[0], CompareEnum.Greater, variables[1])
			};
			Approvals.Verify(ConstraintManager.GetRedundandConstraints(constraints, true).ToPrettyString());
		}
		#endregion
	}
}
