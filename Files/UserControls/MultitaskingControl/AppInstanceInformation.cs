using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Files.Filesystem;
using Files.Interacts;
using Files.ViewModels;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;

namespace Files.UserControls.MultitaskingControl
{
    public class AppInstanceInformation : ObservableObject, ITabItem
    {
        private BaseLayout contentPage = null;

        public Guid ParentAppInstanceId { get; set; }

        public IAppInstance AppInstance => MainPageViewModel.AppInstances.FirstOrDefault(x => x.Id == this.ParentAppInstanceId);

        public CurrentInstanceViewModel InstanceViewModel { get; set; }
        
        public FilesystemHelpers FilesystemHelpers { get; set; }

        public ItemViewModel FilesystemViewModel { get; set; }

        public TabItemArguments TabItemArguments { get; set; }

        public bool IsCurrentInstance { get; set; }

        public CancellationTokenSource CancellationTokenSource { get; internal set; }

        public NavToolbarViewModel NavToolbarViewModel { get; set; } = new NavToolbarViewModel();

        public IBaseLayout SlimContentPage => ContentPage;

        public BaseLayout ContentPage
        {
            get => contentPage;
            set
            {
                if (value != contentPage)
                {
                    SetProperty(ref contentPage, value);
                    OnPropertyChanged(nameof(SlimContentPage));
                }
            }
        }
    }
}
