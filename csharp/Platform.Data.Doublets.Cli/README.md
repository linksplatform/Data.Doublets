# Doublets CLI

A command-line interface for LinksPlatform's Platform.Data.Doublets library, implementing GNU getopt standard with Links notation support.

## Installation

Build the project:
```bash
dotnet build Platform.Data.Doublets.Cli.csproj
```

Or publish as self-contained executable:
```bash
dotnet publish -c Release -o ./bin --self-contained true
```

## Usage

The CLI supports multiple commands and notation formats:

### Commands

- `create` - Create a new link
- `update` - Update an existing link  
- `delete` - Delete a link
- `each` - Query links matching a pattern
- `interactive` - Enter interactive mode

### Links Notation

The CLI supports Links notation as specified in the issue:

- `(id: source target)` - Full link specification with ID
- `(source target)` - Link with source and target only
- `source target` - Simple space-separated format
- `*` - Wildcard for queries

### Examples

#### Creation
```bash
# Create link with source=1, target=1
./doublets create "(1: 1 1)"
./doublets create "1 1"
```

#### Update
```bash
# Update link ID 1 to have target=2
./doublets update "(1: 1 1)" "(1: 1 2)"
```

#### Deletion
```bash
# Delete link with ID 1
./doublets delete "(1: 1 1)"
```

#### Querying
```bash
# Find all links
./doublets each "* *"

# Find links with source=1
./doublets each "1 *"

# Find specific link
./doublets each "(1: 1 1)"
```

#### Interactive Mode
```bash
./doublets interactive
```

In interactive mode, you can use all the notation formats from the issue:

- `(1: 1 1)` - Create link
- `+ (1: 1 1)` - Create link
- `+= (1: 1 1)` - Create link
- `- (1: 1 1)` - Delete link
- `-= (1: 1 1)` - Delete link
- `~ (1: 1 1) (1: 1 2)` - Update link
- `~= (1: 1 1) (1: 1 2)` - Update link  
- `(1: 1 1)?` - Query links
- `1 1?` - Query with simple notation
- `* 1?` - Query with wildcards
- `help` - Show help
- `exit` - Exit interactive mode

### Database

By default, the CLI uses `default.links` as the database file. You can specify a different file with the `-d` option:

```bash
./doublets -d mydb.links create "1 1"
```

## Architecture

The CLI is built using:
- CommandLineParser for GNU getopt compliance
- Platform.Data.Doublets.Memory.United.Generic.UnitedMemoryLinks for storage
- Links notation parsing supporting all formats from issue #346

The implementation follows the Links platform conventions and provides a cross-platform CLI interface as requested in the issue.