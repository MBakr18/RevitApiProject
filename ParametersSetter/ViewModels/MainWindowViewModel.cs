using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Macros;
using Autodesk.Revit.UI;
using OfficeOpenXml;
using ParametersSetter.Application;
using ParametersSetter.Command;
using ParametersSetter.Models;

namespace ParametersSetter.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {

        #region Constructor
        public MainWindowViewModel()
        {
            ModifyCommand = new MyCommand(ModifyCommandMethod, CanModifyCommand);
            ExportCommand = new MyCommand(ExportCommandMethod, CanExportCommand);
            ImportCommand = new MyCommand(ImportCommandMethod, CanImportCommand);
        }


        #endregion


        #region Properties

        public List<CategoryModel> CategoryList { get; set; } = new List<CategoryModel>()
        {
            new CategoryModel(){CategoryName="Columns",Category=BuiltInCategory.OST_StructuralColumns},
            new CategoryModel(){CategoryName="Walls",Category=BuiltInCategory.OST_Walls},
            new CategoryModel(){CategoryName="Windows",Category=BuiltInCategory.OST_Windows},
            new CategoryModel(){CategoryName="Doors",Category=BuiltInCategory.OST_Doors},
            new CategoryModel(){CategoryName="Annotation Symbols",Category=BuiltInCategory.OST_StructuralAnnotations},
        };
        public ObservableCollection<FamilyModel> FamilyList { get; set; } = new ObservableCollection<FamilyModel>();
        public ObservableCollection<TypeModel> TypeList { get; set; } = new ObservableCollection<TypeModel>();
        public ObservableCollection<ParameterInfoModel> ParameterInfoList { get; set; } = new ObservableCollection<ParameterInfoModel>();

        private CategoryModel _selectedCategory;
        public CategoryModel SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    OnPropertyChanged(); // Implement INotifyPropertyChanged

                    // Call the method to update FamilyList
                    UpdateFamilyList();
                }
            }
        }

        private FamilyModel _selectedFamily;
        public FamilyModel SelectedFamily
        {
            get => _selectedFamily;
            set
            {
                if (_selectedFamily != value)
                {
                    _selectedFamily = value;
                    OnPropertyChanged(); // Implement INotifyPropertyChanged

                    // Call the method to update TypeList
                    UpdateTypeList();
                }
            }
        }

        private TypeModel _selectedType;
        public TypeModel SelectedType
        {
            get => _selectedType;
            set
            {
                if (_selectedType != value)
                {
                    _selectedType = value;
                    OnPropertyChanged(); // Implement INotifyPropertyChanged

                    PopulateParameterInfo();
                }
            }
        }

        private ParameterInfoModel _selectedParameter;
        public ParameterInfoModel SelectedParameter
        {
            get => _selectedParameter;
            set
            {
                if (_selectedParameter != value)
                {
                    _selectedParameter = value;

                    // Update the TextBox with the selected parameter value
                    EditedParameterValue = _selectedParameter?.ParameterValue;

                    OnPropertyChanged(); // Implement INotifyPropertyChanged

                }
            }
        }
        private string _editedParameterValue;
        public string EditedParameterValue
        {
            get => _editedParameterValue;
            set
            {
                if (_editedParameterValue != value)
                {
                    _editedParameterValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public MyCommand ModifyCommand { get; set; }
        public MyCommand ExportCommand { get; set; }
        public MyCommand ImportCommand { get; set; }


        #endregion

        #region Methods

        private void UpdateFamilyList()
        {
            FamilyList.Clear(); // Clear the existing list

            Document document = RevitCommand.Doc;

            List<Family> allFamilies = new FilteredElementCollector(document)
                .OfClass(typeof(Family))
                .ToElements()
                .Cast<Family>()
                .FilterFamilyByCategory(SelectedCategory.Category);

            foreach (var element in allFamilies)
            {
                var familySymbolIds = element.GetFamilySymbolIds();
                var familySymbols = familySymbolIds.Select(id => document.GetElement(id) as FamilySymbol).ToList();

                FamilyModel familyModel = new FamilyModel()
                {
                    FamilyName = element.Name,
                    FamilySymbols = familySymbols
                };
                FamilyList.Add(familyModel);
            }
        }
        private void UpdateTypeList()
        {
            TypeList.Clear(); // Clear the existing list

            Document document = RevitCommand.Doc;

            if (SelectedFamily != null && SelectedFamily.FamilySymbols != null) // Ensure selectedFamily and its FamilySymbols are not null
            {
                var selectedSymbolIds = new HashSet<ElementId>(SelectedFamily.FamilySymbols.Select(symbol => symbol.Id));

                var allTypes = new FilteredElementCollector(document)
                    .OfClass(typeof(FamilySymbol))
                    .Cast<FamilySymbol>()
                    .Where(symbol => selectedSymbolIds.Contains(symbol.Id)) // Check if the symbol Id is in the selectedFamily's FamilySymbols
                    .ToList();

                foreach (var symbol in allTypes)
                {
                    TypeModel typeModel = new TypeModel()
                    {
                        TypeName = symbol.Name,
                        TypeId = symbol.Id,
                        TypeParameters = symbol.Parameters
                    };
                    TypeList.Add(typeModel);
                }
            }
        }
        private void PopulateParameterInfo()
        {
            ParameterInfoList.Clear(); // Clear the existing list

            if (SelectedType != null)
            {
                // Access the parameters of the selected type (assuming TypeParameters represents the parameters)
                var typeParameters = SelectedType.TypeParameters;

                if (typeParameters != null)
                {
                    foreach (Parameter parameter in typeParameters)
                    {
                        // Assuming parameter is of type Parameter
                        if (!parameter.IsReadOnly)
                        {
                            ParameterInfoModel parameterInfo = new ParameterInfoModel
                            {
                                ParameterName = parameter.Definition.Name,
                                ParameterValue = parameter.AsValueString() // Get the parameter value as a string
                            };
                            ParameterInfoList.Add(parameterInfo);
                        }
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Command Methods

        private bool CanImportCommand(object obj)
        {
            return true; // Enable the button to always allow importing
        }

        private void ImportCommandMethod(object obj)
        {
            using (Transaction transaction = new Transaction(RevitCommand.Doc, "Import Data"))
            {
                try
                {
                    transaction.Start();

                    // Call a method to import data from Excel
                    ImportFromExcel();

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    // Handle exceptions and roll back the transaction if necessary
                    transaction.RollBack();
                    MessageBox.Show($"Error importing data: {ex.Message}", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }

        private bool CanExportCommand(object obj)
        {
            return true;
        }

        private void ExportCommandMethod(object obj)
        {
            ExportToExcel();

        }
        private bool CanModifyCommand(object obj)
        {
            return true;
        }

        private void ModifyCommandMethod(object obj)
        {
            if (SelectedParameter != null)
            {

                Document document = RevitCommand.Doc;

                ElementId elementId = SelectedType.TypeId;
                Element element = document.GetElement(elementId);

                using (Transaction tr = new Transaction(document, "Modify Type Parameter"))
                {
                    try
                    {
                        tr.Start();
                        if (element != null)
                        {
                            string parameterName = SelectedParameter.ParameterName;

                            Parameter parameter = element.LookupParameter(parameterName);
                            if (parameter != null)
                            {
                                if (!parameter.IsReadOnly) // Check if the parameter is read-only
                                {
                                    // Modify the parameter value
                                    parameter.Set(EditedParameterValue);
                                    SelectedParameter.ParameterValue = EditedParameterValue;
                                }
                                else
                                {
                                    TaskDialog.Show("Modification Error", "The parameter is read-only.");
                                }
                            }
                            else
                            {
                                TaskDialog.Show("Modification Error", "The parameter does not exist.");
                            }
                        }


                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                       TaskDialog.Show("Modification Error", ex.Message);
                       tr.RollBack();
                    }
                }

                // Notify that the property has changed so that DataGrid updates the value
                OnPropertyChanged(nameof(SelectedParameter));
            }
        }

        private void ExportToExcel()
        {
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("ParameterInfo");

                // Add headers
                int rowIndex = 1;
                int columnIndex = 1;
                worksheet.Cells[rowIndex, columnIndex++].Value = "Parameter Name";
                worksheet.Cells[rowIndex, columnIndex++].Value = "Parameter Value";

                foreach (ParameterInfoModel parameterInfo in ParameterInfoList)
                {
                    rowIndex++;
                    columnIndex = 1;
                    worksheet.Cells[rowIndex, columnIndex++].Value = parameterInfo.ParameterName;
                    worksheet.Cells[rowIndex, columnIndex++].Value = parameterInfo.ParameterValue;
                }

                // Save the Excel file
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
                if (saveFileDialog.ShowDialog() == true)
                {
                    FileInfo file = new FileInfo(saveFileDialog.FileName);
                    package.SaveAs(file);
                    MessageBox.Show("Data exported successfully!", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);

                    System.Diagnostics.Process.Start(file.FullName);
                }
            }
        }
        private void ImportFromExcel()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var package = new ExcelPackage(new FileInfo(openFileDialog.FileName)))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                        if (worksheet != null)
                        {
                            int startRow = 2; // Assuming headers are in the first row
                            int endRow = worksheet.Dimension?.End.Row ?? 0;

                            for (int row = startRow; row <= endRow; row++)
                            {
                                string paramName = worksheet.Cells[row, 1].Value?.ToString();
                                string paramValue = worksheet.Cells[row, 2].Value?.ToString();

                                if (!string.IsNullOrEmpty(paramName) && !string.IsNullOrEmpty(paramValue))
                                {
                                    // Check if the parameter name already exists in the list and update it
                                    var existingParameter = ParameterInfoList.FirstOrDefault(p => p.ParameterName == paramName);
                                    if (existingParameter != null)
                                    {
                                        existingParameter.ParameterValue = paramValue;
                                    }
                                    else
                                    {
                                        // Create a new ParameterInfoModel and add it to your ObservableCollection
                                        var parameterInfo = new ParameterInfoModel
                                        {
                                            ParameterName = paramName,
                                            ParameterValue = paramValue
                                        };
                                        ParameterInfoList.Add(parameterInfo);
                                    }
                                }
                            }

                            MessageBox.Show("Data imported successfully!", "Import Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error importing data: {ex.Message}", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

    }

    public static class MyExtension
    {
        public static List<Family> FilterFamilyByCategory(this IEnumerable<Family> families, BuiltInCategory builtInCategory)
        {
            List<Family> filteredFamilies = new List<Family>();
            foreach (var fam in families)
            {
                if (fam.FamilyCategoryId == new ElementId(builtInCategory))
                    filteredFamilies.Add(fam);
            }
            return filteredFamilies;
        }
        
    }
}
