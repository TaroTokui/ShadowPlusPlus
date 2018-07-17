using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

//[ExecuteInEditMode, RequireComponent(typeof(Renderer))]

public class CubeInstancing : MonoBehaviour
{

    // ==============================
    #region // Defines

    const int ThreadBlockSize = 256;

    struct CubeData
    {
        //public Vector3 BasePosition;
        public Vector3 Position;
        public Vector3 Velocity;
        public Vector3 Rotation;
        public Vector3 Albedo;
        public bool isActive;
    }

    #endregion // Defines

    // ==============================
    #region // Serialize Fields

    // cubeの数
    [SerializeField]
    int _instanceCountX = 100;
    [SerializeField]
    int _instanceCountY = 100;

    [SerializeField]
    ComputeShader _ComputeShader;
    
    [SerializeField]
    //LayoutType _LayoutType = LayoutType.LayoutFlat;
    int _LayoutType = 0;
    int _PrevLayoutType = 0;

    // instancingするmesh
    [SerializeField]
    Mesh _CubeMesh;

    [SerializeField]
    Material _CubeMaterial;

    [SerializeField]
    Vector3 _CubeMeshScale = new Vector3(1f, 1f, 1f);

    [SerializeField]
    Vector3 _CubeMeshStep = new Vector3(1f, 1f, 1f);
    
    // compute shaderに渡すtexture
    [SerializeField]
    RenderTexture _NoiseTexture;

    /// 表示領域の中心座標
    [SerializeField]
    Vector3 _BoundCenter = Vector3.zero;

    /// 表示領域のサイズ
    [SerializeField]
    Vector3 _BoundSize = new Vector3(300f, 300f, 300f);

    /// アニメーションの位相
    [Range(-Mathf.PI, Mathf.PI)]
    [SerializeField]
    float _Phi = Mathf.PI;

    /// アニメーションの周期
    [Range(0.01f, 100)]
    [SerializeField]
    float _Lambda = 1;

    /// アニメーションの大きさ
    [SerializeField]
    float _Amplitude = 1;
    
    /// アニメーションの速さ
    [SerializeField]
    [Range(0, 10)]
    float _Speed = 1;


    [SerializeField]
    bool useOsc = true;
    
    [SerializeField]
    float tmpPuls = 0.0f;

    /// 音声入力
    float _InputLow = 0.0f;
    float _InputMid = 0.0f;
    float _InputHigh = 0.0f;
    float _InputKickdetection = 0.0f;
    float _InputSnaredetection = 0.0f;
    float _InputRythm = 0.0f;
    float _InputSpectralcentroid = 0.0f;
    float _InputFmp = 0.0f;
    float _InputSmp = 0.0f;

    #endregion // Serialize Fields

    // ==============================
    #region // Private Fields

    ComputeBuffer _CubeDataBuffer;
    ComputeBuffer _BaseCubeDataBuffer;
    ComputeBuffer _PrevCubeDataBuffer;

    /// GPU Instancingの為の引数
    uint[] _GPUInstancingArgs = new uint[5] { 0, 0, 0, 0, 0 };

    /// GPU Instancingの為の引数バッファ
    ComputeBuffer _GPUInstancingArgsBuffer;

    // instanceの合計数
    int _instanceCount;

    private GameObject noisePlane;

    private Texture2D m_texture;

    #endregion // Private Fields

    // --------------------------------------------------
    #region // MonoBehaviour Methods
        
    void Start()
    {
        ResetCubes();
    }

    private void ResetCubes()
    {
        _instanceCount = _instanceCountX * _instanceCountY;

        // allocate buffers
        _CubeDataBuffer = new ComputeBuffer(_instanceCount, Marshal.SizeOf(typeof(CubeData)));
        _BaseCubeDataBuffer = new ComputeBuffer(_instanceCount, Marshal.SizeOf(typeof(CubeData)));
        _PrevCubeDataBuffer = new ComputeBuffer(_instanceCount, Marshal.SizeOf(typeof(CubeData)));
        _GPUInstancingArgsBuffer = new ComputeBuffer(1, _GPUInstancingArgs.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        
        // init cube position
        int kernelId = _ComputeShader.FindKernel("Init");
        _ComputeShader.SetInt("_Width", _instanceCountX);
        _ComputeShader.SetInt("_Height", _instanceCountY);
        _ComputeShader.SetFloat("_Width2", Mathf.Floor(_instanceCountX / 2.0f));
        _ComputeShader.SetFloat("_Height2", Mathf.Floor(_instanceCountY / 2.0f));
        _ComputeShader.SetBuffer(kernelId, "_CubeDataBuffer", _CubeDataBuffer);
        _ComputeShader.SetBuffer(kernelId, "_BaseCubeDataBuffer", _BaseCubeDataBuffer);
        _ComputeShader.SetBuffer(kernelId, "_PrevCubeDataBuffer", _PrevCubeDataBuffer);
        _ComputeShader.Dispatch(kernelId, (Mathf.CeilToInt(_instanceCount / ThreadBlockSize) + 1), 1, 1);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            ResetCubes();
        }
        
        int kernelId;
        
        // ComputeShader
        kernelId = _ComputeShader.FindKernel("Update");
        _ComputeShader.SetFloat("_Time", Time.time / 5.0f * _Speed);
        _ComputeShader.SetInt("_Width", _instanceCountX);
        _ComputeShader.SetInt("_Height", _instanceCountY);
        _ComputeShader.SetFloat("_Phi", _Phi);
        _ComputeShader.SetFloat("_Lambda", _Lambda);
        _ComputeShader.SetFloat("_Amplitude", _Amplitude);
        _ComputeShader.SetFloat("_StepX", _CubeMeshStep.x);
        _ComputeShader.SetFloat("_StepY", _CubeMeshStep.y);
        _ComputeShader.SetFloat("_StepZ", _CubeMeshStep.z);
        _ComputeShader.SetFloat("_InputLow", _InputLow);
        _ComputeShader.SetFloat("_InputMid", _InputMid);
        _ComputeShader.SetFloat("_InputHigh", _InputHigh);
        _ComputeShader.SetFloat("_InputKick", _InputKickdetection);
        _ComputeShader.SetFloat("_InputSnare", _InputSnaredetection);
        _ComputeShader.SetFloat("_InputRythm", _InputRythm);
        _ComputeShader.SetBuffer(kernelId, "_CubeDataBuffer", _CubeDataBuffer);
        _ComputeShader.SetBuffer(kernelId, "_BaseCubeDataBuffer", _BaseCubeDataBuffer);
        _ComputeShader.SetBuffer(kernelId, "_PrevCubeDataBuffer", _PrevCubeDataBuffer);
        _ComputeShader.SetTexture(kernelId, "_NoiseTex", _NoiseTexture);

        _ComputeShader.Dispatch(kernelId, (Mathf.CeilToInt(_instanceCount / ThreadBlockSize) + 1), 1, 1);
        
        // GPU Instaicing
        _GPUInstancingArgs[0] = (_CubeMesh != null) ? _CubeMesh.GetIndexCount(0) : 0;
        _GPUInstancingArgs[1] = (uint)_instanceCount;
        _GPUInstancingArgsBuffer.SetData(_GPUInstancingArgs);
        _CubeMaterial.SetBuffer("_CubeDataBuffer", _CubeDataBuffer);
        _CubeMaterial.SetVector("_CubeMeshScale", _CubeMeshScale);
        Graphics.DrawMeshInstancedIndirect(_CubeMesh, 0, _CubeMaterial, new Bounds(_BoundCenter, _BoundSize), _GPUInstancingArgsBuffer);
    }

    void OnDestroy()
    {
        if (this._CubeDataBuffer != null)
        {
            this._CubeDataBuffer.Release();
            this._CubeDataBuffer = null;
        }
        if (this._BaseCubeDataBuffer != null)
        {
            this._BaseCubeDataBuffer.Release();
            this._BaseCubeDataBuffer = null;
        }
        if (this._PrevCubeDataBuffer != null)
        {
            this._PrevCubeDataBuffer.Release();
            this._PrevCubeDataBuffer = null;
        }
        if (this._GPUInstancingArgsBuffer != null)
        {
            this._GPUInstancingArgsBuffer.Release();
            this._GPUInstancingArgsBuffer = null;
        }
    }

    #endregion // MonoBehaviour Method
}