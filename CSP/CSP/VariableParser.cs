using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSP
{
	public static class VariableParser
	{
		public static List<Variable> Parse(string input)
		{
			var variableNames = input.Split(',').ToList();
			for (var i = 0; i < variableNames.Count; i++)
			{
				variableNames[i] = variableNames[i].Trim();
				variableNames[i] = variableNames[i].Replace(' ', '_');
			}

			foreach (var variable in variableNames)
			{
				if(string.IsNullOrEmpty(variable))
					throw new ArgumentException("Variable name must not be empty!");
				var cnt = variableNames.Count(x => x == variable);
				if(cnt > 1)
					throw new ArgumentException("Variables are not unique!");
			}

			var variables = new List<Variable>();
			variableNames.ForEach(x => variables.Add(new Variable(x, null)));

			return variables;
		}
	}
}
