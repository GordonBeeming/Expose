using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Expose
{
  internal static class Commands
  {
    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int SW_HIDE = 0;
    const int SW_SHOW = 5;

    internal const string ExposeSecurePortCmd = "[s-port] <443> as <ngrok-site>";
    internal static Task ExposeSecurePort(string[] args)
    {
      if (!int.TryParse(args[1], out int portNumber))
      {
        App.ShowMenuWithError("Port Number or HTTP Path needs to be a valid integer or url for example https://beeming.dev!");
        return Task.CompletedTask;
      }
      return ExposePortPrivate(args, "http ");
    }

    internal const string ExposePortCmd = "[port] <80> as <ngrok-site>";
    internal static Task ExposePort(string[] args)
    {
      if (!int.TryParse(args[1], out int portNumber))
      {
        App.ShowMenuWithError("Port Number or HTTP Path needs to be a valid integer or url for example https://beeming.dev!");
        return Task.CompletedTask;
      }
      return ExposePortPrivate(args, "http ");
    }

    internal const string ExposeHttpCmd = "[http] <url> as <ngrok-site>";
    internal static Task ExposeHttp(string[] args)
    {
      return ExposePortPrivate(args, "http ");
    }

    private static async Task ExposePortPrivate(string[] args, string command)
    {
      var hostHeader = "localhost";
      string portNumberOrHttpPath = args[1];
      if (!portNumberOrHttpPath.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
      {
        if (!int.TryParse(args[1], out int portNumber))
        {
          App.ShowMenuWithError("Port Number or HTTP Path needs to be a valid integer or url for example https://beeming.dev!");
          return;
        }
        command += $"https://localhost:{portNumber}";
      }
      else
      {
        if (portNumberOrHttpPath.IndexOf("://") == -1)
        {
          App.ShowMenuWithError("Port Number or HTTP Path needs to be a valid integer or url for example https://beeming.dev!");
          return;
        }
        hostHeader = portNumberOrHttpPath.Remove(0, portNumberOrHttpPath.IndexOf("://") + 3);
      }
      var subDomain = Settings.CleanseSubDomain(args[3]);
      if (string.IsNullOrWhiteSpace(subDomain))
      {
        App.ShowMenuWithError("Subdomain is required and can only contain letters, numbers dashes (-) or underscores (_).");
        return;
      }
      var subdomainInclude = await Settings.GetNGrokSubDomainInclude();
      subDomain = $"{subDomain}{subdomainInclude}";
      args[3] = subDomain;
      var response = await AddBindingToCloudflare(args);
      if (!response.success)
      {
        return;
      }

      var ngrokDnsWildCard = await Settings.GetNGrokDnsWildCardRecord();
      var ngrokRegion = await Settings.GetNGrokRegionCode() ?? "us";

      var handle = GetConsoleWindow();
      ShowWindow(handle, SW_HIDE);

      var process = Process.Start("ngrok", $@"{command}{portNumberOrHttpPath} -hostname {subDomain}.{ngrokDnsWildCard} -region ""{ngrokRegion}"" --host-header {hostHeader} --bind-tls ""true""");
      process.WaitForExit();

      ShowWindow(handle, SW_SHOW);
    }

    private static async Task<PasteClasses.CreateOrUpdateDnsRecordResponse> AddBindingToCloudflare(string[] args)
    {
      var response = new PasteClasses.CreateOrUpdateDnsRecordResponse { success = false, };
      var zoneId = await Settings.GetCloudflareZoneId();
      if (zoneId == null)
      {
        App.ShowMenuWithError("Cloudflare ZoneId not set!");
        return response;
      }
      var ngrokDnsWildCard = await Settings.GetNGrokDnsWildCardRecord();
      var cnameValue = await Settings.GetCloudflareCNameContent();
      if (string.IsNullOrEmpty(cnameValue))
      {
        var record = await CloudflareService.GetRecord(zoneId, $"*.{ngrokDnsWildCard}");
        if (record == null)
        {
          return response;
        }
        cnameValue = record.content;
      }
      var dnsSubDomain = args[3];
      response = await CloudflareService.CreateOrUpdateDnsRecord(zoneId, $"{dnsSubDomain}.{ngrokDnsWildCard}", DnsRecordType.CNAME, cnameValue, true);
      return response;
    }
  }
}
