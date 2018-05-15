using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSP.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSP_Unittests
{
	[TestClass]
	public class ConstraintTests
	{
		[TestMethod]
		public void WhenConstraintHasSameProperties_ThenIsSameShouldBeTrue()
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
				new Constraint(1, variables[0], CompareEnum.NotEquals, variables[1]),
				new Constraint(2, variables[0], CompareEnum.NotEquals, variables[1])
			};
			Assert.IsTrue(constraints[0].IsSame(constraints[1]));
		}

		[TestMethod]
		public void WhenConstraintHasDifferentX_ThenIsSameShouldBeFalse()
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
				new Constraint(1, variables[0], CompareEnum.NotEquals, variables[1]),
				new Constraint(2, variables[2], CompareEnum.NotEquals, variables[1])
			};
			Assert.IsFalse(constraints[0].IsSame(constraints[1]));
		}

		[TestMethod]
		public void WhenConstraintHasDifferentY_ThenIsSameShouldBeFalse()
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
				new Constraint(1, variables[0], CompareEnum.NotEquals, variables[1]),
				new Constraint(2, variables[0], CompareEnum.NotEquals, variables[2])
			};
			Assert.IsFalse(constraints[0].IsSame(constraints[1]));
		}

		[TestMethod]
		public void WhenConstraintHasDifferentComparator_ThenIsSameShouldBeFalse()
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
				new Constraint(1, variables[0], CompareEnum.NotEquals, variables[1]),
				new Constraint(2, variables[0], CompareEnum.Equals, variables[1])
			};
			Assert.IsFalse(constraints[0].IsSame(constraints[1]));
		}

		[TestMethod]
		public void WhenConstraintIsSameButTurned_givenEquals_ThenIsSameShouldBeTrue()
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
				new Constraint(1, variables[0], CompareEnum.Equals, variables[1]),
				new Constraint(2, variables[1], CompareEnum.Equals, variables[0])
			};
			Assert.IsTrue(constraints[0].IsSame(constraints[1]));
		}

		[TestMethod]
		public void WhenConstraintIsSameButTurned_givenNotEquals_ThenIsSameShouldBeTrue()
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
				new Constraint(1, variables[0], CompareEnum.NotEquals, variables[1]),
				new Constraint(2, variables[1], CompareEnum.NotEquals, variables[0])
			};
			Assert.IsTrue(constraints[0].IsSame(constraints[1]));
		}

		[TestMethod]
		public void WhenConstraintIsTurned_givenGreater_ThenIsSameShouldBeTrue()
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
				new Constraint(2, variables[1], CompareEnum.Smaller, variables[0])
			};
			Assert.IsTrue(constraints[0].IsSame(constraints[1]));
		}

		[TestMethod]
		public void WhenConstraintIsTurned_givenSmaller_ThenIsSameShouldBeTrue()
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
				new Constraint(1, variables[0], CompareEnum.Smaller, variables[1]),
				new Constraint(2, variables[1], CompareEnum.Greater, variables[0])
			};
			Assert.IsTrue(constraints[0].IsSame(constraints[1]));
		}

		[TestMethod]
		public void WhenConstraintIsTurned_givenSmallerEquals_ThenIsSameShouldBeTrue()
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
				new Constraint(1, variables[0], CompareEnum.SmallerOrEquals, variables[1]),
				new Constraint(2, variables[1], CompareEnum.GreaterOrEquals, variables[0])
			};
			Assert.IsTrue(constraints[0].IsSame(constraints[1]));
		}

		[TestMethod]
		public void WhenConstraintIsTurned_givenGreaterEquals_ThenIsSameShouldBeTrue()
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
				new Constraint(1, variables[0], CompareEnum.GreaterOrEquals, variables[1]),
				new Constraint(2, variables[1], CompareEnum.SmallerOrEquals, variables[0])
			};
			Assert.IsTrue(constraints[0].IsSame(constraints[1]));
		}
	}
}
