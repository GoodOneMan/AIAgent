using Autodesk.Navisworks.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AIAgent.Structures
{
    public class Element
    {
        public string PartitionName;
        public string SourceFileName;
        public double Measurement;
        public Geometry Geometry;
        public int VetexCount;
        public ModelItem ModelItem;
        public int PropertyId;
        public List<PropertyCategory> PropertyCategories;

        public Element(ModelItem item)
        {
            foreach (ModelItem part in item.Ancestors)
            {
                if (part.HasModel)
                {
                    //Measurement = Structures.Measurement.GetValue(part.Model.Units.ToString());
                    //LcOaPartition partition = LcOpState.GetActiveInstance().PathToUnitPartition(item);
                    //string name;
                    //partition.GetSwigUserName(out name);
                    //var scn = partition.GetSwigClassName();
                    //var file_name = partition.GetFilename();
                    ////var trans3d = partition.GetFileTransform();
                    //var crearor = partition.GetCreator();
                    //var uv = partition.GetFrontVector();
                    //var sour_file = partition.GetSourceFilename();

                    //var dtatproperty = part.PropertyCategories.FindPropertyByCombinedName(new NamedConstant("LcOaNode", "Элемент"), new NamedConstant("LcOaNode", "Элемент"));

                    PartitionName = part.DisplayName;
                    SourceFileName = part.Model.SourceFileName;

                    break;
                }
            }
        }
    }
}
