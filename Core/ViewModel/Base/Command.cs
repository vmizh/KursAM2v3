using System;
using System.Windows.Input;

namespace Core.ViewModel.Base
{
    public class Command : ICommand
    {
        /// <summary>
        ///     Признак возможности исполнения команды
        /// </summary>
        private readonly Func<object, bool> commandCanExecute;

        /// <summary>
        ///     Обработчик действия команды
        /// </summary>
        private readonly Action<object> commandExecute;

        public Command(Action<object> execute, Func<object, bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            commandExecute = execute;
            commandCanExecute = canExecute;
        }

        public Command(Action<object> execute)
            : this(execute, null)
        {
        }

        public bool CanExecute(object parameter)
        {
            return commandCanExecute == null || commandCanExecute(parameter);
        }

        /// <summary>
        ///     Метод обработки события изменения условая выполнения команды
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        ///     Обработчик команды
        /// </summary>
        /// <param name="parameter">Параметры исполнения команды</param>
        public void Execute(object parameter)
        {
            commandExecute(parameter);
        }
    }
}