using System;
using System.ServiceProcess;
using NLog;

namespace Abnormal_UI.Infra
{
    internal class SvcCtrl
    {
        private static readonly Logger _logger = LogManager.GetLogger("TestToolboxLog");

        public static bool StartService(string serviceName, int timeoutSeconds = 50)
        {
            var service = new ServiceController(serviceName);
            try
            {
                if (service.Status == ServiceControllerStatus.Running)
                {
                    return true;
                }
                var timeout = TimeSpan.FromSeconds(timeoutSeconds);
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                _logger.Debug("Started service {0}",serviceName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString(), "Got exception!");
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
                var timeout = TimeSpan.FromSeconds(timeoutSeconds);
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                _logger.Debug("Stopped service {0}", serviceName);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString(), "Got exception!");
                return false;
            }
        }

        public static bool RestartService(string serviceName, int timeoutSeconds = 30)
        {
            var service = new ServiceController(serviceName);
            try
            {
                var timeout = TimeSpan.FromSeconds(timeoutSeconds);
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                _logger.Debug("Restarted service {0}", serviceName);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString(), "Got exception!");
                return false;
            }
        }

    }
}
