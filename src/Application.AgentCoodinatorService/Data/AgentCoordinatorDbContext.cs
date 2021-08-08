using Application.Common;
using Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.AgentCoodinatorService.Data
{
	public class AgentCoordinatorDbContext: DbContext
	{
		public DbSet<SupportRequest> SupportRequests { get; set; }
		public DbSet<Team> Teams{ get; set; }
		public DbSet<Seniority> Seniorities { get; set; }
		public DbSet<Agent> Agents { get; set; }

		public string DbPath { get; private set; }

		public AgentCoordinatorDbContext()
		{			
			DbPath = "AgentDb.db";
		}

		protected override void OnConfiguring(DbContextOptionsBuilder options)
		   => options.UseSqlite($"Data Source={DbPath}");

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			

			base.OnModelCreating(modelBuilder);
		}
	}
}
