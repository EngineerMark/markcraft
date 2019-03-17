using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimplexNoise;
using System.Threading;
using System;

namespace Markcraft
{
    public enum Block
    {
        Air = 0,
        Stone = 1,
        Dirt = 2,
        Bedrock = 3,
        Grass = 4,
        Water = 5,
        Wood = 6
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

        [SerializeField] private int[,,] chunkData;
        public Mesh visualMesh;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private MeshCollider meshCollider;
        [SerializeField] private MeshFilter meshFilter;

        private Thread generationThread;
        private bool generatorReady = false;
        private static System.Random random;
        private bool fullyComplete = false;


        List<Vector3> vertices;
        List<Vector2> uvs;
        List<int> tris;

        private void Start()
        {
            chunks.Add(this);

            meshRenderer = GetComponent<MeshRenderer>();
            meshCollider = GetComponent<MeshCollider>();
            meshFilter = GetComponent<MeshFilter>();

            ChunkPosition = transform.position;

            visualMesh = new Mesh();
            NewChunk();

            gameObject.name = "Chunk " + transform.position;
        }

        private void NewChunk(){
            if (random == null)
                random = new System.Random(System.DateTime.Now.Millisecond);
            UnityEngine.Random.seed = random.Next();

            generationThread = new Thread(ThreadSystem);
            ChunkManager.self.Add(generationThread);
        }

        private void LoadSavedChunk(int[,,] preloadedBlocks){
            chunkData = (int[,,])preloadedBlocks.Clone();
            StartCoroutine(CreateVisualMeshAsync(true));
            fullyComplete = true;
        }

        public static void UnloadChunk(Chunk c){
            string filename = string.Format("chunk.{0}.dat",c.gameObject.name);
            string path = Application.dataPath;
            Debug.Log(path);
        }

        public void Update()
        {
            if (!generatorReady) return;
            if (fullyComplete) return;
            ChunkManager.self.Continue(generationThread);
            StartCoroutine(CreateVisualMeshAsync());
            fullyComplete = true;
        }

        public void ThreadSystem()
        {
            CalculateMapFromScratch();
            generatorReady = true;
        }

        public static byte GetTheoreticalByte(Vector3 pos)
        {
            Vector3 grain0Offset = new Vector3((float)random.NextDouble() * 10000, (float)random.NextDouble() * 10000, (float)random.NextDouble() * 10000);
            Vector3 grain1Offset = new Vector3((float)random.NextDouble() * 10000, (float)random.NextDouble() * 10000, (float)random.NextDouble() * 10000);
            Vector3 grain2Offset = new Vector3((float)random.NextDouble() * 10000, (float)random.NextDouble() * 10000, (float)random.NextDouble() * 10000);
            return GetTheoreticalByte(pos, grain0Offset, grain1Offset, grain2Offset);
        }

        public static byte GetTheoreticalByte(Vector3 pos, Vector3 offset0, Vector3 offset1, Vector3 offset2)
        {
            IntVector3 newPos = new IntVector3((int)(pos.x + (int.MaxValue) * 0.5f), 0, (int)(pos.z + (int.MaxValue) * 0.5f));
            //heightmap += Noise.Generate(newPos.x * 0.07f, newPos.y * 0.07f) * 20;
            Block brick = Block.Air;

            float stone = NoiseWrapper.PerlinNoise(newPos.x, newPos.y, newPos.z, 10, 3, 1.2f);
            stone += NoiseWrapper.PerlinNoise(newPos.x, newPos.y, newPos.z, 20, 4, 0);
            stone += 10;

            float dirt = NoiseWrapper.PerlinNoise(newPos.x, newPos.y, newPos.z, 50, 2, 0) + 1;

            float cave = SimplexNoise.Noise.Generate(newPos.x / 32f, newPos.z / 32f);

            if (pos.y < stone)
                brick = Block.Stone;
            else if (pos.y < dirt + stone)
                brick = Block.Grass;
            else if (cave < 0.1f && brick != Block.Air)
                brick = Block.Air;
            else if (pos.y < 15 && brick == Block.Air)
                brick = Block.Water;

            if (pos.y == 0)
                brick = Block.Bedrock;

            return (byte)brick;
        }

        public virtual void GenerateMap()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Width; z++)
                    {
                        chunkData[x, y, z] = GetTheoreticalByte(new Vector3(x, y, z) + ChunkPosition);
                    }
                }
            }
        }

        public virtual void CalculateMapFromScratch()
        {
            chunkData = new int[Width, Height, Width];

            Vector3 grain0Offset = new Vector3((float)random.NextDouble() * 10000, (float)random.NextDouble() * 10000, (float)random.NextDouble() * 10000);
            Vector3 grain1Offset = new Vector3((float)random.NextDouble() * 10000, (float)random.NextDouble() * 10000, (float)random.NextDouble() * 10000);
            Vector3 grain2Offset = new Vector3((float)random.NextDouble() * 10000, (float)random.NextDouble() * 10000, (float)random.NextDouble() * 10000);
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
                        if (IsTransparent(x - 1, y, z)) BuildFace(brick, new Vector3(x, y, z), Vector3.up, Vector3.forward, false, vertices, uvs, tris);
                        if (IsTransparent(x + 1, y, z)) BuildFace(brick, new Vector3(x + 1, y, z), Vector3.up, Vector3.forward, true, vertices, uvs, tris);

                        if (IsTransparent(x, y - 1, z)) BuildFace(brick, new Vector3(x, y, z), Vector3.forward, Vector3.right, false, vertices, uvs, tris);
                        if (IsTransparent(x, y + 1, z)) BuildFace(brick, new Vector3(x, y + 1, z), Vector3.forward, Vector3.right, true, vertices, uvs, tris, true);

                        if (IsTransparent(x, y, z - 1)) BuildFace(brick, new Vector3(x, y, z), Vector3.up, Vector3.right, true, vertices, uvs, tris);
                        if (IsTransparent(x, y, z + 1)) BuildFace(brick, new Vector3(x, y, z + 1), Vector3.up, Vector3.right, false, vertices, uvs, tris);
                    }
        }

        public void ApplyChanges()
        {
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

        public virtual void BuildFace(int brick, Vector3 corner, Vector3 up, Vector3 right, bool reversed, List<Vector3> verts, List<Vector2> uvs, List<int> tris, bool isTopBottom = false)
        {
            int index = verts.Count;

            int textureSize = 64;
            float textureDensity = (1024f / textureSize);

            float actualBrickHeight = BrickHeight;

            if (brick == (int)Block.Water)
                actualBrickHeight *= 0.1f;

            corner.y *= actualBrickHeight;
            up.y *= actualBrickHeight;
            right.y *= actualBrickHeight;


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
                case (int)Block.Water:
                case (int)Block.Air:
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
                    return GetTheoreticalByte(worldPos);
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