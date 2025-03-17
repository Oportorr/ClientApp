using Microsoft.EntityFrameworkCore;

namespace CasinoSups
{
    public class CasinopitDbContext : DbContext
    {
        public CasinopitDbContext(DbContextOptions<CasinopitDbContext> options) : base(options)
        {



        }
     
    
    
    }
}
