﻿# Bitlock CLI

An open-source CLI for Bitlocker written in c#.

## Installation

.NET Core 3.1 is needed to build the project.

## Usage

```bash
bitlockcli [files] -t <time>
```

file - the files you want to lock/unlock separated by a single space.   

time - the amount of time (in seconds) until the file will lock automatically.  Defaults to 5 minutes.   

Example:   
```bash
bitlockcli testing1.txt testing2.txt
```
