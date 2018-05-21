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
			var constraints = new List<Constraint>
			{
				new Constraint(1, variables[0], CompareEnum.Equals, variables[1])
			};
			Approvals.Verify(ConstraintManager.GetNotMatchedConstraints(constraints, true).ToPrettyString());
		}
		#endregion
		#region getcircles

		[TestMethod]
		public void When_A_g_B_and_B_g_C_and_C_g_A_thenDetectCircle()
		{
			var variables = new List<Variable>()
			{
				new Variable("A", null),
				new Variable("B", null),
				new Variable("C", null)
			};
			var constraints = new List<Constraint>
			{
				new Constraint(1, variables[0], CompareEnum.Greater, variables[1]),
				new Constraint(2, variables[1], CompareEnum.Greater, variables[2]),
				new Constraint(3, variables[2], CompareEnum.Greater, variables[0])
			};
			Approvals.Verify(ConstraintManager.GetCircles(constraints, null).ToPrettyString());
		}

		[TestMethod]
		public void When_A_g_B_and_B_g_C_and_A_s_C_thenDetectCircle()
		{
			var variables = new List<Variable>()
			{
				new Variable("A", null),
				new Variable("B", null),
				new Variable("C", null)
			};
			var constraints = new List<Constraint>
			{
				new Constraint(1, variables[0], CompareEnum.Greater, variables[1]),
				new Constraint(2, variables[1], CompareEnum.Greater, variables[2]),
				new Constraint(3, variables[0], CompareEnum.Smaller, variables[2])
			};
			Approvals.Verify(ConstraintManager.GetCircles(constraints, null).ToPrettyString());
		}

		[TestMethod]
		public void WhenCircleWithFourNodes_ThenDetectCircle()
		{
			var variables = new List<Variable>()
			{
				new Variable("A", null),
				new Variable("B", null),
				new Variable("C", null),
				new Variable("D", null)
			};

			var constraints = new List<Constraint>
			{
				new Constraint(1, variables[0], CompareEnum.Smaller, variables[2]),
				new Constraint(2, variables[1], CompareEnum.Greater, variables[2]),
				new Constraint(3, variables[1], CompareEnum.Smaller, variables[3]),
				new Constraint(4, variables[0], CompareEnum.Greater, variables[3])
			};
			Approvals.Verify(ConstraintManager.GetCircles(constraints, null).ToPrettyString());
		}

		#endregion

		#region detect inconsistent Constraint

		[TestMethod]
		public void When_detection_not_possible_ThenLastCircleElementIsInconsistent()
		{
			var variables = new List<Variable>()
			{
				new Variable("A", null),
				new Variable("B", null),
				new Variable("C", null),
				new Variable("D", null)
			};

			var constraints = new List<Constraint>
			{
				new Constraint(1, variables[0], CompareEnum.Smaller, variables[2]),
				new Constraint(2, variables[1], CompareEnum.Greater, variables[2]),
				new Constraint(3, variables[1], CompareEnum.Smaller, variables[3]),
				new Constraint(4, variables[0], CompareEnum.Greater, variables[3])
			};
			var circle = ConstraintManager.GetCircles(constraints, null).First();

			Approvals.Verify(ConstraintManager.GetInconsistendConstraint(circle, constraints).ToPrettyString());
		}

		[TestMethod]
		public void When_B_g_C_has_one_contradiction_Then_B_g_C_IsInconsistent()
		{
			var variables = new List<Variable>()
			{
				new Variable("A", null),
				new Variable("B", null),
				new Variable("C", null),
				new Variable("D", null),
				new Variable("E", null)
			};

			var constraints = new List<Constraint>
			{
				new Constraint(1, variables[0], CompareEnum.Smaller, variables[2]),
				new Constraint(2, variables[1], CompareEnum.Greater, variables[2]),
				new Constraint(3, variables[1], CompareEnum.Smaller, variables[3]),
				new Constraint(4, variables[0], CompareEnum.Greater, variables[3]),
				new Constraint(5, variables[1], CompareEnum.Smaller, variables[4]),
				new Constraint(6, variables[4], CompareEnum.Smaller, variables[2])
			};
			var circle = ConstraintManager.GetCircles(constraints, null).First();

			Approvals.Verify(ConstraintManager.GetInconsistendConstraint(circle, constraints).ToPrettyString());
		}

		[TestMethod]
		public void When_A_g_D_is_correct_Then_B_s_D_IsInconsistent()
		{
			var variables = new List<Variable>()
			{
				new Variable("A", null),
				new Variable("B", null),
				new Variable("C", null),
				new Variable("D", null),
				new Variable("E", null)
			};

			var constraints = new List<Constraint>
			{
				new Constraint(1, variables[0], CompareEnum.Smaller, variables[2]),
				new Constraint(2, variables[1], CompareEnum.Greater, variables[2]),
				new Constraint(3, variables[1], CompareEnum.Smaller, variables[3]),
				new Constraint(4, variables[0], CompareEnum.Greater, variables[3]),
				new Constraint(5, variables[0], CompareEnum.Greater, variables[4]),
				new Constraint(6, variables[4], CompareEnum.Greater, variables[3])
			};
			var circle = ConstraintManager.GetCircles(constraints, null).First();

			Approvals.Verify(ConstraintManager.GetInconsistendConstraint(circle, constraints).ToPrettyString());
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

		[TestMethod]
		public void WhenA_is_B_and_B_is_C_and_A_is_C_ThenA_is_C_isRedundand()
		{
			var variables = new List<Variable>()
			{
				new Variable("A", null),
				new Variable("B", null),
				new Variable("C", null)
			};
			var domains = new List<Domain>
			{
				new Domain("red", 1),
				new Domain("blue", 2)
			};
			var constraints = new List<Constraint>
			{
				new Constraint(1, variables[0], CompareEnum.Equals, variables[1]),
				new Constraint(2, variables[1], CompareEnum.Equals, variables[2]),
				new Constraint(2, variables[0], CompareEnum.Equals, variables[2])
			};
			Approvals.Verify(ConstraintManager.GetRedundandConstraints(constraints, false).ToPrettyString());
		}

		[TestMethod]
		public void WhenA_is_B_and_B_is_C_and_C_is_A_ThenC_is_A_isRedundand()
		{
			var variables = new List<Variable>()
			{
				new Variable("A", null),
				new Variable("B", null),
				new Variable("C", null)
			};
			var domains = new List<Domain>
			{
				new Domain("red", 1),
				new Domain("blue", 2)
			};
			var constraints = new List<Constraint>
			{
				new Constraint(1, variables[0], CompareEnum.Equals, variables[1]),
				new Constraint(2, variables[1], CompareEnum.Equals, variables[2]),
				new Constraint(2, variables[2], CompareEnum.Equals, variables[0])
			};
			Approvals.Verify(ConstraintManager.GetRedundandConstraints(constraints, false).ToPrettyString());
		}

		[TestMethod]
		public void WhenB_is_A_and_C_is_B_and_C_is_A_ThenC_is_A_isRedundand()
		{
			var variables = new List<Variable>()
			{
				new Variable("A", null),
				new Variable("B", null),
				new Variable("C", null)
			};
			var domains = new List<Domain>
			{
				new Domain("red", 1),
				new Domain("blue", 2)
			};
			var constraints = new List<Constraint>
			{
				new Constraint(1, variables[1], CompareEnum.Equals, variables[0]),
				new Constraint(2, variables[2], CompareEnum.Equals, variables[1]),
				new Constraint(2, variables[2], CompareEnum.Equals, variables[0])
			};
			Approvals.Verify(ConstraintManager.GetRedundandConstraints(constraints, false).ToPrettyString());
		}

		[TestMethod]
		public void WhenChainA_to_D_and_A_is_D_ThenA_is_D_isRedundand()
		{
			var variables = new List<Variable>()
			{
				new Variable("A", null),
				new Variable("B", null),
				new Variable("C", null),
				new Variable("D", null)
			};
			var domains = new List<Domain>
			{
				new Domain("red", 1),
				new Domain("blue", 2)
			};
			var constraints = new List<Constraint>
			{
				new Constraint(1, variables[0], CompareEnum.Equals, variables[1]),
				new Constraint(2, variables[1], CompareEnum.Equals, variables[2]),
				new Constraint(3, variables[2], CompareEnum.Equals, variables[3]),
				new Constraint(4, variables[0], CompareEnum.Equals, variables[3])
			};
			Approvals.Verify(ConstraintManager.GetRedundandConstraints(constraints, false).ToPrettyString());
		}

		[TestMethod]
		public void WhenInvertedChainA_to_D_and_A_is_D_ThenA_is_D_isRedundand()
		{
			var variables = new List<Variable>()
			{
				new Variable("A", null),
				new Variable("B", null),
				new Variable("C", null),
				new Variable("D", null)
			};
			var domains = new List<Domain>
			{
				new Domain("red", 1),
				new Domain("blue", 2)
			};
			var constraints = new List<Constraint>
			{
				new Constraint(1, variables[0], CompareEnum.Equals, variables[1]),
				new Constraint(2, variables[2], CompareEnum.Equals, variables[1]),
				new Constraint(3, variables[2], CompareEnum.Equals, variables[3]),
				new Constraint(4, variables[0], CompareEnum.Equals, variables[3])
			};
			Approvals.Verify(ConstraintManager.GetRedundandConstraints(constraints, false).ToPrettyString());
		}

		[TestMethod]
		public void WhenA_g_B_and_B_g_C_and_A_g_C_ThenA_g_C_isRedundand()
		{
			var variables = new List<Variable>()
			{
				new Variable("A", null),
				new Variable("B", null),
				new Variable("C", null)
			};
			var domains = new List<Domain>
			{
				new Domain("red", 1),
				new Domain("blue", 2)
			};
			var constraints = new List<Constraint>
			{
				new Constraint(1, variables[0], CompareEnum.Greater, variables[1]),
				new Constraint(2, variables[1], CompareEnum.Greater, variables[2]),
				new Constraint(2, variables[0], CompareEnum.Greater, variables[2])
			};
			Approvals.Verify(ConstraintManager.GetRedundandConstraints(constraints, false).ToPrettyString());
		}

		[TestMethod]
		public void WhenA_g_B_and_B_g_C_and_C_s_A_ThenC_s_A_isRedundand()
		{
			var variables = new List<Variable>()
			{
				new Variable("A", null),
				new Variable("B", null),
				new Variable("C", null)
			};
			var domains = new List<Domain>
			{
				new Domain("red", 1),
				new Domain("blue", 2)
			};
			var constraints = new List<Constraint>
			{
				new Constraint(1, variables[0], CompareEnum.Greater, variables[1]),
				new Constraint(2, variables[1], CompareEnum.Greater, variables[2]),
				new Constraint(2, variables[2], CompareEnum.Smaller, variables[0])
			};
			Approvals.Verify(ConstraintManager.GetRedundandConstraints(constraints, false).ToPrettyString());
		}

		[TestMethod]
		public void WhenB_s_A_and_C_s_B_and_C_s_A_ThenC_s_A_isRedundand()
		{
			var variables = new List<Variable>()
			{
				new Variable("A", null),
				new Variable("B", null),
				new Variable("C", null)
			};
			var domains = new List<Domain>
			{
				new Domain("red", 1),
				new Domain("blue", 2)
			};
			var constraints = new List<Constraint>
			{
				new Constraint(1, variables[1], CompareEnum.Smaller, variables[0]),
				new Constraint(2, variables[2], CompareEnum.Smaller, variables[1]),
				new Constraint(2, variables[2], CompareEnum.Smaller, variables[0])
			};
			Approvals.Verify(ConstraintManager.GetRedundandConstraints(constraints, false).ToPrettyString());
		}

		[TestMethod]
		public void WhenChainA_to_D_and_A_g_D_ThenA_g_D_isRedundand()
		{
			var variables = new List<Variable>()
			{
				new Variable("A", null),
				new Variable("B", null),
				new Variable("C", null),
				new Variable("D", null)
			};
			var domains = new List<Domain>
			{
				new Domain("red", 1),
				new Domain("blue", 2)
			};
			var constraints = new List<Constraint>
			{
				new Constraint(1, variables[0], CompareEnum.Greater, variables[1]),
				new Constraint(2, variables[1], CompareEnum.Greater, variables[2]),
				new Constraint(3, variables[2], CompareEnum.Greater, variables[3]),
				new Constraint(4, variables[0], CompareEnum.Greater, variables[3])
			};
			Approvals.Verify(ConstraintManager.GetRedundandConstraints(constraints, false).ToPrettyString());
		}

		[TestMethod]
		public void WhenInvertedChainA_to_D_and_A_g_D_ThenA_g_D_isRedundand()
		{
			var variables = new List<Variable>()
			{
				new Variable("A", null),
				new Variable("B", null),
				new Variable("C", null),
				new Variable("D", null)
			};
			var domains = new List<Domain>
			{
				new Domain("red", 1),
				new Domain("blue", 2)
			};
			var constraints = new List<Constraint>
			{
				new Constraint(1, variables[0], CompareEnum.Greater, variables[1]),
				new Constraint(2, variables[2], CompareEnum.Smaller, variables[1]),
				new Constraint(3, variables[2], CompareEnum.Greater, variables[3]),
				new Constraint(4, variables[0], CompareEnum.Greater, variables[3])
			};
			Approvals.Verify(ConstraintManager.GetRedundandConstraints(constraints, false).ToPrettyString());
		}

		[TestMethod]
		public void WhenA_g_B_and_B_g_C_and_A_not_C_Then_nothing_isRedundand()
		{
			var variables = new List<Variable>()
			{
				new Variable("A", null),
				new Variable("B", null),
				new Variable("C", null)
			};
			var domains = new List<Domain>
			{
				new Domain("red", 1),
				new Domain("blue", 2)
			};
			var constraints = new List<Constraint>
			{
				new Constraint(1, variables[0], CompareEnum.Greater, variables[1]),
				new Constraint(2, variables[1], CompareEnum.Greater, variables[2]),
				new Constraint(2, variables[0], CompareEnum.NotEquals, variables[2])
			};
			Approvals.Verify(ConstraintManager.GetRedundandConstraints(constraints, false).ToPrettyString());
		}
		#endregion
	}
}
