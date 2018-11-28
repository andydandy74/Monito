using Dynamo.Models;
using Dynamo.ViewModels;
using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Monito
{
	/// <summary>
	/// Shared utility functions for view models
	/// </summary>
	class ViewModelUtils
    {
        private DynamoViewModel viewModel;
        private Window dynWindow;
        
        public ViewModelUtils(DynamoViewModel vm, Window dw)
        {
            viewModel = vm;
            dynWindow = dw;
        }
        /// <summary>
        /// Zoom in on the object with the given GUID.
        /// </summary>
        public void ZoomToObject(string guid)
        {
			GeneralUtils.ClearSelection();
			viewModel.CurrentSpaceViewModel.ResetFitViewToggleCommand.Execute(null);
			if (viewModel.Model.CurrentWorkspace.Nodes.Count(x => x.GUID.ToString() == guid) > 0)
			{
				var zoomObject = viewModel.Model.CurrentWorkspace.Nodes.First(x => x.GUID.ToString() == guid);
				viewModel.AddToSelectionCommand.Execute(zoomObject);
			}
            else if (viewModel.Model.CurrentWorkspace.Notes.Count(x => x.GUID.ToString() == guid) > 0)
			{
				var zoomObject = viewModel.Model.CurrentWorkspace.Notes.First(x => x.GUID.ToString() == guid);
				viewModel.AddToSelectionCommand.Execute(zoomObject);
			}
			// CurrentWorkspace.Annotations has a deprecation warning
			// but for now this seems like the only simple way of adding annotation to the current selection.
            else if (viewModel.Model.CurrentWorkspace.Annotations.Count(x => x.GUID.ToString() == guid) > 0)
			{
				var zoomObject = viewModel.Model.CurrentWorkspace.Annotations.First(x => x.GUID.ToString() == guid);
				viewModel.AddToSelectionCommand.Execute(zoomObject);
			}
			viewModel.FitViewCommand.Execute(null);
			GeneralUtils.ClearSelection();
		}
    }

    /// <summary>
    /// Shared general utility functions
    /// </summary>
    static class GeneralUtils
    {
        public static string Abbreviate(this String str)
        {
            str = str.Replace(Environment.NewLine, " ");
            if (str.Length > 60)
            {
                str = str.Substring(0, 60) + "...";
            }
            return str;
        }

        public static bool GetLoadedSettingAsBoolean(this KeyValueConfigurationCollection loadedsettings, string key)
        {
            if (loadedsettings[key] != null && loadedsettings[key].Value == "1") { return true; }
            else { return false; }
        }

        public static void ClearSelection()
        {
            var dynamoSelection = typeof(DynamoModel).Assembly.GetType("Dynamo.Selection.DynamoSelection");
            var selectionInstance = dynamoSelection.GetProperty("Instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            var clearMethod = dynamoSelection.GetMethod("ClearSelection", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            clearMethod.Invoke(selectionInstance.GetValue(null), null);
        }
    }

    /// <summary>
    /// Class for structured storage of objects in workspace (nodes, text notes, groups).
    /// </summary>
    class ObjectInWorkspace
    {
        private string objectName;
        private string objectGUID;
        private double objectScore;
        private string objectDetails;

        public ObjectInWorkspace(string name, string guid, double score = 0, string details = "")
        {
            objectName = name;
            objectGUID = guid;
            objectScore = score;
            objectDetails = details;
        }

        public string Name
        {
            get { return objectName; }
        }

        public string GUID
        {
            get { return objectGUID; }
        }

        public double Score
        {
            get { return objectScore; }
        }

        public string Details
        {
            get { return objectDetails; }
        }
    }
}
