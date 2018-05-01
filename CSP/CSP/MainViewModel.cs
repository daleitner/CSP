using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Base;

namespace CSP
{
	public class MainViewModel : ViewModelBase
	{
		#region members
		private RelayCommand _applyCommand;
		private RelayCommand _addConstraintCommand;
		private RelayCommand _runCommand;
		private RelayCommand _generateCommand;
		private ObservableCollection<ConstraintViewModel> _constraintItems;
		private ObservableCollection<Variable> _assignments;
		private ObservableCollection<Constraint> _notMatchedConstraints;
		private List<Variable> _allVariables;
		private List<Domain> _allDomains;
		private string _variableString = "";
		private string _domainString = "";
		private string _info = "";
		#endregion

		#region ctors

		#endregion

		#region properties
		public ICommand ApplyCommand
		{
			get
			{
				return _applyCommand ?? (_applyCommand = new RelayCommand(
					       param => Apply()
				       ));
			}
		}

		public ICommand GenerateCommand
		{
			get
			{
				return _generateCommand ?? (_generateCommand = new RelayCommand(
					       param => GenerateConstraints(),
						   param => _allVariables != null
				       ));
			}
		}

		public ICommand AddConstraintCommand
		{
			get
			{
				return _addConstraintCommand ?? (_addConstraintCommand = new RelayCommand(
					       param => AddConstraint()
				       ));
			}
		}

		public ICommand RunCommand
		{
			get
			{
				return _runCommand ?? (_runCommand = new RelayCommand(
					       param => Run()
				       ));
			}
		}

		public ObservableCollection<ConstraintViewModel> ConstraintItems
		{
			get
			{
				return _constraintItems;
			}
			set
			{
				_constraintItems = value;
				OnPropertyChanged("ConstraintItems");
			}
		}

		public ObservableCollection<Variable> Assignments
		{
			get
			{
				return _assignments;
			}
			set
			{
				_assignments = value;
				OnPropertyChanged("Assignments");
			}
		}

		public ObservableCollection<Constraint> NotMatchedConstraints
		{
			get
			{
				return _notMatchedConstraints;
			}
			set
			{
				_notMatchedConstraints = value;
				OnPropertyChanged("NotMatchedConstraints");
			}
		}

		public string VariableString
		{
			get
			{
				return _variableString;
			}
			set
			{
				_variableString = value;
				OnPropertyChanged("VariableString");
			}
		}

		public string DomainString
		{
			get
			{
				return _domainString;
			}
			set
			{
				_domainString = value;
				OnPropertyChanged("DomainString");
			}
		}

		public string Info
		{
			get
			{
				return _info;
			}
			set
			{
				_info = value;
				OnPropertyChanged("Info");
			}
		}
		#endregion

		#region private methods
		private void Apply()
		{
			try
			{
				_allVariables = VariableParser.Parse(VariableString);
				_allDomains = DomainParser.Parse(DomainString);
				ConstraintItems = new ObservableCollection<ConstraintViewModel>();
				AddConstraintViewModel(new ConstraintViewModel(_allVariables));
			}
			catch (ArgumentException e)
			{
				Info = e.Message;
			}
		}

		private void AddConstraint()
		{
			AddConstraintViewModel(new ConstraintViewModel(_allVariables));
		}

		private void Run()
		{
			try
			{
				var constraints = new List<Constraint>();
				foreach (var item in ConstraintItems)
				{
					constraints.Add(new Constraint(item.SelectedVariable1, item.SelectedComparator, item.SelectedVariable2));
				}

				var result = CSPSolver.Solve(_allVariables, _allDomains, constraints);
				Assignments = new ObservableCollection<Variable>(result.Assignments);
				NotMatchedConstraints = new ObservableCollection<Constraint>(result.NotMatchedConstraints);
			}
			catch (Exception e)
			{
				Info = e.Message;
			}
		}

		private void AddConstraintViewModel(ConstraintViewModel viewModel)
		{
			viewModel.RemoveConstraint += ViewModel_RemoveConstraint;
			ConstraintItems.Add(viewModel);
		}

		private void ViewModel_RemoveConstraint(ConstraintViewModel constraint)
		{
			ConstraintItems.Remove(constraint);
		}

		private void GenerateConstraints()
		{
			for (int i = 0; i < _allVariables.Count; i++)
			{
				for (int j = i+1; j < _allVariables.Count; j++)
				{
					var constraint = new ConstraintViewModel(_allVariables)
					{
						SelectedVariable1 = _allVariables[i],
						SelectedVariable2 = _allVariables[j],
						SelectedComparator = CompareEnum.NotEquals
					};
					AddConstraintViewModel(constraint);
				}
			}
		}
		#endregion

		#region public methods
		#endregion
	}
}
