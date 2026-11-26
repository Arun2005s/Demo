using Timer.Interfaces;
using Timer.Models;
using Timer.Repository;
using Timer.Service;
using Timer.Services;
using Timer.View;
using Timer.Views;

namespace Timer
{
    /// <summary>
    /// Entry point of the application.
    /// </summary>
    internal class Program
    {
        private static void Main(string[] args)
        {
            IUserRepository userRepository =
                new JsonUserRepository();

            ITaskRepository taskRepository =
                new JsonTaskRepository();

            AuthenticationServices authService =
                new AuthenticationServices(userRepository);

            TimerServices timerService =
                new TimerServices(taskRepository, userRepository);

            TaskHistoryServices taskHistoryService =
                new TaskHistoryServices(taskRepository);

            CsvServices csvServices = new CsvServices(taskRepository, taskHistoryService);

            AuthenticationView authView =
                new AuthenticationView(authService);

            AnalyticalDashboard analyticalDashboard = new AnalyticalDashboard(taskHistoryService);
            ExportView exportView = new ExportView(csvServices);
            ImportView importView = new ImportView(csvServices);
            TaskView taskView = new TaskView(timerService);
            TaskHistoryView taskHistoryView =
                new TaskHistoryView(taskHistoryService, timerService, csvServices);

            ProfileServices profileServices = new ProfileServices(userRepository);
            ProfileView profileView = new ProfileView(profileServices);
            Dashboard dashboard = new Dashboard(timerService, taskHistoryView, exportView, importView, analyticalDashboard, taskView, profileView);

            while (true)
            {
                User? loggedInUser = authView.ShowLoginPage();

                if (loggedInUser == null)
                {
                    break;
                }

                dashboard.ShowDashboard(loggedInUser);
            }

            Console.WriteLine("\nThank you for using Task Timer!");
        }
    }
}
