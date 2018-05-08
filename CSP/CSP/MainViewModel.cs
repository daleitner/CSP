using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Base;
using FileIO.FileWorker;
using Microsoft.Win32;

namespace CSP
{
	public class MainViewModel : ViewModelBase
	{
		#region members
		private RelayCommand _applyCommand;
		private RelayCommand _addConstraintCommand;
		private RelayCommand _runCommand;
		private RelayCommand _saveFileCommand;
		private RelayCommand _loadFileCommand;
		private RelayCommand _saveResultCommand;
		private ObservableCollection<ConstraintViewModel> _constraintItems;
		private ObservableCollection<Variable> _assignments;
		private ObservableCollection<Constraint> _notMatchedConstraints;
		private List<Variable> _allVariables;
		private List<Domain> _allDomains;
		private string _variableString = "";
		private string _domainString = "";
		private string _info = "";
		private bool _isPairwiseDisjunct;
		private readonly BackgroundWorker _worker;
		private int _calculationProgress;
		private Visibility _loadVisibility = Visibility.Collapsed;
		private CSPContainer _result;
		#endregion

		#region ctors

		public MainViewModel()
		{
			_constraintItems = new ObservableCollection<ConstraintViewModel>();
			_assignments = new ObservableCollection<Variable>();
			_notMatchedConstraints = new ObservableCollection<Constraint>();
			_worker = new BackgroundWorker()
			{
				WorkerReportsProgress = true,
				WorkerSupportsCancellation = true
			};
			_worker.DoWork += _worker_DoWork;
			_worker.ProgressChanged += _worker_ProgressChanged;
			_worker.RunWorkerCompleted += _worker_RunWorkerCompleted;
		}
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

		public ICommand SaveFileCommand
		{
			get
			{
				return _saveFileCommand ?? (_saveFileCommand = new RelayCommand(
					       param => SaveFile()
				       ));
			}
		}

		public ICommand LoadFileCommand
		{
			get
			{
				return _loadFileCommand ?? (_loadFileCommand = new RelayCommand(
					       param => LoadFile()
				       ));
			}
		}

		public ICommand SaveResultCommand
		{
			get
			{
				return _saveResultCommand ?? (_saveResultCommand = new RelayCommand(
					       param => SaveResult()
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

		public bool IsPairwiseDisjunct
		{
			get { return _isPairwiseDisjunct; }
			set
			{
				_isPairwiseDisjunct = value;
				OnPropertyChanged(nameof(IsPairwiseDisjunct));
			}
		}

		public int CalculationProgress
		{
			get { return _calculationProgress; }
			set
			{
				_calculationProgress = value;
				OnPropertyChanged(nameof(CalculationProgress));
			}
		}

		public Visibility LoadVisibility
		{
			get
			{
				return _loadVisibility;
			}
			set
			{
				_loadVisibility = value;
				OnPropertyChanged(nameof(LoadVisibility));
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
				int i = 0;
				foreach (var item in ConstraintItems)
				{
					constraints.Add(new Constraint(i, item.SelectedVariable1, item.SelectedComparator, item.SelectedVariable2));
					i++;
				}

				foreach (var variable in _allVariables)
				{
					variable.Value = null;
				}

				LoadVisibility = Visibility.Visible;
				if (!_worker.IsBusy)
				{
					_worker.RunWorkerAsync(constraints);
				}
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

		private void SaveFile()
		{
			var dialog = new SaveFileDialog {Filter = "Views (*.csv)|*.csv"};
			if (dialog.ShowDialog() == true)
			{
				var constraints = new List<Constraint>();
				int i = 0;
				foreach (var item in ConstraintItems)
				{
					constraints.Add(new Constraint(i, item.SelectedVariable1, item.SelectedComparator, item.SelectedVariable2));
					i++;
				}

				var fileName = dialog.FileName;
				if(File.Exists(fileName))
					FileWorker.DeleteFile(fileName);
				var file = FileWorker.GetInstance(fileName, true);
				var max = _allVariables.Count;
				if (_allDomains.Count > max)
					max = _allDomains.Count;
				if (constraints.Count > max)
					max = constraints.Count;

				for (i = 0; i < max; i++)
				{
					var builder = new StringBuilder();
					if (_allVariables.Count > i)
						builder.Append(_allVariables[i].Name);
					builder.Append(";");
					if (_allDomains.Count > i)
						builder.Append(_allDomains[i].Name);
					builder.Append(";");
					if (constraints.Count > i)
					{
						builder.Append(constraints[i].X.Name);
						builder.Append(";");
						builder.Append(constraints[i].Comparator);
						builder.Append(";");
						builder.Append(constraints[i].Y.Name);
						builder.Append(";");
					}
					else
					{
						builder.Append(";;;");
					}

					if (i == 0)
						builder.Append(IsPairwiseDisjunct);
					file.WriteLine(builder.ToString());
				}
			}
		}

		private void LoadFile()
		{
			var dialog = new OpenFileDialog {Filter = "Views (*.csv)|*.csv"};
			if (dialog.ShowDialog() == true)
			{
				var lines = FileWorker.ReadFile(dialog.FileName).Replace("\r\n", "\n").Split('\n');
				var variables = "";
				var domains = "";
				var strings = new List<string>();
				foreach (var line in lines.Where(x => !string.IsNullOrEmpty(x)))
				{
					var items = line.Split(';');
					if (!string.IsNullOrEmpty(items[0]))
						variables += items[0] + ",";
					if (!string.IsNullOrEmpty(items[1]))
						domains += items[1] + ",";
					if(items.Length > 3)
						strings.Add(items[2] + ";" + items[3] + ";" + items[4]);
				}

				VariableString = variables.Substring(0, variables.Length - 1);
				DomainString = domains.Substring(0, domains.Length - 1);
				Apply();
				ConstraintItems.RemoveAt(0);

				IsPairwiseDisjunct = bool.Parse(lines.First().Split(';')[5]);
				foreach (var s in strings.Where(x => x != ";;"))
				{
					var items = s.Split(';');
					var constraint = new ConstraintViewModel(_allVariables)
					{
						SelectedVariable1 = _allVariables.FirstOrDefault(x => x.Name == items[0]),
						SelectedComparator = (CompareEnum)Enum.Parse(typeof(CompareEnum), items[1]),
						SelectedVariable2 = _allVariables.FirstOrDefault(x => x.Name == items[2])
					};
					AddConstraintViewModel(constraint);
				}
			}
		}

		private void SaveResult()
		{
			var dialog = new SaveFileDialog { Filter = "Views (*.csv)|*.csv" };
			if (dialog.ShowDialog() == true)
			{
				var fileName = dialog.FileName;
				var file = FileWorker.GetInstance(fileName, true);
				foreach (var assignment in Assignments)
				{
					var builder = new StringBuilder();
					builder.Append(assignment.Name);
					builder.Append(";");
					builder.Append(assignment.Value.Name);
					file.WriteLine(builder.ToString());
				}
			}
		}

		private void _worker_DoWork(object sender, DoWorkEventArgs e)
		{
			var worker = (BackgroundWorker)sender;
			var constraints = (List<Constraint>) e.Argument;
			_result = CSPSolver.Solve(_allVariables, new List<Domain>(_allDomains), constraints, IsPairwiseDisjunct, worker);
		}

		private void _worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			CalculationProgress = e.ProgressPercentage;
		}

		private void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			Assignments = new ObservableCollection<Variable>(_result.Assignments);
			NotMatchedConstraints = new ObservableCollection<Constraint>(_result.NotMatchedConstraints);
			LoadVisibility = Visibility.Collapsed;
			CalculationProgress = 0;
		}
		#endregion

		#region public methods
		#endregion
	}
}
