using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSP
{
	public static class VariableParser
	{
		public static List<string> Parse(string input)
		{
			var variables = input.Split(',').ToList();
			for (var i = 0; i < variables.Count; i++)
			{
				variables[i] = variables[i].Trim();
				variables[i] = variables[i].Replace(' ', '_');
			}

			foreach (var variable in variables)
			{
				if(string.IsNullOrEmpty(variable))
					throw new ArgumentException("Variable name must not be empty!");
				var cnt = variables.Count(x => x == variable);
				if(cnt > 1)
					throw new ArgumentException("Variables are not unique!");
			}
			return variables;
		}
	}
}
