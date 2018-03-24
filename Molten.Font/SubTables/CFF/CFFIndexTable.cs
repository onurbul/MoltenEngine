﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{

    /// <summary>
    /// CFF index table. Contains an array of offsets and the offset to the next block of CFF data.
    /// </summary>
    public class CFFIndexTable : FontSubTable
    {
        internal ObjectEntry[] Objects;

        internal long OffsetToNextBlock;

        internal int Length => Objects.Length;

        /// <summary>
        /// Creates an empty Index table.
        /// </summary>
        internal CFFIndexTable(EnhancedBinaryReader reader, IFontTable parent) : base(reader, null, parent, 0)
        {
            Objects = new ObjectEntry[0];
        }

        internal CFFIndexTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset) :
            base(reader, log, parent, offset)
        {
            ushort count = reader.ReadUInt16();
            Objects = new ObjectEntry[count];

            // An empty INDEX is represented by a count field with a 0 value and no additional fields. Thus, the total size of an empty INDEX is 2 bytes.
            if (count == 0)
                return;

            byte offsetSize = reader.ReadByte();
            uint nextOffset = 0;
            uint curOffset = 0;
            uint offsetArraySize = offsetSize * (count + 1U);
            uint headerSize = GetLocalOffset(reader);
            uint objectDataOffset = headerSize + offsetArraySize;
            uint objectDataStreamOffset = objectDataOffset + (uint)Header.StreamOffset;

            switch (offsetSize)
            {
                case 1:
                    curOffset = objectDataStreamOffset + (reader.ReadByte() - 1U);
                    for (int i = 0; i < count; i++)
                    {
                        nextOffset = objectDataStreamOffset + (reader.ReadByte() - 1U);
                        Objects[i] = new ObjectEntry()
                        {
                            Offset = curOffset,
                            DataSize = nextOffset - curOffset,
                        };

                        curOffset = nextOffset;
                    }
                    break;

                case 2:
                    curOffset = objectDataStreamOffset + (reader.ReadUInt16() - 1U);
                    for (int i = 0; i < count; i++)
                    {
                        nextOffset = objectDataStreamOffset + (reader.ReadUInt16() - 1U);
                        Objects[i] = new ObjectEntry()
                        {
                            Offset = curOffset,
                            DataSize = nextOffset - curOffset,
                        };

                        curOffset = nextOffset;
                    }
                    break;

                case 4:
                    curOffset = objectDataStreamOffset + (reader.ReadUInt32() - 1U);
                    for (int i = 0; i < count; i++)
                    {
                        nextOffset = objectDataStreamOffset + (reader.ReadUInt32() - 1U);
                        Objects[i] = new ObjectEntry()
                        {
                            Offset = curOffset,
                            DataSize = nextOffset - curOffset,
                        };

                        curOffset = nextOffset;
                    }
                    break;
            }

            OffsetToNextBlock = nextOffset;
        }

        internal class ObjectEntry
        {
            public uint Offset;

            public uint DataSize;
        }
    }
}
