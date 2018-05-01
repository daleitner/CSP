using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSP
{
	public static class DomainParser
	{
		public static List<Domain> Parse(string input)
		{
			var domainStrings = input.Split(',').ToList();
			for (var i = 0; i < domainStrings.Count; i++)
			{
				domainStrings[i] = domainStrings[i].Trim();
				domainStrings[i] = domainStrings[i].Replace(' ', '_');
				if (IsNumberInterval(domainStrings[i]))
				{
					var interval = domainStrings[i];
					domainStrings.RemoveAt(i);
					i--;
					domainStrings.AddRange(GenerateNumberDomains(interval));
				}
				else if (IsLetterInterval(domainStrings[i]))
				{
					var interval = domainStrings[i];
					domainStrings.RemoveAt(i);
					i--;
					domainStrings.AddRange(GenerateLetterDomains(interval));
				}
			}

			foreach (var domain in domainStrings)
			{
				if (string.IsNullOrEmpty(domain))
					throw new ArgumentException("Domain name must not be empty!");
				var cnt = domainStrings.Count(x => x == domain);
				if (cnt > 1)
					throw new ArgumentException("Domains are not unique!");
			}


			return ToDomains(domainStrings);
		}

		private static List<Domain> ToDomains(List<string> domainStrings)
		{
			var domains = new List<Domain>();
			if (AllDomainsAreNumbers(domainStrings))
			{
				foreach (var domain in domainStrings)
				{
					domains.Add(new Domain(domain, int.Parse(domain)));
				}
			}
			else
			{
				domainStrings.Sort();
				for (var i = 0; i < domainStrings.Count; i++)
				{
					domains.Add(new Domain(domainStrings[i], i));
				}
			}
			return domains;
		}

		private static bool AllDomainsAreNumbers(List<string> domainStrings)
		{
			var pattern = @"^\d+$";
			var regex = new Regex(pattern);
			foreach (var domain in domainStrings)
			{
				if (!regex.IsMatch(domain))
					return false;
			}
			return true;
		}

		private static bool IsNumberInterval(string domain)
		{
			var pattern = @"^\[[0-9]+-[0-9]+\]$";
			var regex = new Regex(pattern);
			return regex.IsMatch(domain);
		}

		private static List<string> GenerateNumberDomains(string interval)
		{
			string interval_without_brackets = interval.Substring(1, interval.Length - 2);
			var split = interval_without_brackets.Split('-');
			int start = int.Parse(split[0]);
			int end = int.Parse(split[1]);
			var domains = new List<string>();
			for (var i = start; i <= end; i++)
			{
				domains.Add(i.ToString());
			}

			return domains;
		}

		private static bool IsLetterInterval(string domain)
		{
			var pattern = @"^\[[A-Z]-[A-Z]\]$";
			var regex = new Regex(pattern);
			return regex.IsMatch(domain);
		}

		private static List<string> GenerateLetterDomains(string interval)
		{
			string interval_without_brackets = interval.Substring(1, interval.Length - 2);
			var split = interval_without_brackets.Split('-');
			char start = split[0][0];
			char end = split[1][0];
			var domains = new List<string>();
			for (int i = start; i <= end; i++)
			{
				domains.Add(Convert.ToChar(i).ToString());
			}

			return domains;
		}
	}
}
