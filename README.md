# Eventy

Windows event log viewer

## Usage

```bash
eventy <command> <options>
```
    
## Commands

* `list` - List all available log names.
* `query` - Query for log entries under log name.
* `view` - View a single log entry.

## Options

* `-m|--max [<number>]` - Set max records to query. Defaults to 10. Omit value for all.
* `-r|--reverse` - Whether to read events from the newest or oldest event.
    
## Examples

List all log names.

```bash
eventy list
```

List log entries under the Application log name.

```bash
eventy query Application
```

List 100 log entries under Application, list from newest to oldest.

```bash
eventy query Application -m 100 -r
```

View a single log entry under the Application log name.

```bash
eventy view Application 123456
```