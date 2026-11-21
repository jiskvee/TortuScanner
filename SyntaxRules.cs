public partial class Syntaxer {

	// PROGRAMA → LINEAS
	NodoPrograma ParsePrograma() => ParseLineas();

	// LINEAS → LINEA LINEAS | epsilon
	NodoPrograma ParseLineas() {
		var nodo = new NodoPrograma();
		while (curtoken != TokenType.EOF) {
			var instruccion = ParseLinea();
			if (instruccion != null)
				nodo.Instrucciones.Add(instruccion);
		}
		return nodo;
	}

	// LINEA → INSTRUCCION SALTO | SALTO
	AstNode? ParseLinea() {
		// linea vacia
		if (curtoken == TokenType.Salto) {
			Consumir(TokenType.Salto);
			return null;
		}
		if (curtoken == TokenType.EOF) return null;

		var inst = ParseInstruccion();
		if (curtoken == TokenType.Salto)
			Consumir(TokenType.Salto);
		else if (curtoken != TokenType.EOF)
			ModoPanico($"Se esperaba salto de linea o fin de instruccion, pero se encontro '{curtoken.Lexema}'");
		return inst;
	}

	// INSTRUCCION → DECLARACION | ASIGNACION | ENTRADA | SALIDA | CONDICIONAL | CICLO_WHILE
	AstNode? ParseInstruccion() {
		if (curtoken == TokenType.Reservada) {
			var l = curtoken.Lexema.ToUpper();
			AstNode? node = l switch {
				"DIM" => ParseDeclaracion(),
				"LET" => ParseAsignacion(),
				"INPUT" => ParseEntrada(),
				"PRINT" => ParseSalida(),
				"IF" => ParseCondicional(),
				"WHILE" => ParseCicloWhile(),
				_ => null
			};
			if (node != null) return node;
			// palabras como ELSE, ENDIF, AS y esas que van dentro de otros bloques
			// no deberian llegar solas a esta parte, si si entonces error
			ModoPanico($"Instruccion inesperada o fuera de lugar '{curtoken.Lexema}'");
			return null;
		}
		ModoPanico($"Se esperaba inicio de instruccion, se encontro '{curtoken.Lexema}'");
		return null;
	}


	// DECLARACION → DIM id AS TIPO
	NodoDIM ParseDeclaracion() {
		string tipodato = "ERROR";
		Consumir(TokenType.Reservada, "DIM");
		var nombre = curtoken.Lexema;
		G.Print($"    [PARSING] Creacion de variable verificada: {curtoken.Lexema}, linea: {curtoken.Linea}", log);
		Consumir(TokenType.Identificador);
		Consumir(TokenType.Reservada, "AS");
		if (curtoken == TokenType.Reservada && (curtoken.Is("INT") || curtoken.Is("REAL"))) {
			tipodato = curtoken.Lexema.ToUpper();
			G.Print($"    [PARSING] Tipo de variable verificada: {tipodato}, linea: {curtoken.Linea}", log);
			Avanzar();
		} else
			ModoPanico($"Se esperaba tipo de dato valido (INT o REAL), pero se encontro '{curtoken.Lexema}'");
		return new NodoDIM{Linea = curtoken.Linea, NombreVariable = nombre, TipoDato = tipodato};
	}


	// ASIGNACION → LET id = EXPRESION
	NodoLET ParseAsignacion() {
		Consumir(TokenType.Reservada, "LET");
		G.Print($"    [PARSING] Asignacion a variable verificada: {curtoken.Lexema}, linea: {curtoken.Linea}", log);
		var nombre = curtoken.Lexema;
		Consumir(TokenType.Identificador);
		Consumir(TokenType.Igual);
		return new NodoLET{Variable = nombre, Linea = curtoken.Linea, Expresion = ParseExpresion()};
	}


	// EXPRESION → TERMINO EXPRESION'
	NodoExp ParseExpresion() {
		var izq = ParseTermino();
		izq.Linea = curtoken.Linea;
		return ParseExpresionPrima(izq);
	}

	// EXPRESION' → + TERMINO EXPRESION' | - TERMINO EXPRESION' | epsilon
	NodoExp ParseExpresionPrima(NodoExp izq) {
		if (curtoken == TokenType.Mas || curtoken == TokenType.Menos) {
			var op = curtoken.Lexema;
			Avanzar();
			var der = ParseTermino();
			var node = new NodoOperacion{Izquierda = izq, Operador = op, Derecha = der};
			return ParseExpresionPrima(node);
		}
		return izq;
	}

	// TERMINO → FACTOR TERMINO'
	NodoExp ParseTermino() {
		var izq = ParseFactor();
		return ParseTerminoPrima(izq);
	}

	// TERMINO' → * FACTOR TERMINO' | / FACTOR TERMINO' | epsilon
	NodoExp ParseTerminoPrima(NodoExp izq) {
		if (curtoken == TokenType.Por || curtoken == TokenType.Entre) {
			var op = curtoken.Lexema;
			Avanzar();
			var der = ParseTermino();
			var node = new NodoOperacion{Izquierda = izq, Operador = op, Derecha = der};
			return ParseTerminoPrima(node);
		}
		return izq;
	}

	// FACTOR → ( EXPRESION ) | id | num_entero | num_real
	NodoExp ParseFactor() {
		if (curtoken == TokenType.AbrirParentesis) {
			Avanzar(); // (
			var node = ParseExpresion();
			Consumir(TokenType.CerrarParentesis); // )
			return node;
		} else if (curtoken == TokenType.Identificador || curtoken == TokenType.NumEntero || curtoken == TokenType.NumReal) {
			G.Print($"    [PARSING] Expresion verificada: {curtoken.Lexema}, linea: {curtoken.Linea}", log);
			NodoExp node;
			if (curtoken == TokenType.Identificador) node = new NodoVariableRef{Nombre = curtoken.Lexema};
			else node = new NodoLiteral{Valor = curtoken.Lexema};
			Avanzar();
			return node;
		} else {
			ModoPanico($"Se esperaba id / num_real / num_entero / '(', se encontró '{curtoken.Lexema}'");
			return new NodoLiteral{Valor = "0"};
		}
	}


	// ENTRADA → INPUT id
	NodoINPUT ParseEntrada() {
		Consumir(TokenType.Reservada, "INPUT");
		G.Print($"    [PARSING] Leer variable {curtoken.Lexema}, linea: {curtoken.Linea}", log);
		var node = new NodoINPUT{Variable = curtoken.Lexema};
		Consumir(TokenType.Identificador);
		return node;
	}

	// SALIDA → PRINT LISTA_SALIDA
	NodoPRINT ParseSalida() {
		var node = new NodoPRINT();
		Consumir(TokenType.Reservada, "PRINT");
		node.Elementos = ParseListaSalida();
		return node;
	}

	// LISTA_SALIDA → ELEM_SALIDA RESTO_SALIDA
	List<AstNode> ParseListaSalida() {
		List<AstNode> elementos = [];
		var elem = ParseElemSalida();
		if (elem != null) elementos.Add(elem);
		ParseRestoSalida(elementos);
		return elementos;
	}
	// ELEM_SALIDA → cadena | EXPRESION
	AstNode ParseElemSalida() {
		if (curtoken == TokenType.Cadena) {
			G.Print($"    [PARSING] Cadena impresa {curtoken.Lexema}, linea: {curtoken.Linea}", log);
			var node = new NodoLiteral{Valor = curtoken.Lexema, EsCadena = true};
			Avanzar();
			return node;
		} else return ParseExpresion();
	}
	// RESTO_SALIDA → , ELEM_SALIDA RESTO_SALIDA | epsilon
	void ParseRestoSalida(List<AstNode> elementos) {
		if (curtoken == TokenType.Coma) {
			Avanzar(); // consumir ','
			var elem = ParseElemSalida();
			if (elem != null) elementos.Add(elem);
			ParseRestoSalida(elementos);
		}
	}


	// CONDICIONAL → IF CONDICION THEN SALTO LINEAS OPCIONAL_ELSE ENDIF
	NodoIF ParseCondicional() {
		Consumir(TokenType.Reservada, "IF");
		AbrirBloque("IF");
		var nodo = new NodoIF{Condicion = ParseCondicion()};
		Consumir(TokenType.Reservada, "THEN");
		Consumir(TokenType.Salto);
		while (curtoken != TokenType.EOF && !(curtoken == TokenType.Reservada && (curtoken.Is("ELSE") || curtoken.Is("ENDIF")))) {
			var l = ParseLinea();
			if (l is not null) nodo.CuerpoVerdadero.Add(l);
		}
		nodo.CuerpoFalso = ParseOpcionalElse();
		if (curtoken.Is("ENDIF")) CerrarBloque("IF");
		Consumir(TokenType.Reservada, "ENDIF");
		return nodo;
	}

	// OPCIONAL_ELSE → ELSE SALTO LINEAS | epsilon
	List<AstNode> ParseOpcionalElse() {
		List<AstNode> lineas = [];
		if (curtoken == TokenType.Reservada && curtoken.Is("ELSE")) {
			Consumir(TokenType.Reservada, "ELSE");
			while (curtoken != TokenType.EOF && !(curtoken == TokenType.Reservada && curtoken.Is("ENDIF"))) {
				var l = ParseLinea();
				if (l is not null) lineas.Add(l);
			}
		}
		return lineas;
	}

	// CONDICION → EXPRESION OP_REL EXPRESION
	NodoCondicion ParseCondicion() {
		return new NodoCondicion {
			Izquierda = ParseExpresion(),
			OperadorRelacional = ParseOpRel(),
			Derecha = ParseExpresion()
		};
	}

	string ParseOpRel() {
		if (curtoken.Tipo == TokenType.MenorQue ||
			curtoken.Tipo == TokenType.MenorIgual ||
			curtoken.Tipo == TokenType.MayorQue ||
			curtoken.Tipo == TokenType.MayorIgual ||
			curtoken.Tipo == TokenType.Igual ||
			curtoken.Tipo == TokenType.DiferenteDe) {
			G.Print($"    [PARSING] Operador relacional verificado: {curtoken.Lexema}, linea: {curtoken.Linea}", log);
			var op = curtoken.Lexema;
			Avanzar();
			return op;
		} else {
			ModoPanico($"Se esperaba un operador relacional (<, <=, >, >=, =, <>), pero se encontro '{curtoken.Lexema}'");
			return "=";
		}
	}


	// CICLO_WHILE → WHILE CONDICION SALTO LINEAS WEND
	NodoWHILE ParseCicloWhile() {
		Consumir(TokenType.Reservada, "WHILE");
		AbrirBloque("WHILE");
		var node = new NodoWHILE{Condicion = ParseCondicion()};
		Consumir(TokenType.Salto);
		while (curtoken != TokenType.EOF && !(curtoken == TokenType.Reservada && curtoken.Is("WEND"))) {
			var l = ParseLinea();
			if (l is not null) node.Cuerpo.Add(l);
		}
		if (curtoken.Is("WEND")) CerrarBloque("WHILE");
		Consumir(TokenType.Reservada, "WEND");
		return node;
	}
}