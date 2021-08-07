using Application.AgentCoodinatorService.Data;
using Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.AgentCoodinatorService.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class WeatherForecastController : ControllerBase
	{
		private static readonly string[] Summaries = new[]
		{
			"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
		};

		private readonly ILogger<WeatherForecastController> _logger;

		public WeatherForecastController(ILogger<WeatherForecastController> logger)
		{
			_logger = logger;
		}


		// TODO: Move this to seed data
		[HttpGet]
		public async Task<IActionResult> InsertMetaData()
		{
			using (var db = new AgentCoordinatorDbContext())
			{
				if (!(db.Seniorities.Count() > 0))
				{
					var seniorities = new List<Seniority>();
					seniorities.Add(new Seniority { Id = 1, Efficiency = 0.4, Name = "Junior" });
					seniorities.Add(new Seniority { Id = 2, Efficiency = 0.6, Name = " Mid-Level" });
					seniorities.Add(new Seniority { Id = 3, Efficiency = 0.8, Name = "Senior" });
					seniorities.Add(new Seniority { Id = 4, Efficiency = 0.5, Name = "Tem Lead" });
					await db.Seniorities.AddRangeAsync(seniorities);
				}


				if (!(db.Teams.Count() > 0))
				{
					var teams = new List<Team>();
					teams.Add(new Team { Id = 1, Name = "Team A", IsOverflow = true, Shift = Shift.OfficeTime });
					teams.Add(new Team { Id = 2, Name = "Team B", IsOverflow = false, Shift = Shift.Evening });
					teams.Add(new Team { Id = 3, Name = "Team C", IsOverflow = false, Shift = Shift.Night });
					teams.Add(new Team { Id = 4, Name = "Overflow", IsOverflow = false, Shift = Shift.None });
					await db.Teams.AddRangeAsync(teams);
				}

				if (!(db.Agents.Count() > 0))
				{
					var agents = new List<Agent>();
					agents.Add(new Agent { Id = 1, Name = "Kelly", SeniorityId = 1, TeamId = 1 });
					agents.Add(new Agent { Id = 2, Name = "Robins", SeniorityId = 4, TeamId = 1 });
					agents.Add(new Agent { Id = 3, Name = "Cullen", SeniorityId = 2, TeamId = 1 });
					agents.Add(new Agent { Id = 4, Name = "Kaitlan", SeniorityId = 2, TeamId = 1 });


					agents.Add(new Agent { Id = 5, Name = "Maverick ", SeniorityId = 3, TeamId = 2 });
					agents.Add(new Agent { Id = 6, Name = "Mcken", SeniorityId = 2, TeamId = 2 });
					agents.Add(new Agent { Id = 7, Name = "Cieran", SeniorityId = 1, TeamId = 2 });
					agents.Add(new Agent { Id = 8, Name = "Hewitt", SeniorityId = 1, TeamId = 2 });

					agents.Add(new Agent { Id = 9, Name = "Kaine", SeniorityId = 2, TeamId = 3 });
					agents.Add(new Agent { Id = 10, Name = "Busby", SeniorityId = 2, TeamId =3 });

					agents.Add(new Agent { Id = 11, Name = "Marina", SeniorityId = 1, TeamId = 4 });
					agents.Add(new Agent { Id = 12, Name = "Desiree", SeniorityId = 1, TeamId = 4 });
					agents.Add(new Agent { Id = 13, Name = "Zarah", SeniorityId = 1, TeamId = 4 });
					agents.Add(new Agent { Id = 14, Name = "Sianna", SeniorityId = 1, TeamId = 4 });
					agents.Add(new Agent { Id = 15, Name = "Bessie", SeniorityId = 1, TeamId = 4 });
					agents.Add(new Agent { Id = 16, Name = "Dilara", SeniorityId = 1, TeamId = 4 });
					await db.Agents.AddRangeAsync(agents);
				}

				await db.SaveChangesAsync();

			}


			return Ok();
		}


	}
}
