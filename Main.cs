Console.WriteLine("\n\n\n");

// leer archivo e imprimirlo como referencia
string filename = Path.Combine(Directory.GetCurrentDirectory(), "pruebas/", "test.tb");
string content = File.ReadAllText(filename);
Console.WriteLine($"////\n{content}\n////");
Console.WriteLine();

var symtable = new SymbolTable();
var lexer = new Lexer(content);
var sintaxer = new Syntaxer(lexer, symtable);
var semantixer = new Semantixer(symtable);
var ast = sintaxer.Analizar();
semantixer.Analizar(ast);

//Console.WriteLine();
//foreach (var item in ast.Instrucciones) Console.WriteLine(item);

Console.WriteLine();
symtable.Print();


public static class G {
	public const StringComparison ic = StringComparison.OrdinalIgnoreCase;
	public static string SintaxError(int linea, string detalles) => $"!> Error sintantico en linea {linea}: {detalles}";
	public static void Print(string line, ConsoleColor color = ConsoleColor.Blue) {
		Console.ForegroundColor = color;
		Console.WriteLine(line);
		Console.ResetColor();
	}
}