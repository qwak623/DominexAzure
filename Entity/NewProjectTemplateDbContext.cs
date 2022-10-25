using Havit.Data.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dominex.Entity;

public class DominexDbContext : Havit.Data.EntityFrameworkCore.DbContext
{
	/// <summary>
	/// Konstruktor.
	/// Pro použití v unit testech, jiné použití nemá.
	/// </summary>
	internal DominexDbContext()
	{
		// NOOP
	}

	/// <summary>
	/// Konstruktor.
	/// </summary>
	public DominexDbContext(DbContextOptions options) : base(options)
	{
		// NOOP
	}

	/// <inheritdoc />
	protected override void CustomizeModelCreating(ModelBuilder modelBuilder)
	{
		base.CustomizeModelCreating(modelBuilder);

		// modelBuilder.HasSequence<int>("XySequence");

		modelBuilder.RegisterModelFromAssembly(typeof(Dominex.Model.Localizations.Language).Assembly);
		modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
	}
}
