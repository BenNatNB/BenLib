﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using static BenLib.Framework.TimingFramework;

namespace BenLib.Framework
{
    public static class ThreadingFramework
    {
        public static MessageBoxResult ShowException(Exception ex) => ex == null ? MessageBoxResult.None : MessageBox.Show(ex.Message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);

        public static void SetInterval(Action action, double milliseconds)
        {
            var dt = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(milliseconds) };
            dt.Tick += (sender, e) => action();

            dt.Start();
        }
    }

    public static partial class Extensions
    {
        public static async Task WithFramesTimeout(this Task task, int framesCountTimeout)
        {
            if (task != await Task.WhenAny(task, FramesDelay(framesCountTimeout))) throw new TimeoutException();
        }

        public static async Task<TResult> WithFramesTimeout<TResult>(this Task<TResult> task, int framesCountTimeout)
        {
            if (task == await Task.WhenAny(task, FramesDelay<TResult>(framesCountTimeout))) return task.Result;
            else throw new TimeoutException();
        }

        public static Task AtLeast(this Task task, int framesCountDelay) => Task.WhenAll(task, FramesDelay(framesCountDelay));
        public static async Task<TResult> AtLeast<TResult>(this Task<TResult> task, int millisecondsDelay) => (await Task.WhenAll(task, FramesDelay<TResult>(millisecondsDelay)))[0];

        public static Task AtMost(this Task task, int framesCountDelay) => Task.WhenAny(task, FramesDelay(framesCountDelay));
        public static async Task<TResult> AtMost<TResult>(this Task<TResult> task, int millisecondsDelay) => (await Task.WhenAny(task, FramesDelay<TResult>(millisecondsDelay))).Result;
    }

    public class RelayCommand<T> : ICommand
    {
        #region Fields

        private readonly Action<T> _execute = null;
        private readonly Predicate<T> _canExecute = null;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="DelegateCommand{T}"/>.
        /// </summary>
        /// <param name="execute">Delegate to execute when Execute is called on the command.  This can be null to just hook up a CanExecute delegate.</param>
        /// <remarks><seealso cref="CanExecute"/> will always return true.</remarks>
        public RelayCommand(Action<T> execute) : this(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException("execute");
            _canExecute = canExecute;
        }

        #endregion

        #region ICommand Members

        ///<summary>
        ///Defines the method that determines whether the command can execute in its current state.
        ///</summary>
        ///<param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        ///<returns>
        ///true if this command can be executed; otherwise, false.
        ///</returns>
        public bool CanExecute(object parameter) => _canExecute == null ? true : _canExecute((T)parameter);

        ///<summary>
        ///Occurs when changes occur that affect whether or not the command should execute.
        ///</summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        ///<summary>
        ///Defines the method to be called when the command is invoked.
        ///</summary>
        ///<param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
        public void Execute(object parameter) => _execute((T)parameter);

        #endregion
    }

    public class CommandHandler : ICommand
    {
        private readonly Action<object> _action;
        private readonly bool _canExecute;
        public CommandHandler(Action<object> action, bool canExecute = true)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter) => _action(parameter);
    }
}
