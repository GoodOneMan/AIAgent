using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIAgent.Structures
{
    public class ElementDto
    {
        public class Property
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public string Name { get; set; }
        public List<Property> Properties { get; set; } = new List<Property>();

        public ElementDto(Element element)
        {
            this.Name = element.ModelItem.DisplayName;

            if(element.PropertyCategories != null)
                foreach (var category in element.PropertyCategories)
                {
                    foreach (var property in category.Properties)
                    {
                        Properties.Add(new Property
                        {
                            Name = property.DisplayName,
                            Value = property.Value.ToString()
                        });
                    }
                }
        }
    }
}
