
namespace FinalCoffee1.Modules.Admin.service;
using FinalCoffee1.Modules.Staff.model;
using FinalCoffee1.common.helperClass;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using FinalCoffee1.common.helperServices;
using FinalCoffee1.Common.model;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;

public class AdminServiceModel
{
    private readonly AuthService authentication;

    List<StaffModel> staffList;
    public AdminServiceModel(AuthService authentication)
    {
        this.authentication = authentication;
    }


    public async Task readStaff()
    {
        try
        {
            var path = new FileManager().DirectoryPath("database", "staff.json");
            if (File.Exists(path))
            {
                var existingData = await File.ReadAllTextAsync(path);
                Trace.WriteLine("This is Existing Data: " + existingData);
                staffList = JsonSerializer.Deserialize<List<StaffModel>>(existingData) ?? new List<StaffModel>();
            }
        }
        catch (Exception error)
        {
            Trace.WriteLine("This is Error: " + error.Message);
        }

    }
    public async Task<List<StaffModel>> getStaffList()
    {
        await this.readStaff();
        Trace.WriteLine("This is staff List: " + this.staffList);
        return this.staffList;
    }
public async Task<CusType> Edit(int id, UserModel model, string fileName)
{
    try
    {
        var path = new FileManager().DirectoryPath("database", fileName);
        if (!File.Exists(path))
        {
            return new CusType { Success = false, Message = "File not found" };
        }

        var existingData = await File.ReadAllTextAsync(path);
        var list = JsonSerializer.Deserialize<List<UserModel>>(existingData) ?? new List<UserModel>();
        var itemToEdit = list.FirstOrDefault(c => c.Id == id);

        if (itemToEdit == null)
        {
            return new CusType { Success = false, Message = "Not Found" };
        }

        itemToEdit.Username = model.Username ?? itemToEdit.Username;
        itemToEdit.Password = model.Password != null ? this.authentication.GenerateHash(model.Password) : itemToEdit.Password;

        int index = list.FindIndex(c => c.Id == id);
        list[index] = itemToEdit;

        var jsonData = JsonSerializer.Serialize(list);
        await File.WriteAllTextAsync(path, jsonData);

        return new CusType { Success = true, Message = "Updated Successfully" };
    }
    catch (Exception error)
    {
        return new CusType { Success = false, Message = error.Message };
    }
}

    [Obsolete]
    public async Task<CusType> GenerateReport(DateTime date, string reportType)
    {
        try
        {
            var FileManager = new FileManager();
            var path = FileManager.DirectoryPath("database", "orderData.json");

            if (File.Exists(path))
            {
                var existingData = await File.ReadAllTextAsync(path);
                var list = JsonSerializer.Deserialize<List<OrdarModel>>(existingData) ?? new List<OrdarModel>();

                List<OrdarModel> orders;
                string reportTitle;

                if (reportType == "daily")
                {
                    orders = list.Where(order => order.Date.Date == date.Date).ToList();
                    reportTitle = $"Coffee Report of Date: {date.Date.ToString("d")}";
                }
                else if (reportType == "monthly")
                {
                    orders = list.Where(order => order.Date.Month == date.Month && order.Date.Year == date.Year).ToList();
                    reportTitle = $"Coffee Report for Month: {date.Month}/{date.Year}";
                }
                else
                {
                    Trace.WriteLine("Invalid report type");
                    return new CusType { Success = false, Message = "Invalid report type" };
                }

                if (orders.Any())
                {
                    var totalRevenue = orders.Sum(order =>
                        order.TotalPrice);

                    string reportDirPath = reportType == "daily"
                        ? FileManager.DirectoryPath("reports", "days")
                        : FileManager.DirectoryPath("reports", "month");
                    if (!Directory.Exists(reportDirPath))
                    {
                        Trace.WriteLine("Creating reports folder");
                        Directory.CreateDirectory(reportDirPath);
                    }

                    string reportPath = reportType == "daily"
                        ? Path.Combine(reportDirPath, $"OrderReport_{date.Month}_{date.Year}_{date.Day}.pdf")
                        : Path.Combine(reportDirPath, $"OrderReport_{date.Month}_{date.Year}.pdf");

                    Trace.WriteLine("Report Path: " + reportPath);
                    Document.Create(container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(1, Unit.Centimetre);
                            page.PageColor(Colors.White);
                            page.DefaultTextStyle(x => x.FontSize(12));
                            page.Header()
                                .Padding(10)
                                .Text(reportTitle)
                                .SemiBold().FontSize(24).FontColor(Colors.Blue.Medium);

                            page.Content()
                                .PaddingVertical((float)0.5, Unit.Centimetre)
                                .Column(column =>
                                {
                                    column.Item().Text($"Total Revenue: {totalRevenue}").FontSize(16).FontColor(Colors.Green.Darken1);

                                    if (reportType == "daily")
                                    {
                                        var coffeeGroup = orders
                                            .SelectMany(order => order.CoffeeData)
                                            .GroupBy(coffee => coffee.CoffeeType)
                                            .OrderByDescending(group => group.Count())
                                            .Take(5)
                                            .ToList();

                                        var addInsGroup = orders
                                            .SelectMany(order => order.CoffeeData.SelectMany(coffee => coffee.AddIns))
                                            .GroupBy(addIn => addIn.Name)
                                            .OrderByDescending(group => group.Count())
                                            .Take(5)
                                            .ToList();

                                        column.Item().Text("Top 5 coffee types sold").FontSize(16).FontColor(Colors.Red.Darken1);
                                        column.Item().Grid(grid =>
                                        {
                                            grid.Columns(2);
                                            grid.Item().Text("Coffee Type").FontColor(Colors.Blue.Medium).SemiBold();
                                            grid.Item().Text("Quantity").FontColor(Colors.Blue.Medium).SemiBold();
                                            foreach (var group in coffeeGroup)
                                            {
                                                grid.Item().Text(group.Key);
                                                grid.Item().Text(group.Count().ToString());
                                            }
                                        });

                                        column.Item().Text("Top 5 Add-ins sold").FontSize(16).FontColor(Colors.Red.Darken1);
                                        column.Item().Grid(grid =>
                                        {
                                            grid.Columns(2);
                                            grid.Item().Text("Add-ins Name").FontColor(Colors.Blue.Medium).SemiBold();
                                            grid.Item().Text("Quantity").FontColor(Colors.Blue.Medium).SemiBold();
                                            foreach (var addInGroup in addInsGroup)
                                            {
                                                grid.Item().Text(addInGroup.Key);
                                                grid.Item().Text(addInGroup.Count().ToString());
                                            }
                                        });
                                    }
                                });

                            page.Footer()
                                .AlignCenter()
                                .Padding(10)
                                .Text(x =>
                                {
                                    x.Span("Page ");
                                    x.CurrentPageNumber().FontColor(Colors.Grey.Medium);
                                });
                        });
                    })
                    .GeneratePdf(reportPath);

                    return new CusType { Success = true, Message = "Report generated" };
                }
                else
                {
                    Trace.WriteLine($"No orders for the specified {reportType}");
                    return new CusType { Success = false, Message = $"No orders for the specified {reportType}" };
                }
            }
            return new CusType { Success = false, Message = "File not found" };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return new CusType { Success = false, Message = $"An error occurred: {ex.Message}" };
        }
    }
    public async Task<(List<OrdarModel>, decimal)> GetSalesTransactionsAndTotalRevenue()
{
    try
    {
        var path = new FileManager().DirectoryPath("database", "orderData.json");
        if (File.Exists(path))
        {
            var existingData = await File.ReadAllTextAsync(path);
            var list = JsonSerializer.Deserialize<List<OrdarModel>>(existingData) ?? new List<OrdarModel>();

            var totalRevenue = list.Sum(order => order.TotalPrice);

            return (list, totalRevenue);
        }
        else
        {
            Trace.WriteLine("File not found");
            return (null, 0);
        }
    }
    catch (Exception error)
    {
        Trace.WriteLine("An error occurred: " + error.Message);
        return (null, 0);
    }
}
}