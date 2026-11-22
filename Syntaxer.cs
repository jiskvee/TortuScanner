public partial class Syntaxer {
	Lexer lexer;
	SymbolTable symtable;
	Token curtoken;
	bool log = false;

	public List<string> Errores = [];
	public List<string> ErroresLexicos = [];

	public Syntaxer(Lexer lex, SymbolTable table, bool logs = false) {
		lexer = lex;
		symtable = table;
		curtoken = new(TokenType.Comentario, "", 0);
		log = logs;
		Avanzar();
	}

	// pila para detectar bloques sin cerrar como IFs y WHILEs
	readonly Stack<(string Tipo, int Linea)> BloquesAbiertos = new();
	void AbrirBloque(string ins) => BloquesAbiertos.Push((ins, curtoken.Linea));
	void CerrarBloque(string ins) {
		if (BloquesAbiertos.Count > 0 && BloquesAbiertos.Peek().Tipo == ins) {
			BloquesAbiertos.Pop();
		}
	}

	// los errores van en una lista para ser reportados todos al final
	void ReportarError(string msg) {
		Errores.Add($"Error sintactico en linea {curtoken.Linea}: {msg}");
	}
	void ModoPanico(string msg) {
		ReportarError(msg);
		DescartarHastaSalto();
	}

	void Avanzar() {
		curtoken = lexer.NextToken(symtable);
		if ((int)curtoken.Tipo >= 500) ErroresLexicos.Add($"Linea {curtoken.Linea}: {curtoken.Tipo} en '{curtoken.Lexema}'");
		if (log) Console.WriteLine(curtoken);
	}

	private void Consumir(TokenType tipoEsperado) {
		if (curtoken.Tipo == tipoEsperado)
			Avanzar();
		else
			ModoPanico($"Se esperaba un token de tipo '{tipoEsperado}', pero se encontro '{curtoken.Tipo}' ('{curtoken.Lexema}')");
	}
	// para palabras reservadas (son lexemas especificos)
	private void Consumir(TokenType tipoEsperado, string lexemaEsperado) {
		if (curtoken.Tipo == tipoEsperado && curtoken.Is(lexemaEsperado))
			Avanzar();
		else
			ModoPanico($"Se esperaba '{lexemaEsperado}', pero se encontro '{curtoken.Lexema}'");
	}

	// aguas este es el metodo
	public NodoPrograma Analizar() {
		// Console.WriteLine("Iniciando analisis sintactico...");
		
		// HACER TODO
		var programa = ParsePrograma();

		// reportar bloques que dejaron abiertos
		while (BloquesAbiertos.Count > 0) {
			var (t, l) = BloquesAbiertos.Pop();
			ReportarError($"Estructura '{t}' abierta en la linea {l} nunca fue cerrada");
		}

		// esto no deberia pasar
		if (curtoken != TokenType.EOF)
			ReportarError($"Se esperaban instrucciones o fin de archivo, pero sobra token '{curtoken.Lexema}'");


		// fin de lexico, esto esta aqui por que nuestro Lexer no emite una lista de tokens,
		// el lexer no sabe nada asi que el metodo NextToken es como un bebe tomando un bloque
		// y pasandoselo al adulto (Syntaxer) para que haga las cosas complicadas
		Console.WriteLine();
		if (ErroresLexicos.Count > 0) {
			Console.WriteLine("=== ERRORES LEXICOS REPORTADOS ===");
			foreach (var err in ErroresLexicos)
				G.Print(err, ConsoleColor.Red);
			return programa;
		} else
			G.Print("Analisis lexico completado sin errores!! omg", ConsoleColor.Green);

		// fin de sintactico
		Console.WriteLine();
		if (Errores.Count > 0) {
			Console.WriteLine("=== ERRORES SINTACTICOS REPORTADOS ===");
			foreach (var err in Errores)
				G.Print(err, ConsoleColor.Red);
		} else
			G.Print("Analisis sintactico completado sin errores!! omg", ConsoleColor.Green);

		return programa;
	}

	private void DescartarHastaSalto() {
		while (curtoken != TokenType.EOF && curtoken != TokenType.Salto)
			Avanzar();
		if (curtoken == TokenType.Salto)
			Avanzar();
	}
}