using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dominex.Entity.Tests;

[TestClass]
public class DominexDbContextTests
{
	[TestMethod]
	public void DominexDbContext_CheckModelConventions()
	{
		// Arrange
		DbContextOptions<DominexDbContext> options = new DbContextOptionsBuilder<DominexDbContext>()
			.UseInMemoryDatabase(nameof(DominexDbContext))
			.Options;
		DominexDbContext dbContext = new DominexDbContext(options);

		// Act
		Havit.Data.EntityFrameworkCore.ModelValidation.ModelValidator modelValidator = new Havit.Data.EntityFrameworkCore.ModelValidation.ModelValidator();
		string errors = modelValidator.Validate(dbContext);

		// Assert
		if (!String.IsNullOrEmpty(errors))
		{
			Assert.Fail(errors);
		}
	}
}
