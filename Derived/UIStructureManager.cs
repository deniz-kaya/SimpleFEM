using System.Numerics;
using Raylib_cs;
using SimpleFEM.Base;
using SimpleFEM.Interfaces;
using SimpleFEM.SceneObjects;
using SimpleFEM.Types.StructureTypes;

namespace SimpleFEM.Derived;

// TODO not necessarily here but overall, manage protected, private and public fields to make sense

public class UIStructureManager : StructureManager, IUIStructureHelper
{
    public UIStructureManager(IStructure structure) : base(structure)
    {
                
    }

    public UIStructureManager(IStructure structure, Settings settings) : base(structure, settings)
    {
        
    }

    public Queue<ISceneObject> GetSceneObjects(DrawSettings drawSettings)
    {
        Queue<ISceneObject> renderQueue = new Queue<ISceneObject>();
        //background
        renderQueue.Enqueue(new BackgroundObject(Color.White));
        
        //grid
        // TODO variables
        renderQueue.Enqueue(new GridObject(200, 50f));
        
        //Elements
        QueueElementSceneObjects(ref renderQueue, drawSettings);
        
        //Nodes
        QueueNodeSceneObjects(ref renderQueue, drawSettings);
        
        //Selection box
        renderQueue.Enqueue(new SelectionBoxObject(SelectionPos1, SelectionPos2, drawSettings.selectionBoxColor));
        
        return renderQueue;
    }

    private void QueueElementSceneObjects(ref Queue<ISceneObject> renderQueue, DrawSettings drawSettings)
    {
        LinesObject selectedElementsObject = new LinesObject(drawSettings.selectedElementColor, drawSettings.elementThickness);
        LinesObject elementsObject = new LinesObject(drawSettings.elementColor, drawSettings.elementThickness);

        int selectedElementListIndexTracker = 0;
        foreach (int i in Structure.GetElementIndexesSorted())
        {
            Element e = Structure.GetElement(i);
            Vector2 position1 = Structure.GetNode(e.Node1ID).Pos;
            Vector2 position2 = Structure.GetNode(e.Node2ID).Pos;
            
            if (i == SelectedElements.ElementAt(selectedElementListIndexTracker))
            {
                selectedElementListIndexTracker++;
                selectedElementsObject.AddLine(position1, position2);
            }
            else
            {
                elementsObject.AddLine(position1, position2);
            }
        }
        
        renderQueue.Enqueue(selectedElementsObject);
        renderQueue.Enqueue(elementsObject);
    }

    private void QueueNodeSceneObjects(ref Queue<ISceneObject> renderQueue, DrawSettings drawSettings)
    {
        SpheresObject selectedNodesObject = new SpheresObject(drawSettings.selectedNodeColor, drawSettings.nodeRadius);
        SpheresObject nodesObject= new SpheresObject(drawSettings.nodeColor, drawSettings.nodeRadius);

        int selectedNodesListIndexTracker = 0;
        foreach (int i in Structure.GetNodeIndexesSorted())
        {
            Vector2 position = Structure.GetNode(i).Pos;
            if (i == selectedNodesListIndexTracker)
            {
                selectedNodesListIndexTracker++;
                selectedNodesObject.AddSphere(position);
            }
            else
            {
                nodesObject.AddSphere(position);
            }
        }
        renderQueue.Enqueue(selectedNodesObject);
        renderQueue.Enqueue(nodesObject);
    }
}