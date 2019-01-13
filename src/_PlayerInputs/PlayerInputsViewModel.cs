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
        public ICommand ResetAllInputsCommand { get; set; }
        public ICommand ResetSelectedInputs { get; set; }
        public ICommand SetSelectedAsInput { get; set; }
        public ICommand ResetAllOutputsCommand { get; set; }
        public ICommand ResetSelectedOutputs { get; set; }
        public ICommand SetSelectedAsOutput { get; set; }
		private string batchProcessInputsResults = "";
		private string batchProcessOutputsResults = "";

		public PlayerInputsViewModel(ReadyParams p, DynamoViewModel vm)
        {
            readyParams = p;
            viewModel = vm;
            p.CurrentWorkspaceModel.NodeAdded += CurrentWorkspaceModel_NodesChanged;
            p.CurrentWorkspaceModel.NodeRemoved += CurrentWorkspaceModel_NodesChanged;
            ResetAllInputsCommand = new DelegateCommand(OnResetAllInputsClicked);
            ResetSelectedInputs = new DelegateCommand(OnResetSelectedInputsClicked);
            SetSelectedAsInput = new DelegateCommand(OnSetSelectedAsInputClicked);
            ResetAllOutputsCommand = new DelegateCommand(OnResetAllOutputsClicked);
            ResetSelectedOutputs = new DelegateCommand(OnResetSelectedOutputsClicked);
            SetSelectedAsOutput = new DelegateCommand(OnSetSelectedAsOutputClicked);
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
				if (batchProcessInputsResults != "")
				{
					currentInputsMsg = batchProcessInputsResults;
					batchProcessInputsResults = "";
				}
				else if (currentInputs.Count > 0) { currentInputsMsg = "All Dynamo Player inputs in current workspace:"; }
                else { currentInputsMsg = "No Dynamo Player inputs in current workspace..."; }
                return currentInputsMsg;
            }
        }

        private string currentOutputsMsg;
        public string CurrentOutputsMsg
        {
            get
            {
				if (batchProcessOutputsResults != "")
				{
					currentOutputsMsg = batchProcessOutputsResults;
					batchProcessOutputsResults = "";
				}
				else if (currentOutputs.Count > 0) { currentOutputsMsg = "All Dynamo Player outputs in current workspace:"; }
                else { currentOutputsMsg = "No Dynamo Player outputs in current workspace..."; }
                return currentOutputsMsg;
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
                    if (node.IsSetAsInput) { unorderedInputs.Add(new ObjectInWorkspace(node.Name, node.GUID.ToString())); }
                }
                currentInputs.Clear();
                foreach (ObjectInWorkspace item in unorderedInputs.OrderBy(x => x.Name)) { currentInputs.Add(item); }
                RaisePropertyChanged(nameof(CurrentInputsMsg));
                return currentInputs;
            }
        }

        private ObservableCollection<ObjectInWorkspace> currentOutputs = new ObservableCollection<ObjectInWorkspace>();
        /// <summary>
        /// The search results as a list representation
        /// </summary>
        public ObservableCollection<ObjectInWorkspace> CurrentOutputs
        {
            get
            {
                List<ObjectInWorkspace> unorderedOutputs = new List<ObjectInWorkspace>();
                foreach (NodeModel node in readyParams.CurrentWorkspaceModel.Nodes)
                {
                    if (node.IsSetAsOutput) { unorderedOutputs.Add(new ObjectInWorkspace(node.Name, node.GUID.ToString())); }
                }
                currentOutputs.Clear();
                foreach (ObjectInWorkspace item in unorderedOutputs.OrderBy(x => x.Name)) { currentOutputs.Add(item); }
                RaisePropertyChanged(nameof(CurrentOutputsMsg));
                return currentOutputs;
            }
        }

        public void OnResetAllInputsClicked(object obj)
        {
			ResetAllInputs();
			RaisePropertyChanged(nameof(CurrentInputs));
        }

        public void OnResetSelectedInputsClicked(object obj)
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

        public void OnResetAllOutputsClicked(object obj)
        {
			ResetAllOutputs();
            RaisePropertyChanged(nameof(CurrentOutputs));
        }

        public void OnResetSelectedOutputsClicked(object obj)
        {
            foreach (var item in readyParams.CurrentWorkspaceModel.CurrentSelection)
            {
                if (item.IsSetAsOutput) { item.IsSetAsOutput = false; }
            }
            RaisePropertyChanged(nameof(CurrentOutputs));
        }

        public void OnSetSelectedAsOutputClicked(object obj)
        {
            foreach (var item in readyParams.CurrentWorkspaceModel.CurrentSelection)
            {
                if (!item.IsSetAsOutput) { item.IsSetAsOutput = true; }
            }
            RaisePropertyChanged(nameof(CurrentOutputs));
        }

		public void ResetAllInputs()
		{
			foreach (NodeModel node in readyParams.CurrentWorkspaceModel.Nodes)
			{
				if (node.IsSetAsInput) { node.IsSetAsInput = false; }
			}
		}

		public void ResetAllOutputs()
		{
			foreach (NodeModel node in readyParams.CurrentWorkspaceModel.Nodes)
			{
				if (node.IsSetAsOutput) { node.IsSetAsOutput = false; }
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
			batchProcessInputsResults = "Reset all inputs in " + graphCount.ToString() + " graphs...";
			RaisePropertyChanged(nameof(CurrentInputs));
		}

		public void OnBatchResetOutputsClicked(string directoryPath)
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
					ResetAllOutputs();
					viewModel.SaveAsCommand.Execute(graph);
					viewModel.CloseHomeWorkspaceCommand.Execute(null);
					graphCount += 1;
				}
			}
			batchProcessOutputsResults = "Reset all outputs in " + graphCount.ToString() + " graphs...";
			RaisePropertyChanged(nameof(CurrentOutputs));
		}

		private void CurrentWorkspaceModel_NodesChanged(NodeModel obj)
        {
            RaisePropertyChanged(nameof(CurrentInputs));
            RaisePropertyChanged(nameof(CurrentOutputs));
        }
    }
}