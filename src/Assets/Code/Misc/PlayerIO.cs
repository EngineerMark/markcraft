using Markcraft;
using System;
using System.Collections;
using System.Collections.Generic;
using UM;
using UnityEngine;
using UnityEngine.UI;

public class PlayerIO : MonoBehaviour
{

    public static PlayerIO singleton;

    private Camera cam;

    [SerializeField] float maxInteractionRange = 8;

    [SerializeField] private Inventory inventory;

    [Header("UI")]
    [SerializeField] Text inventoryText;
    [SerializeField] GameObject inventoryObject;
    [SerializeField] KeyCode inventoryToggleKey = KeyCode.E;

    //variables for the block break effect
    private Queue<GameObject> blockbreakQueue; // Store multiple cubes so you dont need to intialize during the destruction, thus reducing performance hits
    [Header("Block Destruction")]
    [Tooltip("Amount to pool in order to save performance in runtime")]
    [SerializeField] int preGeneration = 150;
    [Tooltip("Amount of 'particles' that drop when destroying a block")]
    [SerializeField] int effectAmount = 10;
    GameObject effectParent;

    [SerializeField] Shader effectShader;

    void Start()
    {
        singleton = this;
        cam = GetComponent<Camera>();
        inventory = new Inventory();
        blockbreakQueue = new Queue<GameObject>();
        // Create empty go for storing the physical objects in
        effectParent = new GameObject("BlockEffectQueue");
        for (int i = 0; i < preGeneration; i++)
        {
            GenerateNewParticle(effectParent.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Fixing performance issues (Hiding anything behind camera)
        for (int i = 0; i < Chunk.chunks.Count; i++)
        {
            if (Chunk.chunks[i] == null) continue;

            Vector3 screenPoint = cam.WorldToScreenPoint(Chunk.chunks[i].ChunkPosition);
            if(GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(cam),
                Chunk.chunks[i].GetComponent<Renderer>().bounds) 
                && Vector3.Distance(Chunk.chunks[i].ChunkPosition,cam.transform.position)>Chunk.Width*2){
                if (!Chunk.chunks[i].gameObject.GetComponent<MeshRenderer>().enabled)
                    Chunk.chunks[i].gameObject.GetComponent<MeshRenderer>().enabled = true;
            }else{
                if (Chunk.chunks[i].gameObject.GetComponent<MeshRenderer>().enabled)
                    Chunk.chunks[i].gameObject.GetComponent<MeshRenderer>().enabled = false;
            }
        }
        
        // Block destroy effects
        foreach (BlockParticle bp in effectParent.GetComponentsInChildren<BlockParticle>())
        {
            if (bp.shouldDequeue)
            {
                bp.shouldDequeue = false;
                GameObject go = bp.gameObject;
                go.SetActive(false);
                go.transform.position = new vec3(0).ToVector3();
                blockbreakQueue.Enqueue(go);
            }
        }

        // Old inventory
        if (Input.GetKeyDown("1")) { inventory.Selected = 0; inventoryText.text = inventory.Stringify(); }
        else if (Input.GetKeyDown("2")) { inventory.Selected = 1; inventoryText.text = inventory.Stringify(); }
        else if (Input.GetKeyDown("3")) { inventory.Selected = 2; inventoryText.text = inventory.Stringify(); }
        else if (Input.GetKeyDown("4")) { inventory.Selected = 3; inventoryText.text = inventory.Stringify(); }
        else if (Input.GetKeyDown("5")) { inventory.Selected = 4; inventoryText.text = inventory.Stringify(); }
        else if (Input.GetKeyDown("6")) { inventory.Selected = 5; inventoryText.text = inventory.Stringify(); }
        else if (Input.GetKeyDown("7")) { inventory.Selected = 6; inventoryText.text = inventory.Stringify(); }
        else if (Input.GetKeyDown("8")) { inventory.Selected = 7; inventoryText.text = inventory.Stringify(); }
        else if (Input.GetKeyDown("9")) { inventory.Selected = 8; inventoryText.text = inventory.Stringify(); }

        // New inventory
        if (Input.GetKeyUp(inventoryToggleKey))
        {
            inventoryObject.SetActive(!inventoryObject.activeSelf);
            Cursor.visible = inventoryObject.activeSelf;
        }

        // Handle block placement/destruction
        if (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1)) return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.5f));

        if (Physics.Raycast(ray, out RaycastHit hit, maxInteractionRange))
        {
            Chunk chunk = hit.transform.GetComponent<Chunk>();
            if (chunk == null)
                return;

            Vector3 p = hit.point;

            p.y /= WorldGen.singleton.brickHeight;
            if (Input.GetMouseButtonDown(0))
            {
                p -= hit.normal / 4;
                Chunk updateChunk = Chunk.FindChunk(p);
                inventory.AddBlock(updateChunk.GetByte(p));
                updateChunk.SetBrick(0, p);
                inventoryText.text = inventory.Stringify();
                GenerateBlockBreakEffect(vec3.ToVec3(p), true);
                updateChunk.SaveChunk();
            }
            else if (inventory.HasBlock(inventory.Selected))
            {
                int block = inventory.GetBlockAtSelection(inventory.Selected);
                if (block > 0 && inventory.GetBlockAmount(block) > 0)
                {
                    p += hit.normal / 4;
                    Chunk updateChunk = Chunk.FindChunk(p);
                    updateChunk.SetBrick(block, p);
                    inventory.RemoveBlock(block);
                }
            }
        }
        inventoryText.text = inventory.Stringify();
    }

    public GameObject GenerateNewParticle(Transform parent, bool enqueue = true)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = Vector3.zero;
        go.transform.localScale = new vec3(WorldGen.singleton.brickHeight * 0.08f).ToVector3();

        go.transform.SetParent(parent.transform);

        go.AddComponent<Rigidbody>();
        go.AddComponent<BlockParticle>();

        if (enqueue)
        {
            go.SetActive(false);
            blockbreakQueue.Enqueue(go);
        }

        return go;
    }

    public void GenerateBlockBreakEffect(vec3 p, bool random = false)
    {
        for (int i = 0; i < effectAmount; i++)
        {
            vec3 updatedPos = new vec3();
            if (random)
            {
                System.Random rand = new System.Random((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                updatedPos.x = rand.Next(-50, 50) * 0.01f;
                //updatedPos.y = rand.Next(-100, 100) * 0.01f;
                updatedPos.z = rand.Next(-50, 50) * 0.01f;
                updatedPos += new vec3(0.25f,0.25f,0.25f);
            }
            GameObject go;
            if (blockbreakQueue.Count > 1)
                go = blockbreakQueue.Dequeue();
            else
                go = GenerateNewParticle(effectParent.transform, false);


            go.transform.position = (p + updatedPos).ToVector3();
            go.SetActive(true);

            Material mat = new Material(effectShader);
            Renderer renderer = go.GetComponent<Renderer>();
            BlockParticle part = go.GetComponent<BlockParticle>();
            part.shouldDequeue = false;
            renderer.material = mat;
        }
    }
}

// Block effect code
public class BlockParticle : MonoBehaviour
{
    // Timer
    private float timer = 0;
    private float maxTimer = 10;
    public bool shouldDequeue = false;

    public void Update()
    {
        timer += Time.deltaTime;
        if (timer > maxTimer)
        {
            // Enqueue and disable
            shouldDequeue = true;
            timer = 0;
        }
    }
}