﻿using Microsoft.EntityFrameworkCore;
using OctoCodes.Models;

namespace OctoCodes.Data
{
    public class OctoCodesContext : DbContext
    {
        public OctoCodesContext(DbContextOptions<OctoCodesContext> options) : base(options) { }

        public DbSet<Users> Users { get; set; }
    }
}
