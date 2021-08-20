using Files.UserControls.MultitaskingControl;
using Files.ViewModels;
using Microsoft.Toolkit.Uwp;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Files.Helpers
{
    public static class MultitaskingTabsHelpers
    {
        public static void CloseTabsToTheRight(IAppInstance clickedTab, IMultitaskingControl multitaskingControl)
        {
            int index = MainPageViewModel.AppInstances.IndexOf(clickedTab);
            List<IAppInstance> tabsToClose = new List<IAppInstance>();

            for (int i = index + 1; i < MainPageViewModel.AppInstances.Count; i++)
            {
                tabsToClose.Add(MainPageViewModel.AppInstances[i]);
            }

            foreach (var item in tabsToClose)
            {
                multitaskingControl?.CloseTab(item);
            }
        }

        public static async Task MoveTabToNewWindow(IAppInstance tab, IMultitaskingControl multitaskingControl)
        {
            // TODO: Serialize AppInstances
            //int index = MainPageViewModel.AppInstances.IndexOf(tab);
            //TabItemArguments tabItemArguments = MainPageViewModel.AppInstances[index].TabItemArguments;

            //multitaskingControl?.CloseTab(MainPageViewModel.AppInstances[index]);

            //if (tabItemArguments != null)
            //{
            //    await NavigationHelpers.OpenTabInNewWindowAsync(tabItemArguments.Serialize());
            //}
            //else
            //{
            //    await NavigationHelpers.OpenPathInNewWindowAsync("NewTab".GetLocalized());
            //}
        }
    }
}