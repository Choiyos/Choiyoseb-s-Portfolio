using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Open.Nat;
using UnityEngine;

public class HolePunching : MonoBehaviour
{
    public HolePunching()
    {
        Punch().Wait();
    }

    private Task Punch()
    {
        var nat = new NatDiscoverer();
        var cts = new CancellationTokenSource();
        cts.CancelAfter(5000);

        NatDevice device = null;
        var sb = new StringBuilder();
        IPAddress ip = null;

        return nat.DiscoverDeviceAsync(PortMapper.Upnp, cts)
            .ContinueWith(task =>
            {
                device = task.Result;
                return device.GetExternalIPAsync();

            })
            .Unwrap()
            .ContinueWith(task =>
            {
                ip = task.Result;
                sb.AppendFormat("\nYour IP: {0}", ip);
                return device.CreatePortMapAsync(new Mapping(Protocol.Tcp, 41426, 41426, 0, "Unity Game : Battle Maze (TCP)"));
            })
            .Unwrap()
            .ContinueWith(task =>
            {
                return device.CreatePortMapAsync(
                    new Mapping(Protocol.Udp, 41426, 41426, 0, "Unity Game : Battle Maze (Udp)"));
            })
            .Unwrap()
            .ContinueWith(task =>
            {
            });
    }
}
