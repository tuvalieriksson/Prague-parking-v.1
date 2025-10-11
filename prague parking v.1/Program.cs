string?[] parkingLot = new string?[100];
while (true)

{
    Console.Clear();
    Console.WriteLine("=== PRAGUE PARKING V1 ===");
    Console.WriteLine("1. Park a vehicle");
    Console.WriteLine("2. Retrieve a vehicle");
    Console.WriteLine("3. Move a vehicle");
    Console.WriteLine("4. Search for a vehicle");
    Console.WriteLine("5. Show all parking spots");
    Console.WriteLine("0. Exit");
    Console.Write("Your choice: ");

    string choice = Console.ReadLine() ?? "";


    switch (choice)
    {
        case "1": ParkVehicle(); break;
        case "2": RemoveVehicle(); break;
        case "3": MoveVehicle(); break;
        case "4": SearchVehicle(); break;
        case "5": PrintParkingLot(); break;
        case "0": return;
        default: Console.WriteLine("Invalid choice, please try again."); break;
    }

    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
}

void ParkVehicle()
{
    Console.Write("Enter vehicle type (CAR/MC): ");
    string type = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
    if (type != "CAR" && type != "MC")
    {
        Console.WriteLine("Invalid vehicle type.");
        return;
    }

    Console.Write("Enter registration number: ");
    string regNr = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

    string vehicle = type + "#" + regNr;

    for (int i = 0; i < parkingLot.Length; i++)
    {
        if (string.IsNullOrEmpty(parkingLot[i]))
        {
            parkingLot[i] = vehicle;
            Console.WriteLine($"{vehicle} parked in spot {i + 1}");
            return;
        }
    }

    Console.WriteLine("Sorry, the parking lot is full!");
}

void RemoveVehicle()
{
    Console.Write("Enter registration number to retrieve: ");
    string regNr = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

    for (int i = 0; i < parkingLot.Length; i++)
    {
        if (!string.IsNullOrEmpty(parkingLot[i]) &&
           (parkingLot[i] == $"CAR#{regNr}" || parkingLot[i] == $"MC#{regNr}"))

        {
            parkingLot[i] = null;
            Console.WriteLine($"Vehicle {regNr} has been retrieved.");
            return;
        }
    }

    Console.WriteLine("The vehicle was not found.");
}

void MoveVehicle()
{
    Console.Write("Enter registration number to move: ");
    string regNr = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

    int oldIndex = -1;
    for (int i = 0; i < parkingLot.Length; i++)
    {
        if (!string.IsNullOrEmpty(parkingLot[i]) &&
           (parkingLot[i] == $"CAR#{regNr}" || parkingLot[i] == $"MC#{regNr}"))

        {
            oldIndex = i;
            break;
        }
    }

    if (oldIndex == -1)
    {
        Console.WriteLine("The vehicle was not found.");
        return;
    }

    Console.Write("Enter new spot (1-100): ");
    if (int.TryParse(Console.ReadLine(), out int newPlace) &&
        newPlace >= 1 && newPlace <= 100 &&
        string.IsNullOrEmpty(parkingLot[newPlace - 1]))
    {
        parkingLot[newPlace - 1] = parkingLot[oldIndex];
        parkingLot[oldIndex] = null;
        Console.WriteLine($"The vehicle has been moved to spot {newPlace}.");
    }
    else
    {
        Console.WriteLine("Invalid spot.");
    }
}


void SearchVehicle()
{
    Console.Write("Enter registration number: ");
    string regNr = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

    for (int i = 0; i < parkingLot.Length; i++)
    {
        if (!string.IsNullOrEmpty(parkingLot[i]) &&
           (parkingLot[i] == $"CAR#{regNr}" || parkingLot[i] == $"MC#{regNr}"))

        {
            Console.WriteLine($"Vehicle {regNr} is parked in spot {i + 1}");
            return;
        }
    }

    Console.WriteLine("The vehicle is not in the parking lot.");
}


void PrintParkingLot()
{
    for (int i = 0; i < parkingLot.Length; i++)
    {
        string place = parkingLot[i] ?? "Empty";
        Console.WriteLine($"Spot {i + 1}: {place}");
    }
}