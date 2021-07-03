using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace ElasticsearchConfigurator
{
  /// <summary>
  /// Основной класс программы.
  /// </summary>
  public static class Program
  {
    private static Options _options;
    
    internal static void Main(string[] args)
    {
      ParseCommandLineOptions(args);
      if (!string.IsNullOrEmpty(_options.CreateIndex))
      {
        if (TryCreateIndex(_options.CreateIndex))
        {
          Console.WriteLine($"Index \"{_options.CreateIndex}\" created.");
        }
      }
      else if (!string.IsNullOrEmpty(_options.DeleteIndex))
      {
        if (TryDeleteIndex(_options.DeleteIndex))
        {
          Console.WriteLine($"Index \"{_options.DeleteIndex}\" deleted.");
        }
      }
    }

    private static bool TryDeleteIndex(string indexName)
    {
      throw new NotImplementedException();
    }

    private static bool TryCreateIndex(string indexName)
    {
      throw new NotImplementedException();
    }

    private static void ParseCommandLineOptions(string[] args)
    {
      static void WithParsed(Options options)
      {
        _options = options;
      }

      static void WithNotParsed(IEnumerable<Error> errors, ParserResult<Options> parserResult)
      {
        var helpText = HelpText.AutoBuild(parserResult, text =>
        {
          text.Heading = "Elasticsearch Configurator by Hladkov Yehor";
          text.AutoVersion = false;
          text.AutoHelp = true;
          return HelpText.DefaultParsingErrorsHandler(parserResult, text);
        }, e => e);
        Console.WriteLine(helpText.ToString());
      }
      
      var parser = new Parser
      {
          Settings =
          {
              AutoHelp = true,
              CaseSensitive = true,
              AutoVersion = false,
              HelpWriter = null,
              IgnoreUnknownArguments = false,
              MaximumDisplayWidth = 80
          }
      };
      var parserResult = parser.ParseArguments<Options>(args);
      parserResult
          .WithParsed(WithParsed)
          .WithNotParsed(errors => WithNotParsed(errors, parserResult));
    }
  }
}