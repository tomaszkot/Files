using Files.Helpers;
using Files.ViewModels;
using Microsoft.Toolkit.Uwp.UI;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Files.UserControls.MultitaskingControl
{
    public class BaseMultitaskingControl : TabView, IMultitaskingControl, INotifyPropertyChanged
    {
        private static bool isRestoringClosedTab = false; // Avoid reopening two tabs

        public const string TabDropHandledIdentifier = "FilesTabViewItemDropHandled";

        public const string TabPathIdentifier = "FilesTabViewItemPath";

        public event PropertyChangedEventHandler PropertyChanged;

        public BaseMultitaskingControl()
        {
        }

        public ObservableCollection<IAppInstance> Items => MainPageViewModel.AppInstances;

        public static List<IAppInstance> RecentlyClosedTabs { get; private set; } = new List<IAppInstance>();

        private void MultitaskingControl_TabSelectionChanged(object sender, CurrentInstanceChangedEventArgs e)
        {
            foreach (IAppInstance instance in Items)
            {
                if (instance is AppInstance single)
                {
                    single.AppInstanceInfos[0].IsCurrentInstance = false;
                }
                else if (instance is AppInstanceGroup group)
                {
                    foreach(AppInstanceInformation info in group.AppInstanceInfos)
                    {
                        info.IsCurrentInstance = false;
                    }
                }
            }

            if (e.CurrentInstance is IPaneHolder paneHolder)
            {
                paneHolder.ActivePane.AppInstanceInfo.IsCurrentInstance = true;
            }
            else if (e.CurrentInstance is IShellPage page)
            {
                page.AppInstanceInfo.IsCurrentInstance = true;
            }
        }

        protected void TabStrip_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            CloseTab(args.Item as IAppInstance);
        }

        protected async void TabView_AddTabButtonClick(TabView sender, object args)
        {
            await MainPageViewModel.AddNewTabAsync();
        }

        public ITabItemContent GetCurrentSelectedTabInstance()
        {
            return ((TabViewItem)this.ContainerFromIndex(App.MainViewModel.TabStripSelectedIndex)).ContentTemplate.LoadContent() as ITabItemContent;
        }

        public void CloseTabsToTheRight(object sender, RoutedEventArgs e)
        {
            MultitaskingTabsHelpers.CloseTabsToTheRight(((FrameworkElement)sender).DataContext as IAppInstance, this);
        }

        public async void ReopenClosedTab(object sender, RoutedEventArgs e)
        {
            if (!isRestoringClosedTab && RecentlyClosedTabs.Any())
            {
                isRestoringClosedTab = true;
                IAppInstance lastTab = RecentlyClosedTabs.Last();
                RecentlyClosedTabs.Remove(lastTab);
                Items.Add(lastTab);
                isRestoringClosedTab = false;
            }
        }

        public async void MoveTabToNewWindow(object sender, RoutedEventArgs e)
        {
            await MultitaskingTabsHelpers.MoveTabToNewWindow(((FrameworkElement)sender).DataContext as IAppInstance, this);
        }

        public void CloseTab(IAppInstance tabItem)
        {
            if (Items.Count == 1)
            {
                App.CloseApp();
            }
            else if (Items.Count > 1)
            {
                Items.Remove(tabItem);
                RecentlyClosedTabs.Add(tabItem);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SetLoadingIndicatorStatus(IAppInstance item, bool loading)
        {
            var instance = ContainerFromItem(item) as Control;
            if (instance is null)
            {
                return;
            }

            if (loading)
            {
                VisualStateManager.GoToState(instance, "Loading", false);
            }
            else
            {
                VisualStateManager.GoToState(instance, "NotLoading", false);
            }
        }
    }
}