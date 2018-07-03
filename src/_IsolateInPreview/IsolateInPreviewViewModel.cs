using System;
using Dynamo.Core;
using Dynamo.Extensions;
using Dynamo.Graph.Nodes;
using Dynamo.ViewModels;
using System.Collections.Generic;
using System.Windows;
using System.Collections.ObjectModel;
using System.Linq;

namespace Monito
{
    class IsolateInPreviewViewModel : NotificationObject, IDisposable
    {
        private ReadyParams readyParams;
        private DynamoViewModel viewModel;
        private Window dynWindow;

        public IsolateInPreviewViewModel(ReadyParams p, DynamoViewModel vm, Window dw)
        {
            readyParams = p;
            viewModel = vm;
            dynWindow = dw;
            p.CurrentWorkspaceModel.NodeAdded += CurrentWorkspaceModel_NodesChanged;
            p.CurrentWorkspaceModel.NodeRemoved += CurrentWorkspaceModel_NodesChanged;
        }

        public void Dispose()
        {
            readyParams.CurrentWorkspaceModel.NodeAdded -= CurrentWorkspaceModel_NodesChanged;
            readyParams.CurrentWorkspaceModel.NodeRemoved -= CurrentWorkspaceModel_NodesChanged;
        }

        private ObservableCollection<ObjectInWorkspace> currentPreviews = new ObservableCollection<ObjectInWorkspace>();
        /// <summary>
        /// The search results as a list representation
        /// </summary>
        public ObservableCollection<ObjectInWorkspace> CurrentPreviews
        {
            get
            {
                List<ObjectInWorkspace> unorderedInputs = new List<ObjectInWorkspace>();
                foreach (NodeViewModel node in viewModel.CurrentSpaceViewModel.Nodes)
                {
                    if (node.IsPreviewInsetVisible)
                    {
                        unorderedInputs.Add(new ObjectInWorkspace(node.NickName, node.NodeModel.GUID.ToString()));
                        //node.IsUpstreamVisible
                    }
                }
                currentPreviews.Clear();
                foreach (ObjectInWorkspace item in unorderedInputs.OrderBy(x => x.Name))
                {
                    currentPreviews.Add(item);
                }
                return currentPreviews;
            }
        }

        private string inputAction;
        /// <summary>
        /// The action that should be performed on the nodes in the workspace
        /// </summary>
        public string InputAction
        {
            get
            {
                return inputAction;
            }
            set
            {
                inputAction = value;
                if (value == "PreviewAll")
                {
                    foreach (NodeModel node in readyParams.CurrentWorkspaceModel.Nodes)
                    {
                        if (node.IsSetAsInput)
                        {
                            node.IsSetAsInput = false;
                        }
                    }
                }
                else if (value == "PreviewSelected")
                {
                    foreach (var item in readyParams.CurrentWorkspaceModel.CurrentSelection)
                    {
                        if (item.IsSetAsInput)
                        {
                            item.IsSetAsInput = false;
                        }
                    }
                }
                RaisePropertyChanged(nameof(CurrentPreviews));
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
                var VMU = new ViewModelUtils(readyParams, viewModel, dynWindow);
                VMU.ZoomToObject(value);
            }
        }

        private void CurrentWorkspaceModel_NodesChanged(NodeModel obj)
        {
            RaisePropertyChanged(nameof(CurrentPreviews));
        }
    }
}