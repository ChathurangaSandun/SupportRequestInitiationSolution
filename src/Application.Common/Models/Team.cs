using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Models
{
	public class Team
	{	
		public short Id { get; set; }
		public string Name { get; set; }
		public Shift Shift { get; set; }
		public bool IsOverflow { get; set; }

		public ICollection<Agent> Agents { get; set; }
	}

	public enum Shift
	{
		None,
		OfficeTime,
		Evening,
		Night
		
	}
}
