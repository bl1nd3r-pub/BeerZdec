using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BeerZdec.ViewModels
{
    // Async-команда без параметра
    public class RelayCommandAsync(Func<Task> execute, Func<bool>? canExecute = null) : ICommand
    {
        private readonly Func<Task> _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        private readonly Func<bool>? _canExecute = canExecute;
        private bool _isExecuting;

        public bool CanExecute(object? parameter)
            => !_isExecuting && (_canExecute?.Invoke() ?? true);

        public async void Execute(object? parameter)
        {
            if (CanExecute(parameter))
            {
                _isExecuting = true;
                CommandManager.InvalidateRequerySuggested();

                try { await _execute(); }
                finally
                {
                    _isExecuting = false;
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    // Async-команда с параметром
    public class RelayCommandAsync<T>(Func<T?, Task> execute, Predicate<T?>? canExecute = null) : ICommand
    {
        private readonly Func<T?, Task> _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        private readonly Predicate<T?>? _canExecute = canExecute;
        private bool _isExecuting;

        public bool CanExecute(object? parameter)
            => !_isExecuting && (_canExecute?.Invoke((T?)parameter) ?? true);

        public async void Execute(object? parameter)
        {
            if (CanExecute(parameter))
            {
                _isExecuting = true;
                CommandManager.InvalidateRequerySuggested();

                try { await _execute((T?)parameter); }
                finally
                {
                    _isExecuting = false;
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}