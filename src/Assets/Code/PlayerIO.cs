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

    //variables for the block break effect
    private Queue<GameObject> blockbreakQueue; // Store multiple cubes so you dont need to intialize during the destruction, thus reducing performance hits
    [Header("Block Destruction")]
    [Tooltip("Amount to pool in order to save performance in runtime")]
    [SerializeField] int preGeneration = 150;
    [Tooltip("Amount of 'particles' that drop when destroying a block")]
    [SerializeField] int effectAmount = 10;
    GameObject effectParent;

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
        foreach(BlockParticle bp in effectParent.GetComponentsInChildren<BlockParticle>()){
            if(bp.shouldDequeue){
                bp.shouldDequeue = false;
                GameObject go = bp.gameObject;
                go.SetActive(false);
                go.transform.position = new vec3(0).ToVector3();
                blockbreakQueue.Enqueue(go);
            }
        }

        if (Input.GetKeyDown("1")) { inventory.Selected = 0; inventoryText.text = inventory.Stringify(); }
        else if (Input.GetKeyDown("2")) { inventory.Selected = 1; inventoryText.text = inventory.Stringify(); }
        else if (Input.GetKeyDown("3")) { inventory.Selected = 2; inventoryText.text = inventory.Stringify(); }
        else if (Input.GetKeyDown("4")) { inventory.Selected = 3; inventoryText.text = inventory.Stringify(); }
        else if (Input.GetKeyDown("5")) { inventory.Selected = 4; inventoryText.text = inventory.Stringify(); }
        else if (Input.GetKeyDown("6")) { inventory.Selected = 5; inventoryText.text = inventory.Stringify(); }
        else if (Input.GetKeyDown("7")) { inventory.Selected = 6; inventoryText.text = inventory.Stringify(); }
        else if (Input.GetKeyDown("8")) { inventory.Selected = 7; inventoryText.text = inventory.Stringify(); }
        else if (Input.GetKeyDown("9")) { inventory.Selected = 8; inventoryText.text = inventory.Stringify(); }

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
                vec3 v = vec3.ToVec3(p);
                v.x += UnityEngine.Random.Range(-10,10)*0.1f;
                v.y += UnityEngine.Random.Range(-10,10)*0.1f;
                v.z += UnityEngine.Random.Range(-10,10)*0.1f;
                GenerateBlockBreakEffect(v);
            }
            else if (inventory.HasBlock(inventory.Selected))
            {
                int block = inventory.GetBlockAtSelection(inventory.Selected);
                if (block > 0 && inventory.GetBlockAmount(block)>0)
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

    public GameObject GenerateNewParticle(Transform parent, bool enqueue = true){
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

    public void GenerateBlockBreakEffect(vec3 p){
        for (int i = 0; i < effectAmount; i++)
        {
            GameObject go;
            if (blockbreakQueue.Count > 1)
                go = blockbreakQueue.Dequeue();
            else
                go = GenerateNewParticle(effectParent.transform,false);

            go.transform.position = p.ToVector3();
            go.SetActive(true);
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

    public void Update(){
        timer += Time.deltaTime;
        if(timer>maxTimer){
            // Enqueue and disable
            shouldDequeue = true;
        }
    }
}