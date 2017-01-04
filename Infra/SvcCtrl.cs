using System;
using System.ServiceProcess;
using NLog;

namespace Abnormal_UI.Infra
{
    internal class SvcCtrl
    {
        private static readonly Logger Logger = LogManager.GetLogger("TestToolboxLog");

        public static bool StartService(string serviceName, int timeoutSeconds = 50)
        {
            var service = new ServiceController(serviceName);
            try
            {
                if (service.Status == ServiceControllerStatus.Running)
                {
                    return true;
                }
                Logger.Debug($"Starting service {serviceName}");
                var timeout = TimeSpan.FromSeconds(timeoutSeconds);
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                Logger.Debug($"Started service {serviceName}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString(), "Got exception!");
                return false;
            }
        }

        public static bool StopService(string serviceName, int timeoutSeconds = 20)
        {
            var service = new ServiceController(serviceName);
            if (service.Status == ServiceControllerStatus.Stopped)
            {
                return true;
            }
            try
            {
                Logger.Debug($"Stopping service {serviceName}");
                var timeout = TimeSpan.FromSeconds(timeoutSeconds);
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                Logger.Debug($"Stopped service {serviceName}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString(), "Got exception!");
                return false;
            }
        }

        public static bool RestartService(string serviceName, int timeoutSeconds = 40)
        {
            var service = new ServiceController(serviceName);
            try
            {
                var timeout = TimeSpan.FromSeconds(timeoutSeconds);
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                Logger.Debug($"Restarted service {serviceName}");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString(), "Got exception!");
                return false;
            }
        }

    }
}
