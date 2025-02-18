using System.Collections;
using System.Numerics;
using System.Threading.Tasks.Dataflow;
using Raylib_cs;

namespace SimpleFEM;

public static class DebugHelpers
{
    public static void PrintList(List<int> list)
    {
        string s = "[";
        foreach (int i in list)
        {
            s += ($"{i.ToString()}, ");
        }

        s += "]";
        Console.WriteLine(s);
    }
}





// TODO merge sort list implementation


