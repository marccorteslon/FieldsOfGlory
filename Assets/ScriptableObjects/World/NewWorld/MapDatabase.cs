using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FieldsOfGlory/World/NewWorld/Map Database")]
public class MapDatabase : ScriptableObject
{
    public List<MapNodeDefinition> nodes = new();
    public List<MapConnectionDefinition> connections = new();

    public MapNodeDefinition GetNodeById(string nodeId)
    {
        foreach (var node in nodes)
        {
            if (node != null && node.nodeId == nodeId)
                return node;
        }

        return null;
    }

    public List<MapConnectionDefinition> GetConnectionsFromNode(string nodeId)
    {
        List<MapConnectionDefinition> result = new();

        foreach (var connection in connections)
        {
            if (connection == null) continue;

            if (connection.nodeAId == nodeId || connection.nodeBId == nodeId)
                result.Add(connection);
        }

        return result;
    }

    public string GetOtherNodeId(string currentNodeId, MapConnectionDefinition connection)
    {
        if (connection.nodeAId == currentNodeId)
            return connection.nodeBId;

        if (connection.nodeBId == currentNodeId)
            return connection.nodeAId;

        return null;
    }

    public MapDirection GetDirectionFromNode(string currentNodeId, MapConnectionDefinition connection)
    {
        if (connection.nodeAId == currentNodeId)
            return connection.directionFromA;

        if (connection.nodeBId == currentNodeId)
            return connection.directionFromB;

        return MapDirection.Up;
    }
}