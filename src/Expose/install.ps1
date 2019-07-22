[Environment]::SetEnvironmentVariable(
    "Path",
    [Environment]::GetEnvironmentVariable("Path", [EnvironmentVariableTarget]::Machine) + ";$($PSScriptRoot)",
    [EnvironmentVariableTarget]::Machine)