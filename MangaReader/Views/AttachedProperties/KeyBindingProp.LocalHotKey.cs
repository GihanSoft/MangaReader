using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DP = System.Windows.DependencyProperty;

using GihanSoft;
using MangaReader;

namespace GihanSoft.Views.AttachedProperties
{
    public static class KeyBindingProp
    {
        private static readonly List<Window> registeredWins = new();
        private static readonly List<KeyBinding> keyBindings = new();

        public static readonly DP LocalHotKeyProperty = DP.RegisterAttached(
            "LocalHotKey",
            typeof(bool),
            typeof(KeyBindingProp),
            new PropertyMetadata(false, (obj, args) =>
            {
                var unregWins = App.Current.Windows.Cast<Window>().Where(x => !registeredWins.Contains(x));
                foreach (var win in unregWins)
                {
                    win.KeyDown += (s, e) =>
                    {
                        foreach (KeyBinding ib in keyBindings)
                        {
                            if (ib.Key == e.Key && ib.Modifiers == e.KeyboardDevice.Modifiers)
                            {
                                if (ib.Command is RoutedCommand rc)
                                {
                                    if (rc.CanExecute(ib.CommandParameter, ib.CommandTarget))
                                    {
                                        rc.Execute(ib.CommandParameter, ib.CommandTarget);
                                    }
                                }
                                else
                                {
                                    if (ib.Command.CanExecute(ib.CommandParameter))
                                    {
                                        ib.Command.Execute(ib.CommandParameter);
                                    }
                                }
                                e.Handled = true;
                            }
                        }
                    };
                }
                registeredWins.AddRange(unregWins);
                if (obj is not KeyBinding inputBinding) { return; }
                if ((bool)args.NewValue)
                {
                    keyBindings.Add(inputBinding);
                }
                else
                {
                    keyBindings.Remove(inputBinding);
                }
            }));

        public static bool GetLocalHotKey(KeyBinding inputBinding)
        {
            return (bool)inputBinding.GetValue(LocalHotKeyProperty);
        }
        public static void SetLocalHotKey(KeyBinding inputBinding, bool value)
        {
            inputBinding.SetValue(LocalHotKeyProperty, value);
        }
    }
}
