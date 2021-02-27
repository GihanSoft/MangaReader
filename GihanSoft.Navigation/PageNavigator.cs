using GihanSoft.Navigation.Events;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Windows;

namespace GihanSoft.Navigation
{
    public class PageNavigator : DependencyObject
    {

        #region DependencyProperty
        private static readonly DependencyPropertyKey CurrentPropertyKey
            = DependencyProperty.RegisterReadOnly(
                nameof(Current),
                typeof(Page),
                typeof(PageNavigator),
                new PropertyMetadata());

        public static readonly DependencyProperty CurrentProperty
            = CurrentPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey CanGoBackPropertyKey
            = DependencyProperty.RegisterReadOnly(
                nameof(CanGoBack),
                typeof(bool),
                typeof(PageNavigator),
                new PropertyMetadata(false));

        public static readonly DependencyProperty CanGoBackProperty
            = CanGoBackPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey CanGoForwardPropertyKey
            = DependencyProperty.RegisterReadOnly(
                nameof(CanGoForward),
                typeof(bool),
                typeof(PageNavigator),
                new PropertyMetadata(false));

        public static readonly DependencyProperty CanGoForwardProperty
            = CanGoForwardPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey BackStackPropertyKey
            = DependencyProperty.RegisterReadOnly(
                nameof(BackStack),
                typeof(IEnumerable<Page>),
                typeof(PageNavigator),
                new PropertyMetadata());

        public static readonly DependencyProperty BackStackProperty
            = BackStackPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey ForwardStackPropertyKey
            = DependencyProperty.RegisterReadOnly(
                nameof(ForwardStack),
                typeof(IEnumerable<Page>),
                typeof(PageNavigator),
                new PropertyMetadata());

        public static readonly DependencyProperty ForwardStackProperty
            = ForwardStackPropertyKey.DependencyProperty;
        #endregion

        private readonly IServiceProvider serviceProvider;

        private readonly Stack<Page> backStack;
        private readonly Stack<Page> forwardStack;

        public PageNavigator(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            backStack = new Stack<Page>();
            forwardStack = new Stack<Page>();
        }

        #region Property
        public bool CanGoBack => (bool)GetValue(CanGoBackProperty);
        public bool CanGoForward => (bool)GetValue(CanGoForwardProperty);
        public IEnumerable<Page> BackStack => GetValue(BackStackProperty) as IEnumerable<Page>;
        public IEnumerable<Page> ForwardStack => GetValue(ForwardStackProperty) as IEnumerable<Page>;
        public Page Current => GetValue(CurrentProperty) as Page;
        #endregion

        public event EventHandler<NavigatedEventArgs>? Navigated;
        public event EventHandler<NavigatingEventArgs>? Navigating;

        public bool GoBack()
        {
            if (!CanGoBack)
            {
                throw new NavigationException();
            }

            Page backPage = backStack.Peek();
            NavigatingEventArgs navigatingEventArgs = new(Current, backPage);
            Navigating?.Invoke(this, navigatingEventArgs);
            if (navigatingEventArgs.Cancel)
            {
                return false;
            }
            backStack.Pop();
            forwardStack.Push(Current);

            SetValue(CurrentPropertyKey, backPage);
            backPage.Refresh();
            SetValue(CanGoBackPropertyKey, backStack.Count > 0);
            SetValue(CanGoForwardPropertyKey, true);
            SetValue(BackStackPropertyKey, backStack.ToImmutableArray());
            SetValue(ForwardStackPropertyKey, forwardStack.ToImmutableArray());

            Navigated?.Invoke(this, navigatingEventArgs);

            return true;
        }
        public bool GoFroward()
        {
            if (!CanGoForward)
            {
                throw new NavigationException();
            }

            Page forwardPage = forwardStack.Peek();
            NavigatingEventArgs navigatingEventArgs = new(Current, forwardPage);
            Navigating?.Invoke(this, navigatingEventArgs);
            if (navigatingEventArgs.Cancel)
            {
                return false;
            }
            forwardStack.Pop();
            backStack.Push(Current);

            SetValue(CurrentPropertyKey, forwardPage);
            forwardPage.Refresh();
            SetValue(CanGoBackPropertyKey, true);
            SetValue(CanGoForwardPropertyKey, forwardStack.Count > 0);
            SetValue(BackStackPropertyKey, backStack.ToImmutableArray());
            SetValue(ForwardStackPropertyKey, forwardStack.ToImmutableArray());

            Navigated?.Invoke(this, navigatingEventArgs);

            return true;
        }
        public bool GoTo<TPage>()
            where TPage : Page
        {
            Page page = ActivatorUtilities.GetServiceOrCreateInstance<TPage>(serviceProvider);
            NavigatingEventArgs navigatingEventArgs = new(Current, page);
            Navigating?.Invoke(this, navigatingEventArgs);
            if (navigatingEventArgs.Cancel)
            {
                return false;
            }

            while (forwardStack.Count > 0)
            {
                Page disposePage = forwardStack.Pop();
                disposePage.Dispose();
            }
            if (Current is not null)
            {
                backStack.Push(Current);
            }
            SetValue(CurrentPropertyKey, page);
            page.Refresh();
            SetValue(CanGoBackPropertyKey, backStack.Count > 0);
            SetValue(CanGoForwardPropertyKey, false);
            SetValue(BackStackPropertyKey, backStack.ToImmutableArray());
            SetValue(ForwardStackPropertyKey, forwardStack.ToImmutableArray());

            Navigated?.Invoke(this, navigatingEventArgs);

            return true;
        }
    }
}
