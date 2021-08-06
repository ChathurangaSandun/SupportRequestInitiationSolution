using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common
{
	public class SupportRequest
	{
		public string User { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public Guid Id { get; set; }
	}
}
