using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace ParametersSetter.Models
{
    public class FamilyModel
    {
        public string FamilyName { get; set; }
        public List<FamilySymbol> FamilySymbols { get; set; }

        public override string ToString()
        {
            return FamilyName;
        }
    }
}
