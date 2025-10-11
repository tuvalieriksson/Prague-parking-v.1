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
    Console.WriteLine("6. Overview map (colors)");
    Console.WriteLine("7. List all cars");
    Console.WriteLine("8. List all motorcycles");
    Console.WriteLine("9. List empty spots");
    Console.WriteLine("10. Generate MC optimization work orders");
    Console.WriteLine("0. Exit");
    Console.Write("Your choice: ");

    string choice = (Console.ReadLine() ?? "").Trim();

    switch (choice)
    {
        case "1": ParkVehicle(); break;
        case "2": RemoveVehicle(); break;
        case "3": MoveVehicle(); break;
        case "4": SearchVehicle(); break;
        case "5": PrintParkingLot(); break;
        case "6": ShowOverview(); break;
        case "7": ListCars(); continue;
        case "8": ListMotorcycles(); continue;
        case "9": ListEmptySpots(); continue;
        case "10": GenerateMcOptimizationWorkOrders(); continue;
        case "0": return;
        default: Console.WriteLine("Invalid choice, please try again."); break;
    }

    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
}

bool IsValidReg(string reg)
{
    if (string.IsNullOrEmpty(reg)) return false;
    foreach (char c in reg)
    {
        if (!char.IsLetterOrDigit(c) && !"ÅÄÖÜ".Contains(c))
            return false;
    }
    return true;
}

bool IsCarSpot(string? spot) => !string.IsNullOrEmpty(spot) && spot!.StartsWith("CAR#");
bool IsMcSpot(string? spot) => !string.IsNullOrEmpty(spot) && spot!.StartsWith("MC#");

string[] GetMcTokens(string spot) => spot.Split('|', StringSplitOptions.RemoveEmptyEntries);

bool MatchVehicle(string? spot, string reg)
{
    if (string.IsNullOrEmpty(spot)) return false;
    if (spot == $"CAR#{reg}") return true;
    if (IsMcSpot(spot))
    {
        foreach (var token in GetMcTokens(spot))
            if (token == $"MC#{reg}") return true;
    }
    return false;
}

bool Exists(string reg)
{
    for (int i = 0; i < parkingLot.Length; i++)
        if (MatchVehicle(parkingLot[i], reg)) return true;
    return false;
}

SpotStatus GetSpotStatus(string? spot)
{
    if (string.IsNullOrEmpty(spot)) return SpotStatus.Empty;
    if (IsCarSpot(spot)) return SpotStatus.Car;
    if (IsMcSpot(spot))
    {
        var n = GetMcTokens(spot!).Length;
        return n >= 2 ? SpotStatus.Mc2 : SpotStatus.Mc1;
    }
    return SpotStatus.Empty;
}

(int empty, int mc1, int mc2, int cars) CountStatuses()
{
    int e = 0, m1 = 0, m2 = 0, c = 0;
    for (int i = 0; i < parkingLot.Length; i++)
    {
        switch (GetSpotStatus(parkingLot[i]))
        {
            case SpotStatus.Empty: e++; break;
            case SpotStatus.Mc1: m1++; break;
            case SpotStatus.Mc2: m2++; break;
            case SpotStatus.Car: c++; break;
        }
    }
    return (e, m1, m2, c);
}

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
    if (Array.Exists(tokens, t => t == $"MC#{reg}"))
        return false;

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


void ParkVehicle()
{
    string type;
    while (true)
    {
        Console.Write("Enter vehicle type (CAR/MC): ");
        type = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

        if (type == "CAR" || type == "MC")
            break;

        Console.WriteLine("Invalid vehicle type. Use CAR or MC.");
    }

    string regNr;
    while (true)
    {
        Console.Write("Enter registration number: ");
        regNr = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

        if (IsValidReg(regNr))
            break;

        Console.WriteLine("Invalid registration number. Use only letters (A–Z, ÅÄÖÜ) and digits (0–9), no spaces.");
    }


    if (Exists(regNr))
    {
        Console.WriteLine($"Vehicle {regNr} is already parked.");
        return;
    }

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

    if (!IsValidReg(regNr))
    {
        Console.WriteLine("Invalid registration number. Use only letters (A–Z, ÅÄÖÜ) and digits (0–9), no spaces.");
        return;
    }

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

    if (!IsValidReg(regNr))
    {
        Console.WriteLine("Invalid registration number. Use only letters (A–Z, ÅÄÖÜ) and digits (0–9), no spaces.");
        return;
    }

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

    int newPlace;
    while (true)
    {
        Console.Write("Enter new spot (1–100): ");
        if (int.TryParse(Console.ReadLine(), out newPlace) && newPlace >= 1 && newPlace <= 100)
            break;

        Console.WriteLine("Invalid input. Please enter a number between 1 and 100.");
    }


    int target = newPlace - 1;

    if (isCar)
    {
        if (!string.IsNullOrEmpty(parkingLot[target]))
        {
            Console.WriteLine("Invalid spot: destination is occupied.");
            return;
        }

        parkingLot[target] = parkingLot[oldIndex];
        parkingLot[oldIndex] = null;
        Console.WriteLine($"The vehicle has been moved to spot {newPlace}.");
    }
    else
    {
        var dest = parkingLot[target];

        if (string.IsNullOrEmpty(dest))
        {
            RemoveMcFromSpot(oldIndex, regNr);
            parkingLot[target] = $"MC#{regNr}";
            Console.WriteLine($"The vehicle has been moved to spot {newPlace}.");
            return;
        }

        if (IsCarSpot(dest))
        {
            Console.WriteLine("Invalid spot: occupied by a car.");
            return;
        }

        var tokens = GetMcTokens(dest!);
        if (tokens.Length >= 2)
        {
            Console.WriteLine("Invalid spot: already has two motorcycles.");
            return;
        }

        RemoveMcFromSpot(oldIndex, regNr);
        if (!TryAddMcToSpot(target, regNr))
        {
            Console.WriteLine("Invalid spot: could not place motorcycle at destination.");
            return;
        }
        Console.WriteLine($"The vehicle has been moved to spot {newPlace}.");
    }
}

void SearchVehicle()
{
    Console.Write("Enter registration number: ");
    string regNr = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

    if (!IsValidReg(regNr))
    {
        Console.WriteLine("Invalid registration number. Use only letters (A–Z, ÅÄÖÜ) and digits (0–9), no spaces.");
        return;
    }

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
void ShowOverview()
{
    Console.Clear();
    Console.WriteLine("=== Overview (E=Empty, m=1 MC, M=2 MC, C=Car) ===\n");

    for (int i = 0; i < parkingLot.Length; i++)
    {
        var st = GetSpotStatus(parkingLot[i]);

        if (st == SpotStatus.Empty)
            Console.ForegroundColor = ConsoleColor.Green;
        else if (st == SpotStatus.Mc1)
            Console.ForegroundColor = ConsoleColor.Yellow;
        else if (st == SpotStatus.Mc2)
            Console.ForegroundColor = ConsoleColor.Red;
        else if (st == SpotStatus.Car)
            Console.ForegroundColor = ConsoleColor.Blue;

        string symbol = st == SpotStatus.Empty ? "E" :
                        st == SpotStatus.Mc1 ? "m" :
                        st == SpotStatus.Mc2 ? "M" : "C";

        Console.Write($"[{symbol}]");
        Console.ResetColor();

        if ((i + 1) % 10 == 0)
            Console.WriteLine($"   spots {i - 8:D2}-{i + 1:D2}");
    }

    var (e, m1, m2, c) = CountStatuses();
    Console.WriteLine($"\n\nEmpty:{e}  Half(1 MC):{m1}  Two-MC:{m2}  Cars:{c}");
    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
}

void ListCars()
{
    Console.Clear();
    Console.WriteLine("=== All cars ===\n");
    bool any = false;
    for (int i = 0; i < parkingLot.Length; i++)
    {
        var spot = parkingLot[i];
        if (IsCarSpot(spot))
        {
            Console.WriteLine($"Spot {i + 1}: {spot}");
            any = true;
        }
    }
    if (!any) Console.WriteLine("No cars parked.");
    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
}

void ListMotorcycles()
{
    Console.Clear();
    Console.WriteLine("=== All motorcycles ===\n");
    bool any = false;
    for (int i = 0; i < parkingLot.Length; i++)
    {
        var spot = parkingLot[i];
        if (!IsMcSpot(spot)) continue;
        foreach (var token in GetMcTokens(spot!))
        {
            Console.WriteLine($"Spot {i + 1}: {token}");
            any = true;
        }
    }
    if (!any) Console.WriteLine("No motorcycles parked.");
    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
}

void ListEmptySpots()
{
    Console.Clear();
    Console.WriteLine("=== Empty spots ===\n");
    bool any = false;
    for (int i = 0; i < parkingLot.Length; i++)
    {
        if (string.IsNullOrEmpty(parkingLot[i]))
        {
            Console.Write($"{i + 1} ");
            any = true;
        }
    }
    if (!any) Console.Write("—");
    Console.WriteLine("\n\nPress any key to continue...");
    Console.ReadKey();
}

void GenerateMcOptimizationWorkOrders()
{
    Console.Clear();
    Console.WriteLine("=== MC optimization work orders ===\n");

    var singles = new System.Collections.Generic.List<int>();
    for (int i = 0; i < parkingLot.Length; i++)
        if (GetSpotStatus(parkingLot[i]) == SpotStatus.Mc1) singles.Add(i);

    if (singles.Count < 2)
    {
        Console.WriteLine("No optimization needed (fewer than two single-MC spots).");
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
        return;
    }

    int orders = 0;
    int left = 0;
    int right = singles.Count - 1;

    while (left < right)
    {
        int to = singles[left];
        int from = singles[right];

        string fromToken = GetMcTokens(parkingLot[from]!)[0];
        string reg = fromToken.Substring(3);

        Console.WriteLine($"Move MC {reg} from spot {from + 1} to spot {to + 1}");
        orders++;
        left++;
        right--;
    }

    Console.WriteLine($"\nTotal moves suggested: {orders}");
    Console.WriteLine("\nNote: These are work orders only. Staff will perform the moves.");
    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
}

enum SpotStatus { Empty, Mc1, Mc2, Car }

