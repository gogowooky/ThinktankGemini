using System;
using System.Windows;

namespace ThinktankApp
{
    public class TTWindow
    {
        private Window _mainWindow;

        public TTWindow(Window window)
        {
            _mainWindow = window;
        }

        public WindowState State
        {
            get 
            {
                if (_mainWindow.Dispatcher.CheckAccess())
                    return _mainWindow.WindowState;
                else
                    return (WindowState)_mainWindow.Dispatcher.Invoke(new Func<WindowState>(() => _mainWindow.WindowState));
            }
            set 
            {
                if (_mainWindow.Dispatcher.CheckAccess())
                    _mainWindow.WindowState = value;
                else
                    _mainWindow.Dispatcher.Invoke(new Action(() => _mainWindow.WindowState = value));
            }
        }

        public double Width
        {
            get 
            {
                if (_mainWindow.Dispatcher.CheckAccess())
                    return _mainWindow.Width;
                else
                    return (double)_mainWindow.Dispatcher.Invoke(new Func<double>(() => _mainWindow.Width));
            }
            set 
            {
                if (_mainWindow.Dispatcher.CheckAccess())
                    _mainWindow.Width = value;
                else
                    _mainWindow.Dispatcher.Invoke(new Action(() => _mainWindow.Width = value));
            }
        }

        public double Height
        {
            get 
            {
                if (_mainWindow.Dispatcher.CheckAccess())
                    return _mainWindow.Height;
                else
                    return (double)_mainWindow.Dispatcher.Invoke(new Func<double>(() => _mainWindow.Height));
            }
            set 
            {
                if (_mainWindow.Dispatcher.CheckAccess())
                    _mainWindow.Height = value;
                else
                    _mainWindow.Dispatcher.Invoke(new Action(() => _mainWindow.Height = value));
            }
        }

        public double Left
        {
            get 
            {
                if (_mainWindow.Dispatcher.CheckAccess())
                    return _mainWindow.Left;
                else
                    return (double)_mainWindow.Dispatcher.Invoke(new Func<double>(() => _mainWindow.Left));
            }
            set 
            {
                if (_mainWindow.Dispatcher.CheckAccess())
                    _mainWindow.Left = value;
                else
                    _mainWindow.Dispatcher.Invoke(new Action(() => _mainWindow.Left = value));
            }
        }

        public double Top
        {
            get 
            {
                if (_mainWindow.Dispatcher.CheckAccess())
                    return _mainWindow.Top;
                else
                    return (double)_mainWindow.Dispatcher.Invoke(new Func<double>(() => _mainWindow.Top));
            }
            set 
            {
                if (_mainWindow.Dispatcher.CheckAccess())
                    _mainWindow.Top = value;
                else
                    _mainWindow.Dispatcher.Invoke(new Action(() => _mainWindow.Top = value));
            }
        }

        public event EventHandler StateChanged
        {
            add { _mainWindow.StateChanged += value; }
            remove { _mainWindow.StateChanged -= value; }
        }

        public event SizeChangedEventHandler SizeChanged
        {
            add { _mainWindow.SizeChanged += value; }
            remove { _mainWindow.SizeChanged -= value; }
        }

        public event EventHandler LocationChanged
        {
            add { _mainWindow.LocationChanged += value; }
            remove { _mainWindow.LocationChanged -= value; }
        }
    }
}
