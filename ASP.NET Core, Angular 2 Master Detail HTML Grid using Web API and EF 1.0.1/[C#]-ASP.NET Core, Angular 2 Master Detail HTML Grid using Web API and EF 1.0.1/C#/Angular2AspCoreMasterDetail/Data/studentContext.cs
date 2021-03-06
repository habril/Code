using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Angular2AspCoreMasterDetail.Data
{
    public class studentContext : DbContext
	{
		public studentContext(DbContextOptions<studentContext> options)
			: base(options) { }
		public studentContext() { }
		public DbSet<StudentMasters> StudentMasters { get; set; }
		public DbSet<StudentDetails> StudentDetails { get; set; }
	}
}
