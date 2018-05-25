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
			var dict = new Dictionary<Variable, List<Domain>>();
			foreach (var variable in variables.Where(x => x.Value == null))
			{
				dict.Add(variable, new List<Domain>(domains));
			}

			if (!SolveCSP(dict, variables.Count, constraints, isPairwiseDisjunct, worker))
				return new CSPContainer {Assignments = new List<Variable>(), NotMatchedConstraints = new List<Constraint>()};
			
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

		private static bool SolveCSP(Dictionary<Variable, List<Domain>> variables, int totalVariables, List<Constraint> constraints, bool isPairwiseDisjunct, BackgroundWorker worker)
		{
			if (variables.Count == 0)
				return true;
			UpdateRemainingValues(variables, constraints, isPairwiseDisjunct);
			var next = MinimumRemainingValues(variables, constraints);
			if (!next.LegalValues.Any())
				return false;

			var domainOrder = LeastConstrainingValueOrder(next, variables, constraints, isPairwiseDisjunct);
			foreach (var domain in domainOrder)
			{
				next.Variable.Value = domain;
				if (isPairwiseDisjunct)
					variables.Keys.ToList().ForEach(x => variables[x].Remove(domain));
				var variableDomains = variables[next.Variable];
				variables.Remove(next.Variable);

				worker.ReportProgress((totalVariables - variables.Count)*100/totalVariables);
				if (IsConsistent(constraints))
				{
					var tmpVariables = new Dictionary<Variable, List<Domain>>();
					foreach (var variable in variables.Keys)
					{
						tmpVariables.Add(variable, new List<Domain>(variables[variable]));
					}
					var isSuccess = SolveCSP(tmpVariables, totalVariables, constraints, isPairwiseDisjunct, worker);
					if (isSuccess)
						return true;
				}
				variables.Add(next.Variable, variableDomains);
				if (isPairwiseDisjunct)
					variables.Keys.ToList().ForEach(x => variables[x].Add(domain));
				next.Variable.Value = null;
			}
			return false;
		}

		private static void UpdateRemainingValues(Dictionary<Variable, List<Domain>> variables, List<Constraint> constraints, bool isPairwiseDisjunct)
		{
			foreach (var variable in variables.Keys)
			{
				for (var index = 0; index < variables[variable].Count; index++)
				{
					var domain = variables[variable][index];
					variable.Value = domain;
					if (!IsConsistent(constraints))
					{
						variables[variable].Remove(domain);
						index--;
					}
					variable.Value = null;
				}
			}
		}

		private static MrvResult MinimumRemainingValues(Dictionary<Variable, List<Domain>> unassignedVariables, List<Constraint> constraints)
		{
			var mrv = new MrvResult {Variable = unassignedVariables.Keys.First(), LegalValues = unassignedVariables[unassignedVariables.Keys.First()]};
			var min = unassignedVariables[unassignedVariables.Keys.First()].Count;
			var mrvVariables = new List<Variable>();
			foreach(var unassignedVariable in unassignedVariables.Keys)
			{
				var legalValues = unassignedVariables[unassignedVariable];
				if (legalValues.Count < min)
				{
					min = legalValues.Count;
					mrv.Variable = unassignedVariable;
					mrv.LegalValues = legalValues;
					mrvVariables = new List<Variable> {unassignedVariable};
				}
				else if (legalValues.Count == min)
					mrvVariables.Add(unassignedVariable);
			}

			mrv.Variable = GetVariableWithMostConstraints(mrvVariables, constraints);
			mrv.LegalValues = unassignedVariables[mrv.Variable];

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

		private static List<Domain> LeastConstrainingValueOrder(MrvResult next, Dictionary<Variable, List<Domain>> variables, List<Constraint> constraints, bool isPairwiseDisjunct)
		{
			Variable act = next.Variable;
			var neighbournodes = GetNeighbours(act, constraints).Where(x => x.Value == null).ToList();
			var neighbours = new Dictionary<Variable, List<Domain>>();
			var constrainingValues = new Dictionary<Variable, int>();
			foreach (var node in neighbournodes)
			{
				neighbours.Add(node, new List<Domain>(variables[node]));
				constrainingValues.Add(node, 0);
			}
			List<Domain> result = new List<Domain>();

			var tmp = new List<Domain>(next.LegalValues);
			while (tmp.Count > 0)
			{
				Domain best = tmp.FirstOrDefault();
				constrainingValues.Keys.ToList().ForEach(x => constrainingValues[x] = 0);
				foreach (var domain in tmp)
				{
					act.Value = domain;
					if (isPairwiseDisjunct) 
						neighbours.Keys.ToList().ForEach(x => neighbours[x].Remove(domain));
					var cnt = CountConstrainingValues(neighbours, constraints);
					if (IsBetter(cnt, constrainingValues))
					{
						constrainingValues = cnt;
						best = domain;
					}
					if(isPairwiseDisjunct)
						neighbours.Keys.ToList().ForEach(x => neighbours[x].Add(domain));
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
			var tmp = new List<int>(cnt.Values);
			tmp.Sort();
			var tmp2 = new List<int>(constrainingValues.Values);
			tmp2.Sort();
			for (var i = 0; i < tmp.Count; i++)
			{
				if (tmp[i] > tmp2[i])
					return true;
				if (tmp[i] < tmp2[i])
					return false;
			}
			return false;
		}

		private static Dictionary<Variable, int> CountConstrainingValues(Dictionary<Variable, List<Domain>> neighbours, List<Constraint> constraints)
		{
			var cnt = new Dictionary<Variable, int>();
			foreach (var neighbour in neighbours.Keys)
			{
				cnt.Add(neighbour, GetLegalValues(neighbour, neighbours[neighbour], constraints).Count);
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
