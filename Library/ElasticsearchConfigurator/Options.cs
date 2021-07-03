using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace ElasticsearchConfigurator
{
  public class Options
  {
    [Option('c', "create", HelpText = "Create index", Required = false)]
    public string CreateIndex { get; }
    
    [Option('d', "delete", HelpText = "Delete index", Required = false)]
    public string DeleteIndex { get; }
  }
}