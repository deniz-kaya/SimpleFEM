using System;

namespace SimpleFEM.Types;

public class StructureDisconnected() : Exception("The structure graph is not connected. Have you checked for floating nodes?");
public class NoLoads() : Exception("Structure has no loads, solving the system will have no impact.");
public class InvalidBoundaryConditions() : Exception("Structure is improperly constrained! Add at least one of each constraint.");
public class EmptyStructure() : Exception("Structure has no nodes/elements. Add some elements before attempting to solve the system.");