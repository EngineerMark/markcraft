﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Markcraft
{
    public class WorldGen : MonoBehaviour
    {
        public static WorldGen singleton;

        public int CHUNK_WIDTH = 16;
        public int CHUNK_HEIGHT = 256;
        public int octaves = 4;
        public float period = 20.0f;
        public float persistence = 0.8f;

        public float viewRange = 30;

        public float brickHeight = 1;

        public Chunk chunkPrefab;
        public GameObject grassPrefab;

        public float updateRate = 1;

        void Awake()
        {
            singleton = this;
        }

        private void Start()
        {
            InvokeRepeating("UpdateTerrain", 0, updateRate);
        }

        void UpdateTerrain()
        {
            for (float x = transform.position.x-viewRange; x < transform.position.x+viewRange; x+=CHUNK_WIDTH)
            {
                for (float z = transform.position.z-viewRange; z < transform.position.z+viewRange; z+= CHUNK_WIDTH)
                {
                    Vector3 pos = new Vector3(x, 0, z);
                    pos.x = Mathf.Floor(pos.x / (float)CHUNK_WIDTH) * CHUNK_WIDTH;
                    pos.z = Mathf.Floor(pos.z / (float)CHUNK_WIDTH) * CHUNK_WIDTH;

                    if (Chunk.FindChunk(pos) != null) continue;
                    Instantiate(chunkPrefab, pos, Quaternion.identity);
                }
            }
        }
    }
}