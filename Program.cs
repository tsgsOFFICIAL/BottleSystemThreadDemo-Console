using System;
using System.Threading;
using System.Collections.Generic;

namespace BottleSystemThreadDemo_Console
{
    class Program
    {
        static int _producerLimit = 100;
        static Queue<Item> _productionBuffer = new Queue<Item>(_producerLimit); // Creates and initializes a Queue with a capacity of 100
        static Queue<Item> _beerBuffer = new Queue<Item>(_producerLimit / 2); // Creates and initializes a Queue with a capacity of 10
        static Queue<Item> _sodaBuffer = new Queue<Item>(_producerLimit / 2); // Creates and initializes a Queue with a capacity of 10
        static void Main()
        {
            // Create threads
            Thread producerThread = new Thread(Produce);
            Thread splitterThread = new Thread(Split);
            Thread beerConsumerThread = new Thread(ConsumeBeer);
            Thread sodaConsumerThread = new Thread(ConsumeSoda);

            // Start threads
            producerThread.Start();
            splitterThread.Start();
            beerConsumerThread.Start();
            sodaConsumerThread.Start();
        }
        static void Produce()
        {
            while (true) // Run forever
            {
                while (_productionBuffer.Count < _producerLimit) // Run while productionBuffer count is less than producer limit
                {
                    Thread.Sleep(new Random().Next(50, 250 + 1)); // Sleep for a random amount of time, between 50ms and 300ms (inclusive, hence +1)
                    Monitor.Enter(_productionBuffer); // Lock the buffer
                    try
                    {
                        Item newItem = new Item((Item.Type)new Random().Next(0, 1 + 1)); // Initialize a new Item object of type Beer(0) or Soda(1)
                        _productionBuffer.Enqueue(newItem); // Enqueue the new Item to the productionBuffer queue
                        Console.ForegroundColor = ConsoleColor.Green; // Change console color
                        Console.WriteLine($"Producer => Buffer({_productionBuffer.Count}/{_producerLimit}) [{newItem.ItemType}]"); // Print to console
                    }
                    catch (Exception)
                    { }
                    finally
                    {
                        Monitor.Pulse(_productionBuffer); // Send a pulse to a waiting thread
                    }
                    if (_productionBuffer.Count >= _producerLimit) // If productionBuffer count is greater or equal to producerLimit
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow; // Change console color
                        Console.WriteLine($"Producer waits, Buffer({_productionBuffer.Count}/{_producerLimit}) is full"); // Print to console
                        Monitor.Wait(_productionBuffer); // Wait & Release the lock
                    }
                    Monitor.Exit(_productionBuffer); // Release the lock
                }
            }
        }
        static void Split()
        {
            while (true) // Run forever
            {
                // Run while productionBuffer is above 0 && beerBuffer is less than half the producerlimit or sodabuffer is liss than half the producerlimit
                while ((_productionBuffer.Count > 0) && (_beerBuffer.Count < _producerLimit / 2 || _sodaBuffer.Count < _producerLimit / 2))
                {
                    Thread.Sleep(new Random().Next(50, 300 + 1)); // Sleep for a random amount of time, between 50ms and 300ms (inclusive, hence +1)
                    Monitor.Enter(_productionBuffer); // Lock the buffer
                    try
                    {
                        Item retrievedItem = _productionBuffer.Dequeue(); // Retrieve the first Item in the Queue
                        if (retrievedItem.ItemType == Item.Type.Beer) // If its of type beer
                        {
                            // If theres room for one more, add it
                            if (_beerBuffer.Count < _producerLimit / 2)
                            {
                                retrievedItem.ItemPosition = Item.Position.BeerBuffer; // Set position to BeerBuffer
                                _beerBuffer.Enqueue(retrievedItem); // Add to beerBuffer queue

                                Console.ForegroundColor = ConsoleColor.Cyan; // Change console color
                                Console.WriteLine($"Splitter took 1 {retrievedItem.ItemType}, from {retrievedItem.OldItemPosition}, and placed it in {retrievedItem.ItemPosition}"); // Print
                                retrievedItem.OldItemPosition = Item.Position.Splitter; // Set old position, helps keep track of route
                            }
                            else
                            {
                                //Console.ForegroundColor = ConsoleColor.Yellow; // Change console color
                                //Console.WriteLine($"Splitter - Theres no more room for beer"); // Print to console
                                _productionBuffer.Enqueue(retrievedItem); // Put the item back
                            }
                        }
                        else if (retrievedItem.ItemType == Item.Type.Soda) // Or if of type soda
                        {
                            // If theres room for one more, add it
                            if (_sodaBuffer.Count < _producerLimit / 2)
                            {
                                retrievedItem.ItemPosition = Item.Position.SodaBuffer; // Set position to BeerBuffer
                                _sodaBuffer.Enqueue(retrievedItem); // Add to beerBuffer queue

                                Console.ForegroundColor = ConsoleColor.Cyan; // Change console color
                                Console.WriteLine($"Splitter took 1 {retrievedItem.ItemType}, from {retrievedItem.OldItemPosition}, and placed it in {retrievedItem.ItemPosition}"); // Print
                                retrievedItem.OldItemPosition = Item.Position.Splitter; // Set old position, helps keep track of route
                            }
                            else
                            {
                                //Console.ForegroundColor = ConsoleColor.Yellow; // Change console color
                                //Console.WriteLine($"Splitter - Theres no more room for soda"); // Print to console
                                _productionBuffer.Enqueue(retrievedItem); // Put the item back
                            }
                        }
                    }
                    catch (Exception)
                    { }
                    finally
                    {
                        Monitor.Pulse(_productionBuffer); // Send a pulse to a waiting thread
                    }
                    // Productionbuffer count is less than or equal to 0
                    if (_productionBuffer.Count <= 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow; // Change console color
                        Console.WriteLine($"Splitter waits, Buffer({_productionBuffer.Count}/{_producerLimit}) is empty"); // Print to console
                        Monitor.Wait(_productionBuffer); // Wait & release the lock
                    }
                    // beerBuffer count is greater or equal to half the producer limit or sodaBuffer count is greater or eqaul to half the producer limit
                    else if (_beerBuffer.Count >= _producerLimit / 2 && _sodaBuffer.Count >= _producerLimit / 2)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow; // Change console color
                        Console.WriteLine($"Splitter waits\nbeerBuffer({_beerBuffer.Count}/{_producerLimit / 2}) is full\nsodaBuffer({_sodaBuffer.Count}/{_producerLimit / 2}) is full"); // Print to console
                        Monitor.Wait(_productionBuffer); // Wait & release the lock
                    }
                    Monitor.Exit(_productionBuffer); // Release the lock
                }
            }
        }
        static void ConsumeBeer()
        {
            while (true) // Run forever
            {
                Thread.Sleep(new Random().Next(50, 300 + 1)); // Sleep for a random amount of time, between 50ms and 300ms (inclusive, hence +1)
                Monitor.Enter(_beerBuffer); // Lock the buffer
                try
                {
                    Item retrievedItem = _beerBuffer.Dequeue();
                    retrievedItem.ItemPosition = Item.Position.RecycleBin;

                    Console.ForegroundColor = ConsoleColor.Blue; // Change console color
                    Console.WriteLine($"beerConsumer took 1 {retrievedItem.ItemType}, from {retrievedItem.OldItemPosition}, and placed it in {retrievedItem.ItemPosition}"); // Print
                    retrievedItem.OldItemPosition = Item.Position.BeerBuffer; // Set old position, helps keep track of route
                }
                catch (Exception)
                { }
                finally
                {
                    Monitor.Exit(_beerBuffer);
                }
            }
        }
        static void ConsumeSoda()
        {
            while (true) // Run forever
            {
                Thread.Sleep(new Random().Next(50, 300 + 1)); // Sleep for a random amount of time, between 50ms and 300ms (inclusive, hence +1)
                Monitor.Enter(_sodaBuffer); // Lock the buffer
                try
                {
                    Item retrievedItem = _sodaBuffer.Dequeue();
                    retrievedItem.ItemPosition = Item.Position.RecycleBin;

                    Console.ForegroundColor = ConsoleColor.Blue; // Change console color
                    Console.WriteLine($"sodaConsumer took 1 {retrievedItem.ItemType}, from {retrievedItem.OldItemPosition}, and placed it in {retrievedItem.ItemPosition}"); // Print
                    retrievedItem.OldItemPosition = Item.Position.SodaBuffer; // Set old position, helps keep track of route
                }
                catch (Exception)
                { }
                finally
                {
                    Monitor.Exit(_sodaBuffer);
                }
            }
        }
    }
    /// <summary>
    /// Creates and manages all produces Items
    /// </summary>
    class Item
    {
        public static int Count { get; private set; } // Total count on all Item objects
        public Type ItemType { get; private set; } // Item Type
        public Position OldItemPosition { get; set; } // Last position on object
        public Position ItemPosition { get; set; } // Current position on object
        public enum Type
        {
            Beer,
            Soda
        }
        public enum Position
        {
            ProductionBuffer,
            Splitter,
            BeerBuffer,
            SodaBuffer,
            RecycleBin
        }
        public Item(Type type)
        {
            ItemType = type; // Set type
            ItemPosition = Position.ProductionBuffer;
            OldItemPosition = ItemPosition;
            Count++; // ++
        }
    }
}
