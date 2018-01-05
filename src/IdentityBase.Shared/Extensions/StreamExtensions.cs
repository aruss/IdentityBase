// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Extensions
{
    using System;
    using System.IO;
    using System.Text;

    internal static partial class StreamExtensions
    {
        internal static readonly Encoding DefaultEncoding =
            new UTF8Encoding(false, true);

        public static BinaryReader CreateReader(this Stream stream)
        {
            return new BinaryReader(stream, DefaultEncoding, true);
        }

        public static BinaryWriter CreateWriter(this Stream stream)
        {
            return new BinaryWriter(stream, DefaultEncoding, true);
        }

        public static DateTimeOffset ReadDateTimeOffset(
            this BinaryReader reader)
        {
            return new DateTimeOffset(
                reader.ReadInt64(),
                TimeSpan.Zero);
        }

        public static void Write(
            this BinaryWriter writer,
            DateTimeOffset value)
        {
            writer.Write(value.UtcTicks);
        }
    }
}
