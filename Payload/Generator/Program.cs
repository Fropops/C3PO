using Common.Payload;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Paylod Generation Test!");

//var gen = new PayloadGenerator(@"E:\Share\Projects\C2Sharp\Payload\Source");
//gen.GeneratePayload(@"E:\Share\Projects\C2Sharp\Payload\tes_pay_start.exe");

var gen = new PayloadGenerator(@"/mnt/Share/Projects/C2Sharp/Payload/Source");
gen.GeneratePayload(@"/mnt/Share/Projects/C2Sharp/Payload/tes_pay_lin.exe");
