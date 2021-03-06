﻿using System;
using System.Linq;
using System.Collections.Generic;

namespace PhantessaCliProto
{
    class Program
    {
        static string help = "Commands:\n"
                            +"help   -- Displays this message\n"
                            +"add    -- Adds a new record to the database\n"
                            +"page   -- Lists the page number\n"
                            +"list   -- Lists the records on the current page\n"
                            +"edit   -- Edits the entry for a record\n"
                            +"Note: the record number to edit is the num of the record on the current page or set of search results\n"
                            +"next   -- Go to the next page\n"
                            +"back   -- Go to the previous page\n"
                            +"goto   -- Go to a page\n"
                            +"search -- Search all records and return records with names, shelfs, or artists that contain the search string\n"
                            +"Note: Search results act as a sort of virtual page, but you are still 'on' the page you were on before the \nsearch, so list, next, back, and goto will clear the search results\n"
                            +"Also, edit and goto will take a num as an arg and skip the prompt for record/page num, and everything after \nthe search command will be treated as the search string";



        static List<Record> GetPage(int page) {
            using var db = new InventoryContext(); 
            return (from record in db.Records
                    orderby record.Name, record.RecordId
                    select record).ToList();
        }

        static void ListPage(int page) {
            var records = GetPage(page);
            Console.WriteLine($"Page {page + 1}: ");
            var pageContent = records.Skip(page * 10).Take(10);
            int i = 0;
            foreach (Record record in pageContent) {
                Console.WriteLine($"{i + 1}: {record.Name}\nBy: {record.Artist}\nOn Shelf: {record.Shelf}\n");
                i++;
            }
        }
        
        static List<Record> SearchRecords(string searchTerm) {
            using var db = new InventoryContext();
            return (from record in db.Records
                    where record.Name.Contains(searchTerm) || record.Artist.Contains(searchTerm) || record.Shelf.Contains(searchTerm)
                    select record).ToList();
        }

        static string GetInputWithDefault(string prompt, string ifnone) {
            Console.Write(prompt);
            string input = Console.ReadLine();
            if (input == "") {
                input = ifnone;
            }
            return input;
        }
        
        static void AddRecord() {
            Console.WriteLine("Enter the records info (or enter for 'Unknown')");
            string Name = GetInputWithDefault("Name: ", "Unknown");
            string Artist = GetInputWithDefault("Artist: ", "Unknown");
            string Shelf = GetInputWithDefault("Shelf: ", "Unknown");
            using (var db = new InventoryContext()) {
                db.Add(new Record { Name = Name, Artist = Artist, Shelf = Shelf});
                db.SaveChanges();
            }
        }

        static void EditRecord(Record record) {
            using var db = new InventoryContext();
            db.Update(record);
            while (true) {
                Console.WriteLine($"Name: {record.Name}\nArtist: {record.Artist}\nShelf: {record.Shelf}\n");
                Console.Write("Enter the property you would like to edit or done to save and exit editing? ");
                string prop = Console.ReadLine();
                if (prop == "done") {
                    db.SaveChanges();
                    break;
                }
                Console.Write("What would you like to set it to? ");
                string val = Console.ReadLine();
                switch (prop) {
                    case "name": record.Name = val; break;
                    case "artist": record.Artist = val; break;
                    case "shelf": record.Shelf = val; break;
                    default: Console.WriteLine("not a property"); break;
                }
            }
        }

        static void Main(string[] args)
        {

            int page = 0;
            List<Record> currentContent = GetPage(page);
            ListPage(page);

            while (true) {
                Console.Write("Enter a command: ");
                string input = Console.ReadLine();
                string[] inputArr = input.Split(' ');
                
                if (inputArr[0] == "done") {
                    break;
                }

                switch (inputArr[0]) {
                    case "help": Console.WriteLine(help); break;
                    case "add": AddRecord(); break;
                    case "page": Console.WriteLine($"On page {page + 1}"); break;
                    case "list": currentContent = GetPage(page); ListPage(page); break;
                    case "edit":
                        string recordNumStr;
                        if (inputArr.Length == 1) {
                            Console.Write("Enter number of record to edit: ");
                            recordNumStr = Console.ReadLine();
                        } else {
                            recordNumStr = inputArr[1];
                        }
                        int recordNum;
                        if (!Int32.TryParse(recordNumStr, out recordNum)) {
                            Console.WriteLine("Not a number");
                            break;
                        }
                        if (recordNum < 0 || recordNum > currentContent.Count) {
                            Console.WriteLine("Number out of range");
                            break;
                        }
                        EditRecord(currentContent[recordNum - 1]);
                        break;
                    case "next": page++; currentContent = GetPage(page); ListPage(page); break;
                    case "back": page--; currentContent = GetPage(page); ListPage(page); break;
                    case "goto": 
                        string pageNumStr;
                        if (inputArr.Length == 1) {
                            Console.Write("Enter page number to go to: ");
                            pageNumStr = Console.ReadLine();
                        } else {
                            pageNumStr = inputArr[1];
                        }
                        int pageNum;
                        if (!Int32.TryParse(pageNumStr, out pageNum)) {
                            Console.WriteLine("Not a number");
                            break;
                        }
                        if (pageNum < 0) {
                            Console.WriteLine("Number out of range");
                            break;
                        }
                        page = pageNum - 1;
                        currentContent = GetPage(page);
                        ListPage(page);
                        break;
                    case "search":
                        string searchString;
                        if (inputArr.Length == 1) {
                            Console.Write("Enter record name to search: ");
                            searchString = Console.ReadLine();
                        } else {
                            List<String> inputList = inputArr.ToList();
                            inputList.RemoveAt(0);
                            searchString = String.Join(" ", inputList);
                        }
                        currentContent = SearchRecords(searchString);
                        int i = 0;
                        foreach (Record record in currentContent) {
                            Console.WriteLine($"{i + 1}: {record.Name}\nBy: {record.Artist}\nOn Shelf: {record.Shelf}\n");
                            i++;
                        }
                        break;
                    default: Console.WriteLine("Not a command (maybe you added an incorrect arg?)"); break;
                }
            }
            
            #if DEBUG
            Console.Write("Press any key to continue...");
            Console.ReadKey();
            #endif
        }
    }
}