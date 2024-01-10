using System.Diagnostics;
using System.Text.Json;
using FinalCoffee1.common.helperClass;
using FinalCoffee1.Modules.Coffee.model;

namespace FinalCoffee1.Modules.Coffee.service;

public class CoffeeService
{
    List<CommonModel> coffeeList;
    List<CommonModel> addingList;
    int count = 0;
    public async Task<CusType> addCoffee(CommonModel Data, string fileName)
    {
        try
        {

            Trace.WriteLine("Calling Add Coffee");
            var path = new FileManager().DirectoryPath("database", fileName);
            Trace.WriteLine("This is Path: " + path);
            if (!File.Exists(path))
            {
                Trace.WriteLine("This is Existing Data: " + "No Data");
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                Data.Id = 1;
                coffeeList = new List<CommonModel> { Data };
            }
            else
            {
                var existingData = await File.ReadAllTextAsync(path);
                Trace.WriteLine("This is Existing Data: " + existingData);
                coffeeList = JsonSerializer.Deserialize<List<CommonModel>>(existingData) ?? new List<CommonModel>();
                Data.Id = coffeeList.Count + 1;
                coffeeList.Add(Data);
            }
            var jsonData = JsonSerializer.Serialize(coffeeList);
            await File.WriteAllTextAsync(path, jsonData);
            return new CusType { Success = true, Message = "Added" };
        }
        catch (Exception error)
        {
            return new CusType { Success = false, Message = error.Message };
        }
    }
    public async Task readData(string fileName)
    {
        try
        {
            Trace.WriteLine("Calling Read Data" + fileName);
            var path = new FileManager().DirectoryPath("database", fileName);
            if (File.Exists(path))
            {
                var existingData = await File.ReadAllTextAsync(path);
                Trace.WriteLine("This is Existing Data: " + existingData);
                var dataList = JsonSerializer.Deserialize<List<CommonModel>>(existingData) ?? new List<CommonModel>();

                if (fileName == "addins.json")
                {
                    addingList = dataList;
                }
                else if (fileName == "coffee.json")
                {
                    coffeeList = dataList;
                }
            }
        }
        catch (Exception error)
        {
            Trace.WriteLine("This is Error: " + error.Message);
        }
    }
    public async Task<List<CommonModel>> getList(string fileName)
    {
        await this.readData(fileName);
        if (fileName == "addins.json")
        {
            Trace.WriteLine("This is Adding List: " + this.addingList);
            return this.addingList;
        }
        else if (fileName == "coffee.json")
        {
            Trace.WriteLine("This is Coffee List: " + this.coffeeList);
            return this.coffeeList;
        }
        else
        {
            return new List<CommonModel>();
        }
    }


    public async Task<CusType> Edit(int id, CommonModel model, string fileName)
    {
        try
        {
            var path = new FileManager().DirectoryPath("database", fileName);
            if (File.Exists(path))
            {
                var existingData = await File.ReadAllTextAsync(path);
                var list = JsonSerializer.Deserialize<List<CommonModel>>(existingData) ?? new List<CommonModel>();
                var itemToEdit = list.FirstOrDefault(c => c.Id == id);
                Trace.WriteLine("This is ItemToEdit: " + itemToEdit);
                if (itemToEdit != null)
                {
                    if (itemToEdit.GetType().GetProperty("Name") != null)
                    {
                        itemToEdit.Name = model.Name;
                    }
                    if (itemToEdit.GetType().GetProperty("CoffeeType") != null)
                    {
                        itemToEdit.CoffeeType = model.CoffeeType;
                    }
                    if (itemToEdit.GetType().GetProperty("Size") != null)
                    {
                        itemToEdit.Size = model.Size;
                    }
                    if (itemToEdit.GetType().GetProperty("Price") != null)
                    {
                        itemToEdit.Price = model.Price;
                    }


                    int index = list.FindIndex(c => c.Id == id);
                    list[index] = itemToEdit;
                    var jsonData = JsonSerializer.Serialize(list);
                    await File.WriteAllTextAsync(path, jsonData);
                    return new CusType { Success = true, Message = "Updated" };
                }
                else
                {
                    return new CusType { Success = false, Message = "Item not found" };
                }
            }
            else
            {
                return new CusType { Success = false, Message = "File not found" };
            }
        }
        catch (Exception error)
        {
            return new CusType { Success = false, Message = error.Message };
        }
    }

}