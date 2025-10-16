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

bool IsCarSpot(string? spot) => !string.IsNullOrEmpty(spot) && spot!.StartsWith("CAR#");
bool IsMcSpot(string? spot) => !string.IsNullOrEmpty(spot) && spot!.StartsWith("MC#");

string[] GetMcTokens(string spot) => spot.Split('|', StringSplitOptions.RemoveEmptyEntries);

bool TryAddMcToSpot(int index, string reg)
{
    var spot = parkingLot[index];
    if (string.IsNullOrEmpty(spot))
    {
        parkingLot[index] = $"MC#{reg}";
        return true;
    }
    if (IsCarSpot(spot)) return false;

    var tokens = GetMcTokens(spot!);
    if (tokens.Length >= 2) return false;
    if (Array.Exists(tokens, t => t == $"MC#{reg}")) return false;

    parkingLot[index] = $"{spot}|MC#{reg}";
    return true;
}

bool RemoveMcFromSpot(int index, string reg)
{
    var spot = parkingLot[index];
    if (!IsMcSpot(spot)) return false;

    var tokens = GetMcTokens(spot!);
    var list = new System.Collections.Generic.List<string>(tokens);
    bool removed = list.Remove($"MC#{reg}");
    if (!removed) return false;

    if (list.Count == 0) parkingLot[index] = null;
    else if (list.Count == 1) parkingLot[index] = list[0];
    else parkingLot[index] = string.Join('|', list);
    return true;
}

bool MatchVehicle(string? spot, string reg)
{
    if (string.IsNullOrEmpty(spot)) return false;
    if (spot == $"CAR#{reg}") return true;
    if (IsMcSpot(spot))
    {
        foreach (var token in GetMcTokens(spot!))
            if (token == $"MC#{reg}") return true;
    }
    return false;
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

    if (type == "CAR")
    {
        for (int i = 0; i < parkingLot.Length; i++)
        {
            if (string.IsNullOrEmpty(parkingLot[i]))
            {
                parkingLot[i] = $"CAR#{regNr}";
                Console.WriteLine($"CAR#{regNr} parked in spot {i + 1}");
                return;
            }
        }
        Console.WriteLine("Sorry, the parking lot is full!");
        return;
    }
    else
    {
        for (int i = 0; i < parkingLot.Length; i++)
        {
            var spot = parkingLot[i];
            if (IsMcSpot(spot))
            {
                var tokens = GetMcTokens(spot!);
                if (tokens.Length == 1)
                {
                    if (TryAddMcToSpot(i, regNr))
                    {
                        Console.WriteLine($"MC#{regNr} parked (sharing) in spot {i + 1}");
                        return;
                    }
                }
            }
        }

        for (int i = 0; i < parkingLot.Length; i++)
        {
            if (string.IsNullOrEmpty(parkingLot[i]))
            {
                parkingLot[i] = $"MC#{regNr}";
                Console.WriteLine($"MC#{regNr} parked in spot {i + 1}");
                return;
            }
        }

        Console.WriteLine("Sorry, the parking lot is full!");
    }
}

void RemoveVehicle()
{
    Console.Write("Enter registration number to retrieve: ");
    string regNr = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

    for (int i = 0; i < parkingLot.Length; i++)
    {
        var spot = parkingLot[i];

        if (spot == $"CAR#{regNr}")
        {
            parkingLot[i] = null;
            Console.WriteLine($"Vehicle {regNr} has been retrieved.");
            return;
        }

        if (RemoveMcFromSpot(i, regNr))
        {
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
    bool isCar = false;

    for (int i = 0; i < parkingLot.Length; i++)
    {
        var spot = parkingLot[i];
        if (spot == $"CAR#{regNr}")
        {
            oldIndex = i;
            isCar = true;
            break;
        }
        if (IsMcSpot(spot))
        {
            foreach (var token in GetMcTokens(spot!))
            {
                if (token == $"MC#{regNr}")
                {
                    oldIndex = i;
                    isCar = false;
                    break;
                }
            }
            if (oldIndex != -1) break;
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
        (isCar ? string.IsNullOrEmpty(parkingLot[newPlace - 1])
               : (string.IsNullOrEmpty(parkingLot[newPlace - 1]) || (IsMcSpot(parkingLot[newPlace - 1]) && GetMcTokens(parkingLot[newPlace - 1]!).Length < 2))))
    {
        int target = newPlace - 1;

        if (isCar)
        {
            parkingLot[target] = parkingLot[oldIndex];
            parkingLot[oldIndex] = null;
            Console.WriteLine($"The vehicle has been moved to spot {newPlace}.");
        }
        else
        {
            if (string.IsNullOrEmpty(parkingLot[target]))
            {
                RemoveMcFromSpot(oldIndex, regNr);
                parkingLot[target] = $"MC#{regNr}";
            }
            else
            {
                RemoveMcFromSpot(oldIndex, regNr);
                TryAddMcToSpot(target, regNr);
            }
            Console.WriteLine($"The vehicle has been moved to spot {newPlace}.");
        }
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
        if (MatchVehicle(parkingLot[i], regNr))
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
