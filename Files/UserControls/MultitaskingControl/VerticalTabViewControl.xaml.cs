using Files.Helpers;
using Files.ViewModels;
using Microsoft.Toolkit.Uwp;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml;

namespace Files.UserControls.MultitaskingControl
{
    public sealed partial class VerticalTabViewControl : BaseMultitaskingControl
    {
        private readonly DispatcherTimer tabHoverTimer = new DispatcherTimer();
        private TabViewItem hoveredTabViewItem = null;

        public VerticalTabViewControl()
        {
            InitializeComponent();
            tabHoverTimer.Interval = TimeSpan.FromMilliseconds(500);
            tabHoverTimer.Tick += TabHoverSelected;
        }

        private void VerticalTabView_TabItemsChanged(TabView sender, Windows.Foundation.Collections.IVectorChangedEventArgs args)
        {
            if (args.CollectionChange == Windows.Foundation.Collections.CollectionChange.ItemRemoved)
            {
                App.MainViewModel.TabStripSelectedIndex = Items.IndexOf(SelectedItem as IAppInstance);
            }

            if (App.MainViewModel.TabStripSelectedIndex >= 0 && App.MainViewModel.TabStripSelectedIndex < Items.Count)
            {
                CurrentSelectedAppInstance = GetCurrentSelectedTabInstance();

                if (CurrentSelectedAppInstance != null)
                {
                    OnCurrentInstanceChanged(new CurrentInstanceChangedEventArgs()
                    {
                        CurrentInstance = CurrentSelectedAppInstance,
                    });
                }
            }
        }

        private async void TabViewItem_Drop(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = await ((sender as TabViewItem).DataContext as IAppInstance).TabItemDrop(sender, e);
            CanReorderTabs = true;
            tabHoverTimer.Stop();
        }

        private void TabViewItem_DragEnter(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = ((sender as TabViewItem).DataContext as IAppInstance).TabItemDragOver(sender, e);
            if (e.AcceptedOperation != DataPackageOperation.None)
            {
                CanReorderTabs = false;
                tabHoverTimer.Start();
                hoveredTabViewItem = sender as TabViewItem;
            }
        }

        private void TabViewItem_DragLeave(object sender, DragEventArgs e)
        {
            tabHoverTimer.Stop();
            hoveredTabViewItem = null;
        }

        // Select tab that is hovered over for a certain duration
        private void TabHoverSelected(object sender, object e)
        {
            tabHoverTimer.Stop();
            if (hoveredTabViewItem != null)
            {
                SelectedItem = hoveredTabViewItem;
            }
        }

        private void TabStrip_TabDragStarting(TabView sender, TabViewTabDragStartingEventArgs args)
        {
            // TODO: Serialize AppInstances 
            //TabItemArguments tabViewItemArgs = null;
            //if ((args.Item as IAppInstance) is AppInstance instance)
            //{
            //    tabViewItemArgs = instance.AppInstanceInfo.TabItemArguments;
            //}
            //else if((args.Item as IAppInstance) is AppInstanceGroup instanceGroup)
            //{
            //    tabViewItemArgs = instanceGroup.AppInstanceInfo.TabItemArguments;
            //}
            //args.Data.Properties.Add(TabPathIdentifier, tabViewItemArgs.Serialize());
            //args.Data.RequestedOperation = DataPackageOperation.Move;
        }

        private void TabStrip_TabStripDragOver(object sender, DragEventArgs e)
        {
            if (e.DataView.Properties.ContainsKey(TabPathIdentifier))
            {
                CanReorderTabs = true;
                e.AcceptedOperation = DataPackageOperation.Move;
                e.DragUIOverride.Caption = "TabStripDragAndDropUIOverrideCaption".GetLocalized();
                e.DragUIOverride.IsCaptionVisible = true;
                e.DragUIOverride.IsGlyphVisible = false;
            }
            else
            {
                CanReorderTabs = false;
            }
        }

        private void TabStrip_DragLeave(object sender, DragEventArgs e)
        {
            CanReorderTabs = true;
        }

        private async void TabStrip_TabStripDrop(object sender, DragEventArgs e)
        {
            CanReorderTabs = true;
            if (!(sender is TabView tabStrip))
            {
                return;
            }

            if (!e.DataView.Properties.TryGetValue(TabPathIdentifier, out object tabViewItemPathObj) || !(tabViewItemPathObj is string tabViewItemString))
            {
                return;
            }

            var index = -1;

            for (int i = 0; i < tabStrip.TabItems.Count; i++)
            {
                var item = tabStrip.ContainerFromIndex(i) as TabViewItem;

                if (e.GetPosition(item).Y - item.ActualHeight < 0)
                {
                    index = i;
                    break;
                }
            }

            var tabViewItemArgs = TabItemArguments.Deserialize(tabViewItemString);
            ApplicationData.Current.LocalSettings.Values[TabDropHandledIdentifier] = true;
            await MainPageViewModel.AddNewTabByParam(tabViewItemArgs.InitialPageType, tabViewItemArgs.NavigationArg, index);
        }

        private void TabStrip_TabDragCompleted(TabView sender, TabViewTabDragCompletedEventArgs args)
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(TabDropHandledIdentifier) &&
                (bool)ApplicationData.Current.LocalSettings.Values[TabDropHandledIdentifier])
            {
                CloseTab(args.Item as IAppInstance);
            }
            else
            {
                SelectedItem = args.Tab;
            }

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(TabDropHandledIdentifier))
            {
                ApplicationData.Current.LocalSettings.Values.Remove(TabDropHandledIdentifier);
            }
        }

        private async void TabStrip_TabDroppedOutside(TabView sender, TabViewTabDroppedOutsideEventArgs args)
        {
            if (sender.TabItems.Count == 1)
            {
                return;
            }

            // TODO: Serialize AppInstances
            //var indexOfTabViewItem = sender.TabItems.IndexOf(args.Tab);
            //var tabViewItemArgs = (args.Item as IAppInstance).TabItemArguments;
            //var selectedTabViewItemIndex = sender.SelectedIndex;
            //CloseTab(args.Item as IAppInstance);
            //if (!await NavigationHelpers.OpenTabInNewWindowAsync(tabViewItemArgs.Serialize()))
            //{
            //    sender.TabItems.Insert(indexOfTabViewItem, args.Tab);
            //    sender.SelectedIndex = selectedTabViewItemIndex;
            //}
        }

        public override DependencyObject ContainerFromItem(IAppInstance item) => ContainerFromItem(item);

        private void BaseMultitaskingControl_TabSelectionChanged(object sender, CurrentInstanceChangedEventArgs e)
        {
            TabStrip_SelectionChanged(null, null);
        }
    }
}