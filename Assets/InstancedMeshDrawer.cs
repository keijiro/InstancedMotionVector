using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class InstancedMeshDrawer : MonoBehaviour
{
    #region Internal resources

    [SerializeField, HideInInspector] Mesh _mesh;
    [SerializeField, HideInInspector] Shader _shader;

    #endregion

    #region Internal objects

    // Temporary objects for instanced rendering
    Material _material;
    ComputeBuffer _drawArgsBuffer;

    // Command buffer used for rendering motion vectors
    CommandBuffer _motionVectorsPass;

    // Huge bounding box
    readonly Bounds kBounds = new Bounds(Vector3.zero, Vector3.one * 10000);

    #endregion

    #region MonoBehaviour methods

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

    void Update()
    {
        DoLazyInitialization();

        _material.SetFloat("_CurrentTime", Application.isPlaying ? Time.time : 0);
        _material.SetFloat("_DeltaTime", Time.deltaTime);
        _material.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);
        _material.SetMatrix("_WorldToLocal", transform.worldToLocalMatrix);

        Graphics.DrawMeshInstancedIndirect(_mesh, 0, _material, kBounds, _drawArgsBuffer);
    }

    void OnRenderObject()
    {
        if (_motionVectorsPass != null &&
            (Camera.current.depthTextureMode & DepthTextureMode.MotionVectors) != 0)
        {
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

        if (_drawArgsBuffer == null)
        {
            _drawArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
            _drawArgsBuffer.SetData(new uint[5] { (uint)_mesh.GetIndexCount(0), 30, 0, 0, 0 });
        }

        if (_motionVectorsPass == null)
        {
            _motionVectorsPass = new CommandBuffer();
            _motionVectorsPass.name = "Motion Vectors";
            _motionVectorsPass.SetRenderTarget(BuiltinRenderTextureType.MotionVectors);
            _motionVectorsPass.DrawMeshInstancedIndirect(_mesh, 0, _material, 0, _drawArgsBuffer);
        }
    }

    #endregion
}
