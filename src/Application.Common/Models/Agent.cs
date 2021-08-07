using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Models
{
	public class Agent
	{
		public int Id { get; set; }
		public string Name { get; set; }

		public short SeniorityId { get; set; }
		public Seniority Seniority { get; set; }

		public short TeamId { get; set; }
		public Team Team { get; set; }
	}
}
