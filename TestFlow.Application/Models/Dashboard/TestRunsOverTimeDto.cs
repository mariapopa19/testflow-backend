using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestFlow.Application.Models.Dashboard;
public class TestRunsOverTimeDto
{
    public DateOnly Date { get; set; }
    public int Count { get; set; }
}
