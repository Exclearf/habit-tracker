using System.Data.SQLite;
using System.Globalization;

internal class Program
{
    private static string connectionString = @"Data Source=habit-Tracker.sqlite;Version=3;";

    private static void Main(string[] args)
    {
        SQLiteConnection.CreateFile("habit-Tracker.sqlite");

        

        using (var connection = new SQLiteConnection(connectionString))
        {
            
            connection.Open();

            string tableCmd =
                @"CREATE TABLE IF NOT EXISTS drinking_water (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Date TEXT,
                Quantity INTEGER
                )";

            var command = new SQLiteCommand(tableCmd, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }
        GetUserInput();
    }

    static void GetUserInput()
    {
        bool openApp = true;
        while (openApp)
        {
            Console.Clear();
            Console.WriteLine("\tMAIN MENU");
            Console.WriteLine("What would you like to do?");
            Console.WriteLine("Type 0 to close an application");
            Console.WriteLine("Type 1 to View All Records");
            Console.WriteLine("Type 2 to Insert Record");
            Console.WriteLine("Type 3 to Delete Record");
            Console.WriteLine("Type 4 to Edit Record");
            Console.WriteLine("-------------------------------");
            Console.Write("Enter the number: ");
            int choice = int.Parse(Console.ReadLine().Trim());

            switch(choice)
            {
                case 0:
                    Console.WriteLine("\nWas nice having you here!");
                    openApp = false;
                    Environment.Exit(0);
                    break;
                case 1:
                    GetAllRecords();
                    Console.ReadKey();
                    break;
                case 2:
                    InsertRecord();
                    Console.ReadKey();
                    break;
                case 3:
                    DeleteRecord();
                    Console.ReadKey();
                    break;
                case 4:
                    EditRecord();
                    break;
                default:
                    Console.WriteLine("\nWrong Input! Number must be between 1 and 4");
                    Console.WriteLine("Press any key to continue");
                    Console.ReadKey();
                    break;
            }
        }
    }

    private static void EditRecord()
    {
        Console.Clear();
        GetAllRecords();

        var recordId = GetNumberInput("Please enter the ID of the record: (0 to return to main menu)");

        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();

            string date = GetDateInput();
            int quantity = GetNumberInput("\n\nPlease, insert number of glasses or other measure of you choice:");

            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText =
                $"UPDATE drinking_water SET date = '{date}', quantity = {quantity} WHERE Id = {recordId}";

            if(Convert.ToInt32(tableCmd.ExecuteScalar()) == 0)
            {
                Console.WriteLine($"\n\nRecord with ID {recordId} does not exist.\n\n");
                connection.Close();
                EditRecord();
            }

            Console.WriteLine("\n\nSuccessully updated the record!\n\n");

            connection.Close();
        }
    }

    private static void DeleteRecord()
    {
        Console.Clear();
        GetAllRecords();

        var recordId = GetNumberInput("\n\nPlease enter the ID of the record: (0 to return to main menu)");

        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            var tablecmd = connection.CreateCommand();

            tablecmd.CommandText =
                $"DELETE from drinking_water WHERE Id = '{recordId}'";
            int rowcount = tablecmd.ExecuteNonQuery();

            if(rowcount == 0)
            {
                Console.WriteLine($"\n\nRecord with ID {recordId} does not exist. \n\n");
                DeleteRecord();
            }
            Console.WriteLine($"\n\nRecord with ID {recordId} was successfully deleted. Press any key to continue\n\n");

            connection.Close();
        }
    }

    private static void InsertRecord()
    {
        string date = GetDateInput();

        int quantity = GetNumberInput("\n\nPlease, insert number of glasses or other measure of you choice:");

        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();

            var tableCmd = connection.CreateCommand();

            tableCmd.CommandText =
                $"INSERT INTO drinking_water(date, quantity) VALUES('{date}', {quantity})";
            if (tableCmd.ExecuteNonQuery() > 0)
            {
                Console.WriteLine("\n\nSuccessfully inserted new entry\nPress any key to continue");
            }
            else
            {
                Console.WriteLine("Failed to insert a new entry!\nPress any key to continue");
            }

            connection.Close();
        }
    }

    private static int GetNumberInput(string message)
    {
        Console.WriteLine(message);

        int numberInput = int.Parse(Console.ReadLine());
        
        if(numberInput == 0) 
            GetUserInput();
        return numberInput;
    }

    private static string GetDateInput()
    {
        Console.Clear();
        Console.WriteLine("Please insert the date: (Format dd-mm-yy). Type 0 to return to main menu.");

        string dateInput = Console.ReadLine();

        while (!DateTime.TryParseExact(dateInput, "dd-mm-yy", new CultureInfo("en-US"), DateTimeStyles.None, out _))
        {
            if (dateInput == "0") GetUserInput();
            Console.Write("\n\nInvalid date. (Format: dd-mm-yy). Type 0 to return to main menu again: ");
            dateInput = Console.ReadLine();
        }

        return dateInput;
    }

    private static void GetAllRecords()
    {
        Console.Clear();

        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();

            var tableCmd = connection.CreateCommand();

            tableCmd.CommandText =
                $"SELECT * FROM drinking_water";

            List<DrinkingWater> tableData = new();

            SQLiteDataReader reader = tableCmd.ExecuteReader();

            if(reader.HasRows) {
                while(reader.Read())
                {
                    tableData.Add(
                        new DrinkingWater
                        {
                            Id = reader.GetInt32(0),
                            Date = DateTime.ParseExact(reader.GetString(1), "dd-MM-yy", new CultureInfo("en-US")),
                            Quantity = reader.GetInt32(2),
                        });
                }
            }
            else
            {
                Console.WriteLine("No records! Press any key to continue");
                return;
            }
             
            connection.Close();

            Console.WriteLine("--------------------------------");
            foreach (var dW in tableData)
            {
                Console.WriteLine($"{dW.Id} - {dW.Date.ToString("dd-MMM-yyyy")} - Quantity: {dW.Quantity}");
            }
            Console.WriteLine("--------------------------------");
        }
    }
}

internal class DrinkingWater
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int Quantity { get; set; }
}