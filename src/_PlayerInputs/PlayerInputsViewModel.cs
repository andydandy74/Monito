using System;
using Dynamo.Core;
using Dynamo.Extensions;
using Dynamo.Graph.Nodes;
using System.Collections.ObjectModel;
using Dynamo.ViewModels;
using System.Windows;
using System.Linq;
using System.Collections.Generic;

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
            updateCurrentInputs();
            p.CurrentWorkspaceModel.NodeAdded += CurrentWorkspaceModel_NodesChanged;
            p.CurrentWorkspaceModel.NodeRemoved += CurrentWorkspaceModel_NodesChanged;
        }

        public void Dispose()
        {
            readyParams.CurrentWorkspaceModel.NodeAdded -= CurrentWorkspaceModel_NodesChanged;
            readyParams.CurrentWorkspaceModel.NodeRemoved -= CurrentWorkspaceModel_NodesChanged;
        }

        private string allInputs;
        /// <summary>
        /// All nodes that currently have IsInput set to true, as a string.
        /// </summary>
        public string AllInputs
        {
            get
            {
                return allInputs;
            }
            set
            {
                allInputs = value;
                RaisePropertyChanged(nameof(AllInputs));
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
                if (value == "ResetAll") { resetAll(); }
                else if (value == "ResetSelected") { resetSelected(); }
                else if (value == "SetSelectedAsInput") { setSelectedAsInput(); }
            }
        }

        private void updateCurrentInputs()
        {
            List<string> inputNodes = new List<string>();
            foreach (NodeModel node in readyParams.CurrentWorkspaceModel.Nodes)
            {
                if (node.IsSetAsInput)
                {
                    inputNodes.Add(node.NickName);
                }
            }
            inputNodes.Sort();
            if (inputNodes.Count > 0)
            {
                AllInputs = String.Join("\n", inputNodes.ToArray());
            }
            else
            {
                AllInputs = "Currently no nodes are set as inputs...";
            }
        }

        private void resetAll()
        {
            foreach (NodeModel node in readyParams.CurrentWorkspaceModel.Nodes)
            {
                if (node.IsSetAsInput)
                {
                    node.IsSetAsInput = false;
                }
            }
            updateCurrentInputs();
        }

        private void resetSelected()
        {
            foreach (var item in readyParams.CurrentWorkspaceModel.CurrentSelection)
            {
                if (item.IsSetAsInput)
                {
                    item.IsSetAsInput = false;
                }
            }
            updateCurrentInputs();
        }

        private void setSelectedAsInput()
        {
            foreach (var item in readyParams.CurrentWorkspaceModel.CurrentSelection)
            {
                if (!item.IsSetAsInput)
                {
                    item.IsSetAsInput = true;
                }
            }
            updateCurrentInputs();
        }

        private void CurrentWorkspaceModel_NodesChanged(NodeModel obj)
        {
            updateCurrentInputs();
        }
    }
}