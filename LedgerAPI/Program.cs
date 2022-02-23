using LedgerLibrary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

var ledgerFile = "./Ledger.nd.json";
var ledgerText = await File.ReadAllLinesAsync(ledgerFile);

var ledgerList = new List<LedgerRecord>();
Ledger ledger;

if (ledgerText.Length == 0)
{
    var hasher = SHA512.Create();
    var seed = hasher.ComputeHash(Encoding.UTF8.GetBytes("This is the starting seed of the app."));
    ledger = new Ledger(seed);
    await AppendToLedger(await ledger.AddBlankRecord());
}
else
{
    foreach (var line in ledgerText) ledgerList.Add(JsonSerializer.Deserialize<LedgerRecord>(line));
    ledger = new Ledger(ledgerList.Last().RecordHash);
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/", async (string fileName, string owner, DateTime timeStamp, string base64FileHash) =>
{
    var result = await ledger.AddFileToLedger(fileName, owner, timeStamp, Convert.FromBase64String(base64FileHash));
    await AppendToLedger(result);
    return Results.Ok(result);
})
.WithName("PostNewRecord");

app.MapGet("/Ledger.json", () => {
    return Results.Ok(ledgerList);
})
.WithName("GetAllRecords")
.Produces<LedgerRecord>(StatusCodes.Status200OK);

app.MapGet("/lastRecord.json", () => {
    return Results.Ok(ledgerList.Last());
})
.WithName("GetLastRecord")
.Produces<LedgerRecord>(StatusCodes.Status200OK);

app.MapGet("/{base64FileHash}.json", (string base64FileHash) => {
    var hash = Convert.FromBase64String(base64FileHash);
    var record = ledgerList.Where(record => record.RecordHash == hash)?.First();
    if (record is null) Results.NotFound();
    return Results.Ok(record);
})
.WithName("GetRecord")
.Produces<LedgerRecord>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);


app.Run();

async Task AppendToLedger(LedgerRecord ledgerRecord)
{
    ledgerList.Add(ledgerRecord);
    await File.AppendAllTextAsync(ledgerFile, ledgerRecord.ToString());
}
