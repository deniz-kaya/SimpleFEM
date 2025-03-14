using System;
using System.Collections.Generic;
using SimpleFEM.Extensions;
using Vector2 = System.Numerics.Vector2;
using SimpleFEM.Interfaces;
using SimpleFEM.LinearAlgebra;
using SimpleFEM.Types;
using SimpleFEM.Types.StructureTypes;

namespace SimpleFEM.Base;

public class StructureSolver
{
    //how many degrees of freedom there are per node
    protected const int DOF = 3;
    
    protected IStructure Structure;
    private Matrix _currentStiffnessMatrix;
    private Graph _currentStructureGraph;
    private Vector _currentForceVector;
    protected Vector CurrentSolution;
    protected Exception LastError;
    protected bool ErrorDuringSolution;
    
    public bool StructureHasBeenChanged => Structure.GetNodeCount() != _currentStructureGraph.NodeCount; 
    public StructureSolver(IStructure structure)
    {
        //construct initial properties using the starting structure
        Structure = structure;
        _currentStructureGraph = ConstructStructureGraph();
        _currentStiffnessMatrix = GetGlobalStiffnessMatrix();
        _currentForceVector = GetForceVector();
    }

    //returns true if the structure was successfully solved
    protected bool Solve()
    {
        try
        {
            //stability checkss
            if (Structure.GetElementCount() == 0)   
            {
                throw new EmptyStructure();
            }
            if (Structure.GetLoadCount() == 0)
            {
                throw new NoLoads();
            }
            if (!ValidBoundaryConditions())
            {
                throw new InvalidBoundaryConditions();
            }
            _currentStructureGraph = ConstructStructureGraph();
            if (!_currentStructureGraph.IsConnected())
            {
                throw new StructureDisconnected();
            }

            //constructing the force vector and stiffness matrix
            if (StructureHasBeenChanged)
            {
                _currentStiffnessMatrix = GetGlobalStiffnessMatrix();
            }
            _currentForceVector = GetForceVector();
            
            //attempt to solve system
            CurrentSolution = LinAlgMethods.Solve(_currentStiffnessMatrix, _currentForceVector);
            //if there is no errors by now, we want to set error during solution to false.
            ErrorDuringSolution = false;
        }
        //catch any potential errors thrown by the program during solution
        catch (Exception e)
        {
            //store the error and set the error during solution flag to true
            LastError = e;
            ErrorDuringSolution = true;
            return false;
        }
        
        return true;
    }
    public Graph ConstructStructureGraph()
    {
        Graph graph = new Graph();
        //add all nodes in the structure to the graph
        foreach (int i in Structure.GetNodeIndexesSorted())
        {
            graph.AddVertex(i);
        }
        //add all elements in the graph as edges between respective nodes into the graph
        foreach (int i in Structure.GetElementIndexesSorted())
        {
            Element e = Structure.GetElement(i);
            graph.AddEdge(e.Node1ID, e.Node2ID);
        }

        return graph;
    }
    private Matrix TransformLocalMatrixIntoGlobal(Matrix matrix, Element element)
    {
        //get angle and pre-calculate cos and sin values for the angle
        float angle = GetElementAngle(element);
        float cosAngle = MathF.Cos(angle);
        float sinAngle = MathF.Sin(angle);
        
        Matrix transformation = new Matrix(DOF * 2,DOF * 2);

        //build the transformation matrix described in the documented design section
        for (int i = 0; i < 4; i+= 3)
        {
            transformation[i, i] = cosAngle;
            transformation[i + 1, i + 1] = cosAngle;
            transformation[i + 2 ,i + 2] = 1f;
            transformation[i, i + 1] = sinAngle;
            transformation[i + 1, i] = -sinAngle;
        }
        
        //matrix is struct type, therefore transformation isnt passed by referencem instead a copy is saved as transpose
        Matrix transpose = transformation;
        
        //hardcoded exclusive transpose operation as it is quite simple
        for (int i = 0; i < 4; i += 3)
        {
            transpose[i, i + 1] *= -1;
            transpose[i + 1, i] *= -1;
        }
        //return the transformed stiffness matrix
        return (transpose * matrix) * transformation;
    }

    private Vector GetForceVector()
    {
        //map uncertainly increasing ID's to ID's which increase by 1 each time 
        Dictionary<int, int> idMap = GetMappedNodeIndexes();
        Vector forceVector = new Vector(idMap.Count * DOF);
        //for each node ID, 
        foreach (int i in idMap.Keys)
        {
            Load l = Structure.GetLoad(i);
            //find the normalized/'increasing by 1' ID of the node, to add associated load to correct position in force vector
            int currentNodeNormalizedID = idMap[i];
            forceVector[currentNodeNormalizedID * DOF] = l.ForceX;
            forceVector[currentNodeNormalizedID * DOF + 1] = l.ForceY;
            forceVector[currentNodeNormalizedID * DOF + 2] = l.Moment;
        }

        return forceVector;
    }

    private Dictionary<int, int> GetMappedNodeIndexes()
    {
        //map uncertainly increasing node ID's to ID's which increase by 1 each time
        List<int> nodeIndexes = Structure.GetNodeIndexesSorted();
        Dictionary<int, int> nodeIDToIndex = new Dictionary<int, int>();
        for (int i = 0; i < nodeIndexes.Count; i++)
        {
            nodeIDToIndex.Add(nodeIndexes[i], i);
        }

        return nodeIDToIndex;
    }

    private Matrix GetGlobalStiffnessMatrix()
    {
        Dictionary<int, int> nodeIDMap = GetMappedNodeIndexes();
        List<int> elementIndexes = Structure.GetElementIndexesSorted();
        Matrix[] elementStiffnessMatrices = new Matrix[elementIndexes.Count];
        (int, int)[] elementNodeIndexes = new (int, int)[elementStiffnessMatrices.Length];

        //get transformed element stiffness matrices for every element in the structure alongside the nodes that it links together
        for (int i = 0; i < elementStiffnessMatrices.Length; i++)
        {
            int index = elementIndexes[i];
            Element e = Structure.GetElement(index);
            elementNodeIndexes[i] = (e.Node1ID, e.Node2ID);
            elementStiffnessMatrices[i] = TransformLocalMatrixIntoGlobal(GetElementStiffnessMatrix(e), e);
        }
        
        int nodeCount = Structure.GetNodeCount();
        
        Matrix k = new Matrix(nodeCount * DOF, nodeCount * DOF);

        //for each element in the structure, add its transformed stiffness matrix to the global stiffness matrix
        for (int i = 0; i < elementStiffnessMatrices.Length; i++)
        {
            Matrix m = elementStiffnessMatrices[i];
            
            //the row/column index of the nodes of the element in the global stiffness matrix
            int firstIdx = nodeIDMap[elementNodeIndexes[i].Item1] * DOF;
            int secondIdx = nodeIDMap[elementNodeIndexes[i].Item2] * DOF;
            
            //hardcoded 
            for (int corner = 0; corner < 4; corner++)
            {
                // corner
                // 0  1
                // 2  3
                
                //corner column values
                // f  t
                // f  t
                
                //corner row values
                // f  f
                // t  t
                
                //values which give us information as to which corner we are at, described better above
                int elementMatrixCornerRow = corner / 2 == 1 ? DOF : 0;
                int elementMatrixCornerCol = corner % 2 == 1 ? DOF : 0;
                    
                int rowStart = corner / 2 == 1 ? secondIdx : firstIdx;
                int colStart = corner % 2 == 1 ? secondIdx : firstIdx;

                //add the element matrix corner to the global stiffness matrix in the correct position
                for (int row = 0; row < DOF; row++)
                {
                    for (int col = 0; col < DOF; col++)
                    {
                        k[row + rowStart, col + colStart] += m[row + elementMatrixCornerRow, col + elementMatrixCornerCol];
                    }
                }
            }
        }

        //apply BC's 
        foreach (int nodeID in Structure.GetNodeIndexesSorted())
        {
            //get the boundary condition and check each constraint to see if it exists,
            //if it does exist, zero out the respective row and column
            BoundaryCondition bc = Structure.GetBoundaryCondition(nodeID);
            if (bc.FixedX)
            {
                int bcIndex = DOF * nodeIDMap[nodeID];
                for (int i = 0; i < k.Columns; i++)
                {
                    k[bcIndex, i] = 0f;
                    k[i, bcIndex] = 0f;
                }
                k[bcIndex, bcIndex] = 1f;
            }
            if (bc.FixedY)
            {
                int bcIndex = DOF * nodeIDMap[nodeID] + 1;
                for (int i = 0; i < k.Columns; i++)
                {
                    k[bcIndex, i] = 0f;
                    k[i, bcIndex] = 0f;
                }
                k[bcIndex, bcIndex] = 1f;
            }
            if (bc.FixedRotation)
            {
                int bcIndex = DOF * nodeIDMap[nodeID] + 2;
                for (int i = 0; i < k.Columns; i++)
                {
                    k[bcIndex, i] = 0f;
                    k[i, bcIndex] = 0f;
                }
                k[bcIndex, bcIndex] = 1f;
            }
            
        }
        return k;
        
    }


    private Matrix GetElementStiffnessMatrix(Element element)
    {
        Material mat = Structure.GetMaterial(element.MaterialID);
        Section sect = Structure.GetSection(element.SectionID);
        //get values which are necessary to calculate the stiffness matrix
        float length = Vector2.Distance(Structure.GetNode(element.Node1ID).Pos, Structure.GetNode(element.Node2ID).Pos);
        float axS = (mat.E * sect.A) / length; //axial stiffness
        float beS = (mat.E * sect.I) / length; // bending stiffness
        float beSSq = 6f * (beS / length);  // bending stiffness squared
        float beSCb = 2f * (beSSq / length);  // bending stiffness cubed

        Matrix m = new Matrix(DOF * 2,DOF * 2);
        
        //loop through each corner adding the necessary values to the stiffness matrix
        for (int corner = 0; corner < 4; corner++)
        {
            int cDiv = corner / 2;
            int cMod = corner % 2;
            int row = DOF * cDiv;
            int col = DOF * cMod;
            
            int signDiagonals = (cDiv ^ cMod) == 1 ? -1 : 1 ; // if we are on top left corner or bottom right corner, its 1 otherwise -1
            int beSCoefficient = signDiagonals == 1 ? 4 : 2 ; // if we are on top left corner or bottom right corner, its 4 otherwise 2
            int signHorizontal = cDiv == 1 ? -1 : 1; // if it's a bottom corner, -1, otherwise 1
            int signVertical = cMod == 1 ? -1 : 1; // if it's a right corner, -1 otherwise 1
            
            //add values to the corner
            m[row, col] = axS * signDiagonals;
            m[row + 1, col + 1] = beSCb * signDiagonals;
            m[row + 1, col + 2] = beSSq * signHorizontal;
            m[row + 2, col + 1] = beSSq * signVertical;
            m[row + 2, col + 2] = beS * beSCoefficient;
        }

        return m;

    }
    private float GetElementAngle(Element e)
    {
        Vector2 node1Pos = Structure.GetNode(e.Node1ID).Pos;
        Vector2 node2Pos = Structure.GetNode(e.Node2ID).Pos;
        
        //use change in y and change in x between the two connecting nodes of the element to get angle relative to horizontal
        return MathF.Atan((node1Pos.Y- node2Pos.Y)/(node1Pos.X - node2Pos.X));
    }

    private bool ValidBoundaryConditions()
    {
        //stability check
        int xCount = 0;
        int yCount = 0;
        int rotCount = 0;
        foreach (int i in Structure.GetNodeIndexesSorted())
        {
            BoundaryCondition bc = Structure.GetBoundaryCondition(i);
            //increment each respective constraint count if the current boundary condition has the constraints
            if (bc.FixedX)
            {
                xCount++;
            }

            if (bc.FixedY)
            {
                yCount++;
            }

            if (bc.FixedRotation)
            {
                rotCount++;
            }

            //exit early if all constraints are constrained at least once
            if (xCount > 0 && yCount > 0 && rotCount > 0)
            {
                return true;
            }
        }
        //code did not exit early, therefore at least one of the constraints counts must be zero
        return false;

    }
}