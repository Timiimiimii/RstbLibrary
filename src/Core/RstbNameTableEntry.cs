using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

namespace RstbLibrary.Core;

[StructLayout(LayoutKind.Sequential, Size = 164)]
public ref struct RstbNameTableEntry
{
    public ReadOnlySpan<byte> name;
    public uint size;

    public static unsafe void Write(string name, uint size, Span<byte> data, int offset, int blockSize, Endianness endian)
    {
        Span<byte> sub = data[offset..];
        ReadOnlySpan<byte> nameData = Encoding.UTF8.GetBytes(name);
        for (int i = 0; i < (nameData.Length <= blockSize ? nameData.Length : blockSize); i++) {
            sub[i] = nameData[i];
        }

        if (endian == Endianness.Big) {
            BinaryPrimitives.WriteUInt32BigEndian(sub[blockSize..(blockSize + 4)], size);
        }
        else {
            BinaryPrimitives.WriteUInt32LittleEndian(sub[blockSize..(blockSize + 4)], size);
        }
    }

    public unsafe string? GetManagedName()
    {
        fixed (byte* ptr = name) {
            return Utf8StringMarshaller.ConvertToManaged(ptr);
        }
    }

    public RstbNameTableEntry(ReadOnlySpan<byte> data, int offset, int blockSize, Endianness endian)
    {
        ReadOnlySpan<byte> sub = data[offset..];
        name = sub[0..blockSize];
        size = endian == Endianness.Big
            ? BinaryPrimitives.ReadUInt32BigEndian(sub[blockSize..(blockSize + 4)])
            : BinaryPrimitives.ReadUInt32LittleEndian(sub[blockSize..(blockSize + 4)]);
    }
}
