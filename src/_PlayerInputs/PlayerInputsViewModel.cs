using System;
using Dynamo.Core;
using Dynamo.Extensions;
using Dynamo.Graph.Nodes;
using Dynamo.ViewModels;
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
                    allInputs = String.Join("\n", inputNodes.ToArray());
                }
                else
                {
                    allInputs = "Currently no nodes are set as inputs...";
                }
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
                if (value == "ResetAll")
                {
                    foreach (NodeModel node in readyParams.CurrentWorkspaceModel.Nodes)
                    {
                        if (node.IsSetAsInput)
                        {
                            node.IsSetAsInput = false;
                        }
                    }
                }
                else if (value == "ResetSelected")
                {
                    foreach (var item in readyParams.CurrentWorkspaceModel.CurrentSelection)
                    {
                        if (item.IsSetAsInput)
                        {
                            item.IsSetAsInput = false;
                        }
                    }
                }
                else if (value == "SetSelectedAsInput")
                {
                    foreach (var item in readyParams.CurrentWorkspaceModel.CurrentSelection)
                    {
                        if (!item.IsSetAsInput)
                        {
                            item.IsSetAsInput = true;
                        }
                    }
                }
                RaisePropertyChanged(nameof(AllInputs));
            }
        }

        private void CurrentWorkspaceModel_NodesChanged(NodeModel obj)
        {
            RaisePropertyChanged(nameof(AllInputs));
        }
    }
}