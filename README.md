# connection-manager

This is a simple CLI tool to launch remote (SSH) connections from the terminal without having to provide credentials every time. 

## Features
* Pick from a list of preconfigured connections and start a session
* Manage the saved connections (host, port, username, password or SSH key, ...)
* Start an ad-hoc connection using e.g. the default SSH key or a given set of credentials

## Technology 
* .NET 9 / C# 13 
* EF Core with SQLite storage 
* Spectre.Console 
