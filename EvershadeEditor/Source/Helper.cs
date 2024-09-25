using System;

namespace EvershadeEditor.LM2 {
    public static class Helper {

        // Dictionary Related
        public const uint Identifier = 0xA9F32458;
        public const string Terminator = ".data\x00.debug\x00";
        public const byte MinDictionarySize = 58;

        // Chunk Related
        public const byte ChunkSize = 12;
        public const uint TexFileIdentifier = 0xE977D350;

        public const uint DDSIdentifier = 0x20534444;
        public const uint NVTTIdentifier = 0x5454564E;
        public const uint NVT3Identifier = 0x3354564E;
        public const uint DXT1Identifier = 0x31545844;
        public const uint DXT5Identifier = 0x35545844;

        public const ushort MinDimension = 4;
        public const ushort MaxDimension = 4096;

        public const byte DXT1MinSize = 136;      // DTX1 2x2
        public const uint DXT1MaxSize = 8388736;  // DTX1 4096x4096
        public const byte DXT5MinSize = 144;      // DTX5 2x2
        public const uint DXT5MaxSize = 16777344; // DTX5 4096x4096
    }

    public enum ChunkType : ushort {
        FileHeader = 0x1301,

        FileTable = 0x1,
        TextureBundles = 0x20,
        ModelBundles = 0x21,
        AnimationBundles = 0x1302,
        Room = 16,
        CutsceneNLB = 0x30,
        Config = 0x31,
        Video = 0x1200,
        AudioBanks = 0x3000,
        Effects = 0x4000,
        Script = 0x5000,
        GameObjectScriptTable = 0x6500,
        GameObject = 0x6510,
        MaterialEffects = 0xB300,
        Texture = 0xB500,
        Shaders = 0xB400,
        ShaderConstants = 0xB404,
        MaterialParams = 0xB310,
        MaterialShaders = 0xB320,
        Model = 0xB000,
        CollisionStatic = 0xC107,
        Hitboxes = 0xC300,
        HitboxRigged = 0xD000,
        ClothPhysics = 0xE000,
        AnimationData = 0x7000,
        Font = 0x7010,
        MessageData = 0x7020,
        Skeleton = 0x7100,
        VAND = 0x9501,

        // Sub data
        ShaderA = 0xB401,
        ShaderB = 0xB402,

        TextureHeader = 0xB501,
        TextureData = 0xB502,

        CollisionDataStart = 0xC100,
        CollisionHeader = 0xC101,
        CollisionSearch = 0xC102,
        CollisionSearchTriIndices = 0xC103,
        CollisionVertexPositions = 0xC110,
        CollisionTriIndices = 0xC111,
        CollisionTriNormals = 0xC112,
        CollisionTriNormalIndices = 0xC113,
        CollisionMaterialHashes = 0xC114,
        CollisionTriMaterialIndices = 0xC115,
        CollisionTriPropertyIndices = 0xC116,

        ModelTransform = 0xB001,
        ModelInfo = 0xB002,
        MeshInfo = 0xB003,
        VertexStartPointers = 0xB004,
        MeshBuffers = 0xB005,
        MaterialData = 0xB006,
        MaterialLookupTable = 0xB007,
        BoundingRadius = 0xB008,
        BoundingBox = 0xB009,
        MeshMorphInfos = 0xB00A,
        MeshMorphIndexBuffer = 0xB00B,
        ModelUnknownSection = 0xB00C,

        FontData = 0x7011,
        FontTextures = 0x7012,

        ShaderData = 0xB400,
        UILayoutStart = 0x7000,
        UILayoutHeader = 0x7001,
        UILayoutData = 0x7002,
        UILayout = 0x7003,

        HavokPhysics = 0xC900,
        PhysicData2 = 0xC901,
        HitboxObjects = 0xC301,
        HitboxObjectParams = 0xC302,

        HitboxRiggedHeader = 0xD001,
        HitboxRiggedData = 0xD002,

        SkinControllerStart = 0xB100,
        SkinBindingModelAssign = 0xB101,
        SkinMatrices = 0xB102,
        SkinHashes = 0xB103,

        ScriptHashBundle = 0x5011,
        ScriptData = 0x5012,
        ScriptHeader = 0x5013,
        ScriptFunctionTable = 0x5014,
        ScriptStringHashes = 0x5015,

        GameObjectDB = 0x6500,
        GameObjectDBScriptHashTable = 0x6501,
        GameObjectDBHashScriptIndexTable = 0x6502,
        GameObjectDBScriptHash = 0x6503,
        GameObjectScriptHash = 0x6511,
        GameObjectComponentOffsets = 0x6512,
        GameObjectComponentHashes = 0x6513,
        GameObjectComponentList = 0x6514,
        GameObjectParentHash = 0x6515,

        SkeletonHeader = 0x7101,
        SkeletonBoneInfo = 0x7102,
        SkeletonBoneTransform = 0x7103,
        SkeletonBoneIndexList = 0x7104,
        SkeletonBoneHashList = 0x7105,
        SkeletonBoneParenting = 0x7106,

        MaterialRasterizerConfig = 0xB321,
        MaterialDepthConfig = 0xB322,
        MaterialBlendConfig = 0xB323,

        MaterialShaderHeader = 0xB325,
        MaterialShaderName = 0xB326,
        MaterialParameterIndices = 0xB327,
        MaterialParameterOffsets = 0xB328,

        MaterialShaderAttrLocations = 0xB329,
        MaterialShaderAttrLocationOffsets = 0xB32A,

        MaterialShaderProgramLocations = 0xB32B,
        MaterialShaderProgramOffsets = 0xB32D,
        MaterialShaderUnknown = 0xB32E,

        MaterialVariation = 0xB330,
        ShaderProgramRenderParams = 0xB331,
        ShaderProgramHeader = 0xB332,
        ShaderProgramLocationOffsets = 0xB333,
        ShaderProgramLocIndices = 0xB334,
        ShaderProgramLocFlags = 0xB335,
        ShaderProgramHashes = 0xB337,
    }
}
