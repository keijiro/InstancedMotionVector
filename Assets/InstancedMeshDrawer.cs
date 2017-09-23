using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class InstancedMeshDrawer : MonoBehaviour
{
    [SerializeField] Mesh _mesh;
    [SerializeField] Shader _shader;

    Bounds _bounds = new Bounds(Vector3.zero, Vector3.one * 1000);

    Material _material;
    ComputeBuffer _drawArgsBuffer;
    MaterialPropertyBlock _props;

    CommandBuffer _motionVectorPass;

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

    void OnDisable()
    {
        if (_drawArgsBuffer != null)
        {
            _drawArgsBuffer.Release();
            _drawArgsBuffer = null;
        }
    }

    void Update()
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

        if (_props == null) _props = new MaterialPropertyBlock();

        _props.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);
        _props.SetMatrix("_WorldToLocal", transform.worldToLocalMatrix);

        if (_motionVectorPass == null)
        {
            _motionVectorPass = new CommandBuffer();
            _motionVectorPass.name = "MotionVectors";

            _motionVectorPass.SetRenderTarget(BuiltinRenderTextureType.MotionVectors);
            _motionVectorPass.DrawMeshInstancedIndirect(_mesh, 0, _material, 0, _drawArgsBuffer, 0, _props);

            Camera.main.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, _motionVectorPass);
        }

        Graphics.DrawMeshInstancedIndirect(_mesh, 0, _material, _bounds, _drawArgsBuffer, 0, _props);
    }
}
