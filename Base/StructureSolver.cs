using ImGuiNET;
using SimpleFEM.Extensions;
using Vector2 = System.Numerics.Vector2;
using SimpleFEM.Interfaces;
using SimpleFEM.LinearAlgebra;
using SimpleFEM.Types;
using SimpleFEM.Types.StructureTypes;

namespace SimpleFEM.Base;

public class StructureSolver
{
    protected const int DOF = 3;
    
    protected IStructure structure;
    protected Matrix CurrentStiffnessMatrix;
    protected Graph CurrentStructureGraph;
    protected Vector CurrentForceVector;
    protected Vector CurrentSolution;
    protected Exception lastError;
    protected bool ErrorDuringSolution;
    
    public bool StructureHasBeenChanged => structure.GetNodeCount() != CurrentStructureGraph.NodeCount; 
    public StructureSolver(IStructure structure)
    {
        this.structure = structure;
        CurrentStructureGraph = ConstructStructureGraph();
        CurrentStiffnessMatrix = GetGlobalStiffnessMatrix();
        CurrentForceVector = GetForceVector();
    }

    
    public bool Solve()
    {
        
        //todo switch to using own exceptions?
        try
        {
            //stability checks
            //todo something funky with constrained nodes and forces
            if (structure.GetElementCount() == 0)
            {
                throw new EmptyStructure();
            }
            if (structure.GetLoadCount() == 0)
            {
                throw new NoLoads();
            }

            if (structure.GetBoundaryConditionCount() == 0)
            {
                throw new NoBoundaryConditions();
            }
            CurrentStructureGraph = ConstructStructureGraph();
            if (!CurrentStructureGraph.IsConnected())
            {
                throw new StructureDisconnected();
            }

            //construct shit
            CurrentStiffnessMatrix = GetGlobalStiffnessMatrix();
            CurrentForceVector = GetForceVector();

            CurrentSolution = LinAlgMethods.Solve(CurrentStiffnessMatrix, CurrentForceVector);
        }
        catch (Exception e)
        {
            lastError = e;
            ErrorDuringSolution = true;
            return false;
        }

        return true;
    }
    public Graph ConstructStructureGraph()
    {
        Graph graph = new Graph();
        foreach (int i in structure.GetNodeIndexesSorted())
        {
            graph.AddVertex(i);
        }
        foreach (int i in structure.GetElementIndexesSorted())
        {
            Element e = structure.GetElement(i);
            graph.AddEdge(e.Node1ID, e.Node2ID);
        }

        return graph;
    }
    public Matrix6x6 TransformLocalMatrixIntoGlobal(Matrix6x6 matrix, Element element)
    {
        float angle = GetElementAngle(element);
        float cosAngle = MathF.Cos(angle);
        float sinAngle = MathF.Sin(angle);
        
        Matrix6x6 transformation = new Matrix6x6();

        for (int i = 0; i < 4; i+= 3)
        {
            transformation[i, i] = cosAngle;
            transformation[i + 1, i + 1] = cosAngle;
            transformation[i + 2 ,i + 2] = 1f;
            transformation[i, i + 1] = sinAngle;
            transformation[i + 1, i] = -sinAngle;
        }

        Matrix6x6 transpose = transformation;

        for (int i = 0; i < 4; i += 3)
        {
            transpose[i, i + 1] *= -1;
            transpose[i + 1, i] *= -1;
        }
        
        return (transpose * matrix) * transformation;
    }
    public Vector GetForceVector()
    {
        Dictionary<int, int> idMap = GetMappedNodeIndexes();
        Vector forceVector = new Vector(idMap.Count * DOF);
        foreach (int i in idMap.Keys)
        {
            Load l = structure.GetLoad(i);
            int currentNode = idMap[i];
            forceVector[currentNode * DOF] = l.ForceX;
            forceVector[currentNode * DOF + 1] = l.ForceY;
            forceVector[currentNode * DOF + 2] = l.Moment;
        }

        return forceVector;
    }
    public Dictionary<int, int> GetMappedNodeIndexes()
    {
        List<int> nodeIndexes = structure.GetNodeIndexesSorted();
        Dictionary<int, int> nodeIDToIndex = new Dictionary<int, int>();
        for (int i = 0; i < nodeIndexes.Count; i++)
        {
            nodeIDToIndex.Add(nodeIndexes[i], i);
        }

        return nodeIDToIndex;
    }
    public Matrix GetGlobalStiffnessMatrix()
    {
        Dictionary<int, int> nodeIDMap = GetMappedNodeIndexes();
        List<int> elementIndexes = structure.GetElementIndexesSorted();
        Matrix6x6[] elementStiffnessMatrices = new Matrix6x6[elementIndexes.Count];
        (int, int)[] elementNodeIndexes = new (int, int)[elementStiffnessMatrices.Length];

        for (int i = 0; i < elementStiffnessMatrices.Length; i++)
        {
            int index = elementIndexes[i];
            Element e = structure.GetElement(index);
            elementNodeIndexes[i] = (e.Node1ID, e.Node2ID);
            elementStiffnessMatrices[i] = TransformLocalMatrixIntoGlobal(GetLocalStiffnessMatrix(e), e);
        }
        
        int nodeCount = structure.GetNodeCount();
        
        Matrix K = new Matrix(nodeCount * DOF, nodeCount * DOF);

        for (int i = 0; i < elementStiffnessMatrices.Length; i++)
        {
            Matrix6x6 m = elementStiffnessMatrices[i];
            
            int firstIdx = nodeIDMap[elementNodeIndexes[i].Item1] * DOF;
            int secondIdx = nodeIDMap[elementNodeIndexes[i].Item2] * DOF;
            
            for (int corner = 0; corner < 4; corner++)
            {
                // 0  1
                // 2  3
                
                //modulo
                // f  t
                // f  t
                
                //div
                // f  f
                // t  t
                
                int elementMatrixCornerRow = corner / 2 == 1 ? DOF : 0;
                int elementMatrixCornerCol = corner % 2 == 1 ? DOF : 0;
                    
                int rowStart = corner / 2 == 1 ? secondIdx : firstIdx;
                int colStart = corner % 2 == 1 ? secondIdx : firstIdx;

                for (int row = 0; row < DOF; row++)
                {
                    for (int col = 0; col < DOF; col++)
                    {
                        K[row + rowStart, col + colStart] += m[row + elementMatrixCornerRow, col + elementMatrixCornerCol];
                    }
                }
            }
        }

        //apply BC's 
        foreach (int nodeID in structure.GetNodeIndexesSorted())
        {
            BoundaryCondition bc = structure.GetBoundaryCondition(nodeID);
            if (bc.FixedX)
            {
                int BCindex = DOF * nodeIDMap[nodeID];
                for (int i = 0; i < K.Columns; i++)
                {
                    K[BCindex, i] = 0f;
                    K[i, BCindex] = 0f;
                }
                K[BCindex, BCindex] = 1f;
            }
            if (bc.FixedY)
            {
                int BCindex = DOF * nodeIDMap[nodeID] + 1;
                for (int i = 0; i < K.Columns; i++)
                {
                    K[BCindex, i] = 0f;
                    K[i, BCindex] = 0f;
                }
                K[BCindex, BCindex] = 1f;
            }
            if (bc.FixedRotation)
            {
                int BCindex = DOF * nodeIDMap[nodeID] + 2;
                for (int i = 0; i < K.Columns; i++)
                {
                    K[BCindex, i] = 0f;
                    K[i, BCindex] = 0f;
                }
                K[BCindex, BCindex] = 1f;
            }
            
        }
        return K;
        
    }


    public Matrix6x6 GetLocalStiffnessMatrix(Element element)
    {
        Material mat = structure.GetMaterial(element.MaterialID);
        Section sect = structure.GetSection(element.SectionID);
        float length = Vector2.Distance(structure.GetNode(element.Node1ID).Pos, structure.GetNode(element.Node2ID).Pos);
        float axS = (mat.E * sect.A) / length; //axial stiffness
        float beS = (mat.E * sect.I) / length; // bending stiffness
        float beSSq = 6f * (beS / length);  // bending stiffness squared
        float beSCb = 2f * (beSSq / length);  // bending stiffness cubed

        Matrix6x6 m = new();
        
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
            
            m[row, col] = axS * signDiagonals;
            m[row + 1, col + 1] = beSCb * signDiagonals;
            m[row + 1, col + 2] = beSSq * signHorizontal;
            m[row + 2, col + 1] = beSSq * signVertical;
            m[row + 2, col + 2] = beS * beSCoefficient;
        }

        return m;

    }
    public float GetElementAngle(Element e)
    {
        Vector2 node1Pos = structure.GetNode(e.Node1ID).Pos;
        Vector2 node2Pos = structure.GetNode(e.Node2ID).Pos;
        
        float angle = MathF.Atan((node1Pos.Y- node2Pos.Y)/(node1Pos.X - node2Pos.X));
        return angle < 0 ? angle + MathF.PI : angle;
    }
    
    //stability checking
    public bool CheckElementIntersections()
    {
        List<int> elementIndexes = structure.GetElementIndexesSorted();
        int currentIndex = 0;
        foreach (int idx in structure.GetElementIndexesSorted())
        {
            Element e = structure.GetElement(idx);
            Vector2 pos1 = structure.GetNode(e.Node1ID).Pos;
            Vector2 pos2 = structure.GetNode(e.Node2ID).Pos;
            currentIndex++;
            for (int i = currentIndex; i < elementIndexes.Count; i++)
            {
                Element testE = structure.GetElement(elementIndexes[i]);
                Vector2 testPos1 = structure.GetNode(testE.Node1ID).Pos;
                Vector2 testPos2 = structure.GetNode(testE.Node2ID).Pos;
                if (Vector2Extensions.CheckSegmentIntersections(testPos1, pos1, testPos2, pos2))
                {
                    return false;
                }
                
            }
        }

        return true;
    } 
    public bool CheckStructureConnected()
    {
        return CurrentStructureGraph.IsConnected();
    }
}