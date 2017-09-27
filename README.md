InstancedMotionVector
=====================

![screenshot](https://i.imgur.com/QUcShdfm.png)
![screenshot](https://i.imgur.com/fpxRPrFm.png)

This is an example that shows how to support rendering motion vectors within
*indirect instanced drawing* of Unity.

(In this document, the term *"indirect instanced drawing"* refers to drawing
instanced geometries with using the indirect drawing API -- specifically using
[Graphics.DrawMeshInstancedIndirect].)

[Graphics.DrawMeshInstancedIndirect]: https://docs.unity3d.com/ScriptReference/Graphics.DrawMeshInstancedIndirect.html

System requirements
-------------------

- Unity 2017.2 or later

How to support motion vectors within indirect instanced drawing
---------------------------------------------------------------

- Write the `MotionVectors` light mode pass for the custom shader. It's
  recommended to use the first pass of the subshader to make it easy to refer
  from command buffers. ⇒ [Example][Example1]

- Implement the `OnRenderObject` method and manually invoke the `MotionVectors`
  pass with using a command buffer. [BuiltinRenderTextureType.MotionVectors]
  can be used to set the motion vectors texture as a render target. ⇒ 
  [Example][Example2]

[Example1]: https://github.com/keijiro/InstancedMotionVector/blob/master/Assets/InstancedMotionVector/InstancedMesh.shader#L13
[Example2]: https://github.com/keijiro/InstancedMotionVector/blob/master/Assets/InstancedMotionVector/InstancedMeshDrawer.cs#L116
[BuiltinRenderTextureType.MotionVectors]: https://docs.unity3d.com/ScriptReference/Rendering.BuiltinRenderTextureType.MotionVectors.html

License
-------

Copyright (c) 2017 Unity Technologies

This repository is to be treated as an example content of Unity; you can use
the code freely in your projects. Also see the [FAQ] about example contents.

[FAQ]: https://unity3d.com/unity/faq#faq-37863
