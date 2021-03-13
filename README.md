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

### Changing Passwords

To change a password, you can run:
```bash
bitlockcli testing1.txt --change
```
From there, you will be prompted to enter 2 passwords: the first one is the previous password for the file, and the second one is the new password.   

Example:
```bash
bitlockcli testing1.txt --change
Enter the file's password:
********
Enter the new password:
******
Locked file(s) successfully.
```

### Incorrect Passwords

On an incorrect password, bitlockcli will say that the password is incorrect for the given files.

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.