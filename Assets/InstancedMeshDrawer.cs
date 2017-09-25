using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

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
    MaterialPropertyBlock _materialProps;
    ComputeBuffer _drawArgsBuffer;

    // Huge bounding box
    readonly Bounds kBounds = new Bounds(Vector3.zero, Vector3.one * 1000);

    // Command buffer used for rendering motion vectors
    CommandBuffer _motionVectorsPass;

    // Camera event used for rendering motion vectors
    const CameraEvent kMotionVectorsEvent = CameraEvent.BeforeImageEffectsOpaque;

    // Cameras that the motion vectors pass has been added to
    HashSet<Camera> _cameras = new HashSet<Camera>();

    #endregion

    #region MonoBehaviour methods

    void OnDisable()
    {
        // Remove the motion vector pass from the cameras.
        foreach (var camera in _cameras)
            camera.RemoveCommandBuffer(kMotionVectorsEvent, _motionVectorsPass);

        _cameras.Clear();

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
    }

    void OnRenderObject()
    {
        if (_motionVectorsPass != null)
        {
            if ((Camera.current.depthTextureMode & DepthTextureMode.MotionVectors) != 0)
                Graphics.ExecuteCommandBuffer(_motionVectorsPass);
        }
    }

    void Update()
    {
        DoLazyInitialization();

        _material.SetFloat("_CurrentTime", Application.isPlaying ? Time.time : 0);
        _material.SetFloat("_DeltaTime", Time.deltaTime);
        _material.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);
        _material.SetMatrix("_WorldToLocal", transform.worldToLocalMatrix);
        /*
        _materialProps.SetFloat("_CurrentTime", Application.isPlaying ? Time.time : 0);
        _materialProps.SetFloat("_DeltaTime", Time.deltaTime);
        _materialProps.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);
        _materialProps.SetMatrix("_WorldToLocal", transform.worldToLocalMatrix);
        */

        Graphics.DrawMeshInstancedIndirect(_mesh, 0, _material, kBounds, _drawArgsBuffer, 0, _materialProps);

        UpdateAllCameras();
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

        if (_materialProps == null)
            _materialProps = new MaterialPropertyBlock();

        if (_drawArgsBuffer == null)
        {
            _drawArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
            _drawArgsBuffer.SetData(new uint[5] { (uint)_mesh.GetIndexCount(0), 30, 0, 0, 0 });
        }

        if (_motionVectorsPass == null)
        {
            _motionVectorsPass = new CommandBuffer();
            _motionVectorsPass.name = "MotionVectors";
            _motionVectorsPass.SetRenderTarget(BuiltinRenderTextureType.MotionVectors);
            _motionVectorsPass.DrawMeshInstancedIndirect(_mesh, 0, _material, 0, _drawArgsBuffer, 0, _materialProps);
        }
    }

    void UpdateAllCameras()
    {
        foreach (var camera in Camera.allCameras)
        {
            if ((camera.depthTextureMode & DepthTextureMode.MotionVectors) != 0)
            {
                /*
                if (!_cameras.Contains(camera))
                {
                    camera.AddCommandBuffer(kMotionVectorsEvent, _motionVectorsPass);
                    _cameras.Add(camera);
                }
                */
            }
            else
            {
                if (_cameras.Contains(camera))
                {
                    camera.RemoveCommandBuffer(kMotionVectorsEvent, _motionVectorsPass);
                    _cameras.Remove(camera);
                }
            }
        }
    }

    #endregion
}
