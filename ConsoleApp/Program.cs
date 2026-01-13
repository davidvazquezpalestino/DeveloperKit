using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DevKit.Extensions;

Console.OutputEncoding = System.Text.Encoding.UTF8;

TestRepo repo = new();

// Case 1: Async Task<List<Impuesto>>
Expression<Func<Task<List<Impuesto>>>> expr1 = () => repo.GetImpuestosAsync(1, "MX");
try {
    string key1 = ExpressionConditionExtractor.BuildRedisKey(expr1);
    Console.WriteLine($"Key 1 (Async List): {key1}");
} catch (Exception ex) { Console.WriteLine($"Error 1: {ex.Message}"); }

// Case 2: Sync List<Impuesto>
Expression<Func<List<Impuesto>>> expr2 = () => repo.GetImpuestos(2);
try {
    string key2 = ExpressionConditionExtractor.BuildRedisKey(expr2);
    Console.WriteLine($"Key 2 (Sync List): {key2}");
} catch (Exception ex) { Console.WriteLine($"Error 2: {ex.Message}"); }

// Case 3: Simple Async Task<int>
Expression<Func<Task<int>>> expr3 = () => repo.CountAsync();
try {
    string key3 = ExpressionConditionExtractor.BuildRedisKey(expr3);
    Console.WriteLine($"Key 3 (Async Int): {key3}");
} catch (Exception ex) { Console.WriteLine($"Error 3: {ex.Message}"); }

// Case 4: Page argument
try {
    string key4 = ExpressionConditionExtractor.BuildRedisKey(expr1, 5);
    Console.WriteLine($"Key 4 (With Page): {key4}");
} catch (Exception ex) { Console.WriteLine($"Error 4: {ex.Message}"); }

// Case 5: Complex type (ClienteRequest)
ClienteRequest request = new()
{
    NumeroPagina = 1,
    RegistrosPagina = 10,
    EmpresaID = 5,
    Search = "test"
};

Expression<Func<Task<List<Cliente>>>> expr5 = () => repo.GetClientesAsync(request);
try {
    string key5 = ExpressionConditionExtractor.BuildRedisKey(expr5);
    Console.WriteLine($"Key 5 (Complex Type): {key5}");
} catch (Exception ex) { Console.WriteLine($"Error 5: {ex.Message}"); }


class TestRepo
{
    public Task<List<Impuesto>> GetImpuestosAsync(int id, string code) => Task.FromResult(new List<Impuesto>());
    public List<Impuesto> GetImpuestos(int id) => new List<Impuesto>();
    public Task<int> CountAsync() => Task.FromResult(0);
    public Task<List<Cliente>> GetClientesAsync(ClienteRequest request) => Task.FromResult(new List<Cliente>());
}

class Impuesto { }

class Cliente { }

class ClienteRequest
{
    public int NumeroPagina { get; set; }
    public int RegistrosPagina { get; set; } = 10;
    public int EmpresaID { get; set; }
    public string Search { get; set; }
}