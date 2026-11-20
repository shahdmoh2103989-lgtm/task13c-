using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ASSLINQ
{
    // Custom comparer: two words are equal if they have the same characters
    class AnagramComparer : IEqualityComparer<string>
    {
        static string Sort(string s) => new string(s.OrderBy(c => c).ToArray());
        public bool Equals(string x, string y) => Sort(x) == Sort(y);
        public int GetHashCode(string s) => Sort(s).GetHashCode();
    }

    class Program
    {
        static void Show<T>(string title, IEnumerable<T> items)
        {
            Console.WriteLine($"\n--- {title} ---");
            foreach (var i in items) Console.WriteLine(i);
        }

        static void Main()
        {
            var products = ListGenerators.ProductList;
            var customers = ListGenerators.CustomerList;
            int[] arr = { 5, 4, 1, 3, 9, 8, 6, 7, 2, 0 };
            string[] words = File.ReadAllLines("dictionary_english.txt");

            // ============ Element Operators ============
            Console.WriteLine("\n--- E1 ---");
            Console.WriteLine(products.First(p => p.UnitsInStock == 0));
            Console.WriteLine("\n--- E2 ---");
            Console.WriteLine(products.FirstOrDefault(p => p.UnitPrice > 1000)?.ToString() ?? "null");
            Console.WriteLine("\n--- E3 ---");
            Console.WriteLine(arr.Where(n => n > 5).ElementAt(1));

            // ============ Aggregate Operators ============
            Console.WriteLine("\n--- A1 odd count ---");
            Console.WriteLine(arr.Count(n => n % 2 != 0));

            Show("A2 customers and orders count",
                customers.Select(c => new { c.CustomerID, OrdersCount = c.Orders.Count() }));

            Show("A3 categories and products count",
                products.GroupBy(p => p.Category)
                        .Select(g => new { Category = g.Key, ProductCount = g.Count() }));

            Console.WriteLine("\n--- A4 sum ---");
            Console.WriteLine(arr.Sum());

            Console.WriteLine("\n--- A5 total chars ---");
            Console.WriteLine(words.Sum(w => w.Length));
            Console.WriteLine("--- A6 shortest ---");
            Console.WriteLine(words.Min(w => w.Length));
            Console.WriteLine("--- A7 longest ---");
            Console.WriteLine(words.Max(w => w.Length));
            Console.WriteLine("--- A8 average ---");
            Console.WriteLine(words.Average(w => w.Length));

            Show("A9 total units in stock per category",
                products.GroupBy(p => p.Category)
                        .Select(g => new { Category = g.Key, TotalUnits = g.Sum(p => p.UnitsInStock) }));

            Show("A10 cheapest price per category",
                products.GroupBy(p => p.Category)
                        .Select(g => new { Category = g.Key, Cheapest = g.Min(p => p.UnitPrice) }));

            // A11 uses LET to store the minimum price of each category
            var a11 = from p in products
                      group p by p.Category into g
                      let minPrice = g.Min(p => p.UnitPrice)
                      select new { Category = g.Key, Products = g.Where(p => p.UnitPrice == minPrice) };
            Console.WriteLine("\n--- A11 products with lowest price per category ---");
            foreach (var g in a11)
            {
                Console.WriteLine(g.Category);
                foreach (var p in g.Products) Console.WriteLine("   " + p.ProductName + " : " + p.UnitPrice);
            }

            Show("A12 highest price per category",
                products.GroupBy(p => p.Category)
                        .Select(g => new { Category = g.Key, Highest = g.Max(p => p.UnitPrice) }));

            var a13 = products.GroupBy(p => p.Category)
                              .Select(g => new
                              {
                                  Category = g.Key,
                                  Products = g.Where(p => p.UnitPrice == g.Max(x => x.UnitPrice))
                              });
            Console.WriteLine("\n--- A13 products with highest price per category ---");
            foreach (var g in a13)
            {
                Console.WriteLine(g.Category);
                foreach (var p in g.Products) Console.WriteLine("   " + p.ProductName + " : " + p.UnitPrice);
            }

            Show("A14 average price per category",
                products.GroupBy(p => p.Category)
                        .Select(g => new { Category = g.Key, AveragePrice = g.Average(p => p.UnitPrice) }));

            // ============ Set Operators ============
            Show("S1 unique categories", products.Select(p => p.Category).Distinct());

            var productLetters = products.Select(p => p.ProductName[0]);
            var customerLetters = customers.Select(c => c.CustomerName[0]);

            Show("S2 unique first letters (union)", productLetters.Union(customerLetters));
            Show("S3 common first letters (intersect)", productLetters.Intersect(customerLetters));
            Show("S4 product letters not in customer letters (except)", productLetters.Except(customerLetters));

            // S5 Concat keeps duplicates
            Show("S5 last three characters of all names",
                products.Select(p => p.ProductName)
                        .Concat(customers.Select(c => c.CustomerName))
                        .Select(n => n.Length >= 3 ? n.Substring(n.Length - 3) : n));

            // ============ Partitioning Operators ============
            var waOrders = customers.Where(c => c.Region == "WA").SelectMany(c => c.Orders);

            Show("P1 first 3 orders from Washington", waOrders.Take(3));
            Show("P2 all but first 2 orders from Washington", waOrders.Skip(2));
            Show("P3 take while number >= position", arr.TakeWhile((n, i) => n >= i));
            Show("P4 skip until first element divisible by 3", arr.SkipWhile(n => n % 3 != 0));
            Show("P5 skip until first element less than its position", arr.SkipWhile((n, i) => n >= i));

            // ============ Quantifiers ============
            Console.WriteLine("\n--- Q1 any word contains 'ei' ---");
            Console.WriteLine(words.Any(w => w.Contains("ei")));

            var q2 = products.GroupBy(p => p.Category).Where(g => g.Any(p => p.UnitsInStock == 0));
            Console.WriteLine("\n--- Q2 categories with at least one out of stock product ---");
            foreach (var g in q2)
            {
                Console.WriteLine(g.Key);
                foreach (var p in g) Console.WriteLine("   " + p.ProductName);
            }

            var q3 = products.GroupBy(p => p.Category).Where(g => g.All(p => p.UnitsInStock > 0));
            Console.WriteLine("\n--- Q3 categories with all products in stock ---");
            foreach (var g in q3)
            {
                Console.WriteLine(g.Key);
                foreach (var p in g) Console.WriteLine("   " + p.ProductName);
            }

            // ============ Grouping Operators ============
            // G1 group numbers by remainder when divided by 5
            List<int> numbers = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            Console.WriteLine("\n--- G1 ---");
            foreach (var g in numbers.GroupBy(n => n % 5).OrderBy(g => g.Key))
            {
                Console.WriteLine($"Numbers with a remainder of {g.Key} when divided by 5:");
                foreach (var n in g) Console.WriteLine(n);
            }

            // G2 group dictionary words by first letter (prints the count to keep the output short)
            Console.WriteLine("\n--- G2 ---");
            foreach (var g in words.Where(w => w.Length > 0).GroupBy(w => char.ToLower(w[0])).OrderBy(g => g.Key))
                Console.WriteLine($"Words starting with '{g.Key}': {g.Count()}");

            // G3 group words that consist of the same characters (custom comparer)
            string[] arr3 = { "from", "salt", "earn", "last", "near", "form" };
            Console.WriteLine("\n--- G3 ---");
            foreach (var g in arr3.GroupBy(w => w, new AnagramComparer()))
            {
                foreach (var w in g) Console.WriteLine(w);
                Console.WriteLine("....");
            }
        }
    }
}