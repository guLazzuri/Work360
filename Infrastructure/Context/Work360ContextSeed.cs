using Microsoft.EntityFrameworkCore;
using Work360.Domain.Entity;

namespace Work360.Infrastructure.Context
{
    public static class Work360ContextSeed
    {
        public static async Task SeedAsync(Work360Context context)
        {
            if (!await context.Users.AnyAsync())
            {
                var user = new User
                {
                    UserID = Guid.NewGuid(),
                    Name = "Usuário Teste",
                    Email = "teste@teste.com",
                    Password = "123456" // Em um cenário real, esta senha seria hasheada
                };

                context.Users.Add(user);
                await context.SaveChangesAsync();
            }
        }
    }
}
