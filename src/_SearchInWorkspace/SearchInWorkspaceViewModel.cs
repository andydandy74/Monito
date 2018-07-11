using System;
using Dynamo.Core;
using Dynamo.Extensions;
using Dynamo.Graph.Nodes;
using System.Collections.ObjectModel;
using Dynamo.ViewModels;
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
            get { return searchTerm; }
            set
            {
                searchTerm = value;
                RaisePropertyChanged(nameof(SearchResults));
            }
        }

        private bool searchInNicknames = true;
        /// <summary>
        /// Include node nicknames in search?
        /// </summary>
        public bool SearchInNicknames
        {
            get { return searchInNicknames; }
            set
            {
                searchInNicknames = value;
                RaisePropertyChanged(nameof(SearchResults));
            }
        }

        private bool searchInOriginalNames = true;
        /// <summary>
        /// Include original node names in search?
        /// </summary>
        public bool SearchInOriginalNames
        {
            get { return searchInOriginalNames; }
            set
            {
                searchInOriginalNames = value;
                RaisePropertyChanged(nameof(SearchResults));
            }
        }

        private bool searchInCategories = true;
        /// <summary>
        /// Include node categories in search?
        /// </summary>
        public bool SearchInCategories
        {
            get { return searchInCategories; }
            set
            {
                searchInCategories = value;
                RaisePropertyChanged(nameof(SearchResults));
            }
        }

        private bool searchInDescriptions = true;
        /// <summary>
        /// Include node descriptions in search?
        /// </summary>
        public bool SearchInDescriptions
        {
            get { return searchInDescriptions; }
            set
            {
                searchInDescriptions = value;
                RaisePropertyChanged(nameof(SearchResults));
            }
        }

        private bool searchInTags = true;
        /// <summary>
        /// Include node tags in search?
        /// </summary>
        public bool SearchInTags
        {
            get { return searchInTags; }
            set
            {
                searchInTags = value;
                RaisePropertyChanged(nameof(SearchResults));
            }
        }

        private bool searchInNotes = true;
        /// <summary>
        /// Include text notes in search?
        /// </summary>
        public bool SearchInNotes
        {
            get { return searchInNotes; }
            set
            {
                searchInNotes = value;
                RaisePropertyChanged(nameof(SearchResults));
            }
        }

        private bool searchInAnnotations = true;
        /// <summary>
        /// Include group titles in search?
        /// </summary>
        public bool SearchInAnnotations
        {
            get { return searchInAnnotations; }
            set
            {
                searchInAnnotations = value;
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
                if (searchTerm != "" && searchTerm != " ")
                {
                    List<ObjectInWorkspace> unorderedResults = new List<ObjectInWorkspace>();
                    Char[] separators = new Char[] { ' ', '.', ',', ':', '(', ')', '!' };
                    string[] searchTermParts = searchTerm.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    int nodesScoreFactor = 0;
                    if (searchInNicknames) { nodesScoreFactor += 1; }
                    if (searchInOriginalNames) { nodesScoreFactor += 1; }
                    if (searchInCategories) { nodesScoreFactor += 1; }
                    if (searchInDescriptions) { nodesScoreFactor += 1; }
                    if (searchInNicknames || searchInOriginalNames || searchInCategories || searchInDescriptions)
                    {
                        foreach (NodeModel node in readyParams.CurrentWorkspaceModel.Nodes)
                        {
                            // ToDo: search in tags and input values
                            int rawScore = 0;
                            double weightedScore = 0;
                            if (searchInNicknames && node.NickName.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant())) { rawScore += 10; }
                            if (searchInCategories && node.Category.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant())) { rawScore += 10; }
                            if (searchInOriginalNames)
                            {
                                if (node.GetType().Name == "DSFunction" && node.CreationName.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant())) { rawScore += 10; }
                                else if (node.GetType().Name != "DSFunction" && node.GetType().Name.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant())) { rawScore += 10; }
                                // Still haven't found out how to access original custom node names
                            }
                            if (searchInDescriptions && node.Description.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant())) { rawScore += 10; }
                            if (searchInTags)
                            {
                                foreach (string tag in node.Tags)
                                {
                                    if (tag.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant())) { rawScore += 2; }
                                    foreach (string part in searchTermParts)
                                    {
                                        if (tag.ToLowerInvariant().Contains(part.ToLowerInvariant())) { rawScore += 1; }
                                    }
                                }
                            }   
                            foreach (string part in searchTermParts)
                            {
                                if (searchInNicknames && node.NickName.ToLowerInvariant().Contains(part.ToLowerInvariant())) { rawScore += 2; }
                                if (searchInCategories && node.Category.ToLowerInvariant().Contains(part.ToLowerInvariant())) { rawScore += 1; }
                                if (searchInOriginalNames) 
                                {
                                    if (node.GetType().Name == "DSFunction" && node.CreationName.ToLowerInvariant().Contains(part.ToLowerInvariant())) { rawScore += 2; }
                                    else if (node.GetType().Name != "DSFunction" && node.GetType().Name.ToLowerInvariant().Contains(part.ToLowerInvariant())) { rawScore += 2; }
                                }
                                if (searchInDescriptions && node.Description.ToLowerInvariant().Contains(part.ToLowerInvariant())) { rawScore += 1; }
                            }
                            weightedScore = rawScore / (10d + searchTermParts.Length) / nodesScoreFactor;
                            if (rawScore > 0)
                            {
                                string toolTip = "Search score: " + weightedScore.ToString();
                                if (searchInOriginalNames && node.GetType().Name == "DSFunction" && node.CreationName != node.NickName && node.CreationName != "")
                                {
                                    toolTip += "\nOriginal name: " + node.CreationName;
                                }
                                else if (searchInOriginalNames && node.GetType().Name != "DSFunction" && node.GetType().Name != "Function" && node.GetType().Name != node.NickName && node.GetType().Name != "")
                                {
                                    toolTip += "\nOriginal name: " + node.GetType().Name;
                                }
                                if (searchInCategories && node.Category != "") { toolTip += "\nCategory: " + node.Category; }
                                if (searchInDescriptions && node.Description != "") { toolTip += "\nDescription: " + node.Description; }
                                if (searchInTags && node.Tags.Count > 0)
                                {
                                    toolTip += "\nTags: " + String.Join(", ", node.Tags.ToArray());
                                    toolTip = toolTip.TrimEnd();
                                    if (toolTip[toolTip.Length-1] == ',') { toolTip = toolTip.Remove(toolTip.Length - 1); }
                                }
                                unorderedResults.Add(new ObjectInWorkspace(node.NickName.Abbreviate() + " [Node]", node.GUID.ToString(), weightedScore, toolTip));
                            }
                        }
                    }
                    if (searchInNotes)
                    {
                        foreach (NoteModel note in viewModel.Model.CurrentWorkspace.Notes)
                        {
                            int rawScore = 0;
                            double weightedScore = 0;
                            if (note.Text.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant())) { rawScore += 10; }
                            foreach (string part in searchTermParts)
                            {
                                if (note.Text.ToLowerInvariant().Contains(part.ToLowerInvariant())) { rawScore += 1; }
                            }
                            weightedScore = rawScore / (10d + searchTermParts.Length);
                            if (rawScore > 0) { unorderedResults.Add(new ObjectInWorkspace(note.Text.Abbreviate() + " [Text Note]", note.GUID.ToString(), weightedScore, "Search score: " + weightedScore.ToString() + "\n\n" + note.Text)); }
                        }
                    }
                    if (searchInAnnotations)
                    {
                        foreach (AnnotationModel anno in viewModel.Model.CurrentWorkspace.Annotations)
                        {
                            int rawScore = 0;
                            double weightedScore = 0;
                            if (anno.AnnotationText.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant())) { rawScore += 10; }
                            foreach (string part in searchTermParts)
                            {
                                if (anno.AnnotationText.ToLowerInvariant().Contains(part.ToLowerInvariant())) { rawScore += 1; }
                            }
                            weightedScore = rawScore / (10d + searchTermParts.Length);
                            if (rawScore > 0) { unorderedResults.Add(new ObjectInWorkspace(anno.AnnotationText.Abbreviate() + " [Group]", anno.GUID.ToString(), weightedScore, "Search score: " + weightedScore.ToString() + "\n\n" + anno.AnnotationText)); }
                        }
                    }
                    foreach (ObjectInWorkspace item in unorderedResults.OrderByDescending(x => x.Score).ThenBy(x => x.Name)) { searchResults.Add(item); }
                }               
                return searchResults;
            }
        }
    }
}