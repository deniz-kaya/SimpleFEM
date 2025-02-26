using System.Numerics;
using Raylib_cs;
using SimpleFEM.Extensions;
using SimpleFEM.Interfaces;
using SimpleFEM.Types.LinAlg;
using SimpleFEM.Types.StructureTypes;

namespace SimpleFEM.Base;

public class StructureSolver
{
    private Matrix globalStiffnessMatrix;
    private IStructure structure;
    private const int DOF = 3;
    public StructureSolver(IStructure structure)
    {
        this.structure = structure;
    }

    public void Solve()
    {
        
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
    public float GetElementAngle(Element e)
    {
        Vector2 node1Pos = structure.GetNode(e.Node1ID).Pos;
        Vector2 node2Pos = structure.GetNode(e.Node2ID).Pos;
        
        float angle = MathF.Atan((node1Pos.Y- node2Pos.Y)/(node1Pos.X - node2Pos.X));
        return angle < 0 ? angle + MathF.PI : angle;
    }
    public Matrix GetGlobalStiffnessMatrix()
    {
        
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
            
            int firstIdx = elementNodeIndexes[i].Item1 * DOF;
            int secondIdx = elementNodeIndexes[i].Item2 * DOF;
            
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
        return K;
        
    }
    public Matrix6x6 GetLocalStiffnessMatrix(Element element)
    {
        float length = Vector2.Distance(structure.GetNode(element.Node1ID).Pos, structure.GetNode(element.Node2ID).Pos);
        Console.WriteLine(length);
        float axS = (element.Material.E * element.Section.A) / length; //axial stiffness
        Console.WriteLine(axS);
        float beS = (element.Material.E * element.Section.I) / length; // bending stiffness
        Console.WriteLine(beS);
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

}