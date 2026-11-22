public class Cuadruplo {
	public string Op = "";
	public string Arg1 = "_";
	public string Arg2 = "_";
	public string Resultado = "_";
	public override string ToString() => $"({Op}, {Arg1}, {Arg2}, {Resultado})";
}

public partial class Cuadruplexer(SymbolTable table) {
	SymbolTable symtable = table;
	int tempcount = 0;
	int labelcount = 0;
	string NuevoTemp() => $"t{tempcount++}";
	string NuevaEtiqueta() => $"L{labelcount++}";

	public List<Cuadruplo> Cuadruplos = [];
	void Gen(string op, string arg1, string arg2, string resultado) {
		Cuadruplos.Add(new Cuadruplo{
			Op = op,
			Arg1 = arg1,
			Arg2 = arg2,
			Resultado = resultado
		});
	}

	public List<Cuadruplo> Generar(NodoPrograma programa, bool imprimir = false) {
		foreach (var ins in programa.Instrucciones) VisitarNodo(ins);

		// la generacion de cuadruplos realmente nunca falla
		// si se llego a esta fase es porque el programa ya no trae errores
		Console.WriteLine();
		G.Print("Cuadruplos generados exitosamente!! asombroso", ConsoleColor.Green);

		// imprime los cuadruplos para tomar un vistazo 👀 
		if (imprimir) {
			var i = 1;
			foreach (Cuadruplo cuad in Cuadruplos) {
				G.Print($"{i}. " + cuad.ToString(), ConsoleColor.White);
				i++;
			}
		}
		return Cuadruplos;
	}

	public void VisitarNodo(AstNode node) {
		switch (node) {
			//case NodoDIM di- NO!!!!!!!! DIM no genera cuadruplos, la tabla de simbolos ya tiene todo lo que necesitamos
			case NodoLET let: GenerarLET(let); break;
			case NodoINPUT inp: GenerarINPUT(inp); break;
			case NodoPRINT pri: GenerarPRINT(pri); break;
			case NodoIF iff: GenerarIF(iff); break;
			case NodoWHILE whi: GenerarWHILE(whi); break;
			case NodoCondicion con: GenerarCondicion(con); break;
			case NodoExp exp: GenerarExpresion(exp); break;
		}
	}
}