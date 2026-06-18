# Installation

Tabrixel is distributed as a .NET global tool. With the [.NET SDK](https://dotnet.microsoft.com/)
installed, run:

```sh
dotnet tool install --global Tabrixel
```

This puts the `tbxl` command on your `PATH`. Update it later with:

```sh
dotnet tool update --global Tabrixel
```

## Verify the install

Check that the command is available and see its version:

```sh
tbxl --version
```

To confirm that credentials and access work, see
[Getting Started](/guide/getting-started), which walks through
`tbxl auth check`.

::: tip Shell note
Forward-slash paths work on every platform — .NET accepts them on Windows too.
Single-quoted JSON arguments work in bash, zsh, and PowerShell. Windows
`cmd.exe` needs different quoting, so prefer PowerShell there.
:::
