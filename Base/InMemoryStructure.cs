using System;
using System.Collections.Generic;
using System.Numerics;
using SimpleFEM.Types.Settings;
using SimpleFEM.Types.StructureTypes;
using SimpleFEM.Interfaces;
using SimpleFEM.Extensions;
namespace SimpleFEM.Base;

public class InMemoryStructure : IStructure
{
    private RecyclingList<Node> _nodes;
    private Dictionary<int, Load> _loads;
    private Dictionary<int, BoundaryCondition> _boundaryConditions;
    private RecyclingList<Element> _elements;
    private RecyclingList<Material> _materials;
    private RecyclingList<Section> _sections;
    
    
    private readonly string _structureName;
    private readonly StructureSettings _settings;
    // TODO maybe replace null checks with setting to default from the structure creation screen
    public InMemoryStructure(string name, StructureSettings? settings) 
    {
        _settings = settings ?? StructureSettings.Default;
        _loads = new Dictionary<int, Load>();
        _boundaryConditions = new Dictionary<int, BoundaryCondition>();
        _structureName = name;
        _nodes = new RecyclingList<Node>();
        _elements = new RecyclingList<Element>();
        _materials = new RecyclingList<Material>();
        _sections = new RecyclingList<Section>();
        
        AddInitialElementProperties();
    }

    private void AddInitialElementProperties()
    {
        AddMaterial(Material.Steel235);   
        AddMaterial(Material.Steel275);   
        AddMaterial(Material.Steel355);   
        AddSection(Section.UB533x312x273);
        AddSection(Section.UC254x254x132);
        AddSection(Section.SHS100x100x5); 
    }
    public StructureSettings GetStructureSettings()
    {
        return _settings;
    }
    public string GetName() => _structureName;

    public bool ValidNodeID(int nodeID)
    {
        return _nodes.ValidIndex(nodeID);
    }

    public bool ValidElementID(int elementID)
    {
        return _elements.ValidIndex(elementID);
    }
    public bool AddNode(Vector2 pos)
    {
        foreach (Node n in _nodes)
        {
            if (n.Pos == pos)
            {
                return false;
            }
        }

        _nodes.Add(new Node(pos));
        _loads[_nodes.LastAddedIndex] = default;
        _boundaryConditions[_nodes.LastAddedIndex] = default;

        return true;
    }

    public bool AddElement(Element element)
    {
        if (element.Node1ID == element.Node2ID) return false;
        if (!_nodes.ValidIndex(element.Node1ID)) return false;
        if (!_nodes.ValidIndex(element.Node2ID)) return false;
        if (!_materials.ValidIndex(element.MaterialID)) return false;
        if (!_sections.ValidIndex(element.SectionID)) return false;
        
        
        bool duplicate = false;
        foreach (Element e in _elements)
        {
            if ((e.Node1ID == element.Node2ID && e.Node2ID == element.Node1ID) || (e.Node1ID == element.Node1ID && e.Node2ID == element.Node2ID))
            {
                duplicate = true;
                break;
            }
        }

        if (!duplicate)
        {
            _elements.Add(element);
            return true;
        }
        return false;
    }

    public bool AddElement(Element element, out int index)
    {
        if (_nodes.ValidIndex(element.Node1ID) && _nodes.ValidIndex(element.Node2ID) && element.Node1ID != element.Node2ID)
        {
            bool duplicate = false;
            foreach (Element e in _elements)
            {
                if ((e.Node1ID == element.Node2ID && e.Node2ID == element.Node1ID) || (e.Node1ID == element.Node1ID && e.Node2ID == element.Node2ID))
                {
                    duplicate = true;
                    break;
                }
            }
            if (!duplicate)
            {
                _elements.Add(element);
                index = _elements.LastAddedIndex;
                return true;
            }
        }

        index = -1;
        return false;
    }

    public bool AddNode(Vector2 pos, out int index)
    {
        foreach (int i in _nodes.GetIndexes())
        {
            if (_nodes[i].Pos == pos)
            {
                index = i;
                return false;
            }
        } 
        
        _nodes.Add(new Node(pos));
        index = _nodes.LastAddedIndex;
        _loads[index] = default;
        _boundaryConditions[index] = default;
        return true;
    }

    public void RemoveNode(int nodeID)
    {
        if (_nodes.ValidIndex(nodeID))
        {
            foreach (int i in _elements.GetIndexes())
            {
                if (_elements[i].Node1ID == nodeID || _elements[i].Node2ID == nodeID)
                {
                    _elements.RemoveAt(i);
                }
            }

            _loads[nodeID] = default;
            _boundaryConditions[nodeID] = default;
            _nodes.RemoveAt(nodeID);
        }
        else
        {
            throw new IndexOutOfRangeException("Invalid NodeID");
        }
        
    }
    
    public void RemoveElement(int elementID)
    {
        if (_elements.ValidIndex(elementID))
        {
            _elements.RemoveAt(elementID);
        }
        else
        {
            throw new IndexOutOfRangeException("Invalid ElementID");
        }
    }

    public Node GetNode(int nodeID)
    {
        if (_nodes.ValidIndex(nodeID))
        {
            return _nodes[nodeID];
        }
        else
        {
            throw new IndexOutOfRangeException("Invalid NodeID");
        }
    }

    public void AddMaterial(Material mat)
    {
        foreach (Material m in _materials)
        {
            if (m.E == mat.E && m.Yield == mat.Yield)
            {
                return;
            }
        }
        _materials.Add(mat);
    }

    public void AddSection(Section sect)
    {
        foreach (Section s in _sections)
        {
            if (s.I == sect.I && s.A == sect.A)
            {
                return;
            }
        }
        _sections.Add(sect);
    }
    public Element GetElement(int elementID)
    {
        if (_elements.ValidIndex(elementID))
        {
            return _elements[elementID];
        }
        else
        {
            throw new IndexOutOfRangeException("Invalid NodeID");
        }
    }

    public List<int> GetSectionIndexesSorted()
    {
        return _sections.GetIndexes();
    }

    public List<int> GetMaterialIndexesSorted()
    {
        return _materials.GetIndexes();
    }
    public Section GetSection(int sectionID)
    {
        if (_sections.ValidIndex(sectionID))
        {
            return _sections[sectionID];
        }

        throw new IndexOutOfRangeException("Invalid SectionID");
    } 
    public Material GetMaterial(int materialID)
    {
        if (_materials.ValidIndex(materialID))
        {
            return _materials[materialID];
        }

        throw new IndexOutOfRangeException("Invalid MaterialID");
    }
    public void SetBoundaryCondition(int nodeID, BoundaryCondition boundaryCondition)
    {
        if (_nodes.ValidIndex(nodeID))
        {
            _boundaryConditions[nodeID] = boundaryCondition;;
        }
        else
        {
            throw new IndexOutOfRangeException("Invalid NodeID");
        }
    }

    public BoundaryCondition GetBoundaryCondition(int nodeID)
    {
        if (_nodes.ValidIndex(nodeID))
        {
            return _boundaryConditions[nodeID];
        }
        else
        {
            throw new IndexOutOfRangeException("Invalid NodeID");
        }

    }

    public void SetLoad(int nodeID, Load load)
    {
        if (_nodes.ValidIndex(nodeID))
        {
            _loads[nodeID] = load;
        }
        else
        {
            throw new IndexOutOfRangeException("Invalid NodeID");
        }
    }

    public Load GetLoad(int nodeID)
    {
        if (_nodes.ValidIndex(nodeID))
        {
            return _loads[nodeID];
        }
        else
        {
            throw new IndexOutOfRangeException("Invalid NodeID");
        }
    }

    public List<int> GetElementIndexesSorted()
    {
        return _elements.GetIndexes();
    }

    public List<int> GetNodeIndexesSorted()
    {
        return _nodes.GetIndexes();
    }

    public int GetNodeCount()
    {
        return _nodes.Count;
    }

    public int GetElementCount()
    {
        return _elements.Count;
    }

    public int GetBoundaryConditionCount()
    {
        int count = 0;
        foreach (BoundaryCondition bc in _boundaryConditions.Values)
        {
            if (bc.FixedX){count++;}
            if (bc.FixedY){count++;}
            if (bc.FixedRotation){count++;}
        }

        return count;
    }

    public int GetLoadCount()
    {
        int count = 0;
        foreach (Load l in _loads.Values)
        {
            if (l.ForceX != 0) {count++;}
            if (l.ForceY != 0) {count++;}
            if (l.Moment != 0) {count++;}
        }

        return count;
    }
}