using System.Security.Cryptography;
using System.Text;

namespace TestConsole;

public static class Deterministic
{
    /// <summary>
    /// Creates a value-based UUID using the algorithm from RFC 4122 §4.3.
    /// </summary>
    /// <param name="namespaceId">The ID of the namespace.</param>
    /// <param name="value">The value (within that namespace).</param>
    /// <returns>A UUID derived from the namespace and value.</returns>
    public static Ulid Create(Ulid namespaceId, string value)
    {
        if (namespaceId == Ulid.Empty)
            throw new ArgumentException("Namespace cannot be an empty GUID.", nameof(namespaceId));

        if (namespaceId == default)
            throw new ArgumentNullException(nameof(namespaceId), "Namespace cannot be null or empty.");

        if (string.IsNullOrEmpty(value))
            throw new ArgumentNullException(nameof(value), "Value cannot be null or empty.");

        // convert the name to a sequence of octets (as defined by the standard or conventions of its namespace) (step 3)
        // ASSUME: UTF-8 encoding is always appropriate
        var nameBytes = Encoding.UTF8.GetBytes(value);

        if (nameBytes.Length == 0)
            throw new ArgumentNullException(nameof(value));

        // convert the namespace UUID to network order (step 3)
        var namespaceBytes = namespaceId.ToByteArray();
        SwapByteOrder(namespaceBytes);

        // compute the hash of the name space ID concatenated with the name (step 4)
        var combinedBytes = new byte[namespaceBytes.Length + nameBytes.Length];
        Buffer.BlockCopy(namespaceBytes, 0, combinedBytes, 0, namespaceBytes.Length);
        Buffer.BlockCopy(nameBytes, 0, combinedBytes, namespaceBytes.Length, nameBytes.Length);

        var hash = SHA1.HashData(combinedBytes);

        // most bytes from the hash are copied straight to the bytes of the new GUID (steps 5-7, 9, 11-12)
        var newUlid = new byte[16];
        Array.Copy(hash, 0, newUlid, 0, 16);

        // set the four most significant bits (bits 12 through 15) of the time_hi_and_version
        // field to the appropriate 4-bit version number from Section 4.1.3 (step 8)
        newUlid[6] = (byte)((newUlid[6] & 0x0F) | (5 << 4));

        // set the two most significant bits (bits 6 and 7) of the clock_seq_hi_and_reserved
        // to zero and one, respectively (step 10)
        newUlid[8] = (byte)((newUlid[8] & 0x3F) | 0x80);

        // convert the resulting UUID to local byte order (step 13)
        SwapByteOrder(newUlid);

        FixTimeOverflow(newUlid);

        return new Ulid(newUlid);
    }

    private static void FixTimeOverflow(byte[] value)
    {
        var ticks = ByteArrayToTicks(value);

        if (ticks > DateTimeOffset.MaxValue.Ticks || ticks < DateTimeOffset.MinValue.Ticks)
        {
            ticks = Math.Abs(ticks) % DateTimeOffset.MaxValue.Ticks;
            var newTime = TicksToByteArray(ticks);

            value[0] = newTime[0];
            value[1] = newTime[1];
            value[2] = newTime[2];
            value[3] = newTime[3];
            value[4] = newTime[4];
            value[5] = newTime[5];
        }
    }

    private static long ByteArrayToTicks(byte[] value)
    {
        var milliseconds = BitConverter.ToInt64(
        [
            value[5],
            value[4],
            value[3],
            value[2],
            value[1],
            value[0],
            0,
            0
        ], 0);

        return milliseconds * 10000 + 621355968000000000L;
    }

    private static byte[] TicksToByteArray(long value)
    {
        var milliseconds = value / 10000 - 62135596800000L;
        byte[] bytes = BitConverter.GetBytes(milliseconds);
        return
        [
            bytes[5],
            bytes[4],
            bytes[3],
            bytes[2],
            bytes[1],
            bytes[0]
        ];
    }

    // Converts a GUID (expressed as a byte array) to/from network order (MSB-first).
    private static void SwapByteOrder(byte[] guid)
    {
        SwapBytes(guid, 0, 3);
        SwapBytes(guid, 1, 2);
        SwapBytes(guid, 4, 5);
        SwapBytes(guid, 6, 7);
    }

    private static void SwapBytes(byte[] guid, int left, int right)
    {
        (guid[right], guid[left]) = (guid[left], guid[right]);
    }
}
