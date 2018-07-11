using System;
using Dynamo.Core;
using Dynamo.Extensions;
using Dynamo.Graph.Nodes;
using Dynamo.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Monito
{
    class PlayerInputsViewModel : NotificationObject, IDisposable
    {
        private ReadyParams readyParams;
        private DynamoViewModel viewModel;

        public PlayerInputsViewModel(ReadyParams p, DynamoViewModel vm)
        {
            readyParams = p;
            viewModel = vm;
            p.CurrentWorkspaceModel.NodeAdded += CurrentWorkspaceModel_NodesChanged;
            p.CurrentWorkspaceModel.NodeRemoved += CurrentWorkspaceModel_NodesChanged;
        }

        public void Dispose()
        {
            readyParams.CurrentWorkspaceModel.NodeAdded -= CurrentWorkspaceModel_NodesChanged;
            readyParams.CurrentWorkspaceModel.NodeRemoved -= CurrentWorkspaceModel_NodesChanged;
        }

        private ObservableCollection<ObjectInWorkspace> currentInputs = new ObservableCollection<ObjectInWorkspace>();
        /// <summary>
        /// The search results as a list representation
        /// </summary>
        public ObservableCollection<ObjectInWorkspace> CurrentInputs
        {
            get
            {
                List<ObjectInWorkspace> unorderedInputs = new List<ObjectInWorkspace>();
                foreach (NodeModel node in readyParams.CurrentWorkspaceModel.Nodes)
                {
                    if (node.IsSetAsInput) { unorderedInputs.Add(new ObjectInWorkspace(node.NickName, node.GUID.ToString())); }
                }
                currentInputs.Clear();
                foreach (ObjectInWorkspace item in unorderedInputs.OrderBy(x => x.Name)) { currentInputs.Add(item); }
                return currentInputs;
            }
        }

        private string inputAction;
        /// <summary>
        /// The action that should be performed on the nodes in the workspace
        /// </summary>
        public string InputAction
        {
            get { return inputAction; }
            set
            {
                inputAction = value;
                if (value == "ResetAll")
                {
                    foreach (NodeModel node in readyParams.CurrentWorkspaceModel.Nodes)
                    {
                        if (node.IsSetAsInput) { node.IsSetAsInput = false; }
                    }
                }
                else if (value == "ResetSelected")
                {
                    foreach (var item in readyParams.CurrentWorkspaceModel.CurrentSelection)
                    {
                        if (item.IsSetAsInput) { item.IsSetAsInput = false; }
                    }
                }
                else if (value == "SetSelectedAsInput")
                {
                    foreach (var item in readyParams.CurrentWorkspaceModel.CurrentSelection)
                    {
                        if (!item.IsSetAsInput) { item.IsSetAsInput = true; }
                    }
                }
                RaisePropertyChanged(nameof(CurrentInputs));
            }
        }

        private void CurrentWorkspaceModel_NodesChanged(NodeModel obj)
        {
            RaisePropertyChanged(nameof(CurrentInputs));
        }
    }
}