using System.Numerics;
using SimpleFEM.Extensions;
using SimpleFEM.Interfaces;
using SimpleFEM.Types;
using SimpleFEM.Types.StructureTypes;

namespace SimpleFEM.Base;

public class StructureSolver
{
    private Matrix globalStiffnessMatrix;
    private IStructure structure;
    
    public StructureSolver(IStructure structure)
    {
        this.structure = structure;
    }

    public void Solve()
    {
        List<int> elementIndexes = structure.GetElementIndexesSorted();
        Matrix6x6[] elementStiffnessMatrices = new Matrix6x6[elementIndexes.Count];
        
    }
    
    public Matrix6x6 GetLocalStiffnessMatrix(Element element)
    {
        float length = Vector2.Distance(structure.GetNode(element.Node1ID).Pos, structure.GetNode(element.Node2ID).Pos);
        
        float axS = (element.Material.E * element.Section.A) / length; //axial stiffness
        float beS = (element.Material.E * element.Section.I) / length; // bending stiffness
        float beSSq = 6f * (beS / length);  // bending stiffness squared 
        float beSCb = 2f * (beSSq / length);  // bending stiffness cubed

        Matrix6x6 m = new();
        
        for (int corner = 0; corner < 4; corner++)
        {
            int cDiv = corner / 2;
            int cMod = corner % 2;
            int row = 3 * cDiv;
            int col = 3 * cMod;
            
            int beSCoefficient = cDiv + cMod == 0 ? 1 : (cDiv + cMod) * 2; // 1 if top left corner, 2 if top right or bottom left, 4 if bottom right
            int signDiagonals = (cDiv ^ cMod) == 1 ? -1 : 1 ; // if we are on top left corner or bottom right corner, its 1 otherwise -1
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
    public void ConstructGlobalStiffnessMatrix()
    {
        
    }
}