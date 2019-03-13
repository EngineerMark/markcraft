using Markcraft;
using System;
using System.Collections;
using System.Collections.Generic;
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

    void Start()
    {
        singleton = this;
        cam = GetComponent<Camera>();
        inventory = new Inventory();
    }

    // Update is called once per frame
    void Update()
    {
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
}
