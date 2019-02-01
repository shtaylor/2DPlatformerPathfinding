using System.Collections;
using System.Collections.Generic;
using Priority_Queue;
using UnityEngine;
/// <summary>
/// Author: Shane Taylor
/// This is the in-game graph object. It uses a 2-dimensional array
/// to organize which messages to give to the entity. For example,
/// the message to travel from u to v is messageList[u][v]. It also
/// contains the mathematical/abstract game graph that it uses for
/// performing searches on. This class must be "setup" by the
/// NavGraphPreLauncher to ensure all the nodes are instantiated and
/// correctly linked before this class creates the abstract graph.
/// </summary>
public class NavGraphPlatformer : MonoBehaviour {

    // The index will also be the node's ID.
    Node[] nodeList;

    // The message to travel from u to v is messageList[u][v].
    public Message[][] messageList;

    // The abstract graph that searching happens in.
    public SparseGraph<GraphNode, GraphEdge> G;


    // I'm toying around with the idea of optimizing the searching.
    // If two entities have the same destination node, and one of them is
    // on the path that the other entity will take (and has already calculated the
    // path for) then, the second entity should get the same path.
    public List<List<int>> currentSetOfPaths;

    // I'm not sure how I'm going to access all of the entities... maybe by a dictionary
    // by their id? Is that something I can easily find out?
    public Dictionary<string, GroundedEntity> allGroundedEntities;

    // We dont want to run any methods unless the graph has been setup
    bool hasBeenSetup = false;

    public int playerLastNodeID = -1;

    public PlayerController player;

    public GameObject playerPrefab;

    Vector3 safeSpawnPosition = Vector3.zero;

    private void Awake()
    {
        allGroundedEntities = new Dictionary<string, GroundedEntity>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        safeSpawnPosition = player.transform.position;
    }

    private void Update()
    {
        if (player == null && Input.GetKeyDown(KeyCode.Escape))
        {
            player = Instantiate<GameObject>(playerPrefab, safeSpawnPosition, Quaternion.identity).GetComponent<PlayerController>();
        }
    }

    // TODO: I need to set up the allGroundedEntities Dictionary
    public void SetupGraph()
    {
        // SET UP THE GRAPH...
        if (hasBeenSetup)
        {
            return;
        }

        // the game objects containing the node graphs
        GameObject[] nodeGameObjects = GameObject.FindGameObjectsWithTag("Node");


        // The nodes in the game world
        nodeList = new Node[nodeGameObjects.Length];

        // initialize the message list dual array
        messageList = new Message[nodeList.Length][];
        for (int i = 0; i < nodeList.Length; i++)
        {
            messageList[i] = new Message[nodeList.Length];
        }

        // Initialize the list of messages
        for (int i = 0; i < nodeList.Length; i++)
        {
            for (int j = 0; j < nodeList.Length; j++)
            {
                messageList[i][j] = Message.DoNothing;
            }
        }

        // Create the abstract graph using the info scanned from the level
        G = new SparseGraph<GraphNode, GraphEdge>();

        // we'll be adding new nodes to the abstract graph, as well as set the ID's of the nodes in the level
        for (int i = 0; i < nodeGameObjects.Length; i++)
        {
            // set the reference for nodeList[i] 
            nodeList[i] = nodeGameObjects[i].GetComponent<Node>();

            // update the node's ID
            nodeList[i].ID = i;

            // add the node to the abstract graph
            G.AddNode(new GraphNode(i));

        }

        // Create the edges in the abstract graph and set the weights
        // Note that the weights are determined heuristically within the node scripts
        // Also, set the correct message in the n by n messageList[][] so that later when we have the information that the path
        //     from say, 1 to 2 is to be taken, then the message to send the game character is messageList[1][2]
        for (int i = 0; i < nodeList.Length; i++)
        {
            nodeList[i].SetWeights();

            // we need to get the information for the edge weight between nodes and create the edge in the abstract graph
            //     so that we can search it efficiently for a path
            for (int j = 0; j < nodeList[i].neighbors.Count; j++)
            {
                // i is the current node ID
                // nodeList[i].neighbors[j].ID is the ID of the current neighbor
                // same with the weight
                G.AddEdge(new GraphEdge(i, nodeList[i].neighbors[j].ID, nodeList[i].weights[j]));

                // save the way to traverse this edge in the dual array
                // This makes it so that you can access the message to get from node u to node v
                //     by looking up messageList[u][v]
                messageList[nodeList[i].ID][nodeList[i].neighbors[j].ID] = nodeList[i].messages[j];
            }
        }
        
        hasBeenSetup = true;

    }

    public List<int> GetPathIDs(int source, int target)
    {
        if (!hasBeenSetup)
        {
            Debug.Log("Graph has not been set up yet.");
            return null;
        }
        Graph_Dijkstra<SparseGraph<GraphNode, GraphEdge>> dijkstra = new Graph_Dijkstra<SparseGraph<GraphNode, GraphEdge>>(G, source, target);
        List<int> path;
        try
        {
            path = dijkstra.GetPath();
        }
        catch (System.Exception)
        {
            return new List<int> { source };
        }
        
        return path;
    }

    // Note that it may be necessary to have the entity reverse the path after the
    public Dictionary<int,Message> GetPathMessages(List<int> path)
    {
        if (!hasBeenSetup)
        {
            Debug.Log("Graph has not been set up yet.");
            return null;
        }

        // The path that the GroundedEntity will be passing this is
        // in the reverse order from what it needs to be...
        // I think...
        List<int> p = new List<int>(path);
        //p.Reverse();
        if (p.Count < 1)
        {
            return null;
        }
        Dictionary<int, Message> pathMessages = new Dictionary<int, Message>();

        for (int i = 0; i < p.Count-1; i++)
        {
            pathMessages.Add(p[i], messageList[p[i]][p[i + 1]]);
        }

        pathMessages.Add(p[path.Count - 1], Message.DoNothing);

        return pathMessages;
    }

    public void AddGroundedEntity(string ID, GroundedEntity entity)
    {
        if (allGroundedEntities != null && !allGroundedEntities.ContainsKey(ID))
        {
            allGroundedEntities.Add(ID, entity);
        }
    }

    public int GetGroundedEntitiesLastWalkNodeID(string ID)
    {
        if (!allGroundedEntities.ContainsKey(ID))
        {
            return -1;
        }
        else
        {
            return allGroundedEntities[ID].lastNodeID;
        }
    }

    public Vector3 PhysicalLocationOfNode(int nodeID)
    {
        if (nodeID != -1)
        {
            return nodeList[nodeID].gameObject.transform.position;
        }

        else return new Vector3(0, 0, -1);
    }
}