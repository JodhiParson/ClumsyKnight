using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class DugeonGenerator
{
    
    List<RoomNode> allNodesCollection = new List<RoomNode>();
    private int dungeonWidth;
    private int dungeonLength;

    public DugeonGenerator(int dungeonWidth, int dungeonLength)
    {
        this.dungeonWidth = dungeonWidth;
        this.dungeonLength = dungeonLength;
    }



    // public List<Node> CalculateDungeon(int maxIterations, int roomWidthMin, int roomLengthMin, float roomBottomCornerModifier, float roomTopCornerMidifier, int roomOffset, int corridorWidth)
    // {
    //     BinarySpacePartitioner bsp = new BinarySpacePartitioner(dungeonWidth, dungeonLength);
    //     allNodesCollection = bsp.PrepareNodesCollection(maxIterations, roomWidthMin, roomLengthMin);
    //     List<Node> roomSpaces = StructureHelper.TraverseGraphToExtractLowestLeafes(bsp.RootNode);

    //     RoomGenerator roomGenerator = new RoomGenerator(maxIterations, roomLengthMin, roomWidthMin);
    //     List<RoomNode> roomList = roomGenerator.GenerateRoomsInGivenSpaces(roomSpaces, roomBottomCornerModifier, roomTopCornerMidifier, roomOffset);

    //     CorridorsGenerator corridorGenerator = new CorridorsGenerator();
    //     var corridorList = corridorGenerator.CreateCorridor(allNodesCollection, corridorWidth);
        
    //     return new List<Node>(roomList).Concat(corridorList).ToList();
    // }
    public List<Node> CalculateDungeon(int maxIterations, int roomWidthMin, int roomLengthMin,
    float roomBottomCornerModifier, float roomTopCornerMidifier, int roomOffset, int corridorWidth)
{
    List<Node> result = null;
    int attempts = 0;
    const int maxAttempts = 10;

    while (attempts < maxAttempts)
    {
        BinarySpacePartitioner bsp = new BinarySpacePartitioner(dungeonWidth, dungeonLength);
        allNodesCollection = bsp.PrepareNodesCollection(maxIterations, roomWidthMin, roomLengthMin);
        List<Node> roomSpaces = StructureHelper.TraverseGraphToExtractLowestLeafes(bsp.RootNode);

        RoomGenerator roomGenerator = new RoomGenerator(maxIterations, roomLengthMin, roomWidthMin);
        List<RoomNode> roomList = roomGenerator.GenerateRoomsInGivenSpaces(roomSpaces, roomBottomCornerModifier, roomTopCornerMidifier, roomOffset);

        CorridorsGenerator corridorGenerator = new CorridorsGenerator();
        var corridorList = corridorGenerator.CreateCorridor(allNodesCollection, corridorWidth);

        result = new List<Node>(roomList).Concat(corridorList).ToList();

        if (StructureHelper.IsFullyConnected(result))
        {
            return result;
        }

        attempts++;
        Debug.LogWarning($"Dungeon generation attempt {attempts} produced a disconnected layout — regenerating.");
    }

    Debug.LogError($"Failed to generate a fully connected dungeon after {maxAttempts} attempts. Using last (possibly disconnected) layout.");
    return result;
}
}