﻿using Molten.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    /// <summary>A shader matrix variable.</summary>
    internal class ScalarFloat4x4Variable : ShaderConstantVariable
    {
        Matrix4F _value;

        public ScalarFloat4x4Variable(ShaderConstantBuffer parent)
            : base(parent)
        {
            SizeOf = Matrix4F.SizeInBytes;
        }

        public override void Dispose() { }

        internal override void Write(RawStream stream)
        {
            stream.Write(ref _value);
        }

        public override object Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = (Matrix4F)value;
                _value.Transpose();
                DirtyParent();
            }
        }
    }
}
