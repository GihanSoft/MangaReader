using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MangaReader;
using System;

namespace GihanSoft.Views.AttachedProperties
{
    public static class DesignModeHelper
    {
        public static bool IsDesignMode => System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
    }

    public static class KeyBindingProp
    {
        private static readonly List<Window> registeredWins = new();
        private static readonly List<KeyBinding> keyBindings = new();

        public static readonly DependencyProperty LocalHotKeyProperty = DependencyProperty.RegisterAttached(
            "LocalHotKey",
            typeof(bool),
            typeof(KeyBindingProp),
            new PropertyMetadata(false, (obj, args) =>
            {
                if (DesignModeHelper.IsDesignMode)
                {
                    return;
                }

                IEnumerable<Window> unregWins = App.Current.Windows
                    .Cast<Window>()
                    .Where(x => !registeredWins.Contains(x));
                foreach (Window win in unregWins)
                {
                    win.PreviewKeyDown += (s, e) =>
                    {
                        foreach (KeyBinding keyBinding in keyBindings)
                        {
                            if (keyBinding.Key == e.Key && keyBinding.Modifiers == e.KeyboardDevice.Modifiers)
                            {
                                if (keyBinding.Command is RoutedCommand routedCommand)
                                {
                                    if (routedCommand.CanExecute(keyBinding.CommandParameter, keyBinding.CommandTarget))
                                    {
                                        routedCommand.Execute(keyBinding.CommandParameter, keyBinding.CommandTarget);
                                        e.Handled = true;
                                    }
                                }
                                else
                                {
                                    if (keyBinding.Command.CanExecute(keyBinding.CommandParameter))
                                    {
                                        keyBinding.Command.Execute(keyBinding.CommandParameter);
                                        e.Handled = true;
                                    }
                                }
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

        /// <summary>Helper for getting <see cref="LocalHotKeyProperty"/> from <paramref name="inputBinding"/>.</summary>
        /// <param name="inputBinding"><see cref="KeyBinding"/> to read <see cref="LocalHotKeyProperty"/> from.</param>
        /// <returns>LocalHotKey property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(KeyBinding))]
        public static bool GetLocalHotKey(KeyBinding inputBinding)
        {
            if (inputBinding is null)
            {
                throw new ArgumentNullException(nameof(inputBinding));
            }
            return (bool)inputBinding.GetValue(LocalHotKeyProperty);
        }

        /// <summary>Helper for setting <see cref="LocalHotKeyProperty"/> on <paramref name="inputBinding"/>.</summary>
        /// <param name="inputBinding"><see cref="KeyBinding"/> to set <see cref="LocalHotKeyProperty"/> on.</param>
        /// <param name="value">LocalHotKey property value.</param>
        [AttachedPropertyBrowsableForType(typeof(KeyBinding))]
        public static void SetLocalHotKey(KeyBinding inputBinding, bool value)
        {
            if (inputBinding is null)
            {
                throw new ArgumentNullException(nameof(inputBinding));
            }
            inputBinding.SetValue(LocalHotKeyProperty, value);
        }
    }
}
