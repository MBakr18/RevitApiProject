using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParametersSetter.Models
{
    public class TypeModel
    {
        public string TypeName { get; set; }
        public ElementId TypeId { get; set; }
        public ParameterSet TypeParameters { get; set; }

        public override string ToString()
        {
            return TypeName;
        }
    }
}
