using System.Globalization;
using System.Text;

public partial class Emuexer {
	// like si dejarias que les cayera un yunque en la cabeza a los siguientes 3 metodos
	// spoiler: ||hmm yo si||
	bool EsReal(string val) {
		if (VarReales.Contains(val)) return true;
		if (val == "_" || string.IsNullOrWhiteSpace(val) || EsCadenaLiteral(val)) return false;
		if (val.Contains('.') && double.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
			return true;
		return false;
	}

	static bool EsCadenaLiteral(string val) =>
		!string.IsNullOrEmpty(val) && val.Length >= 2 && val.StartsWith('"') && val.EndsWith('"');

	static bool EsNumeroLiteral(string val) =>
		int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out _) ||
		double.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out _);


	//
	private string FormatOp(string val, bool forzarEscalaReal = false) {
        if (EsReal(val)) { // iinvariantculture porque al parecer puede dar los numeros con coma dependiendo del idioma?? babosada
            if (double.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out double valReal)) {
                int escalado = (int)Math.Round(valReal * escalaReal);
                return escalado.ToString(CultureInfo.InvariantCulture);
            }
            return val;
        }

        if (forzarEscalaReal && int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out int valInt))
            return (valInt * escalaReal).ToString(CultureInfo.InvariantCulture);

        return val;
    }

	// quiza haya que revisitar esto despues, no es suficiente para todos los casos
	// si la cadena tiene 2 caracteres o menos no sirve
	// hay perdida de datos por convertir " en ' sin distincion a ' reales
	// no toma en cuenta \ grrr quiza hay una libreria o algo asi para hacer esto no see
	/* private static string FormatearCadenaAsm(string literalConComillas) {
		string contenido = literalConComillas[1..^1].Replace("\"", "'");
		return $"\"{contenido}\"";
	} */
	// no encontre ninguna :(

	// nuevo nuevo nuevo nuevo
	// como odioooo este deberian hacer un lenguaje llamado poopssembly que sea super increible y
	// que sea bien wholesome y maneje las cadenas en la forma mas perfecta que ha visto el ser humano jamas
	static string FormatearCadena(string literal) {
		if (string.IsNullOrEmpty(literal)) return "\"\"";
		string interior = EsCadenaLiteral(literal) ? literal[1..^1] : literal;

		var s = new StringBuilder();
		bool enComillas = false;

		void Abrir() {
			if (!enComillas) {
				if (s.Length > 0) s.Append(", ");
				s.Append('"');
				enComillas = true;
			}
		}

		void Cerrar() {
			if (enComillas) {
				s.Append('"');
				enComillas = false;
			}
		}

		for (int i = 0; i < interior.Length; i++) {
			char c = interior[i];
			if (c == '\\' && i + 1 < interior.Length) {
				char esc = interior[++i];
				switch (esc) {
					case 'n':
						Cerrar();
						if (s.Length > 0) s.Append(", ");
						s.Append("0DH, 0AH");
						break;
					case 'r':
						Cerrar();
						if (s.Length > 0) s.Append(", ");
						s.Append("0DH");
						break;
					case 't':
						Cerrar();
						if (s.Length > 0) s.Append(", ");
						s.Append("09H");
						break;
					case '"':
						Cerrar();
						if (s.Length > 0) s.Append(", ");
						s.Append("22H");
						break;
					case '\'':
						Cerrar();
						if (s.Length > 0) s.Append(", ");
						s.Append("27H");
						break;
					case '\\':
						Cerrar();
						if (s.Length > 0) s.Append(", ");
						s.Append("5CH");
						break;
					default:
						Abrir();
						s.Append(esc);
						break;
				}
			} else if (c == '"') {
				Cerrar();
				if (s.Length > 0) s.Append(", ");
				s.Append("22H");
			} else {
				Abrir();
				s.Append(c);
			}
		}

		Cerrar();
		return s.Length == 0 ? "\"\"" : s.ToString();
	}


	// lo que va hasta abajo del archivo
	// confio en que el yo del semestre pasado hizo un buen trabajo con estas
	// solo le cambie los nombres jiji
	static void EmitirBiblioteca(StringBuilder s) {
		s.AppendLine("; Lee un entero con signo del teclado y lo pone en AX");
		s.AppendLine("LeerINT PROC");
		s.AppendLine("    PUSH BX");
		s.AppendLine("    PUSH CX");
		s.AppendLine("    PUSH DX");
		s.AppendLine("    PUSH SI");
		s.AppendLine("    MOV BX, 0");
		s.AppendLine("    MOV SI, 0");
		s.AppendLine("LI_SIG:");
		s.AppendLine("    MOV AH, 01H");
		s.AppendLine("    INT 21H");
		s.AppendLine("    CMP AL, 0DH");
		s.AppendLine("    JE LI_FIN");
		s.AppendLine("    CMP AL, '-'");
		s.AppendLine("    JNE LI_DIG");
		s.AppendLine("    MOV SI, 1");
		s.AppendLine("    JMP LI_SIG");
		s.AppendLine("LI_DIG:");
		s.AppendLine("    CMP AL, '0'");
		s.AppendLine("    JB LI_SIG");
		s.AppendLine("    CMP AL, '9'");
		s.AppendLine("    JA LI_SIG");
		s.AppendLine("    SUB AL, '0'");
		s.AppendLine("    MOV AH, 0");
		s.AppendLine("    PUSH AX");
		s.AppendLine("    MOV AX, BX");
		s.AppendLine("    MOV CX, 10");
		s.AppendLine("    IMUL CX");
		s.AppendLine("    POP BX");
		s.AppendLine("    ADD AX, BX");
		s.AppendLine("    MOV BX, AX");
		s.AppendLine("    JMP LI_SIG");
		s.AppendLine("LI_FIN:");
		s.AppendLine("    MOV AX, BX");
		s.AppendLine("    CMP SI, 0");
		s.AppendLine("    JE LI_SALIDA");
		s.AppendLine("    NEG AX");
		s.AppendLine("LI_SALIDA:");
		s.AppendLine("    PUSH AX");
		s.AppendLine("    MOV AH, 02H");
		s.AppendLine("    MOV DL, 0DH");
		s.AppendLine("    INT 21H");
		s.AppendLine("    MOV DL, 0AH");
		s.AppendLine("    INT 21H");
		s.AppendLine("    POP AX");
		s.AppendLine("    POP SI");
		s.AppendLine("    POP DX");
		s.AppendLine("    POP CX");
		s.AppendLine("    POP BX");
		s.AppendLine("    RET");
		s.AppendLine("LeerINT ENDP");
		s.AppendLine();
		s.AppendLine("; Imprime entero con signo que este en AX");
		s.AppendLine("ImprimirINT PROC");
		s.AppendLine("    PUSH AX");
		s.AppendLine("    PUSH BX");
		s.AppendLine("    PUSH CX");
		s.AppendLine("    PUSH DX");
		s.AppendLine("    CMP AX, 0");
		s.AppendLine("    JGE II_POS");
		s.AppendLine("    PUSH AX");
		s.AppendLine("    MOV AH, 02H");
		s.AppendLine("    MOV DL, '-'");
		s.AppendLine("    INT 21H");
		s.AppendLine("    POP AX");
		s.AppendLine("    NEG AX");
		s.AppendLine("II_POS:");
		s.AppendLine("    MOV CX, 0");
		s.AppendLine("    MOV BX, 10");
		s.AppendLine("II_DIV:");
		s.AppendLine("    XOR DX, DX");
		s.AppendLine("    DIV BX");
		s.AppendLine("    ADD DL, '0'");
		s.AppendLine("    PUSH DX");
		s.AppendLine("    INC CX");
		s.AppendLine("    CMP AX, 0");
		s.AppendLine("    JNE II_DIV");
		s.AppendLine("II_IMP:");
		s.AppendLine("    POP DX");
		s.AppendLine("    MOV AH, 02H");
		s.AppendLine("    INT 21H");
		s.AppendLine("    LOOP II_IMP");
		s.AppendLine("    MOV AH, 02H");
		s.AppendLine("    MOV DL, 0DH");
		s.AppendLine("    INT 21H");
		s.AppendLine("    MOV DL, 0AH");
		s.AppendLine("    INT 21H");
		s.AppendLine("    POP DX");
		s.AppendLine("    POP CX");
		s.AppendLine("    POP BX");
		s.AppendLine("    POP AX");
		s.AppendLine("    RET");
		s.AppendLine("ImprimirINT ENDP");
		s.AppendLine();
		s.AppendLine("; Imprime numero en AX escalado por 100");
		s.AppendLine("ImprimirREAL PROC");
		s.AppendLine("    PUSH AX");
		s.AppendLine("    PUSH BX");
		s.AppendLine("    PUSH CX");
		s.AppendLine("    PUSH DX");
		s.AppendLine("    CMP AX, 0");
		s.AppendLine("    JGE IR_POS");
		s.AppendLine("    PUSH AX");
		s.AppendLine("    MOV AH, 02H");
		s.AppendLine("    MOV DL, '-'");
		s.AppendLine("    INT 21H");
		s.AppendLine("    POP AX");
		s.AppendLine("    NEG AX");
		s.AppendLine("IR_POS:");
		s.AppendLine("    CWD");
		s.AppendLine("    MOV BX, 100");
		s.AppendLine("    IDIV BX");
		s.AppendLine("    PUSH DX");
		s.AppendLine("    ; Parte entera en AX");
		s.AppendLine("    MOV CX, 0");
		s.AppendLine("    MOV BX, 10");
		s.AppendLine("IR_EDIV:");
		s.AppendLine("    XOR DX, DX");
		s.AppendLine("    DIV BX");
		s.AppendLine("    ADD DL, '0'");
		s.AppendLine("    PUSH DX");
		s.AppendLine("    INC CX");
		s.AppendLine("    CMP AX, 0");
		s.AppendLine("    JNE IR_EDIV");
		s.AppendLine("IR_EIMP:");
		s.AppendLine("    POP DX");
		s.AppendLine("    MOV AH, 02H");
		s.AppendLine("    INT 21H");
		s.AppendLine("    LOOP IR_EIMP");
		s.AppendLine("    ; Punto decimal");
		s.AppendLine("    MOV AH, 02H");
		s.AppendLine("    MOV DL, '.'");
		s.AppendLine("    INT 21H");
		s.AppendLine("    ; Parte decimal (0..99) recuperada de la pila");
		s.AppendLine("    POP AX");
		s.AppendLine("    CMP AX, 10");
		s.AppendLine("    JGE IR_DEC_OK");
		s.AppendLine("    PUSH AX");
		s.AppendLine("    MOV AH, 02H");
		s.AppendLine("    MOV DL, '0'");
		s.AppendLine("    INT 21H");
		s.AppendLine("    POP AX");
		s.AppendLine("IR_DEC_OK:");
		s.AppendLine("    MOV CX, 0");
		s.AppendLine("    MOV BX, 10");
		s.AppendLine("IR_DDIV:");
		s.AppendLine("    XOR DX, DX");
		s.AppendLine("    DIV BX");
		s.AppendLine("    ADD DL, '0'");
		s.AppendLine("    PUSH DX");
		s.AppendLine("    INC CX");
		s.AppendLine("    CMP AX, 0");
		s.AppendLine("    JNE IR_DDIV");
		s.AppendLine("IR_DIMP:");
		s.AppendLine("    POP DX");
		s.AppendLine("    MOV AH, 02H");
		s.AppendLine("    INT 21H");
		s.AppendLine("    LOOP IR_DIMP");
		s.AppendLine("    MOV AH, 02H");
		s.AppendLine("    MOV DL, 0DH");
		s.AppendLine("    INT 21H");
		s.AppendLine("    MOV DL, 0AH");
		s.AppendLine("    INT 21H");
		s.AppendLine("    POP DX");
		s.AppendLine("    POP CX");
		s.AppendLine("    POP BX");
		s.AppendLine("    POP AX");
		s.AppendLine("    RET");
		s.AppendLine("ImprimirREAL ENDP");
		s.AppendLine();
	}

	/*
	אלפי שכירים של חרב
	רוצים רק את ראשי
	רק רוצים את האיבה
	האהבה לא אמיתית
	היא רק משנית
	ברמה מציאותית
	השמיים כבר נופלים
	חזרה לבראשית

	un poco de hebreo en el codigo
	para la buena suerte
	todo buen programador lo sabe
	*/
}
