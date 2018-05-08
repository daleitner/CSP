using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApprovalTests;
using ApprovalTests.Reporters;
using CSP.Calculation;
using CSP.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSP_Unittests
{
	[TestClass]
	[UseReporter(typeof(WinMergeReporter))]
	public class CSPValidatorTests
	{
		[TestMethod]
		public void GivenAValidCSP_ThenResultShouldBeTrue()
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
				new Domain("green", 2),
				new Domain("blue", 3)
			};
			var constraints = new List<Constraint>
			{
				new Constraint(1, variables[0], CompareEnum.Greater, variables[1]),
				new Constraint(2, variables[1], CompareEnum.Greater, variables[2]),
				new Constraint(3, variables[2], CompareEnum.Smaller, variables[0])
			};
			var isValid = CSPValidator.Validate(variables, domains, constraints, true);
			Assert.IsTrue(isValid);
		}

		[TestMethod]
		public void WhenVariablesPairwiseDisjunctAndTooLessDomains_ThenThrowException()
		{
			try
			{
				CSPValidator.Validate(new List<Variable> { new Variable("a", null), new Variable("b", null) },
					new List<Domain> { new Domain("x", 1) }, null, true);
			}
			catch (Exception e)
			{
				Approvals.Verify(e.Message);
				return;
			}
			Assert.Fail("no exception thrown");
		}

		[TestMethod]
		public void WhenXOfConstraintIsNull_ThenThrowException()
		{
			var variables = new List<Variable>()
			{
				new Variable("A", null)
			};
			var domains = new List<Domain>
			{
				new Domain("red", 1)
			};
			var constraints = new List<Constraint>
			{
				new Constraint(1, null, CompareEnum.Greater, variables[0])
			};

			try
			{
				var isValid = CSPValidator.Validate(variables, domains, constraints, true);
			}
			catch (Exception e)
			{
				Approvals.Verify(e.Message);
				return;
			}
			Assert.Fail();
		}

		[TestMethod]
		public void WhenYOfConstraintIsNull_ThenThrowException()
		{
			var variables = new List<Variable>()
			{
				new Variable("A", null)
			};
			var domains = new List<Domain>
			{
				new Domain("red", 1)
			};
			var constraints = new List<Constraint>
			{
				new Constraint(1, variables[0], CompareEnum.Greater, null)
			};

			try
			{
				var isValid = CSPValidator.Validate(variables, domains, constraints, true);
			}
			catch (Exception e)
			{
				Approvals.Verify(e.Message);
				return;
			}
			Assert.Fail();
		}

		[TestMethod]
		public void WhenComparatorOfConstraintIsDefault_ThenThrowException()
		{
			var variables = new List<Variable>()
			{
				new Variable("A", null)
			};
			var domains = new List<Domain>
			{
				new Domain("red", 1)
			};
			var constraints = new List<Constraint>
			{
				new Constraint(1, variables[0], CompareEnum.Default, variables[0])
			};

			try
			{
				var isValid = CSPValidator.Validate(variables, domains, constraints, true);
			}
			catch (Exception e)
			{
				Approvals.Verify(e.Message);
				return;
			}
			Assert.Fail();
		}

		[TestMethod]
		public void WhenConstraintWithOneVariable_ThenThrowException()
		{
			var variables = new List<Variable>()
			{
				new Variable("A", null)
			};
			var domains = new List<Domain>
			{
				new Domain("red", 1)
			};
			var constraints = new List<Constraint>
			{
				new Constraint(1, variables[0], CompareEnum.NotEquals, variables[0])
			};

			try
			{
				var isValid = CSPValidator.Validate(variables, domains, constraints, true);
			}
			catch (Exception e)
			{
				Approvals.Verify(e.Message);
				return;
			}
			Assert.Fail();
		}
	}
}
