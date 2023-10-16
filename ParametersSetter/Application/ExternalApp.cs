#region Namespaces

using System;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

#endregion

namespace ParametersSetter.Application
{
    internal class ExternalApp : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            application.CreateRibbonTab("MB Tools");
            RibbonPanel ribbonPanel = application.CreateRibbonPanel("MB Tools", "Parameters Modification");

            string path = Assembly.GetExecutingAssembly().Location;
            PushButton setterBtn = ribbonPanel.AddItem(new PushButtonData("Btn1", "Parameter setter", path,
                "ParametersSetter.Application.RevitCommand")) as PushButton;

            Uri uri = new Uri(@"E:\Mine\02-PROGRAMMING\Codes\01- BIMAD\03-4 Autodesk Revit API\RevitApiProject\ParametersSetter\Assets\MBLogo.png");
            BitmapImage largeImage = new BitmapImage(uri);

            if (setterBtn != null) setterBtn.LargeImage = largeImage;
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
