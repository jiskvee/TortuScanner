public partial class Emuexer {
	const int escalaReal = 100;

	List<string> Data = [];
	List<string> Code = [];
	Dictionary<string, string> Cadenas = [];
	HashSet<string> Variables = [];
	HashSet<string> VarReales = [];
	int compcount = 0;

	public Emuexer(SymbolTable symtable) {
        foreach (var sym in symtable.Dict.Values) {
            if (string.Equals(sym.Tipo, "REAL", G.ic))
                VarReales.Add(MapearVariable(sym.Lexema));
        }
    }

	public string Generar(IEnumerable<Cuadruplo> cuadruplos) {
		Data.Clear();
		Code.Clear();
		Cadenas.Clear();
		Variables.Clear();
		VarReales.Clear();
		compcount = 0;

		foreach (var q in cuadruplos) {
			var vq = new Cuadruplo{
				Op = q.Op,
				Arg1 = MapearVariable(q.Arg1),
				Arg2 = MapearVariable(q.Arg2),
				Resultado = MapearVariable(q.Resultado)
			};
			RegistrarOperandos(vq);
			ProcesarCuadruplo(vq);
		}

		var s = new System.Text.StringBuilder();
		s.AppendLine(".MODEL SMALL");
		s.AppendLine(".STACK 100H");
		s.AppendLine();
		s.AppendLine(".DATA");
		foreach (var d in Data) s.AppendLine("    " + d);
		foreach (var c in Cadenas) s.AppendLine($"    {c.Value} DB {FormatearCadena(c.Key)}, 0DH, 0AH, '$'");
		s.AppendLine();
		s.AppendLine(".CODE");
		s.AppendLine("MAIN PROC");
		s.AppendLine("    MOV AX, @DATA");
		s.AppendLine("    MOV DS, AX");
		s.AppendLine();
		foreach (var linea in Code) s.AppendLine("    " + linea);
		s.AppendLine();
		s.AppendLine("    MOV AH, 4CH");
		s.AppendLine("    INT 21H");
		s.AppendLine("MAIN ENDP");
		s.AppendLine();
		EmitirBiblioteca(s);
		s.AppendLine("END MAIN");

		Console.WriteLine();
		G.Print("Assembly generado exitosamente!! padrisimo", ConsoleColor.Green);
		return s.ToString();
	}

	void RegistrarOperandos(Cuadruplo q) {
		if (q.Op == "TO_REAL" && q.Resultado != "_" && !string.IsNullOrWhiteSpace(q.Resultado))
            VarReales.Add(q.Resultado);

        if (EsReal(q.Arg1) || EsReal(q.Arg2)) {
            if (q.Op is "+" or "-" or "*" or "/" or "TO_REAL" or "SET") {
                if (q.Resultado != "_" && !string.IsNullOrWhiteSpace(q.Resultado))
                    VarReales.Add(q.Resultado);
            }
        }

		RegistrarSiVariable(q.Arg1);
		RegistrarSiVariable(q.Arg2);
		RegistrarSiVariable(q.Resultado);

		if (q.Arg1.StartsWith('"') && q.Arg1.EndsWith('"') && !Cadenas.ContainsKey(q.Arg1))
			Cadenas[q.Arg1] = $"str_{Cadenas.Count}";
	}

	void RegistrarSiVariable(string val) {
		if (string.IsNullOrEmpty(val) || val == "_") return; // nada o '_' que es nuestro simbolo de nada
		if (EsCadenaLiteral(val)) return;
		if (EsNumeroLiteral(val)) return;
		if (EsEtiqueta(val)) return;

		if (Variables.Add(val)) {
			Data.Add($"{val} DW 0");
		}
	}

	void ProcesarCuadruplo(Cuadruplo q) {
        bool resReal = EsReal(q.Resultado);
        bool opReal = resReal || EsReal(q.Arg1) || EsReal(q.Arg2);

		Code.Add($"; {q}");
        switch (q.Op) {
            case "SET":
                Code.Add($"MOV AX, {FormatOp(q.Arg1, resReal)}");
                Code.Add($"MOV {q.Resultado}, AX");
                break;
            case "TO_REAL":
                Code.Add($"MOV AX, {FormatOp(q.Arg1)}");
                Code.Add($"MOV BX, {escalaReal}");
                Code.Add("IMUL BX");
                Code.Add($"MOV {q.Resultado}, AX");
                break;
            case "+":
                Code.Add($"MOV AX, {FormatOp(q.Arg1, opReal)}");
                Code.Add($"ADD AX, {FormatOp(q.Arg2, opReal)}");
                Code.Add($"MOV {q.Resultado}, AX");
                break;
            case "-":
                Code.Add($"MOV AX, {FormatOp(q.Arg1, opReal)}");
                Code.Add($"SUB AX, {FormatOp(q.Arg2, opReal)}");
                Code.Add($"MOV {q.Resultado}, AX");
                break;
            case "*":
                Code.Add($"MOV AX, {FormatOp(q.Arg1, opReal)}");
                if (int.TryParse(FormatOp(q.Arg2, opReal), out _)) {
                    Code.Add($"MOV BX, {FormatOp(q.Arg2, opReal)}");
                    Code.Add("IMUL BX");
                } else
                    Code.Add($"IMUL {FormatOp(q.Arg2, opReal)}");
					
                if (opReal) {
                    Code.Add($"MOV BX, {escalaReal}");
                    Code.Add("CWD");
                    Code.Add("IDIV BX");
                }
                Code.Add($"MOV {q.Resultado}, AX");
                break;

            case "/":
                Code.Add($"MOV AX, {FormatOp(q.Arg1, opReal)}");
                if (opReal) {
                    Code.Add($"MOV BX, {escalaReal}");
                    Code.Add("IMUL BX");
                }
                Code.Add("CWD");
                if (int.TryParse(FormatOp(q.Arg2, false), out _)) {
                    Code.Add($"MOV BX, {FormatOp(q.Arg2, false)}");
                    Code.Add("IDIV BX");
                } else {
                    Code.Add($"IDIV {FormatOp(q.Arg2, false)}");
                }
                Code.Add($"MOV {q.Resultado}, AX");
                break;

            case "LABEL":
                Code.Add($"{q.Resultado}:");
                break;
            case "JMP":
                Code.Add($"JMP {q.Resultado}");
                break;
            case "JZ":
                Code.Add($"CMP {q.Arg1}, 0");
                Code.Add($"JE {q.Resultado}");
                break;
            case "JNZ":
                Code.Add($"CMP {q.Arg1}, 0");
                Code.Add($"JNE {q.Resultado}");
                break;
            case "READ":
                Code.Add("CALL LeerINT");
                if (resReal) {
                    Code.Add($"MOV BX, {escalaReal}");
                    Code.Add("IMUL BX");
                }
                Code.Add($"MOV {q.Resultado}, AX");
                break;
            case "WRITE":
                if (EsCadenaLiteral(q.Arg1) && Cadenas.TryGetValue(q.Arg1, out string? str)) {
                    Code.Add($"LEA DX, {str}");
                    Code.Add("MOV AH, 09H");
                    Code.Add("INT 21H");
                } else if (EsReal(q.Arg1)) {
                    Code.Add($"MOV AX, {FormatOp(q.Arg1, true)}");
                    Code.Add("CALL ImprimirREAL");
                } else {
                    Code.Add($"MOV AX, {FormatOp(q.Arg1)}");
                    Code.Add("CALL ImprimirINT");
                }
                break;
            case "<":
            case "<=":
            case ">":
            case ">=":
            case "=":
            case "<>":
                ProcesarComparacion(q);
                break;
        }
    }

	void ProcesarComparacion(Cuadruplo q) {
        var compReal = EsReal(q.Arg1) || EsReal(q.Arg2);
        string salto = q.Op switch {
            "<" => "JL",
            "<=" => "JLE",
            ">" => "JG",
            ">=" => "JGE",
            "<>" => "JNE",
            _ => "JE"
        };
        int id = compcount++;
        Code.Add($"MOV AX, {FormatOp(q.Arg1, compReal)}");
        Code.Add($"CMP AX, {FormatOp(q.Arg2, compReal)}");
        Code.Add($"{salto} cmp_v_{id}");
        Code.Add($"MOV {q.Resultado}, 0");
        Code.Add($"JMP cmp_f_{id}");
        Code.Add($"cmp_v_{id}:");
        Code.Add($"MOV {q.Resultado}, 1");
        Code.Add($"cmp_f_{id}:");
    }
}