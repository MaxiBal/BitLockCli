# Bitlock CLI

A no-cache open-source CLI for Bitlocker written in c#.

## Installation

.NET Core 3.1 and Visual Studio 2019 is needed to build the project.

1. clone this repo
```bash
git clone https://github.com/MaxiBal/BitLockCli.git
```

2. Next, open BitLockCli.csproj   
3. Build the project with Ctrl+Shift+B   

### Built With

* [Commandlineparser](https://github.com/commandlineparser/commandline)

## Usage

```bash
bitlockcli [files/directories] -t <time>
```

files/directories - the files or directories you want to lock/unlock separated by a single space.   

time - the amount of time (in seconds) until the file will lock automatically.  Defaults to 5 minutes.   

Example:   
```bash
bitlockcli testing1.txt testing2.txt
```
Example with timer:
```bash
bitlockcli testing1.txt testing2.txt -t 800
```

In the case where the timer was set for too long, bitlockcli can also be terminated with Ctrl-c and the opened files will lock.   
Note: if the cmd prompt is closed instead of Ctrl-c, the files will not lock by themselves.

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.