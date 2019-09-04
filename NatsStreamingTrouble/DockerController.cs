using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NatsStreamingTrouble
{
    public class DockerController
    {
        public static void SendStopContainer(string containerName)
        {
            var processInfo = new ProcessStartInfo("docker", $"stop {containerName}");
            using (var process = new Process())
            {
                process.StartInfo = processInfo;
                process.Start();
            }
        }

        public static void SendStartContainer(string containerName)
        {
            var processInfo = new ProcessStartInfo("docker", $"start {containerName}");
            using (var process = new Process())
            {
                process.StartInfo = processInfo;
                process.Start();
            }
        }
    }
}
