using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LedgerLibrary;
public class Ledger
{
    private byte[] CurrentHash { get; set; }

    public Ledger(byte[] StartingHash) => CurrentHash = StartingHash;
    public Ledger()
    {
        CurrentHash = new byte[64];
        Random.Shared.NextBytes(CurrentHash);
    }
    public async Task<LedgerRecord> AddBlankRecord()
    {
        LedgerRecord result;
        lock (this)
        {
            result = new LedgerRecord("EMPTY",
                "ADMIN",
                DateTime.Now,
                new byte[] { 0 },
                CurrentHash);
            CurrentHash = result.RecordHash;
        }
        return result;
    }
    public async Task<LedgerRecord> AddFileToLedger(string fileName, string owner, DateTime timestamp, byte[] fileHash)
    {
        LedgerRecord result;
        lock (this)
        {
            result = new LedgerRecord(fileName,
                owner,
                timestamp,
                fileHash,
                CurrentHash);
            CurrentHash = result.RecordHash;
        }
        return result;
    }
}

public readonly record struct LedgerRecord
{
    public readonly string FileName { get; init; }
    public readonly string Owner { get; init; }
    public readonly DateTime Timestamp { get; init; }
    public readonly byte[] FileHash { get; init; }
    public readonly byte[] PreviousHash { get; init; }

    public readonly byte[] RecordHash { get; init; }
    public LedgerRecord(string fileName, string owner, DateTime timestamp, byte[] fileHash, byte[] previousHash)
    {
        FileName = fileName;
        Owner = owner;
        Timestamp = timestamp;
        FileHash = fileHash;
        PreviousHash = previousHash;

        var fileNameBytes = Encoding.UTF8.GetBytes(fileName);
        var ownerBytes = Encoding.UTF8.GetBytes(owner);
        var timestampBytes = BitConverter.GetBytes(timestamp.ToBinary());

        var baseRecord = new byte[
            fileNameBytes.Length +
            ownerBytes.Length +
            timestampBytes.Length +
            fileHash.Length +
            previousHash.Length
            ];
        var position = 0;

        Array.Copy(fileNameBytes, 0, baseRecord, position, fileNameBytes.Length);
        position += fileName.Length;
        Array.Copy(ownerBytes, 0, baseRecord, position, ownerBytes.Length);
        position += ownerBytes.Length;
        Array.Copy(timestampBytes, 0, baseRecord, position, timestampBytes.Length);
        position += timestampBytes.Length;
        Array.Copy(fileHash, 0, baseRecord, position, fileHash.Length);
        position += fileHash.Length;
        Array.Copy(previousHash, 0, baseRecord, position, previousHash.Length);

        var hasher = SHA512.Create();
        RecordHash = hasher.ComputeHash(baseRecord);
    }
    public override string ToString() => JsonSerializer.Serialize(this) + "\n";
}
