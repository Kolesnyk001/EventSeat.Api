using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var databasePath = Path.Combine(builder.Environment.ContentRootPath, "eventseat.db");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={databasePath}"));

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

var events = app.MapGroup("/api/events");

events.MapGet("", async (AppDbContext db) =>
    await db.Events.AsNoTracking().OrderBy(item => item.Id).ToListAsync());

events.MapGet("/{eventId:int}", async Task<IResult> (int eventId, AppDbContext db) =>
{
    var item = await db.Events.AsNoTracking().FirstOrDefaultAsync(item => item.Id == eventId);
    return item is null ? Results.NotFound() : Results.Ok(item);
});

events.MapPost("", async Task<IResult> (CreateEventRequest request, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) || request.Date is null)
        return Results.BadRequest(new { message = "Вкажіть назву і дату події." });

    var item = new Event { Name = request.Name.Trim(), Date = request.Date.Value };
    db.Events.Add(item);
    await db.SaveChangesAsync();

    return Results.Created($"/api/events/{item.Id}", item);
});

events.MapGet("/{eventId:int}/tables", async Task<IResult> (int eventId, AppDbContext db) =>
{
    if (!await db.Events.AnyAsync(item => item.Id == eventId))
        return Results.NotFound();

    return Results.Ok(await db.SeatTables.AsNoTracking()
        .Where(table => table.EventId == eventId).OrderBy(table => table.Id).ToListAsync());
});

events.MapPost("/{eventId:int}/tables",
    async Task<IResult> (int eventId, CreateNamedItemRequest request, AppDbContext db) =>
{
    if (!await db.Events.AnyAsync(item => item.Id == eventId))
        return Results.NotFound();

    if (string.IsNullOrWhiteSpace(request.Name))
        return Results.BadRequest(new { message = "Вкажіть назву столу." });

    var table = new SeatTable { EventId = eventId, Name = request.Name.Trim() };
    db.SeatTables.Add(table);
    await db.SaveChangesAsync();

    return Results.Json(table, statusCode: StatusCodes.Status201Created);
});

events.MapGet("/{eventId:int}/guests", async Task<IResult> (int eventId, AppDbContext db) =>
{
    if (!await db.Events.AnyAsync(item => item.Id == eventId))
        return Results.NotFound();

    return Results.Ok(await db.Guests.AsNoTracking()
        .Where(guest => guest.EventId == eventId).OrderBy(guest => guest.Id).ToListAsync());
});

events.MapPost("/{eventId:int}/guests",
    async Task<IResult> (int eventId, CreateNamedItemRequest request, AppDbContext db) =>
{
    if (!await db.Events.AnyAsync(item => item.Id == eventId))
        return Results.NotFound();

    if (string.IsNullOrWhiteSpace(request.Name))
        return Results.BadRequest(new { message = "Вкажіть ім’я гостя." });

    var guest = new Guest { EventId = eventId, Name = request.Name.Trim() };
    db.Guests.Add(guest);
    await db.SaveChangesAsync();

    return Results.Json(guest, statusCode: StatusCodes.Status201Created);
});

events.MapPut("/{eventId:int}/guests/{guestId:int}/table",
    async Task<IResult> (int eventId, int guestId, AssignTableRequest request, AppDbContext db) =>
{
    var guest = await db.Guests.FirstOrDefaultAsync(item =>
        item.Id == guestId && item.EventId == eventId);

    if (guest is null)
        return Results.NotFound();

    if (request.TableId is int tableId &&
        !await db.SeatTables.AnyAsync(table => table.Id == tableId && table.EventId == eventId))
    {
        return Results.BadRequest(new { message = "Виберіть стіл цієї події." });
    }

    guest.SeatTableId = request.TableId;
    await db.SaveChangesAsync();

    return Results.Ok(guest);
});

app.Run();
