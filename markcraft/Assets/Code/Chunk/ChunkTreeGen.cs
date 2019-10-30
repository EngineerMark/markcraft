using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Markcraft
{
    public class VectorBlockPair
    {
        public IntVector3 pos;
        public Block block;
    }

    public class ChunkTreeGen : ChunkGenerator
    {
        public List<Chunk> chunksToUpdate;

        public static Dictionary<IntVector3, List<VectorBlockPair>> cachedBlocks;

        public ChunkTreeGen(Chunk chunk) : base(chunk)
        {
            if (cachedBlocks == null)
                cachedBlocks = new Dictionary<IntVector3, List<VectorBlockPair>>();
            chunksToUpdate = new List<Chunk>();
        }

        public override void Generate()
        {
            Vector3 treePosition = Vector3.zero;
            for (int x = 0; x < Chunk.Width; x++)
            {
                for (int z = 0; z < Chunk.Width; z++)
                {
                    bool shouldPlace = Mathf.PerlinNoise((chunk.ChunkPosition.x + x) * 0.1f, (chunk.ChunkPosition.z + z) * 0.1f) > 0.75f;
                    if (shouldPlace)
                        for (int y = Chunk.Height - 2; y > 0; y--)
                        {
                            if (chunk.chunkData[x, y, z] == (int)Block.Grass)
                            {
                                treePosition = new Vector3(x, y + 1, z);
                                PlaceTree(treePosition);
                                break;
                            }
                        }

                }
            }
        }

        private void PlaceTree(Vector3 startPos)
        {
            int height = 0;
            if (startPos.y < Chunk.Height - 25)
                for (int i = 0; i < 7; i++)
                {
                    PlaceTreeBlock(Block.Wood, new Vector3((int)startPos.x, (int)startPos.y + i, (int)startPos.z));
                    height++;
                }

            startPos.y += height;
            for (int x = 0; x < 5; x++)
                for (int z = 0; z < 5; z++)
                    for (int y = 0; y < 3; y++)
                    {
                        if (y == 2 && (x == 0 || x == 4 || z == 0 || z == 4)) continue;
                        Vector3 position = new Vector3(startPos.x - 2 + x, startPos.y - 2 + y, startPos.z - 2 + z);
                        PlaceTreeBlock(Block.Leaves, position);
                    }
        }

        private void PlaceTreeBlock(Block block, Vector3 pos)
        {
            if (pos.x < 0 || pos.x > Chunk.Width-1 || pos.z < 0 || pos.z > Chunk.Width-1)
            {
                // Position out of chunk bounds. Place in adjecent chunk
                CacheToNewChunk(block, pos);
                return;
            };
            if (chunk.chunkData[(int)pos.x, (int)pos.y, (int)pos.z] != (int)Block.Air) return;
            chunk.chunkData[(int)pos.x, (int)pos.y, (int)pos.z] = (int)block;
        }

        private void CacheToNewChunk(Block block, Vector3 thisChunkBlockPosition)
        {
            IntVector3 chunkPosition = new IntVector3(chunk.ChunkPosition.x, chunk.ChunkPosition.y, chunk.ChunkPosition.z);
            IntVector3 blockPosition = new IntVector3(thisChunkBlockPosition.x, thisChunkBlockPosition.y, thisChunkBlockPosition.z);

            // Single axis check
            if (thisChunkBlockPosition.x < 0)
            {
                chunkPosition.x -= Chunk.Width;
                blockPosition.x += Chunk.Width;
            }
            if (thisChunkBlockPosition.x > Chunk.Width-1)
            {
                chunkPosition.x += Chunk.Width;
                blockPosition.x -= Chunk.Width;
            }
            if (thisChunkBlockPosition.z < 0)
            {
                chunkPosition.z -= Chunk.Width;
                blockPosition.z += Chunk.Width;
            }
            if (thisChunkBlockPosition.z > Chunk.Width-1)
            {
                chunkPosition.z += Chunk.Width;
                blockPosition.z -= Chunk.Width;
            }


            // Check if chunk exists first
            Chunk c = Chunk.FindChunk(new Vector3(chunkPosition.x, chunkPosition.y, chunkPosition.z));
            if (c != null)
            {
                if (c.chunkData != null)
                {
                    if (c.chunkData[blockPosition.x, blockPosition.y, blockPosition.z] != (int)Block.Air) return;
                    c.chunkData[blockPosition.x, blockPosition.y, blockPosition.z] = (int)block;

                    if (!chunksToUpdate.Contains(c))
                        chunksToUpdate.Add(c);

                    return;
                }
            }

            // Cache objects for later use in newer generated chunks
            VectorBlockPair pair = new VectorBlockPair()
            {
                block = block,
                pos = blockPosition
            };

            // Check if list doesnt exists and create
            if (!cachedBlocks.ContainsKey(chunkPosition))
                cachedBlocks.Add(chunkPosition, new List<VectorBlockPair>());

            List<VectorBlockPair> allPairs = cachedBlocks[chunkPosition];
            // Check if position is claimed
            for (int i = 0; i < allPairs.Count; i++)
            {
                if (allPairs[i].pos == pair.pos)
                    return;
            }

            cachedBlocks[chunkPosition].Add(pair);
        }
    }
}