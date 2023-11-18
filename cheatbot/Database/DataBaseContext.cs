using cheatbot.Database.models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.Database
{
    public class DataBaseContext : DbContext
    {
        public DbSet<ApiSettings> ApiSettings { get; set; }
        public DbSet<DropModel> Drops { get; set; }
        public DbSet<ChannelModel> Channels { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string db = Directory.GetCurrentDirectory();
            string path = Path.Combine(db, "database.db");
            optionsBuilder.UseSqlite($"Data Source={path}");
        }
    }
}
