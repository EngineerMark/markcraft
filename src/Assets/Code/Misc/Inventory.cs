using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Markcraft
{
    [System.Serializable]
    public class Inventory
    {
        //[SerializeField] private List<BlockAmount> inv;
        [SerializeField] private Dictionary<int, int> inv;
        [SerializeField] private byte selected = 0;

        public byte Selected { get => selected; set => selected = value; }

        public Inventory()
        {
            inv = new Dictionary<int, int>();
            //1st = blocktype, 2nd = amount
        }

        public bool HasBlock(int block)
        {
            return (GetPair(block) == -1 ? false : true);
        }

        public bool HasBlock(byte position){
            if (inv.Count!=0 && inv.Count >= position)
                return true;
            return false;
        }

        public int GetBlockAtSelection(int selection){
            if (inv.Count > 0 && inv.Count >= selection){
                int i = -1;
                foreach (KeyValuePair<int, int> kv in inv)
                {
                    i++;
                    if (i != selection) continue;
                    return kv.Key;
                }
            }
            return -1;
        }

        public int GetBlockAmount(int block){
            if (inv.ContainsKey(block))
                return inv[block];
            return 0;
        }

        public int GetPair(int block)
        {
            //foreach (BlockAmount baPair in inv)
            //{
            //    if (baPair.BlockType != block) continue;
            //    return baPair;
            //}
            if (inv.ContainsKey(block) && inv[block]!=0)
                return block;
            return -1;
        }

        public void AddBlock(int block)
        {
            // Check if exists
            if (inv.ContainsKey(block))
                inv[block]++;
            else
                inv.Add(block, 1);
        }

        public void RemoveBlock(int block)
        {
            if (inv.ContainsKey(block))
                if(inv[block]>0)    
                    inv[block]--;
        }

        public string Stringify(){
            string s = "";
            int i = 0;
            foreach(KeyValuePair<int,int> ba in inv){
                if (i==Selected)
                    s += "> ";
                s += (Block)Enum.ToObject(typeof(Block), ba.Key) + ": " + ba.Value;
                s += "\n";
                i++;
            }
            return s;
        }
    }

    [System.Serializable]
    public class BlockAmount
    {
        [SerializeField] private int blockType;
        [SerializeField] private int amount;

        public int BlockType { get => blockType; set => blockType = value; }
        public int Amount { get => amount; set => amount = value; }
    }
}