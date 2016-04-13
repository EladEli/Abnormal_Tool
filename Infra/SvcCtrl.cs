using System;
using System.ServiceProcess;
using NLog;

namespace Abnormal_UI.Infra
{
    class SvcCtrl
    {
        private static Logger _logger = LogManager.GetLogger("DavidTest");

        public static bool StartService(string serviceName, int timeoutSeconds = 10)
        {
            ServiceController service = new ServiceController(serviceName);
            try
            {
                TimeSpan timeout = TimeSpan.FromSeconds(timeoutSeconds);
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

        public static bool StopService(string serviceName, int timeoutSeconds = 10)
        {
            ServiceController service = new ServiceController(serviceName);
            try
            {
                TimeSpan timeout = TimeSpan.FromSeconds(timeoutSeconds);
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

        public static bool RestartService(string serviceName, int timeoutSeconds = 10)
        {
            ServiceController service = new ServiceController(serviceName);
            try
            {
                TimeSpan timeout = TimeSpan.FromSeconds(timeoutSeconds);
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
