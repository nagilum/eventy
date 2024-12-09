# Eventy

Command line Windows event log viewer

## How to Build

```bash
git clone https://github.com/nagilum/eventy
cd eventy
dotnet build
```

## Requirements

Eventy is written in C# and requires .NET 8 which can be downloaded over at <https://dotnet.microsoft.com/download/dotnet/8.0>

## Usage

```bash
eventy [log-name] [record-id] [options]
```
    
## Options

* `-m|--max <number>` - Set max number of log entries to list. Defaults to 10.
* `-r|--reverse` - Whether to read events from oldest to newest. Defaults to newest to oldest.
* `-f|--from <date>` - List log entries from (including) given date/time.
* `-t|--to <date>` - List log entries to (including) given date/time.
* `-l|--level <name>` -  Only list log entries matching given log level name. Can be repeated.
* `-s|--search <term>` - Search in any field for the given text. Can be repeated.
* `-a|--all` - Set whether all search terms must be found or just one for a match.
    
## Examples

List the about/help screen.

```bash
eventy --help
```

List all log names.

```bash
eventy
```

List log entries under the Application log name.

```bash
eventy Application
```

List 100 log entries under Application, list from oldest to newest

```bash
eventy Application -m 100 -r
```

View a single log entry under the Application log name.

```bash
eventy Application 123456
```

Search for, and display log entry with record ID 123456.

```bash
eventy 123456
```

Search for all log entries under the Application name that are after (and including) December 1st 2024, and match log levels info/error/critical, and contains search terms foo and/or bar.  

```bash
eventy application -m -1 -f 2024-12-01 -l info -l error -l crit -s foo -s bar
```