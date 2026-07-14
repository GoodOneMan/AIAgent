using AIAgent.Structures;
using Autodesk.Navisworks.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace AIAgent.Model
{
    internal class InformationExtractor
    {
        List<Element> elements = null;

        public InformationExtractor(List<Element> elements)
        {
            this.elements = elements;
        }

        public List<Element> GetElements()
        {
            foreach (Element element in elements)
            {
                element.PropertyCategories = DataItem(element.ModelItem);
            }

            return elements;
        }

        private List<PropertyCategory> DataItem(ModelItem item)
        {
            List<PropertyCategory> categories = new List<PropertyCategory>();

            while (true)
            {
                ModelItem parent = item.Parent;

                if (parent == null)
                    return null;

                foreach (var str in Configuration.Instance.Categories)
                {
                    var arr = str.Split(':');
                    PropertyCategory category = parent.PropertyCategories.FindCategoryByCombinedName(new NamedConstant(arr[0].Trim(), arr[1].Trim()));

                    if (category != null)
                        categories.Add(category);

                }


                if (categories.Count == 0)
                {
                    item = parent;
                    Console.WriteLine($"{item.GetHashCode()}  not data");
                }
                else
                {
                    Console.WriteLine($"{item.GetHashCode()}  find data");
                    return categories;
                }
            }
        }
    }
}
