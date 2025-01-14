using HourDataProcessor;
using HourDataProcessor.Db;

DbInitializer.Initialize();
Processor processor = new Processor();
processor.Run();