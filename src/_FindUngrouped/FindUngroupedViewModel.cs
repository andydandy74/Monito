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
using Dynamo.Graph.Notes;
using Dynamo.Graph.Annotations;

namespace Monito
{
    class FindUngroupedViewModel : NotificationObject, IDisposable
    {
        private ReadyParams readyParams;
        private DynamoViewModel viewModel;
        public ICommand FixUngrouped { get; set; }

        public FindUngroupedViewModel(ReadyParams p, DynamoViewModel vm)
        {
            readyParams = p;
            viewModel = vm;
            p.CurrentWorkspaceModel.NodeAdded += CurrentWorkspaceModel_NodesChanged;
            p.CurrentWorkspaceModel.NodeRemoved += CurrentWorkspaceModel_NodesChanged;
            viewModel.CurrentSpace.NoteAdded += CurrentWorkspaceModel_NotesChanged;
            viewModel.CurrentSpace.NoteRemoved += CurrentWorkspaceModel_NotesChanged;
            viewModel.CurrentSpace.AnnotationAdded += CurrentWorkspaceModel_AnnotationChanged;
            viewModel.CurrentSpace.AnnotationRemoved += CurrentWorkspaceModel_AnnotationChanged;
            FixUngrouped = new DelegateCommand(OnFixUngroupedClicked);
        }

        public void Dispose()
        {
            readyParams.CurrentWorkspaceModel.NodeAdded -= CurrentWorkspaceModel_NodesChanged;
            readyParams.CurrentWorkspaceModel.NodeRemoved -= CurrentWorkspaceModel_NodesChanged;
            viewModel.CurrentSpace.NoteAdded -= CurrentWorkspaceModel_NotesChanged;
            viewModel.CurrentSpace.NoteRemoved -= CurrentWorkspaceModel_NotesChanged;
            viewModel.CurrentSpace.AnnotationAdded -= CurrentWorkspaceModel_AnnotationChanged;
            viewModel.CurrentSpace.AnnotationRemoved -= CurrentWorkspaceModel_AnnotationChanged;
        }

        private string currentUngroupedMsg;
        public string CurrentUngroupedMsg
        {
            get
            {
                if (currentUngrouped.Count > 0) { currentUngroupedMsg = "All ungrouped nodes and text notes in current workspace:"; }
                else { currentUngroupedMsg = "No ungrouped nodes or text notes in current workspace..."; }
                return currentUngroupedMsg;
            }
        }

        private ObservableCollection<ObjectInWorkspace> currentUngrouped = new ObservableCollection<ObjectInWorkspace>();
        /// <summary>
        /// The search results as a list representation
        /// </summary>
        public ObservableCollection<ObjectInWorkspace> CurrentUngrouped
        {
            get
            {
                List<string> allGroupedObjects = new List<string>();
                foreach (var anno in viewModel.CurrentSpaceViewModel.Annotations)
                {
                    foreach (var member in anno.AnnotationModel.SelectedModels)
                    {
                        allGroupedObjects.Add(member.GUID.ToString());
                    }
                }
                List<ObjectInWorkspace> unorderedUngrouped = new List<ObjectInWorkspace>();
                foreach (var node in viewModel.CurrentSpaceViewModel.Nodes)
                {
                    if (!allGroupedObjects.Contains(node.NodeModel.GUID.ToString()))
                    {
                        unorderedUngrouped.Add(new ObjectInWorkspace(node.NickName.Abbreviate(), node.NodeModel.GUID.ToString(), 0, "", "Node"));
                    }
                }
                foreach (var note in viewModel.CurrentSpaceViewModel.Notes)
                {
                    if (!allGroupedObjects.Contains(note.Model.GUID.ToString()))
                    {
                        unorderedUngrouped.Add(new ObjectInWorkspace(note.Text.Abbreviate(), note.Model.GUID.ToString(), 0, "", "Text Note"));
                    }
                }
                currentUngrouped.Clear();
                foreach (ObjectInWorkspace item in unorderedUngrouped.OrderBy(x => x.Name)) { currentUngrouped.Add(item); }
                RaisePropertyChanged(nameof(CurrentUngroupedMsg));
                return currentUngrouped;
            }
        }

        public void OnFixUngroupedClicked(object obj)
        {
            RaisePropertyChanged(nameof(CurrentUngrouped));
            foreach (var ungrouped in currentUngrouped)
            {
                if (viewModel.Model.CurrentWorkspace.Nodes.Count(x => x.GUID.ToString() == ungrouped.GUID) > 0)
                {
                    var ungroupedNode = viewModel.Model.CurrentWorkspace.Nodes.First(x => x.GUID.ToString() == ungrouped.GUID);
                    foreach (var anno in viewModel.CurrentSpaceViewModel.Annotations)
                    {
                        if (anno.AnnotationModel.Rect.Contains(ungroupedNode.Rect.TopLeft)
                            || anno.AnnotationModel.Rect.Contains(ungroupedNode.Rect.TopRight)
                            || anno.AnnotationModel.Rect.Contains(ungroupedNode.Rect.BottomLeft)
                            || anno.AnnotationModel.Rect.Contains(ungroupedNode.Rect.BottomRight))
                        {
                            anno.AnnotationModel.Select();
                            readyParams.CommandExecutive.ExecuteCommand(new DynamoModel.AddModelToGroupCommand(ungroupedNode.GUID.ToString()), "d8fcfe56-81e0-4e95-84af-d945ebd6478b", "DynaMonito");
                            anno.AnnotationModel.Deselect();
                        }
                    }
                }
                else if (viewModel.Model.CurrentWorkspace.Notes.Count(x => x.GUID.ToString() == ungrouped.GUID) > 0)
                {
                    var ungroupedNote = viewModel.Model.CurrentWorkspace.Notes.First(x => x.GUID.ToString() == ungrouped.GUID);
                    foreach (var anno in viewModel.CurrentSpaceViewModel.Annotations)
                    {
                        if (anno.AnnotationModel.Rect.Contains(ungroupedNote.Rect.TopLeft)
                            || anno.AnnotationModel.Rect.Contains(ungroupedNote.Rect.TopRight)
                            || anno.AnnotationModel.Rect.Contains(ungroupedNote.Rect.BottomLeft)
                            || anno.AnnotationModel.Rect.Contains(ungroupedNote.Rect.BottomRight))
                        {
                            anno.AnnotationModel.Select();
                            readyParams.CommandExecutive.ExecuteCommand(new DynamoModel.AddModelToGroupCommand(ungroupedNote.GUID.ToString()), "d8fcfe56-81e0-4e95-84af-d945ebd6478b", "DynaMonito");
                            anno.AnnotationModel.Deselect();
                        }
                    }
                }
                
            }
            RaisePropertyChanged(nameof(CurrentUngrouped));
        }

        private void CurrentWorkspaceModel_NodesChanged(NodeModel obj)
        {
            RaisePropertyChanged(nameof(CurrentUngrouped));
        }

        private void CurrentWorkspaceModel_NotesChanged(NoteModel obj)
        {
            RaisePropertyChanged(nameof(CurrentUngrouped));
        }

        private void CurrentWorkspaceModel_AnnotationChanged(AnnotationModel obj)
        {
            RaisePropertyChanged(nameof(CurrentUngrouped));
        }
    }
}