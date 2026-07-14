using AIAgent.Structures;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.ComApi;
using Autodesk.Navisworks.Api.Interop.ComApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace AIAgent.Model
{
    class GeometryExtractor : Extractor
    {
        int counter = 0;
        Dictionary<ModelItem, List<Element>> Elements = new Dictionary<ModelItem, List<Element>>();
        Dictionary<int, int> duplicate_geometry = new Dictionary<int, int>();

        double measurement = 0;

        public GeometryExtractor() { }

        public Dictionary<ModelItem, List<Element>> Run()
        {
            if (Application.ActiveDocument.Models.Count == 0)
                return null;

            Dictionary<ModelItem, ModelItemCollection> models_selection = new Dictionary<ModelItem, ModelItemCollection>();

            measurement = Measurement.GetValue(Application.ActiveDocument.Models[0].Units.ToString());

            foreach (ModelItem item in Application.ActiveDocument.CurrentSelection.SelectedItems)
            {
                models_selection.Add(item, new ModelItemCollection());

                if (item.HasGeometry)
                {
                    models_selection[item].Add(item);
                }
                else
                {
                    models_selection[item].AddRange(SearchByCategory(new ModelItemCollection() { item }, "LcOpGeometryProperty", "Геометрия"));
                }

                counter += models_selection[item].Count;
            }

            foreach (var item in models_selection)
            {
                if (Configuration.Instance.GeometryHandler == 1)
                {
                    Elements.Add(item.Key, GeometryHandlerFast(item.Value));
                }
                else
                {
                    Elements.Add(item.Key, GeometryHandler(item.Value));
                }
            }

            return Elements;
        }

        private List<Element> GeometryHandlerFast(ModelItemCollection collection)
        {
            List<Element> elements = new List<Element>();

            foreach (var item in collection)
            {
                if (duplicate_geometry.ContainsKey(item.InstanceHashCode))
                {
                    duplicate_geometry[item.InstanceHashCode] += 1;
                }
                else
                {
                    duplicate_geometry.Add(item.InstanceHashCode, 1);

                    ModelGeometry geometry = item.Geometry;

                    SimplePrimitivesImplementation SimplePrimitivesCB = new SimplePrimitivesImplementation();
                    InwOaPath oaPath = ComApiBridge.ToInwOaPath(item);
                    InwNodeFragsColl collection_frag = oaPath.Fragments();

                    int fragments_com = collection_frag.Count;
                    int fragments_net = geometry.FragmentCount;

                    for (int index_com = 1; index_com <= fragments_com;)
                    {
                        Element element = new Element(item);
                        element.Measurement = measurement;

                        element.Geometry = CreateGeometry(geometry);

                        for (int index_net = 1; index_net <= fragments_net; index_net++)
                        {
                            InwOaFragment3 inwFragment = collection_frag[index_com];
                            SimplePrimitivesCB.Vertices = new List<Point>();
                            SimplePrimitivesCB.Lines = new List<Line>();
                            inwFragment.GenerateSimplePrimitives(nwEVertexProperty.eNORMAL, SimplePrimitivesCB);

                            if (SimplePrimitivesCB.Vertices.Count > 0)
                            {
                                Fragment fragment = CreateFragment(inwFragment, SimplePrimitivesCB);
                                element.Geometry.Fragments.Add(fragment);
                                element.VetexCount += fragment.Triangles.Item2.Length;
                            }

                            index_com++;
                        }

                        if (element.Geometry.Fragments.Count > 0)
                        {
                            element.ModelItem = item;
                            elements.Add(element);

                            Console.WriteLine($"{counter--} {element.VetexCount}");
                        }
                        else
                        {
                            Console.WriteLine($"{counter--} {element.VetexCount} Null count");
                        }
                    }
                }
            }

            return elements;
        }

        private List<Element> GeometryHandler(ModelItemCollection collection)
        {
            List<Element> elements = new List<Element>();

            foreach (var item in collection)
            {
                ModelGeometry geometry = item.Geometry;

                SimplePrimitivesImplementation SimplePrimitivesCB = new SimplePrimitivesImplementation();
                InwOaPath oaPath = ComApiBridge.ToInwOaPath(item);
                InwNodeFragsColl collection_frag = oaPath.Fragments();

                Element element = new Element(item);
                element.Measurement = measurement;

                element.Geometry = CreateGeometry(geometry);

                int fragments_com = collection_frag.Count;
                int fragments_net = geometry.FragmentCount;

                for (int index_com = 1; index_com <= fragments_com; index_com++)
                {
                    InwOaFragment3 inwFragment = collection_frag[index_com];

                    //if(instance_set.Contains(ComApiBridge.ToModelItem(inwFragment.path).Geometry.GetHashCode()))
                    //    continue;

                    if (geometry.GetHashCode() != ComApiBridge.ToModelItem(inwFragment.path).Geometry.GetHashCode())
                        continue;

                    //instance_set.Add(ComApiBridge.ToModelItem(inwFragment.path).Geometry.GetHashCode());

                    SimplePrimitivesCB.Vertices = new List<Point>();
                    SimplePrimitivesCB.Lines = new List<Line>(); ;
                    inwFragment.GenerateSimplePrimitives(nwEVertexProperty.eNORMAL, SimplePrimitivesCB);

                    if (SimplePrimitivesCB.Vertices.Count > 0)
                    {
                        Fragment fragment = CreateFragment(inwFragment, SimplePrimitivesCB);
                        element.Geometry.Fragments.Add(fragment);
                        element.VetexCount += fragment.Triangles.Item2.Length;
                    }
                }

                if (element.Geometry.Fragments.Count > 0)
                {
                    element.ModelItem = item;
                    elements.Add(element);

                    Console.WriteLine($"{counter--} {element.VetexCount}");
                }
                else
                {
                    Console.WriteLine($"{counter--} {element.VetexCount} Null count");
                }
            }

            return elements;
        }

        private Geometry CreateGeometry(ModelGeometry modelGeometry)
        {
            Geometry geometry = new Geometry();
            geometry.Color = present_color(modelGeometry.ActiveColor);
            geometry.BoundingBox = present_box(modelGeometry.BoundingBox);
            geometry.Transparency = modelGeometry.ActiveTransparency;
            geometry.Fragments = new List<Fragment>();

            return geometry;
        }

        private Fragment CreateFragment(InwOaFragment3 inwFragment, SimplePrimitivesImplementation SimplePrimitivesCB)
        {
            Fragment fragment = new Fragment();
            fragment.Matrix = present_matrix((Array)(object)inwFragment.GetLocalToWorldMatrix().Matrix);
            fragment.Lines = SimplePrimitivesCB.Lines.ToArray();
            if (Configuration.Instance.Optimization == 0)
                fragment.Triangles = NoOptimize(SimplePrimitivesCB.Vertices);
            else
                fragment.Triangles = Optimize(SimplePrimitivesCB.Vertices.ToArray());

            return fragment;
        }

        private Tuple<Point[], uint[]> NoOptimize(List<Point> points)
        {
            if (points == null || points.Count == 0)
            {
                return Tuple.Create(new Point[0], new uint[0]);
            }

            // Создаем словарь для хранения уникальных точек и их индексов
            Dictionary<Point, uint> pointToIndex = new Dictionary<Point, uint>();
            List<Point> uniquePoints = new List<Point>();
            List<uint> indices = new List<uint>();

            uint currentIndex = 0;

            // Проходим по всем точкам и создаем уникальный список
            foreach (Point point in points)
            {
                if (!pointToIndex.ContainsKey(point))
                {
                    pointToIndex[point] = currentIndex;
                    uniquePoints.Add(point);
                    currentIndex++;
                }

                // Добавляем индекс текущей точки в массив индексов
                indices.Add(pointToIndex[point]);
            }

            return Tuple.Create(uniquePoints.ToArray(), indices.ToArray());
        }
        private Tuple<Point[], uint[]> Optimize(Point[] points)
        {
            return  MeshOptimizer.Optimize(points, null, Point.SizeInBytes, 1.5f);
        }

        private ModelItemCollection SearchByCategory(ModelItemCollection collection, string internal_name = "", string user_name = "")
        {
            InwOpState10 opState = ComApiBridge.State;

            InwOpSelectionSet2 opSelSet2 = (InwOpSelectionSet2)(object)opState.ObjectFactory(nwEObjectType.eObjectType_nwOpSelectionSet);
            opSelSet2.name = "ByCategorySelection";

            InwOpFindSpec opFindSpec = (InwOpFindSpec)(object)opState.ObjectFactory(nwEObjectType.eObjectType_nwOpFindSpec);
            InwOpFindCondition opFindCondition = (InwOpFindCondition)(object)opState.ObjectFactory(nwEObjectType.eObjectType_nwOpFindCondition);

            opFindCondition.SetAttributeNames(internal_name, user_name);
            opFindCondition.Condition = nwEFindCondition.eFind_HAS_ATTRIB;

            opFindSpec.selection = ComApiBridge.ToInwOpSelection(collection);

            opFindSpec.SearchMode = nwESearchMode.eSearchMode_ALL_PATHS;
            opFindSpec.ResultDisjoint = false;
            opFindSpec.Conditions().Add(opFindCondition);

            opSelSet2.ImplicitFindSpec = opFindSpec;

            return ComApiBridge.ToModelItemCollection(opSelSet2.selection);
        }
    }
}
