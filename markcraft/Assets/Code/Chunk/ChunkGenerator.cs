using Markcraft;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Markcraft{
    public abstract class ChunkGenerator
    {
        protected Chunk chunk;

        public ChunkGenerator(Chunk chunk){
            this.chunk = chunk;
        }

        public abstract void Generate();
    }
}