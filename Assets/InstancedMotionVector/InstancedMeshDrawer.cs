using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class InstancedMeshDrawer : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] Mesh _mesh;
    [SerializeField] Texture2D _texture;
    [SerializeField] int _instanceCount = 100;

    #endregion

    #region Internal resources

    [SerializeField, HideInInspector] Shader _shader;

    #endregion

    #region Internal objects

    // Material and property sheet for the shader
    Material _material;
    MaterialPropertyBlock _shaderSheet;

    // Indirect draw arguments
    uint [] _drawArgs = new uint [5];
    ComputeBuffer _drawArgsBuffer;

    // Command buffer used for rendering motion vectors
    CommandBuffer _motionVectorsPass;

    // Local-to-world matrix history
    Matrix4x4 _previousLocalToWorld;

    // Huge bounding box
    readonly Bounds kBounds = new Bounds(Vector3.zero, Vector3.one * 10000);

    #endregion

    #region MonoBehaviour methods

    void OnValidate()
    {
        _instanceCount = Mathf.Clamp(_instanceCount, 1, 65535);
    }

    void OnDisable()
    {
        // We have to release compute buffers not in OnDestory but in
        // OnDisable to avoid buffer leakage warning.
        if (_drawArgsBuffer != null)
        {
            _drawArgsBuffer.Release();
            _drawArgsBuffer = null;
        }
    }

    void OnDestroy()
    {
        if (_material != null)
        {
            if (Application.isPlaying)
                Destroy(_material);
            else
                DestroyImmediate(_material);
        }

        if (_motionVectorsPass != null)
            _motionVectorsPass.Dispose();
    }

    void Start()
    {
        _previousLocalToWorld = transform.localToWorldMatrix;
    }

    void Update()
    {
        if (_mesh == null) return;

        DoLazyInitialization();

        // Update the shader property sheet.
        _shaderSheet.Clear();
        _shaderSheet.SetTexture("_MainTex", _texture);

        if (Application.isPlaying)
        {
            _shaderSheet.SetFloat("_CurrentTime", Time.time);
            _shaderSheet.SetFloat("_DeltaTime", Time.deltaTime);
        }
        else
        {
            _shaderSheet.SetFloat("_CurrentTime", 0);
            _shaderSheet.SetFloat("_DeltaTime", 1.0f / 60);
        }

        _shaderSheet.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);
        _shaderSheet.SetMatrix("_WorldToLocal", transform.worldToLocalMatrix);
        _shaderSheet.SetMatrix("_PreviousM", _previousLocalToWorld);

        // Update the indirect draw arguments.
        _drawArgs[0] = _mesh.GetIndexCount(0);
        _drawArgs[1] = (uint)_instanceCount;
        _drawArgsBuffer.SetData(_drawArgs);

        // Instanced indirect draw call
        Graphics.DrawMeshInstancedIndirect(_mesh, 0, _material, kBounds, _drawArgsBuffer, 0, _shaderSheet);

        // Update the internal state.
        _previousLocalToWorld = transform.localToWorldMatrix;
    }

    void OnRenderObject()
    {
        if (_mesh == null) return;

        var camera = Camera.current;

        if (_motionVectorsPass != null &&
            (camera.depthTextureMode & DepthTextureMode.MotionVectors) != 0)
        {
            var nonJitteredVP = GL.GetGPUProjectionMatrix(camera.projectionMatrix, true) * camera.worldToCameraMatrix;

            // Set the per-camera properties.
            _shaderSheet.SetMatrix("_PreviousVP", camera.previousViewProjectionMatrix);
            _shaderSheet.SetMatrix("_NonJitteredVP", nonJitteredVP);

            // Build and execute the motion vector rendering pass.
            _motionVectorsPass.Clear();
            if (camera.allowMSAA && camera.actualRenderingPath == RenderingPath.Forward)
                _motionVectorsPass.SetRenderTarget(BuiltinRenderTextureType.MotionVectors);
            else
                _motionVectorsPass.SetRenderTarget(BuiltinRenderTextureType.MotionVectors, BuiltinRenderTextureType.CameraTarget);
            _motionVectorsPass.DrawMeshInstancedIndirect(_mesh, 0, _material, 0, _drawArgsBuffer, 0, _shaderSheet);
            Graphics.ExecuteCommandBuffer(_motionVectorsPass);
        }
    }

    #endregion

    #region Internal methods

    void DoLazyInitialization()
    {
        if (_material == null)
        {
            _material = new Material(_shader);
            _material.hideFlags = HideFlags.DontSave;
        }

        if (_shaderSheet == null)
            _shaderSheet = new MaterialPropertyBlock();

        if (_drawArgsBuffer == null)
            _drawArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);

        if (_motionVectorsPass == null)
        {
            _motionVectorsPass = new CommandBuffer();
            _motionVectorsPass.name = "Motion Vectors";
        }
    }

    #endregion
}
