using Microsoft.EntityFrameworkCore;
using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
}