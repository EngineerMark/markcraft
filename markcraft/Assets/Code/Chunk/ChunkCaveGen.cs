using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Markcraft
{
    public class ChunkCaveGen : ChunkGenerator
    {
        public ChunkCaveGen(Chunk chunk) : base(chunk) { }

        public override void Generate()
        {
            for (int x = 0; x < Chunk.Width; x++)
            {
                for (int y = 0; y < Chunk.Height; y++)
                {
                    for (int z = 0; z < Chunk.Width; z++)
                    {
                        float val = NoiseWrapper.PerlinNoise(x, y, z, 20, 50, 2.3f);
                        if (val < 0.3f)
                            chunk.chunkData[x, (int)(Chunk.Height*0.5f)-(int)(y*0.5f), z] = (int)Block.Air;
                    }
                }
            }
        }
    }
}