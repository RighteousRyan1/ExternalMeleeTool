namespace ExternalMeleeTool.SlpParser;

using ExternalMeleeTool.Melee.Fighter;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// A specialized reader for extracting the raw event stream from a Slippi (.slp) replay file.
/// </summary>
public class SlpReader {
    // Full lead-up to the "raw" element's data, per the .slp spec:
    // {           -> UBJSON object-open marker (start of the top-level container)
    // U 3 r a w   -> optimized-format key: length-prefixed string "raw"
    // [ $ U # l   -> optimized array header: uint8 array, length given as an int32 ('l')
    // The previous version omitted the leading '{', which shifted every
    // subsequent byte by one and made the SequenceEqual check fail unconditionally.
    private static ReadOnlySpan<byte> ExpectedRawHeader => new byte[]
    {
        (byte)'{',
        (byte)'U', 3, (byte)'r', (byte)'a', (byte)'w',
        (byte)'[', (byte)'$', (byte)'U', (byte)'#', (byte)'l'
    };

    static SlpReader() {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public static SlpFile Parse(string filePath) {
        // Initialize the struct with arrays sized for the 4 potential controller ports
        SlpFile fileData = new SlpFile {
            FileName = Path.GetFileName(filePath),
            Username = new string[4],
            ConnectCode = new string[4],
            ELO = new float[4],
            Fighter = new FtKind[4]
        };

        using FileStream stream = File.OpenRead(filePath);

        // 1. Validate and bypass the UBJSON raw element header
        Span<byte> headerBuffer = stackalloc byte[11];
        stream.ReadExactly(headerBuffer);
        if (!headerBuffer.SequenceEqual(ExpectedRawHeader))
            throw new FormatException("Invalid .slp file.");

        // 2. Read the raw stream length
        Span<byte> lengthBuffer = stackalloc byte[4];
        stream.ReadExactly(lengthBuffer);
        int rawLength = BinaryPrimitives.ReadInt32BigEndian(lengthBuffer);

        // Note: if this file was captured while the game was still in progress,
        // the game itself writes 0 here (to be patched later once the length is
        // known), which means there's no reliable way to know how many raw bytes
        // follow just from this field. Guard against that case explicitly rather
        // than silently reading a zero-length/incomplete buffer.
        if (rawLength <= 0)
            throw new FormatException("Raw element length is 0 - file may be incomplete (in-progress recording) or corrupt.");

        byte[] rawGameData = new byte[rawLength];
        stream.ReadExactly(rawGameData);

        // 3. Locate the Game Start Event (0x36)
        // It immediately follows Event Payloads (0x35) which is the very first event.
        if (rawGameData[0] != 0x35) throw new FormatException("Missing Event Payloads (0x35).");

        int eventPayloadsSize = 1 + rawGameData[1];
        int gameStartOffset = eventPayloadsSize;

        if (rawGameData[gameStartOffset] != 0x36)
            throw new FormatException("Game Start event not found in expected location.");

        // 4. Extract data for each of the 4 ports
        for (int i = 0; i < 4; i++) {
            // Slippi Online Display Name offset: 0x1A5 + 0x1F * i
            // Field is 0x1F (31) bytes wide: up to 15 characters (each 1 or 2 bytes
            // in Shift-JIS) + a null terminator, so the search window is 30 bytes,
            // not 15 - using 15 here would truncate any name using full-width chars.
            int nameOffset = gameStartOffset + 0x1A5 + (0x1F * i);
            fileData.Username[i] = ReadShiftJisNullTerminatedString(rawGameData, nameOffset, 30);

            // Connect Code offset: 0x221 + 0xA * i
            // Field is 0xA (10) bytes wide: 7 half-width chars + 1 full-width '#' (2 bytes)
            // + null terminator, so the search window is 9 bytes.
            int codeOffset = gameStartOffset + 0x221 + (0xA * i);
            fileData.ConnectCode[i] = ReadShiftJisNullTerminatedString(rawGameData, codeOffset, 9);

            // External Character ID offset: Game Info Block (0x5) + 0x60 + 0x24 * i
            int fighterOffset = gameStartOffset + 0x5 + 0x60 + (0x24 * i);
            fileData.Fighter[i] = (FtKind)rawGameData[fighterOffset];
        }

        return fileData;
    }

    private static string ReadShiftJisNullTerminatedString(byte[] data, int startIndex, int maxLength) {
        int length = 0;

        // Find the null terminator within the maximum allowed bytes for this string
        while (length < maxLength && data[startIndex + length] != 0x00) {
            length++;
        }

        // Empty string if player slot is empty
        if (length == 0) return string.Empty;

        // Shift-JIS (Code Page 932) decoding for Japanese/English mixes
        Encoding shiftJis = Encoding.GetEncoding(932);
        return shiftJis.GetString(data, startIndex, length);
    }
}