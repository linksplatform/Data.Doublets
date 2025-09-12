using CommandLine;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;

namespace Platform.Data.Doublets.Cli;

class Program
{
    static async Task<int> Main(string[] args)
    {
        return await Parser.Default.ParseArguments<CreateOptions, UpdateOptions, DeleteOptions, QueryOptions, InteractiveOptions>(args)
            .MapResult(
                (CreateOptions opts) => RunCreate(opts),
                (UpdateOptions opts) => RunUpdate(opts),
                (DeleteOptions opts) => RunDelete(opts),
                (QueryOptions opts) => RunQuery(opts),
                (InteractiveOptions opts) => RunInteractive(opts),
                errs => Task.FromResult(1));
    }

    static async Task<int> RunCreate(CreateOptions opts)
    {
        using var links = CreateLinksStorage(opts.Database);
        
        var parsedLink = ParseLinksNotation(opts.Link);
        if (parsedLink == null)
        {
            Console.Error.WriteLine($"Invalid links notation: {opts.Link}");
            return 1;
        }

        try
        {
            uint createdLinkId;
            if (parsedLink.Source != null && parsedLink.Target != null)
            {
                createdLinkId = links.Create();
                links.Update(createdLinkId, parsedLink.Source.Value, parsedLink.Target.Value);
            }
            else
            {
                createdLinkId = links.Create();
            }
            
            var createdLink = links.GetLink(createdLinkId);
            Console.WriteLine($"({createdLink[0]}: {createdLink[1]} {createdLink[2]})");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error creating link: {ex.Message}");
            return 1;
        }
    }

    static async Task<int> RunUpdate(UpdateOptions opts)
    {
        using var links = CreateLinksStorage(opts.Database);
        
        var oldLink = ParseLinksNotation(opts.OldLink);
        var newLink = ParseLinksNotation(opts.NewLink);
        
        if (oldLink == null || newLink == null)
        {
            Console.Error.WriteLine("Invalid links notation");
            return 1;
        }

        try
        {
            if (oldLink.Id == null || newLink.Source == null || newLink.Target == null)
            {
                Console.Error.WriteLine("Update requires full link specifications");
                return 1;
            }

            var updatedLinkId = links.Update(oldLink.Id.Value, newLink.Source.Value, newLink.Target.Value);
            var updatedLink = links.GetLink(updatedLinkId);
            Console.WriteLine($"({updatedLink[0]}: {updatedLink[1]} {updatedLink[2]})");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error updating link: {ex.Message}");
            return 1;
        }
    }

    static async Task<int> RunDelete(DeleteOptions opts)
    {
        using var links = CreateLinksStorage(opts.Database);
        
        var parsedLink = ParseLinksNotation(opts.Link);
        if (parsedLink == null)
        {
            Console.Error.WriteLine($"Invalid links notation: {opts.Link}");
            return 1;
        }

        try
        {
            if (parsedLink.Id == null)
            {
                Console.Error.WriteLine("Delete requires link ID");
                return 1;
            }

            links.Delete(parsedLink.Id.Value);
            Console.WriteLine($"Link {parsedLink.Id} deleted");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error deleting link: {ex.Message}");
            return 1;
        }
    }

    static async Task<int> RunQuery(QueryOptions opts)
    {
        using var links = CreateLinksStorage(opts.Database);
        
        var parsedLink = ParseLinksNotation(opts.Pattern);
        if (parsedLink == null)
        {
            Console.Error.WriteLine($"Invalid links notation: {opts.Pattern}");
            return 1;
        }

        try
        {
            var constants = links.Constants;
            var query = new uint[3];
            query[0] = parsedLink.Id ?? constants.Any;
            query[1] = parsedLink.Source ?? constants.Any;
            query[2] = parsedLink.Target ?? constants.Any;

            links.Each(link =>
            {
                bool matches = true;
                if (parsedLink.Id != null && link[0] != parsedLink.Id) matches = false;
                if (parsedLink.Source != null && link[1] != parsedLink.Source) matches = false;
                if (parsedLink.Target != null && link[2] != parsedLink.Target) matches = false;

                if (matches)
                {
                    Console.WriteLine($"({link[0]}: {link[1]} {link[2]})");
                }
                return constants.Continue;
            }, query);
            
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error querying links: {ex.Message}");
            return 1;
        }
    }

    static async Task<int> RunInteractive(InteractiveOptions opts)
    {
        using var links = CreateLinksStorage(opts.Database);
        
        Console.WriteLine("Interactive mode. Type 'help' for commands, 'exit' to quit.");
        
        while (true)
        {
            Console.Write("doublets> ");
            var input = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(input))
                continue;
                
            if (input.Trim().ToLower() == "exit")
                break;
                
            if (input.Trim().ToLower() == "help")
            {
                PrintHelp();
                continue;
            }

            try
            {
                await ProcessInteractiveCommand(links, input.Trim());
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }
        }
        
        return 0;
    }

    static async Task ProcessInteractiveCommand(ILinks<uint> links, string input)
    {
        // Handle various notation formats from the issue
        if (input.StartsWith("+") || input.StartsWith("+="))
        {
            var linkNotation = input.TrimStart('+', '=').Trim();
            var parsed = ParseLinksNotation(linkNotation);
            if (parsed != null)
            {
                uint created = links.Create();
                if (parsed.Source != null && parsed.Target != null)
                    links.Update(created, parsed.Source.Value, parsed.Target.Value);
                var link = links.GetLink(created);
                Console.WriteLine($"({link[0]}: {link[1]} {link[2]})");
            }
        }
        else if (input.StartsWith("-") || input.StartsWith("-="))
        {
            var linkNotation = input.TrimStart('-', '=').Trim();
            var parsed = ParseLinksNotation(linkNotation);
            if (parsed?.Id != null)
            {
                links.Delete(parsed.Id.Value);
                Console.WriteLine($"Link {parsed.Id} deleted");
            }
        }
        else if (input.StartsWith("~") || input.StartsWith("~="))
        {
            // Update format: ~= (old) (new)
            var parts = input.TrimStart('~', '=').Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                var oldParsed = ParseLinksNotation(parts[0]);
                var newParsed = ParseLinksNotation(parts[1]);
                if (oldParsed?.Id != null && newParsed?.Source != null && newParsed?.Target != null)
                {
                    var updated = links.Update(oldParsed.Id.Value, newParsed.Source.Value, newParsed.Target.Value);
                    var link = links.GetLink(updated);
                    Console.WriteLine($"({link[0]}: {link[1]} {link[2]})");
                }
            }
        }
        else if (input.EndsWith("?"))
        {
            // Query format
            var pattern = input.TrimEnd('?');
            var parsed = ParseLinksNotation(pattern);
            if (parsed != null)
            {
                var constants = links.Constants;
                var query = new uint[3];
                query[0] = parsed.Id ?? constants.Any;
                query[1] = parsed.Source ?? constants.Any;
                query[2] = parsed.Target ?? constants.Any;

                links.Each(link =>
                {
                    bool matches = true;
                    if (parsed.Id != null && link[0] != parsed.Id) matches = false;
                    if (parsed.Source != null && link[1] != parsed.Source) matches = false;
                    if (parsed.Target != null && link[2] != parsed.Target) matches = false;

                    if (matches)
                    {
                        Console.WriteLine($"({link[0]}: {link[1]} {link[2]})");
                    }
                    return constants.Continue;
                }, query);
            }
        }
        else
        {
            // Try to parse as simple link notation
            var parsed = ParseLinksNotation(input);
            if (parsed != null && parsed.Source != null && parsed.Target != null)
            {
                var created = links.Create();
                links.Update(created, parsed.Source.Value, parsed.Target.Value);
                var link = links.GetLink(created);
                Console.WriteLine($"({link[0]}: {link[1]} {link[2]})");
            }
            else
            {
                Console.WriteLine("Invalid command. Type 'help' for available commands.");
            }
        }
    }

    static void PrintHelp()
    {
        Console.WriteLine("Available commands:");
        Console.WriteLine("  (1: 1 1)      - Create link with source=1, target=1");
        Console.WriteLine("  + (1: 1 1)    - Create link");
        Console.WriteLine("  += (1: 1 1)   - Create link");
        Console.WriteLine("  - (1: 1 1)    - Delete link with id=1");
        Console.WriteLine("  -= (1: 1 1)   - Delete link");
        Console.WriteLine("  ~ (1: 1 1) (1: 1 2) - Update link id=1 to have target=2");
        Console.WriteLine("  ~= (1: 1 1) (1: 1 2) - Update link");
        Console.WriteLine("  (1: 1 1)?     - Query for links matching pattern");
        Console.WriteLine("  1 1?          - Query for links with source=1, target=1");
        Console.WriteLine("  * 1?          - Query for links with any source, target=1");
        Console.WriteLine("  help          - Show this help");
        Console.WriteLine("  exit          - Exit interactive mode");
    }

    record LinkNotation(uint? Id, uint? Source, uint? Target);

    static LinkNotation? ParseLinksNotation(string notation)
    {
        notation = notation.Trim();
        
        // Handle (id: source target) format
        if (notation.StartsWith("(") && notation.EndsWith(")"))
        {
            var content = notation.Substring(1, notation.Length - 2).Trim();
            var parts = content.Split(new[] { ':', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length == 3)
            {
                if (uint.TryParse(parts[0], out var id) &&
                    uint.TryParse(parts[1], out var source) &&
                    uint.TryParse(parts[2], out var target))
                {
                    return new LinkNotation(id, source, target);
                }
            }
            else if (parts.Length == 2)
            {
                if (uint.TryParse(parts[0], out var source) &&
                    uint.TryParse(parts[1], out var target))
                {
                    return new LinkNotation(null, source, target);
                }
            }
            else if (parts.Length == 1)
            {
                if (uint.TryParse(parts[0], out var id))
                {
                    return new LinkNotation(id, null, null);
                }
            }
        }
        // Handle simple "source target" format and wildcard patterns
        else
        {
            var parts = notation.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                uint? source = null, target = null;
                
                if (parts[0] != "*" && uint.TryParse(parts[0], out var s))
                    source = s;
                if (parts[1] != "*" && uint.TryParse(parts[1], out var t))
                    target = t;
                    
                return new LinkNotation(null, source, target);
            }
            else if (parts.Length == 1)
            {
                if (uint.TryParse(parts[0], out var value))
                {
                    return new LinkNotation(value, null, null);
                }
            }
        }
        
        return null;
    }

    static UnitedMemoryLinks<uint> CreateLinksStorage(string databasePath = "default.links")
    {
        return new UnitedMemoryLinks<uint>(databasePath);
    }
}

[Verb("create", HelpText = "Create a new link")]
class CreateOptions
{
    [Option('d', "database", Required = false, Default = "default.links", HelpText = "Database file path")]
    public string Database { get; set; } = "default.links";
    
    [Value(0, Required = true, HelpText = "Link notation, e.g., \"(1: 1 1)\" or \"1 1\"")]
    public string Link { get; set; } = "";
}

[Verb("update", HelpText = "Update an existing link")]
class UpdateOptions
{
    [Option('d', "database", Required = false, Default = "default.links", HelpText = "Database file path")]
    public string Database { get; set; } = "default.links";
    
    [Value(0, Required = true, HelpText = "Old link notation")]
    public string OldLink { get; set; } = "";
    
    [Value(1, Required = true, HelpText = "New link notation")]
    public string NewLink { get; set; } = "";
}

[Verb("delete", HelpText = "Delete a link")]
class DeleteOptions
{
    [Option('d', "database", Required = false, Default = "default.links", HelpText = "Database file path")]
    public string Database { get; set; } = "default.links";
    
    [Value(0, Required = true, HelpText = "Link notation to delete")]
    public string Link { get; set; } = "";
}

[Verb("each", HelpText = "Query links matching a pattern")]
class QueryOptions
{
    [Option('d', "database", Required = false, Default = "default.links", HelpText = "Database file path")]
    public string Database { get; set; } = "default.links";
    
    [Value(0, Required = true, HelpText = "Query pattern, e.g., \"(1: 1 1)\" or \"* 1\"")]
    public string Pattern { get; set; } = "";
}

[Verb("interactive", HelpText = "Interactive mode")]
class InteractiveOptions
{
    [Option('d', "database", Required = false, Default = "default.links", HelpText = "Database file path")]
    public string Database { get; set; } = "default.links";
}