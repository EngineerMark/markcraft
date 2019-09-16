using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Markcraft
{
    public class ChunkTerrainGen : ChunkGenerator
    {
        public ChunkTerrainGen(Chunk chunk) : base(chunk) { }

        public override void Generate()
        {
            for (int x = 0; x < Chunk.Width; x++)
            {
                for (int y = 0; y < Chunk.Height; y++)
                {
                    for (int z = 0; z < Chunk.Width; z++)
                    {
                        chunk.chunkData[x, y, z] = GetTheoreticalByte(new Vector3(x, y, z) + chunk.ChunkPosition);
                    }
                }
            }
        }

        public static byte GetTheoreticalByte(Vector3 pos)
        {
            Vector3 grain0Offset = new Vector3((float)Chunk.random.NextDouble() * 10000, (float)Chunk.random.NextDouble() * 10000, (float)Chunk.random.NextDouble() * 10000);
            Vector3 grain1Offset = new Vector3((float)Chunk.random.NextDouble() * 10000, (float)Chunk.random.NextDouble() * 10000, (float)Chunk.random.NextDouble() * 10000);
            Vector3 grain2Offset = new Vector3((float)Chunk.random.NextDouble() * 10000, (float)Chunk.random.NextDouble() * 10000, (float)Chunk.random.NextDouble() * 10000);
            return GetTheoreticalByte(pos, grain0Offset, grain1Offset, grain2Offset);
        }

        public static byte GetTheoreticalByte(Vector3 pos, Vector3 offset0, Vector3 offset1, Vector3 offset2)
        {
            Biome biome = Biome.DESERT;
            IntVector3 newPos = new IntVector3((int)(pos.x + (int.MaxValue) * 0.5f), 0, (int)(pos.z + (int.MaxValue) * 0.5f));
            //heightmap += Noise.Generate(newPos.x * 0.07f, newPos.y * 0.07f) * 20;
            Block brick = Block.Air;

            float value = Mathf.Abs(NoiseWrapper.PerlinNoise(newPos.x, newPos.y, newPos.z, 100, 6, 2.3f));

            value += NoiseWrapper.Ridgenoise(new Vector3(newPos.x, newPos.y, newPos.z) * 1.242f);
            float stone = NoiseWrapper.PerlinNoise(newPos.x, newPos.y, newPos.z, 10, 3, 1.1f);
            stone += NoiseWrapper.PerlinNoise(newPos.x, newPos.y, newPos.z, 20, 4, 0);
            stone += 10;
            stone /= 1.1f;
            stone += value;

            float dirt = NoiseWrapper.PerlinNoise(newPos.x, newPos.y, newPos.z, 50, 2, 0) + 1;

            if (pos.y < stone)
                brick = Block.Stone;
            else if (pos.y < dirt + stone)
                if (pos.y < 15)
                    brick = Block.Dirt;
                else
                {
                    if (biome == Biome.DESERT)
                        brick = (new System.Random(System.DateTime.Now.Millisecond).Next(1, 100) < 50) ? Block.Sand : Block.DarkSand;
                    else
                        brick = Block.Grass;
                }

            if (pos.y <= 25 && brick == Block.Air)
                brick = Block.Water;

            //if (cave < 0.1f && brick != Block.Air)
            //    brick = Block.Air;


            if (pos.y == 0)
                brick = Block.Bedrock;

            return (byte)brick;
        }
    }
}