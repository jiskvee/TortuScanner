Console.WriteLine("\n\n\n");

// leer archivo e imprimirlo como referencia
string filename = Path.Combine(Directory.GetCurrentDirectory(), "pruebas/", "test.tb");
string content = File.ReadAllText(filename);
Console.WriteLine(new string('/', 55));
Console.WriteLine(content);
Console.WriteLine(new string('/', 55));

// analisis lexico
var symtable = new SymbolTable();
var lexer = new Lexer(content);

// analisis sintactico
var sintaxer = new Syntaxer(lexer, symtable);
var ast = sintaxer.Analizar();
if (sintaxer.Errores.Count > 0) return;

// analisis semantico
var semantixer = new Semantixer(symtable);
semantixer.Analizar(ast);
if (semantixer.Errores.Count > 0) return;

// generacion de codigo intermedio
var cuadruplexer = new Cuadruplexer(symtable);
var cuads = cuadruplexer.Generar(ast);

// generacion de codigo objeto
var emuexer = new Emuexer(symtable);
var asm = emuexer.Generar(cuads);
Console.WriteLine(asm);

G.Print("\nTabla de simbolos:", ConsoleColor.Cyan);
symtable.Print();

//Console.WriteLine();
//foreach (var item in ast.Instrucciones) Console.WriteLine(item);


public static class G {
	public const StringComparison ic = StringComparison.OrdinalIgnoreCase;
	public static void Print(string line, ConsoleColor color = ConsoleColor.Blue) {
		Console.ForegroundColor = color;
		Console.WriteLine(line);
		Console.ResetColor();
	}
	public static void Print(string line, bool should) {
		if (should == false) return;
		Console.ForegroundColor = ConsoleColor.Blue;
		Console.WriteLine(line);
		Console.ResetColor();
	}
}