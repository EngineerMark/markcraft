using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimplexNoise;
using System.Threading;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Markcraft
{
    public enum Block
    {
        Air = 0x0,
        Stone = 0x1,
        Dirt = 0x2,
        Bedrock = 0x3,
        Grass = 0x4,
        Water = 0x5,
        Wood = 0x6,
        WaveGrass = 0x7,
        Leaves = 0x8,
        Sand = 0x9,
        DarkSand = 0xA
    }

    public enum Biome
    {
        PLAINS,
        BEACH,
        DESERT,
        FOREST
    }

    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(MeshFilter))]
    [System.Serializable]
    public class Chunk : MonoBehaviour
    {
        public static List<Chunk> chunks = new List<Chunk>();

        public static int Width
        {
            get { return WorldGen.singleton.CHUNK_WIDTH; }
        }

        public static int Height
        {
            get { return WorldGen.singleton.CHUNK_HEIGHT; }
        }

        public static float BrickHeight
        {
            get { return WorldGen.singleton.brickHeight; }
        }

        private Vector3 chunkPosition;
        public Vector3 ChunkPosition { get => chunkPosition; set => chunkPosition = value; }

        [SerializeField] public int[,,] chunkData;
        public Mesh visualMesh;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private MeshCollider meshCollider;
        [SerializeField] private MeshFilter meshFilter;

        private Thread generationThread;
        private bool generatorReady = false;
        public static System.Random random;
        private bool fullyComplete = false;


        List<Vector3> vertices;
        List<Vector2> uvs;
        List<int> tris;

        List<Vector3> submeshVertices;
        List<Vector2> submeshUvs;
        List<int> submeshTris;

        ChunkTerrainGen terrainGen;
        ChunkTreeGen treeGen;
        ChunkRavineGen ravineGen;
        ChunkCaveGen caveGen;

        private void Start()
        {
            chunks.Add(this);

            terrainGen = new ChunkTerrainGen(this);
            treeGen = new ChunkTreeGen(this);
            ravineGen = new ChunkRavineGen(this);
            caveGen = new ChunkCaveGen(this);

            meshRenderer = GetComponent<MeshRenderer>();
            meshCollider = GetComponent<MeshCollider>();
            meshFilter = GetComponent<MeshFilter>();

            ChunkPosition = transform.position;

            visualMesh = new Mesh();
            NewChunk();

            gameObject.name = "Chunk " + transform.position;
        }

        private void NewChunk()
        {
            if (random == null)
                random = new System.Random(GameManager.saveSeed);
            UnityEngine.Random.InitState(GameManager.saveSeed);
            generationThread = new Thread(ThreadSystem);
            ChunkManager.self.Add(generationThread);
        }

        public void SaveChunk()
        {
            GameManager.saveLoadSystem.SaveChunk(this);
        }

        public void Update()
        {
            if (!generatorReady) return;
            if (fullyComplete) return;
            ChunkManager.self.Continue(generationThread);
            for (int i = 0; i < treeGen.chunksToUpdate.Count; i++)
                StartCoroutine(treeGen.chunksToUpdate[i].CreateVisualMeshAsync(true));
            StartCoroutine(CreateVisualMeshAsync());
            fullyComplete = true;
        }

        public void ThreadSystem()
        {
            if (GameManager.saveLoadSystem.LoadChunk(this))
            {
                CreateVisualMesh();
                generatorReady = true;
            }
            else
            {
                CalculateMapFromScratch();
                generatorReady = true;
            }
        }

        public void GenerateGrass()
        {
            // Generate a single grass prefab on top of each highest grass block
            // Raycast no option since not thread-safe
            // Forced to do inefficient 3rd nested loop for y-axis
            for (int x = 0; x < Width; x++)
            {
                for (int z = 0; z < Width; z++)
                {
                    for (int y = Height; y > 0; y--)
                    {
                        Vector3 p = new Vector3(x, y, z) + chunkPosition;
                        Block b = (Block)GetByte(p);
                        if (b == Block.Grass)
                            if (y < Height - 1)
                                chunkData[x, y + 1, z] = (int)Block.WaveGrass;
                    }
                }
            }
        }

        public virtual void GenerateMap()
        {
            terrainGen.Generate();
            caveGen.Generate();
            treeGen.Generate();

            // Check cached objects

            //if(ChunkTreeGen.cachedBlocks.ContainsKey(IntVector3.FromVector3(chunkPosition))){
            //    List<VectorBlockPair> cachedBlocks = ChunkTreeGen.cachedBlocks[IntVector3.FromVector3(chunkPosition)];
            //    for (int i = 0; i < cachedBlocks.Count; i++)
            //        if(chunkData[cachedBlocks[i].pos.x, cachedBlocks[i].pos.y, cachedBlocks[i].pos.z]==(int)Block.Air)
            //            chunkData[cachedBlocks[i].pos.x, cachedBlocks[i].pos.y, cachedBlocks[i].pos.z] = (int)cachedBlocks[i].block;
            //}
        }

        public virtual void CalculateMapFromScratch()
        {
            chunkData = new int[Width, Height, Width];

            //Vector3 grain0Offset = new Vector3((float)random.NextDouble() * 10000, (float)random.NextDouble() * 10000, (float)random.NextDouble() * 10000);
            //Vector3 grain1Offset = new Vector3((float)random.NextDouble() * 10000, (float)random.NextDouble() * 10000, (float)random.NextDouble() * 10000);
            //Vector3 grain2Offset = new Vector3((float)random.NextDouble() * 10000, (float)random.NextDouble() * 10000, (float)random.NextDouble() * 10000);
            GenerateMap();
            CreateVisualMesh();
        }

        public static Biome Biome(float value)
        {
            return Markcraft.Biome.FOREST;
        }

        public static float CalculateNoiseValue(Vector3 pos, Vector3 offset, float scale)
        {
            float noiseX = Mathf.Abs((pos.x + offset.x) * scale);
            float noiseY = Mathf.Abs((pos.y + offset.y) * scale);
            float noiseZ = Mathf.Abs((pos.z + offset.z) * scale);

            float value = Mathf.PerlinNoise(noiseX, noiseZ) + SimplexNoise.Noise.Generate(noiseX, noiseY, noiseZ);

            for (int i = 0; i < WorldGen.singleton.octaves; i++)
                value += ((1 / (i + 1)) * Mathf.PerlinNoise(noiseX + Mathf.Sqrt(i + 1), noiseZ + Mathf.Sqrt(i + 1)));
            value /= WorldGen.singleton.octaves;

            value += Mathf.Abs(Mathf.PerlinNoise(noiseX * 0.02f, noiseZ * 0.02f) * 40);

            return Mathf.Max(0, value);
        }

        public virtual IEnumerator CreateVisualMeshAsync(bool gen = false)
        {
            if (gen)
                CreateVisualMesh();
            ApplyChanges();
            yield return 0;
        }

        public virtual void CreateVisualMesh()
        {
            vertices = new List<Vector3>();
            uvs = new List<Vector2>();
            tris = new List<int>();

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    for (int z = 0; z < Width; z++)
                    {
                        if (chunkData[x, y, z] == 0) continue;

                        int brick = chunkData[x, y, z];
                        if (brick != (int)Block.WaveGrass)
                        {
                            if (IsTransparent(x - 1, y, z)) BuildBlockFace(brick, new Vector3(x, y, z), Vector3.up, Vector3.forward, false, vertices, uvs, tris);
                            if (IsTransparent(x + 1, y, z)) BuildBlockFace(brick, new Vector3(x + 1, y, z), Vector3.up, Vector3.forward, true, vertices, uvs, tris);

                            if (IsTransparent(x, y - 1, z)) BuildBlockFace(brick, new Vector3(x, y, z), Vector3.forward, Vector3.right, false, vertices, uvs, tris);
                            if (IsTransparent(x, y + 1, z)) BuildBlockFace(brick, new Vector3(x, y + 1, z), Vector3.forward, Vector3.right, true, vertices, uvs, tris, true);

                            if (IsTransparent(x, y, z - 1)) BuildBlockFace(brick, new Vector3(x, y, z), Vector3.up, Vector3.right, true, vertices, uvs, tris);
                            if (IsTransparent(x, y, z + 1)) BuildBlockFace(brick, new Vector3(x, y, z + 1), Vector3.up, Vector3.right, false, vertices, uvs, tris);
                        }
                        else
                        {
                            BuildVegetationModel(brick, new Vector3(x + 0.5f, y, z + 0.5f), vertices, uvs, tris);
                            BuildVegetationModel(brick, new Vector3(x + 0.5f, y, z + 0.5f), vertices, uvs, tris, true);
                            BuildVegetationModel(brick, new Vector3(x + 0.5f, y, z + 0.5f), vertices, uvs, tris, false, 90);
                            BuildVegetationModel(brick, new Vector3(x + 0.5f, y, z + 0.5f), vertices, uvs, tris, true, 90);
                        }
                    }
        }

        public void ApplyChanges()
        {
            //GenerateGrass();
            visualMesh = new Mesh
            {
                vertices = vertices.ToArray(),
                uv = uvs.ToArray(),
                triangles = tris.ToArray()
            };

            visualMesh.RecalculateBounds();
            visualMesh.RecalculateNormals();

            meshFilter.mesh = visualMesh;
            meshCollider.sharedMesh = visualMesh;
        }

        public virtual void BuildVegetationModel(int brick, Vector3 position, List<Vector3> verts, List<Vector2> uvs, List<int> tris, bool mirror = false, float rotation = 0f)
        {
            int index = verts.Count;
            int textureSize = 64;
            float textureDensity = (1024f / textureSize);

            verts.Add(position - (Quaternion.Euler(0, rotation, 0) * new Vector3(0.5f, 0, 0.5f)));
            verts.Add(position + (Quaternion.Euler(0, rotation, 0) * new Vector3(0.5f, 0, 0.5f)));
            verts.Add(position + (Quaternion.Euler(0, rotation, 0) * new Vector3(0.5f, 1, 0.5f)));
            verts.Add(position - (Quaternion.Euler(0, rotation, 0) * new Vector3(0.5f, -1, 0.5f)));

            Vector2 uvWidth = new Vector2(textureSize / 1024f, textureSize / 1024f);
            Vector2 uvCorner = new Vector2(0, 1024f - (textureSize / 1024f));

            uvCorner.x += ((float)(brick - 1) / textureDensity);

            uvs.Add(uvCorner);
            uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));
            uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
            uvs.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));

            if (!mirror)
            {
                tris.Add(index + 1);
                tris.Add(index + 0);
                tris.Add(index + 2);
                tris.Add(index + 3);
                tris.Add(index + 2);
                tris.Add(index + 0);
            }
            else
            {
                tris.Add(index + 0);
                tris.Add(index + 1);
                tris.Add(index + 2);
                tris.Add(index + 2);
                tris.Add(index + 3);
                tris.Add(index + 0);
            }
        }

        public virtual void BuildBlockFace(int brick, Vector3 corner, Vector3 up, Vector3 right, bool reversed, List<Vector3> verts, List<Vector2> uvs, List<int> tris, bool isTopBottom = false)
        {
            int index = verts.Count;

            int textureSize = 64;
            float textureDensity = (1024f / textureSize);

            float actualBrickHeight = BrickHeight;

            //if (brick == (int)Block.Water)
            //    actualBrickHeight *= 0.9f;

            up.y -= (1 - actualBrickHeight);
            right.y -= (1 - actualBrickHeight);


            verts.Add(corner);
            verts.Add(corner + up);
            verts.Add(corner + up + right);
            verts.Add(corner + right);

            Vector2 uvWidth = new Vector2(textureSize / 1024f, textureSize / 1024f);
            Vector2 uvCorner = new Vector2(0, 1024f - (textureSize / 1024f));

            uvCorner.x += ((float)(brick - 1) / textureDensity);

            uvs.Add(uvCorner);
            uvs.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
            uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
            uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));

            if (reversed)
            {
                tris.Add(index + 0);
                tris.Add(index + 1);
                tris.Add(index + 2);
                tris.Add(index + 2);
                tris.Add(index + 3);
                tris.Add(index + 0);
            }
            else
            {
                tris.Add(index + 1);
                tris.Add(index + 0);
                tris.Add(index + 2);
                tris.Add(index + 3);
                tris.Add(index + 2);
                tris.Add(index + 0);
            }
        }

        public virtual bool IsTransparent(int x, int y, int z)
        {
            if (y < 0) return false;
            int brick = GetByte(x, y, z);
            switch (brick)
            {
                case (int)Block.Air:
                case (int)Block.WaveGrass:
                    return true;
                default:
                    return false;
            }
        }

        public virtual int GetByte(int x, int y, int z)
        {
            if ((y < 0) || (y >= Height))
                return 0;
            if ((x < 0) || (z < 0) || (x >= Width) || (z >= Width))
            {
                Vector3 worldPos = new Vector3(x, y, z) + ChunkPosition;
                Chunk chunk = Chunk.FindChunk(worldPos);
                if (chunk == this) return 0;
                if (chunk == null)
                    return ChunkTerrainGen.GetTheoreticalByte(worldPos);
                return chunk.GetByte(worldPos);
            }
            if (chunkData != null)
                return chunkData[x, y, z];
            return 0;
        }

        public virtual int GetByte(Vector3 worldPos)
        {
            worldPos -= ChunkPosition;
            int x = Mathf.FloorToInt(worldPos.x);
            int y = Mathf.FloorToInt(worldPos.y);
            int z = Mathf.FloorToInt(worldPos.z);
            return GetByte(x, y, z);
        }

        public static Chunk FindChunk(Vector3 pos)
        {
            for (int a = 0; a < chunks.Count; a++)
            {
                Vector3 cpos = -Vector3.one;
                while (cpos == -Vector3.one)
                    cpos = chunks[a].ChunkPosition;

                if ((pos.x < cpos.x) || (pos.z < cpos.z) || (pos.x >= cpos.x + Width) || (pos.z >= cpos.z + Width)) continue;
                return chunks[a];
            }
            return null;
        }

        public bool SetBrick(int brick, Vector3 worldPos)
        {
            worldPos -= ChunkPosition;
            return SetBrick(brick, Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y), Mathf.FloorToInt(worldPos.z));
        }

        public bool SetBrick(int brick, int x, int y, int z)
        {
            if ((x < 0) || (y < 0) || (z < 0) || (x >= Width) || (y >= Height) || (z >= Width))
                return false;
            if (chunkData[x, y, z] == brick) return false;
            chunkData[x, y, z] = brick;
            StartCoroutine(CreateVisualMeshAsync(true));

            if (x == 0)
            {
                Chunk chunk = FindChunk(new Vector3(x - 2, y, z) + ChunkPosition);
                if (chunk != null) StartCoroutine(chunk.CreateVisualMeshAsync(true));
                Debug.Log("Adjecent chunk updated as well: " + chunk);
            }
            if (x == Width - 1)
            {
                Chunk chunk = FindChunk(new Vector3(x + 2, y, z) + ChunkPosition);
                if (chunk != null) StartCoroutine(chunk.CreateVisualMeshAsync(true));
                Debug.Log("Adjecent chunk updated as well: " + chunk);
            }
            if (z == 0)
            {
                Chunk chunk = FindChunk(new Vector3(x, y, z - 2) + ChunkPosition);
                if (chunk != null) StartCoroutine(chunk.CreateVisualMeshAsync(true));
                Debug.Log("Adjecent chunk updated as well: " + chunk);
            }
            if (z == Width - 1)
            {
                Chunk chunk = FindChunk(new Vector3(x, y, z + 2) + ChunkPosition);
                if (chunk != null) StartCoroutine(chunk.CreateVisualMeshAsync(true));
                Debug.Log("Adjecent chunk updated as well: " + chunk);
            }

            return true;
        }
    }
}