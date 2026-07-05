# StarCraft 2 Server Blocker

A small Windows utility that blocks outbound connections to StarCraft 2 game server regions using Windows Firewall. Useful when you want to restrict which ladder regions your client can reach.

## Requirements

- Windows 10 or later
- [.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework/net48) (included on most current Windows installs)
- **Administrator rights** — the app must run elevated to create firewall rules

## How it works

1. Pick a region from the dropdown.
2. Click **Block** to add an outbound firewall rule for that region's IP addresses.
3. Click **Unblock** to remove the rule for the selected region.
4. Click **Unblock All** to remove every rule created by this app.

Blocked regions show `(blocked)` in the dropdown, and a summary line lists all currently blocked regions.

Blocked state is refreshed at startup and after block/unblock actions. Keyboard shortcuts: **Enter** = Block, **Ctrl+U** = Unblock, **Ctrl+Shift+U** = Unblock All.

Firewall rules are named with the prefix `Sc` followed by the region name (for example `ScUS East`).

## Server IP lists

Each region's IP addresses are stored as plain-text files in the `Servers` folder next to the executable:

```
Servers/
  US East.ini
  US West.ini
  Korea.ini
  ...
```

Each `.ini` file contains one IP address or CIDR range per line. Lines starting with `;` or `#` are comments. Invalid addresses in INI files are ignored when loading.

You can edit these files directly, or use **Edit IPs...** in the app to add or remove addresses from the UI. Changes are saved back to the corresponding `.ini` file. If a region is already blocked when you save edits, the firewall rule is updated automatically.

**Open Servers Folder** opens the `Servers` directory in File Explorer.

To add a custom region, create a new `Region Name.ini` file in the `Servers` folder and restart the app.

## Building

Open `SC2ServerBlocker.sln` in Visual Studio and build the **Release** configuration, or run:

```powershell
dotnet build SC2ServerBlocker.sln -c Release
```

The solution contains:

- **SC2ServerBlocker.Core** — firewall logic, server lists, and region blocking (no WPF)
- **SC2ServerBlocker** — WPF UI executable
- **SC2ServerBlocker.Tests** — unit tests against Core only

The built executable is copied to `SC2ServerBlocker\bin\Release\net48\`.

Firewall COM interop uses `SC2ServerBlocker.Core/NetFwTypeLib/generated/Interop.NetFwTypeLib.dll`, generated from `%SystemRoot%\System32\FirewallAPI.dll` with TlbImp. To regenerate:

```powershell
& "${env:ProgramFiles(x86)}\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\TlbImp.exe" "$env:SystemRoot\System32\FirewallAPI.dll" /out:SC2ServerBlocker.Core\NetFwTypeLib\generated\Interop.NetFwTypeLib.dll /namespace:NetFwTypeLib /silent
```

## Tests

The solution includes `SC2ServerBlocker.Tests`, an MSTest project that covers core logic without calling Windows Firewall or other OS APIs. Firewall behavior is exercised through in-memory fakes and mocks.

Run all tests:

```powershell
dotnet test SC2ServerBlocker.sln -c Release
```

## Startup checks

When the app launches it verifies:

- It is running with administrator privileges
- Windows Firewall can be accessed
- The active firewall profile is enabled

If something is wrong, a banner appears at the top of the window and block/unblock actions are disabled. Operational messages (success or failure after you click a button) appear in the status bar at the bottom — no popup dialogs for routine actions.

## Undoing everything manually

If needed, you can remove rules yourself:

1. Open **Windows Defender Firewall with Advanced Security**
2. Go to **Outbound Rules**
3. Delete rules whose names start with `Sc`

Or run **Unblock All** in the app.

## License

See [LICENSE](LICENSE) in the repository root.
