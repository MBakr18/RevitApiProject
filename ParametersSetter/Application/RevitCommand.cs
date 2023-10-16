#region Namespaces

using System;
using System.Diagnostics;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using ParametersSetter.Views;

#endregion

namespace ParametersSetter.Application
{
    [Transaction(TransactionMode.Manual)]
    public class RevitCommand : IExternalCommand
    {
        public static Document Doc { get; set; }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Doc = uidoc.Document;

            try
            {
                MainWindow programWindow = new MainWindow();
                programWindow.ShowDialog();
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error MSG", $"{ex.Message}");
                return Result.Failed;
            }
        }
    }
}
