using Files.Filesystem;
using Files.Helpers;
using Files.UserControls.MultitaskingControl;
using Files.ViewModels;
using Files.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Files
{
    public interface IShellPage : ITabItemContent, IMultiPaneInfo, IDisposable
    {
        public AppInstanceInformation AppInstanceInfo { get; set; }

        IBaseLayout SlimContentPage { get; }

        BaseLayout ContentPage { get; }

        FilesystemHelpers FilesystemHelpers { get; }

        CurrentInstanceViewModel InstanceViewModel { get; }

        ItemViewModel FilesystemViewModel { get; }

        NavToolbarViewModel NavToolbarViewModel { get; }

        void Refresh_Click();
        void UpdatePathUIToWorkingDirectory(string newWorkingDir, string singleItemOverride = null);
        void NavigateToPath(string navigationPath, Type sourcePageType, NavigationArguments navArgs = null);

        Type CurrentPageType { get; }

        bool CanNavigateBackward { get; }
        bool CanNavigateForward { get; }


        /// <summary>
        /// Gets the layout mode for the specified path then navigates to it
        /// </summary>
        /// <param name="navigationPath"></param>
        /// <param name="navArgs"></param>
        public void NavigateToPath(string navigationPath, NavigationArguments navArgs = null);

        /// <summary>
        /// Navigates to the home page
        /// </summary>
        public void NavigateHome();

        void NavigateWithArguments(Type sourcePageType, NavigationArguments navArgs);

        void RemoveLastPageFromBackStack();

        void SubmitSearch(string query, bool searchUnindexedItems);
        
        /// <summary>
        /// Used to make commands in the column view work properly
        /// </summary>
        public bool IsColumnView { get; } 
    }

    public interface IPaneHolder : ITabItemContent, IDisposable, INotifyPropertyChanged
    {
        public AppInstanceGroup Instances { get; }

        public IShellPage ActivePane { get; set; }
        public IFilesystemHelpers FilesystemHelpers { get; }
        public List<AppInstanceInformation> TabItemArguments { get; }

        public void OpenPathInNewPane(string path);

        public void CloseActivePane();

        public bool IsMultiPaneActive { get; } // Another pane is shown
        public bool IsMultiPaneEnabled { get; } // Multi pane is enabled
    }

    public interface IMultiPaneInfo
    {
        public bool IsPageMainPane { get; } // The instance is the left (or only) pane
    }
}