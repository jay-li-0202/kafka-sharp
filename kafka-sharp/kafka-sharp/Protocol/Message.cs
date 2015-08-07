﻿// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System.IO;
using DamienG.Security.Cryptography;
using Kafka.Common;
using Kafka.Public;

namespace Kafka.Protocol
{
    struct Message
    {
        public byte[] Key;
        public byte[] Value;

        public void Serialize(MemoryStream stream, CompressionCodec compressionCodec)
        {
            var crcPos = stream.Position;
            stream.Write(Basics.MinusOne32, 0, 4); // crc placeholder
            var bodyPos = stream.Position;

            stream.WriteByte(0); // magic byte
            stream.WriteByte((byte)compressionCodec); // attributes
            if (Key == null)
            {
                stream.Write(Basics.MinusOne32, 0, 4);
            }
            else
            {
                BigEndianConverter.Write(stream, Key.Length);
                stream.Write(Key, 0, Key.Length);
            }

            if (Value == null)
                stream.Write(Basics.MinusOne32, 0, 4);
            else
            {
                BigEndianConverter.Write(stream, Value.Length);
                stream.Write(Value, 0, Value.Length);
            }

            // update crc
            var crc = Crc32.Compute(stream, bodyPos, stream.Position - bodyPos);
            var curPos = stream.Position;
            stream.Position = crcPos;
            BigEndianConverter.Write(stream, (int)crc);
            stream.Position = curPos;
        }
    }
}