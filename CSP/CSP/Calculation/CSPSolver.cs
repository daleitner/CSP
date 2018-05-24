using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using CSP.Data;

namespace CSP.Calculation
{
	public static class CSPSolver
	{
		public static CSPContainer Solve(List<Variable> variables, List<Domain> domains, List<Constraint> constraints, bool isPairwiseDisjunct, BackgroundWorker worker)
		{
			var notMatched = ConstraintManager.GetNotMatchedConstraints(constraints, isPairwiseDisjunct);
			foreach (var constraint in notMatched)
			{
				constraints.Remove(constraint);
			}

			int level = 2;
			worker.ReportProgress(0);
			do
			{
				level++;
				var circles = ConstraintManager.GetCircles(constraints, level);
				worker.ReportProgress(level*100/constraints.Count);
				var toRemoveConstraints = new List<Constraint>();
				foreach (var circle in circles)
				{
					if (circle.Any(c => toRemoveConstraints.Contains(c)))
						continue;
					var inconsistentConstraint = ConstraintManager.GetInconsistendConstraint(circle, constraints);
					toRemoveConstraints.Add(inconsistentConstraint);
					constraints.Remove(inconsistentConstraint);
				}

				notMatched.AddRange(toRemoveConstraints.Distinct());
			} while (level <= constraints.Count);

			worker.ReportProgress(0);
			var redundandConstraints = ConstraintManager.GetRedundandConstraints(constraints, isPairwiseDisjunct);
			foreach (var redundandConstraint in redundandConstraints)
			{
				constraints.Remove(redundandConstraint);
			}

			SetVariablesWithNoConstraints(variables, domains, constraints, isPairwiseDisjunct, worker);
			SolveCSP(variables, domains, constraints, isPairwiseDisjunct, worker);
			
			foreach (var constraint in constraints)
			{
				if(!IsSatisfied(constraint))
					notMatched.Add(constraint);
			}

			for (var index = 0; index < notMatched.Count; index++)
			{
				var constraint = notMatched[index];
				if (IsSatisfied(constraint))
				{
					notMatched.Remove(constraint);
					index--;
				}
			}

			return new CSPContainer {Assignments = variables, NotMatchedConstraints = notMatched};
		}

		private static void SetVariablesWithNoConstraints(List<Variable> variables, List<Domain> domains, List<Constraint> constraints, bool isPairwiseDisjunct, BackgroundWorker worker)
		{
			foreach (var variable in variables)
			{
				if (constraints.Count(constraint => constraint.X == variable || constraint.Y == variable) > 0)
					continue;

				var maxDomain = domains.First();
				foreach (var domain in domains)
				{
					if (domain.Value > maxDomain.Value)
						maxDomain = domain;
				}

				variable.Value = maxDomain;
				if (isPairwiseDisjunct)
					domains.Remove(maxDomain);
				worker.ReportProgress(variables.Count(x => x.Value != null) * 100 / variables.Count);
			}
		}

		private static bool SolveCSP(List<Variable> variables, List<Domain> domains, List<Constraint> constraints, bool isPairwiseDisjunct, BackgroundWorker worker)
		{
			var unassigned = variables.Where(x => x.Value == null).ToList();
			if (unassigned.Count == 0)
				return true;
			var next = MinimumRemainingValues(unassigned, domains, constraints);
			if (!next.LegalValues.Any())
				return false;

			var domainOrder = LeastConstrainingValueOrder(next, domains, constraints, isPairwiseDisjunct);
			foreach (var domain in domainOrder)
			{
				next.Variable.Value = domain;
				if (isPairwiseDisjunct)
					domains.Remove(domain);
				worker.ReportProgress(variables.Count(x => x.Value != null)*100/variables.Count);
				if (IsConsistent(constraints))
				{
					var isSuccess = SolveCSP(variables, domains, constraints, isPairwiseDisjunct, worker);
					if (isSuccess)
						return true;
				}
				next.Variable.Value = null;
				if(isPairwiseDisjunct)
					domains.Add(domain);
			}
			return false;
		}

		private static MrvResult MinimumRemainingValues(List<Variable> unassignedVariables, List<Domain> domains, List<Constraint> constraints)
		{
			var legalValuesDict = new Dictionary<Variable, List<Domain>>();
			foreach (var variable in unassignedVariables)
			{
				var cnt = GetLegalValues(variable, domains, constraints);
				if (!cnt.Any())
					return new MrvResult {Variable = variable, LegalValues = cnt};
				legalValuesDict.Add(variable, cnt);
			}

			var min = legalValuesDict[unassignedVariables.First()].Count;
			var mrv = new MrvResult {Variable = unassignedVariables.First(), LegalValues = legalValuesDict[unassignedVariables.First()]};
			var allEqual = true;
			foreach(var unassignedVariable in unassignedVariables)
			{
				var legalValues = legalValuesDict[unassignedVariable];
				if (legalValues.Count < min)
				{
					min = legalValues.Count;
					mrv.Variable = unassignedVariable;
					mrv.LegalValues = legalValues;
					allEqual = false;
				}
				else if (legalValues.Count > min)
					allEqual = false;
			}

			if (allEqual)
			{
				mrv.Variable = GetVariableWithMostConstraints(unassignedVariables, constraints);
				mrv.LegalValues = legalValuesDict[mrv.Variable];
			}

			return mrv;
		}

		private static List<Domain> GetLegalValues(Variable variable, List<Domain> domains, List<Constraint> constraints)
		{
			var legalValues = new List<Domain>();
			var subset = constraints.Where(x => x.X == variable || x.Y == variable).ToList();
			foreach (var domain in domains)
			{
				variable.Value = domain;
				if (IsConsistent(subset))
					legalValues.Add(domain);
			}
			variable.Value = null;
			return legalValues;
		}

		private static bool IsConsistent(List<Constraint> constraints)
		{
			return constraints.All(IsSatisfied);
		}

		private static bool IsSatisfied(Constraint constraint)
		{
			if (constraint.X.Value == null || constraint.Y.Value == null)
				return true;

			switch (constraint.Comparator)
			{
				case CompareEnum.Default:
					throw new ArgumentException("Comperator must not be default");
				case CompareEnum.Equals:
					return constraint.X.Value.Value == constraint.Y.Value.Value;
				case CompareEnum.NotEquals:
					return constraint.X.Value.Value != constraint.Y.Value.Value;
				case CompareEnum.Greater:
					return constraint.X.Value.Value > constraint.Y.Value.Value;
				case CompareEnum.Smaller:
					return constraint.X.Value.Value < constraint.Y.Value.Value;
				case CompareEnum.GreaterOrEquals:
					return constraint.X.Value.Value >= constraint.Y.Value.Value;
				case CompareEnum.SmallerOrEquals:
					return constraint.X.Value.Value <= constraint.Y.Value.Value;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private static Variable GetVariableWithMostConstraints(List<Variable> unassigned, List<Constraint> constraints)
		{
			var max = -1;
			Variable result = null;
			foreach (var variable in unassigned)
			{
				var count = 0;
				foreach (var constraint in constraints)
				{
					if (constraint.X == variable || constraint.Y == variable)
						count++;
				}

				if (count > max)
				{
					max = count;
					result = variable;
				}
			}

			return result;
		}

		private static List<Domain> LeastConstrainingValueOrder(MrvResult next, List<Domain> domains, List<Constraint> constraints, bool isPairwiseDisjunct)
		{
			Variable act = next.Variable;
			var neighbours = GetNeighbours(act, constraints).Where(x => x.Value == null).ToList();
			List<Domain> result = new List<Domain>();


			var tmp = new List<Domain>(next.LegalValues);
			while (tmp.Count > 0)
			{
				Domain best = tmp.FirstOrDefault();
				Dictionary<Variable, int> constrainingValues = new Dictionary<Variable, int>();
				foreach (var neighbour in neighbours)
				{
					constrainingValues.Add(neighbour, 0);
				}
				
				foreach (var domain in tmp)
				{
					act.Value = domain;
					if (isPairwiseDisjunct)
						domains.Remove(domain);
					var cnt = CountConstrainingValues(neighbours, domains, constraints);
					if (IsBetter(cnt, constrainingValues))
					{
						constrainingValues = cnt;
						best = domain;
					}
					if(isPairwiseDisjunct)
						domains.Add(domain);
				}

				if (!constrainingValues.ContainsValue(0))
				{
					result.Add(best);
				}
				tmp.Remove(best);
			}

			act.Value = null;
			return result;
		}

		private static bool IsBetter(Dictionary<Variable, int> cnt, Dictionary<Variable, int> constrainingValues)
		{
			var atLeastOneBetter = false;
			foreach (var variable in cnt.Keys)
			{
				if (cnt[variable] < constrainingValues[variable])
					return false;
				if (cnt[variable] > constrainingValues[variable])
					atLeastOneBetter = true;
			}

			return atLeastOneBetter;
		}

		private static Dictionary<Variable, int> CountConstrainingValues(List<Variable> neighbours, List<Domain> domains, List<Constraint> constraints)
		{
			var cnt = new Dictionary<Variable, int>();
			foreach (var neighbour in neighbours)
			{
				cnt.Add(neighbour, GetLegalValues(neighbour, domains, constraints).Count);
			}

			return cnt;
		}

		private static List<Variable> GetNeighbours(Variable variable, List<Constraint> constraints)
		{
			var subset = constraints.Where(x => x.X == variable || x.Y == variable);
			var neighbours = new List<Variable>();
			foreach (var constraint in subset)
			{
				if (constraint.X == variable && !neighbours.Contains(constraint.Y))
					neighbours.Add(constraint.Y);
				else if (constraint.Y == variable && !neighbours.Contains(constraint.X))
					neighbours.Add(constraint.X);
			}
			return neighbours;
		}
	}
}
