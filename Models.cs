using Microsoft.EntityFrameworkCore;

public class Event
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateOnly Date { get; set; }
}

public class SeatTable
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public required string Name { get; set; }
}

public class Guest
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public required string Name { get; set; }
    public int? SeatTableId { get; set; }
}

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<SeatTable> SeatTables => Set<SeatTable>();
    public DbSet<Guest> Guests => Set<Guest>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<SeatTable>().HasOne<Event>().WithMany()
            .HasForeignKey(table => table.EventId);

        model.Entity<Guest>().HasOne<Event>().WithMany()
            .HasForeignKey(guest => guest.EventId);

        model.Entity<Guest>().HasOne<SeatTable>().WithMany()
            .HasForeignKey(guest => guest.SeatTableId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public record CreateEventRequest(string? Name, DateOnly? Date);
public record CreateNamedItemRequest(string? Name);
public record AssignTableRequest(int? TableId);
