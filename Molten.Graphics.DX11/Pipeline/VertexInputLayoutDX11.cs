﻿using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11
{
    /// <summary>A helper class that safely wraps InputLayout.</summary>
    internal unsafe class VertexInputLayoutDX11 : GraphicsObject
    {
        ID3D11InputLayout* _native;
        ulong[] _expectedFormatIDs;

        internal VertexInputLayoutDX11(DeviceDX11 device,
            GraphicsStateValueGroup<GraphicsBuffer> vbSlots, 
            ID3D10Blob* vertexBytecode,
            ShaderIOLayout io) : 
            base(device)
        {
            IsValid = true;
            _expectedFormatIDs = new ulong[vbSlots.Length];
            List<InputElementDesc> elements = new List<InputElementDesc>();
            VertexFormat format = null;

            // Store the EOID of each expected vertext format.
            for (int i = 0; i < vbSlots.Length; i++)
            {
                if (vbSlots.BoundValues[i] == null)
                    continue;

                format = vbSlots.BoundValues[i].VertexFormat;

                /* Check if the current vertex segment's format matches 
                   the part of the shader's input structure that it's meant to represent. */
                int startID = elements.Count;
                if (!io.IsCompatible(format.Structure, startID))
                {
                    IsValid = false;
                    break;
                }

                // Collate vertex format elements into layout and set the correct input slot for each element.
                elements.AddRange((format.Structure as ShaderIOLayoutDX11).VertexElements);

                for (int eID = startID; eID < elements.Count; eID++)
                {
                    InputElementDesc e = elements[eID];
                    e.InputSlot = (uint)i; // Vertex buffer input slot.
                    elements[eID] = e;

                    IsInstanced = IsInstanced || e.InputSlotClass == InputClassification.PerInstanceData;
                }

                _expectedFormatIDs[i] = format.EOID;
            }

            // Check if there are actually any elements. If not, use the default placeholder vertex type.
            if (elements.Count == 0)
            {
                VertexFormat nullFormat = device.VertexCache.Get<VertexWithID>();
                elements.Add((nullFormat.Structure as ShaderIOLayoutDX11).VertexElements[0]);
                IsNullBuffer = true;
            }

            InputElementDesc[] finalElements = elements.ToArray();

            // Attempt creation of input layout.
            if (IsValid)
            {
                void* ptrByteCode = vertexBytecode->GetBufferPointer();
                nuint numBytes = vertexBytecode->GetBufferSize();

                fixed(InputElementDesc* ptrElements = &finalElements[0])
                    device.Ptr->CreateInputLayout(ptrElements, (uint)finalElements.Length, ptrByteCode, numBytes, ref _native);

                if(_native == null)
                {
                    device.Log.Error("Failed to create new vertex input layout");
                    device.ProcessDebugLayerMessages();
                }
            }
            else
            {
                device.Log.Warning($"Vertex formats do not match the input layout of shader:");
                for (int i = 0; i < vbSlots.Length; i++)
                {
                    if (vbSlots.BoundValues[i] == null)
                        continue;

                    format = vbSlots.BoundValues[i].VertexFormat;

                    device.Log.Warning("Format - Buffer slot " + i + ": ");
                    for (int f = 0; f < format.Structure.Metadata.Length; f++)
                        device.Log.Warning($"\t[{f}]{format.Structure.Metadata[f].Name} -- index: {format.Structure.Metadata[f].SemanticIndex} -- slot: {i}");
                }

                // List final input structure.
                device.Log.Warning("Shader Input Structure: ");
                for (int i = 0; i < finalElements.Length; i++)
                    device.Log.Warning($"\t[{i}]{format.Structure.Metadata[i].Name} -- index: {finalElements[i].SemanticIndex} -- slot: {finalElements[i].InputSlot}");
            }
        }

        public bool IsMatch(Logger log, GraphicsStateValueGroup<GraphicsBuffer> grp)
        {
            for (int i = 0; i < grp.Length; i++)
            {
                BufferDX11 seg = grp.BoundValues[i] as BufferDX11;

                // If null vertex buffer, check if shader actually need one to be present.
                if (seg == null)
                {
                    // if shader's buffer hash is null for this slot, it's allowed to be null, otherwise no match.
                    if (_expectedFormatIDs[i] == 0)
                        continue;
                    else
                        return false;
                }

                // Prevent vertex buffer segments with no format from crashing the application.
                if (seg.VertexFormat == null)
                {
                    log.Warning($"Missing format for bound vertex segment {seg.Name} in slot {i}. Skipping validation. This may cause a false input layout match.");
                    continue;
                }

                if (seg.VertexFormat.EOID != _expectedFormatIDs[i])
                    return false;
            }

            return true;
        }

        protected override void OnGraphicsRelease()
        {
            SilkUtil.ReleasePtr(ref _native);
        }

        public static implicit operator ID3D11InputLayout*(VertexInputLayoutDX11 resource)
        {
            return resource.NativePtr;
        }

        /// <summary>Gets whether or not the input layout is valid.</summary>
        internal bool IsValid { get; }

        /// <summary>Gets whether or not the vertex input layout is designed for use with instanced draw calls.</summary>
        public bool IsInstanced { get; }

        /// <summary>
        /// Gets whether the current <see cref="VertexInputLayoutDX11"/> can represent a null vertex buffer. e.g. Contains SV_VertexID as the only vertex element.
        /// </summary>
        public bool IsNullBuffer { get; }

        internal ref ID3D11InputLayout* NativePtr => ref _native;
    }
}
