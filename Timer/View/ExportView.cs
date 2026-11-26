using System.Globalization;
using Spectre.Console;
using Timer.Helpers;
using Timer.Models;
using Timer.Service;

namespace Timer.View
{
    /// <summary>
    /// View of Export option
    /// </summary>
    public class ExportView
    {
        private readonly CsvServices _csvServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportView"/> class.
        /// </summary>
        /// <param name="csvServices">The import export service.</param>
        public ExportView(CsvServices csvServices)
        {
            this._csvServices = csvServices;
        }

        /// <summary>
        /// Exports the user's task history to a CSV file using a record limit or date range.
        /// </summary>
        /// <param name="user">The currently authenticated user whose records are being exported.</param>
        public void TriggerHistoryExport(User user)
        {
            AnsiConsole.Clear();
            NotificationHelper.Display();

            AnsiConsole.MarkupLine("[bold]EXPORT HISTORY DATA[/]");
            AnsiConsole.Write(new Rule());

            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select export mode:")
                    .AddChoices(
                        "Export recent records",
                        "Export by date range",
                        "Back"));

            string destinationPath = string.Empty;

            try
            {
                switch (choice)
                {
                    case "Export recent records":
                        string input = AnsiConsole.Prompt(
                            new TextPrompt<string>(
                                "Enter the number of recent records (leave empty for all):")
                                .DefaultValue(string.Empty)
                                .AllowEmpty());

                        int? limit = null;

                        if (!string.IsNullOrWhiteSpace(input))
                        {
                            if (int.TryParse(input, out int parsedLimit) && parsedLimit > 0)
                            {
                                limit = parsedLimit;
                            }
                            else
                            {
                                AnsiConsole.MarkupLine(
                                    "[yellow]Invalid number. Exporting all records instead.[/]");
                            }
                        }

                        destinationPath = this._csvServices.ExportTasksToCsv(
                            user.UserId,
                            user.Name,
                            limit);

                        break;

                    case "Export by date range":
                        if (!DateOnly.TryParseExact(
                            AnsiConsole.Ask<string>("Enter start date (yyyy-MM-dd):"),
                            "yyyy-MM-dd",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out DateOnly startDate))
                        {
                            AnsiConsole.MarkupLine("[red]Invalid start date.[/]");
                            this.WaitForUser();
                            return;
                        }

                        if (!DateOnly.TryParseExact(
                            AnsiConsole.Ask<string>("Enter end date (yyyy-MM-dd):"),
                            "yyyy-MM-dd",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out DateOnly endDate))
                        {
                            AnsiConsole.MarkupLine("[red]Invalid end date.[/]");
                            this.WaitForUser();
                            return;
                        }

                        if (startDate > endDate)
                        {
                            AnsiConsole.MarkupLine(
                                "[yellow]Start date cannot be after end date.[/]");

                            this.WaitForUser();
                            return;
                        }

                        destinationPath = this._csvServices.ExportTasksToCsv(
                            user.UserId,
                            user.Name,
                            startDate,
                            endDate);

                        break;

                    case "Back":
                        return;
                }

                AnsiConsole.MarkupLine("\n[green]Export completed successfully.[/]");
                AnsiConsole.MarkupLine(
                    $"File location: {Markup.Escape(destinationPath)}");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine(
                    $"[red]Export failed: {Markup.Escape(ex.Message)}[/]");
            }

            this.WaitForUser();
        }

        /// <summary>
        /// Waits for the user to press a key before continuing.
        /// </summary>
        private void WaitForUser()
        {
            AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
            Console.ReadKey(true);
        }
    }
}
