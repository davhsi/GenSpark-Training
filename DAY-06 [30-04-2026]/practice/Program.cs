
class Program
{
    static void Main()
    {
        int[] numbers = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

        List<string> names = ["Alice", "Bob", "Charlie", "David"];


        // IEnumerable -> interface to create a collection that can be enumerated(iterated) 
        // but does not support modification
        IEnumerable<int> moreNumbers = [..numbers, 11, 12, 13];

        IEnumerable<string> empty = [];

        Console.WriteLine("Numbers:");
        foreach (var n in numbers)
        {
            Console.Write(n + " ");
        }

        Console.WriteLine("\n\nNames:");
        foreach (var name in names)
        {
            Console.Write(name + " ");
        }

        Console.WriteLine("\n\nMore Numbers:");
        foreach (var n in moreNumbers)
        {
            Console.Write(n + " ");
        }

        Console.WriteLine("\n\nEmpty collection:");
        Console.WriteLine(empty is not null ? "Initialized successfully" : "Null");


        Console.WriteLine(names[^2]); // prints the second last name (Charlie)
        Console.WriteLine(string.Join(", ", numbers[1..3])); // prints the elements at indices 1 and 2

        


    }
}