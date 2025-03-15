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
    public InMemoryStructure(string name, StructureSettings? settings) 
    {
        //if given structure settings is null, set to default otherwise set to the setting
        _settings = settings ?? StructureSettings.Default;
        //initialise all variables to their default
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
        //add initial materials and sections
        AddMaterial(Material.Steel);   
        AddMaterial(Material.Aluminium);   
        AddMaterial(Material.Concrete);   
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
        //check that there isnt a node with the same position in the structure
        foreach (Node n in _nodes)
        {
            if (n.Pos == pos)
            {
                return false;
            }
        }

        //the node must be valid, therefore add it to the list
        _nodes.Add(new Node(pos));
        //initialise load and boundary condition of node to be default
        _loads[_nodes.LastAddedIndex] = default;
        _boundaryConditions[_nodes.LastAddedIndex] = default;

        return true;
    }

    public bool AddElement(Element element)
    {
        //discard the index
        return AddElement(element, out _);
    }

    public bool AddElement(Element element, out int index)
    {
        //initialise index as an invalid value for early returns
        index = -1;
        //if the node1, node2, material or section indexes of the element are invalid, do not add element
        if (element.Node1ID == element.Node2ID) return false;
        if (!_nodes.ValidIndex(element.Node1ID)) return false;
        if (!_nodes.ValidIndex(element.Node2ID)) return false;
        if (!_materials.ValidIndex(element.MaterialID)) return false;
        if (!_sections.ValidIndex(element.SectionID)) return false;
        
        //check each element in the structure to see if the element being added is a duplicate of it in terms of position
        bool duplicate = false;
        foreach (Element e in _elements)
        {
            if ((e.Node1ID == element.Node2ID && e.Node2ID == element.Node1ID) || (e.Node1ID == element.Node1ID && e.Node2ID == element.Node2ID))
            {
                return false;
            }
        }

        //passed all checks, therefore add element
        _elements.Add(element);
        index = _elements.LastAddedIndex;
        return true;
    }

    public bool AddNode(Vector2 pos, out int index)
    {
        //check each node in the structure to see if it has the same position as the one being added
        foreach (int i in _nodes.GetIndexes())
        {
            if (_nodes[i].Pos == pos)
            {
                //set the index to the node that is already present
                index = i;
                return false;
            }
        } 
        
        //node must not be present, therfore add it to the list and update index
        _nodes.Add(new Node(pos));
        index = _nodes.LastAddedIndex;
        //initialise default load and boundary conditions
        _loads[index] = default;
        _boundaryConditions[index] = default;
        return true;
    }

    public void RemoveNode(int nodeID)
    {
        if (_nodes.ValidIndex(nodeID))
        {
            //remove all elements which are connected to the node
            foreach (int i in _elements.GetIndexes())
            {
                if (_elements[i].Node1ID == nodeID || _elements[i].Node2ID == nodeID)
                {
                    _elements.RemoveAt(i);
                }
            }
            //set load and boundary conditions of the node to default, and remove the node
            _loads[nodeID] = default;
            _boundaryConditions[nodeID] = default;
            _nodes.RemoveAt(nodeID);
        }
    }
    
    public void RemoveElement(int elementID)
    {
        if (_elements.ValidIndex(elementID))
        {
            _elements.RemoveAt(elementID);
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
        //fail silently if the material E is not valid
        if (mat.E <= 0)
        {
            return;
        }
        //check that a material with the same E does not exist
        foreach (Material m in _materials)
        {
            if (m.E == mat.E)
            {
                return;
            }
        }
        //add if it is a valid new material
        _materials.Add(mat);
    }

    public void AddSection(Section sect)
    {
        //fail silently if section I and A are out of bounds
        if (sect.I <= 0 || sect.A <= 0)
        {
            return;
        }
        //check each section to see if the one being added is a duplicate
        foreach (Section s in _sections)
        {
            if (s.I == sect.I && s.A == sect.A)
            {
                return;
            }
        }
        //add section if it is a valid new section
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
        //fail silently otherwise
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
        //fail silently otherwise
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
        //get the count of constrained degrees of freedom in the whole structure
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
        //get the total number of degrees of freedom with loads in the whole structure
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