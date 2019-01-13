using System;
using Dynamo.Core;
using Dynamo.Extensions;
using Dynamo.Graph.Nodes;
using Dynamo.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Dynamo.UI.Commands;
using Dynamo.Models;

namespace Monito
{
    class PlayerInputsViewModel : NotificationObject, IDisposable
    {
        private ReadyParams readyParams;
        private DynamoViewModel viewModel;
        public ICommand ResetAll { get; set; }
        public ICommand ResetSelected { get; set; }
        public ICommand SetSelectedAsInput { get; set; }
		private string batchProcessResults = "";

		public PlayerInputsViewModel(ReadyParams p, DynamoViewModel vm)
        {
            readyParams = p;
            viewModel = vm;
            p.CurrentWorkspaceModel.NodeAdded += CurrentWorkspaceModel_NodesChanged;
            p.CurrentWorkspaceModel.NodeRemoved += CurrentWorkspaceModel_NodesChanged;
            ResetAll = new DelegateCommand(OnResetAllClicked);
            ResetSelected = new DelegateCommand(OnResetSelectedClicked);
            SetSelectedAsInput = new DelegateCommand(OnSetSelectedAsInputClicked);
        }

        public void Dispose()
        {
            readyParams.CurrentWorkspaceModel.NodeAdded -= CurrentWorkspaceModel_NodesChanged;
            readyParams.CurrentWorkspaceModel.NodeRemoved -= CurrentWorkspaceModel_NodesChanged;
        }

        private string currentInputsMsg;
        public string CurrentInputsMsg
        {
            get
            {
				if (batchProcessResults != "")
				{
					currentInputsMsg = batchProcessResults;
					batchProcessResults = "";
				}
				else if (currentInputs.Count > 0) { currentInputsMsg = "All Dynamo Player inputs in current workspace:"; }
                else { currentInputsMsg = "No Dynamo Player inputs in current workspace..."; }
                return currentInputsMsg;
            }
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
                RaisePropertyChanged(nameof(CurrentInputsMsg));
                return currentInputs;
            }
        }

        public void OnResetAllClicked(object obj)
        {
			ResetAllInputs();
			RaisePropertyChanged(nameof(CurrentInputs));
        }

        public void OnResetSelectedClicked(object obj)
        {
            foreach (var item in readyParams.CurrentWorkspaceModel.CurrentSelection)
            {
                if (item.IsSetAsInput) { item.IsSetAsInput = false; }
            }
            RaisePropertyChanged(nameof(CurrentInputs));
        }

        public void OnSetSelectedAsInputClicked(object obj)
        {
            foreach (var item in readyParams.CurrentWorkspaceModel.CurrentSelection)
            {
                if (!item.IsSetAsInput) { item.IsSetAsInput = true; }
            }
            RaisePropertyChanged(nameof(CurrentInputs));
        }

		public void ResetAllInputs()
		{
			foreach (NodeModel node in readyParams.CurrentWorkspaceModel.Nodes)
			{
				if (node.IsSetAsInput) { node.IsSetAsInput = false; }
			}
		}

		public void OnBatchResetInputsClicked(string directoryPath)
		{
			// Read directory contents
			var graphs = System.IO.Directory.EnumerateFiles(directoryPath);
			int graphCount = 0;
			foreach (var graph in graphs)
			{
				var ext = System.IO.Path.GetExtension(graph);
				if (ext == ".dyn")
				{
					viewModel.OpenCommand.Execute(graph);
					viewModel.CurrentSpaceViewModel.RunSettingsViewModel.Model.RunType = RunType.Manual;
					ResetAllInputs();
					viewModel.SaveAsCommand.Execute(graph);
					viewModel.CloseHomeWorkspaceCommand.Execute(null);
					graphCount += 1;
				}
			}
			batchProcessResults = "Reset all inputs in " + graphCount.ToString() + " graphs...";
			RaisePropertyChanged(nameof(CurrentInputs));
		}

		private void CurrentWorkspaceModel_NodesChanged(NodeModel obj)
        {
            RaisePropertyChanged(nameof(CurrentInputs));
        }
    }
}