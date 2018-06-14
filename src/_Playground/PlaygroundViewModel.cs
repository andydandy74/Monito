using System;
using Dynamo.Core;
using Dynamo.Extensions;
using Dynamo.Graph.Nodes;
using System.Collections.ObjectModel;
using Dynamo.PackageManager;
using Dynamo.ViewModels;
using System.Linq;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Notes;
using System.Collections.Generic;
using System.Windows;

namespace Monito
{
    class PlaygroundViewModel : NotificationObject, IDisposable
    {
        private ReadyParams readyParams;
        private DynamoViewModel viewModel;
        private Window dynWindow;

        public PlaygroundViewModel(ReadyParams p, DynamoViewModel vm, Window dw)
        {
            readyParams = p;
            viewModel = vm;
            dynWindow = dw;
        }

        public void Dispose() { }

        private ObservableCollection<ObjectInWorkspace> packageList = new ObservableCollection<ObjectInWorkspace>();
        /// <summary>
        /// The search results as a list representation
        /// </summary>
        public ObservableCollection<ObjectInWorkspace> PackageList
        {
            get
            {
                packageList.Clear();
                // InstalledPackagesViewModel IPVM = new InstalledPackagesViewModel(viewModel, PaLo);
                // BLOCKER: No access to PackageLoader
                return packageList;
            }
        }

        private string zoomGUID;
        /// <summary>
        /// The GUID of the node that was selected from the search results. Triggered by button click.
        /// </summary>
        public string ZoomGUID
        {
            get
            {
                return zoomGUID;
            }
            set
            {
                zoomGUID = value;
                //var VMU = new ViewModelUtils(readyParams, viewModel);
                //VMU.ZoomToObject(value);
            }
        }
    }
}