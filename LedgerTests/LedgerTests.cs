using NUnit.Framework;
using LedgerLibrary;
using System.IO;
using System.Security.Cryptography;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace LedgerTests;

public class LedgerTests
{
    byte[] Seed;
    byte[] ChurchHash;
    byte[] IcelandHash;
    byte[] MountainHash;
    byte[] RoadHash;
    string WrittenLedger;

    [SetUp]
    public async Task SetupAsync()
    {
        Seed = new byte[]
        {
            1, 2, 3, 4, 5, 6, 7 , 8,
            1, 2, 3, 4, 5, 6, 7 , 8,
            1, 2, 3, 4, 5, 6, 7 , 8,
            1, 2, 3, 4, 5, 6, 7 , 8,
            1, 2, 3, 4, 5, 6, 7 , 8,
            1, 2, 3, 4, 5, 6, 7 , 8,
            1, 2, 3, 4, 5, 6, 7 , 8,
            1, 2, 3, 4, 5, 6, 7 , 8,
        };
        var hasher = SHA256.Create();
        using var churchStream = File.OpenRead("./TestingFiles/church.jpg");
        ChurchHash = hasher.ComputeHash(churchStream);
        using var icelandStream = File.OpenRead("./TestingFiles/iceland.jpg");
        IcelandHash = hasher.ComputeHash(icelandStream);
        using var mountainStream = File.OpenRead("./TestingFiles/mountain.jpg");
        MountainHash = hasher.ComputeHash(mountainStream);
        using var roadStream = File.OpenRead("./TestingFiles/road.jpg");
        RoadHash = hasher.ComputeHash(roadStream);

        WrittenLedger = await File.ReadAllTextAsync("./TestingFiles/Ledger.nd.json");
    }

    [Test]
    public async Task IdenticalLedgersCreateIdenticalResultsAsync()
    {
        var ledgerA = new Ledger(Seed);
        var ledgerB = new Ledger(Seed);

        var churchTime = DateTime.Parse("2018-08-18T07:22:16.0000000Z", CultureInfo.InvariantCulture);
        var churchA = await ledgerA.AddFileToLedger("church.jpg", "John.Smith@example.com", churchTime, ChurchHash);
        var churchB = await ledgerB.AddFileToLedger("church.jpg", "John.Smith@example.com", churchTime, ChurchHash);
        Assert.AreEqual(churchA.RecordHash, churchB.RecordHash);

        var icelandTime = DateTime.Parse("2018-08-18T07:22:16.0000000Z", CultureInfo.InvariantCulture);
        var icelandA = await ledgerA.AddFileToLedger("iceland.jpg", "John.Smith@example.com", icelandTime, IcelandHash);
        var icelandB = await ledgerB.AddFileToLedger("iceland.jpg", "John.Smith@example.com", icelandTime, IcelandHash);
        Assert.AreEqual(icelandA.RecordHash, icelandB.RecordHash);

        var mountainTime = DateTime.Parse("2018-08-18T07:22:16.0000000Z", CultureInfo.InvariantCulture);
        var mountainA = await ledgerA.AddFileToLedger("mountain.jpg", "John.Smith@example.com", mountainTime, MountainHash);
        var mountainB = await ledgerB.AddFileToLedger("mountain.jpg", "John.Smith@example.com", mountainTime, MountainHash);
        Assert.AreEqual(mountainA.RecordHash, mountainB.RecordHash);

        var roadTime = DateTime.Parse("2018-08-18T07:22:16.0000000Z", CultureInfo.InvariantCulture);
        var roadA = await ledgerA.AddFileToLedger("mountain.jpg", "John.Smith@example.com", roadTime, RoadHash);
        var roadB = await ledgerB.AddFileToLedger("mountain.jpg", "John.Smith@example.com", roadTime, RoadHash);
        Assert.AreEqual(roadA.RecordHash, roadB.RecordHash);
    }

    [Test]
    public async Task CanCreateLedgerInMiddle()
    {
        var ledgerA = new Ledger(Seed);

        var churchTime = DateTime.Parse("2018-08-18T07:22:16.0000000Z", CultureInfo.InvariantCulture);
        _ = ledgerA.AddFileToLedger("church.jpg", "John.Smith@example.com", churchTime, ChurchHash);

        var icelandTime = DateTime.Parse("2028-08-18T07:22:16.0000000Z", CultureInfo.InvariantCulture);
        var icelandA = await ledgerA.AddFileToLedger("iceland.jpg", "John.Smith@example.com", icelandTime, IcelandHash);

        var ledgerB = new Ledger(icelandA.RecordHash);

        var mountainTime = DateTime.Parse("2018-08-05T07:22:16.0000000Z", CultureInfo.InvariantCulture);
        var mountainA = await ledgerA.AddFileToLedger("mountain.jpg", "John.Smith@example.com", mountainTime, MountainHash);
        var mountainB = await ledgerB.AddFileToLedger("mountain.jpg", "John.Smith@example.com", mountainTime, MountainHash);
        Assert.AreEqual(mountainA.RecordHash, mountainB.RecordHash);

        var roadTime = DateTime.Parse("2000-01-18T07:22:16.0000000Z", CultureInfo.InvariantCulture);
        var roadA = await ledgerA.AddFileToLedger("road.jpg", "John.Smith@example.com", roadTime, RoadHash);
        var roadB = await ledgerB.AddFileToLedger("road.jpg", "John.Smith@example.com", roadTime, RoadHash);
        Assert.AreEqual(roadA.RecordHash, roadB.RecordHash);
    }

    [Test]
    public async Task WrittenLedgerIsCorrect()
    {
        var ledger = new Ledger(Seed);
        var testLedger = string.Empty;

        var churchTime = DateTime.Parse("2018-08-18T07:22:16.0000000Z", CultureInfo.InvariantCulture);
        var church = await ledger.AddFileToLedger("church.jpg", "John.Smith@example.com", churchTime, ChurchHash);
        testLedger += church.ToString();

        var icelandTime = DateTime.Parse("1945-08-18T07:22:16.0000000Z", CultureInfo.InvariantCulture);
        var iceland = await ledger.AddFileToLedger("iceland.jpg", "John.Smith@example.com", icelandTime, IcelandHash);
        testLedger += iceland.ToString();

        var mountainTime = DateTime.Parse("1728-08-18T07:22:16.0000000Z", CultureInfo.InvariantCulture);
        var mountain = await ledger.AddFileToLedger("mountain.jpg", "John.Smith@example.com", mountainTime, MountainHash);
        testLedger += mountain.ToString();

        var roadTime = DateTime.Parse("2000-01-18T07:22:16.0000000Z", CultureInfo.InvariantCulture);
        var road = await ledger.AddFileToLedger("road.jpg", "John.Smith@example.com", roadTime, RoadHash);
        testLedger += road.ToString();

        Assert.AreEqual(WrittenLedger, testLedger);
    }
}
