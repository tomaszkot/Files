using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Files.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;

namespace Files.UserControls.MultitaskingControl
{
    public interface IAppInstance
    {
        public Guid Id { get; set; }

        public ObservableCollection<AppInstanceInformation> AppInstanceInfos { get; }

        public string Header { get; set; }

        public IconSource IconSource { get; set; }

        public bool AllowStorageItemDrop { get; set; }

        public void AddAppInstanceByArguments(TabItemArguments args);

        public void UpdateTabInfo();

        /// <summary>
        /// Update tab information such as Header and Icon from a collection of arguments.
        /// Items with an index other than zero will only be taken into account if the 
        /// app instance implementation supports performing the action on multiple instances. 
        /// </summary>
        /// <param name="args">Collection of arguments used to retrieve the correct property values</param>
        public void UpdateTabInfo(List<TabItemArguments> args);

        public DataPackageOperation TabItemDragOver(object sender, DragEventArgs e);

        public Task<DataPackageOperation> TabItemDrop(object sender, DragEventArgs e);
    }
}
