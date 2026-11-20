public class Lexer(string input)
{
	// matriz de transiciones
	readonly int[,] TRANSICIONES = new int[,]
	{
        { 1, 6, 500, 109, 110, 111, 112, 113, 114, 9, 10, 11, 2, 1, 1, 0, 0, 500 },
        { 1, 1, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 1, 1, 1, 100, 100, 100 },
        { 1, 1, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 1, 3, 1, 100, 100, 100 },
        { 1, 1, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 1, 1, 4, 100, 100, 100 },
        { 1, 1, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 1, 1, 1, 100, 5, 100 },
        { 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 0, 5, 5 },
        { 101, 6, 7, 101, 101, 101, 101, 101, 101, 101, 101, 101, 101, 101, 101, 101, 101, 101 },
        { 501, 8, 501, 501, 501, 501, 501, 501, 501, 501, 501, 501, 501, 501, 501, 501, 501, 501 },
        { 102, 8, 102, 102, 102, 102, 102, 102, 102, 102, 102, 102, 102, 102, 102, 102, 102, 102 },
        { 103, 103, 103, 103, 103, 103, 103, 103, 104, 103, 105, 103, 103, 103, 103, 103, 103, 103 },
        { 106, 106, 106, 106, 106, 106, 106, 106, 107, 106, 106, 106, 106, 106, 106, 106, 106, 106 },
        { 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 108, 11, 11, 11, 502, 11, 11 }
    };

	// usamos hash (brown) para las papas- digo, palabras reservadas
	// porque una busqueda hash es complejidad O(1), no ocupamos encriptar esto,
	// asi que realmente no hay razon para no usar hash
	// no es un diccionario porque no nos importan las llaves, solo los valores
	readonly static HashSet<string> RESERVADAS = new(StringComparer.OrdinalIgnoreCase) 
	{"LET", "DIM", "AS", "INT", "REAL", "INPUT", "PRINT", "IF", "THEN", "ELSE", "ENDIF", "WHILE", "WEND", "REM"};

	// set de tokens que se activan con caracteres externos, esto es para que no consuman ese caracter que los activo
	// ejemplo: '>' no se activa solo, ocupa otro caracter que le siga, sin esto, el token generado podria ser '>\n'
	readonly static HashSet<TokenType> RETRACTS = 
	[TokenType.Identificador, TokenType.NumEntero, TokenType.NumReal, TokenType.MenorQue, TokenType.MayorQue];

	// obtener la columna correspondiente de la matriz
	private static int GetColumn(char c)
	{
		if (c == 'R' || c == 'r') return 12;
		if (c == 'E' || c == 'e') return 13;
		if (c == 'M' || c == 'm') return 14;

		if (char.IsLetter(c)) return 0;
		if (char.IsDigit(c)) return 1;

		return c switch
		{
			'.' => 2,
			',' => 3,
			'+' => 4,
			'-' => 5,
			'*' => 6,
			'/' => 7,
			'=' => 8,
			'<' => 9,
			'>' => 10,
			'"' => 11,
			'\n' => 15,
			_ when char.IsWhiteSpace(c) => 16,
			_ => 17
		};
	}

	// el ciclo de NextToken termina en cuanto el archivo se termina de leer
	// eso puede causar que algun texto no se termine de procesar en algunos casos
	// como somos malvados, simplemente añadimos un salto de linea al final y se arregla jiji
	readonly string Input = input + '\n';
	int Linea = 1;
	int Pos;

	// la cosa
	public Token NextToken(SymbolTable symtable, bool log = false, bool hideIgnored = true)
	{
		int actual = 0;
		string lexema = "";
		while (Pos < Input.Length)
		{
			char c = Input[Pos];
			int columna = GetColumn(c);

			int siguiente = TRANSICIONES[actual, columna];
			if (log) Console.WriteLine($"    actual: {actual}, {columna} | siguiente: {siguiente}, {columna}");

			// si es 0, estamos en espacio blanco, salto de linea, o terminando un comentario
			if (siguiente == 0)
			{
				Pos++;
				if (c == '\n') {
					var l = Linea;
					Linea++;
					return new Token(TokenType.Salto, "\\n", l);
				}
				if (hideIgnored) {
					actual = 0;
					lexema = ""; // aqui descartamos el comentario
					continue;
				}
				return new(TokenType.Comentario, lexema, Linea);
			}

			if (siguiente >= 100)
			{
				// solo avanzamos si el token no esta en RETRACTS
				// asi nos aseguramos que este caracter si pertenezca al token
				if (!RETRACTS.Contains((TokenType)siguiente)) {
					lexema += c;
					Pos++;
				}

				// identificadores
				if (siguiente == 100)
				{
					// prevenir identificadores llamados REM como el sueño
					if (lexema.Equals("REM", G.ic))
					{
						if (hideIgnored) {
							actual = 0;
							continue; }
						return new(TokenType.Comentario, lexema, Linea);
					}

					// resto de reservadas o identificadores
					if (RESERVADAS.Contains(lexema))
						return new(TokenType.Reservada, lexema, Linea);
					
					symtable.AñadirLinea(lexema, Linea);
					return new(TokenType.Identificador, lexema, Linea);
				}

				// resto de tokens, shoutout a la super tabla
				TokenType token = (TokenType)siguiente;
				if (Enum.IsDefined(token))
					return new(token, lexema, Linea);
				
				// apocalipsis! un error que ni siquiera tenemos definido (deberia ser imposible)
				Console.WriteLine($"!!!> SUPER ERROR APOCALIPTICO: '{lexema} en '{c}'");
				return new(TokenType.Error_Apocalipsis, lexema, Linea);
			}
			Pos++;
			lexema += c;
			actual = siguiente;
		}

		// dame un caracter de ese archivo
		// se acabo, ese era el ultimo
		return new(TokenType.EOF, lexema, Linea);
		// pero tenia tantas ganas de leer ese archivo
	}
}