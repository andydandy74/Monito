using System;
using Dynamo.Core;
using Dynamo.Extensions;
using Dynamo.Graph.Nodes;
using System.Collections.ObjectModel;
using Dynamo.ViewModels;
using System.Windows;
using System.Linq;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Notes;
using System.Collections.Generic;

namespace Monito
{
    class SearchInWorkspaceViewModel : NotificationObject, IDisposable
    {
        private ReadyParams readyParams;
        private DynamoViewModel viewModel;

        public SearchInWorkspaceViewModel(ReadyParams p, DynamoViewModel vm)
        {
            readyParams = p;
            viewModel = vm;
        }

        public void Dispose() { }

        private string searchTerm;
        /// <summary>
        /// The search term. Changes in the search field will trigger searchInCanvas().
        /// </summary>
        public string SearchTerm
        {
            get
            {
                return searchTerm;
            }
            set
            {
                searchTerm = value;
                RaisePropertyChanged(nameof(SearchResults));
            }
        }

        private ObservableCollection<ObjectInWorkspace> searchResults = new ObservableCollection<ObjectInWorkspace>();
        /// <summary>
        /// The search results as a list representation
        /// </summary>
        public ObservableCollection<ObjectInWorkspace> SearchResults
        {
            get
            {
                searchResults.Clear();
                List<ObjectInWorkspace> unorderedResults = new List<ObjectInWorkspace>();
                Char[] separators = new Char[] { ' ', '.', ',', ':', '(', ')', '!' };
                string[] searchTermParts = searchTerm.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                foreach (NodeModel node in readyParams.CurrentWorkspaceModel.Nodes)
                {
                    // ToDo: search in tags and input values
                    int score = 0;
                    if (node.NickName.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant()))
                    {
                        score += 10;
                    }
                    if (node.Category.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant()))
                    {
                        score += 10;
                    }
                    if (node.CreationName.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant()))
                    {
                        score += 10;
                    }
                    if (node.Description.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant()))
                    {
                        score += 10;
                    }
                    foreach (string part in searchTermParts)
                    {
                        if (node.NickName.ToLowerInvariant().Contains(part.ToLowerInvariant()))
                        {
                            score += 2;
                        }
                        if (node.Category.ToLowerInvariant().Contains(part.ToLowerInvariant()))
                        {
                            score += 1;
                        }
                        if (node.CreationName.ToLowerInvariant().Contains(part.ToLowerInvariant()))
                        {
                            score += 2;
                        }
                        if (node.Description.ToLowerInvariant().Contains(part.ToLowerInvariant()))
                        {
                            score += 1;
                        }
                    }
                    if (score > 0)
                    {
                        unorderedResults.Add(new ObjectInWorkspace(node.NickName + " [Node, Score="+ score.ToString() + "]", node.GUID.ToString(), score));
                    }
                }
               foreach (NoteModel note in viewModel.Model.CurrentWorkspace.Notes)
                {
                    int score = 0;
                    if (note.Text.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant()))
                    {
                        score += 10;
                    }
                    foreach (string part in searchTermParts)
                    {
                        if (note.Text.ToLowerInvariant().Contains(part.ToLowerInvariant()))
                        {
                            score += 1;
                        }
                    }
                    if (score > 0)
                    {
                        unorderedResults.Add(new ObjectInWorkspace(note.Text + " [Text Note, Score=" + score.ToString() + "]", note.GUID.ToString(), score));
                    }
                }
                foreach (AnnotationModel anno in viewModel.Model.CurrentWorkspace.Annotations)
                {
                    int score = 0;
                    if (anno.AnnotationText.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant()))
                    {
                        score += 10;
                    }
                    foreach (string part in searchTermParts)
                    {
                        if (anno.AnnotationText.ToLowerInvariant().Contains(part.ToLowerInvariant()))
                        {
                            score += 1;
                        }
                    }
                    if (score > 0)
                    {
                        unorderedResults.Add(new ObjectInWorkspace(anno.AnnotationText + " [Group, Score=" + score.ToString() + "]", anno.GUID.ToString(), score));
                    }
                }
                foreach (ObjectInWorkspace item in unorderedResults.OrderByDescending(x => x.Score).ThenBy(x => x.Name))
                {
                    searchResults.Add(item);
                }
                return searchResults;
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
                ZoomToObject(value);
            }
        }

        /// <summary>
        /// Zoom in on the object with the given GUID.
        /// </summary>
        private void ZoomToObject(string guid)
        {
            try
            {
                // Clear current selection and select our node
                foreach (var item in readyParams.CurrentWorkspaceModel.Nodes)
                {
                    item.Deselect();
                    item.IsSelected = false;
                }

                bool isNode = readyParams.CurrentWorkspaceModel.Nodes.Count(x => x.GUID.ToString() == guid) > 0;
                bool isNote = viewModel.Model.CurrentWorkspace.Notes.Count(x => x.GUID.ToString() == guid) > 0;
                bool isAnno = viewModel.Model.CurrentWorkspace.Annotations.Count(x => x.GUID.ToString() == guid) > 0;

                // Zoom in on our node and deselect it again
                // BUG: Apparently this does NOT remove the node from the selection again
                // so each time we click on another button we add one more node to our selection
                // which results in only the first zoom operation being successful
                viewModel.CurrentSpaceViewModel.ResetFitViewToggleCommand.Execute(null);
                if (isNode)
                {
                    var zoomNode = readyParams.CurrentWorkspaceModel.Nodes.First(x => x.GUID.ToString() == guid);
                    viewModel.AddToSelectionCommand.Execute(zoomNode);
                    viewModel.FitViewCommand.Execute(null);
                    zoomNode.Deselect();
                    zoomNode.IsSelected = false;
                }
                else if (isNote)
                {
                    var zoomNote = viewModel.Model.CurrentWorkspace.Notes.First(x => x.GUID.ToString() == guid);
                    viewModel.AddToSelectionCommand.Execute(zoomNote);
                    viewModel.FitViewCommand.Execute(null);
                    zoomNote.Deselect();
                    zoomNote.IsSelected = false;
                }
                else if (isAnno)
                {
                    var zoomAnno = viewModel.Model.CurrentWorkspace.Annotations.First(x => x.GUID.ToString() == guid);
                    viewModel.AddToSelectionCommand.Execute(zoomAnno);
                    viewModel.FitViewCommand.Execute(null);
                    zoomAnno.Deselect();
                    zoomAnno.IsSelected = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}