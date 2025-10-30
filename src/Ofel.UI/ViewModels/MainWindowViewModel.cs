using System;
using Microsoft.Extensions.DependencyInjection;
using Ofel.Engine;

namespace Ofel.UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ICalculationService _calc;
        public MainWindowViewModel(ICalculationService calc)
        {
            _calc = calc;
            RunCommand = new RelayCommand(_ => _calc.Run("default"));
        }

        public RelayCommand RunCommand { get; }
    }
}