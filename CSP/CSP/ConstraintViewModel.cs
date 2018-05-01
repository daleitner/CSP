using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Base;

namespace CSP
{
	public class ConstraintViewModel : ViewModelBase
	{
		private Variable _selectedVariable1;
		private Variable _selectedVariable2;
		private CompareEnum _selectedComparator;
		private RelayCommand _removeCommand;

		public delegate void RemoveConstraintEventHandler(ConstraintViewModel constraint);

		public event RemoveConstraintEventHandler RemoveConstraint = null;
		public ConstraintViewModel(List<Variable> allVariables)
		{
			AllVariables = allVariables;
		}

		public List<Variable> AllVariables { get; set; }

		public Variable SelectedVariable1
		{
			get { return _selectedVariable1; }
			set
			{
				_selectedVariable1 = value;
				OnPropertyChanged(nameof(SelectedVariable1));
			}
		}
		public Variable SelectedVariable2
		{
			get { return _selectedVariable2; }
			set
			{
				_selectedVariable2 = value;
				OnPropertyChanged(nameof(SelectedVariable2));
			}
		}
		public CompareEnum SelectedComparator
		{
			get { return _selectedComparator; }
			set
			{
				_selectedComparator = value;
				OnPropertyChanged(nameof(SelectedComparator));
			}
		}

		public ICommand RemoveCommand
		{
			get
			{
				return _removeCommand ?? (_removeCommand = new RelayCommand(
					       p => Remove()));
			}
		}

		private void Remove()
		{
			RemoveConstraint?.Invoke(this);
		}
	}
}
