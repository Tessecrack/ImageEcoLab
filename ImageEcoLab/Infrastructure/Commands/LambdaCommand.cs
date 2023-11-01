using System;
using ImageEcoLab.Infrastructure.Commands.Base;

namespace ImageEcoLab.Infrastructure.Commands
{
    internal class LambdaCommand : Command
	{
		private readonly Func<object, bool> _canExecute;

		private readonly Action<object> _execute;

        public LambdaCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _canExecute = canExecute;
			_execute = execute;
		}

		public override bool CanExecute(object? parameter) => _canExecute?.Invoke(arg: parameter) ?? true;

		public override void Execute(object? parameter) => _execute(obj: parameter);
	}
}
